using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator.MembersData
{
    public class ConstructorData
    {

        public string Name { get; private set; }

        public Dictionary<string, string> Parameters { get; private set; }

        public ConstructorData(string name, Dictionary<string, string> parametersMap)
        {
            Name = name;
            Parameters = parametersMap;
        }
    }
}
