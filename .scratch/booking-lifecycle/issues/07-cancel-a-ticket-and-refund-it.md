# 07: Cancel a ticket and refund it

**What to build:** A customer cancels a single ticket in a confirmed booking and receives a refund equal to the ticket price minus the cancellation fee, which depends on how long before that ticket's train trip departs. The seat goes back on sale and the booking status follows from its tickets. See spec: user stories 34, 36–48; Cancellation fee policy; Refund.

**Blocked by:** 05

**Status:** ready-for-agent

- [ ] `init.sql`: new `Refunds` table (booking, ticket, ticket price, fee, refunded amount, created at). Change reported.
- [ ] Cancellation fee policy: ≥ 24 h before departure → 10 %; ≥ 4 h and < 24 h → 20 %; < 4 h or departed → refused. Fee rounded down to whole đồng.
- [ ] `POST /api/bookings/{code}/tickets/{ticketId}/cancel` returns the refund (ticket price, fee, refunded amount) and booking status.
- [ ] Fee uses the ticket's own train trip departure time and the ticket price, never the current fare.
- [ ] Booking total never changes.
- [ ] Booking stays `Confirmed` while any active ticket remains; becomes `Cancelled` when every ticket is cancelled.
- [ ] Cancelled ticket's seat shows as free again and can be booked.
- [ ] Already-cancelled ticket → 409; another customer's booking → rejected.
- [ ] Boundary tests at exactly 24 h, 23 h 59 m, exactly 4 h, 3 h 59 m and after departure.
