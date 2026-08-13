from pydantic import BaseModel, Field


class PredictionRequest(BaseModel):
    hour: int = Field(..., ge=0, le=23, description="Hour of day (0-23)")
    dayOfWeek: int = Field(..., ge=0, le=6, description="Day of week (0=Monday, 6=Sunday)")
    month: int = Field(..., ge=1, le=12, description="Month (1-12)")
    temperature: float = Field(..., description="Temperature in Celsius")
    previousConsumption: float = Field(..., description="Previous hour consumption in kWh")


class PredictionResponse(BaseModel):
    predictedConsumption: float = Field(..., description="Predicted energy consumption in kWh")


class HealthResponse(BaseModel):
    status: str
