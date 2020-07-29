using BDO.Core;
using BDO.Core.Commands;
using BDO.Core.Queries;
using Xunit;
using Xunit.Abstractions;
using ZES.Interfaces.Domain;
using ZES.Interfaces.Pipes;
using ZES.Utils;

namespace BDO.Tests
{
    public class ItemTests : BdoTest
    {
        public ItemTests(ITestOutputHelper outputHelper) 
            : base(outputHelper)
        {
        }

        [Fact]
        public async void CanAddItem()
        {
            var container = CreateContainer();
            var bus = container.GetInstance<IBus>();
            var repository = container.GetInstance<IEsRepository<IAggregate>>();

            var name = "Memory Fragment";

            await await bus.CommandAsync(new AddItem(name));
            var item = await repository.Find<Item>(name);
            
            Assert.NotNull(item);
        }

        [Fact]
        public async void CanSetItemId()
        {
            var container = CreateContainer();
            var bus = container.GetInstance<IBus>();
            
            var name = "Memory Fragment";
            await await bus.CommandAsync(new AddItem(name));
            await await bus.CommandAsync(new UpdateItemInfo(name, 44195));

            await bus.QueryUntil(new ItemInfoQuery(name), itemInfo => itemInfo.ItemId == 44915);
        }
    }
}