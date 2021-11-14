using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator.MembersData
{
    public class FileData
    {
        public List<ClassData> Classes { get; private set; }

        public FileData(List<ClassData> classes)
        {
            Classes.AddRange(classes);
        }
    }
}
