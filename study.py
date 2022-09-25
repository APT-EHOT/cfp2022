# -*- coding: UTF-8 -*-
from datetime import datetime

from catboost.datasets import titanic
from sklearn.model_selection import train_test_split
from catboost import CatBoostClassifier, Pool, metrics, cv, CatBoostRanker, CatBoostRegressor
from sklearn.metrics import accuracy_score
import numpy as np
import pandas as pd


def main():
    train_df = pd.read_csv("result2.csv")

    X = train_df.drop('y', axis=1)
    y = train_df.y

    model = CatBoostRegressor(random_state=42, silent=True)
    model.fit(
        X, y
    )

    model.save_model('catboost_model2.dump')


main()
