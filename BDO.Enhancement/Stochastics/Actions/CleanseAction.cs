using System.Collections.Generic;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Actions
{
    public class CleanseAction : MarkovActionBase<EnhancementState>
    {
        protected override EnhancementState[] GetStates(EnhancementState current)
        {
            return new[]
            {
                current.Clone(s =>
                {
                    s.Items[5]--;
                    s.Items[4]++;
                }),
            };
        }

        public override IEnumerable<EnhancementState> this[EnhancementState current]
        {
            get
            {
                if (current.Items[5] == 0)
                    return new List<EnhancementState>();

                return base[current];
            }
        }
    }
}