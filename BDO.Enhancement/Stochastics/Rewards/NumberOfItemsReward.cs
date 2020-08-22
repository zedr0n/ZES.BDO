using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class NumberOfItemsReward : ActionReward<EnhancementState, EnhancementAction>
    {
        private readonly int _targetGrade;

        public NumberOfItemsReward(int targetGrade)
        {
            _targetGrade = targetGrade;
        }

        public override double this[EnhancementState @from, EnhancementState to, EnhancementAction action]
        {
            get
            {
                if (to.Items[_targetGrade] - from.Items[_targetGrade] > 0)
                    return 1.0;
                return 0.0;
            }
        }
    }
}