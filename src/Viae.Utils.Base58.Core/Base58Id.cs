// Copyright (c) Curt Gilman. Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Viae.Utils.Base58.Core;

/// <summary>
/// Represents a Base58-encoded identifier with automatic conversion between
/// string representation and internal ulong storage.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public readonly struct Base58Id : IEquatable<Base58Id>, IComparable<Base58Id>
{
    /// <summary>
    /// Gets the internal numeric value.
    /// </summary>
    public ulong Value { get; }

    /// <summary>
    /// Creates a Base58Id from a ulong value.
    /// </summary>
    public Base58Id(ulong value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a Base58Id from a Base58-encoded string.
    /// </summary>
    public Base58Id(string? base58String)
    {
        if (string.IsNullOrEmpty(base58String))
        {
            throw new ArgumentException(
                "Base58 string cannot be null or empty.",
                nameof(base58String)
            );
        }

        Value = Base58Converter.Decode(base58String);
    }

    /// <summary>
    /// Returns the Base58-encoded string representation.
    /// </summary>
    public override string ToString() => Base58Converter.Encode(Value);

    // Implicit conversions
    public static implicit operator Base58Id(ulong value) => new(value);

    public static implicit operator Base58Id(string base58String) => new(base58String);

    public static implicit operator string(Base58Id id) => id.ToString();

    public static implicit operator ulong(Base58Id id) => id.Value;

    // Equality
    public bool Equals(Base58Id other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is Base58Id other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Base58Id left, Base58Id right) => left.Equals(right);

    public static bool operator !=(Base58Id left, Base58Id right) => !left.Equals(right);

    // Comparison
    public int CompareTo(Base58Id other) => Value.CompareTo(other.Value);

    public static bool operator <(Base58Id left, Base58Id right) => left.Value < right.Value;

    public static bool operator <=(Base58Id left, Base58Id right) => left.Value <= right.Value;

    public static bool operator >(Base58Id left, Base58Id right) => left.Value > right.Value;

    public static bool operator >=(Base58Id left, Base58Id right) => left.Value >= right.Value;

    // Parsing
    public static Base58Id Parse(string s) => new(s);

    public static bool TryParse(string? s, out Base58Id result)
    {
        if (string.IsNullOrEmpty(s) || !Base58Converter.TryDecode(s, out ulong value))
        {
            result = default;
            return false;
        }
        result = new Base58Id(value);
        return true;
    }
}
