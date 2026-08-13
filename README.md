# Intelligent Energy Management System

A university project demonstrating how Artificial Intelligence can be used to **monitor**, **analyze**, and **predict** energy consumption.

---

## Architecture Overview

```
Vue.js 3 (localhost:5173)
        │
        │  REST API (HTTP/JSON)
        ▼
ASP.NET Core Web API (localhost:5000)
        │                    │
        │  SQL Server         │  HTTP → Python AI Service (localhost:8000)
        ▼                    ▼
  IntelligentEnergy DB    FastAPI + scikit-learn (Random Forest)
```

---

## Technology Stack

| Layer       | Technology                              |
|-------------|------------------------------------------|
| Frontend    | Vue.js 3, TypeScript, Vite, Tailwind CSS, Chart.js |
| Backend     | ASP.NET Core 8, C#, Entity Framework Core 8 |
| Database    | Microsoft SQL Server (local)             |
| AI Service  | Python 3.11+, FastAPI, scikit-learn      |
| API Docs    | Swagger / OpenAPI (localhost:5000/swagger) |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Python 3.11+](https://www.python.org/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express is fine)
- SQL Server Management Studio or `sqlcmd`

---

## Setup Instructions

### 1. Database (SQL Server – lokal / local)

Hap **SQL Server Management Studio (SSMS)** ose përdor `sqlcmd` dhe ekzekuto skriptin:

```sql
-- Open and execute:
Database\CreateDatabase.sql
```

Skripti krijon bazën **`IntelligentEnergySystem`**, tabelat dhe ~8,640 lexime ore (3 muaj × 4 pajisje).

> **Shënim:** Skedari `.mdf` ruhet në `C:\SQLData\` (folder i pakompresuar). Nëse `CREATE DATABASE` dështon me gabimin *"file is compressed"*, ekzekuto:
> ```cmd
> compact /U /S:"C:\SQLData"
> ```

Nëse instanca juaj SQL Server nuk është `localhost`, ndryshoni connection string në `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=IntelligentEnergySystem;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Për **SQL Server Express** përdorni: `Server=localhost\\SQLEXPRESS;...`

---

### 2. ASP.NET Core API (Visual Studio ose CLI)

**Me Visual Studio:**
1. Hap `Backend\IntelligentEnergyManagement.sln`
2. Set `IntelligentEnergy.API` si startup project
3. Shtyp **F5** (Debug) ose **Ctrl+F5** (Run)

**Me terminal:**

```bash
cd Backend\IntelligentEnergy.API
dotnet run
```

Edit `appsettings.json` if your SQL Server instance name differs:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=IntelligentEnergySystem;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

- API base URL: http://localhost:5000
- Swagger UI:   http://localhost:5000/swagger

---

### 3. Python AI Service

```bash
cd AI-Service

# Install dependencies
pip install -r requirements.txt

# Train the model (only needed once)
python -m app.model

# Start the service
uvicorn app.main:app --reload --port 8000
```

- API base URL:  http://localhost:8000
- Swagger docs:  http://localhost:8000/docs
- Health check:  http://localhost:8000/health

> **Note:** If the model file (`models/energy_model.pkl`) is missing, the service trains it automatically on first request.

---

### 4. Vue.js Frontend

```bash
cd Frontend
npm install
npm run dev
```

- Frontend URL: http://localhost:5173

---

## API Endpoints Reference

### Energy (ASP.NET Core)

| Method | Endpoint                    | Description                            |
|--------|-----------------------------|----------------------------------------|
| GET    | `/api/energy`               | List readings (supports filters)       |
| GET    | `/api/energy/daily`         | Daily totals for last N days           |
| GET    | `/api/energy/monthly`       | Monthly totals                         |
| GET    | `/api/energy/statistics`    | KPIs + chart data                      |
| GET    | `/api/energy/devices`       | List of device names                   |

### Prediction

| Method | Endpoint                    | Description                            |
|--------|-----------------------------|----------------------------------------|
| POST   | `/api/prediction`           | Request AI prediction                  |
| GET    | `/api/prediction/history`   | View past predictions                  |

### Recommendations

| Method | Endpoint                    | Description                            |
|--------|-----------------------------|----------------------------------------|
| POST   | `/api/recommendations`      | Generate a recommendation              |
| GET    | `/api/recommendations`      | List recent recommendations            |

### Python AI Service

| Method | Endpoint    | Description              |
|--------|-------------|--------------------------|
| GET    | `/health`   | Health check             |
| POST   | `/predict`  | ML-based consumption prediction |

---

## Project Structure

```
IntelligentEnergyManagement/
│
├── Database/
│   └── CreateDatabase.sql          # SQL Server schema + seed data
│
├── Backend/
│   ├── IntelligentEnergyManagement.sln   # Hap në Visual Studio
│   └── IntelligentEnergy.API/
│       ├── Controllers/            # EnergyController, PredictionController, RecommendationController
│       ├── Models/                 # EnergyReading, Prediction, Recommendation
│       ├── DTOs/                   # Data transfer objects
│       ├── Services/               # Business logic
│       ├── Data/                   # ApplicationDbContext (EF Core)
│       ├── Program.cs
│       └── appsettings.json
│
├── AI-Service/
│   ├── app/
│   │   ├── main.py                 # FastAPI entry point
│   │   ├── model.py                # Model training script
│   │   ├── prediction.py           # Model loading + inference
│   │   └── schemas.py              # Pydantic schemas
│   ├── data/
│   │   └── energy_data.csv         # Generated training data
│   ├── models/
│   │   └── energy_model.pkl        # Trained Random Forest model
│   └── requirements.txt
│
└── Frontend/
    └── src/
        ├── api/index.ts            # Axios API client
        ├── router/index.ts         # Vue Router configuration
        ├── views/
        │   ├── DashboardView.vue
        │   ├── MonitoringView.vue
        │   ├── PredictionView.vue
        │   └── RecommendationsView.vue
        └── components/
            └── KpiCard.vue
```

---

## AI Model Details

### Dataset
- 2,160 synthetic hourly energy readings (90 days × 24 hours)
- Generated with realistic patterns (morning/evening peaks, weekend reductions, temperature effects)

### Features

| Feature               | Description                          |
|-----------------------|--------------------------------------|
| `hour`                | Hour of day (0–23)                   |
| `dayOfWeek`           | Day of week (0=Monday, 6=Sunday)     |
| `month`               | Month of year (1–12)                 |
| `temperature`         | Ambient temperature (°C)             |
| `previousConsumption` | Previous period consumption (kWh)   |

### Target Variable
- `energyConsumption` – actual energy consumption in kWh

### Model
- **Algorithm:** Random Forest Regressor (100 trees, max depth 10)
- **Library:** scikit-learn

### Evaluation Results (on 20% test set)

| Metric | Value       |
|--------|-------------|
| MAE    | ~0.12 kWh   |
| RMSE   | ~0.17 kWh   |
| R²     | ~0.99       |

### Recommendation Rules

| Condition                              | Type       |
|----------------------------------------|------------|
| predicted > average × 1.20            | HIGH       |
| predicted > average × 1.05            | MODERATE   |
| predicted between 0.90× and 1.05×     | NORMAL     |
| predicted ≤ average × 0.90            | LOW        |

---

## Running All Services

Open three terminals:

```bash
# Terminal 1 – Backend
cd Backend\IntelligentEnergy.API
dotnet run

# Terminal 2 – AI Service
cd AI-Service
uvicorn app.main:app --reload --port 8000

# Terminal 3 – Frontend
cd Frontend
npm run dev
```

Then open http://localhost:5173 in your browser.

---

## Notes

- Authentication is intentionally omitted to keep the project demo-friendly.
- If the Python AI service is unreachable, the backend falls back to a rule-based prediction so the rest of the application still works.
- The SQL seed data covers January–March 2026 with four devices: HVAC, Lighting, Appliances, Computers.
