"""Script to classify vulnerabilities."""
from argparse import ArgumentParser
import json

import joblib
import numpy as np
from sklearn.feature_extraction.text import TfidfVectorizer

parser = ArgumentParser()
parser.add_argument('model_path')
parser.add_argument('features_file_path')
parser.add_argument('vocabulary_path')
parser.add_argument('classified_file_path')

args = parser.parse_args()


# Borrowed from:
# https://stackoverflow.com/questions/26646362/numpy-array-is-not-json-serializable
class NumpyEncoder(json.JSONEncoder):
    """Special json encoder for numpy types."""

    def default(self, obj):
        """Numpy json encoder."""
        if isinstance(obj, np.integer):
            return int(obj)
        elif isinstance(obj, np.floating):
            return float(obj)
        elif isinstance(obj, np.ndarray):
            return obj.tolist()
        return json.JSONEncoder.default(self, obj)


categories = ['No Flaw', 'CWE022', 'CWE078', 'CWE089', 'CWE090', 'CWE091']
model = joblib.load(args.model_path)
vocabulary = joblib.load(args.vocabulary_path)

with open(args.features_file_path) as json_file:
    scanned_files = np.array(json.load(json_file))

features = [scanned_file['Features'] for scanned_file in scanned_files]

vectorizer = TfidfVectorizer(
    vocabulary=vocabulary, token_pattern=r'(?u)\b\w[\w\.(),]+\b', stop_words=None
)
feature_vectors = vectorizer.fit_transform(features)
predicted_labels = model.predict(feature_vectors)

for example_index, scanned_file in enumerate(scanned_files):
    # 0 = No vulnerability
    if not features[example_index]:
        predicted_label = 0  # No vulnerability
    else:
        predicted_label = predicted_labels[example_index]

    scanned_file['ClassName'] = categories[predicted_label]

with open(args.classified_file_path, 'w') as json_file:
    json.dump(scanned_files, json_file, cls=NumpyEncoder)
