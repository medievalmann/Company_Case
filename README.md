This is a backend case project originally prepared for a specific company. The name has been changed to "Company Case" and shared here with the company's knowledge. I've uploaded it for reference purposes and in the hope that others may benefit from the solution or the implementation approach.

# Service Provider Ratings & Notifications

This project is a microservice-based backend system developed with .NET 8 that allows customers to rate service providers, and notifies the providers of new ratings in real-time.

>📘 **Extended Technical Documentation:**  
>Contains detailed design rationale, trade-off analyses, architectural patterns, testing strategies, and future improvement plans.  
>👉 [View Full Technical Document](https://docs.google.com/document/d/17Zxt9uo2vxTyY8hYmB9jAw5AntVR30-8Kfv0ktC0IQw/edit?usp=sharing)


---

## 🧩 Services

- **RatingService.API**  
  Accepts customer ratings and calculates average rating per provider. Publishes a notification event on each new rating.

- **NotificationService.API**  
  Listens to rating events and stores notifications in-memory. Provides an endpoint for service providers to fetch their new notifications.

---

## 🚀 Getting Started

### 1. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/)
- [Docker](https://www.docker.com/) & Docker Compose

---

### 2. Running with Docker (Recommended)

```bash
docker-compose -f Company_Case/docker-compose.yml up --build
```

> ⏱️ This will spin up:
> - `RatingService` on http://localhost:5000
> - `NotificationService` on http://localhost:5010
> - PostgreSQL (port 5432)
> - RabbitMQ (port 5672 + management UI on 15672)

To stop the services:

```bash
docker-compose -f Company_Case/docker-compose.yml down
```

---

### 3. Running Locally Without Docker

1. Start PostgreSQL and RabbitMQ manually or with Docker
2. Update connection strings in `appsettings.json` if needed
3. Run each service:

```bash
cd Company_Case/RatingService.API
dotnet run

cd Company_Case/NotificationService.API
dotnet run
```

---

## 📬 API Endpoints

### RatingService (http://localhost:5000)

- `POST /api/ratings` – Submit a rating
- `GET /api/ratings/average/{providerId}` – Get average score

### NotificationService (http://localhost:5010)

- `GET /api/notifications/{providerId}` – Fetch & clear notifications

Both services expose Swagger UI at `/swagger` when running in Development mode.

---

## ✅ Running Tests

To run unit & integration tests (requires Docker running):

```bash
dotnet test Company_Case_Project.sln
```

> Uses [Testcontainers](https://github.com/testcontainers/testcontainers-dotnet) to spin up PostgreSQL and RabbitMQ for integration tests.

---

## 🔁 CI/CD Pipeline

GitHub Actions pipeline automatically runs on push/PR to `main`:

- Format check via `dotnet format`
- Integration tests with Testcontainers
- Docker image builds
- Docker Compose validation

Workflow config: `.github/workflows/main.yml`



