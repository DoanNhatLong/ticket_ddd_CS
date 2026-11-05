# 06: Payment reversal for unneeded payments

**What to build:** A successful payment the booking did not need is recorded and returned in full with no fee: a second successful payment on an already-paid booking, or a successful payment on an expired or cancelled booking. An expired booking never becomes paid. See spec: user stories 23, 30–32; Payment.

**Blocked by:** 04, 05

**Status:** ready-for-agent

- [ ] Second successful payment on a confirmed booking → payment `Reversed`, booking unchanged.
- [ ] Successful payment after the hold ran out → payment `Reversed`, booking stays `Expired`, seats stay released.
- [ ] Successful payment on a cancelled unpaid booking → payment `Reversed`.
- [ ] The response states the payment was reversed and the full amount returned.
- [ ] HTTP-level tests cover the above using the fake clock.
