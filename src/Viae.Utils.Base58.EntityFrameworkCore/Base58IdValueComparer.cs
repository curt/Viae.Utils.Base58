using Microsoft.EntityFrameworkCore.ChangeTracking;
using Viae.Utils.Base58.Core;

namespace Viae.Utils.Base58.EntityFrameworkCore;

public class Base58IdValueComparer : ValueComparer<Base58Id>
{
    public Base58IdValueComparer()
        : base((l, r) => l.Equals(r), v => v.GetHashCode()) { }
}
