using System;
using System.Collections.Generic;

namespace Ophelia.Globalization.NumberToWords
{
    internal class FrenchConverter : Converter
    {
        public FrenchConverter(string Culturecode, string CurrencyCode)
            : base(Culturecode, CurrencyCode)
        {
            this.Ones = new string[] { "zéro", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf", "dix", "onze", "douze", "treize", "quatorze", "quinze", "seize", "dix-sept", "dix-huit", "dix-neuf" };
            this.Tens = new string[] { "vingt", "trente", "quarante", "cinquante", "soixante", "soixante-dix", "quatre-vingt", "quatre-vingt-dix" };
            this.Groups = new string[] { "cent", "mille", "million", "milliard", "trillion", "quadrillion", "quintillion" };
            this.CurrencyName = "Euro";
            this.PluralCurrencyName = "Euro";
            this.PartPrecision = 2;
            this.Prefix = "juste";
            this.AndOperatorString = " et ";
            this.CurrencyPartName = "cent";
            this.PluralCurrencyPartName = "cent";
        }
    }
}
