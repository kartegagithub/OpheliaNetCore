using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Ophelia.Globalization.NumberToWords
{
    internal class SerbianCyrlConverter : Converter
    {
        private static readonly string[] UnitsMap = { "нула", "један", "два", "три", "четири", "пет", "шест", "седам", "осам", "девет", "десет", "једанест", "дванаест", "тринаест", "четрнаест", "петнаест", "шестнаест", "седамнаест", "осамнаест", "деветнаест" };
        private static readonly string[] TensMap = { "нула", "десет", "двадесет", "тридесет", "четрдесет", "петдесет", "шестдесет", "седамдесет", "осамдесет", "деветдесет" };
    }
}
