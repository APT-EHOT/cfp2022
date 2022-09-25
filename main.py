# -*- coding: UTF-8 -*-
from datetime import datetime

from catboost.datasets import titanic
from sklearn.model_selection import train_test_split
from catboost import CatBoostClassifier, Pool, metrics, cv, CatBoostRanker, CatBoostRegressor
from sklearn.metrics import accuracy_score
import numpy as np
import pandas as pd


def main():
    train_df = pd.read_csv("predict4.csv")

    model = CatBoostRegressor()
    model.load_model('catboost_model2.dump')

    predicts = model.predict(train_df)
    print(predicts)

    sum = 0
    for predict in predicts:
        sum = sum + predict
    print(sum * 100 / len(predicts))

main()
