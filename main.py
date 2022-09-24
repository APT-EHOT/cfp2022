# -*- coding: UTF-8 -*-
from datetime import datetime

from catboost.datasets import titanic
from sklearn.model_selection import train_test_split
from catboost import CatBoostClassifier, Pool, metrics, cv
from sklearn.metrics import accuracy_score
import numpy as np
import pandas as pd


def main():
    train_df = pd.read_csv("result.csv")
    #test_df = pd.read_csv("test.csv")
    train_df.head()

    with pd.option_context('display.max_rows', None, 'display.max_columns', None):
        print(train_df)
    #print(test_df)

    for item in train_df:
        del item[1]

    X = train_df.drop('currentprice', axis=1)
    y = train_df.currentprice

    categorical_features_indices = np.where(X.dtypes != datetime)[0]
    X_train, X_validation, y_train, y_validation = train_test_split(X, y, train_size=0.75)

    model = CatBoostClassifier(custom_loss=[metrics.Accuracy()], iterations=10, logging_level='Silent')
    model.fit(
        X_train, y_train,
        cat_features=categorical_features_indices,
        eval_set=(X_validation, y_validation)
    )
    #cv_params = model.get_params()
    #cv_params.update({
    #    'loss_function': metrics.Logloss()
    #})
    #cv_data = cv(
    #    Pool(X, y, cat_features=categorical_features_indices),
    #    cv_params
    #)
    #print('Best validation accuracy score: {:.2f}±{:.2f} on step {}'.format(
    #    np.max(cv_data['test-Accuracy-mean']),
    #    cv_data['test-Accuracy-std'][np.argmax(cv_data['test-Accuracy-mean'])],
    #    np.argmax(cv_data['test-Accuracy-mean'])
    #))
    #print('Precise validation accuracy score: {}'.format(np.max(cv_data['test-Accuracy-mean'])))


main()
