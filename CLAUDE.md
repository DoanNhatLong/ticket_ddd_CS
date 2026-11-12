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

## Priorities
- Business logic (domain rules, use cases, API behaviour) comes first.
- Testing is deferred: do NOT write, update or fix tests unless the user explicitly asks for it.
  Tickets' "tests cover the above" criteria stay open until then.
- When a code change breaks existing tests, say so in the report instead of fixing them.

## Task estimates
- When the user assigns a task, first give a rough estimate of the tokens it will consume
  (e.g. small < 50k, medium 50k–150k, large > 150k, with a number) and what drives the cost,
  then proceed unless the user objects.

## Commands
- Build: `dotnet build`
- Test: `dotnet test` (only when the user asks for testing work)
- After changing code, run `dotnet build` and fix compile errors before reporting the task as done.

## Database
- Never run scripts against a real/production database. Use a test database only.
- Any change to `init.sql` must be reported (see global rules).

## Structure
- `TrainBooking.Domain/`: aggregates, entities, value objects, repository interfaces, domain errors.
- `TrainBooking.Application/`: MediatR commands/queries, handlers, pipeline behaviors, response DTOs.
- `TrainBooking.Infrastructure/`: EF Core (scaffolded entities, `AppDbContext`), repository implementations, Redis.
- `TrainBooking.Api/`: FastEndpoints endpoints, HTTP error mapping, composition root (`Program.cs`).
- `TrainBooking.Api.Tests/`: HTTP-level integration tests against the MySQL test database.
- `web/`: React + Vite + TypeScript frontend. Its own rules are in `web/CLAUDE.md`.
- `mock/`: mock work (contents to be defined).
- `monitoring/`: Prometheus config, Grafana provisioning, MySQL exporter user script.
- `docs/`: ADRs, agent docs, Postman collection. `.scratch/`: specs and tickets.
- `init.sql`: MySQL schema and seed data.
- The rules in this file about .NET, DDD and `dotnet build` apply to the backend projects only.

## Task scope (what to read)
Do not scan the whole repository for every task. Read only what the task's area needs:
- **Backend task** (API, domain, use cases, database): read only the backend files the task touches
  (`TrainBooking.*` projects, `init.sql`, and `GLOSSARY.md`/`docs/adr/`/`.scratch/` when the task refers to them).
  Do not open `web/` or `mock/`.
- **Frontend task**: work only inside `web/` (follow `web/CLAUDE.md`). Do not open backend code;
  if the UI needs an API change, say so instead of reading or changing the backend.
- **Mock task**: work only inside `mock/`.
- Look outside the task's area only when the user asks, or when the task genuinely cannot be done
  without it; then say what you read and why.

## Agent skills

### Issue tracker

Issues are tracked as local markdown files under `.scratch/<feature>/`. See `docs/agents/issue-tracker.md`.

### Domain docs

Single-context: one `GLOSSARY.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.
