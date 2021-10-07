using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ophelia.Reflection
{
    public class ObjectIterator : IDisposable
    {
        public int MaxIterationCount { get; set; }
        public int IterationCount { get; private set; }
        private List<object> IteratedObjects { get; set; }
        public Func<object, object> IterationCallback { get; set; }
        public void Dispose()
        {
            this.IteratedObjects.Clear();
            this.IteratedObjects = null;
        }

        public ObjectIterator Iterate(object obj)
        {
            if (obj == null)
                return this;
            if (!obj.GetType().IsClass)
                return this;

            this.IterateInternal(obj);
            return this;
        }
        private void IterateInternal(object obj)
        {
            this.IterationCount += 1;
            if (this.IterationCount >= this.MaxIterationCount)
                return;

            var props = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in props)
            {
                if (p.PropertyType.IsClass && !p.PropertyType.IsAbstract && p.GetMethod != null && p.GetMethod.IsPublic) 
                {
                    try
                    {
                        if (p.Name == "Parameters" || p.GetMethod.GetParameters().Length > 0)
                            continue;

                        var val = p.GetValue(obj);
                        if (val == null || this.IteratedObjects.Contains(val))
                            continue;

                        this.IteratedObjects.Add(val);
                        if (this.IterationCallback != null)
                            this.IterationCallback(val);
                        this.IterateInternal(val);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }                    
                }
            }
        }
        public ObjectIterator()
        {
            this.IteratedObjects = new List<object>();
            this.MaxIterationCount = 100;
        }
    }
}
