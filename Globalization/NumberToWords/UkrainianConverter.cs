using System;
using System.Collections.Generic;

namespace Ophelia.Globalization.NumberToWords
{
    internal class UkrainianConverter : Converter
    {
        private static readonly string[] HundredsMap = { "нуль", "сто", "двісті", "триста", "чотириста", "п'ятсот", "шістсот", "сімсот", "вісімсот", "дев'ятсот" };
        private static readonly string[] TensMap = { "нуль", "десять", "двадцять", "тридцять", "сорок", "п'ятдесят", "шістдесят", "сімдесят", "вісімдесят", "дев'яносто" };
        private static readonly string[] UnitsMap = { "нуль", "один", "два", "три", "чотири", "п'ять", "шість", "сім", "вісім", "дев'ять", "десять", "одинадцять", "дванадцять", "тринадцять", "чотирнадцять", "п'ятнадцять", "шістнадцять", "сімнадцять", "вісімнадцять", "дев'ятнадцять" };
        
        //private static string GetEndingForGender(GrammaticalGender gender, int number)
        //{
        //    switch (gender)
        //    {
        //        case GrammaticalGender.Masculine:
        //            if (number == 3)
        //                return "ій";
        //            return "ий";
        //        case GrammaticalGender.Feminine:
        //            if (number == 3)
        //                return "я";
        //            return "а";
        //        case GrammaticalGender.Neuter:
        //            if (number == 3)
        //                return "є";
        //            return "е";
        //        default:
        //            throw new ArgumentOutOfRangeException("gender");
        //    }
        //}
    }
}
