using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Actions
{
    public class ValkAction : MarkovActionBase<EnhancementState>
    {
        private readonly int _amount = 0;

        public ValkAction(int amount)
        {
            _amount = amount;
        }
        
        protected override EnhancementState[] GetStates(EnhancementState current)
        {
            if (current.NumberOfValks == 0)
                return new EnhancementState[0];
            
            return new[]
            {
                current.Clone(s =>
                {
                    s.JustFailedGrade = -1;
                    s.FailStack += _amount;
                    s.NumberOfValks -= _amount;
                }),
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Valk[{_amount}]";
        }
    }
}