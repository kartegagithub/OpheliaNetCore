using System.Collections.Generic;
using System.Globalization;

namespace Ophelia.Globalization.NumberToWords
{
    internal class SlovenianConverter : Converter
    {
        private static readonly string[] UnitsMap = {"nič", "ena", "dva", "tri", "štiri", "pet", "šest", "sedem", "osem", "devet", "deset", "enajst", "dvanajst", "trinajst", "štirinajst", "petnajst", "šestnajst", "sedemnajst", "osemnajst", "devetnajst"};
        private static readonly string[] TensMap = {"nič", "deset", "dvajset", "trideset", "štirideset", "petdeset", "šestdeset", "sedemdeset", "osemdeset", "devetdeset"};
    }
}