using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BDO.Enhancement.Static;
using ZES.Infrastructure.Stochastics;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement
{
    [DebuggerDisplay("{DebuggerDisplay}, {NumberOfAttempts}, {FailStack}")]
    public class EnhancementState : IMarkovState, IEquatable<EnhancementState>
    {
        public EnhancementState(int failStack = 0)
        {
            Items = new List<int> { int.MaxValue, 0, 0, 0, 0 };
            NumberOfAttempts = 0;
            FailStack = failStack;
        }

        private string DebuggerDisplay
        {
            get
            {
                return $"[{Items.Aggregate(string.Empty, (s, i) => s + ( i == int.MaxValue ? "-" : i.ToString() ) + ",")}]";
            }
        }

        public List<int> Items { get; set; }
        public int NumberOfAttempts { get; set; }
        public int FailStack { get; set; }

        public EnhancementState Clone(Action<EnhancementState> action = null)
        {
            var state = new EnhancementState();
            for (var grade = 0; grade < 5; grade++)
                state.Items[grade] = Items[grade];

            state.NumberOfAttempts = NumberOfAttempts;
            state.FailStack = FailStack;
            action?.Invoke(state);

            return state;
        }

        /// <inheritdoc/>
        public bool Equals(EnhancementState other)
        {
            if (ReferenceEquals(null, other)) 
                return false;
            if (ReferenceEquals(this, other)) 
                return true;

            var b = true;
            for (var i = 0; i < Items.Count; ++i)
                b &= Items[i] == other.Items[i];
            
            return b && NumberOfAttempts == other.NumberOfAttempts && FailStack == other.FailStack;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((EnhancementState)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 0;
                foreach (var i in Items)
                    hashCode = (hashCode * 397) ^ i.GetHashCode();
                hashCode = (hashCode * 397) ^ NumberOfAttempts;
                hashCode = (hashCode * 397) ^ FailStack;
                return hashCode;
            }
        }
    }

    /// <inheritdoc />
    public class EnhancementAction : IMarkovAction<EnhancementState>
    {
        public EnhancementAction(int grade)
        {
            Grade = grade;
        }

        public int Grade { get; }

        public IEnumerable<EnhancementState> this[EnhancementState current]
        {
            get
            {
                var list = new List<EnhancementState>();
                if (current.Items[Grade - 1] > 0)
                {
                    list.AddRange(
                    new List<EnhancementState>
                    {
                        current.Clone(s =>
                        {
                            s.Items[Grade]++;
                            s.NumberOfAttempts++;
                            s.FailStack = 0;
                        }),
                        current.Clone(s =>
                        {
                            s.FailStack++;
                            s.NumberOfAttempts++;
                        }),
                    });
                }

                return list;
            }
        } 
    }

    /// <inheritdoc />
    public class FailstackAction : IMarkovAction<EnhancementState>
    {
        public IEnumerable<EnhancementState> this[EnhancementState current] => new List<EnhancementState>
        {
            current.Clone(s => s.FailStack++),
        };
    }

    /// <inheritdoc />
    public class EnhancementProbability : ITransitionProbability<EnhancementState>
    {
        private readonly Enhancement _enhancement;
        private readonly Failstack _failstack;

        public EnhancementProbability(string item)
        {
            _enhancement = new Enhancement(item);
            _failstack = new Failstack();
        }
        
        public double this[EnhancementState from, EnhancementState to, IMarkovAction<EnhancementState> action]
        {
            get
            {
                switch (action)
                {
                    case EnhancementAction enhancementAction:
                        return _enhancement[from, to, enhancementAction];
                    case FailstackAction failstackAction:
                        return _failstack[from, to, failstackAction];
                }

                return 0.0;
            }
        }

        /// <inheritdoc />
        private class Failstack : ITransitionProbability<EnhancementState, FailstackAction>
        {
            public double this[EnhancementState from, EnhancementState to, FailstackAction action] => 
                action[from].All(s => !s.Equals(to)) ? 1.0 : 0.0;
        }

        /// <inheritdoc />
        private class Enhancement : ITransitionProbability<EnhancementState, EnhancementAction>
        {
            private readonly Dictionary<int, Data.EnhancementInfo> _info = new Dictionary<int, Data.EnhancementInfo>();
       
            public Enhancement(string item)
            {
                for (var grade = 0; grade < 5; grade++)
                    _info[grade] = Data.EnhancementInfos.SingleOrDefault(i => i.IsFor(item, grade));
            }
        
            public double this[EnhancementState from, EnhancementState to, EnhancementAction action]
            {
                get
                {
                    var states = action[from];
                    if (states.All(s => !s.Equals(to)))
                        return 0;

                    var chance = GetChance(action.Grade, from.FailStack);
                    if (to.Items[action.Grade] == from.Items[action.Grade]) // failure
                        return 1.0 - chance;
                    return chance;
                }
            }

            private double GetChance(int grade, int failstack)
            {
                var info = _info[grade - 1];
                var chance = info.BaseChance;
                while (--failstack >= 0)
                {
                    chance += chance > info.SoftCap ? info.SoftCapIncrease : info.BaseIncrease;
                    if (chance >= Config.HardCap)
                        return Config.HardCap;
                }

                return chance;
            }
        }
    }
    
    public class NumberOfAttempsReward : IReward<EnhancementState>
    {
        private readonly EnhancementProbability _enhancementProbability;
        private readonly EnhancementReward _enhancementReward;

        public NumberOfAttempsReward(string item, int targetGrade)
        {
            _enhancementProbability = new EnhancementProbability(item);
            _enhancementReward = new EnhancementReward(targetGrade);
        }
        
        public double this[IMarkovAction<EnhancementState> action, EnhancementState state]
        {
            get
            {
                var states = action[state];
                var expectation = 0.0;
                foreach (var s in states)
                    expectation += _enhancementProbability[state, s, action] * _enhancementReward[state, s, action];

                return expectation;
            }
        }

        private class EnhancementReward
        {
            private readonly int _targetGrade;

            public EnhancementReward(int targetGrade)
            {
                _targetGrade = targetGrade;
            }
            
            public double this[EnhancementState from, EnhancementState to, IMarkovAction<EnhancementState> action]
            {
                get
                {
                    if (action[from].All(s => !s.Equals(to)))
                        return 0.0;

                    if (!(action is EnhancementAction))
                        return 0.0;
                    
                    if (to.Items[_targetGrade] == 1 && from.Items[_targetGrade] == 0)
                        return to.NumberOfAttempts;

                    return 0.0;
                }
            }
        }
    }

    public class JustEnhancePolicy : IPolicy<EnhancementState>
    {
        private readonly int _targetGrade;

        public JustEnhancePolicy(int targetGrade)
        {
            _targetGrade = targetGrade;
        }
        
        public double this[IMarkovAction<EnhancementState> action, EnhancementState state]
        {
            get
            {
                if (state.Items[_targetGrade] > 0)
                    return 0.0;
                
                var grade = state.Items.FindLastIndex(i => i > 0) + 1;

                if (action is EnhancementAction enhancementAction)
                    return enhancementAction.Grade == grade ? 1.0 : 0.0;

                return 0.0;
            }
        }
    }

    public class NextValueFunction : IValueFunction<EnhancementState>
    {
        private readonly int _targetGrade;
        private readonly Dictionary<EnhancementState, double> _map = new Dictionary<EnhancementState, double>();
        
        public NextValueFunction(int targetGrade)
        {
            _targetGrade = targetGrade;
        }

        public double this[EnhancementState s]
        {
            get => _map[s];
            set => _map[s] = value;
        }
    }

    /// <inheritdoc />
    public class EnhancementProcess : MarkovDecisionProcessBase<EnhancementState, EnhancementProbability>
    {
        private readonly int _targetGrade;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancementProcess"/> class.
        /// </summary>
        /// <param name="item">Item to enhance</param>
        /// <param name="targetGrade">Target grade</param>
        /// <param name="failstack">Initial failstack</param>
        /// <param name="initialState">Starting state</param>
        public EnhancementProcess(string item, int targetGrade, int failstack, EnhancementState initialState = default) 
            : base(
                initialState ?? new EnhancementState(failstack),  
                new EnhancementProbability(item), 
                Enumerable.Range(1, targetGrade).Select(i => new EnhancementAction(i)).ToList(), 
                new NumberOfAttempsReward(item, targetGrade))
        {
            _targetGrade = targetGrade;
        }

        /// <inheritdoc/>
        protected override IValueFunction<EnhancementState> NextFunction() => new NextValueFunction(_targetGrade);
    }
}