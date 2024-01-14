using ProtoBuf;

namespace NZ.RdpMaid.App.SerializationModels.SensitiveData
{
    /// <summary>
    /// Структура безопасного хранилища.
    /// </summary>
    [ProtoContract]
    internal class SensitiveParameters
    {
        /// <summary>
        /// Секретный ключ для генерации одноразовых пин-кодов (TOTP).
        /// </summary>
        [ProtoMember(1)]
        public string? PinCodeSource { get; set; }

        /// <summary>
        /// Пароль для входа в удалённую систему.
        /// </summary>
        [ProtoMember(2)]
        public string? Password { get; set; }
    }
}