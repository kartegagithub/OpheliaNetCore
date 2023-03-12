using System;
using System.Collections.Generic;

namespace Ophelia.Globalization.NumberToWords
{
    internal class GermanConverter : Converter
    {
        public GermanConverter(string Culturecode, string CurrencyCode)
            : base(Culturecode, CurrencyCode)
        {
            this.Ones = new string[] { "null", "ein", "zwei", "drei", "vier", "fünf", "sechs", "sieben", "acht", "neun", "zehn", "elf", "zwölf", "dreizehn", "vierzehn", "fünfzehn", "sechzehn", "siebzehn", "achtzehn", "neunzehn" };
            this.Tens = new string[] { "zwanzig", "dreißig", "vierzig", "fünfzig", "sechzig", "siebzig", "achtzig", "neunzig" };
            this.Groups = new string[] { "hundert", "tausend", "million", "milliarde", "trillion", "billiarde", "quintillion" };
            this.CurrencyName = "Euro";
            this.PluralCurrencyName = "Euro";
            this.PartPrecision = 2;
            this.Prefix = "nur";
            this.AndOperatorString = " und ";
            this.CurrencyPartName = "cent";
            this.PluralCurrencyPartName = "cent";
        }

        //private static string GetEndingForGender(GrammaticalGender gender)
        //{
        //    switch (gender)
        //    {
        //        case GrammaticalGender.Masculine:
        //            return "ter";
        //        case GrammaticalGender.Feminine:
        //            return "te";
        //        case GrammaticalGender.Neuter:
        //            return "tes";
        //        default:
        //            throw new ArgumentOutOfRangeException("gender");
        //    }
        //}
    }
}