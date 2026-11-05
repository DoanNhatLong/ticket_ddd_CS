# 02: View seats and fares for a train trip

**What to build:** A customer can list every seat on a train trip with its carriage number, seat number, seat class, fare and whether it is free. Price is set only by seat class and the train trip's fares (glossary: Seat class, Fare). See spec: user stories 1–6, Schema changes.

**Blocked by:** 01

**Status:** done

- [x] `init.sql` (MySQL syntax): new `SeatClasses` table; `Seats.SeatType` replaced by a `SeatClassId` foreign key; `TripSeatPrices` renamed to `Fares` with a `SeatClassId` foreign key and a unique index on (train trip, seat class); seed data updated. The `init.sql` change is reported per the global rules.
- [x] EF entities and `AppDbContext` match the new schema.
- [x] `GET /api/train-trips/{tripId}/seats` returns each seat of the train running that trip with carriage number, seat number, seat class, fare and free/taken.
- [x] Unknown train trip → 404.
- [x] A seat class with no fare on the trip is shown without a fare (and is not bookable later).
- [x] Carriage type does not affect price.
- [x] HTTP-level tests cover the above.
