using System.Reflection;
using SimpleInjector;
using ZES.Infrastructure;
using ZES.Utils;

namespace BDO.Core
{
    public class Config
    {
        [Registration]
        public static void RegisterAll(Container c)
        {
            c.RegisterAll(Assembly.GetExecutingAssembly());
        }

        public static void RegisterWithoutSagas(Container c)
        {
            var assembly = Assembly.GetExecutingAssembly();
            c.RegisterEvents(assembly);
            c.RegisterAlerts(assembly);
            c.RegisterCommands(assembly);
            c.RegisterQueries(assembly);
            c.RegisterProjections(assembly);
            c.RegisterAggregates(assembly);
        }
    }
}