# -*- coding: UTF-8 -*-

from catboost.datasets import titanic
from sklearn.model_selection import train_test_split
from catboost import CatBoostClassifier, Pool, metrics, cv
from sklearn.metrics import accuracy_score
import numpy as np
import pandas as pd


def main():
    train_df, test_df = titanic()
    train_df.head()

    with pd.option_context('display.max_rows', None, 'display.max_columns', None):
        print(train_df)
    print(test_df)

    null_value_stats = train_df.isnull().sum(axis=0)
    null_value_stats[null_value_stats != 0]

    train_df.fillna(-999, inplace=True)
    test_df.fillna(-999, inplace=True)
    X = train_df.drop('Survived', axis=1)
    y = train_df.Survived
    print(train_df.Survived)
    print(X.dtypes)

    categorical_features_indices = np.where(X.dtypes != float)[0]
    X_train, X_validation, y_train, y_validation = train_test_split(X, y, train_size=0.75, random_state=42)

    model = CatBoostClassifier(
        custom_loss=[metrics.Accuracy()],
        random_seed=42,
        logging_level='Silent'
    )
    model.fit(
        X_train, y_train,
        cat_features=categorical_features_indices,
        eval_set=(X_validation, y_validation)
    )
    cv_params = model.get_params()
    cv_params.update({
        'loss_function': metrics.Logloss()
    })
    cv_data = cv(
        Pool(X, y, cat_features=categorical_features_indices),
        cv_params
    )
    print('Best validation accuracy score: {:.2f}±{:.2f} on step {}'.format(
        np.max(cv_data['test-Accuracy-mean']),
        cv_data['test-Accuracy-std'][np.argmax(cv_data['test-Accuracy-mean'])],
        np.argmax(cv_data['test-Accuracy-mean'])
    ))
    print('Precise validation accuracy score: {}'.format(np.max(cv_data['test-Accuracy-mean'])))


main()
