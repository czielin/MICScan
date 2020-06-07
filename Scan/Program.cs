using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scan
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Scanner scanner = new Scanner();
            var csFiles = await scanner.ScanFolder(@"C:\git\sard\TestApplication");
        }
    }
}
