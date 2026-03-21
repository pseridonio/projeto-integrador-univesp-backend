# EF Core Migrations with .NET Native AOT

This document explains recommended approaches to handle Entity Framework Core migrations when publishing applications using .NET Native AOT.

Context
- Native AOT disables or restricts certain runtime features such as dynamic code generation and reflection emit. Many EF Core migrations operations rely on runtime model building and proxies which may use features not available in AOT builds. Calling `db.Database.Migrate()` at runtime in an AOT binary can trigger warnings (IL3050) and may fail.

Recommended approaches

1) Run migrations outside the AOT process (recommended for production)
- Generate SQL migration scripts during CI or deployment time using `dotnet ef migrations script` or `dotnet ef database update` from a non-AOT toolchain (developer machine or CI runner).
- Apply the SQL scripts against the target database using your CI/CD pipeline or a database migration tool (Flyway, Liquibase, psql, etc.).

Example script to generate SQL:

```
# generate SQL script for all pending migrations
dotnet ef migrations script --project app/CafeSystem.Infra --startup-project app/CafeSystem.API -o migrations.sql
```

2) Use a separate non-AOT migration runner
- Create a small console app (not published as AOT) that references the same `AppDbContext` and runs `db.Database.Migrate()` on startup.
- Run this tool as a deployment step before starting the AOT application.

3) Conditional runtime migration (development only)
- Keep `db.Database.Migrate()` only for development runs and skip it in production AOT deployments. Use `RuntimeFeature.IsDynamicCodeSupported` to detect if migrations are safe to run (code in `Program.cs` already checks this).

4) Avoid runtime model building features that require dynamic code when possible
- Limit usage of features like runtime proxy generation, value converter expressions that compile at runtime, etc. Prefer compile-time model configuration via `IEntityTypeConfiguration<T>` and explicit mappings.

Checklist for migrating to AOT
- [ ] Ensure migrations are generated during CI and stored as SQL files.
- [ ] Apply migrations using a controlled deployment step.
- [ ] Do not call `db.Database.Migrate()` in the AOT runtime for production.
- [ ] Use `RuntimeFeature.IsDynamicCodeSupported` if you need to detect environment capabilities.

References
- https://learn.microsoft.com/dotnet/core/deploying/native-aot/warnings/il3050
- https://learn.microsoft.com/ef/core/managing-schemas/migrations/

