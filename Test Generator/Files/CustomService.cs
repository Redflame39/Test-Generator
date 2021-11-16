using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Files
{
    public class CustomService
    {

        private IDependency _dependency;

        public CustomService(IDependency dependency)
        {
            _dependency = dependency;
        }

        public int IntService(CustomEntity entity, int id)
        {
            return 0;
        }

        public double DoubleService(string s)
        {
            return 0;
        }

        public string StringService(long l)
        {
            l = 1;
            return "Hello, World!";
        }

        public DateTime DateTimeService()
        {
            return DateTime.Now;
        }

        public void VoidService()
        {

        }

        private void PrivateService()
        {

        }

        protected void ProtectedService()
        {

        }
    }

    public interface IDependency
    {
        public CustomEntity CreateCustomEntity(string name, double value);

        public string DependencyString(List<string> list);
    }

    public class CustomDependency : IDependency
    {
        public object Obj { get; set; }

        public CustomDependency()
        {

        }

        public CustomEntity CreateCustomEntity(string name, double value)
        {
            return new CustomEntity();
        }

        public string DependencyString(List<string> list)
        {
            return list.ToString();
        }

    }

    public class CustomEntity
    {
        public String EntityString { get; set; }

        public double EntityDouble { get; set; }
    }
}
