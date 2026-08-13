# AI Service – Intelligent Energy Management System

Python FastAPI microservice that exposes the trained scikit-learn model for energy consumption prediction.

## Endpoints

| Method | URL        | Description                    |
|--------|------------|--------------------------------|
| GET    | `/health`  | Health check                   |
| POST   | `/predict` | Predict energy consumption     |
| GET    | `/docs`    | Swagger UI (auto-generated)    |

## Setup

```bash
pip install -r requirements.txt
python -m app.model
uvicorn app.main:app --reload --port 8000
```

The service runs at **http://localhost:8000**.

If `models/energy_model.pkl` is missing, the model is trained automatically on the first prediction request.

## Example Request

```json
POST /predict
{
  "hour": 18,
  "dayOfWeek": 2,
  "month": 8,
  "temperature": 28,
  "previousConsumption": 15.4
}
```

## Example Response

```json
{
  "predictedConsumption": 17.82
}
```
