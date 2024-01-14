using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NZ.RdpMaid.App.Core.Services
{
    internal class SafeContainer
    {
        public static class Format
        {
            public static readonly byte[] FormatMark = Encoding.UTF8.GetBytes("NZCS");

            public const byte IVMark = 10;
            public const byte EncryptedDataMark = 20;

            public static readonly PaddingMode EncryptionPadding = PaddingMode.PKCS7;
        }

        public static byte[] Create(byte[] iv, byte[] encryptedData)
        {
            using var container = new MemoryStream();
            using var writer = new BinaryWriter(container);

            writer.Write(Format.FormatMark);
            writer.Write(Format.IVMark);
            writer.Write(iv.Length);
            writer.Write(iv);
            writer.Write(Format.EncryptedDataMark);
            writer.Write(encryptedData.Length);
            writer.Write(encryptedData);

            writer.Flush();

            container.Position = 0;

            return container.ToArray();
        }

        public static (byte[] IV, byte[] EncryptedData) Unbox(byte[] containerBytes)
        {
            byte[] iv = [];
            byte[] encryptedData = [];

            using (var container = new MemoryStream(containerBytes))
            using (var reader = new BinaryReader(container))
            {
                var actualFormatMark = reader.ReadBytes(Format.FormatMark.Length);

                if (actualFormatMark.Length != Format.FormatMark.Length)
                {
                    throw new InvalidOperationException("Format mark wrong length");
                }

                for (int i = 0; i < Format.FormatMark.Length; i++)
                {
                    if (actualFormatMark[i] != Format.FormatMark[i])
                    {
                        throw new InvalidOperationException($"Format mark wrong byte at {i}");
                    }
                }

                var actualIvMark = reader.ReadByte();

                if (actualIvMark != Format.IVMark)
                {
                    throw new InvalidOperationException($"Expected IV mark {Format.IVMark} got {actualIvMark}");
                }

                var ivSize = reader.ReadInt32();
                iv = reader.ReadBytes(ivSize);

                if (iv.Length != ivSize)
                {
                    throw new InvalidOperationException($"Expected IV size {ivSize} got {iv.Length}");
                }

                var actualDataMark = reader.ReadByte();

                if (actualDataMark != Format.EncryptedDataMark)
                {
                    throw new InvalidOperationException($"Expected data mark {Format.EncryptedDataMark} got {actualDataMark}");
                }

                var dataSize = reader.ReadInt32();
                encryptedData = reader.ReadBytes(dataSize);

                if (encryptedData.Length != dataSize)
                {
                    throw new InvalidOperationException($"Expected encrypted data size {dataSize} got {encryptedData.Length}");
                }
            }

            return (IV: iv, EncryptedData: encryptedData);
        }
    }
}