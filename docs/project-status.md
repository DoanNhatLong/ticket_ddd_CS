# Project status (as of 2026-11-26)

A snapshot of where TrainBooking stands, written so that a new session (human or AI) can pick the
work up again. Read this first, then `CLAUDE.md`, `web/CLAUDE.md`, `GLOSSARY.md` and the tickets.
Update this file whenever a larger piece of work is finished.

## What the project is

A train ticket booking system built for learning:

- **Backend:** .NET 8 API, DDD with Domain / Application / Infrastructure / Api layers, MediatR,
  FastEndpoints, EF Core with Pomelo (MySQL). Runs on `http://localhost:7777`.
- **Frontend:** `web/` (Vite + React + TypeScript), `http://localhost:5173`, proxies `/api` to the API.
- **Infrastructure (Docker Compose):** MySQL, Redis, Prometheus, Grafana, mysqld/redis exporters,
  the ELK stack (Elasticsearch, Logstash, Kibana) and nginx.
- The customer is identified by the `X-Customer-Id` header (seed users 1–3 are customers, 4 staff,
  5 admin). There is no real authentication yet.

## How to run it

```powershell
cd "D:\0 CS\project_ticket"
docker compose up -d                        # everything; or only: mysql-db redis elasticsearch
dotnet run --project TrainBooking.Api       # wait for "Indexed 5 stations." / "Indexed 5 train trips."
# optional second API copy behind nginx (another terminal; --no-build because copy 1 locks the exe):
dotnet run --project TrainBooking.Api --no-build --launch-profile http-2
cd web; npm run dev                         # http://localhost:5173
```

| Service | Address | Notes |
|---|---|---|
| API | http://localhost:7777 | Postman collection: `docs/postman/TrainBooking.postman_collection.json` |
| Web | http://localhost:5173 | |
| MySQL | localhost:3306 | Dev database `0test1`, test database `trainbooking_test`; root password in `.env` |
| Redis | localhost:6380 | Registered in the API but not used yet |
| Elasticsearch | http://localhost:9200 | Single node, security disabled (dev only) |
| Kibana | http://localhost:5601 | Dashboard "TrainBooking logs" |
| Grafana / Prometheus | :3000 / :9090 | API, MySQL and Redis metrics |
| API copy 2 | http://localhost:7778 | Profile `http-2`; see "Two API copies behind nginx" |
| Nginx | http://localhost:8080 | `/api/` is spread over 7777 and 7778; `/` is still the default nginx page |

Things that have bitten us:

- A native Windows MySQL service (`MySQL84`) used to hold port 3306, so the API silently talked to
  the wrong database. It is now stopped and set to Manual. If `train_mysql` shows no
  `0.0.0.0:3306->3306/tcp`, check that first.
- Port 7777 is inside a Windows-reserved range on this machine; it was excluded manually with
  `netsh int ipv4 add excludedportrange`.
- Stop the API (both copies) before `dotnet build` (the running exe is locked), and before stopping Docker
  (otherwise the booking expiry job logs a connection error every 30 s).
- `init.sql` rebuilds the whole schema and seed. To apply it to the dev database:
  `docker cp init.sql train_mysql:/tmp/init.sql` then
  `docker exec train_mysql sh -c 'mysql --default-character-set=utf8mb4 -uroot -p"$MYSQL_ROOT_PASSWORD" < /tmp/init.sql'`.
  Seed train trips run 2027-01-10 to 2027-01-14.

## Two API copies behind nginx

Simulates two servers on one machine. Config: `monitoring/nginx/conf.d/default.conf`.

```
client -> nginx :8080 -> /api/ -> upstream trainbooking_api
                                   |-- host.docker.internal:7777  api-1 (profile "http")
                                   `-- host.docker.internal:7778  api-2 (profile "http-2")
