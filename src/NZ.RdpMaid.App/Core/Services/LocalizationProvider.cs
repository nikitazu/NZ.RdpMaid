namespace NZ.RdpMaid.App.Core.Services
{
    internal class LocalizationProvider
    {
        public const string EnterPinCodeMessage = "Хозяин, вводи пин-код скорее!";
        public const string AutoPinCodeMessage = "Хозяин, не парься, пин-код введу сама!";

        public static string GetPinCodeLifetimeMessage(int secondsLeft) => $"До смены кода осталось {secondsLeft} сек.";
    }
}
