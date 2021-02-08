using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Globalization.NumberToWords
{
    public class ArabicConverter : Converter
    {
        /// <summary>
        /// Is the currency name feminine ( Mua'anath مؤنث)
        /// ليرة سورية : مؤنث = true
        /// درهم : مذكر = false
        /// </summary>
        public Boolean IsCurrencyNameFeminine { get; set; }

        /// <summary>
        /// Arabic Currency Name for 1 unit only
        /// ليرة سورية
        /// درهم إماراتي
        /// </summary>
        public string Arabic1CurrencyName { get; set; }

        /// <summary>
        /// Arabic Currency Name for 2 units only
        /// ليرتان سوريتان
        /// درهمان إماراتيان
        /// </summary>
        public string Arabic2CurrencyName { get; set; }

        /// <summary>
        /// Arabic Currency Name for 3 to 10 units
        /// خمس ليرات سورية
        /// خمسة دراهم إماراتية
        /// </summary>
        public string Arabic310CurrencyName { get; set; }

        /// <summary>
        /// Arabic Currency Name for 11 to 99 units
        /// خمس و سبعون ليرةً سوريةً
        /// خمسة و سبعون درهماً إماراتياً
        /// </summary>
        public string Arabic1199CurrencyName { get; set; }

        /// <summary>
        /// Is the currency part name feminine ( Mua'anath مؤنث)
        /// هللة : مؤنث = true
        /// قرش : مذكر = false
        /// </summary>
        public Boolean IsCurrencyPartNameFeminine { get; set; }


        /// <summary>
        /// Arabic Currency Part Name for 1 unit only
        /// قرش
        /// هللة
        /// </summary>
        public string Arabic1CurrencyPartName { get; set; }

        /// <summary>
        /// Arabic Currency Part Name for 2 unit only
        /// قرشان
        /// هللتان
        /// </summary>
        public string Arabic2CurrencyPartName { get; set; }

        /// <summary>
        /// Arabic Currency Part Name for 3 to 10 units
        /// قروش
        /// هللات
        /// </summary>
        public string Arabic310CurrencyPartName { get; set; }

        /// <summary>
        /// Arabic Currency Part Name for 11 to 99 units
        /// قرشاً
        /// هللةً
        /// </summary>
        public string Arabic1199CurrencyPartName { get; set; }

        private static string[] arabicOnes =
           new string[] {
            String.Empty, "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة",
            "عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر", "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر"
        };

        private static string[] arabicFeminineOnes =
           new string[] {
            String.Empty, "إحدى", "اثنتان", "ثلاث", "أربع", "خمس", "ست", "سبع", "ثمان", "تسع",
            "عشر", "إحدى عشرة", "اثنتا عشرة", "ثلاث عشرة", "أربع عشرة", "خمس عشرة", "ست عشرة", "سبع عشرة", "ثماني عشرة", "تسع عشرة"
        };

        private static string[] arabicTens =
            new string[] {
            "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون"
        };

        private static string[] arabicHundreds =
            new string[] {
            "", "مائة", "مئتان", "ثلاثمائة", "أربعمائة", "خمسمائة", "ستمائة", "سبعمائة", "ثمانمائة","تسعمائة"
        };

        private static string[] arabicAppendedTwos =
            new string[] {
            "مئتا", "ألفا", "مليونا", "مليارا", "تريليونا", "كوادريليونا", "كوينتليونا", "سكستيليونا"
        };

        private static string[] arabicTwos =
            new string[] {
            "مئتان", "ألفان", "مليونان", "ملياران", "تريليونان", "كوادريليونان", "كوينتليونان", "سكستيليونان"
        };

        private static string[] arabicGroup =
            new string[] {
            "مائة", "ألف", "مليون", "مليار", "تريليون", "كوادريليون", "كوينتليون", "سكستيليون"
        };

        private static string[] arabicAppendedGroup =
            new string[] {
            "", "ألفاً", "مليوناً", "ملياراً", "تريليوناً", "كوادريليوناً", "كوينتليوناً", "سكستيليوناً"
        };

        private static string[] arabicPluralGroups =
            new string[] {
            "", "آلاف", "ملايين", "مليارات", "تريليونات", "كوادريليونات", "كوينتليونات", "سكستيليونات"
        };

        public ArabicConverter(string Culturecode, string CurrencyCode)
            : base(Culturecode, CurrencyCode)
        {
            this.Prefix = "فقط";
            this.Suffix = "لا غير.";
            this.PartPrecision = 2;
            this.IsCurrencyPartNameFeminine = false;
            this.IsCurrencyNameFeminine = false;
            
            switch (Culturecode)
            {
                case "ar-SA" /*Arabic - Saudi Arabia*/: 
                    this.Arabic1CurrencyName = "ريال سعودي";
                    this.Arabic2CurrencyName = "ريالان سعوديان";
                    this.Arabic310CurrencyName = "ريالات سعودية";
                    this.Arabic1199CurrencyName = "ريالاً سعودياً";
                    this.Arabic1CurrencyPartName = "هللة";
                    this.Arabic2CurrencyPartName = "هللتان";
                    this.Arabic310CurrencyPartName = "هللات";
                    this.Arabic1199CurrencyPartName = "هللة";
                    this.IsCurrencyPartNameFeminine = true;
                    break;
                case "ar-SY" /*Arabic - Syria*/:
                case "syr-SY" /*Syriac - Syria*/:
                    this.IsCurrencyNameFeminine = true;
                    this.Arabic1CurrencyName = "ليرة سورية";
                    this.Arabic2CurrencyName = "ليرتان سوريتان";
                    this.Arabic310CurrencyName = "ليرات سورية";
                    this.Arabic1199CurrencyName = "ليرة سورية";
                    this.Arabic1CurrencyPartName = "قرش";
                    this.Arabic2CurrencyPartName = "قرشان";
                    this.Arabic310CurrencyPartName = "قروش";
                    this.Arabic1199CurrencyPartName = "قرشاً";                    
                    break;
                case "ar-TN" /*Arabic - Tunisia*/:
                    this.Arabic1CurrencyName = "دينار تونسي";
                    this.Arabic2CurrencyName = "ديناران تونسيان";
                    this.Arabic310CurrencyName = "دنانير تونسية";
                    this.Arabic1199CurrencyName = "ديناراً تونسياً";
                    this.Arabic1CurrencyPartName = "مليم";
                    this.Arabic2CurrencyPartName = "مليمان";
                    this.Arabic310CurrencyPartName = "ملاليم";
                    this.Arabic1199CurrencyPartName = "مليماً";
                    this.PartPrecision = 3;
                    break;
                case "ar-AE" /*Arabic - United Arab Emirates*/:                    
                    this.Arabic1CurrencyName = "درهم إماراتي";
                    this.Arabic2CurrencyName = "درهمان إماراتيان";
                    this.Arabic310CurrencyName = "دراهم إماراتية";
                    this.Arabic1199CurrencyName = "درهماً إماراتياً";
                    this.Arabic1CurrencyPartName = "فلس";
                    this.Arabic2CurrencyPartName = "فلسان";
                    this.Arabic310CurrencyPartName = "فلوس";
                    this.Arabic1199CurrencyPartName = "فلساً";
                    break;
            }
        }

        public override string Get(decimal Amount)
        {
            Decimal tempNumber = Amount;

            if (tempNumber == 0)
                return "صفر";

            // Get Text for the decimal part
            string decimalString = ProcessArabicGroup(this.DecimalValue, -1, 0);

            string retVal = String.Empty;
            Byte group = 0;
            while (tempNumber >= 1)
            {
                // seperate number into groups
                int numberToProcess = (int)(tempNumber % 1000);

                tempNumber = tempNumber / 1000;

                // convert group into its text
                string groupDescription = ProcessArabicGroup(numberToProcess, group, Math.Floor(tempNumber));

                if (groupDescription != String.Empty)
                { // here we add the new converted group to the previous concatenated text
                    if (group > 0)
                    {
                        if (retVal != String.Empty)
                            retVal = String.Format("{0} {1}", "و", retVal);

                        if (numberToProcess != 2)
                        {
                            if (numberToProcess % 100 != 1)
                            {
                                if (numberToProcess >= 3 && numberToProcess <= 10) // for numbers between 3 and 9 we use plural name
                                    retVal = String.Format("{0} {1}", arabicPluralGroups[group], retVal);
                                else
                                {
                                    if (retVal != String.Empty) // use appending case
                                        retVal = String.Format("{0} {1}", arabicAppendedGroup[group], retVal);
                                    else
                                        retVal = String.Format("{0} {1}", arabicGroup[group], retVal); // use normal case
                                }
                            }
                            else
                            {
                                retVal = String.Format("{0} {1}", arabicGroup[group], retVal); // use normal case
                            }
                        }
                    }

                    retVal = String.Format("{0} {1}", groupDescription, retVal);
                }

                group++;
            }

            String formattedNumber = String.Empty;
            formattedNumber += (this.Prefix != String.Empty) ? String.Format("{0} ", this.Prefix) : String.Empty;
            formattedNumber += (retVal != String.Empty) ? retVal : String.Empty;
            if (this.IntegerValue != 0)
            { // here we add currency name depending on this.IntegerValue : 1 ,2 , 3--->10 , 11--->99
                int remaining100 = (int)(this.IntegerValue % 100);

                if (remaining100 == 0)
                    formattedNumber += this.Arabic1CurrencyName;
                else
                    if (remaining100 == 1)
                        formattedNumber += this.Arabic1CurrencyName;
                    else
                        if (remaining100 == 2)
                        {
                            if (this.IntegerValue == 2)
                                formattedNumber += this.Arabic2CurrencyName;
                            else
                                formattedNumber += this.Arabic1CurrencyName;
                        }
                        else
                            if (remaining100 >= 3 && remaining100 <= 10)
                                formattedNumber += this.Arabic310CurrencyName;
                            else
                                if (remaining100 >= 11 && remaining100 <= 99)
                                    formattedNumber += this.Arabic1199CurrencyName;
            }
            formattedNumber += (this.DecimalValue != 0) ? " و " : String.Empty;
            formattedNumber += (this.DecimalValue != 0) ? decimalString : String.Empty;
            if (this.DecimalValue != 0)
            { // here we add currency part name depending on this.IntegerValue : 1 ,2 , 3--->10 , 11--->99
                formattedNumber += " ";

                int remaining100 = (int)(this.DecimalValue % 100);

                if (remaining100 == 0)
                    formattedNumber += this.Arabic1CurrencyPartName;
                else
                    if (remaining100 == 1)
                        formattedNumber += this.Arabic1CurrencyPartName;
                    else
                        if (remaining100 == 2)
                            formattedNumber += this.Arabic2CurrencyPartName;
                        else
                            if (remaining100 >= 3 && remaining100 <= 10)
                                formattedNumber += this.Arabic310CurrencyPartName;
                            else
                                if (remaining100 >= 11 && remaining100 <= 99)
                                    formattedNumber += this.Arabic1199CurrencyPartName;
            }
            formattedNumber += (this.Suffix != String.Empty) ? String.Format(" {0}", this.Suffix) : String.Empty;

            return formattedNumber;
        }
        private string ProcessArabicGroup(int groupNumber, int groupLevel, Decimal remainingNumber)
        {
            int tens = groupNumber % 100;

            int hundreds = groupNumber / 100;

            string retVal = String.Empty;

            if (hundreds > 0)
            {
                if (tens == 0 && hundreds == 2) // حالة المضاف
                    retVal = String.Format("{0}", arabicAppendedTwos[0]);
                else //  الحالة العادية
                    retVal = String.Format("{0}", arabicHundreds[hundreds]);
            }

            if (tens > 0)
            {
                if (tens < 20)
                { // if we are processing under 20 numbers
                    if (tens == 2 && hundreds == 0 && groupLevel > 0)
                    { // This is special case for number 2 when it comes alone in the group
                        if (this.IntegerValue == 2000 || this.IntegerValue == 2000000 || this.IntegerValue == 2000000000 || this.IntegerValue == 2000000000000 || this.IntegerValue == 2000000000000000 || this.IntegerValue == 2000000000000000000)
                            retVal = String.Format("{0}", arabicAppendedTwos[groupLevel]); // في حالة الاضافة
                        else
                            retVal = String.Format("{0}", arabicTwos[groupLevel]);//  في حالة الافراد
                    }
                    else
                    { // General case
                        if (retVal != String.Empty)
                            retVal += " و ";

                        if (tens == 1 && groupLevel > 0 && hundreds == 0)
                            retVal += " ";
                        else
                            if ((tens == 1 || tens == 2) && (groupLevel == 0 || groupLevel == -1) && hundreds == 0 && remainingNumber == 0)
                                retVal += String.Empty; // Special case for 1 and 2 numbers like: ليرة سورية و ليرتان سوريتان
                            else
                                retVal += GetDigitFeminineStatus(tens, groupLevel);// Get Feminine status for this digit
                    }
                }
                else
                {
                    int ones = tens % 10;
                    tens = (tens / 10) - 2; // 20's offset

                    if (ones > 0)
                    {
                        if (retVal != String.Empty)
                            retVal += " و ";

                        // Get Feminine status for this digit
                        retVal += GetDigitFeminineStatus(ones, groupLevel);
                    }

                    if (retVal != String.Empty)
                        retVal += " و ";

                    // Get Tens text
                    retVal += arabicTens[tens];
                }
            }

            return retVal;
        }

        private string GetDigitFeminineStatus(int digit, int groupLevel)
        {
            if (groupLevel == -1)
            { // if it is in the decimal part
                if (this.IsCurrencyPartNameFeminine)
                    return arabicFeminineOnes[digit]; // use feminine field
                else
                    return arabicOnes[digit];
            }
            else
                if (groupLevel == 0)
                {
                    if (this.IsCurrencyNameFeminine)
                        return arabicFeminineOnes[digit];// use feminine field
                    else
                        return arabicOnes[digit];
                }
                else
                    return arabicOnes[digit];
        }
    }
}
