using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class CleanseReward : ActionReward<EnhancementState, CleanseAction>
    {
        private readonly double _cost = 100000;
        
        public override double this[EnhancementState @from, EnhancementState to, CleanseAction action]
        {
            get
            {
                if (action[from, to] == 0)
                    return 0.0;

                return _cost;
            }
            
        }
    }
}