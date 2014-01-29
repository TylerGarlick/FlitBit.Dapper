using System.Configuration;
using FlitBit.Wireup;

namespace FlitBit.Dapper.Tests.Infrastructure
{
    public class AbstractTest
    {
        public string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["Db"].ToString(); }
        }

        public AbstractTest()
        {
            WireupCoordinator.SelfConfigure();
        }
    }
}
