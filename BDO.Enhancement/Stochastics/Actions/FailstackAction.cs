using System.Collections.Generic;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Actions
{
    /// <inheritdoc />
    public class FailstackAction : MarkovActionBase<EnhancementState>
    {
        public override IEnumerable<EnhancementState> this[EnhancementState current] => new List<EnhancementState>
        {
            current.Clone(s =>
            {
                s.FailStack++;
                s.JustFailedGrade = -1;
            }),
        };
    }
}