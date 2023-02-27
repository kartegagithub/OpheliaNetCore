using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace Ophelia.Data.Querying.Query.Helpers
{
    [DataContract]
    public class Sorter : IDisposable
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool Ascending { get; set; }

        [DataMember]
        public Sorter SubSorter { get; set; }

        public static Sorter Create(Expression expression, bool Ascending)
        {
            return ExpressionParser.Create(expression).ToSorter(Ascending);
        }
        public string Build(BaseQuery query)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                if (this.SubSorter != null)
                    return this.SubSorter.Build(query);
                return "";
            }
            if (query.Data.Groupers.Any())
                this.Name = this.Name.Replace("Key.", "");

            //TODO: GroupBy(op => new { PName = op.Product.Name }).OrderBy(op => op.Key.PName) is not working
            if (this.Name.IndexOf(".") > -1)
            {
                var props = this.Name.Split('.');
                var parentIncluder = query.Data.Includers.Where(op => op.Name == props.FirstOrDefault()).FirstOrDefault();
                if (parentIncluder != null)
                {
                    Includer lastIncluder = parentIncluder;
                    for (int i = 1; i < props.Length - 1; i++)
                    {
                        lastIncluder = lastIncluder.SubIncluders.Where(op => op.Name == props[i]).FirstOrDefault();
                    }
                    if (lastIncluder != null)
                    {
                        if (query.Data.Groupers.Count == 0 || query.Data.Groupers.Where(op => op.Name == props.LastOrDefault()).Any())
                            return lastIncluder.Table.Alias + "." + query.Context.Connection.FormatDataElement(query.Context.Connection.GetMappedFieldName(props.LastOrDefault())) + (this.Ascending ? " ASC" : " DESC");
                    }
                }
            }
            else if (!string.IsNullOrEmpty(this.Name))
            {
                if (query.Data.Groupers.Count == 0 || query.Data.Groupers.Where(op => op.Name == this.Name).Any())
                {
                    return query.Data.MainTable.Alias + "." + query.Context.Connection.FormatDataElement(query.Context.Connection.GetMappedFieldName(this.Name)) + (this.Ascending ? " ASC" : " DESC");
                }
            }
            return "";
        }
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

        }
        public Sorter Serialize()
        {
            var entity = new Sorter();
            entity.Name = this.Name;
            entity.Ascending = this.Ascending;
            if (this.SubSorter != null)
                entity.SubSorter = this.SubSorter.Serialize();
            return entity;
        }
    }
}