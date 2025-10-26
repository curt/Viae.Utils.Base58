// Copyright (c) Curt Gilman. Licensed under the MIT License. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Viae.Utils.Base58.Core.Tests;

[TestClass]
public class Base58ConverterTests
{
    [TestClass]
    public class Encode
    {
        [TestMethod]
        public void Should_Encode_Zero_As_Eleven_Ones()
        {
            // Arrange
            ulong value = 0;

            // Act
            var result = Base58Converter.Encode(value);

            // Assert
            result.Should().Be("11111111111");
            result.Should().HaveLength(11);
        }

        [TestMethod]
        public void Should_Encode_One()
        {
            // Arrange
            ulong value = 1;

            // Act
            var result = Base58Converter.Encode(value);

            // Assert
            result.Should().Be("11111111112");
            result.Should().HaveLength(11);
        }

        [TestMethod]
        public void Should_Encode_FiftyEight()
        {
            // Arrange
            ulong value = 58;

            // Act
            var result = Base58Converter.Encode(value);

            // Assert
            result.Should().Be("11111111121");
            result.Should().HaveLength(11);
        }

        [TestMethod]
        public void Should_Encode_MaxValue()
        {
            // Arrange
            ulong value = ulong.MaxValue;

            // Act
            var result = Base58Converter.Encode(value);

            // Assert
            result.Should().Be("jpXCZedGfVQ");
            result.Should().HaveLength(11);
        }

        [TestMethod]
        [DataRow(0ul, "11111111111")]
        [DataRow(1ul, "11111111112")]
        [DataRow(57ul, "1111111111z")]
        [DataRow(58ul, "11111111121")]
        [DataRow(3364ul, "11111111211")] // 58^2
        [DataRow(195112ul, "11111112111")] // 58^3
        [DataRow(12345ul, "111111114fr")]
        [DataRow(987654321ul, "111112WGzDn")]
        [DataRow(ulong.MaxValue, "jpXCZedGfVQ")]
        public void Should_Encode_Known_Values(ulong input, string expected)
        {
            // Act
            var result = Base58Converter.Encode(input);

            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public void Should_Always_Return_Eleven_Characters()
        {
            // Arrange
            var testValues = new[]
            {
                0ul,
                1ul,
                100ul,
                1000ul,
                1000000ul,
                1000000000ul,
                ulong.MaxValue / 2,
                ulong.MaxValue,
            };

            // Act & Assert
            foreach (var value in testValues)
            {
                var result = Base58Converter.Encode(value);
                result.Should().HaveLength(11, $"encoding {value} should be 11 characters");
            }
        }

        [TestMethod]
        public void Should_Use_Only_Valid_Base58_Characters()
        {
            // Arrange
            const string validChars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
            var testValues = new[] { 0ul, 1ul, 12345ul, ulong.MaxValue };

            // Act & Assert
            foreach (var value in testValues)
            {
                var result = Base58Converter.Encode(value);
                result.Should().NotBeNullOrEmpty();
                foreach (var c in result)
                {
                    validChars
                        .Should()
                        .Contain(c.ToString(), $"'{c}' should be a valid Base58 character");
                }
            }
        }
    }

    [TestClass]
    public class Decode
    {
        [TestMethod]
        public void Should_Decode_Eleven_Ones_As_Zero()
        {
            // Arrange
            string encoded = "11111111111";

            // Act
            var result = Base58Converter.Decode(encoded);

            // Assert
            result.Should().Be(0);
        }

        [TestMethod]
        public void Should_Decode_Single_Character()
        {
            // Arrange
            string encoded = "2";

            // Act
            var result = Base58Converter.Decode(encoded);

            // Assert
            result.Should().Be(1);
        }

        [TestMethod]
        public void Should_Decode_With_Leading_Padding()
        {
            // Arrange
            string encoded = "11111111112";

            // Act
            var result = Base58Converter.Decode(encoded);

            // Assert
            result.Should().Be(1);
        }

