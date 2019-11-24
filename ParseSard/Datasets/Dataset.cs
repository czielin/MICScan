using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ParseSard.Datasets
{
    public abstract class Dataset
    {
        protected List<Example> dataset = new List<Example>();
        protected abstract string FileName { get; set; }

        public abstract void AddExample(string className, bool isFlawed, List<string> fileContents);
        public void SaveToFile(string directory)
        {
            string json = JsonConvert.SerializeObject(dataset);
            File.WriteAllText(Path.Combine(directory, $"{FileName}.json"), json);
        }
    }
}
