namespace LearningSystem.Web.Infrastructure.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        private const string CurrencyFormat = "C2";
        private const string NumberFormat = "N0";
        private const string PercentageFormat = "P2";

        private const string BgAlphabet = "а б в г д е ж  з и й к л м н о п р с т у ф х ц  ч  ш  щ   ъ ь ю  я";
        private const string EnAlphabet = "a b v g d e zh z i y k l m n o p r s t u f h ts ch sh sht a y yu ya";
        private static Dictionary<char, string> transliterationTable;

        static StringExtensions()
        {
            InititializeTransliterationTable();
        }

        public static string ToCurrency(this decimal price)
            => price.ToString(CurrencyFormat);

        public static string ToNumber(this int number)
            => number.ToString(NumberFormat);

        public static string ToNumber(this long number)
            => number.ToString(NumberFormat);

        public static string ToPercentage(this double percentage)
            => percentage.ToString(PercentageFormat);

        public static string ToFriendlyUrl(this string text)
            => Regex.Replace(TranslitConvert(text), @"[^A-Za-z0-9_\.~]+", "-");

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
