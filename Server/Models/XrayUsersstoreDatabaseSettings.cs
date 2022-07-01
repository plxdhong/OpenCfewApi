using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CFEW.Server.Models
{
    public class XrayUsersstoreDatabaseSettings : IXrayUsersstoreDatabaseSettings
    {
        public string XrayUsersCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
    public interface IXrayUsersstoreDatabaseSettings
    {
        string XrayUsersCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
