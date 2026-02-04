Feature Flags Engine

A runtime feature management system implemented using .NET 10, C#, SQL Server, and Angular 21.
It provides a production-grade feature flag engine supporting:

✅ Global defaults
✅ User-based overrides
✅ Group-based overrides
✅ Region-based overrides
✅ Idempotent API semantics
✅ Snapshot-based in-memory evaluation
✅ Strict precedence rules
✅ Admin UI for evaluating, managing features, overrides & engine status

📘 Overview

Feature Flags (a.k.a Feature Toggles) allow teams to control system behavior at runtime, without redeploying code.
They enable:

Gradual rollouts

Canary releases

Region-based feature rollout

User/group experiments

Emergency killswitches

Safe operational toggling

This implementation includes:

A clean domain model (Aggregate Roots, Value Objects, Validators)

A database-backed repository layer (EF Core 10)

Cached in-memory snapshot store for fast evaluations

Strict override precedence (User → Group → Region → Default)

Fully versioned REST API

Extensive test coverage (Core, API, Infrastructure)

Angular admin UI to view, create, update, override & evaluate features

🧱 Tech Stack
Backend

.NET 10

C# 12

Entity Framework Core 10

SQL Server 2022

ASP.NET Minimal Hosting Model

ASP.NET API Versioning 7

Swagger / OpenAPI

xUnit + FluentAssertions

Frontend

Angular 21 (Standalone Components)

TypeScript 5.9

Angular Forms + HttpClient

Proxy configuration for API routing

🏛️ Architecture & Design

This project follows Clean Architecture principles:

├── src
│   ├── FeatureFlags.Core           (Domain)
│   │   ├── Domain Models
│   │   ├── Validation
│   │   ├── Errors
│   │   └── Evaluation Engine
│   │
│   ├── FeatureFlags.Infrastructure (Persistence & Caching)
│   │   ├── EF Core DB Context
│   │   ├── Entities
│   │   ├── Repositories
│   │   ├── Snapshot Loader
│   │   └── Cached Feature Store
│   │
│   ├── FeatureFlags.Api            (API Layer)
│   │   ├── Controllers
│   │   ├── DTOs
│   │   ├── Middleware
│   │   ├── DI Extensions
│   │   └── Swagger/OpenAPI
│   │
│   ├── FeatureFlags.Tests          (Unit Tests)
│
└── ui
    └── feature-flags-admin         (Angular UI)

🧠 SOLID Principles Adopted
Principle	Implementation
S – Single Responsibility	Every class handles one responsibility: evaluator, repository, snapshot loader, validators, controllers
O – Open/Closed	Evaluator is closed to modification but open for extension (more override types possible)
L – Liskov Substitution	Interfaces & abstractions properly enforced
I – Interface Segregation	Separate repositories: IFeatureFlagRepository, IFeatureOverrideRepository, IFeatureFlagStore
D – Dependency Inversion	Core layer never references Infrastructure; DI injects abstractions
🧩 Design Patterns Used
Pattern	Usage
Repository Pattern	Clean persistence logic
Unit of Work	Coordinated SaveChangesAsync() across repositories
Factory / Mapper	Entity ↔ Domain conversions
Strategy	Evaluator precedence strategy for overrides
Gateway Pattern	Snapshot loader reads DB → Cached store
Decorator (implicit)	Exception middleware wraps request pipeline
⚡️ Key Features
1️⃣ Fast Runtime Evaluation

The Evaluator performs zero I/O — all lookups are in-memory.

2️⃣ Snapshot-Based Refresh

Any mutation triggers:
POST /api/v1/admin/feature-flags/refresh
→ Rebuilds entire in-memory store atomically.

3️⃣ Strict Override Precedence
User Override
  ↓
Group Override
  ↓
Region Override
  ↓
Global Default

4️⃣ Idempotent API Design

POST /features → idempotent-by-key

PUT and DELETE → naturally idempotent

Conflicts return correct 409 ProblemDetails

5️⃣ Angular Admin UI

List features

Create/update/delete

Evaluate via API

Manage overrides

Check engine status

Refresh snapshot

📦 Setup Instructions
1. Clone Repository
git clone https://github.com/jayaram-pullati/feature-flags-engine
cd feature-flags-engine

2. Setup Backend
Install .NET 10

https://dotnet.microsoft.com/download

Install SQL Server (local or Docker)

Docker option:

docker compose up -d

Apply Migrations
dotnet ef database update -p src/FeatureFlags.Infrastructure -s src/FeatureFlags.Api

Run API
cd src/FeatureFlags.Api
dotnet run


API runs at:

http://localhost:5042
https://localhost:7215


Swagger UI:

http://localhost:5042/swagger

▶️ Running Tests
Run Backend Tests
cd src/FeatureFlags.Tests
dotnet test


Test suite includes:

✔ Core evaluator
✔ Domain validators
✔ Repository tests (in-memory SQLite)
✔ API tests with mocks
✔ Middleware tests

🖥️ Running Angular UI
1. Install dependencies
cd ui/feature-flags-admin
npm install

2. Start UI
npm start


Opens:

http://localhost:4200


UI talks to backend via Angular proxy (proxy.conf.json).

📡 Example API Requests
Create Feature (Idempotent POST)
POST /api/v1/features
{
  "key": "dark-mode",
  "defaultState": true,
  "description": "UI theme"
}

Update Feature
PUT /api/v1/features/dark-mode
{
  "defaultState": false,
  "description": "Temporary disable"
}

Upsert Override
PUT /api/v1/features/dark-mode/overrides/user/u123
{
  "state": true
}

Evaluate
POST /api/v1/evaluate/dark-mode
{
  "userId": "u123",
  "groupIds": ["beta"],
  "region": "IN"
}

📝 Assumptions & Tradeoffs
Assumptions

Feature keys are globally unique

Overrides use normalized IDs (case-insensitive)

Region codes must be uppercase (ISO-like)

Evaluator should never hit the database (only cached store)

Tradeoffs (Intentional)

1. No authentication yet
→ UI & API open for simplicity (future-ready).

2. Overrides list UI is local-only
Backend intentionally does not expose “list overrides per feature”.
(Left as “next step” to avoid over-scope.)

3. Snapshot refresh is explicit
On each mutation, UI calls POST /refresh.
Keeps evaluator extremely fast and loosely coupled from persistence.

🚀 What’s Next (If we had more time)
🔐 Authentication & Authorization

OAuth2 + Bearer tokens

Role-based admin access

Multi-tenant feature flag access

📋 Advanced UI

Material UI or Fluent UI table

Search, filters, paging

Override history

Feature analytics dashboard

🗃 DB Improvements

Auditing table (who changed what)

Caching invalidation events

Outbox pattern for distributed refresh

☁️ Cloud Extensions

Redis-backed cache for multi-instance API

Global region-based flags (CDN-level propagation)

📈 Full Feature Flag SDK

.NET client library

JavaScript SDK

Streaming updates (push mode)

📦 Final Notes

This project demonstrates production instincts:

Clean architecture

Perfect separation of concerns

Predictable evaluation logic

Fast & scalable design

Strong tests

Meaningful commit history

High-quality domain modeling

Strict idempotency and problem-details handling
