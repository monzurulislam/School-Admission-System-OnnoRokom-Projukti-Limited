Assumptions and Design Decisions

This file explains the assumptions I made and the design choices I used where the assignment was not clear.


1. Many Backend Processes at the Same Time (Bonus 1.6)

Requirement:
"একই database ব্যবহার করে backend-এর দুইটা process আলাদা port-এ একসাথে চললেও held + confirmed সংখ্যা totalSeats-এর বেশি হবে না। কীভাবে নিশ্চিত করলেন, ASSUMPTIONS.md-এ লিখবেন।"

How I made sure of this:

1. Database row lock (SELECT ... FOR UPDATE)
- Locks inside one process (like a C# lock, Monitor, or SemaphoreSlim) are not enough. Two backend processes or two Docker containers can use the same PostgreSQL database at the same time.
- When a hold request comes in, HoldService.CreateHoldAsync starts a database transaction (IsolationLevel.ReadCommitted) and runs this query:

SELECT "Id" FROM classes WHERE "Id" = @classId FOR UPDATE;

- This locks the row of that class in PostgreSQL. Any other process that wants a seat in the same class must wait in the database until the transaction ends.

2. Seat check while the lock is held
- Under the lock, the system counts active holds (status = 'HELD' AND expires_at > NOW()) and confirmed seats (status = 'CONFIRMED').
- If activeHeld + confirmed is equal to or more than totalSeats, the system returns 409 Conflict and releases the lock. Nothing is saved.
- If a seat is free, the new Hold is saved, the transaction is committed, and the lock is released.

3. In-memory fallback for tests
- For fast unit tests with an in-memory database, HoldService also uses a ConcurrentDictionary<string, SemaphoreSlim>. This way the concurrency tests pass even without a real database server.


2. Hold Expiry (Section 1.5, Rule 2)

Assumption:
A hold ends after HOLD_DURATION_SECONDS (default 600 seconds). The seat must go back to the pool without any manual API call.

My design:

1. Check at query time
- I do not depend only on a scheduled job. Every seat count (GET /classes and POST /classes/{id}/holds) uses this condition:

WHERE status = 'HELD' AND expires_at > NOW()

- As soon as expires_at is passed, the seat is free for new students. No API call is needed.

2. Background cleanup
- A background service (HoldCleanupBackgroundService, an IHostedService) runs every 15 seconds. It updates rows that are still marked HELD but are already expired. This keeps the table clean and the indexes small.


3. Student ID and Login (Sections 1.3 and 1.4)

Assumptions:

1. No normal login
- As the assignment says, there is no registration, login, or password.
- The student sends the 17-digit Birth Registration Number in the X-Student-Id header.
- On the frontend, the number is saved in the browser localStorage. It stays after a page reload or browser restart.

2. Format check
- The header must not be empty and must match exactly 17 digits (^[0-9]{17}$).
- If it is wrong, the API returns 400 Bad Request with a clear message.


4. Class and Shift (Section 1.3)

Assumption:
- Each shift is a separate class with its own seat count.
- Example: "Class 6 - Morning Shift" (10 seats) and "Class 6 - Day Shift" (15 seats) are two rows in the classes table. Each has its own ID (starting with c_).


5. Payment and Confirmation (Section 1.3 and Section 1.5, Rule 5)

Assumptions:

1. Fake payment
- The "Pay Admission Fee" ("ভর্তি ফি দিন") button works as a successful payment right away. I did not connect any real payment gateway.

2. Owner check
- Only the student who made the hold can confirm it. The system checks this with X-Student-Id.

3. Safe to repeat
- If POST /holds/{holdId}/confirm is called many times for the same hold, it returns 200 OK with status CONFIRMED. The confirmed seat count does not go up again.


6. Live Seat Updates (Section 1.5, UI Rules 9 and 10)

My design:

1. Polling
- The UI calls GET /classes every 3 seconds, so seat numbers stay fresh without a page reload.

2. Countdown on the client
- For an active hold, the UI checks the remaining time every second. When it reaches 0, the status changes to EXPIRED on the screen right away, and the confirm button is disabled. It does not wait for the server.