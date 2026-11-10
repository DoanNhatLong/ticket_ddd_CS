# Frontend Rules (web/)

These rules apply to the React frontend in `web/`. The root `CLAUDE.md` still applies
(priorities, deferred testing, task estimates, language rules); its .NET-specific rules
(DDD layers, `dotnet build`, MySQL) do not apply here.

## Stack
- Vite + React + TypeScript (strict). No other framework.
- Do NOT add libraries (UI kits, CSS frameworks, state managers, routers, HTTP clients, date libraries)
  without asking the user first. Prefer plain React, `fetch` and CSS.

## Commands (run inside `web/`)
- Dev server: `npm run dev` → http://localhost:5173
- After changing code, run `npm run build` and `npm run lint` and fix errors before reporting the task as done.
- Do not run `npm audit fix` or upgrade packages without asking.

## Structure (`web/src/`)
- `api/`: API clients and their TypeScript types. All calls go through `apiFetch` in `api/http.ts`;
  never call `fetch` directly from components.
- `components/`: one component per file, named after the component (`SeatTile.tsx`).
  Every component declares its props as `interface <Name>Props`.
- `hooks/`: custom hooks (`useXxx.ts`) for data loading and other reusable state.
- `utils/`: pure helpers such as formatters (`utils/format.ts`).
- `booking/` (and similar feature folders): feature-specific types and helpers that are not components.
- `App.tsx`: holds page-level state and composes components; no large markup or business rules there.

## Conventions
- Data flows down through props; actions flow up through callbacks (`onSelect`, `onChange`, `onSubmit`).
- Use the domain vocabulary from the root `GLOSSARY.md` in names and UI text
  (Booking, Ticket, Passenger, Seat hold, Seat class, Fare, Booking total, ...).
- Handle API errors by their stable `code` (e.g. `Booking.SeatTaken`) via `ApiError`, not by parsing messages.
- Times are shown in UTC (the API stores UTC) until the user decides otherwise.
- Colors come from the CSS variables in `src/index.css`; every new style must work in both light and dark mode.
- Keep it accessible: real `<button>`/`<label>` elements, keyboard support for clickable items.

## Talking to the API
- In development, Vite proxies `/api` to `http://localhost:7777` (see `vite.config.ts`); use relative `/api/...` paths.
- The customer is identified by the `X-Customer-Id` header (passed via `apiFetch`'s `customerId` option)
  until real authentication exists.
- If an endpoint or field the UI needs does not exist, say so and propose the API change; do not fake data in the UI.

## Boundaries
- No business rules in the frontend: seat hold expiry, cancellation fees, refunds, booking/ticket status
  and fare selection are decided by the API. The UI only displays what the API returns and may pre-check
  obvious input (e.g. required fields) for convenience.
