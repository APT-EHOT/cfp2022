# -*- coding: UTF-8 -*-
from datetime import datetime

from catboost.datasets import titanic
from sklearn.model_selection import train_test_split
from catboost import CatBoostClassifier, Pool, metrics, cv, CatBoostRanker, CatBoostRegressor
from sklearn.metrics import accuracy_score
import numpy as np
import pandas as pd


def main():
    train_df = pd.read_csv("result.csv")
    #test_df = pd.read_csv("test.csv")
    #train_df.head()

    #with pd.option_context('display.max_rows', None, 'display.max_columns', None):
     #   print(train_df)
    #print(test_df)

    X = train_df.drop('y', axis=1)
    y = train_df.y

    X_train, X_validation, y_train, y_validation = train_test_split(X, y, train_size=0.75)

    model = CatBoostRegressor(random_state=42, silent=True)
    model.fit(
        X_train, y_train,
        #eval_set=(X_validation, y_validation)
    )
    cv_params = model.get_params()
    # cv_params.update({
    #    'loss_function': metrics.Logloss()
    # })
    cv_data = cv(
       Pool(X_validation, y_validation),
       cv_params,
    )

    predicts = model.predict(X_validation)
    print(np.take(predicts, [i for i in range(0, 9)]), y_validation[0:10])

    print(cv_data)

    # print('Best validation accuracy score: {:.2f}±{:.2f} on step {}'.format(
    #    np.max(cv_data['test-Accuracy-mean']),
    #    cv_data['test-Accuracy-std'][np.argmax(cv_data['test-Accuracy-mean'])],
    #    np.argmax(cv_data['test-Accuracy-mean'])
    # ))
    # print('Precise validation accuracy score: {}'.format(np.max(cv_data['test-Accuracy-mean'])))


main()
