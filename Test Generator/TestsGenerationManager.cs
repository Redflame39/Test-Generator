using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestGenerator;

namespace ConsoleApp
{
    public class TestsGenerationManager
    {
        public Task Generate(IEnumerable<string> files, string pathToGenerated, int maxThreads)
        {
            var execOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxThreads };
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            var downloadStringBlock = new TransformBlock<string, string>
            (
                async path =>
                {
                    using (var reader = new StreamReader(path))
                    {
                        return await reader.ReadToEndAsync();
                    }
                },
                execOptions
            );
            var generateTestsBlock = new TransformManyBlock<string, KeyValuePair<string, string>>
            (
                async sourceCode =>
                {
                    CodeParser parser = new CodeParser();
                    var fileInfo = await Task.Run(() => parser.GetFileData(sourceCode));
                    TestsGenerator generator = new TestsGenerator();
                    return await Task.Run(() => generator.GenerateTests(fileInfo));
                },
                execOptions
            );
            var writeFileBlock = new ActionBlock<KeyValuePair<string, string>>
            (
                async fileNameCodePair =>
                {
                    using (var writer = new StreamWriter(pathToGenerated + '\\' + fileNameCodePair.Key + ".cs"))
                    {
                        Console.WriteLine(fileNameCodePair.Key);
                        await writer.WriteAsync(fileNameCodePair.Value);
                    }
                },
                execOptions
            );
            downloadStringBlock.LinkTo(generateTestsBlock, linkOptions);
            generateTestsBlock.LinkTo(writeFileBlock, linkOptions);

            foreach (var file in files)
            {
                downloadStringBlock.Post(file);
                Console.WriteLine(file);
            }

            downloadStringBlock.Complete();
            return writeFileBlock.Completion;
        }
    }
}
