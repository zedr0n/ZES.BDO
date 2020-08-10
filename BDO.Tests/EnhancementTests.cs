using System;
using System.Reactive.Linq;
using System.Threading;
using BDO.Enhancement.Commands;
using BDO.Enhancement.Queries;
using Xunit;
using Xunit.Abstractions;
using ZES.Infrastructure.Alerts;
using ZES.Infrastructure.Branching;
using ZES.Interfaces.Branching;
using ZES.Interfaces.Domain;
using ZES.Interfaces.Pipes;
using ZES.Utils;
using ILog = ZES.Interfaces.ILog;

namespace BDO.Tests
{
    public class EnhancementTests : BdoTest
    {
        public EnhancementTests(ITestOutputHelper outputHelper) 
            : base(outputHelper)
        {
        }

        [Fact]
        public async void CanCreateEnhancement()
        {
            var container = CreateContainer();
            var bus = container.GetInstance<IBus>();
            var repository = container.GetInstance<IEsRepository<IAggregate>>();

            await await bus.CommandAsync(new StartEnhancement("Test"));
            
            Assert.NotNull(await repository.Find<Enhancement.Enhancement>("Test"));
        }

        [Fact]
        public async void CanFailEnhancements()
        {
            var container = CreateContainer(useSagas: false);
            var bus = container.GetInstance<IBus>();
            
            var itemId = 44915.ToString();
            await await bus.CommandAsync(new StartEnhancement("Test"));
            await await bus.CommandAsync(new SetEnhancementInfo("Test", itemId, itemId, 2, 0, 0, 0, 0.7, 0));
            await await bus.CommandAsync(new FailEnhancement("Test"));

            var info = await bus.QueryUntil(new EnhancementResultsQuery("Test"), r => r.NumberOfAttempts > 0);
            
            Assert.Equal(1, info.NumberOfAttempts);
        }
           
        [Fact]
        public async void CanGetExpectedNumberOfAttempts()
        {
            
            var container = CreateContainer(useSagas: true);
            var bus = container.GetInstance<IBus>();
            var log = container.GetInstance<ILog>();
            var manager = container.GetInstance<IBranchManager>();
            var messageQueue = container.GetInstance<IMessageQueue>();

            var itemId = 44915.ToString();

            var total = 100;
            var nBatches = 2;

            var stats = await bus.QueryAsync(new StatsQuery());
            var totalSum = 0.0;
            for (var iBatch = 0; iBatch < nBatches; ++iBatch)
            {
                await manager.Branch($"test{iBatch}");

                var nTests = total / nBatches;
                for (int iTest = 0; iTest < nTests; iTest++)
                {
                    var test = $"Test{iTest}";
                    await await bus.CommandAsync(new StartEnhancement(test));
                    await await bus.CommandAsync(new SetEnhancementInfo(test, itemId, itemId, 2, 36, 0.1, 0.01, 0.5, 0.002));
                }

                await manager.Ready;
                /*await bus.QueryUntil(
                    new EnhancementResultsQuery($"Test{nTests - 1}"),
                    r => r.NumberOfAttempts > 0,
                    TimeSpan.FromSeconds(300));*/
                
                messageQueue.Alert(new InvalidateProjections());
                stats = await bus.QueryAsync(new StatsQuery());
                var res = ((double)stats.NumberOfFailures / nTests) + 1;
                log.Info($"Expected number of attempts for batch {iBatch} : {res}");
                
                totalSum += res;
                
                await manager.Branch(BranchManager.Master);
                await manager.DeleteBranch($"test{iBatch}");
            }

            var expectation = totalSum / nBatches;
            log.Info($"Expected number of attempts total : {expectation}");
            Assert.True(Math.Abs(expectation - 2.13) < 1);
        }
    }
}