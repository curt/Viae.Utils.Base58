using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Viae.Utils.Base58.Core;

namespace Viae.Utils.Base58.EntityFrameworkCore;

public class Base58IdValueConverter : ValueConverter<Base58Id, string>
{
    public Base58IdValueConverter()
        : base(
            id => id.ToString(), // To database
            str => new Base58Id(str)
        ) // From database
    { }
}
