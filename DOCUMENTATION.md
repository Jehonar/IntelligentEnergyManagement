# Project Documentation
# Intelligent Energy Management System

---

## 1. Introduction

Energy consumption is one of the most important challenges facing modern organizations and households. Unmonitored and unoptimized energy usage leads to higher costs, unnecessary carbon emissions, and wasted resources.

Traditional energy monitoring systems only show historical data without any ability to predict future usage. By combining modern web technologies with Machine Learning, an intelligent system can not only display consumption data but also forecast future usage and proactively suggest ways to reduce it.

This project implements an **Intelligent Energy Management System** — a web application that demonstrates how AI can be applied to the energy domain in a practical, understandable way. It is designed as a university capstone project that integrates a modern frontend, a clean backend API, a SQL Server database, and a Python-based Machine Learning service into a single working application.

---

## 2. Objectives

The system is designed to achieve the following goals:

1. **Monitor energy consumption** — display current, historical, and aggregated energy data in an easy-to-understand dashboard.
2. **Predict future consumption** — use a trained Machine Learning model to forecast how much energy will be consumed at a given hour under given conditions.
3. **Identify high consumption periods** — automatically detect when predicted usage is above average.
4. **Provide actionable recommendations** — generate simple, rule-based recommendations to help users reduce consumption during high-usage periods.

---

## 3. Technologies

### Vue.js 3
Vue.js is a progressive JavaScript framework used to build the user interface. Version 3 introduces the Composition API, which makes it easier to organize component logic. It is paired with TypeScript for type safety and Vue Router for client-side navigation.

### Tailwind CSS
Tailwind is a utility-first CSS framework that allows rapid UI development without writing custom CSS files. Each UI element is styled using small utility classes directly in the HTML template.

### Chart.js + vue-chartjs
Chart.js is a JavaScript charting library used to render line charts, bar charts, and comparison charts. The `vue-chartjs` wrapper integrates it seamlessly with Vue 3 components.

### ASP.NET Core 8
ASP.NET Core is a cross-platform, high-performance web framework for building REST APIs with C#. It handles incoming requests from the frontend, interacts with the database through Entity Framework Core, and orchestrates calls to the Python AI service.

### C#
C# is a strongly typed, modern programming language developed by Microsoft. It is used to implement all business logic, data models, DTOs, and service classes in the backend.

### Entity Framework Core 8
EF Core is an object-relational mapper (ORM) that allows the application to interact with SQL Server using C# objects instead of raw SQL queries.

### Microsoft SQL Server
SQL Server is a relational database management system used to store all application data — energy readings, predictions, and recommendations. It is accessed via EF Core from the ASP.NET Core backend.

### Python 3
Python is used exclusively for the Machine Learning component. Its extensive ecosystem of data science libraries makes it the best choice for training and serving ML models.

### FastAPI
FastAPI is a modern Python web framework optimized for building APIs with automatic documentation generation (Swagger). It exposes the ML model as a microservice with a `/predict` endpoint.

### scikit-learn
scikit-learn is a Python library providing simple and efficient tools for predictive data analysis. In this project it is used to train a Random Forest Regressor for energy prediction.

### pandas & numpy
pandas is used for tabular data manipulation during model training. numpy provides numerical array operations. Both are foundational to the data science workflow.

### joblib
joblib is used to serialize (save) and deserialize (load) the trained scikit-learn model to/from disk, so it does not need to be retrained on every application start.

---

## 4. System Architecture

The system follows a clean three-tier architecture with an additional AI microservice.

```
User Browser
     │
     │  HTTP (REST/JSON)
     ▼
Vue.js 3 Frontend (port 5173)
     │
     │  HTTP (REST/JSON)
     ▼
ASP.NET Core Web API (port 5000)
     │                     │
     │  EF Core / SQL       │  HTTP (REST/JSON)
     ▼                     ▼
SQL Server              Python FastAPI AI Service (port 8000)
(IntelligentEnergySystem DB)  scikit-learn Random Forest Model
```

### Communication flow

1. The **Vue.js frontend** makes HTTP requests to the ASP.NET Core API using Axios.
2. The **ASP.NET Core API** reads/writes energy data from **SQL Server** using EF Core.
3. For prediction requests, the **ASP.NET Core API** forwards the request to the **Python AI service** via HTTP.
4. The **Python service** runs the trained model and returns a `predictedConsumption` value.
5. The ASP.NET Core API saves the prediction to the database and returns the full result to Vue.js.
6. The frontend displays the prediction and allows the user to request a recommendation.

### Separation of concerns

| Component       | Responsibility                                 |
|-----------------|------------------------------------------------|
| Vue.js          | Presentation and user interaction only         |
| ASP.NET Core    | Business logic, API routing, data access       |
| SQL Server      | Persistent storage of all application data     |
| Python/FastAPI  | Machine learning inference only               |

---

## 5. Modules

