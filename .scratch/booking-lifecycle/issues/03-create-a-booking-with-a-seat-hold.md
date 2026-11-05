# 03: Create a booking with a seat hold

**What to build:** A customer picks one or more seats (possibly on several train trips), names a passenger for each, and gets a pending booking with a booking code, booking total and a 5-minute seat hold. Held seats show as taken. A seat can never be sold twice, even under concurrent requests. The customer can look the booking up by its code. See spec: user stories 7–20, 51; Domain; Seat availability and double-booking.

**Blocked by:** 02

**Status:** ready-for-agent

- [ ] `init.sql`: `BookingItems` renamed to `Tickets`; ticket status column (`Held|Active|Cancelled|Used`); nullable occupied-seat key (train trip + seat) with a unique index; passenger identity number column; seed data updated. Change reported.
- [ ] Domain: `Booking.Create` factory and `Ticket` child entity; booking total = sum of ticket prices; hold ends at now + 5 minutes; ticket price fixed from the current fare.
- [ ] `POST /api/bookings` returns 201 with booking code, `Pending`, booking total, hold end and tickets.
- [ ] One booking may contain tickets on several train trips; the same passenger may hold several tickets on one trip.
- [ ] 409 when any chosen seat is taken; the whole booking fails (all-or-nothing).
- [ ] 400 for: missing passenger identity number, departed train trip, seat not on that trip's train, seat class without a fare.
- [ ] Two concurrent requests for the same seat: exactly one 201 and one 409 (enforced by the unique index, not a pre-check).
- [ ] Booked seats show as taken in the seat list from ticket 02.
- [ ] `GET /api/bookings/{code}` returns the booking detail; only its owning customer may read it.
- [ ] HTTP-level tests cover the above.
