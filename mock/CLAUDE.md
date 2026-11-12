# Mock Rules (mock/)

These rules apply to everything in `mock/`. The root `CLAUDE.md` still applies
(language rules, priorities, deferred testing, task estimates, task scope); its .NET-specific rules
(DDD layers, `dotnet build`, MySQL) do not apply here unless a mock is written in .NET.

## Purpose
- <fill in: what the mocks are for (e.g. a fake API for the frontend, fake payment gateway, sample data)>

## Scope
- A mock task works only inside `mock/`. Do not read or change `web/` or the backend projects.
- If a mock must match the real API or domain (endpoints, fields, error codes, status names),
  ask the user for the contract or say which backend/GLOSSARY file you need to read, and why, before reading it.

## Stack and commands
- <fill in: language/tool for the mocks, how to run them, port>
- Do NOT add libraries or tools, or install anything globally, without asking the user first.
- Do not use ports already taken by the project: 7777 (API), 5173 (web), 3306/6380 (MySQL/Redis),
  9090 (Prometheus), 3000 (Grafana). Ask before picking a new port.

## Data
- Mock data must be clearly fake: no real names, ID numbers, phone numbers, emails or payment details.
- Use the domain vocabulary from the root `GLOSSARY.md` (Booking, Ticket, Passenger, Seat hold, ...)
  in names and fields.
- Mocks never connect to the real development database (`0test1`) or any production system.

## Structure
- <fill in once the first mock exists>
