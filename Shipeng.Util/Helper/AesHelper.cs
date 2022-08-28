using System.Security.Cryptography;
using System.Text;

namespace Shipeng.Util.Helper
{
    /// <summary>
    /// https://blog.csdn.net/zimumanbu/article/details/81660970
    /// </summary>
    public class AesHelper
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="Data"> </param>
        /// <param name="Key"> </param>
        /// <returns> </returns>
        public static string AESEncrypts(string Data, string Key)
        {
            // 256-AES key
            byte[] keyArray = HexStringToBytes(Key);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(Data);

            RijndaelManaged rDel = new RijndaelManaged
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return BytesToHexString(resultArray);
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="Data"> </param>
        /// <param name="Key"> </param>
        /// <returns> </returns>
        public static string AESDecrypt(string Data, string Key)
        {
            byte[] keyArray = HexStringToBytes(Key);
            RijndaelManaged rijndaelCipher = new RijndaelManaged
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            byte[] toEncryptArray = HexStringToBytes(Data);

            //byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(Data);
            //byte[] byteArray = Convert.FromBase64String(Data);
            ICryptoTransform transform = rijndaelCipher.CreateDecryptor();
            if (transform.IsNullOrEmpty())
            {
                throw new BusException(false, "解密失败", 1000);
            }
            byte[] plainText = transform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return UTF8Encoding.UTF8.GetString(plainText);
            // return BytesToHexString(plainText);
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key">密钥</param>
        /// <param name="iv">初始向量</param>
        /// <param name="padding">填充模式</param>
        /// <param name="mode">加密模式</param>
        /// <returns></returns>
        public static string AESDecryptText(string source, string key, string iv = "", PaddingMode padding = PaddingMode.PKCS7, CipherMode mode = CipherMode.ECB)
        {
            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] textBytes = Encoding.UTF8.GetBytes(source);
                byte[] ivBytes = Encoding.UTF8.GetBytes(iv);

                byte[] useKeyBytes = new byte[16];
                byte[] useIvBytes = new byte[16];

                if (keyBytes.Length > useKeyBytes.Length)
                {
                    Array.Copy(keyBytes, useKeyBytes, useKeyBytes.Length);
                }
                else
                {
                    Array.Copy(keyBytes, useKeyBytes, keyBytes.Length);
                }

                if (ivBytes.Length > useIvBytes.Length)
                {
                    Array.Copy(ivBytes, useIvBytes, useIvBytes.Length);
                }
                else
                {
                    Array.Copy(ivBytes, useIvBytes, ivBytes.Length);
                }

                Aes aes = System.Security.Cryptography.Aes.Create();
                aes.KeySize = 256;//秘钥的大小，以位为单位,128,256等
                aes.BlockSize = 128;//支持的块大小
                aes.Padding = padding;//填充模式
                aes.Mode = mode;
                aes.Key = useKeyBytes;
                aes.IV = useIvBytes;//初始化向量，如果没有设置默认的16个0

                ICryptoTransform decryptoTransform = aes.CreateDecryptor();
                byte[] resultBytes = decryptoTransform.TransformFinalBlock(textBytes, 0, textBytes.Length);

                return Encoding.UTF8.GetString(resultBytes);
            }
            catch (Exception ex)
            {
                throw new BusException(ex.Message);
            }
        }

        /// <summary>
        /// Byte array to convert 16 hex string
        /// </summary>
        /// <param name="bytes"> byte array </param>
        /// <returns> 16 hex string </returns>
        public static string BytesToHexString(byte[] bytes)
        {
            StringBuilder returnStr = new StringBuilder();
            if (bytes != null || bytes.Length == 0)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr.Append(bytes[i].ToString("X2"));
                }
            }
            return returnStr.ToString();
        }

        /// <summary>
        /// 16 hex string converted to byte array
        /// </summary>
        /// <param name="hexString"> 16 hex string </param>
        /// <returns> byte array </returns>
        public static byte[] HexStringToBytes(string hexString)
        {
            if (hexString == null || hexString.Equals(""))
            {
                return null;
            }
            int length = hexString.Length / 2;
            if (hexString.Length % 2 != 0)
            {
                return null;
            }
            byte[] d = new byte[length];
            for (int i = 0; i < length; i++)
            {
                d[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return d;
        }

        #region Aes解密
        public static int KEYSIZE = 1024;
        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="CryptText">待解密字符串</param>
        /// <param name="PrivateKey">解密私钥</param>
        /// <param name="TextData">输出:已解密的字符串</param>
        /// <returns>0:成功解密 -1:待解密字符串不为能空 -2:解密私钥不能为空 -4:其他错误</returns>
        public static int DecryptText(string CryptText, string PrivateKey, out string TextData)
        {
            TextData = "";

            if (string.IsNullOrEmpty(CryptText))
            {
                return -1;
            }

            if (string.IsNullOrEmpty(PrivateKey))
            {
                return -2;
            }

            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(KEYSIZE);

                byte[] prikey = Convert.FromBase64String(PrivateKey); ;
                rsa.ImportCspBlob(prikey);

                byte[] cipherBytes = Convert.FromBase64String(CryptText);
                byte[] plainText = rsa.Decrypt(cipherBytes, false);

                TextData = Encoding.UTF8.GetString(plainText);

                return 0;
            }
            catch { return -4; }
        }

        #endregion
    }



    public class AesHelperDatat
    {
        /// <summary>
        /// 加密文本
        /// </summary>
        public string txt { get; set; }

        /// <summary>
        /// key
        /// </summary>
        public string key { get; set; }
    }
}