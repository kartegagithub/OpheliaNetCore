using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ophelia
{
    public static class ReflectionExtensions
    {
        private static List<string> IncludedAssemblies { get; set; } = new List<string>();

        public static List<string> ExcludedAssemblies { get; private set; } = new List<string>();

        public static void AddNamespaceToSearch(params string[] namespaces)
        {
            if (namespaces != null && namespaces.Length > 0)
                IncludedAssemblies.AddRange(namespaces);
        }

        public static List<Assembly> GetValidAssemblies()
        {
            if (IncludedAssemblies.Count > 0)
                return AppDomain.CurrentDomain.GetAssemblies().Where(op => !ExcludedAssemblies.Contains(op.FullName) && IncludedAssemblies.Any(op2 => op.FullName.Contains(op2, StringComparison.InvariantCultureIgnoreCase))).ToList();
            return AppDomain.CurrentDomain.GetAssemblies().Where(op => !ExcludedAssemblies.Contains(op.FullName)).ToList();
        }

        public static T2 CopyTo<T1, T2>(this T1 obj1, T2 obj2, params string[] excludedProps)
            where T1 : class
            where T2 : class
        {
            if (obj1 == null || obj2 == null)
                return obj2;

            var type1 = obj1.GetType();
            var type2 = obj2.GetType();
            new Reflection.ObjectIterator()
            {
                IterationCallback = (obj) =>
                {
                    var props = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
                    foreach (var p in props)
                    {
                        if (p.SetMethod == null || !p.SetMethod.IsPublic || p.GetMethod == null || !p.GetMethod.IsPublic)
                            continue;

                        if (type2.GetProperty(p.Name) != null && (excludedProps == null || !excludedProps.Any() || !excludedProps.Contains(p.Name)))
                        {
                            try
                            {
                                var type2Prop = type2.GetProperty(p.Name);
                                if (type2Prop.PropertyType.IsAssignableFrom(p.PropertyType))
                                    type2Prop.SetValue(obj2, p.GetValue(obj1));
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    return null;
                }
            }.Iterate(obj1).Dispose();
            return obj2;
        }

        public static List<T> ToList<T>(this System.Collections.ArrayList arrayList)
        {
            List<T> list = new List<T>(arrayList.Count);
            foreach (T instance in arrayList)
            {
                list.Add(instance);
            }
            return list;
        }

        public static System.Collections.IEnumerable ToList(this System.Collections.ArrayList arrayList, Type toType)
        {
            var listType = typeof(List<>).MakeGenericType(toType);
            var list = (System.Collections.IList)Activator.CreateInstance(listType);
            foreach (object instance in arrayList)
            {
                list.Add(instance);
            }
            return list;
        }

        public static object ConvertData(this Type targetType, object value, bool setDefaultForNullValues = true)
        {
            try
            {
                object convertedValue = null;
                var isNull = value == null;
                if (!isNull && string.IsNullOrEmpty(Convert.ToString(value, System.Globalization.CultureInfo.CurrentCulture)))
                    isNull = true;

                if (isNull && !setDefaultForNullValues)
                    return value;

                if (targetType == typeof(bool) || targetType == typeof(bool?))
                {
                    convertedValue = isNull ? default : Convert.ToInt64(value, System.Globalization.CultureInfo.CurrentCulture) != 0;
                }
                else if (targetType == typeof(byte) || targetType == typeof(byte?))
                {
                    convertedValue = isNull ? default : Convert.ToByte(value, System.Globalization.CultureInfo.CurrentCulture);
                }
                else if (targetType == typeof(int) || targetType == typeof(int?))
                {
                    convertedValue = isNull ? default : Convert.ToInt32(value, System.Globalization.CultureInfo.CurrentCulture);
                }
                else if (targetType == typeof(short) || targetType == typeof(short?))
                {
                    convertedValue = isNull ? default : Convert.ToInt16(value, System.Globalization.CultureInfo.CurrentCulture);
                }
                else if (targetType == typeof(long) || targetType == typeof(long?))
                {
                    convertedValue = isNull ? default : Convert.ToInt64(value, System.Globalization.CultureInfo.CurrentCulture);
                }
                else if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                {
                    convertedValue = isNull ? default : Convert.ToDecimal(value, System.Globalization.CultureInfo.CurrentCulture);
                }
                else if (targetType == typeof(double) || targetType == typeof(double?))
                {
                    convertedValue = isNull ? default : Convert.ToDouble(value, System.Globalization.CultureInfo.CurrentCulture);
                }
                else if (targetType.IsEnum)
                {
                    convertedValue = Enum.ToObject(targetType, Enum.GetUnderlyingType(targetType).ConvertData(value));
                }
                else if (!value.GetType().IsStringType() && value.GetType().IsEnumarable())
                {
                    convertedValue = value;
                }
                else if (value != null)
                {
                    var valueType = value.GetType();
                    var c1 = System.ComponentModel.TypeDescriptor.GetConverter(valueType);
                    if (c1.CanConvertTo(targetType)) // this returns false for string->bool
                    {
                        convertedValue = c1.ConvertTo(value, targetType);
                    }
                    else
                    {
                        var c2 = System.ComponentModel.TypeDescriptor.GetConverter(targetType);
                        if (c2.CanConvertFrom(valueType)) // this returns true for string->bool, but will throw for "1"
                        {
                            convertedValue = c2.ConvertFrom(value);
                        }
                        else
                        {
                            convertedValue = Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.CurrentCulture); // this will throw for "1"
                        }
                    }
                }
                if (targetType.IsNullable())
                {
                    var typeArgument = targetType.GetGenericArguments()[0];
                    var ctor = targetType.GetConstructor(new[] { typeArgument });
                    return ctor.Invoke(new[] { convertedValue });
                }
                return convertedValue;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static bool IsStringType(this Type type)
        {
            if (type.Name == "String")
                return true;
            return false;
        }
        public static bool IsDecimal(this Type type)
        {
            if (type == null)
                return false;
            return Type.GetTypeCode(type) switch
            {
                TypeCode.Decimal => true,
                _ => false,
            };
        }

        public static bool IsDouble(this Type type)
        {
            if (type == null)
                return false;
            return Type.GetTypeCode(type) switch
            {
                TypeCode.Double => true,
                _ => false,
            };
        }

        public static bool IsSingle(this Type type)
        {
            if (type == null)
                return false;
            return Type.GetTypeCode(type) switch
            {
                TypeCode.Single => true,
                _ => false,
            };
        }

        public static bool IsNumeric(this Type type)
        {
            if (type == null)
                return false;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return Nullable.GetUnderlyingType(type).IsNumeric();

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static byte ToByte(this Type type, object value)
        {
            return Convert.ToByte(type.ToInt64(value));
        }

        public static short ToInt16(this Type type, object value)
        {
            return Convert.ToInt16(type.ToInt64(value));
        }

        public static int ToInt32(this Type type, object value)
        {
            return Convert.ToInt32(type.ToInt64(value));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "<Pending>")]
        public static long ToInt64(this Type type, object value)
        {
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                    return 0;
                case TypeCode.Boolean:
                    return (bool)value ? 1 : 0;
                case TypeCode.Char:
                    return Convert.ToInt64(char.GetNumericValue((char)value));
                case TypeCode.SByte:
                    return Convert.ToInt64((sbyte)value);
                case TypeCode.Byte:
                    return Convert.ToInt64((byte)value);
                case TypeCode.Int16:
                    return Convert.ToInt64((short)value);
                case TypeCode.UInt16:
                    return Convert.ToInt64((ushort)value);
                case TypeCode.Int32:
                    return Convert.ToInt64((int)value);
                case TypeCode.UInt32:
                    return Convert.ToInt64((uint)value);
                case TypeCode.Int64:
                    return (long)value;
                case TypeCode.UInt64:
                    return Convert.ToInt64((ulong)value);
                case TypeCode.Single:
                    return Convert.ToInt64((float)value);
                case TypeCode.Double:
                    return Convert.ToInt64((double)value);
                case TypeCode.Decimal:
                    return Convert.ToInt64((decimal)value);
                case TypeCode.DateTime:
                    return 0;
                default:
                    var decimalSeperator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                    var sValue = value.ToString().Trim().TrimStart('-').TrimStart('+');
                    if (sValue.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) > -1 && sValue.IndexOf(",", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        string thousandSeperator;
                        if (decimalSeperator == ",")
                            thousandSeperator = ".";
                        else
                            thousandSeperator = ",";
                        sValue = sValue.Replace(thousandSeperator, "");
                    }
                    decimal decValue;
                    decimal.TryParse(sValue, out decValue);
                    return Convert.ToInt64(decValue);
            }
        }

        public static bool ToBoolean(this Type type, object value)
        {
            if (value != null)
            {
                var typeCode = Type.GetTypeCode(type);
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        return (bool)value;
                    case TypeCode.Char:
                        var cValue = (char)value;
                        return cValue.Equals("Y") || cValue.Equals("y");
                    case TypeCode.String:
                        var sValue = value.ToString().ToLowerInvariant();
                        if (string.IsNullOrEmpty(sValue))
                            return false;

                        return value.Equals("true") || value.Equals("yes");
                    default:
                        return type.ToInt64() > 0;
                }
            }
            return false;
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static object GetDefaultValue(this Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static string GetNamespace(this Type type)
        {
            var arr = type.FullName.Split('.');
            var ns = arr[^2];
            return ns;
        }

        public static object ExecuteMethod<T>(this T entity, string method, params object[] parameters) where T : class
        {
            if (entity != null)
            {
                var methods = entity.GetType().GetMethods().Where(op => op.Name.Equals(method, StringComparison.OrdinalIgnoreCase)).ToList();
                MethodInfo m = null;
                if (parameters != null)
                    m = methods.Where(op => op.GetParameters().Length == parameters.Length).FirstOrDefault();
                if (m == null)
                    m = methods.FirstOrDefault();
                if (m != null)
                {
                    return m.Invoke(entity, parameters);
                }
            }
            return null;
        }

        public static List<MethodInfo> GetImplementingMethods<T>(this T attributeClass, Type classType) where T : Type
        {
            if (classType == null)
                return new List<MethodInfo>();
            return classType.GetMethods().Where(op => op.GetCustomAttributes(attributeClass, true).Length > 0).ToList();
        }

        public static List<MethodInfo> GetImplementingMethods<T>(this T attributeClass, string AssemblyPath, string className) where T : Type
        {
            if (string.IsNullOrEmpty(className))
                return new List<MethodInfo>();
            if (System.IO.File.Exists(AssemblyPath))
            {
                var a = Assembly.LoadFile(AssemblyPath);
                var classType = a.GetType(className);
                return classType.GetMethods().Where(op => op.GetCustomAttributes(attributeClass, true).Length > 0).ToList();
            }
            return new List<MethodInfo>();
        }

        public static List<Type> GetAssignableClasses<T>(this T baseClass, string RootNamespace = "") where T : Type
        {
            var list = new List<Type>();
            foreach (Assembly a in GetValidAssemblies())
            {
                try
                {
                    var Types = a.GetTypes().Where(op => !string.IsNullOrEmpty(op.Namespace)).ToList();
                    if (Types.Count > 0 && (string.IsNullOrEmpty(RootNamespace) || Types[0].Namespace.StartsWith(RootNamespace, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (Types != null)
                        {
                            Types = Types.Where(op => baseClass.IsAssignableFrom(op) && !op.IsInterface).ToList();
                            if (Types != null && Types.Count > 0)
                            {
                                list.AddRange(Types);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    ExcludedAssemblies.Add(a.FullName);
                    continue;
                }
            }
            return list;
        }

        public static List<Type> GetAssignableClassesFromFile<T>(this T baseClass, string AssemblyPath, string RootNamespace = "") where T : Type
        {
            var list = new List<Type>();
            if (System.IO.File.Exists(AssemblyPath))
            {
                var a = Assembly.LoadFile(AssemblyPath);
                try
                {
                    var Types = a.GetTypes().Where(op => !string.IsNullOrEmpty(op.Namespace)).ToList();
                    if (Types.Count > 0 && (string.IsNullOrEmpty(RootNamespace) || Types[0].Namespace.StartsWith(RootNamespace, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (Types != null)
                        {
                            Types = Types.Where(op => baseClass.IsAssignableFrom(op) && !op.IsInterface).ToList();
                            if (Types != null && Types.Count > 0)
                            {
                                list.AddRange(Types);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return list;
                }
                a = null;
            }
            return list;
        }

        public static PropertyInfo GetDisplayTextProperty(this Type type, string baseProperty)
        {
            var baseType = type.GetPropertyInfo(baseProperty).PropertyType;
            var prop = baseType.GetPropertyInfo("Name");
            if (prop != null)
                return prop;

            prop = baseType.GetPropertyInfo("Title");
            if (prop != null)
                return prop;

            prop = baseType.GetPropertyInfo("Text");
            if (prop != null)
                return prop;

            prop = baseType.GetPropertyInfo("FullName");
            if (prop != null)
                return prop;

            prop = baseType.GetPropertyInfo("FirstName");
            if (prop != null)
                return prop;

            prop = baseType.GetPropertyInfo("LastName");
            if (prop != null)
                return prop;

            prop = baseType.GetPropertyInfo("UserName");
            if (prop != null)
                return prop;

            prop = baseType.GetPropertyInfo("Code");
            if (prop != null)
                return prop;

            prop = baseType.GetPropertyInfo("ShortName");
            if (prop != null)
                return prop;
            return null;
        }

        public static PropertyInfo GetPropertyInfo(this Type type, string property)
        {
            PropertyInfo prop = null;
            if (property.IndexOf('.') > -1)
            {
                var splitted = property.Split('.');
                var tmpType = type;
                foreach (var item in splitted)
                {
                    prop = tmpType.GetProperties().Where(op => op.Name.Equals(item, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (prop != null)
                        tmpType = prop.PropertyType;
                    else if (tmpType.IsGenericType)
                    {
                        prop = tmpType.GenericTypeArguments[0].GetPropertyInfo(item);
                        if (prop != null)
                            tmpType = prop.PropertyType;
                    }
                }
            }
            if (prop == null)
            {
                prop = type.GetProperties().Where(op => op.Name.Equals(property, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (prop == null && type.IsGenericType)
                    prop = type.GenericTypeArguments[0].GetPropertyInfo(property);
            }
            return prop;
        }

        public static PropertyInfo[] GetPropertyInfoTree(this Type type, string property)
        {
            var props = new List<PropertyInfo>();
            if (property.IndexOf('.') > -1)
            {
                foreach (var p in property.Split('.'))
                {
                    var prop = type.GetProperties().FirstOrDefault(op => op.Name.Equals(p, StringComparison.OrdinalIgnoreCase));
                    if (prop != null)
                    {
                        props.Add(prop);
                        type = props.LastOrDefault().PropertyType;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
                props.Add(type.GetProperties().Where(op => op.Name.Equals(property, StringComparison.OrdinalIgnoreCase)).FirstOrDefault());
            return props.ToArray();
        }

        public static object GetPropertyValue<TResult>(this TResult source, string property) where TResult : class
        {
            if (source != null)
            {
                if (property.IndexOf('.') > -1)
                {
                    object entity = source;
                    foreach (var item in property.Split('.'))
                    {
                        var tmpEntity = entity.GetPropertyValue(item);
                        if (tmpEntity != null && tmpEntity.GetType().IsClass)
                        {
                            entity = tmpEntity;
                        }
                        else
                            return tmpEntity;
                    }
                }
                else
                {
                    var props = source.GetType().GetProperties().Where(op => op.Name.Equals(property, StringComparison.OrdinalIgnoreCase));
                    PropertyInfo propInfo = null;
                    if (props.Count() > 1)
                        propInfo = props.Where(op => op.DeclaringType == source.GetType()).FirstOrDefault();
                    if (props.Any())
                        propInfo = props.FirstOrDefault();
                    if (propInfo != null)
                    {
                        if (propInfo.IsStaticProperty())
                            return propInfo.GetStaticPropertyValue();
                        else
                            return propInfo.GetValue(source);
                    }
                    props = null;

                    var fields = source.GetType().GetFields().Where(op => op.Name.Equals(property, StringComparison.OrdinalIgnoreCase));
                    if (fields.Count() > 1)
                        return fields.Where(op => op.DeclaringType == source.GetType()).FirstOrDefault()?.GetValue(source);
                    if (fields.Any())
                        return fields.FirstOrDefault()?.GetValue(source);
                }
            }
            return null;
        }

        public static void SetPropertyValue<TResult>(this TResult source, string property, object value) where TResult : class
        {
            if (source != null)
            {
                var props = source.GetType().GetProperties().Where(op => op.Name.Equals(property, StringComparison.OrdinalIgnoreCase));
                PropertyInfo p = null;
                if (props.Count() > 1)
                    p = props.Where(op => op.DeclaringType == source.GetType()).FirstOrDefault();
                else
                    p = props.FirstOrDefault();

                if (p != null)
                {
                    var type = p.PropertyType;
                    if (p.PropertyType.IsGenericType)
                        type = p.PropertyType.GenericTypeArguments[0];

                    var val = type.ConvertData(value);
                    p.SetValue(source, val);
                }
            }
        }

        public static bool IsEnumarableGeneric(this Type type)
        {
            return type.IsGenericType && (type.IsAssignableFrom(typeof(System.Collections.IEnumerable)) || typeof(System.Collections.IEnumerable).IsAssignableFrom(type));
        }

        public static bool IsEnumarable(this Type type, bool checkForGenericType = true)
        {
            if (checkForGenericType)
                return type.IsEnumarableGeneric();
            return (type.IsAssignableFrom(typeof(System.Collections.IEnumerable)) || typeof(System.Collections.IEnumerable).IsAssignableFrom(type));
        }

        public static List<PropertyInfo> GetPropertiesByType(this Type ObjectType, Type PropertyTpe)
        {
            if (ObjectType != null && PropertyTpe != null)
                return ObjectType.GetProperties().Where(op => op.PropertyType == PropertyTpe).ToList();
            else
                return null;
        }

        public static List<Type> GetSimilarTypes(this string objectType, bool exactMatch = false)
        {
            var types = new List<Type>();
            try
            {
                foreach (var a in GetValidAssemblies())
                {
                    try
                    {
                        IEnumerable<Type> Types = null;
                        if (exactMatch)
                            Types = a.GetTypes().Where(op => op.Name.Equals(objectType, StringComparison.OrdinalIgnoreCase) || op.FullName.Equals(objectType, StringComparison.OrdinalIgnoreCase));
                        else
                            Types = a.GetTypes().Where(op => op.FullName.IndexOf(objectType, StringComparison.OrdinalIgnoreCase) > -1);
                        if (Types.Any())
                            types.AddRange(Types);
                    }
                    catch (Exception)
                    {
                        ExcludedAssemblies.Add(a.FullName);
                        continue;
                    }
                }
            }
            catch (Exception)
            {

            }
            return types;
        }

        private static Dictionary<Type, List<Type>> TypeCache { get; set; } = new Dictionary<Type, List<Type>>();

        private static object lockObj = new object();

        public static void RemoveTypeCache(Type type)
        {
            try
            {
                lock (lockObj)
                {
                    TypeCache.Remove(type);
                }
            }
            catch (Exception)
            {

            }
        }

        private static List<Type> GetExistingTypes(Type baseType)
        {
            try
            {
                if (TypeCache.TryGetValue(baseType, out List<Type> existingTypes))
                    return existingTypes;
            }
            catch (Exception)
            {

            }
            return new List<Type>();
        }

        private static void AddTypeCache(Type baseType, List<Type> existingTypes)
        {
            try
            {
                lock (lockObj)
                {
                    TypeCache[baseType] = existingTypes;
                }
            }
            catch (Exception)
            {

            }
        }

        public static List<Type> GetRealTypes(this Type baseType, bool baseTypeIsDefault = true)
        {
            var returnTypes = GetExistingTypes(baseType);
            if (returnTypes.Count > 0) return returnTypes;

            try
            {
                foreach (var a in GetValidAssemblies())
                {
                    try
                    {
                        var Types = a.GetTypes().Where(op => !op.IsInterface && (op.IsSubclassOf(baseType) || baseType.IsAssignableFrom(op)));
                        if (Types.Any())
                        {
                            if (!baseTypeIsDefault)
                                returnTypes.AddRange(Types.Where(op => op != baseType));
                            else
                                returnTypes.AddRange(Types);
                        }
                    }
                    catch (Exception)
                    {
                        ExcludedAssemblies.Add(a.FullName);
                        continue;
                    }
                }
            }
            catch (Exception)
            {

            }
            AddTypeCache(baseType, returnTypes);
            return returnTypes;
        }

        public static Type GetRealType(this Type baseType, bool baseTypeIsDefault = true)
        {
            var types = baseType.GetRealTypes(baseTypeIsDefault);
            var type = types.Where(op => op != baseType && !op.Assembly.IsDynamic).FirstOrDefault();
            if (type == null && baseTypeIsDefault && !baseType.IsInterface)
                type = baseType;
            return type;
        }

        public static object GetRealTypeInstance(this Type baseType, bool baseTypeIsDefault = true, params object[] parameters)
        {
            try
            {
                var subType = baseType.GetRealType(baseTypeIsDefault);
                if (subType != null)
                {
                    return Activator.CreateInstance(subType, parameters);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public static Type ResolveType(this string typeName)
        {
            Type finalType = Type.GetType(typeName);
            try
            {
                foreach (var a in GetValidAssemblies())
                {
                    try
                    {
                        var Types = a.GetTypes().Where(op => op.FullName.Equals(typeName, StringComparison.OrdinalIgnoreCase));
                        if (Types.Any())
                        {
                            return Types.FirstOrDefault();
                        }
                    }
                    catch (Exception)
                    {
                        ExcludedAssemblies.Add(a.FullName);
                        continue;
                    }
                }
            }
            catch (Exception)
            {

            }
            return finalType;
        }

        public static string GetPropertyStringValue<TResult>(this TResult source, string property) where TResult : class
        {
            return source.GetPropertyValue(property)?.ToString();
        }

        public static bool IsStaticProperty(this PropertyInfo source)
        {
            return source.GetGetMethod().IsStatic;
        }

        public static object GetStaticPropertyValue(this PropertyInfo source)
        {
            return source.GetValue(null);
        }

        public static List<object> GetCustomAttributes(this MethodInfo type, Type attributeType)
        {
            if (type == null)
                return new List<object>();

            var list = type.GetCustomAttributes(true).Where(op => op.GetType().IsAssignableFrom(attributeType));
            return new List<object>(list);
        }

        public static List<object> GetCustomAttributes(this Type type, Type attributeType)
        {
            if (type == null)
                return new List<object>();

            var list = type.GetCustomAttributes(true).Where(op => op.GetType().IsAssignableFrom(attributeType));
            return new List<object>(list);
        }

        public static IEnumerable<object> GetCustomAttributes(this PropertyInfo info, Type attributeType, bool checkBase = false)
        {
            if (info == null)
                yield return new List<object>();

            var list = info.GetCustomAttributes(true);
            foreach (var op in list)
            {
                if (op.GetType().IsAssignableFrom(attributeType) || checkBase && op.GetType().UnderlyingSystemType.BaseType.Name != "Attribute" && op.GetType().UnderlyingSystemType.BaseType.IsAssignableFrom(attributeType))
                    yield return op;
            }
        }

        public static Type GetMemberInfoType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
            }
            return null;
        }

        private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(string)) return true;
            return type.IsValueType & type.IsPrimitive;
        }

        public static object Clone(this object originalObject)
        {
            return InternalCopy(originalObject, new Dictionary<object, object>(new ReferenceEqualityComparer()));
        }

        private static object InternalCopy(object originalObject, IDictionary<object, object> visited)
        {
            if (originalObject == null) return null;
            var typeToReflect = originalObject.GetType();
            if (typeToReflect.IsPrimitive()) return originalObject;
            if (visited.TryGetValue(originalObject, out object value)) return value;
            if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;
            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (!arrayType.IsPrimitive())
                {
                    Array clonedArray = (Array)cloneObject;
                    clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
                }

            }
            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (filter != null && filter(fieldInfo) == false) continue;
                if (fieldInfo.FieldType.IsPrimitive()) continue;
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }

        public static Type ToNullableType(this Type type)
        {
            if (type.IsValueType)
                return typeof(Nullable<>).MakeGenericType(type);
            return type;
        }

        public static TTarget CloneByProperties<TSource, TTarget>(this TSource source, List<string> excludeProperties = null) where TTarget : new()
        {
            var target = new TTarget();
            var sourceProps = typeof(TSource).GetProperties();
            var targetProps = typeof(TTarget).GetProperties();

            foreach (var sourceProp in sourceProps)
            {
                if (excludeProperties != null && excludeProperties.Contains(sourceProp.Name))
                    continue;

                var targetProp = targetProps.FirstOrDefault(p => p.Name == sourceProp.Name);
                if (targetProp != null && targetProp.CanWrite)
                {
                    var value = sourceProp.GetValue(source);
                    targetProp.SetValue(target, targetProp.PropertyType.ConvertData(value, false));
                }
            }

            return target;
        }
    }

    public class ReferenceEqualityComparer : EqualityComparer<object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        public override int GetHashCode(object obj)
        {
            if (obj == null) return 0;
            return obj.GetHashCode();
        }
    }
}