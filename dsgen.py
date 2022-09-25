from catboost.datasets import titanic

import pandas as pd
import numpy as np

train_df, test_df = titanic()
train_df.head()

null_value_stats = train_df.isnull().sum(axis=0)
null_value_stats[null_value_stats != 0]
train_df.fillna(-999, inplace=True)
test_df.fillna(-999, inplace=True)

X = train_df.drop('Survived', axis=1)
y = train_df.Survived

categorical_features_indices = np.where(X.dtypes != float)[0]

from sklearn.model_selection import train_test_split

X_train, X_validation, y_train, y_validation = train_test_split(X, y, train_size=0.75, random_state=42)

X_test = test_df

with pd.option_context('display.max_rows', None, 'display.max_columns', None):
    print(X_train)
print(y_train)
with pd.option_context('display.max_rows', None, 'display.max_columns', None):
    print(X_validation)
print(y_validation)
