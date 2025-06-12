using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Ophelia;

namespace Ophelia
{
    public static class HolidayExtensions
    {
        public static List<PublicHoliday> TurkishHolidays(this int year)
        {
            var items = new List<PublicHoliday>
            {
                new PublicHoliday(year, 1, 1, "Yılbaşı", "New Year's Day"),
                new PublicHoliday(year, 4, 23, "Ulusal Egemenlik ve Çocuk Bayramı", "National Independence & Children's Day", 1920),
                new PublicHoliday(year, 5, 1, "İşçi Bayramı", "Labour Day"),
                new PublicHoliday(year, 5, 19, "Atatürk'ü Anma, Gençlik ve Spor Bayramı", "Atatürk Commemoration & Youth Day", 1919),
                new PublicHoliday(year, 7, 15, "Demokrasi Bayramı", "Democracy Day", 2016),
                new PublicHoliday(year, 8, 30, "Zafer Bayramı", "Victory Day", 1922),
                new PublicHoliday(year, 10, 28, "Cumhuriyet Bayramı Arifesi", "Republic Day Eve", 1923, true),
                new PublicHoliday(year, 10, 29, "Cumhuriyet Bayramı", "Republic Day", 1923)
            };

            UmAlQuraCalendar umAlQura = new UmAlQuraCalendar();
            var nowUmAlQuraHijriYear = umAlQura.GetYear(new DateTime(year, 1, 1));

            // Ramazan Bayramı eklemesi
            // Ramazan Bayramı Arifesi 
            var ramadanEve = umAlQura.ToDateTime(nowUmAlQuraHijriYear, 10, 1, 0, 0, 0, 0).AddDays(-1);
            if (ramadanEve.Year == year)
                items.Add(new PublicHoliday(ramadanEve, "Ramazan Bayramı Arifesi", "Ramadan Feast Eve", halfDay: true));

            // Ramazan Bayramı 1. Gün (Şevval 1)
            var ramadan1 = umAlQura.ToDateTime(nowUmAlQuraHijriYear, 10, 1, 0, 0, 0, 0);
            if (ramadan1.Year == year)
                items.Add(new PublicHoliday(ramadan1, "Ramazan Bayramı 1. Günü", "Ramadan Feast First Day"));

            // Ramazan Bayramı 2. Gün (Şevval 2)
            var ramadan2 = umAlQura.ToDateTime(nowUmAlQuraHijriYear, 10, 2, 0, 0, 0, 0);
            if (ramadan2.Year == year)
                items.Add(new PublicHoliday(ramadan2, "Ramazan Bayramı 2. Günü", "Ramadan Feast Second Day"));

            // Ramazan Bayramı 3. Gün (Şevval 3)
            var ramadan3 = umAlQura.ToDateTime(nowUmAlQuraHijriYear, 10, 3, 0, 0, 0, 0);
            if (ramadan3.Year == year)
                items.Add(new PublicHoliday(ramadan3, "Ramazan Bayramı 3. Günü", "Ramadan Feast Third Day"));

            //Kurban Bayramı eklemesi
            // Kurban Bayramı Arifesi 
            var sacrificeEve = umAlQura.ToDateTime(nowUmAlQuraHijriYear, 12, 10, 0, 0, 0, 0).AddDays(-1);
            if (sacrificeEve.Year == year)
                items.Add(new PublicHoliday(sacrificeEve, "Kurban Bayramı Arifesi", "Sacrifice Feast Eve", halfDay: true));

            // Kurban Bayramı 1. Gün (Zilhicce 10)
            var sacrifice1 = umAlQura.ToDateTime(nowUmAlQuraHijriYear, 12, 10, 0, 0, 0, 0);
            if (sacrifice1.Year == year)
                items.Add(new PublicHoliday(sacrifice1, "Kurban Bayramı 1. Günü", "Sacrifice Feast First Day"));

            // Kurban Bayramı 2. Gün (Zilhicce 11)
            var sacrifice2 = umAlQura.ToDateTime(nowUmAlQuraHijriYear, 12, 11, 0, 0, 0, 0);
            if (sacrifice2.Year == year)
                items.Add(new PublicHoliday(sacrifice2, "Kurban Bayramı 2. Günü", "Sacrifice Feast Second Day"));

            // Kurban Bayramı 3. Gün (Zilhicce 12)
            var sacrifice3 = umAlQura.ToDateTime(nowUmAlQuraHijriYear, 12, 12, 0, 0, 0, 0);
            if (sacrifice3.Year == year)
                items.Add(new PublicHoliday(sacrifice3, "Kurban Bayramı 3. Günü", "Sacrifice Feast Third Day"));

            // Kurban Bayramı 4. Gün (Zilhicce 13)
            var sacrifice4 = umAlQura.ToDateTime(nowUmAlQuraHijriYear, 12, 13, 0, 0, 0, 0);
            if (sacrifice4.Year == year)
                items.Add(new PublicHoliday(sacrifice4, "Kurban Bayramı 4. Günü", "Sacrifice Feast Fourth Day"));

            return items.Where(op => op.LaunchYear == null || op.LaunchYear <= year).OrderBy(op => op.Date).ToList();
        }
    }

    /// <summary>
    /// Genel Tatil
    /// </summary>
    public class PublicHoliday
    {
        /// <summary>
        /// Tarih
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Yarım Gün
        /// </summary>
        public bool HalfDay { get; set; }

        /// <summary>
        /// TR İsim
        /// </summary>
        public string LocalName { get; set; }

        /// <summary>
        /// EN İsim
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Resmi tatilin lansman yılı
        /// </summary>
        public int? LaunchYear { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="localName"></param>
        /// <param name="englishName"></param>
        public PublicHoliday(int year, int month, int day, string localName, string englishName, int? launchYear = null, bool halfDay = false)
        {
            Date = new DateTime(year, month, day);
            LocalName = localName;
            Name = englishName;
            HalfDay = halfDay;
            LaunchYear = launchYear;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="localName"></param>
        /// <param name="englishName"></param>
        public PublicHoliday(DateTime date, string localName, string englishName, int? launchYear = null, bool halfDay = false)
        {
            Date = date;
            LocalName = localName;
            Name = englishName;
            HalfDay = halfDay;
            LaunchYear = launchYear;
        }
    }
}
