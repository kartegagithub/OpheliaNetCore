using System;
using System.Collections.Generic;

namespace Ophelia.Globalization.NumberToWords
{
    internal class BrazilianPortugueseConverter : Converter
    {
        public BrazilianPortugueseConverter(string Culturecode, string CurrencyCode)
            : base(Culturecode, CurrencyCode)
        {
            this.Ones = new string[] { "zero", "um", "dois", "três", "quatro", "cinco", "seis", "sete", "oito", "nove", "dez", "onze", "doze", "treze", "quatorze", "quinze", "dezesseis", "dezessete", "dezoito", "dezenove" };
            this.Tens = new string[] { "vinte", "trinta", "quarenta", "cinquenta", "sessenta", "setenta", "oitenta", "noventa" };
            this.Groups = new string[] { "cento", "migliaia", "milione", "miliardo", "trilhão", "dieci alla ventiquattresima", "quintillion" };
            this.CurrencyName = "Euro";
            this.PluralCurrencyName = "Euro";
            this.PartPrecision = 2;
            this.Prefix = "há pouco";
            this.AndOperatorString = " e ";
            this.CurrencyPartName = "centavo";
            this.PluralCurrencyPartName = "centavo";
        }

        //private static string ApplyGender(string toWords, GrammaticalGender gender)
        //{
        //    if (gender != GrammaticalGender.Feminine) 
        //        return toWords;

        //    if (toWords.EndsWith("os"))
        //        return toWords.Substring(0, toWords.Length - 2) + "as";

        //    if (toWords.EndsWith("um"))
        //        return toWords.Substring(0, toWords.Length - 2) + "uma";

        //    if (toWords.EndsWith("dois"))
        //        return toWords.Substring(0, toWords.Length - 4) + "duas";

        //    return toWords;
        //}
    }
}