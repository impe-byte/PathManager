using System;
using System.Runtime.InteropServices;
using System.Security;
using PathManagerProfessional.Core.Domain;
using PathManagerProfessional.Core.Ports;

namespace PathManagerProfessional.Infrastructure
{
    public class Win32FileSystemAdapter : IFileSystemAdapter
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool MoveFileW(string lpExistingFileName, string lpNewFileName);

        public bool TryApplyTransaction(PathTransaction transaction, out string errorMessage)
        {
            try
            {
                if (transaction == null || string.IsNullOrEmpty(transaction.OriginalPath) || string.IsNullOrEmpty(transaction.ProposedPath))
                {
                    errorMessage = "Invalid transaction logic.";
                    return false;
                }

                string longExistingPath = GetLongPath(transaction.OriginalPath);
                string longNewPath = GetLongPath(transaction.ProposedPath);

                bool success = MoveFileW(longExistingPath, longNewPath);

                if (!success)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    errorMessage = string.Format("Win32 Error: {0}", errorCode);
                    return false;
                }

                errorMessage = null;
                return true;
            }
            catch (UnauthorizedAccessException uae)
            {
                errorMessage = string.Format("Access Denied: {0}", uae.Message);
                return false;
            }
            catch (SecurityException se)
            {
                errorMessage = string.Format("Security Exception: {0}", se.Message);
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = string.Format("Unexpected error: {0}", ex.Message);
                return false;
            }
        }

        private string GetLongPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;

            // Already prefixed
            if (path.StartsWith(@"\\?\")) return path;

            // UNC Path
            if (path.StartsWith(@"\\"))
            {
                // Convert \\server\share to \\?\UNC\server\share
                return @"\\?\UNC\" + path.Substring(2);
            }
            
            // Local drive path (e.g. C:\)
            return @"\\?\" + path;
        }
    }
}
