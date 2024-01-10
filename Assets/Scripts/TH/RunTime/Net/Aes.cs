using System;
using System.IO;
using System.Security.Cryptography;

namespace Net
{
    public static class Aes
    {
        private static byte[] TmpBuffer = null;

        private static byte[] GetTmpBuffer(int cap)
        {
            if (TmpBuffer == null)
            {
                TmpBuffer = new byte[System.Math.Min(int.MaxValue, cap * 2)];
            }
            return TmpBuffer;
        }

        public static int Encrypt(byte[] key, byte[] src, byte[] output)
        {
            return Encrypt(key, src, 0, src.Length, output, 0);
        }

        public static int Decrypt(byte[] key, byte[] src, byte[] output)
        {
            return Decrypt(key, src, 0, src.Length, output, 0);
        }

        /// <summary>  
        /// AES加密(无向量)  
        /// </summary>  
        /// <param name="src">被加密的明文</param>
        /// <param name="srcOffset"></param>
        /// <param name="srcLength"></param>
        /// <param name="key">密钥</param>
        /// <param name="output">结果</param>
        /// <param name="outputOffset"></param>
        /// <returns>密文</returns>  
        public static int Encrypt(byte[] key, byte[] src, int srcOffset, int srcLength, byte[] output, int outputOffset)
        {
            try
            {
                using (MemoryStream outputMs = new MemoryStream(output, outputOffset, output.Length - outputOffset))
                using (RijndaelManaged aes = new RijndaelManaged())
                {
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = key;
                    aes.IV = key;
                    using (CryptoStream cryptoStream = new CryptoStream(outputMs, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        if (srcOffset != 0)
                        {
                            var buff = GetTmpBuffer(srcLength);
                            Buffer.BlockCopy(src, srcOffset, buff, 0, srcLength);
                            cryptoStream.Write(buff, 0, srcLength);
                        }
                        else
                        {
                            cryptoStream.Write(src, 0, srcLength);
                        }
                        cryptoStream.FlushFinalBlock();
#if UNITY_EDITOR
                        if (outputMs.Position % key.Length != 0)
                        {
                            UnityEngine.Debug.LogWarning("Aes: Error Encrypt Result Size: " + (int) outputMs.Position);
                        }
#endif
                        return (int)outputMs.Position;
                    }
                }
            }
            catch
            {
                return 0;
            }
        }


        /// <summary>  
        /// AES解密(无向量)  
        /// </summary>  
        /// <param name="src">被加密的明文</param>
        /// <param name="srcOffset"></param>
        /// <param name="srcLength"></param>
        /// <param name="key">密钥</param>
        /// <param name="output">解密后数据</param>
        /// <param name="outputOffset"></param>
        /// <returns></returns>  
        public static int Decrypt(byte[] key, byte[] src, int srcOffset, int srcLength, byte[] output, int outputOffset)
        {
            try
            {
                using (MemoryStream inputStream = new MemoryStream(src, srcOffset, srcLength))
                using (RijndaelManaged aes = new RijndaelManaged())
                {
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = key;
                    aes.IV = key;
                    using (CryptoStream cryptoStream = new CryptoStream(
                        inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        return cryptoStream.Read(output, outputOffset, output.Length - outputOffset);
                    }
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}
