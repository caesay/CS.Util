using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Cryptography
{
    public abstract class DeriveBytesGenerator<T>
    {
        internal DeriveBytesGenerator()
        {
        }
        public abstract T CreateNew(byte[] password, byte[] salt, int iterations);
    }
    public class DeriveBytesBase<TAlg, TGen>
       where TAlg : DeriveBytes
       where TGen : DeriveBytesGenerator<TAlg>
    {
        public static Encoding Encoding
        {
            get { return CngHashBase<HashAlgorithm>.Encoding; }
            set { CngHashBase<HashAlgorithm>.Encoding = value; }
        }
        public static int Iterations { get; set; } = 5000;
        public static int DerivedKeyLength { get; set; } = 20;

        internal DeriveBytesBase()
        {
        }

        public static string Compute(string password, string salt)
        {
            return Compute(Encoding.GetBytes(password), Encoding.GetBytes(salt));
        }
        public static string Compute(string password, byte[] salt)
        {
            return Compute(Encoding.GetBytes(password), salt);
        }
        public static string Compute(byte[] password, string salt)
        {
            return Compute(password, Encoding.GetBytes(salt));
        }
        public static string Compute(byte[] password, byte[] salt)
        {
            return Convert.ToBase64String(GetBytes(DerivedKeyLength, password, salt, Iterations));
        }

        private static byte[] GetBytes(int dklen, byte[] password, byte[] salt, int iterationCount)
        {
            var generator = Activator.CreateInstance<TGen>();
            using (var alg = generator.CreateNew(password, salt, iterationCount))
            {
                return alg.GetBytes(dklen);
            }
        }
    }
}
