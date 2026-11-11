AI Usage Disclosure

This is a short, honest note on how I used AI while building the School Admission System, where it helped, and where it got things wrong.


1. Tool I Used

- Tool: Google Antigravity (Gemini 3.8 Flash model)
- What I used it for: Planning the architecture, generating the initial project boilerplate, sketching REST endpoints, scaffolding unit and concurrency tests, and laying out the React UI components.


2. Where AI Helped Me

. Understanding the requirements: I used AI to break the assignment brief into clear constraints and turn them into a phased roadmap (PLAN.md).

. Preventing seat overselling in PostgreSQL: I discussed locking strategies with the AI and settled on a pessimistic row-level lock (SELECT ... FOR UPDATE). It makes sure no seats are oversold, even with multiple server processes running (Bonus 1.6).

. Writing the concurrency test: AI scaffolded a test that fires 50 simultaneous seat requests using Parallel.ForEachAsync. I used it to check that the seat count stays correct under load.

. Building the React UI: It helped me design clean Tailwind CSS components with Bangla and English feedback messages, a countdown timer, and automatic polling to refresh data.


3. Where AI Got It Wrong and What I Did About It

Mistake 1: Wrong package versions
- When I added the PostgreSQL and EF Core Design packages, the tool picked a preview version (10.0.3). It does not work with net8.0.
- The build failed with error NU1202.
- I set the stable version 8.0.11 in both project files (SchoolAdmission.Api.csproj and SchoolAdmission.Tests.csproj).

Mistake 2: Wrong port in the frontend proxy
- The Vite proxy sent /api calls to port 5058.
- I checked the requirement (1.2). It says the backend must run on http://localhost:8080.
- I changed the port to 8080 in vite.config.ts and launchSettings.json.

Mistake 3: No way to test without Docker
- The AI setup only worked with Docker. My Docker Desktop was stopped, so I could not test the UI.
- I added an in-memory database for development in Program.cs and appsettings.Development.json. In production (Docker Compose), the app uses PostgreSQL 16 through DATABASE_URL.

Mistake 4: Hold countdown was slow to update
- When a seat hold expired, the screen changed only on the next polling. The Pay button stayed active for a short time.
- I tested it against §1.5 Rule 9. It says the change must be instant.
- I added a timer in HoldCountdown.tsx that checks the remaining time every second. When time is over, the state changes to EXPIRED and the Pay button is disabled. No page reload is needed.


4. Takeaway

AI saved me a lot of time on boilerplate and test setup. But I had to check everything it wrote. I found the mistakes through build errors, by reading the requirements again, and by running the app myself.