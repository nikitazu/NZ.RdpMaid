using System.IO;
using System.Security.Cryptography;

namespace NZ.RdpMaid.App.Extensions.Encryption
{
    internal static class SymmetricAlgorithmExtensions
    {
        public static byte[] EncryptByteArray(this SymmetricAlgorithm aes, byte[] inputData)
        {
            using var input = new MemoryStream(inputData);
            using var output = new MemoryStream();
            using (var pipe = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                input.CopyTo(pipe);
            }

            return output.ToArray();
        }

        public static byte[] DecryptByteArray(this SymmetricAlgorithm aes, byte[] inputData)
        {
            using (var input = new MemoryStream(inputData))
            using (var output = new MemoryStream())
            {
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var pipe = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
                {
                    pipe.CopyTo(output);
                }

                output.Position = 0;

                return output.ToArray();
            }
        }
    }
}