// Copyright (c) Curt Gilman. Licensed under the MIT License. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viae.Utils.Base58.Core;
using Viae.Utils.Base58.EntityFrameworkCore;

namespace Viae.Utils.Base58.EntityFrameworkCore.Tests;

[TestClass]
public class Base58IdValueComparerTests
{
    [TestClass]
    public class Construction
    {
        [TestMethod]
        public void Should_Create_Comparer_Instance()
        {
            // Act
            var comparer = new Base58IdValueComparer();

            // Assert
            comparer.Should().NotBeNull();
        }
    }

    [TestClass]
    public class Equals_Method
    {
        [TestMethod]
        public void Should_Return_True_For_Equal_Values()
        {
            // Arrange
            var comparer = new Base58IdValueComparer();
            var id1 = new Base58Id(12345);
            var id2 = new Base58Id(12345);

            // Act
            var result = comparer.Equals(id1, id2);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Should_Return_False_For_Different_Values()
        {
            // Arrange
            var comparer = new Base58IdValueComparer();
            var id1 = new Base58Id(12345);
            var id2 = new Base58Id(54321);

            // Act
            var result = comparer.Equals(id1, id2);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Should_Return_True_For_Zero_Values()
        {
            // Arrange
            var comparer = new Base58IdValueComparer();
            var id1 = new Base58Id(0);
            var id2 = new Base58Id(0);

            // Act
            var result = comparer.Equals(id1, id2);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Should_Return_True_For_MaxValue()
        {
            // Arrange
            var comparer = new Base58IdValueComparer();
            var id1 = new Base58Id(ulong.MaxValue);
            var id2 = new Base58Id(ulong.MaxValue);

            // Act
            var result = comparer.Equals(id1, id2);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Should_Return_True_For_Default_Values()
        {
            // Arrange
            var comparer = new Base58IdValueComparer();
            var id1 = default(Base58Id);
            var id2 = default(Base58Id);

            // Act
            var result = comparer.Equals(id1, id2);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        [DataRow(0ul, 0ul, true)]
        [DataRow(1ul, 1ul, true)]
        [DataRow(12345ul, 12345ul, true)]
        [DataRow(0ul, 1ul, false)]
        [DataRow(12345ul, 54321ul, false)]
        [DataRow(1ul, ulong.MaxValue, false)]
        public void Should_Compare_Known_Values(ulong value1, ulong value2, bool expected)
        {
            // Arrange
            var comparer = new Base58IdValueComparer();
            var id1 = new Base58Id(value1);
            var id2 = new Base58Id(value2);

            // Act
            var result = comparer.Equals(id1, id2);

            // Assert
            result.Should().Be(expected);
        }
    }

    [TestClass]
    public class GetHashCode_Method
    {
        [TestMethod]
        public void Should_Return_Same_HashCode_For_Equal_Values()
        {
            // Arrange
            var comparer = new Base58IdValueComparer();
            var id1 = new Base58Id(12345);
            var id2 = new Base58Id(12345);

            // Act
            var hash1 = comparer.GetHashCode(id1);
            var hash2 = comparer.GetHashCode(id2);

            // Assert
            hash1.Should().Be(hash2);
        }

        [TestMethod]
        public void Should_Return_Different_HashCode_For_Different_Values()
        {
            // Arrange
            var comparer = new Base58IdValueComparer();
            var id1 = new Base58Id(12345);
            var id2 = new Base58Id(54321);

            // Act
            var hash1 = comparer.GetHashCode(id1);
            var hash2 = comparer.GetHashCode(id2);

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [TestMethod]
        public void Should_Return_Consistent_HashCode()
        {
            // Arrange
            var comparer = new Base58IdValueComparer();
            var id = new Base58Id(12345);

            // Act
            var hash1 = comparer.GetHashCode(id);
            var hash2 = comparer.GetHashCode(id);

            // Assert
            hash1.Should().Be(hash2);
        }

        [TestMethod]
        public void Should_Return_Same_HashCode_For_Default_Values()
        {
            // Arrange
            var comparer = new Base58IdValueComparer();
            var id1 = default(Base58Id);
            var id2 = default(Base58Id);

            // Act
            var hash1 = comparer.GetHashCode(id1);
            var hash2 = comparer.GetHashCode(id2);

            // Assert
            hash1.Should().Be(hash2);
        }

        [TestMethod]
        [DataRow(0ul)]
        [DataRow(1ul)]
        [DataRow(12345ul)]
        [DataRow(987654321ul)]
        [DataRow(ulong.MaxValue)]
        public void Should_Return_Consistent_HashCode_For_Known_Values(ulong value)
        {
            // Arrange
            var comparer = new Base58IdValueComparer();
            var id = new Base58Id(value);

            // Act
            var hash1 = comparer.GetHashCode(id);
            var hash2 = comparer.GetHashCode(id);

            // Assert
            hash1.Should().Be(hash2);
        }
    }

    [TestClass]
    public class Snapshot
    {
        [TestMethod]
        public void Should_Create_Snapshot_Of_Value()
        {
            // Arrange
            var comparer = new Base58IdValueComparer();
            var id = new Base58Id(12345);

            // Act
            var snapshot = comparer.Snapshot(id);

            // Assert
            snapshot.Value.Should().Be(12345);
        }

        [TestMethod]
        public void Should_Create_Independent_Snapshot()
        {
            // Arrange
            var comparer = new Base58IdValueComparer();
            var id = new Base58Id(12345);

            // Act
            var snapshot = comparer.Snapshot(id);

            // Assert - Since Base58Id is a struct, the snapshot should be independent
            snapshot.Should().Be(id);
            snapshot.Equals(id).Should().BeTrue();
        }

        [TestMethod]
        [DataRow(0ul)]
        [DataRow(1ul)]
        [DataRow(12345ul)]
        [DataRow(ulong.MaxValue)]
        public void Should_Create_Snapshot_For_Known_Values(ulong value)
        {
            // Arrange
            var comparer = new Base58IdValueComparer();
            var id = new Base58Id(value);

            // Act
            var snapshot = comparer.Snapshot(id);

            // Assert
            snapshot.Value.Should().Be(value);
        }
    }
}
