﻿using System;

namespace Ophelia.Data.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataProperty : Attribute
    {
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool Unicode { get; set; }
        public DataProperty()
        {
            this.Unicode = true;
        }
        public DataProperty(int length) : this()
        {
            this.Precision = length;
        }
        public DataProperty(int precision, int scale) : this()
        {
            this.Precision = precision;
            this.Scale = scale;
        }
        public DataProperty(bool uniCode) : this()
        {
            this.Unicode = uniCode;
        }
        public DataProperty(bool uniCode, int length) : this()
        {
            this.Unicode = uniCode;
            this.Precision = length;
        }
    }
}
