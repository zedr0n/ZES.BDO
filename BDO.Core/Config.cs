using System.Reflection;
using BDO.Core.Commands;
using BDO.Core.Queries;
using SimpleInjector;
using ZES.Infrastructure;
using ZES.Infrastructure.GraphQl;
using ZES.Interfaces;
using ZES.Interfaces.Pipes;
using ZES.Utils;

#pragma warning disable SA1600

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
            c.RegisterAll(Assembly.GetExecutingAssembly(), false);
        }

        public class Query : GraphQlQuery
        {
            public Query(IBus bus)
                : base(bus)
            {
            }

            public ItemInfo ItemInfo(string name) => Resolve(new ItemInfoQuery(name));
        }

        public class Mutation : GraphQlMutation
        {
            public Mutation(IBus bus, ILog log) 
                : base(bus, log)
            {
            }

            public bool AddItem(string name) => Resolve(new AddItem(name));
        }
    }
}