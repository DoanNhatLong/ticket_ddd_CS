# Spec: Booking lifecycle (seat hold, payment, ticket cancellation)

Status: ready-for-agent

Vocabulary follows `GLOSSARY.md`. Respects ADR `docs/adr/0001-tickets-cover-the-whole-train-trip.md`.

## Problem Statement

A customer cannot buy a train ticket today. The API can only list bookings and users; there is no way to pick seats on a train trip, hold them while paying, pay, or cancel a ticket and get money back. The domain model has no behaviour at all, so none of the rules the business relies on (seat hold time, cancellation fees, which state a booking is in) exist anywhere in code.

## Solution

A customer can see which seats on a train trip are free and what each costs, pick one or more seats (possibly on several train trips, e.g. outbound and return) and name a passenger for each, and get a booking that holds those seats for 5 minutes. Paying the booking total in full within that window confirms the booking. Afterwards the customer can cancel any single ticket, or all of them, and receives a refund equal to the ticket price minus a cancellation fee that depends on how long before departure they cancel. Unpaid bookings expire on their own and release their seats. Payments the booking did not need (duplicates, late payments) are reversed in full automatically. The booking's status always follows from the state of its tickets.

## User Stories

### Finding seats

1. As a customer, I want to see every seat on a train trip with whether it is free, so that I can choose where to sit.
2. As a customer, I want to see the fare for each seat's seat class on that train trip, so that I know what I will pay before booking.
3. As a customer, I want seats that are held by someone else's unpaid booking to show as taken, so that I do not try to book them.
4. As a customer, I want seats whose seat hold has run out to show as free again, so that I can book seats others abandoned.
5. As a customer, I want seats on a cancelled ticket to show as free again, so that I can buy seats others gave back.
6. As a customer, I want the seat list to tell me each seat's carriage and seat number, so that I can find it on the train.

### Booking

7. As a customer, I want to book one or more seats in a single checkout, so that my family travels together on one booking.
8. As a customer, I want to name a passenger (name and identity document number) for each ticket, so that each traveller's ticket is valid.
9. As a customer, I want to book tickets on several train trips in one booking, so that I can buy my outbound and return trip together.
10. As a customer, I want to book two seats for the same passenger on the same train trip, so that I can have extra space.
11. As a customer, I want each ticket's price fixed at the fare in force when I book, so that later fare changes do not affect me.
12. As a customer, I want to receive a booking code, the booking total, and the time my seat hold ends, so that I know what to pay and by when.
13. As a customer, I want booking to fail with a clear reason if any chosen seat is already taken, so that I can pick another seat.
14. As a customer, I want a booking to be all-or-nothing, so that I never end up holding only some of the seats I chose.
15. As a customer, I want booking to fail if a passenger has no identity document number, so that I do not buy an unusable ticket.
16. As a customer, I want booking to fail for a seat that does not belong to the train running that train trip, so that I cannot buy a seat that does not exist.
17. As a customer, I want booking to fail for a train trip that has already departed, so that I do not pay for a train I cannot catch.
18. As a customer, I want booking to fail when a seat class has no fare on that train trip, so that I am never charged an undefined price.
19. As a customer, when two people try to book the same seat at the same moment, I want exactly one of them to succeed, so that a seat is never sold twice.

### Seat hold and expiry

20. As a customer, I want my seats held for 5 minutes after booking, so that nobody else takes them while I pay.
21. As a customer, I want an unpaid booking to become an expired booking once its seat hold runs out, so that its state is clear.
22. As a customer, I want an expired booking's seats to go back on sale, so that seats are not blocked by abandoned bookings.
23. As a customer, I want an expired booking to never become paid, so that I am not charged for seats that may have been resold.
24. As a customer, I want to cancel my unpaid booking before the hold runs out, so that I can release seats I no longer want without paying anything.

### Payment

25. As a customer, I want to pay the full booking total in one payment, so that my booking is confirmed.
26. As a customer, I want my booking to become a confirmed booking as soon as one payment succeeds within the seat hold, so that my tickets become active.
27. As a customer, I want a failed payment to leave my booking unpaid with the hold still running, so that I can try again with another method.
28. As a customer, I want to try several payments on the same booking, so that one declined card does not lose me the seats.
29. As a customer, I want a payment for an amount different from the booking total to be rejected, so that partial payments never happen.
30. As a customer, I want a second successful payment on an already-paid booking to be returned in full by a payment reversal, so that I am never charged twice.
31. As a customer, I want a successful payment that arrives after my booking expired to be returned in full by a payment reversal, so that I do not pay for nothing.
32. As a customer, I want a payment reversal to carry no fee, so that I get back exactly what I paid.
33. As a customer, I want the same payment result reported twice (same transaction id) to be recorded only once, so that retries from the payment provider do not create duplicate payments.

### Ticket cancellation and refunds

