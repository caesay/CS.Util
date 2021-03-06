﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CS.Util.Extensions;

namespace CS.Util
{
    /// <summary>
    /// A class designed to securely hold in memory, or on disk, a set of credentials (a username, and password).
    /// The password will never exist in plaintext in managed memory or on the disk provided that this class is initialized properly.
    /// </summary>
    public class Credentials : ICloneable, IDisposable, IEquatable<Credentials>
    {
        public string Username { get; }
        private readonly SecureString _password;

        [Obsolete("Using this constructor will leave a plain-text copy of the password in managed memory because strings are immutable. Consider using one of the alternatives.")]
        public unsafe Credentials(string username, string password)
        {
            Username = username;
            fixed (char* chPtr = password)
              _password = new SecureString(chPtr, password.Length);
            _password.MakeReadOnly();
        }
        public unsafe Credentials(string username, char[] password)
        {
            Username = username;
            fixed (char* chPtr = password)
              _password = new SecureString(chPtr, password.Length);
            _password.MakeReadOnly();
        }
        public Credentials(string username, SecureString password)
        {
            Username = username;
            _password = password;
            _password.MakeReadOnly();
        }

        ~Credentials()
        {
            Dispose();
        }

        private const int VAR_LENGTH = 4;
        public byte[] Encrypt(SecureScope scope, byte[] entropy = null)
        {
            if (entropy == null)
                entropy = new byte[0];

            if (entropy.Length > 255)
                throw new ArgumentOutOfRangeException(nameof(entropy), "Entropy must be no longer than 255 bytes for encryption.");

            if (Username.Length > 255)
                throw new ArgumentOutOfRangeException(nameof(Username), "Username must be no longer than 255 bytes for encryption.");

            var userBytes = Encoding.Unicode.GetBytes(Username);
            var bstr = Marshal.SecureStringToBSTR(_password);
            try
            {
                var length = Marshal.ReadInt32(bstr, -4);
                if (length > 255)
                    throw new ArgumentOutOfRangeException(nameof(_password), "Password must be no longer than 255 bytes for encryption.");
                var bytes = new byte[length + userBytes.Length];
                var bytesPin = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                try
                {
                    Buffer.BlockCopy(userBytes, 0, bytes, 0, userBytes.Length);
                    Marshal.Copy(bstr, bytes, userBytes.Length, length);
                    byte[] encrypted = ProtectedData.Protect(bytes, entropy, (DataProtectionScope)(int)scope);

                    
                    byte[] wrapped = new byte[encrypted.Length + VAR_LENGTH + entropy.Length];
                    wrapped[0] = (byte)scope;
                    wrapped[1] = (byte)entropy.Length;
                    wrapped[2] = (byte)userBytes.Length;
                    wrapped[3] = (byte)_password.Length;
                    Buffer.BlockCopy(entropy, 0, wrapped, VAR_LENGTH, entropy.Length);
                    Buffer.BlockCopy(encrypted, 0, wrapped, VAR_LENGTH + entropy.Length, encrypted.Length);
                    return wrapped;
                }
                finally
                {
                    for (var i = 0; i < bytes.Length; i++)
                        bytes[i] = 0;
                    bytesPin.Free();
                }
            }
            finally
            {
                Marshal.ZeroFreeBSTR(bstr);
            }
        }
        public static unsafe Credentials Decrypt(byte[] credentialBytes)
        {
            // retrieve variables
            var scope = (DataProtectionScope)credentialBytes[0];
            var entropyLength = credentialBytes[1];
            var userLength = credentialBytes[2];
            var passLength = credentialBytes[3];

            // retrieve entropy
            var entropy = new byte[entropyLength];
            Buffer.BlockCopy(credentialBytes, VAR_LENGTH, entropy, 0, entropyLength);

            // retrieve secured data
            var secureData = new byte[credentialBytes.Length - VAR_LENGTH - entropyLength];
            Buffer.BlockCopy(credentialBytes, VAR_LENGTH + entropyLength, secureData, 0, secureData.Length);

            // pin the byte array to minimize the chance of it getting copied or leaked.
            byte[] plainBytes = ProtectedData.Unprotect(secureData, entropy, scope);
            var plainBytesPin = GCHandle.Alloc(plainBytes, GCHandleType.Pinned);
            try
            {
                // retrieve username
                byte[] userBytes = new byte[userLength];
                Buffer.BlockCopy(plainBytes, 0, userBytes, 0, userLength);
                string username = Encoding.Unicode.GetString(userBytes);

                // retrieve password (unsafely, import directly into SecureString)
                fixed (byte* pinnedSrc = plainBytes)
                {
                    char* loc = (char*)(pinnedSrc + userLength);
                    SecureString secure = new SecureString(loc, passLength);
                    return new Credentials(username, secure);
                }
            }
            finally
            {
                // clear plain text data from memory
                for (var i = 0; i < plainBytes.Length; i++)
                    plainBytes[i] = 0;
                plainBytesPin.Free();
            }
        }

        public SecureString GetPasswordCopy()
        {
            return _password.Copy();
        }

        public Credentials Clone()
        {
            return new Credentials(Username, _password.Copy());
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
        public bool Equals(Credentials other)
        {
            if (other == null) return false;
            if (other.Username != Username) return false;

            if (_password != null && other._password != null)
                return _password.UseSecurely(p1 => other._password.UseSecurely(p2 => p1 == p2));

            return ((_password == null) == (other._password == null));
        }
        public override bool Equals(object obj)
        {
            var comp = obj as Credentials;
            if (comp == null) return false;
            return Equals(comp);
        }
        public override string ToString()
        {
            return $"User:{Username}, Pass:" + new string('*', _password.Length);
        }
        public override int GetHashCode()
        {
            int pHash = _password.UseSecurely(pclr => pclr.GetHashCode());
            return this.GetAutoHashCode(Username.GetHashCode(), pHash);
        }
        public void Dispose()
        {
            _password.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public enum SecureScope
    {
        CurrentUser,
        LocalMachine,
    }
}
