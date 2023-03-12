using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ophelia.Globalization.NumberToWords
{
    internal class UzbekCyrlConverter : Converter
    {
        private static readonly string[] UnitsMap = { "нол", "бир", "икки", "уч", "тўрт", "беш", "олти", "етти", "саккиз", "тўққиз" };
        private static readonly string[] TensMap = { "нол", "ўн", "йигирма", "ўттиз", "қирқ", "эллик", "олтмиш", "етмиш", "саксон", "тўқсон" };
    }
}
