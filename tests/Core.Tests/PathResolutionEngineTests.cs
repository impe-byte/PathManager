using System.Linq;
using PathManagerProfessional.Core.Domain;
using PathManagerProfessional.Core.Engine;

namespace PathManagerProfessional.Core.Tests
{
    // Minimal mock for xUnit Fact since no NuGet
    public class FactAttribute : System.Attribute { }
    
    public static class Assert
    {
        public static void Single<T>(System.Collections.Generic.IEnumerable<T> collection)
        {
            if (collection.Count() != 1) throw new System.Exception(string.Format("Expected 1 item, got {0}", collection.Count()));
        }
        public static void Equal<T>(T expected, T actual)
        {
            if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(expected, actual))
                throw new System.Exception(string.Format("Expected: {0}, Actual: {1}", expected, actual));
        }
        public static void True(bool condition, string message = "")
        {
            if (!condition) throw new System.Exception("Expected true but was false: " + message);
        }
    }

    public class PathResolutionEngineTests
    {
        [Fact]
        public void Should_Generate_Truncate_Transaction_When_Path_Exceeds_Threshold()
        {
            // Arrange
            var engine = new PathResolutionEngine();
            string extension = ".txt";
            string baseDir = @"C:\Shares\DepartmentData\";
            string name = new string('A', 230);
            string longPath = baseDir + name + extension; // 25 + 230 + 4 = 259 chars
            string[] badPaths = new string[] { longPath };
            int threshold = 250;

            // Act
            var results = engine.GenerateResolutionPlan(badPaths, threshold).ToList();

            // Assert
            Assert.Single(results);
            var transaction = results.First();
            Assert.Equal(TransactionType.Truncate, transaction.Type);
            Assert.True(transaction.ProposedPath.Length <= threshold, "Proposed path is not under threshold");
            Assert.True(transaction.ProposedPath.EndsWith(".txt"), "Extension was lost");
            Assert.Equal(longPath.Length - threshold, transaction.ExcessCharacters);
        }
    }
}
