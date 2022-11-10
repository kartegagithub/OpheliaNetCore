using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Ophelia.Reflection
{
    public class Accessor
    {
        private object oItem;
        private object oValue;
        private string sMemberName = "";
        private object oValueItem;
        private string ValueMemberName = "";
        public event ItemInitiliazedEventHandler ItemInitiliazed;
        public event NullReferenceEventDelegate NullReferenceEventHandler;
        public delegate void ItemInitiliazedEventHandler(object Item);
        public delegate void NullReferenceEventDelegate(object Item, PropertyInfo prop);
        public event ValueInitiliazedEventHandler ValueInitiliazed;
        public delegate void ValueInitiliazedEventHandler(ref object Value);
        private bool bIgnoreNullReferences = false;
        public MethodCallExpression MethodCallExpression { get; set; }
        public object Item
        {
            get { return this.oItem; }
            set
            {
                this.oItem = value;
                this.Refresh();
                if (ItemInitiliazed != null)
                {
                    ItemInitiliazed(this.oItem);
                }
            }
        }
        public object Value
        {
            get
            {
                this.GetValue();
                return this.oValue;
            }
            set
            {
                if ((!object.ReferenceEquals(value, this.oValue)))
                {
                    this.oValue = value;
                    this.SetValue();
                }
            }
        }
        public string MemberName
        {
            get { return this.sMemberName; }
            set { this.sMemberName = value; }
        }
        public bool IgnoreNullReferences
        {
            get { return this.bIgnoreNullReferences; }
            set { this.bIgnoreNullReferences = value; }
        }
        public object ValueItem
        {
            get
            {
                this.GetValue();
                return this.oValueItem;
            }
        }
        public void ResetItem()
        {
            this.oItem = null;
        }
        public void UpdateValue(object Value)
        {
            this.oValue = Value;
        }
        public void Refresh()
        {
            this.oValue = null;
            this.oValueItem = null;
            this.ValueMemberName = "";
        }
        protected virtual object GetValue()
        {
            if ((this.Item != null))
            {
                if (!string.IsNullOrEmpty(this.MemberName))
                {
                    string Index = "";
                    bool Indexed = false;
                    if (this.MemberName == "ItemItself")
                    {
                        this.oValue = this.Item;
                    }
                    else
                    {
                        if (this.MemberName.IndexOf(".") > -1 || this.MemberName.IndexOf("(") > -1)
                        {
                            string[] sArrReferences = this.MemberName.Split('.');
                            object TempValue = this.Item;
                            int n = 0;
                            string TemporaryMemberName = "";
                            try
                            {
                                for (n = 0; n <= sArrReferences.Length - 1; n++)
                                {
                                    bool processed = false;
                                    if (TempValue != null)
                                    {
                                        TemporaryMemberName = sArrReferences[n];
                                        if (n == sArrReferences.Length - 1)
                                        {
                                            this.oValueItem = TempValue;
                                            this.ValueMemberName = sArrReferences[n];
                                        }
                                        if (TemporaryMemberName.IndexOf("(") > -1)
                                        {
                                            Indexed = true;
                                            Index = TemporaryMemberName.Substring(TemporaryMemberName.IndexOf("(") + 1, TemporaryMemberName.Length - TemporaryMemberName.IndexOf("(") - 2);
                                            TemporaryMemberName = TemporaryMemberName.Substring(0, TemporaryMemberName.Length - 2 - Index.ToString().Length);
                                        }
                                        else if (TemporaryMemberName.IndexOf("[") > -1)
                                        {
                                            var tmp = TemporaryMemberName.Left(TemporaryMemberName.IndexOf("["));
                                            var index = Convert.ToInt32(TemporaryMemberName.Replace(tmp, "").Replace("[", "").Replace("]", ""));
                                            TemporaryMemberName = tmp;
                                            tmp = "";
                                            TempValue = this.GetPropertyInfo(TempValue, TemporaryMemberName).GetValue(TempValue, null);

                                            MethodInfo method = null;
                                            if (TempValue.GetType().GetProperty("Length") != null)
                                            {
                                                if (Convert.ToInt32(TempValue.GetPropertyValue("Length")) > 0)
                                                {
                                                    method = TempValue.GetType().GetMethod("Get");
                                                    TempValue = method.Invoke(TempValue, new object[] { index });
                                                }
                                                else
                                                    TempValue = null;
                                            }
                                            else if (TempValue.GetType().GetProperty("Count") != null)
                                            {
                                                if (Convert.ToInt32(TempValue.GetPropertyValue("Count")) > 0)
                                                {
                                                    method = TempValue.GetType().GetMethod("Get");
                                                    TempValue = method.Invoke(TempValue, new object[] { index });
                                                }
                                                else
                                                    TempValue = null;
                                            }
                                            else
                                            {
                                                TempValue = null;
                                            }
                                            processed = true;
                                        }
                                        if (!processed)
                                        {
                                            if (Indexed)
                                            {
                                                ArrayList Parameters = this.ReArrangeParameters(Index);
                                                try
                                                {
                                                    var property = this.GetPropertyInfo(TempValue, TemporaryMemberName);
                                                    if (property != null)
                                                    {
                                                        if (Parameters != null)
                                                            TempValue = property.GetValue(TempValue, Parameters.ToArray());
                                                        else
                                                            TempValue = property.GetValue(TempValue, null);
                                                    }
                                                    else
                                                    {
                                                        if (this.MethodCallExpression != null)
                                                        {
                                                            if (this.MethodCallExpression.Method.IsStatic)
                                                            {
                                                                Parameters = new ArrayList();
                                                                Parameters.Add(TempValue);
                                                                TempValue = this.MethodCallExpression.Method.Invoke(null, Parameters.ToArray());
                                                            }
                                                            else
                                                            {
                                                                if (Parameters == null)
                                                                {
                                                                    TempValue = this.MethodCallExpression.Method.Invoke(TempValue, null);
                                                                }
                                                                else
                                                                {
                                                                    TempValue = this.MethodCallExpression.Method.Invoke(TempValue, Parameters.ToArray());
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (Parameters == null)
                                                            {
                                                                TempValue = TempValue.GetType().InvokeMember(TemporaryMemberName, System.Reflection.BindingFlags.InvokeMethod, null, TempValue, null);
                                                            }
                                                            else
                                                            {
                                                                TempValue = TempValue.GetType().InvokeMember(TemporaryMemberName, System.Reflection.BindingFlags.InvokeMethod, null, TempValue, Parameters.ToArray());
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    if ((TempValue == null))
                                                    {
                                                        throw new Exception("Before " + TemporaryMemberName + " property binded, binding value became nothing.", ex);
                                                    }
                                                    else
                                                    {
                                                        throw new Exception(TemporaryMemberName + " property not found at the type of " + TempValue.GetType().FullName, ex);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if ((TempValue != null))
                                                {
                                                    var propertyInfo = this.GetPropertyInfo(TempValue, TemporaryMemberName);
                                                    var preTempValue = TempValue;
                                                    TempValue = propertyInfo.GetValue(preTempValue, null);
                                                    if (TempValue == null && this.NullReferenceEventHandler != null)
                                                    {
                                                        this.NullReferenceEventHandler(preTempValue, propertyInfo);
                                                        TempValue = propertyInfo.GetValue(preTempValue, null);
                                                    }
                                                }
                                            }
                                        }
                                        Indexed = false;
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                this.oValue = TempValue;
                                this.ValueMemberName = TemporaryMemberName;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Exception occured while getting requested property. " + (!string.IsNullOrEmpty(this.ValueMemberName) ? "Value Member : " + this.ValueMemberName + "," : "") + " Member of Value Member : " + TemporaryMemberName, ex);
                            }
                        }
                        else
                        {
                            this.ValueMemberName = this.MemberName;
                            try
                            {
                                this.oValue = this.GetPropertyInfo(this.Item, this.ValueMemberName).GetValue(this.Item, null);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(this.ValueMemberName + " property not found at the type of " + this.Item.GetType().FullName, ex.InnerException);
                            }
                        }
                        if (ValueInitiliazed != null)
                        {
                            ValueInitiliazed(ref this.oValue);
                        }
                    }
                }
            }
            if ((this.oValueItem == null))
            {
                this.oValueItem = this.Item;
            }
            return this.oValue;
        }
        protected virtual object SetValue()
        {
            this.oValueItem = this.Item;
            this.ValueMemberName = this.MemberName;
            if ((this.oValueItem != null) && !string.IsNullOrEmpty(this.MemberName))
            {
                System.Reflection.PropertyInfo PropertyInfo = null;
                if (this.MemberName.IndexOf('.') > -1 || this.MemberName.IndexOf('(') > -1)
                {
                    string[] sArrReferences = this.MemberName.Split('.');
                    object TempValue = this.Item;
                    int n = 0;
                    string TemporaryMemberName = "";
                    bool Indexed = false;
                    string Index = "";
                    for (n = 0; n <= sArrReferences.Length - 2; n++)
                    {
                        if (!(TempValue == null && this.IgnoreNullReferences))
                        {
                            TemporaryMemberName = sArrReferences[n];
                            if (n == sArrReferences.Length - 1)
                            {
                                this.oValueItem = TempValue;
                                this.ValueMemberName = sArrReferences[n];
                            }
                            if (TemporaryMemberName.IndexOf("(") > -1)
                            {
                                Indexed = true;
                                Index = TemporaryMemberName.Substring(TemporaryMemberName.IndexOf("(") + 1, TemporaryMemberName.Length - TemporaryMemberName.IndexOf("(") - 2);
                                TemporaryMemberName = TemporaryMemberName.Substring(0, TemporaryMemberName.Length - 2 - Index.ToString().Length);
                            }
                            try
                            {
                                if (Indexed)
                                {
                                    ArrayList Parameters = this.ReArrangeParameters(Index);
                                    try
                                    {
                                        TempValue = this.GetPropertyInfo(TempValue, TemporaryMemberName).GetValue(TempValue, Parameters.ToArray());
                                    }
                                    catch (Exception ex)
                                    {
                                        try
                                        {
                                            if (Parameters != null)
                                            {
                                                TempValue = TempValue.GetType().InvokeMember(TemporaryMemberName, System.Reflection.BindingFlags.InvokeMethod, null, TempValue, Parameters.ToArray());
                                            }
                                            else
                                            {
                                                TempValue = TempValue.GetType().InvokeMember(TemporaryMemberName, System.Reflection.BindingFlags.InvokeMethod, null, TempValue, null);
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            if ((TempValue == null))
                                            {
                                                throw new Exception("Before " + TemporaryMemberName + " property binded, binding value became nothing.", ex);
                                            }
                                            else
                                            {
                                                throw new Exception(TemporaryMemberName + " property not found at the type of " + TempValue.GetType().FullName, ex);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    TempValue = this.GetPropertyInfo(TempValue, TemporaryMemberName).GetValue(TempValue, null);
                                }
                            }
                            catch (Exception Ex)
                            {
                                if ((TempValue == null))
                                {
                                    throw new Exception("Before " + TemporaryMemberName + " property binded, binding value became nothing.", Ex);
                                }
                                else
                                {
                                    throw new Exception(TemporaryMemberName + " property not found at the type of " + TempValue.GetType().FullName, Ex);
                                }
                            }
                            Indexed = false;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    if (n == sArrReferences.Length - 1)
                    {
                        if (sArrReferences[n].IndexOf("(") > -1)
                        {
                            if (sArrReferences[n].IndexOf("(") > -1)
                            {
                                Indexed = true;
                                TemporaryMemberName = sArrReferences[n];
                                Index = TemporaryMemberName.Substring(TemporaryMemberName.IndexOf("(") + 1, TemporaryMemberName.Length - TemporaryMemberName.IndexOf("(") - 2);
                                TemporaryMemberName = sArrReferences[n].Substring(0, sArrReferences[n].Length - 2 - Index.ToString().Length);
                            }
                            ArrayList Parameters = this.ReArrangeParameters(Index);
                            this.GetPropertyInfo(TempValue, TemporaryMemberName).SetValue(TempValue, this.oValue, Parameters.ToArray());
                        }
                        else
                        {
                            PropertyInfo = this.GetPropertyInfo(TempValue, sArrReferences[n]);
                            if ((this.oValue != null) && (this.oValue.GetType().GetInterface("IConvertible") != null))
                            {
                                TypeCode TypeCode = Type.GetTypeCode(PropertyInfo.PropertyType);
                                this.oValue = System.Convert.ChangeType(this.oValue, TypeCode);
                            }
                            PropertyInfo.SetValue(TempValue, this.oValue, null);
                        }
                    }
                }
                else
                {
                    PropertyInfo = this.GetPropertyInfo(this.oValueItem, this.ValueMemberName);
                    if ((PropertyInfo != null) && PropertyInfo.CanWrite)
                    {
                        if ((this.oValue != null) && (this.oValue.GetType().GetInterface("IConvertible") != null))
                        {
                            TypeCode TypeCode = Type.GetTypeCode(PropertyInfo.PropertyType);
                            this.oValue = System.Convert.ChangeType(this.oValue, TypeCode);
                        }
                        PropertyInfo.SetValue(this.oValueItem, this.oValue, null);
                    }
                }
            }
            return this.oValue;
        }
        public PropertyInfo GetPropertyInfo(object Item, string MemberName)
        {
            return Item.GetType().GetProperties().Where(Prop => Prop.Name.Equals(MemberName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }
        private ArrayList ReArrangeParameters(string RawParameters)
        {
            string[] Parameters = RawParameters.Split(',');
            ArrayList Array = new ArrayList();
            int n = 0;
            for (n = 0; n <= Parameters.Length - 1; n++)
            {
                if (Parameters[n].IsNumeric())
                {
                    if (Parameters[n].IndexOf('.') > -1)
                    {
                        Array.Add(Convert.ToDouble(Parameters[n]));
                    }
                    else
                    {
                        Array.Add(Convert.ToInt32(Parameters[n]));
                    }
                }
                else if (Parameters[n].IsDate())
                {
                    Array.Add(Convert.ToDateTime(Parameters[n]));
                }
                else if (!string.IsNullOrEmpty(Parameters[n]))
                {
                    Array.Add(Parameters[n].Replace("'", ""));
                }
            }
            if (Array.Count == 0)
                return null;
            return Array;
        }
    }
    public enum RefreshAction
    {
        None = 0,
        RebindCell = 1,
        RebindRow = 2,
        RebindBinder = 3,
        RefreshBinder = 4
    }
}