        [TestMethod]
        public void Should_Decode_Without_Leading_Padding()
        {
            // Arrange
            string encodedWithPadding = "11111111112";
            string encodedWithoutPadding = "2";

            // Act
            var resultWithPadding = Base58Converter.Decode(encodedWithPadding);
            var resultWithoutPadding = Base58Converter.Decode(encodedWithoutPadding);

            // Assert
            resultWithPadding.Should().Be(resultWithoutPadding);
        }

        [TestMethod]
        [DataRow("11111111111", 0ul)]
        [DataRow("11111111112", 1ul)]
        [DataRow("1111111111z", 57ul)]
        [DataRow("11111111121", 58ul)]
        [DataRow("11111111211", 3364ul)]
        [DataRow("11111112111", 195112ul)]
        [DataRow("111111114fr", 12345ul)]
        [DataRow("111112WGzDn", 987654321ul)]
        [DataRow("jpXCZedGfVQ", ulong.MaxValue)]
        public void Should_Decode_Known_Values(string input, ulong expected)
        {
            // Act
            var result = Base58Converter.Decode(input);

            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public void Should_Throw_ArgumentNullException_For_Null()
        {
            // Arrange
            string? encoded = null;

            // Act
            Action act = () => Base58Converter.Decode(encoded);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("encoded");
        }

        [TestMethod]
        public void Should_Throw_ArgumentException_For_Empty_String()
        {
            // Arrange
            string encoded = "";

            // Act
            Action act = () => Base58Converter.Decode(encoded);

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*cannot be empty*")
                .WithParameterName("encoded");
        }

        [TestMethod]
        [DataRow("0")] // Contains '0' which is not in Base58 alphabet
        [DataRow("O")] // Contains 'O' which is not in Base58 alphabet
        [DataRow("I")] // Contains 'I' which is not in Base58 alphabet
        [DataRow("l")] // Contains lowercase 'l' which is not in Base58 alphabet
        [DataRow("11111111110")] // Contains '0'
        [DataRow("test@123")] // Contains invalid characters
        public void Should_Throw_ArgumentException_For_Invalid_Characters(string invalidEncoded)
        {
            // Act
            Action act = () => Base58Converter.Decode(invalidEncoded);

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*Invalid Base58 character*")
                .WithParameterName("encoded");
        }

        [TestMethod]
        public void Should_Throw_OverflowException_For_Value_Exceeding_ULong_Max()
        {
            // Arrange - A string that would decode to a value > ulong.MaxValue
            string encoded = "jpXCZedGfVR"; // One more than max

            // Act
            Action act = () => Base58Converter.Decode(encoded);

            // Assert
            act.Should().Throw<OverflowException>();
        }
    }

    [TestClass]
    public class TryDecode
    {
        [TestMethod]
        public void Should_Return_True_And_Decode_Valid_String()
        {
            // Arrange
            string encoded = "11111111112";

            // Act
            var success = Base58Converter.TryDecode(encoded, out var result);

            // Assert
            success.Should().BeTrue();
            result.Should().Be(1);
        }

        [TestMethod]
        public void Should_Return_False_For_Null()
        {
            // Arrange
            string? encoded = null;

            // Act
            var success = Base58Converter.TryDecode(encoded, out var result);

            // Assert
            success.Should().BeFalse();
            result.Should().Be(0);
        }

        [TestMethod]
        public void Should_Return_False_For_Empty_String()
        {
            // Arrange
            string encoded = "";

            // Act
            var success = Base58Converter.TryDecode(encoded, out var result);

            // Assert
            success.Should().BeFalse();
            result.Should().Be(0);
        }

        [TestMethod]
        [DataRow("0")]
        [DataRow("O")]
        [DataRow("I")]
        [DataRow("l")]
        [DataRow("test@123")]
        public void Should_Return_False_For_Invalid_Characters(string invalidEncoded)
        {
            // Act
            var success = Base58Converter.TryDecode(invalidEncoded, out var result);

            // Assert
            success.Should().BeFalse();
            result.Should().Be(0);
        }

