# 05: Pay a booking to confirm it

**What to build:** A customer pays the booking total in full within the seat hold; the first successful payment makes it a confirmed booking and its tickets active. Failed payments leave the hold running so the customer can retry. See spec: user stories 25–29, 33; Payment.

**Blocked by:** 03

**Status:** ready-for-agent

- [ ] `init.sql`: `Payments.Status` values `Pending|Succeeded|Failed|Reversed`, `ReversedAt` column. Change reported.
- [ ] `POST /api/bookings/{code}/payments` with transaction id, amount, payment method and success flag (stands in for the payment provider callback).
- [ ] Successful payment within the hold → booking `Confirmed`, tickets `Active`.
- [ ] Failed payment → booking stays `Pending`; a later successful payment still confirms it.
- [ ] Amount different from the booking total → rejected, nothing recorded as succeeded.
- [ ] Same transaction id reported twice → recorded once, same response.
- [ ] Only the owning customer may pay their booking.
- [ ] HTTP-level tests cover the above.
