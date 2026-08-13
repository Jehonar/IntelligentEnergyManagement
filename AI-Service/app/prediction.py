"""
Loads the trained model and exposes a predict() function.
If the model file does not exist, trains it on the fly.
"""

import os
import numpy as np
import joblib
from .model import MODEL_PATH, train
from .schemas import PredictionRequest

_model = None


def get_model():
    global _model
    if _model is None:
        if os.path.exists(MODEL_PATH):
            _model = joblib.load(MODEL_PATH)
        else:
            print("Model not found – training now...")
            _model = train()
    return _model


def predict(request: PredictionRequest) -> float:
    model = get_model()

    features = np.array([[
        request.hour,
        request.dayOfWeek,
        request.month,
        request.temperature,
        request.previousConsumption,
    ]])

    result = model.predict(features)[0]
    return round(float(result), 4)
