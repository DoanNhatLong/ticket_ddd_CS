# 01: Test harness and shared seams

**What to build:** A safe foundation for every later ticket. An automated test can start the real API, call it over HTTP against a dedicated MySQL **test** database built from `init.sql`, control the current time, and act as a given customer. Business-rule failures come back as clear HTTP errors. Prefactoring only; no new booking behaviour. See spec `.scratch/booking-lifecycle/spec.md` (Testing Decisions, Customer identity).

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

- [ ] A new xUnit test project starts the API through `WebApplicationFactory` and talks to it over HTTP.
- [ ] Tests run against a separate MySQL test database (never the dev/real one), created from `init.sql` and reset to seed data between tests.
- [ ] Application code reads the current time only through an injected `TimeProvider`; tests replace it with a fake clock.
- [ ] The customer is identified by a request header carrying the user id; a missing or unknown id is rejected.
- [ ] Rule violations returned as Results map to 400 / 404 / 409 with a readable error body; unexpected errors no longer return an empty 500.
- [ ] A test for the existing `GET /api/bookings` passes against the test database.
- [ ] `dotnet build` and `dotnet test` pass.
