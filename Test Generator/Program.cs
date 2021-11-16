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

            string pathToGenerated = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + "\\GeneratedTests\\Tests";
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
            int maxThreads = GetNumberFromUser();
            var files = from file in allFiles
                        where file.Substring(file.Length - 3) == ".cs"
                        select file;
            TestsGenerationManager manager = new TestsGenerationManager();
            
            Task task = manager.Generate(files, pathToGenerated, maxThreads);
            task.Wait();
            Console.WriteLine("end.");
        }

        private static int GetNumberFromUser()
        {
            int threadsNum;
            do
            {
                Console.WriteLine("Write max number of threads: ");
                string threads = Console.ReadLine();
                try
                {
                    threadsNum = int.Parse(threads);
                    return threadsNum;
                }
                catch (FormatException)
                {
                    Console.WriteLine("\"" + threads + "\"" + " is not a number.");
                    continue;
                }
            } while (true);
        }
    }
}
