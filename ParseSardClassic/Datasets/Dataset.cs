using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ParseSard.Datasets
{
    public abstract class Dataset
    {
        protected List<Example> dataset = new List<Example>();
        protected abstract string FileName { get; }
        private readonly Dictionary<string, List<Type>> assemblyReferenceMappings = new Dictionary<string, List<Type>>
        {
            { "using System;", new List<Type> { typeof(object) } },
            { "using System.Diagnostics;", new List<Type> { typeof(System.Diagnostics.Process) } },
            { "System.ComponentModel.Win32Exception", new List<Type> { typeof(System.ComponentModel.Win32Exception) } },
            { "using System.Data;", new List<Type> { typeof(System.Data.SqlDbType) } },
            { "using System.Data.SqlClient;", new List<Type> { typeof(System.Data.SqlClient.SqlCommand) } },
            { "using MySql.Data.MySqlClient;", new List<Type> { typeof(MySql.Data.MySqlClient.MySqlCommand), typeof(System.Data.SqlDbType) } },
            { "using Npgsql;", new List<Type> { typeof(Npgsql.NpgsqlCommand) } },
            { "using System.Linq;", new List<Type> { typeof(System.Linq.Enumerable) } },
            { "using System.Data.SQLite;", new List<Type> { typeof(System.Data.SQLite.SQLiteCommand) } },
            { "using System.Data.OracleClient;", new List<Type> { typeof(System.Data.OracleClient.OracleCommand), typeof(System.Data.SqlDbType) } },
            { "using System.DirectoryServices;", new List<Type> { typeof(System.DirectoryServices.DirectoryEntry) } },
            { "using System.Xml;", new List<Type> { typeof(System.Xml.XmlDocument) } },
            { "using System.Xml.XPath;", new List<Type> { typeof(System.Xml.XPath.Extensions) } },
            { "using System.IO;", new List<Type> { typeof(System.IO.StreamReader) } },
            { "using System.Xml.Linq;", new List<Type> { typeof(System.Xml.Linq.XDocument) } }
        };

        public abstract void AddExample(string className, bool isFlawed, List<string> fileContents);
        public void SaveToFile(string directory)
        {
            string json = JsonConvert.SerializeObject(dataset);
            File.WriteAllText(Path.Combine(directory, $"{FileName}.json"), json);
        }

        protected static string CombineContents(List<string> fileContents)
        {
            return string.Join("\r\n\r\n", fileContents);
        }

        protected static string RemoveComments(List<string> fileContents)
        {
            string combinedContents = CombineContents(fileContents);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(combinedContents);
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
            CommentRemovalRewriter rewriter = new CommentRemovalRewriter();
            SyntaxNode newRoot = rewriter.Visit(root);
            string processedContents = newRoot.ToFullString();
            return processedContents;
        }

        protected void PrintDescendentNodes(IEnumerable<SyntaxNode> syntaxNodes, SemanticModel semanticModel)
        {
            foreach (SyntaxNode syntaxNode in syntaxNodes)
            {
                PrintDescendentNodes(syntaxNode, semanticModel);
            }
        }

        protected void PrintDescendentNodes(SyntaxNode syntaxNode, SemanticModel semanticModel, string indent = "")
        {
            var symbol = semanticModel.GetSymbolInfo(syntaxNode).Symbol;
            Console.WriteLine($"{indent}{syntaxNode} (Kind: {syntaxNode.Kind()}, SymbolKind: {symbol?.Kind})");
            foreach (var child in syntaxNode.ChildNodes())
            {
                PrintDescendentNodes(child, semanticModel, indent + " ");
            }
        }

        protected CSharpCompilation CreateCompilation(SyntaxTree syntaxTree)
        {
            CSharpCompilation compilation = CSharpCompilation.Create("ExamineSnippet");
            HashSet<string> locations = new HashSet<string>();
            locations.Add(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.dll");
            foreach (SyntaxNode syntaxNode in syntaxTree.GetRoot().DescendantNodes())
            {
                string nodeContent = syntaxNode.ToString();
                if (assemblyReferenceMappings.ContainsKey(nodeContent))
                {
                    foreach (Type type in assemblyReferenceMappings[nodeContent])
                    {
                        if (!locations.Contains(type.Assembly.Location))
                        {
                            locations.Add(type.Assembly.Location);
                        }
                    }
                }
            }
            foreach (string location in locations)
            {
                compilation = compilation.AddReferences(MetadataReference.CreateFromFile(location));
            }
            return compilation.AddSyntaxTrees(syntaxTree);
        }
    }
}
