using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Ophelia
{
    public static class HolidayExtensions
    {
        public static List<PublicHoliday> Holidays(this int year)
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

            HijriCalendar hijri = new HijriCalendar();
            CultureInfo arSA = new CultureInfo("ar-SA");
            arSA.DateTimeFormat.Calendar = new HijriCalendar();

            var nowHijriYear = hijri.GetYear(new DateTime(year, 1, 1));

            //Günü denklemek için bir gün ekleniyor.

            //Ramazan Bayramı eklemesi
            var gregorianCalendarRamadanEve = DateTime.ParseExact($"30/09/{nowHijriYear}", "dd/MM/yyyy", arSA).AddDays(1);
            items.Add(new PublicHoliday(gregorianCalendarRamadanEve, "Ramazan Bayramı Arifesi", "Ramadan Feast Eve", halfDay: true));

            var gregorianCalendarRamadan1 = DateTime.ParseExact($"01/10/{nowHijriYear}", "dd/MM/yyyy", arSA).AddDays(1);
            items.Add(new PublicHoliday(gregorianCalendarRamadan1, "Ramazan Bayramı 1. Günü", "Ramadan Feast First Day"));

            var gregorianCalendarRamadan2 = DateTime.ParseExact($"02/10/{nowHijriYear}", "dd/MM/yyyy", arSA).AddDays(1);
            items.Add(new PublicHoliday(gregorianCalendarRamadan2, "Ramazan Bayramı 2. Günü", "Ramadan Feast Second Day"));

            var gregorianCalendarRamadan3 = DateTime.ParseExact($"03/10/{nowHijriYear}", "dd/MM/yyyy", arSA).AddDays(1);
            items.Add(new PublicHoliday(gregorianCalendarRamadan3, "Ramazan Bayramı 3. Günü", "Ramadan Feast Third Day"));

            //Kurban Bayramı eklemesi
            var gregorianCalendarSacrificeArife = DateTime.ParseExact($"09/12/{nowHijriYear}", "dd/MM/yyyy", arSA).AddDays(1);
            items.Add(new PublicHoliday(gregorianCalendarSacrificeArife, "Kurban Bayramı Arifesi", "Sacrifice Feast Eve", halfDay: true));

            var gregorianCalendarSacrifice1 = DateTime.ParseExact($"10/12/{nowHijriYear}", "dd/MM/yyyy", arSA).AddDays(1);
            items.Add(new PublicHoliday(gregorianCalendarSacrifice1, "Kurban Bayramı 1. Günü", "Sacrifice Feast First Day"));

            var gregorianCalendarSacrifice2 = DateTime.ParseExact($"11/12/{nowHijriYear}", "dd/MM/yyyy", arSA).AddDays(1);
            items.Add(new PublicHoliday(gregorianCalendarSacrifice2, "Kurban Bayramı 2. Günü", "Sacrifice Feast Second Day"));

            var gregorianCalendarSacrifice3 = DateTime.ParseExact($"12/12/{nowHijriYear}", "dd/MM/yyyy", arSA).AddDays(1);
            items.Add(new PublicHoliday(gregorianCalendarSacrifice3, "Kurban Bayramı 3. Günü", "Sacrifice Feast Third Day"));

            var gregorianCalendarSacrifice4 = DateTime.ParseExact($"13/12/{nowHijriYear}", "dd/MM/yyyy", arSA).AddDays(1);
            items.Add(new PublicHoliday(gregorianCalendarSacrifice4, "Kurban Bayramı 4. Günü", "Sacrifice Feast Fourth Day"));

            return items.OrderBy(op => op.Date).ToList();
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
            this.Date = new DateTime(year, month, day);
            this.LocalName = localName;
            this.Name = englishName;
            this.HalfDay = halfDay;
            this.LaunchYear = launchYear;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="localName"></param>
        /// <param name="englishName"></param>
        public PublicHoliday(DateTime date, string localName, string englishName, int? launchYear = null, bool halfDay = false)
        {
            this.Date = date;
            this.LocalName = localName;
            this.Name = englishName;
            this.HalfDay = halfDay;
            this.LaunchYear = launchYear;
        }
    }
}
