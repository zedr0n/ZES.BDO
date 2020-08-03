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
            var grade = 0;

            await await bus.CommandAsync(new AddItem(name, grade));
            var item = await repository.Find<Item>(GetTarget(name, grade));
            
            Assert.NotNull(item);
        }

        [Fact]
        public async void CanSetItemId()
        {
            var container = CreateContainer(useSagas: false);
            var bus = container.GetInstance<IBus>();
            
            var name = "Memory Fragment";
            var grade = 0;
            await await bus.CommandAsync(new AddItem(name, 0));
            await await bus.CommandAsync(new UpdateItemInfo(name, grade, 44195));

            var item = await bus.QueryUntil(new ItemInfoQuery(name, grade), itemInfo => itemInfo.ItemId == 44915);
            Assert.Equal(44195, item.ItemId); 
        }

        [Fact]
        public async void CanGetItemIdFromNet()
        {
            var container = CreateContainer();
            var bus = container.GetInstance<IBus>();
            
            var name = "Memory Fragment";
            var grade = 0;
            await await bus.CommandAsync(new AddItem(name, grade));
            
            var item = await bus.QueryUntil(new ItemInfoQuery(name, grade), itemInfo => itemInfo.ItemId == 44195);
            Assert.Equal(44195, item.ItemId);
            Assert.Equal(0, item.Grade);
        }
        
        private string GetTarget(string name, int grade) => $"{name}_{grade}";
    }
}