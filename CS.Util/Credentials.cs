using System;
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
    public class Credentials : ICloneable, IDisposable, IEquatable<Credentials>
    {
        public string Username { get; }
        public SecureString Password { get; }
        public bool IsReadOnly => true;

        [Obsolete("Using this constructor will leave a plain-text copy of the password in managed memory because strings are immutable. Consider using one of the alternatives.")]
        public unsafe Credentials(string username, string password)
        {
            Username = username;
            fixed (char* chPtr = password)
              Password = new SecureString(chPtr, password.Length);
            Password.MakeReadOnly();
        }
        public unsafe Credentials(string username, char[] password)
        {
            Username = username;
            fixed (char* chPtr = password)
              Password = new SecureString(chPtr, password.Length);
            Password.MakeReadOnly();
        }
        public Credentials(string username, SecureString password)
        {
            Username = username;
            Password = password;
            Password.MakeReadOnly();
        }
        ~Credentials()
        {
            Dispose();
        }

        public Credentials Clone()
        {
            return new Credentials(Username, Password.Copy());
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
        public bool Equals(Credentials other)
        {
            if (other == null) return false;
            if (other.Username != Username) return false;

            if (Password != null && other.Password != null)
                return Password.UseSecurely(p1 => other.Password.UseSecurely(p2 => p1 == p2));

            return ((Password == null) == (other.Password == null));
        }
        public override bool Equals(object obj)
        {
            var comp = obj as Credentials;
            if (comp == null) return false;
            return Equals(comp);
        }
        public override string ToString()
        {
            return $"User:{Username}, Pass:" + new string('*', Password.Length);
        }
        public override int GetHashCode()
        {
            int pHash = Password.UseSecurely(pclr => pclr.GetHashCode());
            return this.GetAutoHashCode(Username.GetHashCode(), pHash);
        }
        public void Dispose()
        {
            Password.Dispose();
        }
    }
}
