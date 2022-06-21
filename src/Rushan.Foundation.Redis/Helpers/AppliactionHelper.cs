using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Rushan.Foundation.Redis.Helpers
{
    internal static class ApplicationHelper
    {
        internal static string GetApplicationName()
        {
            var executedAssembly = Assembly.GetExecutingAssembly();

            if (executedAssembly == null)
                return null;

            string version = null;

            var versionInfo = FileVersionInfo.GetVersionInfo(executedAssembly.Location);

            var assemblyInformationalVersion =
                (AssemblyInformationalVersionAttribute)executedAssembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).
                    SingleOrDefault();

            if (assemblyInformationalVersion != null)
                version = assemblyInformationalVersion.InformationalVersion;
            else
            {
                if (!string.IsNullOrWhiteSpace(versionInfo.FileVersion))
                    version = versionInfo.FileVersion;
            }

            var applicationName =
                version == null ?
                    versionInfo.ProductName :
                    string.Format("{0} v{1}", versionInfo.ProductName, version);

            return applicationName;
        }
    }
}
