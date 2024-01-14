using System;
using System.IO;
using System.Linq;
using NZ.RdpMaid.App.SerializationModels.GoogleAuthenticator;
using OtpNet;
using ProtoBuf;

namespace NZ.RdpMaid.App.Core.Services
{
    internal class PinCodeSourceImporter
    {
        public record ImportResult(
            string? SecretCode,
            string Feedback);

        public ImportResult Import(string importUri)
        {
            if (string.IsNullOrWhiteSpace(importUri))
            {
                return new(SecretCode: null, Feedback: "Ошибка: входная строка пуста");
            }

            if (!Uri.TryCreate(importUri, UriKind.Absolute, out var parsedUri))
            {
                return new(SecretCode: null, Feedback: "Ошибка: входная строка не является валидной ссылкой");
            }

            if (parsedUri.Scheme != "otpauth-migration")
            {
                return new(SecretCode: null, Feedback: "Ошибка: ссылка должна быть в формате [otpauth-migration]");
            }

            if (string.IsNullOrWhiteSpace(parsedUri.Query))
            {
                return new(SecretCode: null, Feedback: "Ошибка: ссылка должна содержать строку запроса");
            }

            var dataIndex = parsedUri.Query.IndexOf("?data=");

            if (dataIndex == -1)
            {
                return new(SecretCode: null, Feedback: "Ошибка: ссылка должна содержать параметр [data]");
            }

            try
            {
                var escapedData = parsedUri.Query[(dataIndex + "?data=".Length)..];

                var nextParameterIndex = escapedData.IndexOf("&");

                if (nextParameterIndex != -1)
                {
                    escapedData = escapedData.Substring(0, nextParameterIndex);
                }

                var base64data = Uri.UnescapeDataString(escapedData);
                var rawBytes = Convert.FromBase64String(base64data);

                using var protobufStream = new MemoryStream(rawBytes);

                var payload = Serializer.Deserialize<Payload>(protobufStream);

                var otpParameter = payload.Parameters.FirstOrDefault();

                if (otpParameter is null)
                {
                    return new(SecretCode: null, Feedback: "Ошибка: ссылка не содержит данных");
                }

                if (otpParameter.Secret is null || otpParameter.Secret.Length == 0)
                {
                    return new(SecretCode: null, Feedback: "Ошибка: обнаруженый секрет пуст");
                }

                var secretBase32 = Base32Encoding.ToString(otpParameter.Secret);

                return new(SecretCode: secretBase32, Feedback: "Успех");
            }
            catch (Exception e)
            {
                return new(SecretCode: null, Feedback: $"Ошибка: не удалось десериализовать данные. Причина: {e.Message}");
            }
        }
    }
}