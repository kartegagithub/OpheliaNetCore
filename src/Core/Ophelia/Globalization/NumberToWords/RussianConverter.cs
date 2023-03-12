using System;
using System.Collections.Generic;

namespace Ophelia.Globalization.NumberToWords
{
    internal class RussianConverter : Converter
    {
        private static readonly string[] HundredsMap = { "ноль", "сто", "двести", "триста", "четыреста", "пятьсот", "шестьсот", "семьсот", "восемьсот", "девятьсот" };
        private static readonly string[] TensMap = { "ноль", "десять", "двадцать", "тридцать", "сорок", "пятьдесят", "шестьдесят", "семьдесят", "восемьдесят", "девяносто" };
        private static readonly string[] UnitsMap = { "ноль", "один", "два", "три", "четыре", "пять", "шесть", "семь", "восемь", "девять", "десять", "одиннадцать", "двенадцать", "тринадцать", "четырнадцать", "пятнадцать", "шестнадцать", "семнадцать", "восемнадцать", "девятнадцать" };
        
        //private static string GetEndingForGender(GrammaticalGender gender, int number)
        //{
        //    switch (gender)
        //    {
        //        case GrammaticalGender.Masculine:
        //            if (number == 0 || number == 2 || number == 6 || number == 7 || number == 8 || number == 40)
        //                return "ой";
        //            if (number == 3)
        //                return "ий";
        //            return "ый";
        //        case GrammaticalGender.Feminine:
        //            if (number == 3)
        //                return "ья";
        //            return "ая";
        //        case GrammaticalGender.Neuter:
        //            if (number == 3)
        //                return "ье";
        //            return "ое";
        //        default:
        //            throw new ArgumentOutOfRangeException("gender");
        //    }
        //}
    }
}
