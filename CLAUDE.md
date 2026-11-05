# Project Rules

## Locked files / busy ports
- If a build or run fails because a file is locked by another process (e.g. MSB3021/MSB3027 on `TrainBooking.Api.exe`) or a port is already in use (e.g. 7777), STOP immediately and notify the user.
- Do NOT work around it: no building to a temporary/scratch output directory, no running a parallel copy on another port, and no killing the user's processes.
- Wait for the user to free the file/port before retrying.

## Architecture (intentional, do not change)
- This project uses DDD with three layers: Domain, Application, Infrastructure.
- Layer separation and dependency direction are deliberate.
- Do NOT suggest merging layers, removing repository/domain interfaces, or
  collapsing entities, value objects and handlers to reduce file count.
- Architecture review suggestions must work within this structure. Flag
  anything that would break it instead of proposing it.
## Stack
- .NET / C#, DDD with Clean Architecture (Domain, Application, Infrastructure, Api).
- Database: MySQL. All SQL, including `init.sql`, must use MySQL syntax.
- Use a MySQL EF Core provider (e.g. Pomelo.EntityFrameworkCore.MySql). Do not use SQL Server or PostgreSQL providers.

## Commands
- Build: `dotnet build`
- Test: `dotnet test`
- After changing code, run build and test, and fix errors before reporting the task as done.

## Database
- Never run scripts against a real/production database. Use a test database only.
- Any change to `init.sql` must be reported (see global rules).

## Structure
- `<fill in>`: one line per project/folder, e.g. `src/TrainBooking.Domain`, `src/TrainBooking.Application`, `src/TrainBooking.Infrastructure`, `src/TrainBooking.Api`.

## Agent skills

### Issue tracker

Issues are tracked as local markdown files under `.scratch/<feature>/`. See `docs/agents/issue-tracker.md`.

### Domain docs

Single-context: one `GLOSSARY.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.
