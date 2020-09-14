using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using BDO.Enhancement;
using BDO.Enhancement.Commands;
using BDO.Enhancement.Queries;
using BDO.Enhancement.Stochastics;
using BDO.Enhancement.Stochastics.Policies;
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
        private readonly ITestOutputHelper _testOutputHelper;

        public EnhancementTests(ITestOutputHelper outputHelper, ITestOutputHelper testOutputHelper) 
            : base(outputHelper)
        {
            _testOutputHelper = testOutputHelper;
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
        public void CanGetExpectedNumberOfAttemptsViaProcess()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();
            
            var item = "Silver Embroidered";
            var targetGrade = 2;
            var failstack = 36;
            var initialState = new EnhancementState(failstack);
            initialState.Items[targetGrade - 1] = int.MaxValue;
            var process = new NumberOfAttemptsProcess(targetGrade, initialState) { Log = null };
            var policy = new JustEnhancePolicy(item, targetGrade) { TrackNumberOfAttempts = true };

            var value = process.GetOptimalValue(policy); 
            log.Info($"Optimal value : {value}");
            Assert.Equal(2.0855537713302703, value);
        }

        [Fact]
        public void CanUseDecisionProcess()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();
            
            var item = "Gold accessory";
            var targetGrade = 1;
            var failstack = 0;
            var process = new NumberOfAttemptsProcess(targetGrade, failstack) { Log = null };
            var policy = new JustEnhancePolicy(item, targetGrade) { TrackNumberOfAttempts = true };

            var value = process.GetOptimalValue(policy); 
            log.Info($"Optimal value : {value}");
            Assert.Equal(3.1213873601591193, value);
        }
        
        [Fact]
        public void CanCalculateSuccessProbability()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();
            
            var item = "Silver Embroidered";
            var targetGrade = 2;
            var failstack = 0;
            var initialState = new EnhancementState(failstack)
            {
                Items = new []
                {
                    50,
                    1,
                    0,
                    0,
                    0,
                },
            };
            var process = new SuccessProbabilityProcess(targetGrade, initialState) { Log = null };
            var policy = new JustEnhancePolicy(item, targetGrade);

            var value = process.GetOptimalValue(policy); 
          
            log.Info($"Enhancement probability : {value}");
            Assert.Equal(0.6391845543605477, value);
        }
        
        [Fact]
        public void CanCalculateSuccessProbabilityWithTieredFailstack()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();
            
            var item = "Silver Embroidered";
            var targetGrade = 3;
            // var quantities = Enumerable.Range(3, 3).Select(i => 10 * i);
            // var quantities = new List<int> { 20, 24, 28, 32, 36, 40 };
            var quantities = new List<int> { 16 };
            // var quantities = new List<int> { 8, 12, 16, 24, 28, 32 };
            
            var probabilities = new Dictionary<(int, int, int, int), double>();
            var costs = new Dictionary<(int, int, int, int), double>();
            var efficiency = new Dictionary<(int, int, int, int), double>();
            foreach (var quantity in quantities)
            {
                var initialState = new EnhancementState()
                {
                    Items = new []
                    {
                        quantity,
                        0,
                        0,
                        0,
                        0,
                    },
                    StoredFailstacks = new []
                    {
                        37,
                        24,
                        0,
                    },
                };
                
                for (var failstack0 = 5; failstack0 <= 10; ++failstack0)
                {
                    for (var failstack1 = 15; failstack1 <= 25; ++failstack1)
                    {
                        for(var failstack2 = 35; failstack2 <= 35; ++failstack2)
                        {
                            var key = (quantity, failstack0, failstack1, failstack2);
                            var process = new SuccessProbabilityProcess(targetGrade, initialState) { Log = null };
                            var costProcess = new ExpectedCostProcess(initialState) { Log = null };

                            var policy = new TieredFailstackPolicy(item, targetGrade, new Dictionary<int, int>
                            {
                                {0, failstack0},
                                {1, failstack1},
                                {2, failstack2},
                                {3, failstack2},
                            }) { Log = null };

                            probabilities[key] = process.GetOptimalValue(policy);
                            costs[key] = -costProcess.GetOptimalValue(policy, 1);
                            efficiency[key] = (int)(costs[key] / probabilities[key] / 100); 
                            // log.Info($"Enhancement probability for failstack = ({key}): {probabilities[key]} at {costs[key]}, efficiency : {efficiency[key]}");
                        }
                    }
                }
            }

            for (var p = 0.01; p < 1; p += 0.01)
            {
                var keys = probabilities.Where(x => Math.Abs(x.Value - p) < 0.005).Select(x => x.Key).ToList();
                if (!keys.Any())
                    continue;
                var max = keys.Select(k => efficiency[k]).Min();
                var key = efficiency.First(e => e.Value == max).Key;
                log.Info($"Most efficient for probability {probabilities[key]} : {key} at cost {max*100*probabilities[key]}");
            }
            
            // var max = probabilities.First(v => v.Value == probabilities.Max(s => s.Value));
            //log.Info($"Enhancement probability for failstack = ({max.Key}): {max.Value} at {-costs[max.Key]}");
        }
        
        [Fact]
        public void CanCalculateExpectedNumberOfItems()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();
            
            var item = "Gold accessory";
            var targetGrade = 2;
            var failstack = 0;
            var initialState = new EnhancementState(failstack)
            {
                Items = new []
                {
                    50,
                    1,
                    0,
                    0,
                    0,
                },
            };
            var process = new ExpectedNumberOfItemsProcess(targetGrade, initialState) { Log = null };
            var policy = new KeepEnhancingPolicy(item, targetGrade);

            var value = process.GetOptimalValue(policy); 
            log.Info($"Expected number of items : {value}");
            Assert.Equal(0.8641442918318539, value);
        }

        [Fact]
        public void CanCalculateFailstackCost()
        {
            
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();

            var prevValue = 0.0;
            for (var failstack = 1; failstack < 11; failstack++)
            {
                var initialState = new EnhancementState(0)
                {
                    Items = new []
                    {
                        0,
                        0,
                        0,
                        0,
                        1,
                        0,
                    },
                };
                var process = new ReblathCostProcess(failstack, initialState);
                var policy = new ReblathPolicy(failstack);

                var value = process.GetOptimalValue(policy, 100); 
                log.Info($"Failstack = {failstack} cost: {value - prevValue}");
                prevValue = value;
            }
            
            Assert.Equal(2734185.1255891407, prevValue);
        }

        [Fact]
        public void CanCalculateExpectedProfit()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();
            
            var item = "Silver Embroidered";
            var targetGrade = 2;
            var failstack = 0;
            var initialCost = failstack * 200000;
            var initialState = new EnhancementState(failstack)
            {
                Items = new []
                {
                    20,
                    0,
                    0,
                    0,
                    0,
                },
            };
            var process = new ExpectedProfitProcess(targetGrade, initialState) { Log = null };
            var policy = new JustEnhancePolicy(item, targetGrade);

            var value = process.GetOptimalValue(policy);
            
            log.Info($"Expected profit for failstack={failstack} : {value - initialCost}");
        }

        [Fact]
        public void CanCalculateExpectedCost()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();
            
            var item = "Silver Embroidered";
            var targetGrade = 3;

            var failstack0 = 2;
            var failstack1 = 18;
            var failstack2 = 23;
            
            var initialState = new EnhancementState(0)
            {
                Items = new []
                {
                    20,
                    1,
                    0,
                    0,
                    0,
                },
            };
            var process = new ExpectedCostProcess(initialState) { Log = null };
            var policy = new TieredFailstackPolicy(item, targetGrade, new Dictionary<int, int>
            {
                {0, failstack0},
                {1, failstack1},
                {2, failstack2}
            });
            var value = process.GetOptimalValue(policy, 1000);
            log.Info($"Expected cost for ({failstack0},{failstack1},{failstack2}) : {-value}");
        }
        
        [Fact]
        public void CanGetExpectedProfitWithTieredFailstack()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();
            
            var item = "Silver Embroidered";
            var targetGrade = 3;

            var minFailstack0 = 10;
            var maxFailstack0 = 10;
            
            var minFailstack1 = 15;
            var maxFailstack1 = 25;
            
            var minFailstack2 = 25;
            var maxFailstack2 = 35;
            
            var dict = new Dictionary<(int, int, int), double>();

            var quantities = Enumerable.Range(1, 1).Select(i => 10 * i);
            foreach (var quantity in quantities)
            {
                var prevMax0 = double.MinValue;
                for (var failstack0 = minFailstack0; failstack0 <= maxFailstack0; failstack0++)
                {
                    for (var failstack2 = minFailstack2; failstack2 <= maxFailstack2; ++failstack2)
                    {
                        for (var failstack1 = minFailstack1; failstack1 <= maxFailstack1; ++failstack1)
                        {
                            var initialState = new EnhancementState(0)
                            {
                                Items = new []
                                {
                                    quantity,
                                    0,
                                    0,
                                    0,
                                    0,
                                },
                            };
                            var process = new ExpectedProfitProcess(targetGrade, initialState) { Log = null };
                            var policy = new TieredFailstackPolicy(item, targetGrade, new Dictionary<int, int>
                            {
                                {0, failstack0},
                                {1, failstack1},
                                {2, failstack2},
                                {3, failstack2},
                            });
                            policy.StopAtOnce = false;

                            var value = process.GetOptimalValue(policy, 1000);
                            dict[(failstack0, failstack1, failstack2)] = value / quantity;
                            if (failstack1 > minFailstack1 && dict[(failstack0, failstack1 - 1, failstack2)] > value / quantity)
                                break;
                            // log.Info($"Expected profit for failstack=({failstack0},{failstack1},{failstack2}) : {value}");
                        }
                        // log.Info($"Expected profit from {quantity} items for +2 failstack={failstack2} and +0 failstack={failstack0} : {values.Max()}, per unit : {values.Max()/quantity} at +1 failstack={minFailstack1 + values.IndexOf(values.Max())}");
                    }
                    var max0 = dict.Where(v => v.Key.Item1 == failstack0).Max(v => v.Value);
                    var key0 = dict.Single(v => v.Value == max0).Key;
                    log.Info($"Expected profit from {quantity} items for failstack {key0} : {max0*quantity}, per unit : {max0}");
                    if (prevMax0 > max0)
                        break;
                    prevMax0 = max0;
                }

                // var max = dict.Max(v => v.Value);
                // var key = dict.Single(v => v.Value == max).Key;
                // log.Info($"Expected profit from {quantity} items for failstack {key} : {max*quantity}, per unit : {max}");
            }
        }            
    }
}