"""
Trains a Random Forest Regressor on synthetic energy data and saves the model.
Run this script once before starting the FastAPI service:
    python -m app.model
"""

import os
import numpy as np
import pandas as pd
import joblib
from sklearn.ensemble import RandomForestRegressor
from sklearn.model_selection import train_test_split
from sklearn.metrics import mean_absolute_error, mean_squared_error, r2_score

MODEL_PATH = os.path.join(os.path.dirname(__file__), "..", "models", "energy_model.pkl")
DATA_PATH  = os.path.join(os.path.dirname(__file__), "..", "data", "energy_data.csv")


def generate_training_data(n_days: int = 90) -> pd.DataFrame:
    """Generates realistic synthetic hourly energy readings."""
    rng   = np.random.default_rng(42)
    rows  = []
    start = pd.Timestamp("2026-01-01")

    for day_offset in range(n_days):
        date = start + pd.Timedelta(days=day_offset)
        month      = date.month
        day_of_week = date.dayofweek  # 0=Monday

        for hour in range(24):
            # Temperature by month and time of day
            base_temp = 5 + (month - 1) * 1.5 + (2 if 10 <= hour <= 15 else 0)
            temp = base_temp + rng.normal(0, 2)

            # Consumption shaped by hour and day type
            base = 2.0
            if 6 <= hour <= 8:
                base = 5.5
            elif 9 <= hour <= 17:
                base = 3.5
            elif 18 <= hour <= 21:
                base = 6.5
            elif 0 <= hour <= 5:
                base = 1.2

            # Weekend reduction
            if day_of_week >= 5:
                base *= 0.7

            # Temperature effect
            if temp > 25:
                base *= 1.15
            elif temp < 0:
                base *= 1.20

            # Random noise
            base *= rng.uniform(0.88, 1.12)

            rows.append({
                "hour":                hour,
                "dayOfWeek":           day_of_week,
                "month":               month,
                "temperature":         round(temp, 2),
                "previousConsumption": round(base * rng.uniform(0.85, 1.05), 4),
                "energyConsumption":   round(base, 4),
            })

    return pd.DataFrame(rows)


def train():
    os.makedirs(os.path.dirname(MODEL_PATH), exist_ok=True)
    os.makedirs(os.path.dirname(DATA_PATH),  exist_ok=True)

    df = generate_training_data(n_days=90)
    df.to_csv(DATA_PATH, index=False)
    print(f"Training data saved: {DATA_PATH} ({len(df)} rows)")

    features = ["hour", "dayOfWeek", "month", "temperature", "previousConsumption"]
    target   = "energyConsumption"

    X = df[features]
    y = df[target]

    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

    model = RandomForestRegressor(n_estimators=100, max_depth=10, random_state=42, n_jobs=-1)
    model.fit(X_train, y_train)

    y_pred = model.predict(X_test)

    mae  = mean_absolute_error(y_test, y_pred)
    rmse = mean_squared_error(y_test, y_pred) ** 0.5
    r2   = r2_score(y_test, y_pred)

    print(f"Model evaluation:")
    print(f"  MAE  = {mae:.4f} kWh")
    print(f"  RMSE = {rmse:.4f} kWh")
    print(f"  R²   = {r2:.4f}")

    joblib.dump(model, MODEL_PATH)
    print(f"Model saved: {MODEL_PATH}")
    return model


if __name__ == "__main__":
    train()
