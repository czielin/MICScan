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
            Scanner scanner = new Scanner(true);
            //var csFiles = await scanner.ScanFolder(@"C:\git\sard\TestApplication");
            await scanner.ScanProject(@"C:\git\sard\TestApplication\TestApplication.csproj");
        }
    }
}
