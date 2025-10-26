using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Viae.Utils.Base58.Core;

namespace Viae.Utils.Base58.EntityFrameworkCore;

/// <summary>
/// Generates sequential Base58Id values.
/// </summary>
public class Base58IdValueGenerator : ValueGenerator<Base58Id>
{
    private long _counter = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public override Base58Id Next(EntityEntry entry)
    {
        var value = (ulong)Interlocked.Increment(ref _counter);
        return new Base58Id(value);
    }

    public override bool GeneratesTemporaryValues => false;
}
