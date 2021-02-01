using System.Collections.Generic;
using System.Linq;

namespace Ophelia.Globalization.NumberToWords
{
    internal class DutchConverter : Converter
    {
        public DutchConverter(string Culturecode, string CurrencyCode)
            : base(Culturecode, CurrencyCode)
        {
            this.Ones = new string[] { "nul", "een", "twee", "drie", "vier", "vijf", "zes", "zeven", "acht", "negen", "tien", "elf", "twaalf", "dertien", "veertien", "vijftien", "zestien", "zeventien", "achttien", "negentien" };
            this.Tens = new string[] { "twintig", "dertig", "veertig", "vijftig", "zestig", "zeventig", "tachtig", "negentig" };
            this.Groups = new string[] { "honderdtal", "duizend", "miljoen", "bilhão", "triljoen", "quadriljoen", "triljoen" };
            this.CurrencyName = "Euro";
            this.PluralCurrencyName = "Euro";
            this.PartPrecision = 2;
            this.Prefix = "net";
            this.AndOperatorString = " en ";
            this.CurrencyPartName = "cent";
            this.PluralCurrencyPartName = "cent";
        }
    }
}