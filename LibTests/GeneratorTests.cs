using ConsoleApp;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LibTests
{
    public class GeneratorTests
    {
        string SourcePath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + "\\Test Generator\\Files";
        string PathToGenerated = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + "\\GeneratedTests\\Tests";

        IEnumerable<string> files;
        string[] generatedFiles;

        [SetUp]
        public void Setup()
        {
            files = Directory.GetFiles(SourcePath);
        }

        [Test]
        public void FilesNumber()
        {
            int expected = 1;
            int actual = files.Count();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestsGenerationSucceedsTest()
        {
            if (!Directory.Exists(PathToGenerated))
            {
                Directory.CreateDirectory(PathToGenerated);
            }
            try
            {
                Task task = new TestsGenerationManager().Generate(files, PathToGenerated, 5);
                task.Wait();
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void CorrectNumberOfGeneratedFilesTest()
        {
            if (!Directory.Exists(PathToGenerated))
            {
                Directory.CreateDirectory(PathToGenerated);
            }
            Task task = new TestsGenerationManager().Generate(files, PathToGenerated, 5);
            task.Wait();
            generatedFiles = Directory.GetFiles(PathToGenerated);
            int expected = 2;
            int actual = generatedFiles.Length;
            Assert.AreEqual(expected, actual);
        }
    }
}