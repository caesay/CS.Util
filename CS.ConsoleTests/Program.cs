using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CS.Reactive;
using CS.Reactive.Network;
using CS.Util;
using CS.Util.Cryptography;
using CS.Util.Extensions;

namespace CS.ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine();
        }

        static void rsaEncr()
        {
            using (var rsa = new RSACryptoServiceProvider(1024))
            {
                try
                {
                    //RSAParameters privateKey = rsa.ExportParameters(true);
                    RSAParameters publicKey = rsa.ExportParameters(false);


                    var bytes = asd.ConvertPublicKey(publicKey);
                    File.WriteAllBytes("test.pem", bytes);
                    ////var asnMessage = AsnKeyBuilder.PublicKeyToX509(publicKey);
                    //var asnMessage = AsnKeyBuilder.PrivateKeyToPKCS8(privateKey);
                    //var asnBytes = asnMessage.GetBytes();

                    X509Certificate2 cert = new X509Certificate2(bytes, "", X509KeyStorageFlags.MachineKeySet);
                    //var a = new AsymmetricAlgorithm()
                    //Console.WriteLine();
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        static void testCred()
        {
            byte[] entropy = Encoding.UTF8.GetBytes("Hello, this is some entropy");

            Credentials c = new Credentials("user", "pass");

            var secure = c.Encrypt(SecureScope.LocalMachine, entropy);

            Credentials c2 = Credentials.Decrypt(secure);
            string pass = Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(c2.Password));
            Console.WriteLine();
        }

        static void testNetClient()
        {
            NetServer server = new NetServer(9556);
            server.ClientConnected.Subscribe(tracked =>
            {
                Console.WriteLine("[SERVER]: Client connected.");
                tracked.Value.MessageRecieved.Subscribe(
                    next_msg => Console.WriteLine("[SERVER]: Client Message: " + Encoding.UTF8.GetString(next_msg.Message)),
                    next_err => Console.WriteLine("[SERVER]: Client Read Error: " + next_err.Message),
                    () => Console.WriteLine("[SERVER]: Client Disconnected."));

                var sClient = tracked.Observe();
                sClient.Write(Encoding.UTF8.GetBytes("Hello, new client!"));
            });
            server.Start();

            NetClient client = new NetClient("localhost", 9556);
            client.MessageRecieved.Subscribe(
                next_msg => Console.WriteLine("[CLIENT]: Server Message: " + Encoding.UTF8.GetString(next_msg.Message)),
                next_err => Console.WriteLine("[CLIENT]: Server Read Error: " + next_err.Message),
                () => Console.WriteLine("[CLIENT]: Server Disconnected."));
            client.Connect();
            Console.WriteLine("[CLIENT]: Connected.");

            client.Write(Encoding.UTF8.GetBytes("Hello back!!!"));

            Console.ReadLine();
        }
    }

    class asd
    {
        // Object ID for RSA
        private static byte[] RSA_OID = { 0x30, 0xD, 0x6, 0x9, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0xD, 0x1, 0x1, 0x1, 0x5, 0x0 }; 

        // Corresponding ASN identification bytes
        const byte INTEGER = 0x2;
        const byte SEQUENCE = 0x30;
        const byte BIT_STRING = 0x3;
        const byte OCTET_STRING = 0x4;

        public static byte[] ConvertPublicKey(RSAParameters param)
        {
            List<byte> arrBinaryPublicKey = new List<byte>();

            arrBinaryPublicKey.InsertRange(0, param.Exponent);
            arrBinaryPublicKey.Insert(0, (byte)arrBinaryPublicKey.Count);
            arrBinaryPublicKey.Insert(0, INTEGER);

            arrBinaryPublicKey.InsertRange(0, param.Modulus);
            AppendLength(ref arrBinaryPublicKey, param.Modulus.Length);
            arrBinaryPublicKey.Insert(0, INTEGER);

            AppendLength(ref arrBinaryPublicKey, arrBinaryPublicKey.Count);
            arrBinaryPublicKey.Insert(0, SEQUENCE);

            arrBinaryPublicKey.Insert(0, 0x0); // Add NULL value

            AppendLength(ref arrBinaryPublicKey, arrBinaryPublicKey.Count);

            arrBinaryPublicKey.Insert(0, BIT_STRING);
            arrBinaryPublicKey.InsertRange(0, RSA_OID);

            AppendLength(ref arrBinaryPublicKey, arrBinaryPublicKey.Count);

            arrBinaryPublicKey.Insert(0, SEQUENCE);

            return arrBinaryPublicKey.ToArray();
        }
        private static void AppendLength(ref List<byte> arrBinaryData, int nLen)
        {
            if (nLen <= byte.MaxValue)
            {
                arrBinaryData.Insert(0, Convert.ToByte(nLen));
                arrBinaryData.Insert(0, 0x81); //This byte means that the length fits in one byte
            }
            else
            {
                arrBinaryData.Insert(0, Convert.ToByte(nLen % (byte.MaxValue + 1)));
                arrBinaryData.Insert(0, Convert.ToByte(nLen / (byte.MaxValue + 1)));
                arrBinaryData.Insert(0, 0x82); //This byte means that the length fits in two byte
            }

        }
    }
}
