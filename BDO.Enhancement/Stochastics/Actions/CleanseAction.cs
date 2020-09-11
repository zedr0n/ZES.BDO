using System.Collections.Generic;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Actions
{
    public class CleanseAction : MarkovActionBase<EnhancementState>
    {
        public override IEnumerable<EnhancementState> this[EnhancementState current]
        {
            get
            {
                var list = new List<EnhancementState>();
                if (current.Items[5] > 0)
                {
                    list.Add(current.Clone(s =>
                    {
                        s.Items[5]--;
                        s.Items[4]++;
                    }));
                }

                return list;
            }
        }
    }
}