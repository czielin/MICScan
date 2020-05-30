# MICScan

This application takes the NIST SARD (Software Assurance Reference Dataset) and trains a model that can be used to detect if a code sample contains a vulnerability, and if so classify what type of vulnerability it contains. Currently it is limited to only the C# samples within the SARD.

The C# command line application loops through each of the code samples and compiles, analyzes and partially executes the code in order to extract features that can be fed into the model. The features are output as JSON files into the Output directory.

The Notebooks directory contains some Jupyter notebooks used to train and test the model against these datasets. The notebook imports the JSON datasets that the C# application outputs.

The SARD dataset is fairly large and is not included in this repository. It must be downloaded separately, extracted and referenced using the `sardRoot` variable in Program.cs.
