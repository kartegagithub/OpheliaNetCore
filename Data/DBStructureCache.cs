using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Ophelia.Data
{
    public class DBStructureCache
    {
        public static List<CacheType> TypeCache { get; internal set; }

        public void LoadFromEDMX(string connectionStringName)
        {
            if (TypeCache != null)
                return;

            TypeCache = new List<CacheType>();
            if (!string.IsNullOrEmpty(connectionStringName))
            {
                var metadata = System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName].ToString().Split(';').Where(op => op.Contains("metadata=")).FirstOrDefault();
                if (!string.IsNullOrEmpty(metadata))
                {
                    var splittedPaths = metadata.Replace("metadata=", "").Split('|');
                    var csdlPath = splittedPaths.Where(op => op.EndsWith(".csdl")).FirstOrDefault();
                    var mslPath = splittedPaths.Where(op => op.EndsWith(".msl")).FirstOrDefault();
                    var ssdlPath = splittedPaths.Where(op => op.EndsWith(".ssdl")).FirstOrDefault();
                    if (!string.IsNullOrEmpty(csdlPath) && !string.IsNullOrEmpty(mslPath))
                    {
                        csdlPath = csdlPath.Replace("res://*/", "");
                        mslPath = mslPath.Replace("res://*/", "");
                        ssdlPath = ssdlPath.Replace("res://*/", "");
                        Assembly assembly = null;
                        foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            try
                            {
                                if (item.GetManifestResourceNames().Contains(csdlPath))
                                {
                                    assembly = item;
                                    break;
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                        if (assembly != null)
                        {
                            this.ParseCSDL(assembly, csdlPath);
                            this.ParseMSL(assembly, mslPath);
                            this.ParseSSDL(assembly, ssdlPath);
                        }
                    }
                }
            }
        }
        private void ParseSSDL(Assembly assembly, string ssdlPath)
        {
            using (var stream = assembly.GetManifestResourceStream(ssdlPath))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var doc = new XmlDocument();
                        doc.LoadXml(reader.ReadToEnd());
                        var container = (XmlElement)doc.GetElementsByTagName("EntityContainer")[0];

                        if (container != null)
                        {
                            foreach (XmlElement item in container.GetElementsByTagName("EntitySet"))
                            {
                                var types = TypeCache.Where(op => op.NavigationProperties.Where(op2 => op2.TableName == item.Attributes["Name"].InnerText).Any());
                                foreach (var type in types)
                                {
                                    var p = type.NavigationProperties.Where(op => op.TableName == item.Attributes["Name"].InnerText).FirstOrDefault();
                                    p.SchemaName = item.Attributes["Schema"].InnerText;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ParseMSL(Assembly assembly, string mslPath)
        {
            using (var stream = assembly.GetManifestResourceStream(mslPath))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var doc = new XmlDocument();
                        doc.LoadXml(reader.ReadToEnd());
                        var associationSetMappings = doc.GetElementsByTagName("AssociationSetMapping");

                        if (associationSetMappings.Count > 0)
                        {
                            foreach (XmlElement item in associationSetMappings)
                            {
                                var types = TypeCache.Where(op => op.NavigationProperties.Where(op2 => op2.QueryOverRelation && op2.TableName == item.Attributes["Name"].InnerText).Any());
                                foreach (var type in types)
                                {
                                    for (int i = 0; i < 2; i++)
                                    {
                                        foreach (XmlElement endProp in item.ChildNodes)
                                        {
                                            if (endProp.Attributes["Name"].InnerText == type.Type.Name)
                                            {
                                                var scalarProp = (XmlElement)endProp.ChildNodes[0];

                                                var p = type.NavigationProperties.Where(op => op.TableName == item.Attributes["Name"].InnerText).FirstOrDefault();
                                                p.Field = scalarProp.Attributes["ColumnName"].InnerText;
                                                p.ReferencedField = scalarProp.Attributes["Name"].InnerText;

                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private Assembly ParseCSDL(Assembly assembly, string csdlPath)
        {
            using (var stream = assembly.GetManifestResourceStream(csdlPath))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var doc = new XmlDocument();
                        doc.LoadXml(reader.ReadToEnd());
                        var entityTypes = doc.GetElementsByTagName("EntityType");
                        var associations = doc.GetElementsByTagName("Association");

                        Assembly typeAssembly = null;
                        foreach (XmlElement item in entityTypes)
                        {
                            if (item.GetElementsByTagName("NavigationProperty").Count > 0)
                            {
                                var props = item.GetElementsByTagName("NavigationProperty");
                                foreach (XmlElement p in props)
                                {
                                    Type type = null;
                                    if (typeAssembly != null)
                                    {
                                        type = typeAssembly.GetTypes().Where(op => op.Name == item.Attributes["Name"].InnerText).FirstOrDefault();
                                    }
                                    else
                                    {
                                        type = item.Attributes["Name"].InnerText.GetSimilarTypes(true).Where(op => op.FullName.StartsWith(assembly.FullName.Split('.')[0])).FirstOrDefault();
                                    }
                                    if (type == null)
                                        continue;

                                    typeAssembly = type.Assembly;

                                    var cacheType = TypeCache.Where(op => op.Type == type).FirstOrDefault();
                                    if (cacheType == null)
                                    {
                                        cacheType = new CacheType()
                                        {
                                            Type = type
                                        };
                                        cacheType.NavigationProperties = new List<NavigationProperty>();
                                        TypeCache.Add(cacheType);
                                    }
                                    var propInfo = type.GetProperties().Where(op => op.Name == p.Attributes["Name"].InnerText).FirstOrDefault();
                                    var navProp = new NavigationProperty()
                                    {
                                        PropInfo = propInfo,
                                        IsMultiRelation = propInfo.PropertyType.IsQueryableDataSet() || typeof(System.Collections.IEnumerable).IsAssignableFrom(propInfo.PropertyType)
                                    };
                                    if (navProp.IsMultiRelation)
                                        navProp.ToType = propInfo.PropertyType.GetGenericArguments()[0];
                                    else
                                        navProp.ToType = propInfo.PropertyType;

                                    foreach (XmlElement assoc in associations)
                                    {
                                        if (assoc.Attributes["Name"].InnerText == p.Attributes["Relationship"].InnerText.Split('.')[1])
                                        {
                                            var elems = assoc.GetElementsByTagName("ReferentialConstraint");
                                            if (elems != null && elems.Count > 0)
                                            {
                                                var elem = (XmlElement)elems[0];
                                                navProp.Field = ((XmlElement)((XmlElement)elem.GetElementsByTagName("Dependent")[0]).GetElementsByTagName("PropertyRef")[0]).Attributes["Name"].InnerText;
                                                navProp.ReferencedField = ((XmlElement)((XmlElement)elem.GetElementsByTagName("Principal")[0]).GetElementsByTagName("PropertyRef")[0]).Attributes["Name"].InnerText;
                                            }
                                            else
                                            {
                                                navProp.QueryOverRelation = true;
                                                navProp.TableName = assoc.Attributes["Name"].InnerText;
                                            }
                                            break;
                                        }
                                    }
                                    cacheType.NavigationProperties.Add(navProp);
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }

    public class CacheType
    {
        public Type Type { get; set; }
        public List<NavigationProperty> NavigationProperties { get; set; }
    }
    public class NavigationProperty
    {
        public bool QueryOverRelation { get; set; }
        public string ReferencedField { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string Field { get; set; }
        public PropertyInfo PropInfo { get; set; }
        public Type ToType { get; set; }
        public bool IsMultiRelation { get; set; }
    }
}
