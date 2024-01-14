namespace NZ.RdpMaid.App.Settings
{
    internal class AppSettings
    {
        public string GatewayHostname { get; init; } = default!;
        public string Domain { get; init; } = default!;
        public string Login { get; init; } = default!;
        public string Address { get; init; } = default!;

        /// <summary>
        /// Использовать пользовательский каталог `AppData` для хранения данных приложения.
        /// </summary>
        public bool UseAppDataDirectoryForStorage { get; init; } = false;

        /// <summary>
        /// Использовать автоматический генератор пин-кодов.
        /// </summary>
        public bool UsePinCodeGenerator { get; init; } = true;

        /// <summary>
        /// Тема оформления. Допустимые значения: PinkLoli | NightMistress
        /// </summary>
        public string Theme { get; set; } = "PinkLoli";
    }
}