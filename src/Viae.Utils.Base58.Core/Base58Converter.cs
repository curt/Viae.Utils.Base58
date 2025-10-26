// Copyright (c) Curt Gilman. Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Viae.Utils.Base58.Core;

/// <summary>
/// Provides Base58 encoding and decoding utilities for unsigned 64-bit integers.
/// </summary>
public static class Base58Converter
{
    private const string Base58Alphabet =
        "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    private const int EncodedLength = 11;
    private const char PaddingChar = '1'; // First character in Base58 alphabet (represents 0)

    /// <summary>
    /// Encodes a ulong value to a Base58 string with zero-padding to 11 characters.
    /// </summary>
    /// <param name="value">The unsigned 64-bit integer to encode.</param>
    /// <returns>A Base58-encoded string representation of the value, padded to 11 characters.</returns>
    public static string Encode(ulong value)
    {
        if (value == 0)
        {
            return new string(PaddingChar, EncodedLength);
        }

        var result = string.Empty;

        while (value > 0)
        {
            var remainder = (int)(value % 58);
            value /= 58;
            result = Base58Alphabet[remainder] + result;
        }

        // Pad to 11 characters with '1' (which represents 0 in Base58)
        return result.PadLeft(EncodedLength, PaddingChar);
    }

    /// <summary>
    /// Decodes a Base58 string to a ulong value.
    /// Accepts strings of any length as long as the decoded value fits in a ulong.
    /// </summary>
    /// <param name="encoded">The Base58-encoded string to decode.</param>
    /// <returns>The decoded unsigned 64-bit integer value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when encoded is null.</exception>
    /// <exception cref="ArgumentException">Thrown when encoded contains invalid Base58 characters or is empty.</exception>
    /// <exception cref="OverflowException">Thrown when the decoded value exceeds ulong.MaxValue.</exception>
    public static ulong Decode(string? encoded)
    {
        if (encoded == null)
        {
            throw new ArgumentNullException(nameof(encoded));
        }

        if (encoded.Length == 0)
        {
            throw new ArgumentException("Encoded string cannot be empty.", nameof(encoded));
        }

        ulong result = 0;

        foreach (var c in encoded)
        {
            var digit = Base58Alphabet.IndexOf(c);
            if (digit < 0)
            {
                throw new ArgumentException($"Invalid Base58 character: '{c}'", nameof(encoded));
            }
            checked
            {
                result = (result * 58) + (ulong)digit;
            }
        }

        return result;
    }

    /// <summary>
    /// Tries to decode a Base58 string to a ulong value.
    /// Accepts strings of any length as long as the decoded value fits in a ulong.
    /// </summary>
    /// <param name="encoded">The Base58-encoded string to decode.</param>
    /// <param name="value">When this method returns, contains the decoded value if successful, or 0 if failed.</param>
    /// <returns>true if the decode was successful; otherwise, false.</returns>
    public static bool TryDecode(string? encoded, out ulong value)
    {
        value = 0;

        if (string.IsNullOrEmpty(encoded))
        {
            return false;
        }

        try
        {
            foreach (var c in encoded!)
            {
                var digit = Base58Alphabet.IndexOf(c);
                if (digit < 0)
                {
                    value = 0;
                    return false;
                }

                // Check for overflow before multiplication
                if (value > ulong.MaxValue / 58)
                {
                    value = 0;
                    return false;
                }

                var newValue = (value * 58) + (ulong)digit;

                // Additional overflow check after addition
                if (newValue < value)
                {
                    value = 0;
                    return false;
                }

                value = newValue;
            }

            return true;
        }
        catch (OverflowException)
        {
            value = 0;
            return false;
        }
    }

    /// <summary>
    /// Validates whether a string is a valid Base58-encoded value.
    /// </summary>
    /// <param name="encoded">The string to validate.</param>
    /// <returns>true if the string is valid Base58; otherwise, false.</returns>
    public static bool IsValid(string? encoded)
    {
        if (string.IsNullOrEmpty(encoded))
        {
            return false;
        }

        foreach (var c in encoded!)
        {
            if (Base58Alphabet.IndexOf(c) < 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the maximum value that can be represented in Base58 with the standard 11-character encoding.
    /// </summary>
    public static ulong MaxEncodedValue => ulong.MaxValue; // jpXCZedGfVQ in Base58
}
