# SafetyOps

A full-stack workplace-safety app: an ASP.NET Core Web API (.NET 10, C#) with a React + TypeScript (Vite) frontend. It covers personnel records, training classes, and medical surveillance appointments, and it doubles as the system under test for the companion end-to-end suite, [SafetyOpsTests-Playwright](https://github.com/jamesmyers4/SafetyOpsTests-Playwright).

> **Status:** work in progress. Data is currently held in memory and resets when the API restarts. Persistence, validation, authentication, and tests are being added.

## Tech stack

| Layer | Technology |
| ----- | ---------- |
| Backend | ASP.NET Core Web API, C#, .NET 10 |
| Frontend | React 19, TypeScript, Vite, react-router |
| Data | In-memory (per process) |

## Project structure

```text
SafetyOps.slnx
SafetyOps.Api/            ASP.NET Core Web API
  Controllers/            Auth, Personnel, Training, MedicalSurveillance
  Program.cs              App bootstrap; serves the built SPA in production
safetyops-web/            React + TypeScript SPA (Vite)
  src/services/api.ts     Fetch wrapper for every API call
  src/pages/              Route-level components
  vite.config.ts          Dev server; proxies /api and /auth to the API
```

## API endpoints

| Method | Route | Description |
| ------ | ----- | ----------- |
| POST | `/auth/login` | Demo login |
| GET | `/auth/check` | Is the auth cookie present |
| POST | `/auth/logout` | Clear the auth cookie |
| GET | `/api/personnel/users?search=` | List or search personnel |
| POST | `/api/personnel/create` | Create a personnel record |
| GET / PUT / DELETE | `/api/personnel/users/{id}` | Read, update, delete a personnel record |
| GET / POST | `/api/training/classes` | List or create training classes |
| GET / PUT / DELETE | `/api/training/classes/{id}` | Read, update, delete a class |
| GET | `/api/training/courses` | Course catalog |
| GET / POST | `/api/medical-surveillance/appointments` | List or create appointments |
| GET / PUT / DELETE | `/api/medical-surveillance/appointments/{id}` | Read, update, delete an appointment |
| GET | `/api/medical-surveillance/persons` | People available to evaluate |
| GET | `/api/medical-surveillance/work-tasks` | Work tasks and their stressors |

## Getting started

Prerequisites: .NET 10 SDK and Node.js 20+.

```bash
# Terminal 1: API on https://localhost:7121
dotnet run --project SafetyOps.Api --launch-profile https

# Terminal 2: frontend on https://localhost:14418
cd safetyops-web
npm install
npm run dev
```

Open <https://localhost:14418> and sign in with the demo account `admin` / `admin`.

In Visual Studio, open `SafetyOps.slnx` and run `SafetyOps.Api`; the SPA proxy starts the Vite dev server for you.

## License

[MIT](LICENSE)
