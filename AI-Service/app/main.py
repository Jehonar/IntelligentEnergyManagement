from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from .schemas import PredictionRequest, PredictionResponse, HealthResponse
from .prediction import predict

app = FastAPI(
    title="Intelligent Energy AI Service",
    description="Python FastAPI service exposing ML-based energy consumption predictions.",
    version="1.0.0",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/health", response_model=HealthResponse, tags=["Health"])
def health():
    """Returns the health status of the AI service."""
    return {"status": "AI service is running"}


@app.post("/predict", response_model=PredictionResponse, tags=["Prediction"])
def predict_consumption(request: PredictionRequest):
    """
    Predicts energy consumption using a Random Forest model.

    - **hour**: hour of day (0-23)
    - **dayOfWeek**: day of week (0=Monday, 6=Sunday)
    - **month**: month of year (1-12)
    - **temperature**: temperature in Celsius
    - **previousConsumption**: previous consumption in kWh
    """
    predicted = predict(request)
    return PredictionResponse(predictedConsumption=predicted)
