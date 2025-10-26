# Viae.Utils.Base58

A high-performance, type-safe Base58 encoding library for .NET with Entity Framework Core integration.

## Features

- 🔢 **Type-safe IDs** - `Base58Id` struct provides compile-time safety
- 🎯 **Fixed-length encoding** - Always encodes to exactly 11 characters
- ⚡ **High performance** - Direct `ulong` conversion without byte array allocations
- 🔄 **Implicit conversions** - Seamlessly converts between `ulong`, `string`, and `Base58Id`
