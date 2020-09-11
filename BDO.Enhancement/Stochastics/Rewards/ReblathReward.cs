using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class ReblathReward : ActionReward<EnhancementState, EnhancementAction>
    {
        private int _targetFailstack;
        private const double _cost = 210000;
        private const double _repairCost = 25000 / 10 * 5;
        
        public ReblathReward(int targetFailstack)
        {
            
        }
        
        public override double this[EnhancementState @from, EnhancementState to, EnhancementAction action]
        {
            get
            {
                if (action[from, to] == 0)
                    return 0.0;

                return _cost + ( to.Items[5] == 0 ? _repairCost : 0.0 );
            }
        }
    }
}