```

- Round-robin. A copy that refuses a connection is skipped for 10 s (`max_fails=1 fail_timeout=10s`)
  and the request goes to the other copy. nginx only retries a POST if it never reached the API.
- Every response through nginx has an `X-Upstream` header naming the copy that answered.
- api-2 (`launchSettings.json`, profile `http-2`): `Instance:Name=api-2`, logs to
  `logs/trainbooking-api2-*.json`, Elasticsearch rebuild on startup off, booking expiry job off.
  So only api-1 runs the background work.
- Every log line has an `Instance` property (`instance` in Kibana); Prometheus scrapes both copies
  with the label `instance_name`.
- Gotcha: inside the nginx container `host.docker.internal` also resolves to an IPv6 address that
  cannot be reached. nginx therefore resolves the name itself (`resolver 127.0.0.11 ipv6=off` plus
  `resolve` on each server); with a plain `server host.docker.internal:7777;` it would see 4 servers.
- The `instance` field was added to the log index template; the live data stream got it with
  `PUT logs-trainbooking-default/_mapping`. If the Elasticsearch volume is recreated, the template
  (installed by `elasticsearch-setup`) already has it.

## Business: booking lifecycle

Specification: `.scratch/booking-lifecycle/spec.md`. Vocabulary: `GLOSSARY.md`.
Decision record: `docs/adr/0001-tickets-cover-the-whole-train-trip.md`.

Decided rules: seat hold 5 minutes; cancellation fee 10% when 24 h or more before departure,
20% from 4 h to 24 h, refused under 4 h or after departure (fee rounded down); a payment that
arrives late or twice is returned in full (payment reversal). Out of scope: counter sales,
operator cancellations/delays, selling parts of a train trip.

| Ticket | Status |
|---|---|
| 01 Test harness | Done |
| 02 Seats and fares of a train trip | Done |
| 03 Create a booking with a seat hold | Business logic done; tests not written (file still says ready-for-agent) |
| 04 Seat hold expiry, cancel an unpaid booking | Business logic done; tests deferred |
| 05 Pay a booking | **Next** |
| 06 Payment reversal | Not started (needs 04, 05) |
| 07 Cancel a ticket and refund it | Not started (needs 05) |
| 08 Cancel a whole paid booking | Not started (needs 07) |
| 09 Used tickets and completed bookings | Not started (needs 04, 07) |

Details of what exists:

- Double booking is prevented by MySQL: `Tickets.OccupiedSeatKey` (generated column) has a unique index.
- A booking whose hold ran out becomes `Expired` and its tickets become `Cancelled` (no separate
  ticket status). Expiry happens in three places: when someone books the same seat, in the
  background job `BookingExpiryService` (every 30 s, `BookingExpiry:IntervalSeconds`, 0 disables it),
  and when the booking is read or cancelled.
- Known gaps: ticket responses lack seat number/carriage; a booking code collision gives a 500.
- Seed data inconsistency: booking `BK20261103001` is `Pending` but has a `Completed` payment
  (a late payment; ticket 06 will handle that case).

## API endpoints

| Method | Path | Purpose |
|---|---|---|
| GET | `/api/customers/me` | Current customer (`X-Customer-Id`) |
| GET | `/api/train-trips/{id}/seats` | Seat map with fares and free/taken |
| GET | `/api/train-trips/search?q=&from=&to=&date=&page=&pageSize=` | Search train trips (Elasticsearch) |
| GET | `/api/stations/suggest?q=&max=` | Station suggestions while typing (Elasticsearch) |
| POST | `/api/bookings` | Create a booking (holds seats) |
| GET | `/api/bookings/{code}` | One of the customer's bookings |
| POST | `/api/bookings/{code}/cancel` | Cancel an unpaid booking |
| GET | `/api/bookings` | All bookings (staff view, no auth yet) |
| GET | `/api/users/summary` | Users summary |
| POST | `/api/admin/search/stations/rebuild` | Copy stations from MySQL to Elasticsearch |
| POST | `/api/admin/search/train-trips/rebuild` | Copy train trips from MySQL to Elasticsearch |

Errors are RFC 7807 problem responses with a stable `code` (e.g. `Booking.SeatTaken` 409,
`Search.Unavailable` 503).

## Elasticsearch search

MySQL is the source of truth; Elasticsearch holds read-only copies that can be rebuilt any time.

- Port in `TrainBooking.Application` (`IStationSuggester`, `ITrainTripSearch`), adapters in
  `TrainBooking.Infrastructure/Search/`. `SearchIndexBuilder` creates a new index, fills it and
  switches the alias (`stations`, `train-trips`), so searches never see a half-built index.
- The API rebuilds both indices on startup (`Elasticsearch:RebuildOnStartup`, default true).
  Nothing updates them when data changes; call the rebuild endpoints after changing stations or trips.
- Matching: `folding` analyzer (lowercase + ASCII folding, so "ha noi" finds "Hà Nội"),
  fuzziness `AUTO` with the first character required to be right.
  - Station suggestions: completion suggester; each station is stored with its name, name without
    "Ga ", city and code.
  - Train trip search: `from`/`to` are exact station codes, `date` is a UTC day, `q` is free text
    where every word must match (typos allowed); departed train trips are never returned.
- Known behaviour: a short train code like `se2` also matches SE4/SE7/SE21 (exact match ranks first);
  words like "đi"/"đến" are not ignored, so "sai gon di da nang" finds nothing.

## Logging (ELK)

```
API (Serilog) → logs/trainbooking-*.json → Logstash → Elasticsearch (logs-trainbooking-default) → Kibana
```

- `TrainBooking.Api/Logging/LoggingSetup.cs`: console + JSON file (`FileLogging` settings: 10 MB per
  file, 7 files). One line per request; `/metrics` and routine SQL are kept out of the file.
  Request bodies and query strings are never logged (passenger data stays out).
- `monitoring/logstash/pipeline/trainbooking.conf`: reads the files, maps the Serilog fields,
  deduplicates by a hash of the line.
- `monitoring/elasticsearch/trainbooking-logs-template.json`: data stream fields and 7-day retention,
  installed by the one-shot `elasticsearch-setup` container.
- `monitoring/kibana/trainbooking-logs.ndjson`: data view + dashboard; re-import it in
  Kibana → Stack Management → Saved Objects if the Elasticsearch volume is lost.

## Frontend (`web/`)

- Train trip search: From/To with station suggestions, date, free text, results with paging;
  "Choose seats" opens the seat map of that trip.
- Seat map, seat selection across trips, passenger details, booking creation, booking confirmation
  with the seat hold countdown.
- Not built yet: cancel / pay buttons, booking history.

## Tests

Testing is deferred (see `CLAUDE.md`). `TrainBooking.Api.Tests` has an HTTP harness against
`trainbooking_test`, but:

- `TripSeatsTests` is probably broken (written against the old schema and seed).
- When tests are resumed, set `BookingExpiry:IntervalSeconds=0` and
  `Elasticsearch:RebuildOnStartup=false` in the test factory, and `FileLogging:Path` to empty;
  otherwise background jobs change data mid-test and tests overwrite the real search indices.
- `trainbooking_test` still has the old seed dates (November 2026) until the harness rebuilds it.

## Possible next steps

1. Ticket 05 (pay a booking), then 06–09.
2. Frontend: cancel and pay buttons, booking history.
3. Keep search indices in sync when trips change (instead of rebuild only).
4. Bind Elasticsearch to `127.0.0.1:9200` (it is open to the LAN with security disabled).
5. Real authentication instead of `X-Customer-Id`.
6. Architecture ideas from `.scratch/architecture-review-20261105.html` (deepen the Booking aggregate).
