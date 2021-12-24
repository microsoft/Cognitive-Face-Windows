using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLibrary
{
    public enum DataSourceType
    {
        [Description("Using an SQL connection string")]
        SqlConnectionString = 1,
        [Description("Using local Isolated Storage")]
        LocalIsolatedStorage = 2
    }
}
