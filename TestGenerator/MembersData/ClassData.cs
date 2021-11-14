using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator.MembersData
{
    public class ClassData
    {
        public string Name { get; private set; }

        public List<ConstructorData> Constructors { get; private set; }

        public List<MethodData> Methods { get; private set; }

        public ClassData(string name, List<ConstructorData> constructors, List<MethodData> methods)
        {
            Name = name;
            Constructors.AddRange(constructors);
            Methods.AddRange(methods);
        }
    }
}
