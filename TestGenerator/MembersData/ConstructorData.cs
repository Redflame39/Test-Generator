using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator.MembersData
{
    public class ConstructorData
    {
        public string ReturnValueType { get; private set; }

        public string Name { get; private set; }

        public Dictionary<string, string> Parameters { get; private set; }

        public ConstructorData(string returnValue, string name, Dictionary<string, string> parametersMap)
        {
            ReturnValueType = returnValue;
            Name = name;
            Parameters = parametersMap;
        }
    }
}
