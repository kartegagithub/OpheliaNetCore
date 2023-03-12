using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ophelia.Globalization.NumberToWords
{
    internal class UzbekConverter : Converter
    {
        private static readonly string[] UnitsMap = { "nol", "bir", "ikki", "uch", "to`rt", "besh", "olti", "yetti", "sakkiz", "to`qqiz" };
        private static readonly string[] TensMap = { "nol", "o`n", "yigirma", "o`ttiz", "qirq", "ellik", "oltmish", "yetmish", "sakson", "to`qson" };
    }
}
