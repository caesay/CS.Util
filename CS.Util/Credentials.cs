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

        public unsafe Credentials(string username, string password)
        {
            Username = username;
            fixed (char* chPtr = password)
              Password = new SecureString(chPtr, password.Length);
        }
        public unsafe Credentials(string username, char[] password)
        {
            Username = username;
            fixed (char* chPtr = password)
              Password = new SecureString(chPtr, password.Length);
        }
        public Credentials(string username, SecureString password)
        {
            Username = username;
            Password = password;
        }

        public Credentials Clone()
        {
            return new Credentials(Username, Password.Copy());
        }
        public void Dispose()
        {
            Password.Dispose();
        }
        public bool Equals(Credentials other)
        {
            if (other == null) return false;
            if (other.Username != Username) return false;
            if (Password == null && (other.Password == null)) return true;
            if (Password == null != (other.Password == null)) return false;
            if (Password.Length != other.Password.Length) return false;
            bool equal = false;
            Password.UseSecurely(p1 => { other.Password.UseSecurely(p2 => { equal = p1 == p2; }); });
            return equal;
        }
        public override string ToString()
        {
            return $"User:{Username}, Pass:" + new string('*', Password.Length);
        }
        public override int GetHashCode()
        {
            return this.GetAutoHashCode(MemberSelector.PublicProperties);
        }
        public override bool Equals(object obj)
        {
            var comp = obj as Credentials;
            if (comp == null) return false;
            return Equals(comp);
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
