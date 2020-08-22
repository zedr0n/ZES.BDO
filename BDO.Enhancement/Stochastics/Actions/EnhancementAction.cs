using System.Collections.Generic;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Actions
{
    /// <inheritdoc />
    public class EnhancementAction : MarkovActionBase<EnhancementState>
    {
        public EnhancementAction(int grade)
        {
            Grade = grade;
        }

        public int Grade { get; }

        public override IEnumerable<EnhancementState> this[EnhancementState current]
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
                                s.Items[Grade - 1]--;
                                s.Items[0]--;
                                s.NumberOfAttempts++;
                                s.FailStack = 0;
                            }),
                            current.Clone(s =>
                            {
                                s.Items[Grade - 1]--;
                                s.Items[0]--;
                                s.FailStack++;
                                s.NumberOfAttempts++;
                            }),
                        });
                }

                return list;
            }
        } 
    }
}