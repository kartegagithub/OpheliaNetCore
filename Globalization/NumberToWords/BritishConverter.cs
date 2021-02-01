using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Globalization.NumberToWords
{
    public class BritishConverter: AmericanConverter
    {
        public BritishConverter(string Culturecode, string CurrencyCode)
            : base(Culturecode, CurrencyCode)
        {
            this.CurrencyName = "Pound";
            this.PluralCurrencyName = "Pound";
            this.PartPrecision = 2;
            this.CurrencyPartName = "cent";
            this.PluralCurrencyPartName = "cent";
        }
    }
}
