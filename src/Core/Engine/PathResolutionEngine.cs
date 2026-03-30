using System.Collections.Generic;
using System.IO;
using PathManagerProfessional.Core.Domain;

namespace PathManagerProfessional.Core.Engine
{
    public class PathResolutionEngine
    {
        public IEnumerable<PathTransaction> GenerateResolutionPlan(IEnumerable<string> badPaths, int threshold)
        {
            var plan = new List<PathTransaction>();

            foreach (var path in badPaths)
            {
                if (path.Length > threshold)
                {
                    int excess = path.Length - threshold;
                    string extension = Path.GetExtension(path);
                    string directory = Path.GetDirectoryName(path) ?? string.Empty;
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);

                    int targetNameLength = fileNameWithoutExtension.Length - excess;
                    
                    if (targetNameLength <= 0)
                    {
                        targetNameLength = 1;
                    }

                    string newName = fileNameWithoutExtension.Substring(0, targetNameLength);
                    string newFileName = string.Concat(newName, extension);
                    string proposedPath = string.IsNullOrEmpty(directory) ? newFileName : Path.Combine(directory, newFileName);

                    plan.Add(new PathTransaction(
                        path,
                        proposedPath,
                        TransactionType.Truncate,
                        excess,
                        string.Format("Path exceeds threshold of {0} by {1} characters.", threshold, excess)
                    ));
                }
            }

            return plan;
        }
    }
}
