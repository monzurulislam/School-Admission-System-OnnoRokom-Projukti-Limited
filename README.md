School Admission System

An online school admission system where seats are given first-come, first-served (FCFS). It can handle many users at the same time. It is built with .NET 8 Web API, PostgreSQL, React (TypeScript and Vite), and Docker Compose.


Quick Start with Docker

The whole system (database, backend, and frontend) runs with one command. You do not need to install any SDK.

docker compose up --build

URLs after it starts:
- Frontend: http://localhost:3000
- Backend API: http://localhost:8080
- Swagger API docs: http://localhost:8080/swagger
- Health check: http://localhost:8080/health

To stop the system and keep all admission data:

docker compose down

(Data is saved in the Docker volume named postgres_data.)


Running the Tests

The project has unit tests, hold lifecycle tests, and one stress test with 50 applicants at the same time. The stress test checks that held seats plus confirmed seats never go above total seats.

Run tests with the .NET CLI:

dotnet test backend/SchoolAdmission.sln

Run tests in Docker (no local .NET SDK needed):

docker run --rm -v ${PWD}/backend:/app -w /app mcr.microsoft.com/dotnet/sdk:8.0 dotnet test SchoolAdmission.sln


Main Features

1. Safe seat booking when many users apply together
- Row locking: I use SELECT ... FOR UPDATE in PostgreSQL. Even if thousands of students apply at the same time, only the available seats are given.
- Works with many servers (Bonus 1.6): The lock is in the database, so many backend instances on different ports can share the same data safely.
- In-memory fallback: For tests that use an in-memory database, a SemaphoreSlim lock keeps the code thread-safe.

2. Automatic hold expiry
- The hold time is set with the HOLD_DURATION_SECONDS environment variable. The default is 600 seconds (10 minutes).
- When data is read, the system checks if a hold is still valid (status = 'HELD' and expires_at > NOW()). Expired holds go back to the available seats automatically.
- A background service (HoldCleanupBackgroundService) runs from time to time and updates expired holds in the database.

3. Student rules and safe payment
- Student ID: The X-Student-Id is a 17-digit birth registration number. The system checks that it has exactly 17 digits.
- One active application: A student cannot have more than one active hold or confirmed admission in the same class.
- Safe payment confirmation: If the student clicks many times or the network retries, the seat is confirmed only once.

4. React frontend
- Student ID bar: A box at the top for the 17-digit birth registration number. It is saved in localStorage.
- Auto refresh: Seat numbers update every 3 seconds without reloading the page.
- Live countdown: An active hold shows a countdown timer. When time is over, the status changes to EXPIRED and the Pay button is disabled. No page reload is needed.
- Admin class creation: A simple pop-up form to create new classes and shifts.
- Bengali and English errors: Error messages are clear in both languages. There are no blank screens or raw error dumps.


API Summary

POST /classes
Create a new class.

GET /classes
Show all classes with seat numbers.

GET /classes/{classId}
Show details and seat breakdown for one class.

POST /classes/{classId}/holds
Needs X-Student-Id. Hold a seat for a short time. Returns 409 if the class is full.

GET /holds/{holdId}
Show the hold status: HELD, CONFIRMED, EXPIRED, or CANCELLED.

POST /holds/{holdId}/confirm
Needs X-Student-Id. Confirm admission and simulate payment.

DELETE /holds/{holdId}
Needs X-Student-Id. Cancel an active hold and free the seat.

GET /me/holds
Needs X-Student-Id. Show all holds and applications of the student.