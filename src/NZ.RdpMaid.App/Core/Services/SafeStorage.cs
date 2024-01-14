using System;
using System.IO;
using System.Security.Cryptography;
using NZ.RdpMaid.App.Extensions.Encryption;
using NZ.RdpMaid.App.SerializationModels.SensitiveData;
using ProtoBuf;

namespace NZ.RdpMaid.App.Core.Services
{
    internal class SafeStorage
    {
        private const string _keyFileName = "key.b64";
        private const string _dataFileName = "safe.bin";

        private readonly FileStorage _fileStorage;

        public SafeStorage(FileStorage fileStorage)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        }

        public bool IsKeyFileCreated() =>
            _fileStorage.FileExists(_keyFileName);

        public void Save(SensitiveParameters input)
        {
            using var aes = CreateEncryptionAlgorythm();

            InitKey(aes);

            aes.GenerateIV();

            var inputData = Serialize(input);
            var encryptedData = aes.EncryptByteArray(inputData);
            var safeContainer = SafeContainer.Create(aes.IV, encryptedData);

            _fileStorage.CreateBinaryFile(_dataFileName, safeContainer);
        }

        public SensitiveParameters? Load()
        {
            if (!_fileStorage.FileExists(_keyFileName))
            {
                return null;
            }

            if (!_fileStorage.FileExists(_dataFileName))
            {
                return null;
            }

            var containerBytes = _fileStorage.ReadBinaryFile(_dataFileName);
            var safeContainer = SafeContainer.Unbox(containerBytes);

            using var aes = CreateEncryptionAlgorythm();

            InitKey(aes);

            aes.IV = safeContainer.IV;

            var decryptedData = aes.DecryptByteArray(safeContainer.EncryptedData);
            var result = Deserialize(decryptedData);

            return result;
        }

        private static Aes CreateEncryptionAlgorythm()
        {
            var aes = Aes.Create();

            aes.Padding = SafeContainer.Format.EncryptionPadding;

            return aes;
        }

        private void InitKey(Aes aes)
        {
            if (!_fileStorage.FileExists(_keyFileName))
            {
                aes.GenerateKey();
                var keyBase64 = Convert.ToBase64String(aes.Key);
                _fileStorage.CreateTextFile(_keyFileName, keyBase64);
            }
            else
            {
                var keyBase64 = _fileStorage.ReadTextFile(_keyFileName);
                aes.Key = Convert.FromBase64String(keyBase64);
            }
        }

        private static byte[] Serialize(SensitiveParameters input)
        {
            using var buffer = new MemoryStream();

            Serializer.Serialize(buffer, input);

            buffer.Position = 0;

            return buffer.ToArray();
        }

        private static SensitiveParameters Deserialize(byte[] input)
        {
            using var buffer = new MemoryStream(input);

            return Serializer.Deserialize<SensitiveParameters>(buffer);
        }
    }
}