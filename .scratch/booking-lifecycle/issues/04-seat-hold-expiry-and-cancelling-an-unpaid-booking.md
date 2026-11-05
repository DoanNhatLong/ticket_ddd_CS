# 04: Seat hold expiry and cancelling an unpaid booking

**What to build:** An unpaid booking whose 5-minute seat hold runs out becomes an expired booking and its seats go back on sale. A customer can also cancel their own unpaid booking before the hold runs out, releasing the seats with no refund. See spec: user stories 21–24, 52; Expiry.

**Blocked by:** 03

**Status:** ready-for-agent

- [ ] Booking status gains `Expired` (domain and `init.sql`; change reported).
- [ ] Booking a seat whose previous hold has run out succeeds: the overdue booking is expired in the same transaction.
- [ ] A background service periodically expires overdue bookings and clears their occupied-seat keys.
- [ ] Expired bookings show as `Expired` in `GET /api/bookings/{code}` and in the existing `GET /api/bookings`.
- [ ] `POST /api/bookings/{code}/cancel` on a pending booking → `Cancelled`, seats released, no refund.
- [ ] Tests move the fake clock past the hold instead of sleeping.
