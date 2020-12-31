using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyTestFunction
{
public static class ApplicationHelper
    {
        private static bool IsStarted = false;
        private static object _syncLock = new object();
        ///<summary>
        /// Sets up the app before running any other code
        /// </summary>
 
        public static void Startup(ILogger logger)
        {
            if (!IsStarted)
            {
                lock (_syncLock)
                {
                    if (!IsStarted)
                    {
                        //AssemblyLoadingHelper.ConfigureAssemblyLoading(logger);
                        IsStarted = true;
                    }
                }
            }
        }
    }
}
