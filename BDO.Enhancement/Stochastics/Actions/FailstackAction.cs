using System.Collections.Generic;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Actions
{
    /// <inheritdoc />
    public class FailstackAction : MarkovActionBase<EnhancementState>
    {
        private int _amount;

        public FailstackAction(int amount = 1)
        {
            _amount = amount;
        }

        public override double this[EnhancementState @from, EnhancementState to] => 1.0;

        public override IEnumerable<EnhancementState> this[EnhancementState current] => new List<EnhancementState>
        {
            current.Clone(s =>
            {
                s.FailStack += _amount;
                s.JustFailedGrade = -1;
            }),
        };
    }
}