using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CS.Reactive;
using CS.Reactive.Network;
using CS.Util;
using CS.Util.Collections;
using CS.Util.Cryptography;
using CS.Util.Dynamic;
using CS.Util.Extensions;
using CS.Util.Json;

namespace CS.ConsoleTests
{
    public interface ASDTEST
    {
        string i2 { get; set; }
        string i3 { get; set; }

        string Combine(string arg, string arg2);
        string GetProps();
    }

    public class ASD 
    {
        public string i1 = "asdasdsad";
        public string i2 { get; set; } = "asdasdsad";
        private string i3 { get; set; } = "asdasdsad";
        public ASD2 i4 = new ASD2();
    }

    public class ASD2
    {
        public string hello = "asdsad";
        public int[] numbers = new[] {1, 2, 3, 4};
    }
    class Program
    {
        static void Main(string[] args)
        {
            var t = DynamicTypeFactory.Class()
                .Interface<ASDTEST>() // new class implements this interface
                .Field<string>("_TEST") // has a field named _TEST
                .Field<string>("_TEST2") // has a field named _TEST
                .Property<string>("i2", (ctx) => (string)ctx.Field("_TEST"), (ctx, s) => ctx.Field("_TEST", ctx.Field("_TEST2") + s)) // has a property using _TEST as a backing field
                .Property<string>("i3", (ctx) => (string)ctx.Field("_TEST2"), (ctx, s) => ctx.Field("_TEST2", s)) // has an auto property
                .Method<string, string, string>("Combine", (ctx, s1, s2) => ctx.Field("_TEST2") + s1 + s2)
                .Method<string>("GetProps", (ctx) =>
                {
                    var ret = ctx.Field("_TEST2") + (string) ctx.Property("i2");
                    ctx.Property("i2", ret);
                    return ret;
                })
                .Build();

            var obj = (ASDTEST)Activator.CreateInstance(t);

            obj.i3 = "pre_";
            obj.i2 = "test";

            var asa = obj.i2;
            var as2 = obj.Combine(" HI ", "BYE");
            var as3 = obj.GetProps();
            var as4 = obj.i2;

            Console.WriteLine(  );
        }

        static void testPrettyTime()
        {
            var st2 = PrettyTime.Format(DateTime.Now.AddDays(1).AddHours(1));
            var st32 = PrettyTime.Format(DateTime.Now.AddHours(23));
            var st22 = PrettyTime.Format(DateTime.Now.AddDays(2));
            var st356 = PrettyTime.Format(DateTime.Now.AddHours(33));

            var st3 = PrettyTime.Format(DateTime.Now.AddHours(-23));
            var st = PrettyTime.Format(DateTime.Now.AddDays(-1));
            var st4 = PrettyTime.Format(DateTime.Now.AddDays(-2));
            var st35 = PrettyTime.Format(DateTime.Now.AddHours(-33));
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

            //Credentials c2 = Credentials.Decrypt(secure);
            //string pass = Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(c2.Password));
            //Console.WriteLine();
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
