// Copyright (c) Curt Gilman. Licensed under the MIT License. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viae.Utils.Base58.Core;
using Viae.Utils.Base58.EntityFrameworkCore;

namespace Viae.Utils.Base58.EntityFrameworkCore.Tests;

[TestClass]
public class Base58IdValueGeneratorTests
{
    [TestClass]
    public class Construction
    {
        [TestMethod]
        public void Should_Create_Generator_Instance()
        {
            // Act
            var generator = new Base58IdValueGenerator();

            // Assert
            generator.Should().NotBeNull();
        }
    }

    [TestClass]
    public class GeneratesTemporaryValues_Property
    {
        [TestMethod]
        public void Should_Return_False()
        {
            // Arrange
            var generator = new Base58IdValueGenerator();

            // Act
            var result = generator.GeneratesTemporaryValues;

            // Assert
            result.Should().BeFalse();
        }
    }

    [TestClass]
    public class Next_Method
    {
        [TestMethod]
        public void Should_Generate_Base58Id()
        {
            // Arrange
            var generator = new Base58IdValueGenerator();
            var entry = CreateMockEntityEntry();

            // Act
            var result = generator.Next(entry);

            // Assert
            result.Should().NotBe(default(Base58Id));
            result.Value.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void Should_Generate_Sequential_Values()
        {
            // Arrange
            var generator = new Base58IdValueGenerator();
            var entry = CreateMockEntityEntry();

            // Act
            var id1 = generator.Next(entry);
            var id2 = generator.Next(entry);
            var id3 = generator.Next(entry);

            // Assert
            id2.Value.Should().BeGreaterThan(id1.Value);
            id3.Value.Should().BeGreaterThan(id2.Value);
            (id2.Value - id1.Value).Should().Be(1);
            (id3.Value - id2.Value).Should().Be(1);
        }

        [TestMethod]
        public void Should_Generate_Unique_Values()
        {
            // Arrange
            var generator = new Base58IdValueGenerator();
            var entry = CreateMockEntityEntry();
            var generatedIds = new HashSet<ulong>();

            // Act
            for (int i = 0; i < 1000; i++)
            {
                var id = generator.Next(entry);
                generatedIds.Add(id.Value);
            }

            // Assert
            generatedIds.Count.Should().Be(1000, "all generated IDs should be unique");
        }

        [TestMethod]
        public void Should_Generate_Values_Based_On_Timestamp()
        {
            // Arrange
            var generator = new Base58IdValueGenerator();
            var entry = CreateMockEntityEntry();
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Act
            var id = generator.Next(entry);

            // Assert
            // The generated value should be close to the current timestamp
            // (within a reasonable range since it's incremented from the initial timestamp)
            id.Value.Should().BeGreaterOrEqualTo((ulong)now);
            id.Value.Should().BeLessThan((ulong)(now + 1000)); // Within 1 second
        }

        [TestMethod]
        public void Should_Generate_Thread_Safe_Values()
        {
            // Arrange
            var generator = new Base58IdValueGenerator();
            var entry = CreateMockEntityEntry();
            var generatedIds = new System.Collections.Concurrent.ConcurrentBag<ulong>();
            var tasks = new List<Task>();

            // Act - Generate IDs from multiple threads
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < 100; j++)
                    {
                        var id = generator.Next(entry);
                        generatedIds.Add(id.Value);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert
            generatedIds.Count.Should().Be(1000);
            generatedIds.Distinct().Count().Should().Be(1000, "all generated IDs should be unique even when generated concurrently");
        }

        [TestMethod]
        public void Should_Generate_Valid_Base58_Encodable_Values()
        {
            // Arrange
            var generator = new Base58IdValueGenerator();
            var entry = CreateMockEntityEntry();

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                var id = generator.Next(entry);
                var encoded = id.ToString();

                // Verify it can be encoded and decoded
                encoded.Should().NotBeNullOrEmpty();
                encoded.Should().HaveLength(11);

                var decoded = new Base58Id(encoded);
                decoded.Should().Be(id);
            }
        }

        private static EntityEntry CreateMockEntityEntry()
        {
            // Create a minimal DbContext for testing
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);
            var entity = new TestEntity { Id = new Base58Id(0) };
            return context.Entry(entity);
        }
    }

    // Test entities and context
    private class TestEntity
    {
        public Base58Id Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestEntity> TestEntities { get; set; } = null!;
    }
}
