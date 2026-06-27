using NPOI.POIFS.Crypt;
using System.Text;

namespace Shipeng.Util.Helper
{
    public sealed class EncryptUtils
    {

        public static string decrypt(string content, string key)
        {
            byte[] data = hex2byte(content);
            byte[] keys = hex2byte(key);
            data = decrypt(data, keys);
            return UTF8Encoding.UTF8.GetString(data);
        }

        private static byte[] hex2byte(string inputString)
        {
            if (inputString == null || inputString.Length < 2)
            {
                return new byte[0];
            }
            inputString = inputString.ToLower();
            int l = inputString.Length / 2;
            byte[] result = new byte[l];
            for (int i = 0; i < l; i++)
            {
                result[i] = Convert.ToByte(inputString.Substring(i * 2, 2), 16);
            }
            return result;
        }

        public static byte[] decrypt(byte[] content, byte[] key)
        {
            try
            {

                Cipher cipher = Cipher.GetInstance("AES/ECB/PKCS5Padding");
                cipher.Init(2, new SecretKeySpec(key, "AES"));
                return cipher.DoFinal(content);
            }
            catch (Exception e)
            {
                throw new BusException(e.Message);
            }
        }


    }
}
