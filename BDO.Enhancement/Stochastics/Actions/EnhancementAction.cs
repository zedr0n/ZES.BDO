using System.Collections.Generic;
using System.Linq;
using BDO.Enhancement.Static;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Actions
{
    /// <inheritdoc />
    public class EnhancementAction : MarkovActionBase<EnhancementState>
    {
        public EnhancementAction(int grade, string item)
        {
            Grade = grade;
            _info = Data.EnhancementInfos.SingleOrDefault(i => i.IsFor(item, grade - 1));
        }


        private readonly Data.EnhancementInfo _info;

        public int Grade { get; }

        /// <inheritdoc/>
        public override double this[EnhancementState @from, EnhancementState to]
        {
            get
            {
                if (base[from, to] == 0)
                    return 0.0;

                var chance = GetChance(from.FailStack);
                if (to.Items[Grade] == from.Items[Grade]) // failure
                    return 1.0 - chance;
                return chance;
            }
        }

        public override IEnumerable<EnhancementState> this[EnhancementState current]
        {
            get
            {
                if (current.Items[Grade - 1] > 0)
                {
                    return new List<EnhancementState>
                        {
                            current.Clone(s =>
                            {
                                s.Items[Grade]++;
                                s.Items[Grade - 1]--;
                                s.Items[0] -= _info.ItemLoss;
                                s.NumberOfAttempts++;
                                s.FailStack = 0;
                                s.JustFailedGrade = -1;
                            }),
                            current.Clone(s =>
                            {
                                s.Items[Grade - 1] -= _info.ItemLoss;
                                s.Items[0] -= _info.ItemLoss;
                                s.FailStack++;
                                s.NumberOfAttempts++;
                                s.JustFailedGrade = Grade;
                            }),
                        };
                }
                
                return new List<EnhancementState>();
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