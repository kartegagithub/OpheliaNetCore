using System;

/* http://www.codeproject.com/Articles/112949/Number-To-Word-Arabic-Version */

namespace Ophelia.Globalization.NumberToWords
{
    public abstract class Converter
    {
        /// <summary>Decimal Part</summary>
        public int DecimalValue { get; set; }

        /// <summary> integer part </summary>
        public long IntegerValue { get; set; }

        public string CultureCode { get; set; }
        public string CurrencyCode { get; set; }
        public string AndOperatorString { get; set; }

        public byte PartPrecision { get; set; }

        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public string CurrencyName { get; set; }
        public string PluralCurrencyName { get; set; }
        public string CurrencyPartName { get; set; }
        public string PluralCurrencyPartName { get; set; }

        public string[] Ones { get; set; }
        public string[] Tens { get; set; }
        public string[] Groups { get; set; }


        public Converter()
        {
            this.Ones = new string[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            this.Tens = new string[] { "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
            this.Groups = new string[] { "Hundred", "Thousand", "Million", "Billion", "Trillion", "Quadrillion", "Quintillion" };
            this.CurrencyName = "Dollar";
            this.PluralCurrencyName = "Dollars";
            this.Prefix = "Only";
            this.AndOperatorString = " and ";
            this.CurrencyPartName = "Penny";
            this.PluralCurrencyPartName = "Pennies";
        }

        public Converter(string CultureCode, string CurrencyCode)
            : this()
        {
            this.CultureCode = CultureCode;
            this.CurrencyCode = CurrencyCode;
        }


        public static string Get(string amount, string cultureCode, string currencyCode)
        {
            return Get(Convert.ToDecimal(amount.ToString().Replace(".", ",")), cultureCode, currencyCode);
        }

        public static string Get(decimal amount, string cultureCode, string CurrencyCode)
        {
            Converter Converter = null;
            switch (cultureCode)
            {
                case "tr-TR" /*Turkish - Turkey*/:
                    Converter = new TurkishConvertor(cultureCode, CurrencyCode);
                    break;
                case "Cy-uz-UZ" /*Uzbek (Cyrillic) - Uzbekistan*/:
                    Converter = new UzbekCyrlConverter();
                    break;
                case "Lt-uz-UZ" /*Uzbek (Latin) - Uzbekistan*/:
                    Converter = new UzbekConverter();
                    break;
                case "uk-UA" /*Ukrainian - Ukraine*/:
                    Converter = new UkrainianConverter();
                    break;

                case "ar-DZ" /*Arabic - Algeria*/:
                case "ar-BH" /*Arabic - Bahrain*/:
                case "ar-EG" /*Arabic - Egypt*/:
                case "ar-IQ" /*Arabic - Iraq*/:
                case "ar-JO" /*Arabic - Jordan*/:
                case "ar-KW" /*Arabic - Kuwait*/:
                case "ar-LB" /*Arabic - Lebanon*/:
                case "ar-LY" /*Arabic - Libya*/:
                case "ar-MA" /*Arabic - Morocco*/:
                case "ar-OM" /*Arabic - Oman*/:
                case "ar-QA" /*Arabic - Qatar*/:
                case "ar-SA" /*Arabic - Saudi Arabia*/:
                case "ar-SY" /*Arabic - Syria*/:
                case "syr-SY" /*Syriac - Syria*/:
                case "ar-TN" /*Arabic - Tunisia*/:
                case "ar-AE" /*Arabic - United Arab Emirates*/:
                case "ar-YE" /*Arabic - Yemen*/:
                    Converter = new ArabicConverter(cultureCode, CurrencyCode);
                    break;

                case "nl-BE" /*Dutch - Belgium*/:
                case "nl-NL" /*Dutch - The Netherlands*/:
                    Converter = new BritishConverter(cultureCode, CurrencyCode);
                    break;

                case "en-ZA" /*English - South Africa*/:
                case "en-AU" /*English - Australia*/:
                case "en-GB" /*English - United Kingdom*/:
                    Converter = new BritishConverter(cultureCode, CurrencyCode);
                    break;

                case "fr-BE" /*French - Belgium*/:
                case "fr-CA" /*French - Canada*/:
                case "fr-FR" /*French - France*/:
                case "fr-LU" /*French - Luxembourg*/:
                case "fr-MC" /*French - Monaco*/:
                case "fr-CH" /*French - Switzerland*/:
                    Converter = new FrenchConverter(cultureCode, CurrencyCode);
                    break;

                case "de-AT" /*German - Austria*/:
                case "de-DE" /*German - Germany*/:
                case "de-LI" /*German - Liechtenstein*/:
                case "de-LU" /*German - Luxembourg*/:
                case "de-CH" /*German - Switzerland*/:
                    Converter = new GermanConverter(cultureCode, CurrencyCode);
                    break;

                case "he-IL" /*Hebrew - Israel*/:
                    Converter = new HebrewConverter();
                    break;

                case "pl-PL" /*Polish - Poland*/:
                    Converter = new PolishConverter();
                    break;

                case "ru-RU" /*Russian - Russia*/:
                    Converter = new RussianConverter();
                    break;

                case "Cy-sr-SP" /*Serbian (Cyrillic) - Serbia*/:
                    Converter = new SerbianCyrlConverter();
                    break;

                case "Lt-sr-SP" /*Serbian (Latin) - Serbia*/:
                    Converter = new SerbianConverter();
                    break;

                case "sk-SK" /*Slovak - Slovakia*/:
                case "sl-SI" /*Slovenian - Slovenia*/:
                    Converter = new SlovenianConverter();
                    break;

                case "es-AR" /*Spanish - Argentina*/:
                case "es-BO" /*Spanish - Bolivia*/:
                case "es-CL" /*Spanish - Chile*/:
                case "es-CO" /*Spanish - Colombia*/:
                case "es-CR" /*Spanish - Costa Rica*/:
                case "es-DO" /*Spanish - Dominican Republic*/:
                case "es-EC" /*Spanish - Ecuador*/:
                case "es-SV" /*Spanish - El Salvador*/:
                case "es-GT" /*Spanish - Guatemala*/:
                case "es-HN" /*Spanish - Honduras*/:
                case "es-MX" /*Spanish - Mexico*/:
                case "es-NI" /*Spanish - Nicaragua*/:
                case "es-PA" /*Spanish - Panama*/:
                case "es-PY" /*Spanish - Paraguay*/:
                case "es-PE" /*Spanish - Peru*/:
                case "es-PR" /*Spanish - Puerto Rico*/:
                case "es-ES" /*Spanish - Spain*/:
                case "es-UY" /*Spanish - Uruguay*/:
                case "es-VE" /*Spanish - Venezuela*/:
                    Converter = new SpanishConverter();
                    break;

                //Default: American English
                case "hy-AM" /*Armenian - Armenia*/: break;
                case "Cy-az-AZ" /*Azeri (Cyrillic) - Azerbaijan*/: break;
                case "Lt-az-AZ" /*Azeri (Latin) - Azerbaijan*/: break;
                case "eu-ES" /*Basque - Basque*/: break;
                case "be-BY" /*Belarusian - Belarus*/: break;
                case "bg-BG" /*Bulgarian - Bulgaria*/: break;
                case "ca-ES" /*Catalan - Catalan*/: break;
                case "zh-CN" /*Chinese - China*/: break;
                case "zh-HK" /*Chinese - Hong Kong SAR*/: break;
                case "zh-MO" /*Chinese - Macau SAR*/: break;
                case "zh-SG" /*Chinese - Singapore*/: break;
                case "zh-TW" /*Chinese - Taiwan*/: break;
                case "zh-CHS" /*Chinese (Simplified)*/: break;
                case "zh-CHT" /*Chinese (Traditional)*/: break;
                case "hr-HR" /*Croatian - Croatia*/: break;
                case "cs-CZ" /*Czech - Czech Republic*/: break;
                case "da-DK" /*Danish - Denmark*/: break;
                case "div-MV" /*Dhivehi - Maldives*/: break;
                case "en-BZ" /*English - Belize*/: break;
                case "en-CA" /*English - Canada*/: break;
                case "en-CB" /*English - Caribbean*/: break;
                case "en-IE" /*English - Ireland*/: break;
                case "en-JM" /*English - Jamaica*/: break;
                case "en-NZ" /*English - New Zealand*/: break;
                case "en-PH" /*English - Philippines*/: break;
                case "en-TT" /*English - Trinidad and Tobago*/: break;
                case "en-ZW" /*English - Zimbabwe*/: break;
                case "et-EE" /*Estonian - Estonia*/: break;
                case "fo-FO" /*Faroese - Faroe Islands*/: break;
                case "fa-IR" /*Farsi - Iran*/: break;
                case "fi-FI" /*Finnish - Finland*/: break;
                case "gl-ES" /*Galician - Galician*/: break;
                case "ka-GE" /*Georgian - Georgia*/: break;
                case "el-GR" /*Greek - Greece*/: break;
                case "gu-IN" /*Gujarati - India*/: break;
                case "hi-IN" /*Hindi - India*/: break;
                case "hu-HU" /*Hungarian - Hungary*/: break;
                case "is-IS" /*Icelandic - Iceland*/: break;
                case "id-ID" /*Indonesian - Indonesia*/: break;
                case "it-IT" /*Italian - Italy*/: break;
                case "it-CH" /*Italian - Switzerland*/: break;
                case "ja-JP" /*Japanese - Japan*/: break;
                case "kn-IN" /*Kannada - India*/: break;
                case "kk-KZ" /*Kazakh - Kazakhstan*/: break;
                case "kok-IN" /*Konkani - India*/: break;
                case "ko-KR" /*Korean - Korea*/: break;
                case "ky-KZ" /*Kyrgyz - Kazakhstan*/: break;
                case "lv-LV" /*Latvian - Latvia*/: break;
                case "lt-LT" /*Lithuanian - Lithuania*/: break;
                case "mk-MK" /*Macedonian (FYROM)*/: break;
                case "ms-BN" /*Malay - Brunei*/: break;
                case "ms-MY" /*Malay - Malaysia*/: break;
                case "mr-IN" /*Marathi - India*/: break;
                case "mn-MN" /*Mongolian - Mongolia*/: break;
                case "nb-NO" /*Norwegian (Bokmål) - Norway*/: break;
                case "nn-NO" /*Norwegian (Nynorsk) - Norway*/: break;
                case "pt-BR" /*Portuguese - Brazil*/: break;
                case "pt-PT" /*Portuguese - Portugal*/: break;
                case "pa-IN" /*Punjabi - India*/: break;
                case "ro-RO" /*Romanian - Romania*/: break;
                case "sa-IN" /*Sanskrit - India*/: break;
                case "sw-KE" /*Swahili - Kenya*/: break;
                case "sv-FI" /*Swedish - Finland*/: break;
                case "sv-SE" /*Swedish - Sweden*/: break;
                case "ta-IN" /*Tamil - India*/: break;
                case "tt-RU" /*Tatar - Russia*/: break;
                case "te-IN" /*Telugu - India*/: break;
                case "th-TH" /*Thai - Thailand*/: break;
                case "ur-PK" /*Urdu - Pakistan*/: break;
                case "vi-VN" /*Vietnamese - Vietnam*/: break;
                case "af-ZA" /*Afrikaans - South Africa*/: break;
                case "sq-AL" /*Albanian - Albania*/: break;
            }
            if (Converter == null)
            {
                Converter = new AmericanConverter(cultureCode, CurrencyCode);
                Converter.PartPrecision = 2;
            }

            Converter.ExtractIntegerAndDecimalParts(amount);
            return Converter.Get(amount);
        }

        public virtual string Get(decimal Amount)
        {
            Decimal tempNumber = Amount;

            if (tempNumber == 0)
                return "Zero";

            string decimalString = ProcessGroup(this.DecimalValue);

            string retVal = String.Empty;

            int group = 0;

            if (tempNumber < 1)
            {
                retVal = this.Ones[0];
            }
            else
            {
                while (tempNumber >= 1)
                {
                    int numberToProcess = (int)(tempNumber % 1000);

                    tempNumber = tempNumber / 1000;

                    string groupDescription = ProcessGroup(numberToProcess);

                    if (groupDescription != String.Empty)
                    {
                        if (group > 0)
                        {
                            retVal = String.Format("{0} {1}", this.Groups[group], retVal);
                        }

                        retVal = this.ValidateGroup(String.Format("{0} {1}", groupDescription, retVal), tempNumber);
                    }
                    group++;
                }
            }

            String formattedNumber = String.Empty;
            formattedNumber += (this.Prefix != String.Empty) ? String.Format("{0} ", this.Prefix) : String.Empty;
            formattedNumber += (retVal != String.Empty) ? retVal : String.Empty;
            formattedNumber += (retVal != String.Empty) ? (this.IntegerValue == 1 ? this.CurrencyName : this.PluralCurrencyName) : String.Empty;
            formattedNumber += (decimalString != String.Empty) ? " " + this.AndOperatorString + " " : String.Empty;
            formattedNumber += (decimalString != String.Empty) ? decimalString : String.Empty;
            formattedNumber += (decimalString != String.Empty) ? " " + (this.DecimalValue == 1 ? this.CurrencyPartName : this.PluralCurrencyPartName) : String.Empty;
            formattedNumber += (this.Suffix != String.Empty) ? String.Format(" {0}", this.Suffix) : String.Empty;

            return formattedNumber;
        }

        protected virtual string ValidatePart(string value, int Hundreds, int Tens, int Ones)
        {
            return value;
        }

        protected virtual string ValidateGroup(string value, decimal decValue)
        {
            return value;
        }

        internal void ExtractIntegerAndDecimalParts(decimal amount)
        {
            String[] splits = amount.ToString().Replace(",", ".").Split('.');

            this.IntegerValue = Convert.ToInt32(splits[0]);

            if (splits.Length > 1)
                this.DecimalValue = Convert.ToInt32(this.GetDecimalValue(splits[1]));
        }

        internal string GetDecimalValue(string decimalPart)
        {
            string result = String.Empty;

            if (this.PartPrecision != decimalPart.Length)
            {
                int decimalPartLength = decimalPart.Length;

                for (int i = 0; i < this.PartPrecision - decimalPartLength; i++)
                {
                    decimalPart += "0"; //Fix for 1 number after decimal ( 10.5 , 1442.2 , 375.4 ) 
                }

                result = String.Format("{0}.{1}", decimalPart.Substring(0, this.PartPrecision), decimalPart.Substring(this.PartPrecision, decimalPart.Length - this.PartPrecision));

                result = (Math.Round(Convert.ToDecimal(result))).ToString();
            }
            else
                result = decimalPart;

            for (int i = 0; i < this.PartPrecision - result.Length; i++)
            {
                result += "0";
            }

            return result;
        }

        internal string ProcessGroup(int groupNumber)
        {
            int tens = groupNumber % 100;

            int hundreds = groupNumber / 100;

            int ones = tens % 10;

            string retVal = String.Empty;

            if (hundreds > 0)
            {
                retVal = this.ValidatePart(String.Format("{0} {1}", this.Ones[hundreds], this.Groups[0]), hundreds, tens, ones);
            }
            if (tens > 0)
            {
                if (tens < 20)
                {
                    retVal += this.ValidatePart(((retVal != String.Empty) ? " " : String.Empty) + this.Ones[tens], hundreds, tens, ones);
                }
                else
                {
                    tens = (tens / 10) - 2; // 20's offset

                    retVal += this.ValidatePart(((retVal != String.Empty) ? " " : String.Empty) + this.Tens[tens], hundreds, tens, ones);

                    if (ones > 0)
                    {
                        retVal += this.ValidatePart(((retVal != String.Empty) ? " " : String.Empty) + this.Ones[ones], hundreds, tens, ones);
                    }
                }
            }

            return retVal;
        }
    }
}
