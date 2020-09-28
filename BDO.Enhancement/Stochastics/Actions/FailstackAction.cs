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

        /// <inheritdoc/>
        public override double this[EnhancementState @from, EnhancementState to] => 1.0;

        /// <inheritdoc/>
        protected override EnhancementState[] GetStates(EnhancementState current)
        {
            return new[]
            {
                current.Clone(s =>
                {
                    s.FailStack += _amount;
                    s.JustFailedGrade = -1;
                }),
            };
        }

        // public override IEnumerable<EnhancementState> this[EnhancementState current] => GetStates(current);
    }
}