using OtpNet;

namespace NZ.RdpMaid.App.Core.Services
{
    internal class PinCodeProvider
    {
        private Totp? _otpProvider;

        public void SetSource(string? source)
        {
            _otpProvider = !string.IsNullOrEmpty(source)
                ? new Totp(Base32Encoding.ToBytes(source))
                : null;
        }

        public string? GetPinCode()
        {
            return _otpProvider?.ComputeTotp();
        }

        public int? GetLifetimeSeconds()
        {
            return _otpProvider?.RemainingSeconds();
        }
    }
}
