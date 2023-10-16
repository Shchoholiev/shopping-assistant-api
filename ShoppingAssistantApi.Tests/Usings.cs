global using Xunit;

// Parallel running was disabled because it causes errors with the database access
[assembly: CollectionBehavior(DisableTestParallelization = true)]