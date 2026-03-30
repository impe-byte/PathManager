using System;
using System.Linq;
using System.Reflection;

namespace PathManagerProfessional.Core.Tests
{
    public class TestRunner
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("  Running Core.Tests using Minimal Test Runner");
            Console.WriteLine("==============================================");
            int passed = 0;
            int failed = 0;

            var testClasses = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Name.EndsWith("Tests"));

            foreach (var testClass in testClasses)
            {
                var methods = testClass.GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any());

                var instance = Activator.CreateInstance(testClass);

                foreach (var method in methods)
                {
                    try
                    {
                        var result = method.Invoke(instance, null);
                        var task = result as System.Threading.Tasks.Task;
                        if (task != null)
                        {
                            task.GetAwaiter().GetResult();
                        }
                        Console.WriteLine(string.Format("[PASS] {0}", method.Name));
                        passed++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("[FAIL] {0}", method.Name));
                        Console.WriteLine(string.Format("       {0}", ex.InnerException != null ? ex.InnerException.Message : ex.Message));
                        failed++;
                    }
                }
            }

            Console.WriteLine("==============================================");
            Console.WriteLine(string.Format("Total: {0}, Passed: {1}, Failed: {2}", passed + failed, passed, failed));
            
            return failed == 0 ? 0 : 1;
        }
    }
}