34. As a customer, I want to cancel a single ticket in my confirmed booking, so that a passenger who cannot travel does not block the others.
35. As a customer, I want to cancel every ticket in my booking at once, so that I can call off the whole trip in one step.
36. As a customer, I want a refund for each cancelled ticket, so that I get my money back.
37. As a customer, I want the cancellation fee to be 10% of the ticket price when I cancel 24 hours or more before departure, so that early cancellation costs little.
38. As a customer, I want the cancellation fee to be 20% of the ticket price when I cancel between 4 and 24 hours before departure, so that the rule is predictable.
39. As a customer, I want cancellation to be refused less than 4 hours before departure or after departure, so that I know the cancellation cutoff.
40. As a customer, I want the fee calculated from the ticket's own train trip departure time, so that cancelling the return leg of a round trip uses the return train's time.
41. As a customer, I want the fee calculated from the ticket price, never the current fare, so that fare changes after booking do not change my refund.
42. As a customer, I want the booking total to stay what I was charged even after cancellations, so that my purchase record is not rewritten.
43. As a customer, I want a cancelled ticket's seat to go back on sale, so that someone else can use it.
44. As a customer, I want cancelling an already-cancelled ticket to be refused, so that I cannot get refunded twice.
45. As a customer, I want cancelling a used ticket to be refused, so that refunds only apply to travel not yet taken.
46. As a customer, I want only my own bookings to be cancellable by me, so that nobody else can cancel my tickets.

### Booking status

47. As a customer, I want my booking to stay a confirmed booking while it still has at least one active ticket, so that partial cancellation does not end it.
48. As a customer, I want my booking to become a cancelled booking when every ticket in it is cancelled, so that its state reflects reality.
49. As a customer, I want my booking to become a completed booking when no active ticket is left and at least one ticket was used, so that a round trip whose return I cancelled after travelling the outbound leg is shown as completed.
50. As a customer, I want a ticket to become a used ticket once its train trip has finished, so that I can see which travel is done.
51. As a customer, I want to look up my booking by its code and see its status, total, every ticket with its passenger, seat, train trip, ticket price and status, and every refund, so that I have a full picture of my purchase.

### Staff/admin visibility

52. As an admin, I want the existing booking list to keep working and show the new statuses (including expired), so that reporting does not break.

## Implementation Decisions

### Domain (Bookings aggregate)

- `Booking` stays the aggregate root and owns its tickets, payments and refunds. All rules below live in the domain; Application handlers only load, call and save.
- New child entity **Ticket** (replaces the "booking item" name). It has a passenger name, passenger identity document number, train trip, seat, ticket price, and a ticket status: `Held` (booking unpaid), `Active`, `Cancelled`, `Used`. `Held` is the state of a ticket in an unpaid booking; glossary "active ticket" means paid, so a held ticket is not active.
- New factory `Booking.Create(...)` for new bookings, taking the customer, the chosen seats with passengers and their fares, and the current time. It sets the booking total as the sum of ticket prices and the seat hold end as now + 5 minutes. `Rehydrate` stays for repositories.
- Booking status values: `Pending`, `Expired`, `Confirmed`, `Cancelled`, `Completed`. `Expired` is new.
- Booking status is derived, never set directly:
  - unpaid and hold not over → `Pending`; unpaid and hold over → `Expired`; unpaid and cancelled by customer → `Cancelled`.
  - paid: any active ticket → `Confirmed`; else any used ticket → `Completed`; else (all cancelled) → `Cancelled`.
- Behaviour on the aggregate: `RecordPayment`, `CancelTicket`, `CancelAllTickets`, `Expire`, `MarkTicketsUsedFor(trainTrip)`. Each takes the current time where time matters and returns a Result (use the existing Result pattern skill) instead of throwing for rule violations.
- **Cancellation fee policy** is a domain service / value object of its own: given ticket price, departure time and cancellation time it returns either "refused (past cutoff)" or the fee. Rates: ≥ 24 h → 10 %; ≥ 4 h and < 24 h → 20 %; < 4 h or departed → refused. Boundaries: exactly 24 h counts as 10 %, exactly 4 h counts as 20 %.
- **Refund** is a child entity: ticket, ticket price, fee, refunded amount (price − fee), created at. One refund per cancelled paid ticket. Cancelling a ticket of an unpaid booking creates no refund.
- **Payment** is a child entity: transaction id, amount, payment method, status `Pending | Succeeded | Failed | Reversed`, paid at, reversed at. Rules: amount must equal booking total; the first successful payment within the hold confirms the booking and moves its tickets from `Held` to `Active`; any further successful payment, or any successful payment on an expired or cancelled booking, is recorded and immediately marked `Reversed` (payment reversal, full amount, no fee).
- Train trip has finished when its status is `Completed`. Changing train trip status is outside this spec; tickets become used when the system observes a completed train trip (see expiry/sweeper below).

### Application

- Commands: `CreateBooking`, `RecordPaymentResult`, `CancelTicket`, `CancelBooking` (all tickets / unpaid booking). Queries: `GetTripSeats` (seat availability + fares), `GetBookingByCode`. Existing `GetAllBookings` keeps working with the new statuses.
- All time comes from an injected `TimeProvider`, never `DateTime.Now`, so tests can control the clock.
- New repository interfaces stay in Domain (train trips/seats/fares read access as needed); implementations in Infrastructure. Layering is unchanged.
- Booking code is generated by the application (unique, short, uppercase alphanumeric), stored in the existing unique column.

