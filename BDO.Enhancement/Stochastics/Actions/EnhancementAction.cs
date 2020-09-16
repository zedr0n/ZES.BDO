using System.Collections.Generic;
using System.Linq;
using BDO.Enhancement.Static;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Actions
{
    /// <inheritdoc />
    public class EnhancementAction : MarkovActionBase<EnhancementState>
    {
        private Dictionary<EnhancementState, IEnumerable<EnhancementState>> _nextStates = new Dictionary<EnhancementState, IEnumerable<EnhancementState>>();
        
        public EnhancementAction(int grade, string item)
        {
            Grade = grade;
            _info = Data.EnhancementInfos.SingleOrDefault(i => i.IsFor(item, grade - 1));
        }

        public EnhancementAction(int grade, Data.EnhancementInfo info)
        {
            Grade = grade;
            _info = info;
        }

        public bool TrackNumberOfAttempts { get; set; } = false;
        private readonly Data.EnhancementInfo _info;

        public int Grade { get; }

        /// <inheritdoc/>
        public override double this[EnhancementState @from, EnhancementState to]
        {
            get
            {
                /*var fromItems = from.Items;
                var toItems = to.Items;
                var diffGrade = toItems[Grade] - fromItems[Grade];

                if (diffGrade < 0 || diffGrade > 1)
                    return 0.0;
                
                if (base[from, to] == 0)
                    return 0.0;*/

                var chance = GetChance(from.FailStack);
                if (to.Items[Grade] == from.Items[Grade]) // failure
                    return 1.0 - chance;
                return chance;
            }
        }

        protected override EnhancementState[] GetStates(EnhancementState current)
        {
            return new[]
            {
                current.Clone(s =>
                {
                    s.Items[Grade]++;
                    s.Items[Grade - 1]--;
                    if (_info.ItemLoss > 0)
                        s.Items[0] -= _info.ItemLoss;

                    if (TrackNumberOfAttempts)
                        s.NumberOfAttempts++;
                    s.FailStack = 0;
                    s.JustFailedGrade = -1;
                }),
                current.Clone(s =>
                {
                    if (_info.ItemLoss > 0)
                    {
                        s.Items[Grade - 1] -= _info.ItemLoss;
                        s.Items[0] -= _info.ItemLoss;
                    }

                    if (_info.DropEnhancementGrade)
                    {
                        s.Items[Grade - 1]--;
                        s.Items[Grade - 2]++;
                    }

                    s.FailStack += _info.NumberOfStacks;
                    if (TrackNumberOfAttempts)
                        s.NumberOfAttempts++;
                    s.JustFailedGrade = Grade;
                }),
            };
        }

        public override IEnumerable<EnhancementState> this[EnhancementState current]
        {
            get
            {
                if (current.Items[Grade - 1] <= 0) 
                    return new List<EnhancementState>();

                return base[current];

                // return GetStates(current);
                
                if (!_nextStates.ContainsKey(current))
                    _nextStates[current] = GetStates(current);
                
                return _nextStates[current];
            }
        } 
        
        private double GetChance(int failstack)
        {
            var chance = _info.BaseChance;
            failstack += Data.EnhancementBonus;
            while (--failstack >= 0)
            {
                chance += chance > _info.SoftCap ? _info.SoftCapIncrease : _info.BaseIncrease;
                if (chance >= Config.HardCap)
                    return Config.HardCap;
            }

            return chance;
        }
    }
}