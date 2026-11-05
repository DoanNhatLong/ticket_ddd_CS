# Tickets cover the whole train trip, no segment sales

A train trip has exactly one departure station and one arrival station, and every ticket is valid for the whole trip; a seat taken on a trip is taken from departure to arrival. We chose this over selling segments (boarding and alighting at intermediate stops, with the same seat resold on non-overlapping legs) because segment sales need a stop list per trip, overlap-based seat availability, and per-segment pricing, which is far more complexity than the project needs before its core booking flow works.

## Consequences

- A real-world line such as Hanoi → Vinh → Da Nang → Saigon must be modelled as one trip per sold origin/destination pair, or only its end-to-end trip is sellable.
- Seat availability is a simple "is there an active ticket or seat hold for this seat on this trip" check.
- Moving to segment sales later means changing the ticket, the train trip, seat availability, and pricing together.
