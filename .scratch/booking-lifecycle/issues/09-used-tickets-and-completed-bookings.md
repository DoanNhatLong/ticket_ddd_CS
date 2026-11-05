# 09: Used tickets and completed bookings

**What to build:** Once a train trip has finished (status `Completed`), its tickets become used tickets. A paid booking with no active ticket left and at least one used ticket becomes a completed booking, including a round trip where the outbound leg was travelled and the return was cancelled. Used tickets cannot be cancelled. See spec: user stories 45, 49–50; Booking status.

**Blocked by:** 04, 07

**Status:** ready-for-agent

- [ ] The background service from ticket 04 marks active tickets of completed train trips as `Used`.
- [ ] Booking with only used (and possibly cancelled) tickets → `Completed`.
- [ ] Round trip: outbound used, return still active → `Confirmed`; return then cancelled → `Completed` immediately.
- [ ] Cancelling a used ticket → 409.
- [ ] Train trip status is changed by test setup only (operating train trips is out of scope).
- [ ] HTTP-level tests cover the above.
