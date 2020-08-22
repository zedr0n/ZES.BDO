using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class NumberOfAttemptsReward : ActionReward<EnhancementState, EnhancementAction>
    {
        private readonly int _targetGrade;

        public NumberOfAttemptsReward(int targetGrade)
        {
            _targetGrade = targetGrade;
        }
            
        public override double this[EnhancementState from, EnhancementState to, EnhancementAction action]
        {
            get
            {
                if (to.Items[_targetGrade] == 1 && from.Items[_targetGrade] == 0)
                    return to.NumberOfAttempts;

                return 0.0;
            }
        }
    }
}