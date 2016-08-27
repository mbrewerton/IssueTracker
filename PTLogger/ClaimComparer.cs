using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PTLogger
{
    //Credit Thinktecture.IdentityModel, just didn't want the whole package
    //https://github.com/IdentityModel/Thinktecture.IdentityModel/blob/master/source/Core/ClaimComparer.cs
    public class ClaimComparer : IEqualityComparer<Claim>
    {
        public bool Equals(Claim x, Claim y)
        {
            if (x == null && y == null) return true;
            if (x == null && y != null) return false;
            if (x != null && y == null) return false;

            return (x.Type == y.Type &&
                    x.Value == y.Value);
        }

        public int GetHashCode(Claim claim)
        {
            if (Object.ReferenceEquals(claim, null)) return 0;

            int typeHash = claim.Type == null ? 0 : claim.Type.GetHashCode();
            int valueHash = claim.Value == null ? 0 : claim.Value.GetHashCode();

            return typeHash ^ valueHash;
        }
    }
}