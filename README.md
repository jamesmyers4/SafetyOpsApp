# SafetyOps

A full-stack workplace-safety app: an ASP.NET Core Web API (.NET 10, C#) with a React + TypeScript (Vite) frontend. It covers personnel records, training classes, medical surveillance appointments, and incident reports, and it doubles as the system under test for the companion end-to-end suite, [SafetyOpsTests-Playwright](https://github.com/jamesmyers4/SafetyOpsTests-Playwright).

> **Status:** work in progress.

## Tech stack

| Layer | Technology |
| ----- | ---------- |
| Backend | ASP.NET Core Web API, C#, .NET 10 |
| Frontend | React 19, TypeScript, Vite, react-router |
| Data | Entity Framework Core 10 + SQLite |

## Project structure

```text
SafetyOps.slnx
SafetyOps.Api/            ASP.NET Core Web API
  Domain/                 Entities
  Data/                   DbContext, configurations, migrations, demo-data seeder
  Features/<Module>/      Contracts (DTOs), service, controller per module
  Program.cs              App bootstrap; serves the built SPA in production
safetyops-web/            React + TypeScript SPA (Vite)
  src/services/api.ts     Fetch wrapper for every API call
  src/pages/              Route-level components
  vite.config.ts          Dev server; proxies /api to the API
```

## API endpoints

Every endpoint except sign-in requires the auth cookie and returns `401` (never a redirect) without it. Resource lists are paged (`?search=&page=1&pageSize=25`, max 100) and return `{ items, page, pageSize, totalCount, totalPages }`. Errors use [RFC 9457 problem details](https://www.rfc-editor.org/rfc/rfc9457); validation failures return `400` with an `errors` map. Dates are ISO 8601 (`yyyy-MM-dd`). In Development, interactive docs are at `/scalar` and the OpenAPI document at `/openapi/v1.json`.

| Method | Route | Success |
| ------ | ----- | ------- |
| POST | `/api/auth/login` | 200 + HttpOnly auth cookie (anonymous) |
| GET | `/api/auth/me` | 200 current user, 401 if signed out |
| POST | `/api/auth/logout` | 204 |
| GET | `/api/personnel` | 200 paged |
| POST | `/api/personnel` | 201 + `Location` |
| GET / PUT | `/api/personnel/{id}` | 200 |
| DELETE | `/api/personnel/{id}` | 204 (409 if the person has appointments) |
| GET | `/api/training/classes` | 200 paged |
| POST | `/api/training/classes` | 201 + `Location` |
| GET / PUT | `/api/training/classes/{id}` | 200 |
| DELETE | `/api/training/classes/{id}` | 204 |
| GET | `/api/training/courses` | 200 |
| GET | `/api/medical-surveillance/appointments` | 200 paged |
| POST | `/api/medical-surveillance/appointments` | 201 + `Location` |
| GET / PUT | `/api/medical-surveillance/appointments/{id}` | 200 |
| DELETE | `/api/medical-surveillance/appointments/{id}` | 204 |
| GET | `/api/medical-surveillance/persons` | 200 |
| GET | `/api/medical-surveillance/work-tasks` | 200 |
| GET | `/api/incidents` | 200 paged (`?status=&category=` filters) |
| POST | `/api/incidents` | 201 + `Location` |
| GET / PUT | `/api/incidents/{id}` | 200 |
| DELETE | `/api/incidents/{id}` | 204 |
| GET | `/api/personnel/lookup` | 200 (id/name options) |
| GET | `/api/access/org-units` | 200 (units you can see, with your role) |
| GET / POST | `/api/access/assignments` | 200 / 201 (Admin) |
| DELETE | `/api/access/assignments/{id}` | 204 (Admin; not your own) |
| GET | `/api/access/users` | 200 (Admin) |

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

Open <https://localhost:14418> and sign in with one of the demo accounts:

| User | Password | Role | Sees |
| ---- | -------- | ---- | ---- |
| `admin` | `admin` | Admin on the whole organization | Everything; can grant and revoke roles |
| `manager` | `manager` | Manager on Manufacturing Division | Manufacturing, North Plant, and South Plant records; can create, edit, delete |
| `viewer` | `viewer` | Viewer on North Plant | North Plant records, read-only |

The demo accounts come from `Auth:DemoUsers` in `appsettings.Development.json` and are stored with hashed passwords on first run; other environments supply accounts through configuration (environment variables or user secrets).

In Visual Studio, open `SafetyOps.slnx` and run `SafetyOps.Api`; the SPA proxy starts the Vite dev server for you.

### Run it with Docker

```bash
docker compose up --build
```

Open <http://localhost:8080> and sign in with the demo accounts above. The image builds the SPA in a Node stage, publishes the API with the SPA in `wwwroot`, and runs as a non-root user on port 8080 with demo data seeded. The SQLite database lives on the `safetyops-data` volume, so data survives restarts; `docker compose down -v` resets it. `GET /healthz` reports liveness.

## Access control

Records (people, training classes, incidents) belong to a unit in an organization tree: the organization, two divisions, and two sites under each. Appointments belong to the unit of the person evaluated. A role (Viewer, Manager, Admin) is granted on a unit and applies to that unit and everything below it. The API enforces this on every endpoint: records outside your units return `404`, read-only records return `403` on change, and Admins manage roles on the **Access Levels** page (`/api/access/*`). Permissions are looked up on each request, so a grant or revoke applies immediately.

## License

[MIT](LICENSE)
