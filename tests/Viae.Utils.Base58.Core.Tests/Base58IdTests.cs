// Copyright (c) Curt Gilman. Licensed under the MIT License. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Viae.Utils.Base58.Core.Tests;

[TestClass]
public class Base58IdTests
{
    [TestClass]
    public class Construction
    {
        [TestMethod]
        public void Should_Create_From_ULong()
        {
            // Arrange
            ulong value = 12345;

            // Act
            var id = new Base58Id(value);

            // Assert
            id.Value.Should().Be(value);
        }

        [TestMethod]
        public void Should_Create_From_Valid_String()
        {
            // Arrange
            string encoded = "111111114fr";

            // Act
            var id = new Base58Id(encoded);

            // Assert
            id.Value.Should().Be(12345);
        }

        [TestMethod]
        public void Should_Throw_ArgumentException_For_Null_String()
        {
            // Arrange
            string? encoded = null;

            // Act
#pragma warning disable CA1806 // Do not ignore method results
            Action act = () => new Base58Id(encoded);
#pragma warning restore CA1806 // Do not ignore method results

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*cannot be null or empty*")
                .WithParameterName("base58String");
        }

        [TestMethod]
        public void Should_Throw_ArgumentException_For_Empty_String()
        {
            // Arrange
            string encoded = "";

            // Act
#pragma warning disable CA1806 // Do not ignore method results
            Action act = () => new Base58Id(encoded);
#pragma warning restore CA1806 // Do not ignore method results

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*cannot be null or empty*")
                .WithParameterName("base58String");
        }

        [TestMethod]
        public void Should_Throw_ArgumentException_For_Invalid_Base58_String()
        {
            // Arrange
            string encoded = "invalid0chars";

            // Act
#pragma warning disable CA1806 // Do not ignore method results
            Action act = () => new Base58Id(encoded);
#pragma warning restore CA1806 // Do not ignore method results

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }

    [TestClass]
    public class ImplicitConversions
    {
        [TestMethod]
        public void Should_Implicitly_Convert_From_ULong()
        {
            // Arrange
            ulong value = 12345;

            // Act
            Base58Id id = value;

            // Assert
            id.Value.Should().Be(value);
        }

        [TestMethod]
        public void Should_Implicitly_Convert_To_ULong()
        {
            // Arrange
            var id = new Base58Id(12345);

            // Act
            ulong value = id;

            // Assert
            value.Should().Be(12345);
        }

        [TestMethod]
        public void Should_Implicitly_Convert_From_String()
        {
            // Arrange
            string encoded = "111111114fr";

            // Act
            Base58Id id = encoded;

            // Assert
            id.Value.Should().Be(12345);
        }

        [TestMethod]
        public void Should_Implicitly_Convert_To_String()
        {
            // Arrange
            var id = new Base58Id(12345);

            // Act
            string encoded = id;

            // Assert
            encoded.Should().Be("111111114fr");
        }
    }

    [TestClass]
    public class ToString_Method
    {
        [TestMethod]
        public void Should_Return_Encoded_String()
        {
            // Arrange
            var id = new Base58Id(12345);

            // Act
            var result = id.ToString();

            // Assert
            result.Should().Be("111111114fr");
        }

        [TestMethod]
        public void Should_Return_Padded_String_For_Zero()
        {
            // Arrange
            var id = new Base58Id(0);

            // Act
            var result = id.ToString();

            // Assert
            result.Should().Be("11111111111");
        }
    }

    [TestClass]
    public class Equality
    {
        [TestMethod]
        public void Should_Be_Equal_When_Values_Match()
        {
            // Arrange
            var id1 = new Base58Id(12345);
            var id2 = new Base58Id(12345);

            // Assert
            id1.Should().Be(id2);
            id1.Equals(id2).Should().BeTrue();
            (id1 == id2).Should().BeTrue();
            (id1 != id2).Should().BeFalse();
        }

        [TestMethod]
        public void Should_Not_Be_Equal_When_Values_Differ()
        {
            // Arrange
            var id1 = new Base58Id(12345);
            var id2 = new Base58Id(54321);

            // Assert
            id1.Should().NotBe(id2);
            id1.Equals(id2).Should().BeFalse();
            (id1 == id2).Should().BeFalse();
            (id1 != id2).Should().BeTrue();
        }

        [TestMethod]
        public void Should_Be_Equal_When_Created_From_String_And_ULong()
        {
            // Arrange
            var id1 = new Base58Id(12345);
            var id2 = new Base58Id("111111114fr");

            // Assert
            id1.Should().Be(id2);
        }

        [TestMethod]
        public void Should_Have_Same_HashCode_For_Equal_Values()
        {
            // Arrange
            var id1 = new Base58Id(12345);
            var id2 = new Base58Id(12345);

            // Assert
            id1.GetHashCode().Should().Be(id2.GetHashCode());
        }

