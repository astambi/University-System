namespace LearningSystem.Web.Infrastructure.Extensions
{
    public static class StringExtentions
    {
        private const string CurrencyFormat = "C2";
        private const string NumberFormat = "N0";
        private const string PercentageFormat = "P2";

        public static string ToCurrency(this decimal price)
            => price.ToString(CurrencyFormat);

        public static string ToNumber(this int number)
            => number.ToString(NumberFormat);

        public static string ToNumber(this long number)
            => number.ToString(NumberFormat);

        public static string ToPercentage(this double percentage)
            => percentage.ToString(PercentageFormat);
    }
}
