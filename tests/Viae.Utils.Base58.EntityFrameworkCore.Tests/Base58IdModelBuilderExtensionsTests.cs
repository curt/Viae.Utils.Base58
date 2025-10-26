// Copyright (c) Curt Gilman. Licensed under the MIT License. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viae.Utils.Base58.Core;

namespace Viae.Utils.Base58.EntityFrameworkCore.Tests;

[TestClass]
public class Base58IdModelBuilderExtensionsTests
{
    [TestClass]
    public class ConfigureBase58Id_Method
    {
        [TestMethod]
        public void Should_Configure_Base58Id_Properties()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            // Act
            context
                .Model.GetEntityTypes()
                .SelectMany(e => e.GetProperties())
                .Where(p => p.ClrType == typeof(Base58Id))
                .Should()
                .NotBeEmpty("there should be Base58Id properties");

            // Assert - Check that Base58Id properties are configured
            foreach (var entityType in context.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(Base58Id))
                    {
                        // Verify converter is set
                        property.GetValueConverter().Should().NotBeNull();
                        property.GetValueConverter()!.ProviderClrType.Should().Be(typeof(string));

                        // Verify comparer is set
                        property.GetValueComparer().Should().NotBeNull();

                        // Verify column type annotation exists
                        var columnType = property.FindAnnotation("Relational:ColumnType");
                        columnType.Should().NotBeNull();
                        columnType!.Value.Should().Be("char(11)");
                    }
                }
            }
        }

        [TestMethod]
        public void Should_Configure_Value_Generator_For_Id_Property()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            // Act
            var entityType = context.Model.FindEntityType(typeof(EntityWithId))!;
            var idProperty = entityType.FindProperty(nameof(EntityWithId.Id))!;

            // Assert
            idProperty.GetValueGeneratorFactory().Should().NotBeNull();
        }

        [TestMethod]
        public void Should_Configure_Value_Generator_For_EntityNameId_Property()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            // Act
            var entityType = context.Model.FindEntityType(typeof(EntityWithConventionalId))!;
            var idProperty = entityType.FindProperty(
                nameof(EntityWithConventionalId.EntityWithConventionalIdId)
            )!;

            // Assert
            idProperty.GetValueGeneratorFactory().Should().NotBeNull();
        }

        [TestMethod]
        public void Should_Not_Configure_Value_Generator_For_Non_Id_Properties()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            // Act
            var entityType = context.Model.FindEntityType(
                typeof(EntityWithMultipleBase58Properties)
            )!;
            var codeProperty = entityType.FindProperty(
                nameof(EntityWithMultipleBase58Properties.Code)
            )!;

            // Assert
            codeProperty.GetValueGeneratorFactory().Should().BeNull();
        }

        [TestMethod]
        public void Should_Allow_Saving_And_Retrieving_Entities()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using (var context = new TestDbContext(options))
            {
                var entity = new EntityWithId { Id = new Base58Id(12345), Name = "Test" };

                // Act
                context.Entities.Add(entity);
                context.SaveChanges();
            }

            // Assert
            using (var context = new TestDbContext(options))
            {
                var retrieved = context.Entities.Single();
                retrieved.Id.Value.Should().Be(12345);
                retrieved.Name.Should().Be("Test");
            }
        }

        [TestMethod]
        public void Should_Store_Base58Id_As_String_In_Database()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var testId = new Base58Id(12345);

            using (var context = new TestDbContext(options))
            {
                var entity = new EntityWithId { Id = testId, Name = "Test" };
                context.Entities.Add(entity);
                context.SaveChanges();
            }

            // Assert - Verify the value is stored correctly
            using (var context = new TestDbContext(options))
            {
                var retrieved = context.Entities.Single();
                retrieved.Id.Should().Be(testId);
                retrieved.Id.ToString().Should().Be("111111114fr");
            }
        }

        [TestMethod]
        public void Should_Handle_Multiple_Base58Id_Properties()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using (var context = new TestDbContext(options))
            {
                var entity = new EntityWithMultipleBase58Properties
                {
                    Id = new Base58Id(100),
                    Code = new Base58Id(200),
                    ReferenceId = new Base58Id(300),
                };

                // Act
                context.MultiPropertyEntities.Add(entity);
                context.SaveChanges();
            }

            // Assert
            using (var context = new TestDbContext(options))
            {
                var retrieved = context.MultiPropertyEntities.Single();
                retrieved.Id.Value.Should().Be(100);
                retrieved.Code.Value.Should().Be(200);
                retrieved.ReferenceId.Value.Should().Be(300);
            }
        }

        [TestMethod]
        public void Should_Support_Queries_With_Base58Id()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var searchId = new Base58Id(12345);

            using (var context = new TestDbContext(options))
            {
                context.Entities.AddRange(
                    new EntityWithId { Id = new Base58Id(100), Name = "First" },
                    new EntityWithId { Id = searchId, Name = "Second" },
                    new EntityWithId { Id = new Base58Id(300), Name = "Third" }
                );
                context.SaveChanges();
            }

            // Act
            using (var context = new TestDbContext(options))
            {
                var result = context.Entities.Single(e => e.Id == searchId);

                // Assert
                result.Name.Should().Be("Second");
            }
        }
    }

    [TestClass]
    public class HasBase58IdConversion_Method
    {
        [TestMethod]
        public void Should_Configure_Property_With_Converter()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ManualConfigDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ManualConfigDbContext(options);

            // Act
            var entityType = context.Model.FindEntityType(typeof(ManuallyConfiguredEntity))!;
            var property = entityType.FindProperty(nameof(ManuallyConfiguredEntity.CustomId))!;

            // Assert
            property.GetValueConverter().Should().NotBeNull();
            property.GetValueConverter()!.ProviderClrType.Should().Be(typeof(string));
        }

        [TestMethod]
        public void Should_Configure_Property_With_Comparer()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ManualConfigDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ManualConfigDbContext(options);

            // Act
            var entityType = context.Model.FindEntityType(typeof(ManuallyConfiguredEntity))!;
            var property = entityType.FindProperty(nameof(ManuallyConfiguredEntity.CustomId))!;

            // Assert
            property.GetValueComparer().Should().NotBeNull();
        }

        [TestMethod]
        public void Should_Configure_Property_With_Column_Type()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ManualConfigDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ManualConfigDbContext(options);

            // Act
            var entityType = context.Model.FindEntityType(typeof(ManuallyConfiguredEntity))!;
            var property = entityType.FindProperty(nameof(ManuallyConfiguredEntity.CustomId))!;

            // Assert
            var columnType = property.FindAnnotation("Relational:ColumnType");
            columnType.Should().NotBeNull();
            columnType!.Value.Should().Be("char(11)");
        }

        [TestMethod]
        public void Should_Allow_Saving_And_Retrieving_Manually_Configured_Entities()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ManualConfigDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var testId = new Base58Id(99999);

            using (var context = new ManualConfigDbContext(options))
            {
                var entity = new ManuallyConfiguredEntity { CustomId = testId, Data = "Test Data" };
                context.ManualEntities.Add(entity);
                context.SaveChanges();
            }

            // Assert
            using (var context = new ManualConfigDbContext(options))
            {
                var retrieved = context.ManualEntities.Single();
                retrieved.CustomId.Should().Be(testId);
                retrieved.Data.Should().Be("Test Data");
            }
        }
    }

    // Test entities
    private sealed class EntityWithId
    {
        public Base58Id Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class EntityWithConventionalId
    {
        public Base58Id EntityWithConventionalIdId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class EntityWithMultipleBase58Properties
    {
        public Base58Id Id { get; set; }
        public Base58Id Code { get; set; }
        public Base58Id ReferenceId { get; set; }
    }

    private sealed class ManuallyConfiguredEntity
    {
        public int Id { get; set; }
        public Base58Id CustomId { get; set; }
        public string Data { get; set; } = string.Empty;
    }

    // Test DbContext
    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<EntityWithId> Entities { get; set; } = null!;
        public DbSet<EntityWithConventionalId> ConventionalEntities { get; set; } = null!;
        public DbSet<EntityWithMultipleBase58Properties> MultiPropertyEntities { get; set; } =
            null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureBase58Id();
        }
    }

    private sealed class ManualConfigDbContext(DbContextOptions<ManualConfigDbContext> options)
        : DbContext(options)
    {
        public DbSet<ManuallyConfiguredEntity> ManualEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<ManuallyConfiguredEntity>()
                .Property(e => e.CustomId)
                .HasBase58IdConversion();
        }
    }
}
