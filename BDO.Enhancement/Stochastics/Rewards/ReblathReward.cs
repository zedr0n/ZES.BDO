using System.Linq;
using BDO.Enhancement.Static;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class ReblathReward : ActionReward<EnhancementState, EnhancementAction>
    {
        private readonly double _cost;
        private readonly double _repairCost;
        
        public ReblathReward()
        {
            var info = Data.EnhancementInfos.Single(i => i.IsFor("Reblath", 4));
            _cost = Data.ArmorStonePrice * info.ArmorStone;
            _repairCost = info.RepairCost;
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