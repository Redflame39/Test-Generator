using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string filesDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\Files";
            Console.WriteLine(filesDirectory);

            string pathToGenerated = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + "\\GeneratedTests\\GeneratedFiles";
            Console.WriteLine(pathToGenerated);
            if (!Directory.Exists(filesDirectory))
            {
                Console.WriteLine($"Couldn't find directory {filesDirectory}");
                return;
            }
            if (!Directory.Exists(pathToGenerated))
            {
                Directory.CreateDirectory(pathToGenerated);
            }

            var allFiles = Directory.GetFiles(filesDirectory);

            var files = from file in allFiles
                        where file.Substring(file.Length - 3) == ".cs"
                        select file;
            TestsGenerationManager manager = new TestsGenerationManager();
            Task task = manager.Generate(files, pathToGenerated);
            task.Wait();
            Console.WriteLine("end.");
        }
    }
}
