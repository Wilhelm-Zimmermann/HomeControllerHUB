using Xunit;

// The database-backed tests share one PostgreSQL Testcontainer and create an isolated database per test.
// Running these tests in parallel overloads local Docker/Postgres startup and causes intermittent readiness failures.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
