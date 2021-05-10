using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TEA
{
    public static class AlgoTea
    {
        public static string Encrypt(string data, string key) => Encoding.Default.GetString(AlgoTea.Encrypt(Encoding.Default.GetBytes(data), Encoding.Default.GetBytes(key)));

        public static string Decrypt(string data, string key) => Encoding.Default.GetString(AlgoTea.Decrypt(Encoding.Default.GetBytes(data), Encoding.Default.GetBytes(key)));

        public static byte[] Encrypt(byte[] data, byte[] key)
        {

            uint[] key1 = AlgoTea.CreateKey(key);
            uint[] v = new uint[2];
            byte[] buffer = new byte[AlgoTea.NextMultipleOf8(data.Length + 4)];
            
            byte[] bytes = BitConverter.GetBytes(data.Length);
            Array.Copy((Array)bytes, (Array)buffer, bytes.Length);
            Array.Copy((Array)data, 0, (Array)buffer, bytes.Length, data.Length);
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter((Stream)memoryStream))
                {
                    for (int startIndex = 0; startIndex < buffer.Length; startIndex += 8)
                    {
                        v[0] = BitConverter.ToUInt32(buffer, startIndex);
                        v[1] = BitConverter.ToUInt32(buffer, startIndex + 4);

                        AlgoTea.BlockEncrypt(v, key1);
                        binaryWriter.Write(v[0]);
                        binaryWriter.Write(v[1]);
                    }
                }
            }            
            return buffer;
            
        }

        public static byte[] Decrypt(byte[] data1, byte[] key)
        {
            byte[] data = AlgoTea.Encrypt(data1, key);            
            if (data.Length % 8 != 0)
                throw new ArgumentException("Длина зашифрованных данных должна быть кратна 8 байтам");
            uint[] key1 = AlgoTea.CreateKey(key);
            uint[] v = new uint[2];
            byte[] buffer = new byte[data.Length];
            Array.Copy((Array)data, (Array)buffer, data.Length);
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter((Stream)memoryStream))
                {
                    for (int startIndex = 0; startIndex < buffer.Length; startIndex += 8)
                    {
                        v[0] = BitConverter.ToUInt32(buffer, startIndex);
                        v[1] = BitConverter.ToUInt32(buffer, startIndex + 4);
                        AlgoTea.BlockDecrypt(v, key1);
                        binaryWriter.Write(v[0]);
                        binaryWriter.Write(v[1]);
                    }
                }
            }
            uint uint32 = BitConverter.ToUInt32(buffer, 0);
            if ((long)uint32 > (long)(buffer.Length - 4))
                throw new ArgumentException("Зашифрованные данные разрушены");
            byte[] numArray = new byte[(int)uint32];
            Array.Copy((Array)buffer, 4L, (Array)numArray, 0L, (long)uint32);
            return numArray;
        }

        private static int NextMultipleOf8(int length) => (length + 7) / 8 * 8;

        private static uint[] CreateKey(byte[] key)
        {
            byte[] numArray = new byte[16];
            for (int index = 0; index < key.Length; ++index)
                numArray[index % 16] = (byte)(31U * (uint)numArray[index % 16] ^ (uint)key[index]);
            for (int length = key.Length; length < numArray.Length; ++length)
                numArray[length] = (byte)((uint)(17 * length) ^ (uint)key[length % key.Length]);
            return new uint[4]
            {
        BitConverter.ToUInt32(numArray, 0),
        BitConverter.ToUInt32(numArray, 4),
        BitConverter.ToUInt32(numArray, 8),
        BitConverter.ToUInt32(numArray, 12)
            };
        }

        private static void BlockEncrypt(uint[] v, uint[] key)
        {            
            uint num1 = v[0];
            uint num2 = v[1];
            uint num3 = 0;
            uint num4 = key[0];
            uint num5 = key[1];
            uint num6 = key[2];
            uint num7 = key[3];
            for (uint index = 0; index < 32U; ++index)
            {
                num3 += 2654435769U;//золотое сечение
                num1 += (uint)(((int)num2 << 4) + (int)num4 ^ (int)num2 + (int)num3 ^ (int)(num2 >> 5) + (int)num5);
                num2 += (uint)(((int)num1 << 4) + (int)num6 ^ (int)num1 + (int)num3 ^ (int)(num1 >> 5) + (int)num7);
            }
            v[0] = num1;
            v[1] = num2;
        }

        private static void BlockDecrypt(uint[] v, uint[] key)
        {
            uint num1 = v[0];
            uint num2 = v[1];
            uint num3 = 3337565984;
            uint num4 = key[0];
            uint num5 = key[1];
            uint num6 = key[2];
            uint num7 = key[3];
            for (uint index = 0; index < 32U; ++index)
            {
                num2 -= (uint)(((int)num1 << 4) + (int)num6 ^ (int)num1 + (int)num3 ^ (int)(num1 >> 5) + (int)num7);
                num1 -= (uint)(((int)num2 << 4) + (int)num4 ^ (int)num2 + (int)num3 ^ (int)(num2 >> 5) + (int)num5);
                num3 -= 2654435769U;
            }
            v[0] = num1;
            v[1] = num2;
        }
    }
}
