# 08: Cancel a whole paid booking

**What to build:** A customer cancels every remaining ticket of a confirmed booking in one step and gets one refund per ticket. If any remaining ticket is past its cancellation cutoff, the whole request is refused and nothing changes. See spec: user story 35; API contracts.

**Blocked by:** 07

**Status:** ready-for-agent

- [ ] `POST /api/bookings/{code}/cancel` on a confirmed booking cancels all active tickets, one refund each, booking → `Cancelled`.
- [ ] Round trip where one leg is past its cutoff → 409, no ticket cancelled, no refund created.
- [ ] Already-cancelled tickets in the booking are skipped, not refunded again.
- [ ] All seats released.
- [ ] HTTP-level tests cover the above.
