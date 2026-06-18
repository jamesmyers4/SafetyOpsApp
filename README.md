SafetyOpsApp

A full-stack web application simulating an enterprise safety and fire management system. Built as a local development and test target for SafetyOpsTests, this project demonstrates a modern ASP.NET Core Web API backend paired with a React/TypeScript frontend, with real data persistence via Entity Framework Core and SQLite.


Tech Stack

LayerTechnologyBackendASP.NET Core Web API / C# / .NET 10FrontendReact 18 + TypeScript (Vite)ORMEntity Framework Core + SQLiteRouting (client)react-router-domHTTP ClientFetch API with credentials: 'include'IDEVisual Studio Community 2022


Project Structure

SafetyOpsApp/
├── EsamsClient.Server/         # ASP.NET Core Web API
│   ├── Controllers/
│   │   └── PersonnelController.cs   # CRUD endpoints for user/personnel records
│   ├── Data/
│   │   └── AppDbContext.cs          # EF Core DbContext with User entity
│   ├── Models/
│   │   └── User.cs                  # Personnel data model
│   └── Program.cs                   # App bootstrap, CORS, EF config
│
└── esamsclient.client/         # Vite + React + TypeScript frontend
    ├── src/
    │   ├── services/
    │   │   └── api.ts               # Centralized HTTP layer
    │   ├── pages/                   # Route-level components
    │   └── main.tsx                 # App entry point with react-router-dom
    └── vite.config.ts


API Endpoints

MethodRouteDescriptionGET/api/personnelRetrieve all personnel recordsPOST/api/personnelCreate a new personnel recordDELETE/api/personnel/{id}Delete a personnel record by ID


Getting Started

Prerequisites


.NET 10 SDK
Node.js 18+
Visual Studio Community 2022 with the ASP.NET and Node.js workloads


Run the Backend

bashcd EsamsClient.Server
dotnet run

The API will start on https://localhost:7075.

Run the Frontend

bashcd esamsclient.client
npm install
npm run dev

The React client will start on https://localhost:14418 (or https://localhost:5173 in standalone Vite mode).

Database

EF Core with SQLite handles persistence automatically. On first run, the database is created and migrations are applied. No additional setup required.


CORS Configuration

The backend allows cross-origin requests from both frontend origins:


https://localhost:5173 (standalone Vite)
https://localhost:14418 (VS-hosted client)


Session cookies are passed with credentials: 'include' on all fetch calls from the frontend.


Purpose

This application exists as a realistic local test target for the SafetyOpsTests C# NUnit/Playwright suite. The domain — personnel and safety record management — is modeled after enterprise safety systems used in regulated environments including federal and DoD contexts.


Related Repository

SafetyOpsTests — The NUnit + Playwright test suite that runs against this application.


Author

James Myers — QA Engineer / SDET
