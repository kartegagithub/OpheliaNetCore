using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Globalization.NumberToWords
{
    public class TurkishConvertor : Converter
    {
        public TurkishConvertor(string Culturecode, string CurrencyCode)
            : base(Culturecode, CurrencyCode)
        {
            this.Ones = new string[] { "Sıfır", "Bir", "İki", "Üç", "Dört", "Beş", "Altı", "Yedi", "Sekiz", "Dokuz", "On", "On bir", "On iki", "On üç", "On dört", "On beş", "On altı", "On yedi", "On sekiz", "On dokuz" };
            this.Tens = new string[] { "Yirmi", "Otuz", "Kırk", "Elli", "Altmış", "Yetmiş", "Seksen", "Doksan" };
            this.Groups = new string[] { "Yüz", "Bin", "Milyon", "Milyar", "Trilyon", "Katrilyon", "Kentilyon" };
            this.CurrencyName = "Türk Lirası";
            this.PluralCurrencyName = "Türk Lirası";
            this.PartPrecision = 2;
            this.Prefix = "Yalnızca";
            this.AndOperatorString = " ve ";
            this.CurrencyPartName = "Kuruş";
            this.PluralCurrencyPartName = "Kuruş";
        }

        protected override string ValidatePart(string value, int Hundreds, int Tens, int Ones)
        {
            if (Hundreds == 1 && value.Equals("Bir Yüz", StringComparison.InvariantCultureIgnoreCase))
                return "Yüz";
            else
                return base.ValidatePart(value, Hundreds, Tens, Ones);
        }
    }
}
