import sys
from joblib import dump, load
import numpy as np
import json
from sklearn.feature_extraction.text import *

model_path = sys.argv[1]
features_file_path = sys.argv[2]
vocabulary_path = sys.argv[3]
classified_file_path = sys.argv[4]

# Borrowed from:
# https://stackoverflow.com/questions/26646362/numpy-array-is-not-json-serializable
class NumpyEncoder(json.JSONEncoder):
    """ Special json encoder for numpy types """
    def default(self, obj):
        if isinstance(obj, np.integer):
            return int(obj)
        elif isinstance(obj, np.floating):
            return float(obj)
        elif isinstance(obj, np.ndarray):
            return obj.tolist()
        return json.JSONEncoder.default(self, obj)

categories = ['No Flaw', 'CWE022', 'CWE078', 'CWE089', 'CWE090', 'CWE091']
#print(model_path)
#print(features_file_path)
model = load(model_path)
vocabulary = load(vocabulary_path)
features = []
with open(features_file_path) as json_file:
    scanned_files = np.array(json.load(json_file))
    json_file.close()
for example_index in range(0, len(scanned_files)):
    features.append(scanned_files[example_index]['Features'])

vectorizer = TfidfVectorizer(vocabulary=vocabulary,token_pattern=r"(?u)\b\w[\w\.(),]+\b",stop_words=None)
feature_vectors = vectorizer.fit_transform(features)
predicted_labels = model.predict(feature_vectors)
for example_index in range(0, len(scanned_files)):
    #print(scanned_files[example_index]['SourcePath'])
    if features[example_index] == '':
        predicted_label = 0 # No vulnerability
    else:
        predicted_label = predicted_labels[example_index]
    #print(categories[predicted_label])
    scanned_files[example_index]['ClassName'] = categories[predicted_label]

with open(classified_file_path, 'w') as json_file:
    json.dump(scanned_files, json_file, cls=NumpyEncoder)
    json_file.flush()
    json_file.close()
