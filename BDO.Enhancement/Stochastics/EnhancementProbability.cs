using System.Collections.Generic;
using System.Linq;
using BDO.Enhancement.Static;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement.Stochastics
{
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
                action[from, to] ? 1.0 : 0.0;
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
                    if (!action[from, to])
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
}