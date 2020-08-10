using System.Reflection;
using SimpleInjector;
using ZES.Infrastructure;
using ZES.Utils;

namespace BDO.Enhancement
{
    public class Config
    {
        public static double HardCap => 0.9;
        
        [Registration]
        public static void RegisterAll(Container c)
        {
            c.RegisterAll(Assembly.GetExecutingAssembly());
        }

        public static void RegisterWithoutSagas(Container c)
        {
            c.RegisterAll(Assembly.GetExecutingAssembly(), false);
        }
    }
}