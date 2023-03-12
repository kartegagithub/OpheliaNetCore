using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ophelia
{
    public static class ReflectionExtensions
    {
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
        public static object ConvertData(this Type targetType, object value)
        {
            try
            {
                object convertedValue = null;
                var isNull = value == null;
                if (!isNull && string.IsNullOrEmpty(Convert.ToString(value)))
                    isNull = true;

                if ((targetType == typeof(bool) || targetType == typeof(bool?)))
                {
                    convertedValue = isNull ? default(bool) : Convert.ToInt64(value) != 0;
                }
                else if ((targetType == typeof(byte) || targetType == typeof(byte?)))
                {
                    convertedValue = isNull ? default(byte) : Convert.ToByte(value);
                }
                else if ((targetType == typeof(Int32) || targetType == typeof(Int32?)))
                {
                    convertedValue = isNull ? default(Int32) : Convert.ToInt32(value);
                }
                else if ((targetType == typeof(Int16) || targetType == typeof(Int16?)))
                {
                    convertedValue = isNull ? default(Int16) : Convert.ToInt16(value);
                }
                else if ((targetType == typeof(Int64) || targetType == typeof(Int64?)))
                {
                    convertedValue = isNull ? default(Int64) : Convert.ToInt64(value);
                }
                else if ((targetType == typeof(decimal) || targetType == typeof(decimal?)))
                {
                    convertedValue = isNull ? default(decimal) : Convert.ToDecimal(value);
                }
                else if ((targetType == typeof(double) || targetType == typeof(double?)))
                {
                    convertedValue = isNull ? default(double) : Convert.ToDouble(value);
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
                            convertedValue = Convert.ChangeType(value, targetType); // this will throw for "1"
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
        public static bool IsDecimal(this Type type)
        {
            if (type == null)
                return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsNumeric(this Type type)
        {
            if (type == null)
                return false;
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
            return Convert.ToByte(ToInt64(type, value));
        }
        public static Int16 ToInt16(this Type type, object value)
        {
            return Convert.ToInt16(ToInt64(type, value));
        }
        public static Int32 ToInt32(this Type type, object value)
        {
            return Convert.ToInt32(ToInt64(type, value));
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
                    return ((bool)value) ? 1 : 0;
                case TypeCode.Char:
                    return Convert.ToInt64(Char.GetNumericValue((char)value));
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
            var ns = arr[arr.Length - 2];
            arr = null;
            return ns;
        }
        public static object ExecuteMethod<T>(this T entity, string method, params object[] parameters) where T : class
        {
            if (entity != null)
            {
                var methods = entity.GetType().GetMethods().Where(op => op.Name.Equals(method, StringComparison.InvariantCultureIgnoreCase)).ToList();
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
            foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies())
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
            if (property.IndexOf(".") > -1)
            {
                var splitted = property.Split('.');
                var tmpType = type;
                foreach (var item in splitted)
                {
                    prop = tmpType.GetProperties().Where(op => op.Name.Equals(item, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
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
                prop = type.GetProperties().Where(op => op.Name.Equals(property, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (prop == null && type.IsGenericType)
                    prop = type.GenericTypeArguments[0].GetPropertyInfo(property);
            }
            return prop;
        }
        public static PropertyInfo[] GetPropertyInfoTree(this Type type, string property)
        {
            var props = new List<PropertyInfo>();
            if (property.IndexOf(".") > -1)
            {
                foreach (var p in property.Split('.'))
                {
                    var prop = type.GetProperties().FirstOrDefault(op => op.Name.Equals(p, StringComparison.InvariantCultureIgnoreCase));
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
                props.Add(type.GetProperties().Where(op => op.Name.Equals(property, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault());
            return props.ToArray();
        }
        public static object GetPropertyValue<TResult>(this TResult source, string property) where TResult : class
        {
            if (source != null)
            {
                if (property.IndexOf(".") > -1)
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
                    var props = source.GetType().GetProperties().Where(op => op.Name.Equals(property, StringComparison.InvariantCultureIgnoreCase));
                    PropertyInfo propInfo = null;
                    if (props.Count() > 1)
                        propInfo = props.Where(op => op.DeclaringType == source.GetType()).FirstOrDefault();
                    if (props.Count() > 0)
                        propInfo = props.FirstOrDefault();
                    if (propInfo != null)
                    {
                        if (propInfo.IsStaticProperty())
                            return propInfo.GetStaticPropertyValue();
                        else
                            return propInfo.GetValue(source);
                    }
                    props = null;

                    var fields = source.GetType().GetFields().Where(op => op.Name.Equals(property, StringComparison.InvariantCultureIgnoreCase));
                    if (fields.Count() > 1)
                        return fields.Where(op => op.DeclaringType == source.GetType()).FirstOrDefault()?.GetValue(source);
                    if (fields.Count() > 0)
                        return fields.FirstOrDefault()?.GetValue(source);
                }
            }
            return null;
        }
        public static void SetPropertyValue<TResult>(this TResult source, string property, object value) where TResult : class
        {
            if (source != null)
            {
                var props = source.GetType().GetProperties().Where(op => op.Name.Equals(property, StringComparison.InvariantCultureIgnoreCase));
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
        public static bool IsEnumarable(this Type type)
        {
            return type.IsGenericType && (type.IsAssignableFrom(typeof(System.Collections.IEnumerable)) || typeof(System.Collections.IEnumerable).IsAssignableFrom(type));
        }
        public static List<PropertyInfo> GetPropertiesByType(this Type ObjectType, Type PropertyTpe)
        {
            if (ObjectType != null && PropertyTpe != null)
                return ObjectType.GetProperties().Where(op => op.PropertyType == PropertyTpe).ToList();
            else
                return null;
        }
        public static List<Type> GetSimilarTypes(this string ObjectType, bool exactMatch = false)
        {
            var types = new List<Type>();
            try
            {
                foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        var Types = a.GetTypes();
                        if (Types != null)
                        {
                            if (exactMatch)
                                Types = Types.Where(op => op.Name.Equals(ObjectType, StringComparison.InvariantCultureIgnoreCase) || op.FullName.Equals(ObjectType, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                            else
                                Types = Types.Where(op => op.FullName.IndexOf(ObjectType, StringComparison.InvariantCultureIgnoreCase) > -1).ToArray();
                            if (Types != null && Types.Length > 0)
                            {
                                types.Add(Types.FirstOrDefault());
                            }
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                return types;
            }
            return types;
        }
        public static Type GetRealType(this string baseType, bool baseTypeIsDefault = true)
        {
            try
            {
                foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        if (a.FullName.Contains(".Redis") || a.FullName.StartsWith("Microsoft.") || a.FullName.StartsWith("System.") || a.FullName.StartsWith("Newtonsoft."))
                            continue;

                        var Types = a.GetTypes();
                        if (Types != null)
                        {
                            Types = Types.Where(op => !op.IsInterface && op.FullName.Equals(baseType, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                            if (Types != null && Types.Length > 0)
                            {
                                return Types.FirstOrDefault();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
        }
        public static List<Type> GetRealTypes(this Type baseType, bool baseTypeIsDefault = true)
        {
            var returnTypes = new List<Type>();
            try
            {

                foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        if (a.FullName.Contains(".Redis") || a.FullName.StartsWith("Microsoft.") || a.FullName.StartsWith("System.") || a.FullName.StartsWith("Newtonsoft."))
                            continue;

                        var Types = a.GetTypes();
                        if (Types != null)
                        {
                            Types = Types.Where(op => !op.IsInterface && (op.IsSubclassOf(baseType) || baseType.IsAssignableFrom(op))).ToArray();
                            if (Types != null && Types.Length > 0)
                            {
                                if (Types.FirstOrDefault() != baseType)
                                {
                                    returnTypes.AddRange(Types);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return returnTypes;
        }
        public static Type GetRealType(this Type baseType, bool baseTypeIsDefault = true)
        {
            var types = baseType.GetRealTypes(baseTypeIsDefault);
            var type = types.Where(op => op != baseType).FirstOrDefault();
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
                foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        if (a.FullName.Contains(".Redis") || a.FullName.StartsWith("Microsoft.") || a.FullName.StartsWith("System.") || a.FullName.StartsWith("Newtonsoft."))
                            continue;

                        var Types = a.GetTypes();
                        if (Types != null)
                        {
                            Types = Types.Where(op => op.FullName.Equals(typeName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                            if (Types != null && Types.Length > 0)
                            {
                                finalType = Types.FirstOrDefault();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                return finalType;
            }
            return finalType;
        }
        public static string GetPropertyStringValue<TResult>(this TResult source, string property) where TResult : class
        {
            return GetPropertyValue(source, property)?.ToString();
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
                if (op.GetType().IsAssignableFrom(attributeType) || (checkBase && op.GetType().UnderlyingSystemType.BaseType.Name != "Attribute" && op.GetType().UnderlyingSystemType.BaseType.IsAssignableFrom(attributeType)))
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


        private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(String)) return true;
            return (type.IsValueType & type.IsPrimitive);
        }

        public static Object Clone(this Object originalObject)
        {
            return InternalCopy(originalObject, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
        }

        private static Object InternalCopy(Object originalObject, IDictionary<Object, Object> visited)
        {
            if (originalObject == null) return null;
            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect)) return originalObject;
            if (visited.ContainsKey(originalObject)) return visited[originalObject];
            if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;
            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (IsPrimitive(arrayType) == false)
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
                if (IsPrimitive(fieldInfo.FieldType)) continue;
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }
    }

    public class ReferenceEqualityComparer : EqualityComparer<Object>
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
