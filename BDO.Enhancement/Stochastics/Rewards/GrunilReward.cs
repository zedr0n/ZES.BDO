using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class GrunilReward : ActionReward<EnhancementState, EnhancementAction>
    {
        private const double _cost = 2100000;
        
        // armor
        // private const double _repairCost = 25000 / 10 * 5;
        // shoes
        private const double _armorCost = 121000 / 10 * 5;
        private const double _helmetCost = 421000 / 10 * 5;
        
        public GrunilReward()
        {
        }
        
        public override double this[EnhancementState @from, EnhancementState to, EnhancementAction action]
        {
            get
            {
                if (action[from, to] == 0)
                    return 0.0;

                var isFailure = to.FailStack > from.FailStack;
                
                return _cost + ( isFailure ? _armorCost : 0.0);
            }
        }
    }
}