        [TestMethod]
        public void Should_Not_Equal_Null_Object()
        {
            // Arrange
            var id = new Base58Id(12345);
            object? nullObject = null;

            // Act
            var result = id.Equals(nullObject);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Should_Not_Equal_Different_Type()
        {
            // Arrange
            var id = new Base58Id(12345);
            object other = 12345; // Using an int boxed as object - a type that cannot be converted to Base58Id

            // Act
            var result = id.Equals(other);

            // Assert
            result.Should().BeFalse();
        }
    }

    [TestClass]
    public class Comparison
    {
        [TestMethod]
        public void Should_Compare_Correctly()
        {
            // Arrange
            var smaller = new Base58Id(100);
            var larger = new Base58Id(200);

            // Assert
            smaller.CompareTo(larger).Should().BeLessThan(0);
            larger.CompareTo(smaller).Should().BeGreaterThan(0);
            smaller.CompareTo(smaller).Should().Be(0);
        }

        [TestMethod]
        public void Should_Support_Less_Than_Operator()
        {
            // Arrange
            var smaller = new Base58Id(100);
            var larger = new Base58Id(200);

            // Assert
            (smaller < larger)
                .Should()
                .BeTrue();
            (larger < smaller).Should().BeFalse();
#pragma warning disable CS1718 // Comparison made to same variable
            (smaller < smaller).Should().BeFalse();
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [TestMethod]
        public void Should_Support_Less_Than_Or_Equal_Operator()
        {
            // Arrange
            var smaller = new Base58Id(100);
            var larger = new Base58Id(200);
            var equal = new Base58Id(100);

            // Assert
            (smaller <= larger)
                .Should()
                .BeTrue();
            (smaller <= equal).Should().BeTrue();
            (larger <= smaller).Should().BeFalse();
        }

        [TestMethod]
        public void Should_Support_Greater_Than_Operator()
        {
            // Arrange
            var smaller = new Base58Id(100);
            var larger = new Base58Id(200);

            // Assert
            (larger > smaller)
                .Should()
                .BeTrue();
            (smaller > larger).Should().BeFalse();
#pragma warning disable CS1718 // Comparison made to same variable
            (smaller > smaller).Should().BeFalse();
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [TestMethod]
        public void Should_Support_Greater_Than_Or_Equal_Operator()
        {
            // Arrange
            var smaller = new Base58Id(100);
            var larger = new Base58Id(200);
            var equal = new Base58Id(200);

            // Assert
            (larger >= smaller)
                .Should()
                .BeTrue();
            (larger >= equal).Should().BeTrue();
            (smaller >= larger).Should().BeFalse();
        }
    }

    [TestClass]
    public class Parsing
    {
        [TestMethod]
        public void Parse_Should_Create_Valid_Id()
        {
            // Arrange
            string encoded = "111111114fr";

            // Act
            var id = Base58Id.Parse(encoded);

            // Assert
            id.Value.Should().Be(12345);
        }

        [TestMethod]
        public void Parse_Should_Throw_For_Invalid_String()
        {
            // Arrange
            string encoded = "invalid0";

            // Act
            Action act = () => Base58Id.Parse(encoded);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void TryParse_Should_Return_True_For_Valid_String()
        {
            // Arrange
            string encoded = "111111114fr";

            // Act
            var success = Base58Id.TryParse(encoded, out var result);

            // Assert
            success.Should().BeTrue();
            result.Value.Should().Be(12345);
        }

        [TestMethod]
        public void TryParse_Should_Return_False_For_Null()
        {
            // Arrange
            string? encoded = null;

            // Act
            var success = Base58Id.TryParse(encoded, out var result);

            // Assert
            success.Should().BeFalse();
            result.Should().Be(default);
        }

        [TestMethod]
        public void TryParse_Should_Return_False_For_Empty()
        {
            // Arrange
            string encoded = "";

            // Act
            var success = Base58Id.TryParse(encoded, out var result);

            // Assert
            success.Should().BeFalse();
            result.Should().Be(default);
        }

        [TestMethod]
        public void TryParse_Should_Return_False_For_Invalid_String()
        {
            // Arrange
            string encoded = "invalid0";

            // Act
            var success = Base58Id.TryParse(encoded, out var result);

            // Assert
            success.Should().BeFalse();
            result.Should().Be(default);
        }
    }

    [TestClass]
    public class RoundTrip
    {
        [TestMethod]
        public void Should_Round_Trip_Through_String()
        {
            // Arrange
            var original = new Base58Id(12345);

            // Act
            string encoded = original;
            Base58Id decoded = encoded;

            // Assert
            decoded.Should().Be(original);
        }

        [TestMethod]
        public void Should_Round_Trip_Through_ULong()
        {
            // Arrange
            var original = new Base58Id(12345);

            // Act
            ulong value = original;
            Base58Id reconstructed = value;

            // Assert
            reconstructed.Should().Be(original);
        }
    }
}
