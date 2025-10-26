// Copyright (c) Curt Gilman. Licensed under the MIT License. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viae.Utils.Base58.Core;
using Viae.Utils.Base58.EntityFrameworkCore;

namespace Viae.Utils.Base58.EntityFrameworkCore.Tests;

[TestClass]
public class Base58IdValueConverterTests
{
    [TestClass]
    public class Construction
    {
        [TestMethod]
        public void Should_Create_Converter_Instance()
        {
            // Act
            var converter = new Base58IdValueConverter();

            // Assert
            converter.Should().NotBeNull();
        }
    }

    [TestClass]
    public class ConvertToProvider
    {
        [TestMethod]
        public void Should_Convert_Base58Id_To_String()
        {
            // Arrange
            var converter = new Base58IdValueConverter();
            var id = new Base58Id(12345);

            // Act
            var result = converter.ConvertToProvider(id);

            // Assert
            result.Should().Be("111111114fr");
        }

        [TestMethod]
        public void Should_Convert_Zero_To_Padded_String()
        {
            // Arrange
            var converter = new Base58IdValueConverter();
            var id = new Base58Id(0);

            // Act
            var result = converter.ConvertToProvider(id);

            // Assert
            result.Should().Be("11111111111");
        }

        [TestMethod]
        public void Should_Convert_MaxValue_To_String()
        {
            // Arrange
            var converter = new Base58IdValueConverter();
            var id = new Base58Id(ulong.MaxValue);

            // Act
            var result = converter.ConvertToProvider(id);

            // Assert
            result.Should().Be("jpXCZedGfVQ");
        }

        [TestMethod]
        [DataRow(0ul, "11111111111")]
        [DataRow(1ul, "11111111112")]
        [DataRow(12345ul, "111111114fr")]
        [DataRow(987654321ul, "111112WGzDn")]
        [DataRow(ulong.MaxValue, "jpXCZedGfVQ")]
        public void Should_Convert_Known_Values_To_String(ulong value, string expected)
        {
            // Arrange
            var converter = new Base58IdValueConverter();
            var id = new Base58Id(value);

            // Act
            var result = converter.ConvertToProvider(id);

            // Assert
            result.Should().Be(expected);
        }
    }

    [TestClass]
    public class ConvertFromProvider
    {
        [TestMethod]
        public void Should_Convert_String_To_Base58Id()
        {
            // Arrange
            var converter = new Base58IdValueConverter();
            string encoded = "111111114fr";

            // Act
            var result = (Base58Id)converter.ConvertFromProvider(encoded)!;

            // Assert
            result.Value.Should().Be(12345);
        }

        [TestMethod]
        public void Should_Convert_Padded_String_To_Zero()
        {
            // Arrange
            var converter = new Base58IdValueConverter();
            string encoded = "11111111111";

            // Act
            var result = (Base58Id)converter.ConvertFromProvider(encoded)!;

            // Assert
            result.Value.Should().Be(0);
        }

        [TestMethod]
        public void Should_Convert_MaxValue_String_To_Base58Id()
        {
            // Arrange
            var converter = new Base58IdValueConverter();
            string encoded = "jpXCZedGfVQ";

            // Act
            var result = (Base58Id)converter.ConvertFromProvider(encoded)!;

            // Assert
            result.Value.Should().Be(ulong.MaxValue);
        }

        [TestMethod]
        [DataRow("11111111111", 0ul)]
        [DataRow("11111111112", 1ul)]
        [DataRow("111111114fr", 12345ul)]
        [DataRow("111112WGzDn", 987654321ul)]
        [DataRow("jpXCZedGfVQ", ulong.MaxValue)]
        public void Should_Convert_Known_Strings_To_Values(string encoded, ulong expected)
        {
            // Arrange
            var converter = new Base58IdValueConverter();

            // Act
            var result = (Base58Id)converter.ConvertFromProvider(encoded)!;

            // Assert
            result.Value.Should().Be(expected);
        }

        [TestMethod]
        public void Should_Convert_String_Without_Padding()
        {
            // Arrange
            var converter = new Base58IdValueConverter();
            string encoded = "2"; // Without padding

            // Act
            var result = (Base58Id)converter.ConvertFromProvider(encoded)!;

            // Assert
            result.Value.Should().Be(1);
        }
    }

    [TestClass]
    public class RoundTrip
    {
        [TestMethod]
        [DataRow(0ul)]
        [DataRow(1ul)]
        [DataRow(58ul)]
        [DataRow(12345ul)]
        [DataRow(987654321ul)]
        [DataRow(ulong.MaxValue / 2)]
        [DataRow(ulong.MaxValue)]
        public void Should_Round_Trip_Values(ulong original)
        {
            // Arrange
            var converter = new Base58IdValueConverter();
            var id = new Base58Id(original);

            // Act
            var encoded = converter.ConvertToProvider(id);
            var decoded = (Base58Id)converter.ConvertFromProvider(encoded!)!;

            // Assert
            decoded.Value.Should().Be(original);
        }

        [TestMethod]
        public void Should_Round_Trip_Random_Values()
        {
            // Arrange
            var converter = new Base58IdValueConverter();
            var random = new Random(42);

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                var original = (ulong)random.NextInt64(0, long.MaxValue);
                var id = new Base58Id(original);
                var encoded = converter.ConvertToProvider(id);
                var decoded = (Base58Id)converter.ConvertFromProvider(encoded!)!;

                decoded.Value.Should().Be(original, $"iteration {i} should round-trip correctly");
            }
        }
    }
}
