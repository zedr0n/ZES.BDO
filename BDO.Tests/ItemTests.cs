using BDO.Core;
using BDO.Core.Commands;
using Xunit;
using Xunit.Abstractions;
using ZES.Interfaces.Domain;
using ZES.Interfaces.Pipes;

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
    }
}