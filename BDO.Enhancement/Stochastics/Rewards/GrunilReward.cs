using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class GrunilReward : ActionReward<EnhancementState, EnhancementAction>
    {
        private int _targetFailstack;
        private const double _cost = 2100000;
        
        // armor
        // private const double _repairCost = 25000 / 10 * 5;
        // shoes
        private const double _armorCost = 58500 / 10 * 5;
        private const double _helmetCost = 421000 / 10 * 5;
        
        public GrunilReward(int targetFailstack)
        {
            _targetFailstack = targetFailstack;
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