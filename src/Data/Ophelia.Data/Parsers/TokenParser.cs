using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ophelia.Data.Parsers
{
    public class TokenParser
    {
        public static TokenData Tokenize(string expression)
        {
            expression = expression
                .Replace(" ", "")
                .Replace("AND", "&")
                .Replace("OR", "|")
                .Replace("&&", "&")
                .Replace("||", "|")
                .Replace("{", "(")
                .Replace("[", "(")
                .Replace("}", ")")
                .Replace("]", ")");
            List<string> result = new List<string>();
            List<string> tokens = new List<string>();

            tokens.Add("(^\\()");// matches opening bracket
            tokens.Add("(^[&|<=>!]+)"); // matches operators and other special characters
            tokens.Add("(^[\\w]+)");// matches words and integers
            tokens.Add("(^[\\)])"); // matches closing bracket

            while (0 != expression.Length)
            {
                bool foundMatch = false;

                foreach (string token in tokens)
                {
                    Match match = Regex.Match(expression, token);
                    if (false == match.Success)
                    {
                        continue;
                    }

                    result.Add(match.Groups[1].Value);
                    expression = Regex.Replace(expression, token, "");
                    foundMatch = true;
                    expression = Regex.Replace(expression, @"\s*\+\s*", "");    // Remove concatenation
                    break;
                }

                if (false == foundMatch)
                {
                    break;
                }
            }
            return Create(result.ToArray());
        }

        private static TokenData Create(string[] tokens)
        {
            var data = new TokenData();
            TokenFilterGroup group = data.RootGroup;
            TokenFilterGroup parentGroup = data.RootGroup;
            var index = 0;
            Constraint? constraint = Constraint.And;
            foreach (var token in tokens)
            {
                switch (token)
                {
                    case "(":
                        parentGroup = group;
                        group = new TokenFilterGroup() { Index = index, Constraint = constraint };
                        index++;
                        group.Parent = parentGroup;
                        parentGroup.Entities.Add(group);
                        constraint = null;
                        break;
                    case ")":
                        group = parentGroup;
                        parentGroup = parentGroup.Parent;
                        break;
                    case "|":
                        constraint = Constraint.Or;
                        break;
                    case "&":
                        constraint = Constraint.And;
                        break;
                    default:
                        if (group.Index == -1)
                        {
                            parentGroup = group;
                            group = new TokenFilterGroup() { Index = index, Constraint = constraint };
                            index++;
                            group.Parent = parentGroup;
                            parentGroup.Entities.Add(group);
                        }
                        group.Entities.Add(new TokenFilter()
                        {
                            Name = token,
                            Constraint = constraint
                        });
                        break;
                }
            }
            data.RootGroup.Entities.RemoveAll(op => string.IsNullOrEmpty(op.Name) && !op.Entities.Any());
            return data;
        }
        public class TokenData
        {
            public TokenData()
            {
                this.RootGroup = new TokenFilterGroup()
                {
                    Index = -1
                };
            }
            public TokenFilterGroup RootGroup { get; set; }
            public void SetSQL(string filterName, string SQL)
            {
                foreach (var entity in this.RootGroup.Entities)
                    entity.SetSQL(filterName, SQL);
            }
            public override string ToString()
            {
                return this.RootGroup.ToString();
            }
        }
        public abstract class TokenFilterEntity
        {
            public string Name { get; set; }
            public int Index { get; set; }
            public Constraint? Constraint { get; set; }
            public List<TokenFilterEntity> Entities { get; set; }
            public void SetSQL(string filterName, string SQL)
            {
                foreach (var filter in this.Entities)
                {
                    if ((filter is TokenFilter) && filter.Name.Equals(filterName, StringComparison.InvariantCultureIgnoreCase))
                        (filter as TokenFilter).SQL = SQL;
                    else if (filter is TokenFilterGroup)
                        filter.SetSQL(filterName, SQL);
                }
            }
            public TokenFilterEntity()
            {
                this.Entities = new List<TokenFilterEntity>();
            }
        }
        public class TokenFilterGroup : TokenFilterEntity
        {
            public TokenFilterGroup Parent { get; set; }

            public override string ToString()
            {
                var result = "";
                if (this.Entities != null && this.Entities.Any())
                {
                    result += "(";
                    foreach (var entity in this.Entities)
                    {
                        var tmpEntity = entity.ToString();
                        if (!string.IsNullOrEmpty(tmpEntity))
                        {
                            if (entity != this.Entities.FirstOrDefault())
                            {
                                if (entity.Constraint.HasValue)
                                    result += $" {entity.Constraint} ";
                                else if (this.Constraint.HasValue)
                                    result += $" {this.Constraint} ";
                                else
                                    result += $" INVALID ";
                            }
                            result += $"{tmpEntity}";
                        }
                    }
                    result = result.TrimEnd(" AND ".ToCharArray()).TrimEnd(" OR ".ToCharArray());
                    result += ")";
                }
                return result;
            }
        }
        public class TokenFilter : TokenFilterEntity
        {
            public string SQL { get; set; }
            public override string ToString()
            {
                if (!string.IsNullOrEmpty(this.SQL))
                    return $"{this.SQL}";
                return $"{this.Name}";
            }
        }
    }
}
