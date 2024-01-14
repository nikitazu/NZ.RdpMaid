using System;
using ProtoBuf;

namespace NZ.RdpMaid.App.SerializationModels.GoogleAuthenticator
{
    [ProtoContract]
    internal class Payload
    {
        [ProtoMember(1)]
        public OtpParameters[] Parameters { get; set; } = Array.Empty<OtpParameters>();

        [ProtoMember(2)]
        public int Version { get; set; }

        [ProtoMember(3)]
        public int BatchSize { get; set; }

        [ProtoMember(4)]
        public int BatchIndex { get; set; }

        [ProtoMember(5)]
        public int BatchId { get; set; }
    }
}
