# Train Booking

Selling train tickets to customers: searching trips, holding seats, paying, and cancelling.

## Language

### People

**Customer**:
A person with an account who places a booking and owns it; the only one who may cancel its tickets and the one who receives refunds. Every booking belongs to exactly one customer.
_Avoid_: Buyer, client, account holder

**Passenger**:
The person who travels on a ticket, identified only by name and identity document; has no account and no rights over the booking. One passenger may hold several tickets on the same train trip (e.g. a second seat for extra space). Every passenger must have an identity document number; passengers without one (such as young children) are not supported.
_Avoid_: Traveller, guest, rider

### Booking

**Booking**:
One purchase made by a customer in a single checkout, grouping one or more tickets under a single booking code; the tickets may be for different train trips (e.g. an outbound and a return trip).
_Avoid_: Order, reservation

**Ticket**:
The right for one passenger to occupy one seat on one train trip; the unit that can be cancelled on its own within a booking.
_Avoid_: Booking item, line item, seat booking

**Ticket cancellation**:
Withdrawing a single ticket from a booking while the other tickets in the same booking stay valid. Cancelling a whole booking means cancelling every ticket in it.
_Avoid_: Partial refund, booking cancellation (when only some tickets are affected)

**Booking total**:
The amount charged for a booking at checkout; it never changes afterwards, even when tickets are cancelled.
_Avoid_: Current total, remaining amount

**Seat hold**:
The temporary claim an unpaid booking has on its seats, so no one else can buy them until the booking is paid or the hold runs out.
_Avoid_: Lock, reservation, block

**Expired booking**:
A booking whose seat hold ran out before payment; its seats go back on sale and it can never become paid. Distinct from a cancelled booking, which the customer ended on purpose.
_Avoid_: Timed-out booking, auto-cancelled booking

**Confirmed booking**:
A paid booking that still has at least one active ticket.
_Avoid_: Paid booking, successful booking

**Cancelled booking**:
A paid booking in which every ticket has been cancelled.
_Avoid_: Void booking, refunded booking

**Completed booking**:
A paid booking with no active tickets left and at least one used ticket. Together with confirmed and cancelled, it covers every paid booking with no overlap.
_Avoid_: Closed booking, finished booking

**Active ticket**:
A ticket that is paid for, not cancelled, and whose train trip has not finished yet.
_Avoid_: Valid ticket, open ticket

**Used ticket**:
A ticket that was not cancelled and whose train trip has finished.
_Avoid_: Consumed ticket, completed ticket

### Pricing

**Seat class**:
The kind of place a seat offers (e.g. soft seat, lower berth in a four-berth cabin); every seat belongs to exactly one seat class, and it is the only thing that sets a seat's price. The carriage a seat is in does not affect price.
_Avoid_: Seat type, carriage type, ticket class

**Fare**:
The price a train trip asks for one seat class; each train trip sets its own fares, and they may change before departure.
_Avoid_: Trip seat price, base price, rate

**Ticket price**:
The fare that applied when the ticket was booked, fixed on the ticket for good; refunds and cancellation fees are calculated from it, never from the current fare.
_Avoid_: Current price, seat price

### Payment

**Payment**:
One attempt by a customer to pay a booking's total in full; a booking may have several payments but at most one successful one, and that one confirms the booking.
_Avoid_: Transaction, charge, partial payment

### Money back

**Refund**:
Money returned to the customer as the result of one ticket cancellation; equals the ticket price minus the cancellation fee.
_Avoid_: Negative payment, chargeback (and never call it a payment reversal, which is a different thing)

**Payment reversal**:
Returning the full amount of a successful payment that the booking did not need: a duplicate payment, or a payment that arrived after the booking expired. It carries no fee and is not tied to any ticket.
_Avoid_: Refund, double-payment refund, rollback

**Cancellation fee**:
The portion of a ticket's price kept by the seller when the ticket is cancelled, determined by how long before the train trip departs the cancellation happens.
_Avoid_: Penalty, refund fee

**Cancellation cutoff**:
The moment before a train trip's departure after which its tickets can no longer be cancelled.
_Avoid_: Deadline, lock time

### Travel

**Train trip**:
One scheduled run of a train from one departure station to one arrival station, with a fixed departure and arrival time; a ticket always covers the whole train trip.
_Avoid_: Journey, schedule, departure

**Departure time**:
When a train trip leaves its departure station; the reference point for cancellation fees and the cancellation cutoff.
_Avoid_: Start time, boarding time
