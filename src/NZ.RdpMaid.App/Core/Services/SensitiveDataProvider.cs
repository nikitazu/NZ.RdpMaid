using System;
using NZ.RdpMaid.App.SerializationModels.SensitiveData;
using NZ.RdpMaid.App.Settings;

namespace NZ.RdpMaid.App.Core.Services
{
    internal class SensitiveDataProvider
    {
        private readonly AppSettingsProvider _settingsProvider;
        private readonly SafeStorage _safeStorage;

        public SensitiveDataProvider(
            AppSettingsProvider settingsProvider,
            SafeStorage safeStorage
        )
        {
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
            _safeStorage = safeStorage ?? throw new ArgumentNullException(nameof(safeStorage));
        }

        public void TryMigrateToSafeStorageIfNeeded()
        {
            if (!_safeStorage.IsKeyFileCreated())
            {
                var sensitiveData = new SensitiveParameters()
                {
                    Password = null,
                    PinCodeSource = null,
                };

                _safeStorage.Save(sensitiveData);
            }
        }

        public string? GetPassword()
        {
            return _safeStorage.Load()?.Password;
        }

        public string? GetPinCodeSource()
        {
            if (!_settingsProvider.Settings.UsePinCodeGenerator)
            {
                return null;
            }

            return _safeStorage.Load()?.PinCodeSource;
        }

        public void SavePassword(string password)
        {
            var sensitiveData = _safeStorage.Load() ?? new SensitiveParameters();

            sensitiveData.Password = password;

            _safeStorage.Save(sensitiveData);
        }

        public void SavePinCodeSource(string source)
        {
            var sensitiveData = _safeStorage.Load() ?? new SensitiveParameters();

            sensitiveData.PinCodeSource = source;

            _safeStorage.Save(sensitiveData);
        }
    }
}