### Module 1 – Energy Monitoring

The monitoring module displays raw and aggregated energy data. Users can filter readings by date range and device. The module shows:

- Total consumption
- Average consumption
- Highest and lowest readings
- Daily, hourly, and monthly consumption charts

Data is stored in the `EnergyReadings` table with columns: `Id`, `ReadingDate`, `ReadingHour`, `EnergyConsumption`, `Temperature`, `DeviceName`, `CreatedAt`.

### Module 2 – AI Energy Prediction

The prediction module allows users to input parameters (hour, day of week, month, temperature, previous consumption) and receive an AI-generated prediction for future energy consumption.

The workflow:
1. User fills the form in the frontend.
2. Vue.js POSTs to `/api/prediction`.
3. ASP.NET Core forwards the request to the Python AI service at `/predict`.
4. The Python service runs inference with the Random Forest model.
5. The prediction is saved to the `Predictions` table and returned to Vue.js.
6. The frontend displays the predicted value and a comparison chart.

### Module 3 – AI Recommendations

The recommendation module generates advice based on the predicted consumption compared to the historical average. The rules are:

| Condition                       | Recommendation Type |
|---------------------------------|---------------------|
| prediction > average × 1.20    | HIGH                |
| prediction > average × 1.05    | MODERATE            |
| prediction between 0.90–1.05×  | NORMAL              |
| prediction ≤ average × 0.90    | LOW                 |

Recommendations are saved in the `Recommendations` table linked to the prediction.

---

## 6. AI Model

### Dataset

The model is trained on synthetic but realistic data generated programmatically in `app/model.py`. The dataset contains **2,160 hourly records** covering 90 days (January–March 2026).

Generation rules:
- Higher consumption during morning (6–9 AM) and evening (18–21) peaks.
- Weekend consumption is 30% lower than weekdays.
- High temperatures (> 25 °C) and very low temperatures (< 0 °C) increase HVAC consumption.
- ±15% random noise is added to simulate real-world variability.

### Features

| Feature               | Type    | Description                          |
|-----------------------|---------|--------------------------------------|
| `hour`                | int     | Hour of the day (0–23)               |
| `dayOfWeek`           | int     | Day of week (0=Monday, 6=Sunday)     |
| `month`               | int     | Month (1–12)                         |
| `temperature`         | float   | Ambient temperature in Celsius       |
| `previousConsumption` | float   | Consumption in the previous period   |

### Target Variable

`energyConsumption` — energy consumed in kWh during the given hour.

### Training

```python
model = RandomForestRegressor(n_estimators=100, max_depth=10, random_state=42)
model.fit(X_train, y_train)
```

The data is split 80/20 for training and testing. The model is saved to `models/energy_model.pkl` using `joblib`.

### Model Evaluation

| Metric | Value      | Interpretation                                |
|--------|------------|-----------------------------------------------|
| MAE    | ~0.12 kWh  | Average absolute error per prediction          |
| RMSE   | ~0.17 kWh  | Root mean squared error (penalizes large errors)|
| R²     | ~0.99      | 99% of variance in consumption is explained    |

The model achieves near-perfect accuracy on the synthetic data, demonstrating that the features chosen are highly predictive of energy consumption.

---

## 7. Testing

### API Testing
The ASP.NET Core API is documented via Swagger at `http://localhost:5000/swagger`. All endpoints can be tested interactively through the Swagger UI.

Example test cases:
- `GET /api/energy/statistics` — verify KPIs are returned.
- `POST /api/prediction` with `{ "hour": 18, "dayOfWeek": 2, "month": 8, "temperature": 28, "previousConsumption": 15.4 }` — verify a prediction is returned and saved.
- `POST /api/recommendations` with a predicted value — verify a recommendation is returned.

### AI Model Testing
Run `python -m app.model` to retrain and evaluate the model. The output prints MAE, RMSE, and R² on the test set.

### Python Service Testing
The FastAPI service has built-in interactive docs at `http://localhost:8000/docs`. Test the `/predict` endpoint directly from the browser.

### Frontend Testing
Manual end-to-end testing:
1. Open `http://localhost:5173`.
2. Navigate to Dashboard — verify KPI cards load.
3. Navigate to Energy Monitoring — verify table and filter work.
4. Navigate to AI Prediction — fill form, click Predict, verify result.
5. Click "Get Recommendation" — verify a recommendation appears.
6. Navigate to Recommendations — verify list populates.

### Integration Testing
With all three services running, verify the full prediction flow:
1. Vue.js → POST `/api/prediction` → ASP.NET Core → POST `/predict` → Python AI → returns result → Vue.js displays prediction.

---

## 8. Conclusion

The Intelligent Energy Management System demonstrates how modern web technologies and machine learning can be combined into a clean, maintainable application. The clear separation of frontend, backend, database, and AI service makes each component understandable and independently testable — while together they deliver a complete, working intelligent system.
