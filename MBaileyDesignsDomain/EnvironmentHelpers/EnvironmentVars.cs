using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MBaileyDesignsDomain.EnvironmentHelpers
{

    public static class EnvironmentVars
    {
        public static string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";
        public static string? GetEnvironmentVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }
    }
}
