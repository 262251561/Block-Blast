using System;
using System.IO;
using System.Security.Cryptography;

namespace Net
{
    public class CsMsgCryptor
    {
        private byte[] _tempBuffer = null;
        private RijndaelManagedTransform2 _encryptor = null;
        private RijndaelManagedTransform2 _decryptor = null;
        private byte[] _key = null;

        public byte[] CryptKey
        {
            get { return _key; }
            set
            {
                _key = value;
                _encryptor = null;
                _decryptor = null;
            }
        }

        public CsMsgCryptor()
        {
            _encryptor = null;
            _decryptor = null;
        }

        void EnsureBufferSize(int size)
        {
            if (_tempBuffer == null || _tempBuffer.Length < size)
            {
                _tempBuffer = new byte[size];
            }
        }

        /// <summary>  
        /// AES加密(无向量)  
        /// </summary>  
        /// <param name="src">被加密的明文</param>
        /// <param name="srcOffset"></param>
        /// <param name="srcLength"></param>
        /// <param name="output">结果</param>
        /// <param name="outputOffset"></param>
        /// <returns>密文</returns>  
        public int Encrypt(byte[] src, int srcOffset, int srcLength, byte[] output, int outputOffset)
        {
            //return Plugins.Common.Encryption.Aes.Encrypt(_key, src, srcOffset, srcLength, output, outputOffset);
            try
            {
                if (_encryptor == null)
                {
                    _encryptor = new RijndaelManagedTransform2(_key, CipherMode.CBC, _key, 128, 128, PaddingMode.PKCS7, RijndaelManagedTransformMode.Encrypt);
                }
                else
                {
                    _encryptor.Reset();
                }

                using (MemoryStream outputMs = new MemoryStream(output, outputOffset, output.Length - outputOffset))
                using (CryptoStream cryptoStream = new CryptoStream(outputMs, _encryptor, CryptoStreamMode.Write))
                {
                    if (srcOffset != 0)
                    {
                        EnsureBufferSize(srcLength);
                        Buffer.BlockCopy(src, srcOffset, _tempBuffer, 0, srcLength);
                        cryptoStream.Write(_tempBuffer, 0, srcLength);
                    }
                    else
                    {
                        cryptoStream.Write(src, 0, srcLength);
                    }
                    cryptoStream.FlushFinalBlock();
                    return (int)outputMs.Position;
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
        /// <param name="output">解密后数据</param>
        /// <param name="outputOffset"></param>
        /// <returns></returns>  
        public int Decrypt(byte[] src, int srcOffset, int srcLength, byte[] output, int outputOffset)
        {
            if (_decryptor == null)
            {
                _decryptor = new RijndaelManagedTransform2(_key, CipherMode.CBC, _key, 128, 128, PaddingMode.PKCS7, RijndaelManagedTransformMode.Decrypt);
            }
            return _decryptor.DecryptBytes(src, srcOffset, srcLength, output, outputOffset);
        }
    }
}
