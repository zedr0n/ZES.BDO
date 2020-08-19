using System;
using System.Reactive.Linq;
using System.Threading;
using BDO.Enhancement;
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

#pragma warning disable SA1600

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
            var container = CreateContainer(useSagas: false);
            var bus = container.GetInstance<IBus>();
            var repository = container.GetInstance<IEsRepository<IAggregate>>();

            await await bus.CommandAsync(new StartEnhancement("Test", "Item", 0, 0));
            
            Assert.NotNull(await repository.Find<Enhancement.Enhancement>("Test"));
        }

        [Fact]
        public async void CanFailEnhancements()
        {
            var container = CreateContainer(useSagas: false);
            var bus = container.GetInstance<IBus>();
            
            await await bus.CommandAsync(new StartEnhancement("Test", "Dummy", 0, 0));
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

            var total = 100;
            var nBatches = 2;
            RandomGenerator.Initialise(total);

            var stats = await bus.QueryAsync(new StatsQuery());
            var totalSum = 0.0;
            for (var iBatch = 0; iBatch < nBatches; ++iBatch)
            {
                await manager.Branch($"test{iBatch}");

                var nTests = total / nBatches;
                for (var iTest = 0; iTest < nTests; iTest++)
                {
                    var test = $"Batch{iBatch}_Test{iTest}";
                    await await bus.CommandAsync(new StartEnhancement(test, "Silver Embroidered", 1, 36));
                }

                await manager.Ready;
               
                log.Info($"Batch {iBatch} finished...");
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
        
        [Fact]
        public async void CanGetExpectedNumberOfAttemptsWithSaga()
        {
            var container = CreateContainer(useSagas: true);
            var bus = container.GetInstance<IBus>();
            var log = container.GetInstance<ILog>();
            var manager = container.GetInstance<IBranchManager>();

            var testsPerBatch = 50;
            var nPaths = 100;
            RandomGenerator.Initialise(nPaths);
            var nBatches = nPaths / testsPerBatch;
            var numberOfFailures = 0;

            var item = "Silver Embroidered";
            
            for (var iBatch = 0; iBatch < nBatches; iBatch++)
            {
                await manager.Branch($"SagaBatch{iBatch}");
                await await bus.CommandAsync(new CreateEnhancementTest($"SagaBatch{iBatch}", nPaths / nBatches, item, 1, 36));
                
                await manager.Ready;
                var results = await bus.QueryAsync(new EnhancementTestResultQuery($"SagaBatch{iBatch}"));
                numberOfFailures += results.NumberOfFailures;

                await manager.Branch(BranchManager.Master);
                await manager.DeleteBranch($"SagaBatch{iBatch}");
            }

            var expectation = ((double)numberOfFailures / nPaths) + 1;
            log.Info($"Expected number of attempts total : {expectation}");
            Assert.True(Math.Abs(expectation - 2.13) < 0.5);
        }

        [Fact]
        public void CanUseDecisionProcess()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();
            
            var item = "Gold accessory";
            var targetGrade = 2;
            var failstack = 0;
            var process = new EnhancementProcess(item, targetGrade, failstack);
            var policy = new JustEnhancePolicy(targetGrade);

            var value = process.GetOptimalValue(policy); 
            log.Info($"Optimal value : {value}");
            Assert.Equal(10.18571038208039, value);
        }
    }
}