        [TestMethod]
        public void Should_Return_False_For_Overflow()
        {
            // Arrange - A string that would overflow ulong
            string encoded = "zzzzzzzzzzzz"; // 12 z's - definitely overflows

            // Act
            var success = Base58Converter.TryDecode(encoded, out var result);

            // Assert
            success.Should().BeFalse();
            result.Should().Be(0);
        }

        [TestMethod]
        [DataRow("11111111111", 0ul)]
        [DataRow("11111111112", 1ul)]
        [DataRow("jpXCZedGfVQ", ulong.MaxValue)]
        public void Should_Decode_Known_Valid_Values(string input, ulong expected)
        {
            // Act
            var success = Base58Converter.TryDecode(input, out var result);

            // Assert
            success.Should().BeTrue();
            result.Should().Be(expected);
        }
    }

    [TestClass]
    public class IsValid
    {
        [TestMethod]
        public void Should_Return_True_For_Valid_Base58_String()
        {
            // Arrange
            string encoded = "11111111112";

            // Act
            var result = Base58Converter.IsValid(encoded);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        [DataRow("1")]
        [DataRow("2")]
        [DataRow("11111111111")]
        [DataRow("jpXCZedGfVQ")]
        [DataRow("ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz123456789")]
        public void Should_Return_True_For_Valid_Strings(string validEncoded)
        {
            // Act
            var result = Base58Converter.IsValid(validEncoded);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Should_Return_False_For_Null()
        {
            // Arrange
            string? encoded = null;

            // Act
            var result = Base58Converter.IsValid(encoded);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Should_Return_False_For_Empty_String()
        {
            // Arrange
            string encoded = "";

            // Act
            var result = Base58Converter.IsValid(encoded);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        [DataRow("0")]
        [DataRow("O")]
        [DataRow("I")]
        [DataRow("l")]
        [DataRow("test@123")]
        [DataRow("11111111110")] // Contains '0'
        [DataRow("hello world")] // Contains space
        public void Should_Return_False_For_Invalid_Characters(string invalidEncoded)
        {
            // Act
            var result = Base58Converter.IsValid(invalidEncoded);

            // Assert
            result.Should().BeFalse();
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
            // Act
            var encoded = Base58Converter.Encode(original);
            var decoded = Base58Converter.Decode(encoded);

            // Assert
            decoded.Should().Be(original);
        }

        [TestMethod]
        public void Should_Round_Trip_All_Powers_Of_58()
        {
            // Arrange
            ulong power = 1;

            for (int i = 0; i < 11; i++) // 58^11 would overflow
            {
                // Act
                var encoded = Base58Converter.Encode(power);
                var decoded = Base58Converter.Decode(encoded);

                // Assert
                decoded.Should().Be(power, $"58^{i} should round-trip correctly");

                // Prepare next power
                if (power <= ulong.MaxValue / 58)
                {
                    power *= 58;
                }
                else
                {
                    break;
                }
            }
        }

        [TestMethod]
        public void Should_Round_Trip_Random_Values()
        {
            // Arrange
            var random = new Random(42); // Fixed seed for reproducibility
            var testCount = 1000;

            // Act & Assert
            for (int i = 0; i < testCount; i++)
            {
                var original = (ulong)random.NextInt64(0, long.MaxValue);
                var encoded = Base58Converter.Encode(original);
                var decoded = Base58Converter.Decode(encoded);

                decoded.Should().Be(original, $"iteration {i} should round-trip correctly");
            }
        }
    }

    [TestClass]
    public class MaxEncodedValue
    {
        [TestMethod]
        public void Should_Return_ULong_MaxValue()
        {
            // Act
            var result = Base58Converter.MaxEncodedValue;

            // Assert
            result.Should().Be(ulong.MaxValue);
        }
    }
}