### Customer identity

- There is no authentication yet. Until there is, the customer is identified by a request header carrying the customer's user id; endpoints reject requests without it, and a customer can only read or change bookings they own. Replacing this with real authentication is out of scope.

### API contracts (FastEndpoints, same style as existing endpoints)

- `GET /api/train-trips/{tripId}/seats` → seats with carriage number, seat number, seat class, fare, free/taken.
- `POST /api/bookings` → body: list of { trainTripId, seatId, passengerName, passengerIdNumber }. Returns booking code, status `Pending`, booking total, hold end time, tickets. 409 when any seat is taken; 400 for validation failures (missing id number, departed trip, seat not on that train, no fare).
- `POST /api/bookings/{code}/payments` → body: { transactionId, amount, paymentMethod, succeeded }. Stands in for the payment provider callback; idempotent on transaction id. Returns the payment outcome (`Succeeded`, `Failed`, `Reversed`) and the booking status.
- `POST /api/bookings/{code}/tickets/{ticketId}/cancel` → returns the refund (price, fee, refunded amount) and the booking status. 409 for past cutoff / already cancelled / used.
- `POST /api/bookings/{code}/cancel` → cancels every remaining ticket (or the unpaid booking); returns refunds and status. Refused as a whole if any remaining active ticket is past the cutoff.
- `GET /api/bookings/{code}` → booking detail per user story 51.
- Validation via FluentValidation in the MediatR pipeline.

### Seat availability and double-booking

- A seat on a train trip is taken while a ticket for it is `Held` (and its booking's hold has not run out) or `Active`.
- Double-booking is prevented by the database, not by a check-then-insert: tickets carry an "occupied seat" key (train trip + seat) that is set while the ticket is held or active and cleared when it is cancelled or its booking expires; the key has a unique index (MySQL allows many NULLs). A unique-violation on insert maps to 409.
- Expiry: before creating a booking, overdue holds on the requested seats are expired in the same transaction, so an abandoned hold never blocks a new customer. A background hosted service also periodically expires overdue bookings and marks tickets of completed train trips as used.
- Caching (Redis) must not be used for seat availability.

### Schema changes (`init.sql`, MySQL syntax)

- `BookingItems` → renamed to `Tickets`; add `Status` (`Held|Active|Cancelled|Used`), add nullable occupied-seat key with a unique index; rename `PassengerIdCard` to a passenger identity number column.
- `Bookings.Status` gains `Expired`.
- New `SeatClasses` table; `Seats.SeatType` replaced by a `SeatClassId` foreign key.
- `TripSeatPrices` → renamed to `Fares` with `SeatClassId` foreign key (replacing the free-text `SeatTypeId`) and a unique index on (train trip, seat class).
- `Payments.Status` values `Pending|Succeeded|Failed|Reversed`; add `ReversedAt`.
- New `Refunds` table (booking, ticket, ticket price, fee, refunded amount, created at).
- `Carriages.Type` stays descriptive only; it never affects price.
- Seed data updated to match. The scaffolded EF entities and `AppDbContext` follow the schema.

## Testing Decisions

- **One seam: the HTTP API.** Tests call the real endpoints through `WebApplicationFactory`, against a dedicated MySQL **test** database (never the dev/real one), created from `init.sql` and reset between tests. This exercises domain rules, handlers, EF mapping and the unique index together.
- The application's `TimeProvider` is replaced with a fake clock in tests, so hold expiry and fee boundaries are tested by moving time, not by sleeping.
- A good test states a customer-visible scenario and checks only HTTP responses and what later requests return (status codes, booking status, refund amounts, seat availability). Tests must not inspect internal classes, private state or SQL.
- Scenarios to cover at minimum: book → pay → confirmed; hold expiry releases seats and blocks late payment (reversal); duplicate payment reversed; same transaction id twice recorded once; wrong amount rejected; cancel at 24 h, 23 h 59 m, 4 h, 3 h 59 m, after departure; round trip with outbound used and return cancelled ends completed; cancelling all tickets ends cancelled; cancelled seat bookable again; two concurrent bookings for one seat → one 201, one 409; customer cannot touch another customer's booking; same passenger twice on one trip allowed.
- New test project (xUnit). There is no prior art in the repo; this spec establishes it.

## Out of Scope

- Selling segments of a train trip (ADR 0001).
- Counter sales by staff, walk-in customers without accounts, cash payments.
- Train trip cancellation or delay by the operator and their effect on tickets; changing train trip status.
- Passengers without an identity document number (e.g. young children).
- Real authentication/authorization and a real payment provider integration.
- Redis caching of any of the new reads.
- Exchanging a ticket for another seat or trip.

## Further Notes

- Assumption not discussed in grilling: fees and refunds are rounded to whole đồng, rounding the fee down (in the customer's favour). Change in the fee policy if the business says otherwise.
- Assumption: exactly 24 h before departure gets the 10 % rate and exactly 4 h gets the 20 % rate (cutoff is strictly less than 4 h).
- The `Staff` role remains unused by this spec.
- The Polly `BookingPipeline` registered in the API is not required by this spec.
