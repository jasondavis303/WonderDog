using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WonderDog
{
    static class Krypto
    {
        const int BUFFER_SIZE = 81920;
        const string MAGIC_STRING = "Krypto the Wonder Dog";

        private static Aes CreateAes(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            var ret = Aes.Create();

            //AES Key = 256 bits, SHA256 = 256 bits, so...
            using var sha = SHA256.Create();
            ret.Key = sha.ComputeHash(Encoding.UTF8.GetBytes(password));

            //Just a tiny bit of obfuscation. Won't really add much extra security, but I like it
            byte[] iv = new byte[16];
            for (int i = 0; i < 16; i++)
                iv[i] = ret.Key[31 - (i * 2)];
            ret.IV = iv;

            return ret;
        }

        private static byte[] GetMagicBytes() => Encoding.UTF8.GetBytes(MAGIC_STRING);

        private static bool IsMagicString(byte[] data) => Encoding.UTF8.GetString(data) == MAGIC_STRING;


        /// <summary>
        /// Encrypt small strings
        /// </summary>
        public static string Encrypt(string data, string password)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentNullException(nameof(data));

            using var aes = CreateAes(password);
            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            var magic = GetMagicBytes();
            cs.Write(magic, 0, magic.Length);

            using var sw = new StreamWriter(cs);
            sw.Write(data);
            sw.Flush();
            cs.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray(), Base64FormattingOptions.None);
        }

        /// <summary>
        /// Encrypt small chunks of data
        /// </summary>
        public static byte[] Encrypt(byte[] data, string password)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data));

            using var aes = CreateAes(password);
            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();

            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            var magic = GetMagicBytes();
            cs.Write(magic, 0, magic.Length);

            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();

            return ms.ToArray();
        }

        public static async Task EncryptFileAsync(string filename, string password)
        {
            string tmpFile = filename + ".tmp";
            using (var src = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, true))
            {
                using var aes = CreateAes(password);
                using var encryptor = aes.CreateEncryptor();

                using var dst = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE, true);
                
                using var cs = new CryptoStream(dst, encryptor, CryptoStreamMode.Write);
                var magic = GetMagicBytes();
                await cs.WriteAsync(magic, 0, magic.Length).ConfigureAwait(false);
            
                await src.CopyToAsync(cs, BUFFER_SIZE).ConfigureAwait(false);
                cs.FlushFinalBlock();
            }

            File.Move(tmpFile, filename, true);
        }

        /// <summary>
        /// Decrypt small strings
        /// </summary>
        public static string Decrypt(string data, string password)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentNullException(nameof(data));

            using var aes = CreateAes(password);
            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(Convert.FromBase64String(data));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);

            var magic = new byte[MAGIC_STRING.Length];
            int read = cs.Read(magic, 0, magic.Length);
            if (read != magic.Length)
                throw new Exception("Error descrypting magic string");
            if (!IsMagicString(magic))
                throw new Exception("Invalid password, or file not encrypted with WonderDog");

            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

        /// <summary>
        /// Decrypt small chunks of data
        /// </summary>
        public static byte[] Decrypt(byte[] data, string password)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data));

            using var aes = CreateAes(password);
            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(data);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);

            var magic = new byte[MAGIC_STRING.Length];
            int read = cs.Read(magic, 0, magic.Length);
            if (read != magic.Length)
                throw new Exception("Error descrypting magic string");
            if (!IsMagicString(magic))
                throw new Exception("Invalid password, or file not encrypted with WonderDog");
           
            var ret = new byte[data.Length - magic.Length];
            int totalRead = 0;
            while((read = cs.Read(ret, totalRead, ret.Length - totalRead)) != 0)
            {
                totalRead += read;
                if (totalRead == ret.Length)
                    break;
            }

            return ret;
        }

        public static async Task DecryptFileAsync(string filename, string password)
        {
            string tmpFile = filename + ".tmp";
            using (var src = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, true))
            {
                using var aes = CreateAes(password);
                using var decryptor = aes.CreateDecryptor();
                using var cs = new CryptoStream(src, decryptor, CryptoStreamMode.Read);

                var magic = new byte[MAGIC_STRING.Length];
                int read = await cs.ReadAsync(magic, 0, magic.Length).ConfigureAwait(false);
                if (read != magic.Length)
                    throw new Exception("Error descrypting magic string");
                
                if (!IsMagicString(magic))
                    throw new Exception("Invalid password, or file not encrypted with WonderDog");

                using var dst = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE, true);
                await cs.CopyToAsync(dst, BUFFER_SIZE).ConfigureAwait(false);
            }

            File.Move(tmpFile, filename, true);
        }
     
    }
}
