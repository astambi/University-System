namespace University.Web.Infrastructure.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        private const string CurrencyFormat = "C2";
        private const string CultureBg = "bg-BG";
        private const string NumberFormat = "N0";
        private const string NumberDecimal = "F2";
        private const string PercentageFormat = "P2";

        private const string BgAlphabet = "а б в г д е ж  з и й к л м н о п р с т у ф х ц  ч  ш  щ   ъ ь ю  я";
        private const string EnAlphabet = "a b v g d e zh z i y k l m n o p r s t u f h ts ch sh sht a y yu ya";
        private static Dictionary<char, string> transliterationTable;

        static StringExtensions()
        {
            InititializeTransliterationTable();
        }

        public static string ToCurrency(this decimal price)
        //=> price.ToString(CurrencyFormat, CultureInfo.CreateSpecificCulture(CultureBg));
            => price.ToString(CurrencyFormat);

        public static string ToFriendlyName(this Enum enumValue)
            => enumValue.ToString().ToFriendlyName();

        public static string ToFriendlyName(this string text)
        {
            var containsMidUpperLetter = text
                .Skip(1)
                .Any(ch => char.IsUpper(ch));

            if (!containsMidUpperLetter)
            {
                return text;
            }

            var builder = new StringBuilder()
                .Append(text[0]);

            for (var i = 1; i < text.Length; i++)
            {
                var symbol = text[i];
                if (char.IsUpper(symbol))
                {
                    builder.Append(" ");
                }

                builder.Append(symbol);
            }

            return builder.ToString().Trim();
        }

        public static string ToFriendlyUrl(this string text)
            => Regex.Replace(TranslitConvert(text), @"[^A-Za-z0-9_\.~]+", "-");

        public static string ToNumberDecimal(this decimal number)
            => number.ToString(NumberDecimal);

        public static string ToNumberDecimal(this int number)
            => number.ToString(NumberDecimal);

        public static string ToNumber(this int number)
            => number.ToString(NumberFormat);

        public static string ToNumber(this long number)
            => number.ToString(NumberFormat);

        public static string ToPercentage(this double percentage)
            => percentage.ToString(PercentageFormat);

        private static string TranslitConvert(string text)
        {
            text = text.ToLower();

            var builder = new StringBuilder();
            foreach (var ch in text)
            {
                if (transliterationTable.ContainsKey(ch))
                {
                    builder.Append(transliterationTable[ch]);
                }
                else
                {
                    builder.Append(ch);
                }
            }

            return builder.ToString();
        }

        private static void InititializeTransliterationTable()
        {
            var bg = BgAlphabet.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var en = EnAlphabet.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            transliterationTable = new Dictionary<char, string>();
            for (var i = 0; i < bg.Length; i++)
            {
                transliterationTable[bg[i][0]] = en[i];
            }
        }
    }
}
