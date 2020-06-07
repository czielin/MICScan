from joblib import dump, load
model = load('model.pkl')
print("Logistic Regression (saved model)")
predicted_labels = model.predict(tfidf_dev_feature_vectors)
print("F1 score:", metrics.f1_score(dev_labels, predicted_labels, average='weighted'))
print("Accuracy: ", metrics.accuracy_score(dev_labels, predicted_labels))
print()