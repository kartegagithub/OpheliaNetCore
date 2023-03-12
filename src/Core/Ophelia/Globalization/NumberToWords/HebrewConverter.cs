using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ophelia.Globalization.NumberToWords
{
    internal class HebrewConverter : Converter
    {
        private static readonly string[] UnitsFeminine = { "אפס", "אחת", "שתיים", "שלוש", "ארבע", "חמש", "שש", "שבע", "שמונה", "תשע", "עשר" };
        private static readonly string[] UnitsMasculine = { "אפס", "אחד", "שניים", "שלושה", "ארבעה", "חמישה", "שישה", "שבעה", "שמונה", "תשעה", "עשרה" };
        private static readonly string[] TensUnit = { "עשר", "עשרים", "שלושים", "ארבעים", "חמישים", "שישים", "שבעים", "שמונים", "תשעים" };
    }
}