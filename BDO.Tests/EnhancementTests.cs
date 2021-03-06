using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using BDO.Enhancement;
using BDO.Enhancement.Commands;
using BDO.Enhancement.Queries;
using BDO.Enhancement.Static;
using BDO.Enhancement.Stochastics;
using BDO.Enhancement.Stochastics.Actions;
using BDO.Enhancement.Stochastics.Policies;
using BDO.Enhancement.Stochastics.Rewards;
using NLog.Fluent;
using Xunit;
using Xunit.Abstractions;
using ZES.Infrastructure;
using ZES.Infrastructure.Alerts;
using ZES.Infrastructure.Branching;
using ZES.Infrastructure.Stochastics;
using ZES.Interfaces.Branching;
using ZES.Interfaces.Domain;
using ZES.Interfaces.Pipes;
using ZES.Interfaces.Stochastic;
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
            var process = new EnhancementProcess(initialState)
                {Rewards = new List<IActionReward<EnhancementState>> {new NumberOfAttemptsReward(targetGrade)}};
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
            var initialState = new EnhancementState(failstack);
            var process = new EnhancementProcess(initialState)
                {Rewards = new List<IActionReward<EnhancementState>> {new NumberOfAttemptsReward(targetGrade)}};
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
            var process = new EnhancementProcess(initialState) { Rewards = new List<IActionReward<EnhancementState>> { new SuccessProbabilityReward(targetGrade) }};
            var policy = new JustEnhancePolicy(item, targetGrade);

            var value = process.GetOptimalValue(policy); 
          
            log.Info($"Enhancement probability : {value}");
            Assert.Equal(0.6391845543605477, value);
        }

        private class TierIterator
        {
            private int _quantity;
            private int[] _failstacks;

            private readonly int _targetGrade;
            private readonly int[] _minFailstacks;
            private readonly int[] _maxFailstacks;

            public int Quantity => _quantity;
            public int[] Failstacks => _failstacks;
            
            public TierIterator(int targetGrade, int quantity, int[] minFailstacks, int[] maxFailstacks)
                : this(targetGrade, quantity, minFailstacks)
            {
                _minFailstacks = minFailstacks;
                _maxFailstacks = maxFailstacks;
            }

            private TierIterator(int targetGrade, int quantity, int[] failstacks)
            {
                _targetGrade = targetGrade;
                _failstacks = new int[4];
                _quantity = quantity;
                failstacks.CopyTo(_failstacks, 0);
            }

            public TierIterator Next()
            {
                _failstacks[_targetGrade - 1]++;
                for (var i = _targetGrade - 1; i > 0; --i)
                {
                    if (_failstacks[i] <= _maxFailstacks[i]) 
                        continue;
                  
                    _failstacks[i] = _minFailstacks[i];
                    _failstacks[i - 1] += 1;
                }

                if (_failstacks[0] > _maxFailstacks[0])
                    return null;

                return this;
            }

            public TierIterator Skip(int grade = -1)
            {
                if (grade == -1)
                    grade = _targetGrade - 1;
                for (var i = grade; i < _targetGrade; ++i)
                {
                    while (_failstacks[i] < _maxFailstacks[i])
                        Next();
                }
                
                return this;
            }
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
            var quantities = new List<int> { 10 };
            // var quantities = new List<int> { 8, 12, 16, 24, 28, 32 };
            
            var probabilities = new Dictionary<(int, int, int, int, int), double>();
            var costs = new Dictionary<(int, int, int, int, int), double>();
            var efficiency = new Dictionary<(int, int, int, int, int), double>();
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
                        0,
                        0,
                        0,
                        0,
                    },
                };

                var minFailstack = new[] { 10, 19, 28, 40 };
                var maxFailstack = new[] { 10, 20, 30, 40 };

                for (var grade = targetGrade; grade < minFailstack.Length; ++grade)
                {
                    minFailstack[grade] = -1;
                    maxFailstack[grade] = -1;
                }
                
                var it = new TierIterator(targetGrade, quantity, minFailstack, maxFailstack);
                do
                {
                    // log.Info($"({it.Quantity}:{string.Join(',', it.Failstacks)})"); 
                    
                    var key = (quantity, it.Failstacks[0], it.Failstacks[1], it.Failstacks[2], it.Failstacks[3]);
                    var process = new EnhancementProcess(initialState) { Rewards = new List<IActionReward<EnhancementState>> { new SuccessProbabilityReward(targetGrade) }};
                    var costProcess = new EnhancementProcess(initialState)
                        {Rewards = new List<IActionReward<EnhancementState>> {new FailstackReward()}};

                    var policy = new TieredFailstackPolicy(item, targetGrade, new Dictionary<int, int>
                    {
                        { 0, it.Failstacks[0] },
                        { 1, it.Failstacks[1] },
                        { 2, it.Failstacks[2] },
                        { 3, it.Failstacks[3] },
                    }) { Log = null };

                    probabilities[key] = process.GetOptimalValue(policy);
                    costs[key] = -costProcess.GetOptimalValue(policy, 1);
                    efficiency[key] = (int)(costs[key] / probabilities[key] / 100); 
                    // log.Info($"Enhancement probability for {quantity} items and failstack ({string.Join(',', it.Failstacks.ToList().Where(i => i > 0).ToList())}): {probabilities[key]} at {costs[key]}, efficiency : {efficiency[key]}");
 
                    it = it.Next();
                }
                while (it != null);

                if (quantity == 10)
                    Assert.Equal(0.15627782835360016, probabilities[(10, 10, 19, 28, -1)]);
            }

            for (var p = 0.01; p < 1; p += 0.01)
            {
                var keys = probabilities.Where(x => Math.Abs(x.Value - p) < 0.005).Select(x => x.Key).ToList();
                if (!keys.Any())
                    continue;
                var max = keys.Select(k => efficiency[k]).Min();
                var key = efficiency.First(e => e.Value == max).Key;
                var quantity = key.Item1;
                var failstacks = (new List<int> {key.Item2, key.Item3, key.Item4, key.Item5}).Where(i => i > 0).ToList();
                log.Info($"Most efficient probability {probabilities[key]} : {quantity} items with failstack ({string.Join(',', failstacks)}) at cost {max*100*probabilities[key]}");
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
            var process = new EnhancementProcess(initialState)
                {Rewards = new List<IActionReward<EnhancementState>> {new NumberOfItemsReward(targetGrade)}};
            var policy = new JustEnhancePolicy(item, targetGrade) { StopAtOnce = false }; 

            var value = process.GetOptimalValue(policy); 
            log.Info($"Expected number of items : {value}");
            Assert.Equal(0.864144291831854, value);
        }

        [Fact]
        public void CanGetGrunilItemCosts()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();
            
            var item = "Boss";
            var targetGrade = 1;

            var costs = new Dictionary<(int, int, int, int, int), double>();
            var quantity = 10;
            var minFailstacks = new[] {5, 0, 20, 40};
            var maxFailstacks = new[] {10, 25, 35, 40};
            var it = new TierIterator(targetGrade, quantity, minFailstacks, maxFailstacks);
            
            var initialState = new EnhancementState
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

            var allKeys = new List<(int, int, int, int, int)>();
            
            do
            {
                allKeys.Add((quantity, it.Failstacks[0], it.Failstacks[1], it.Failstacks[2], it.Failstacks[3]));
                it = it.Next();
            } while (it != null);

            Parallel.ForEach(allKeys, key =>
            {
                var costProcess = new GrunilCostProcess(initialState) { Log = null };

                var failstacks = new[] {key.Item2, key.Item3, key.Item4, key.Item5};

                var policy = new TieredFailstackPolicy(item, targetGrade, new Dictionary<int, int>
                {
                    { 0, failstacks[0] },
                    { 1, failstacks[1] },
                    { 2, failstacks[2] },
                    { 3, failstacks[3] },
                }) { Log = null };

                var cost = costProcess.GetOptimalValue(policy, 100);
                lock (costs)
                {
                    costs[key] = cost;
                    
                    /*var k = (quantity, failstacks[0], failstacks[1], failstacks[2] - 1, failstacks[3]);
                    if (costs.ContainsKey(k) && costs[k] > cost)
                        it = it.Skip(2);

                    k = (it.Failstacks[0], it.Failstacks[1] - 1, it.Failstacks[2], it.Failstacks[3]);
                    if (dict.ContainsKey(k) && dict[k] > value / quantity)
                        it = it.Skip(1);*/
                }

                log.Info($"Enhancement cost for {quantity} items and failstack ({string.Join(',', failstacks.ToList().Where(i => i > 0).ToList())}): {costs[key]}");
            });
            
            /*do
            {
                // log.Info($"({it.Quantity}:{string.Join(',', it.Failstacks)})"); 
                
                var key = (quantity, it.Failstacks[0], it.Failstacks[1], it.Failstacks[2], it.Failstacks[3]);
                var costProcess = new ExpectedCostProcess(initialState) { Log = null };

                var policy = new TieredFailstackPolicy(item, targetGrade, new Dictionary<int, int>
                {
                    { 0, it.Failstacks[0] },
                    { 1, it.Failstacks[1] },
                    { 2, it.Failstacks[2] },
                    { 3, it.Failstacks[3] },
                }) { Log = null };

                costs[key] = -costProcess.GetOptimalValue(policy, 1);
                log.Info($"Enhancement cost for {quantity} items and failstack ({string.Join(',', it.Failstacks.ToList().Where(i => i > 0).ToList())}): {costs[key]}");
                // log.Info($"Enhancement probability for {quantity} items and failstack ({string.Join(',', it.Failstacks.ToList().Where(i => i > 0).ToList())}): {probabilities[key]} at {costs[key]}, efficiency : {efficiency[key]}");

                it = it.Next();
            }
            while (it != null);*/

            var min = costs.Min(x => x.Value);
            var minKey = costs.First(x => x.Value == min);

            log.Info($"Optimal cost at {minKey} : {min}");
        }

        [Fact]
        public void CanCalculateGrunilFailstackCost()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();

            var minFailstack = 28;
            var maxFailstack = 41;
            var prevValue = 0.0;
            var dict = new Dictionary<int, double>();
            var failstacks = Enumerable.Range(minFailstack, maxFailstack - minFailstack + 1);
            Parallel.ForEach(failstacks, failstack =>
                // for (var failstack = minFailstack + 1; failstack < maxFailstack; failstack++)
            {
                var initialState = new EnhancementState
                {
                    Items = new[]
                    {
                        10,
                        0,
                        0,
                        0,
                        0,
                        0
                    },
                };

                var process = new GrunilCostProcess(initialState);
                var policy = new GrunilPolicy(failstack, minFailstack, 4)
                {
                    FailstackToGrade = new Dictionary<int, int>
                    {
                        {34, 2},
                        {42, 3},
                    },
                };

                var value = process.GetOptimalValue(policy, 100);
                lock (dict)
                {
                    dict[failstack] = value;
                    // log.Info($"Failstack = {failstack} cost: {value - prevValue}");
                }

                log.Info($"Failstack = {failstack} cost: {value}");
                prevValue = value;
            });


            for (var failstack = minFailstack + 1; failstack < maxFailstack; failstack++)
            { 
                log.Info($"Failstack = {failstack} cost: {dict[failstack]}");
            }
            
            Assert.Equal(157240726.07937875, prevValue);
        }

        [Fact]
        public void CanCalculateGrunilFailstackOptimalCost()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();
            var item = "Grunil";
            var sellTarget = true;
            var targetGrade = 5;
            var sellGrade = 4;

            var initialState = new EnhancementState(0)
            {
                Items = new []
                {
                    0, 
                    1,
                    0,
                    0,
                    0,
                    0,
                },
            };

            var failstacks = Enumerable.Range(46, 1).ToList();
            var rewards = new List<IActionReward<EnhancementState>> { new EnhancementReward(item, targetGrade), new FailstackReward() };
            if (sellGrade > 0)
                rewards.Add(new SellReward(item, sellGrade));
            Parallel.ForEach(failstacks, failstack =>
            {
                var process = new EnhancementProcess(initialState)
                {
                    Rewards = rewards, 
                    Log = log,
                    UseReachableStateSpace = false,
                    LogProgress = failstacks.Count == 1,
                    RelativeOptimalTolerance = 1,
                };
                
                var policy = new TieredFailstackPolicy(item, targetGrade)
                {
                    // 46 => (9,30,37) [0]
                    // 50 => (9,29.36) [0]
                    // 59 => (9,28,35) [8]
                    Failstacks = new Dictionary<int, int>
                    {
                        {0, 4},
                        {1, 9},
                        {2, 29},
                        {3, 36},
                        {4, 45},
                    },
                    MinFailstacks = new Dictionary<int, int>
                    {
                        {0, 4},
                        {1, 9},
                        {2, 25},
                        {3, 30},
                        {4, 50}
                    },
                    MaxFailstacks = new Dictionary<int, int>
                    {
                        {0, 4},
                        {1, 30},
                        {2, 41},
                        {3, 60},
                        {4, 100}
                    },
                    StopAtOnce = false,
                    TargetFailstack = failstack,
                    SellGrade = sellGrade,
                    MaxFailstackShift = 0,
                };

                var tolerance = 1000;
                var cost = process.GetOptimalValue(policy, tolerance);
                log.Info($"Base cost for failstack={failstack} : {-cost}");

                var optimalCost = process.GetOptimalValueViaPolicyIteration(policy, out var optimalPolicy, tolerance);
                log.Info($"Optimal cost for failstack={failstack} : {-optimalCost}");

                var testCost = process.GetOptimalValue(optimalPolicy, tolerance);
                log.Info($"Test cost = {testCost}");
                if (failstack == 46)
                    Assert.Equal(83491365.28205448, -optimalCost);

                if (failstack != failstacks.Last())
                    return;

                var changes = process.Changes;
                var ordered = new List<(EnhancementState state, IMarkovAction<EnhancementState> optimalAction, IMarkovAction<EnhancementState> baseAction, double change)>();
                foreach (var state in optimalPolicy.Modifications)
                {
                    var action = optimalPolicy[state];
                    if (policy[state] == null)
                        log.Info($"{state} : {action} >> null");

                    if (policy[state] == null || action.ToString() == policy[state].ToString()) 
                        continue;
                    
                    changes.TryGetValue(state, out var change);
                    if (change > Math.Abs(cost)*0.025)
                        ordered.Add((state, action, policy[state], change));
                }
                foreach (var (state, action, baseAction, change) in ordered.OrderByDescending(x => x.change))
                    log.Info($"{state} : {action} >> {baseAction} [{change}]");
            });
        }
        
        [Fact]
        public void CanCalculateFailstackCost()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();

            var prevValue = 0.0;
            var failstacks = Enumerable.Range(1, 10);
            Parallel.ForEach(failstacks, new ParallelOptions { MaxDegreeOfParallelism = 1 }, failstack =>
            {
                var initialState = new EnhancementState(0)
                {
                    Items = new[]
                    {
                        0,
                        0,
                        0,
                        0,
                        1,
                        0,
                    },
                };
                var process = new EnhancementProcess(initialState)
                {
                    Rewards = new List<IActionReward<EnhancementState>>
                        {new ReblathReward(), new CleanseReward() },
                };
                var policy = new ReblathPolicy(failstack);

                var value = process.GetOptimalValue(policy, 100);
                log.Info($"Failstack = {failstack} cost: {value}");
                prevValue = value;
            });
            
            Assert.Equal(2662495.2529060086, prevValue);
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
            var process = new ExpectedProfitProcess(item, targetGrade, initialState) { Log = null };
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
            var process = new EnhancementProcess(initialState)
                {Rewards = new List<IActionReward<EnhancementState>> {new FailstackReward(false)}};
            var policy = new TieredFailstackPolicy(item, targetGrade, new Dictionary<int, int>
            {
                {0, failstack0},
                {1, failstack1},
                {2, failstack2}
            });
            var value = process.GetOptimalValue(policy, 1000);
            log.Info($"Expected cost for ({failstack0},{failstack1},{failstack2}) : {value}");
            
            Assert.Equal(18491165.27436404, value);
        }

        [Fact]
        public void CanCalculateLoggiaAccessoryCost()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();
            
            var item = "Loggia Accessory";
            var targetGrade = 2;

            var initialState = new EnhancementState(0);
           
            var process = new EnhancementProcess(initialState)
            {
                Rewards = new List<IActionReward<EnhancementState>> { new EnhancementReward(item, targetGrade), new FailstackReward() },
                Log = log,
            };
            
            var policy = new JustEnhancePolicy(item, targetGrade);
            var tolerance = 1000;
            var value = process.GetOptimalValueAndVariance(policy, tolerance);
            log.Info($"Expected cost for +{targetGrade} : {-value.Mean} at variance {value.Variance}");
            if (targetGrade == 3)
                Assert.Equal(-42998725.41710255, value.Mean);
            else if (targetGrade == 2)
                Assert.Equal(-9981311.112739593, value.Mean);
            
            // var graph = new EnhancementGraph(policy, initialState, process.NumberOfIterations);
            // graph.Serialize(nameof(CanCalculateLoggiaAccessoryCost));
        }
        
        [Fact]
        public void CanCalculateLoggiaCost()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();
            
            var item = "Loggia"; 
            var targetGrade = 3;

            var initialState = new EnhancementState(0)
            {
                Items = new []
                {
                    0,
                    1,
                    0,
                    0,
                    0,
                    0,
                },
                NumberOfValks = 0,
            };

            var process = new EnhancementProcess(initialState)
            {
                Rewards = new List<IActionReward<EnhancementState>> { new EnhancementReward(item, targetGrade), new FailstackReward(), new BookReward() },
                Log = log,
                UseReachableStateSpace = false,
            };
            
            // (9,14,29) for TRI
            var policy = new TieredFailstackPolicy(item, targetGrade, new Dictionary<int, int>
            {
                {0, 9},
                {1, 14},
                {2, 29},
                {3, 33},
            })
            {
                MaxGradeFailstacks = new Dictionary<int, int>
                {
                    {0, 10},
                    {1, 20},
                    {2, 30},
                    {3, 45},
                },
                BookFailstack = 40,
            };
            double tolerance = 10000;
            
            var value = process.GetOptimalValue(policy, tolerance);
            log.Info($"Expected cost for {string.Join(',', policy.Failstacks.Values)} : {-value}");

            var optimalValue = process.GetOptimalValueViaPolicyIteration(policy, out var optimalPolicy, tolerance);
            log.Info($"Optimal cost : {-optimalValue}");

            var valueAndVariance = process.GetOptimalValueAndVariance(optimalPolicy, tolerance);
            log.Info($"Optimal policy cost : {-valueAndVariance.Mean} at variance {valueAndVariance.Variance}");
            
            foreach (var state in optimalPolicy.Modifications)
            {
                var action = optimalPolicy[state]; 
                if (policy[state] != null && action.ToString() != policy[state].ToString())
                    log.Info($"{state} : {action} >> {policy[state]}");     
            }
        }
        
        [Fact]
        public void CanCalculateOptimalProfitViaPolicyIteration()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();
            
            var item = "Silver Embroidered";
            var targetGrade = 3;

            var check = true;
            var quantities = new List<int> { 10 };
            var infos = Data.EnhancementInfos.Where(i => i.Name == item).ToArray();
            
            Parallel.ForEach(quantities, quantity =>
            {
                var initialState = new EnhancementState(0)
                {
                    Items = new[]
                    {
                        quantity,
                        0,
                        0,
                        0,
                        0,
                    },
                };

                var policy = new TieredFailstackPolicy(item, targetGrade)
                {
                    StopAtOnce = false,
                    Failstacks = new Dictionary<int, int>
                    {
                        { 0, 0 },
                        { 1, 0 },
                        { 2, 0 },
                        { 3, 0 },
                    },
                    MaxGradeFailstacks = new Dictionary<int, int>
                    {
                        { 0, 15 },
                        { 1, 22 },
                        { 2, 30 }, 
                    },
                };
                var process = new EnhancementProcess(initialState)
                {
                    Rewards = new List<IActionReward<EnhancementState>> { new ProfitReward(targetGrade, infos), new FailstackReward()},
                    Log = log,
                };

                double tol = 100;
                
                var baseValue = process.GetOptimalValue(policy, tol);
                if (quantity == 10 && check)
                    Assert.Equal(869292.7303920856, baseValue);
                log.Info($"Base value : {baseValue}");

                var value = process.GetOptimalValueViaPolicyIteration(policy, out var optimalPolicy, tol);
                if (quantity == 10 && check)
                    Assert.Equal(3743108.641005154, value);
                
                log.Info($"Optimal value : {value}");

                var optimalValue = process.GetOptimalValue(optimalPolicy, 100);
                log.Info($"Test optimal value: {optimalValue}");

                /*var otherPolicy = new TieredFailstackPolicy(item, targetGrade)
                {
                    StopAtOnce = false,
                    Failstacks = new Dictionary<int, int>
                    {
                        { 0, 30 },
                        { 1, 30 },
                        { 2, 30 },
                        { 3, 30 },
                    }
                };

                var otherValue =
                    process.GetOptimalValueViaPolicyIteration(otherPolicy, out var otherOptimalPolicy, 1000);
                
                foreach (var state in optimalPolicy.Modifications.Union(policy.Modifications))
                {
                    var action = optimalPolicy[state];
                    var otherAction = otherOptimalPolicy[state];
                    if (action.ToString() != otherAction.ToString())
                        log.Info($"({string.Join(',', state.Items)}) |{state.FailStack}| [{state.StoredFailstacks[0]},{state.StoredFailstacks[1]}] : {action} >> {otherAction}");     
                }*/

                 var testPolicy = new TieredFailstackPolicy(item, targetGrade)
                 {
                     StopAtOnce = false,
                     Failstacks = policy.Failstacks,
                 };

                
                foreach (var state in optimalPolicy.Modifications)
                {
                    var action = optimalPolicy[state]; 
                    if (testPolicy[state] != null && action.ToString() != testPolicy[state].ToString())
                        log.Info($"({string.Join(',', state.Items)}) |{state.FailStack}| [{state.StoredFailstacks[0]},{state.StoredFailstacks[1]}] : {action} >> {testPolicy[state]}");     
                }
            });
        }

        [Fact]
        public void CanGetExpectedProfitWithTieredFailstack()
        {
            var container = CreateContainer();
            var log = container.GetInstance<ILog>();
            
            var item = "Silver Embroidered";
            var targetGrade = 3;

            var quantities = Enumerable.Range(1, 1).Select(i => 10 * i);
            var infos = Data.EnhancementInfos.Where(i => i.Name == item).ToArray();
            
            // foreach (var quantity in quantities)
            Parallel.ForEach(quantities, quantity =>
            {
                var initialState = new EnhancementState(0)
                {
                    Items = new[]
                    {
                        quantity,
                        0,
                        0,
                        0,
                        0,
                    },
                };

                var policy = new TieredFailstackPolicy(item, targetGrade) { StopAtOnce = false };
                var minFailstacks = new[] {10, 15, 24, 35};
                var maxFailstacks = new[] {10, 25, 35, 40};

                var dict = new Dictionary<(int, int, int, int), double>();
                var dictValue = new Dictionary<(int, int, int, int), Value>();
                var it = new TierIterator(targetGrade, quantity, minFailstacks, maxFailstacks);
                do
                {
                    var process = new EnhancementProcess(initialState)
                    {
                        Rewards = new List<IActionReward<EnhancementState>> { new ProfitReward(targetGrade, infos), new FailstackReward()}
                    };
                    // policy = new TieredFailstackPolicy(item, targetGrade);
                    policy.Failstacks = new Dictionary<int, int>
                    {
                        {0, it.Failstacks[0]},
                        {1, it.Failstacks[1]},
                        {2, it.Failstacks[2]},
                        {3, it.Failstacks[3]},
                    };
                    // policy.MaxGradeFailstacks = new Dictionary<int, int> { { 2, 30 } };

                    var valueAndVariance = process.GetOptimalValueAndVariance(policy, 1000);
                    var value = valueAndVariance.Mean;
                    var k = (it.Failstacks[0], it.Failstacks[1], it.Failstacks[2], it.Failstacks[3]);

                    dict[k] = value / quantity;
                    dictValue[k] = valueAndVariance;
                    
                    // log.Info(
                    //    $"Expected optimal profit from {quantity} items for failstack {k} : {dict[k] * quantity}, per unit : {dict[k]} at variance {dictValue[k].Variance} ({ dictValue[k].Variance / dict[k] / quantity} )");
                    // log.Info($"Expected profit for failstack=({string.Join(',', it.Failstacks)}) : {value}");
                    k = (it.Failstacks[0], it.Failstacks[1], it.Failstacks[2] - 1, it.Failstacks[3]);
                    if (dict.ContainsKey(k) && dict[k] > value / quantity)
                        it = it.Skip(2);

                    // k = (it.Failstacks[0], it.Failstacks[1] - 1, it.Failstacks[2], it.Failstacks[3]);
                    // if (dict.ContainsKey(k) && dict[k] > value / quantity)
                    //    it = it.Skip(1);

                    //k = (it.Failstacks[0] - 1, it.Failstacks[1], it.Failstacks[2], it.Failstacks[3]);
                    //if (dict.ContainsKey(k) && dict[k] > value / quantity)
                    //    it = it.Skip(0);

                    it = it.Next();
                } while (it != null);

                var max = dict.Max(v => v.Value);
                var key = dict.Single(v => v.Value == max).Key;
                policy.Failstacks = new Dictionary<int, int>
                {
                    {0, key.Item1},
                    {1, key.Item2},
                    {2, key.Item3},
                    {3, key.Item4},
                };
                    
                var probabilityProcess = new EnhancementProcess(initialState) { Rewards = new List<IActionReward<EnhancementState>> { new SuccessProbabilityReward(targetGrade) } }; 
                var successProbability = probabilityProcess.GetOptimalValue(policy, 1e-6);
                log.Info(
                    $"Expected optimal profit from {quantity} items for failstack {key} : {max * quantity}, per unit : {max} at variance {dictValue[key].Variance} ({ dictValue[key].Variance / dict[key] / quantity} ) and probability {successProbability}");

                /*log.Info($"Profit cdf from {quantity} items for failstack {key}:");
                foreach (var x in dictValue[key].Cdf.Abscissas)
                    log.Info($"P({x}) = {dictValue[key].Cdf[x]}");*/

                if (dictValue.Any(v => dict[v.Key] > 0))
                {
                    var min = dictValue.Where(v => dict[v.Key] > 0).Min(v => Math.Abs(v.Value.Variance / dict[v.Key] / quantity));
                    var minKey = dictValue.Single(v => v.Value.Variance / dict[v.Key] / quantity == min).Key;
                    probabilityProcess = new EnhancementProcess(initialState) { Rewards = new List<IActionReward<EnhancementState>> { new SuccessProbabilityReward(targetGrade) } }; 
                    policy.Failstacks = new Dictionary<int, int>
                    {
                        {0, minKey.Item1},
                        {1, minKey.Item2},
                        {2, minKey.Item3},
                        {3, minKey.Item4},
                    };
                    successProbability = probabilityProcess.GetOptimalValue(policy, 1e-6);
                    log.Info(
                        $"Expected minvar profit from {quantity} items for failstack {minKey} : {dict[minKey] * quantity}, per unit : {dict[minKey]} at variance {dictValue[minKey].Variance} ( {dictValue[minKey].Variance / dict[minKey] / quantity} ) and probability {successProbability}");
                }
                
                /*for (var i = minFailstacks[0]; i < maxFailstacks[0]; ++i)
                {
                    var tKey = (i, key.Item2, key.Item3, key.Item4);
                    log.Info(
                        $"Expected profit from {quantity} items for failstack {tKey} : {dict[tKey] * quantity}, per unit : {dict[tKey]}");
                }*/

                if (quantity == 10 && key.Item1 == 10)
                    Assert.Equal(-92648.60297140495, max);
            });
        }            
    }
}