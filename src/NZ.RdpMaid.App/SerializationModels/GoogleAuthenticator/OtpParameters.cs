using System;
using ProtoBuf;

namespace NZ.RdpMaid.App.SerializationModels.GoogleAuthenticator
{
    [ProtoContract]
    internal class OtpParameters
    {
        [ProtoMember(1)]
        public byte[] Secret { get; set; } = Array.Empty<byte>();

        [ProtoMember(2)]
        public string Name { get; set; } = string.Empty;

        [ProtoMember(3)]
        public string Issuer { get; set; } = string.Empty;

        [ProtoMember(4)]
        public Algorithm Algorithm { get; set; }

        [ProtoMember(5)]
        public DigitCount Digits { get; set; }

        [ProtoMember(6)]
        public OtpType Type { get; set; }

        [ProtoMember(7)]
        public UInt64 Counter { get; set; }
    }

    [ProtoContract]
    internal enum Algorithm
    {
        [ProtoEnum]
        ALGORITHM_UNSPECIFIED = 0,

        [ProtoEnum]
        ALGORITHM_SHA1 = 1,

        [ProtoEnum]
        ALGORITHM_SHA256 = 2,

        [ProtoEnum]
        ALGORITHM_SHA512 = 3,

        [ProtoEnum]
        ALGORITHM_MD5 = 4,
    }

    [ProtoContract]
    internal enum DigitCount
    {
        [ProtoEnum]
        DIGIT_COUNT_UNSPECIFIED = 0,

        [ProtoEnum]
        DIGIT_COUNT_SIX = 1,

        [ProtoEnum]
        DIGIT_COUNT_EIGHT = 2,
    }

    [ProtoContract]
    internal enum OtpType
    {
        [ProtoEnum]
        OTP_TYPE_UNSPECIFIED = 0,

        [ProtoEnum]
        OTP_TYPE_HOTP = 1,

        [ProtoEnum]
        OTP_TYPE_TOTP = 2,
    }
}
