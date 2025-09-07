# LevelUpClone (MVP — Sync CQRS, Clean Architecture)

- .NET 8, SQL Server (procs), Dapper
- Sync-only handlers (no async/await)
- API with JWT + Swagger
- Razor Pages web with SweetAlert2 login and simple daily check-in

## Quickstart
1. Run SQL scripts in `/sql` (01..04).
2. `dotnet build LevelUpClone.sln`
3. Start API: `dotnet run --project src/LevelUpClone.Api`
4. Start Web: `dotnet run --project src/LevelUpClone.Web`
5. Open `http://localhost:5073/` (web) and login with any user/pass (demo token).

Update `appsettings.json` connection string.
