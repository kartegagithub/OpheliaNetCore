using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Ophelia.Globalization.NumberToWords
{
    internal class SerbianConverter : Converter
    {
        private static readonly string[] UnitsMap = { "nula", "jedan", "dva", "tri", "četiri", "pet", "šest", "sedam", "osam", "devet", "deset", "jedanaest", "dvanaest", "trinaest", "četrnaestt", "petnaest", "šestnaest", "sedemnaest", "osemnaest", "devetnaest" };
        private static readonly string[] TensMap = { "nula", "deset", "dvadeset", "trideset", "četrdeset", "petdeset", "šestdeset", "sedamdeset", "osamdeset", "devetdeset" };
    }
}
