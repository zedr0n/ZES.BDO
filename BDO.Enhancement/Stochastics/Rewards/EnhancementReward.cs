using BDO.Enhancement.Static;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class EnhancementReward : ActionReward<EnhancementState, EnhancementAction>
    {
        private readonly double _repairCost;
        private readonly double _cost;
        private readonly double _cronCost;
        
        public EnhancementReward(Data.EnhancementInfo info)
        {
            _repairCost = info.RepairCost;
            _cost = (info.ArmorStone * Data.ArmorStonePrice) 
                    + (info.ConcentratedWeaponStone * Data.ConcentratedWeaponStonePrice)
                    + (info.BlackGem * Data.BlackGemPrice);
            _cronCost = info.Cron * Data.CronCost;
        }

        public override double this[EnhancementState from, EnhancementState to, EnhancementAction action]
        {
            get
            {
                var isSuccess = to.Items[action.Grade] - from.Items[action.Grade] == 1;

                var reward = -_cost;
                if (!isSuccess)
                    reward -= _repairCost;

                if (action.UseCron)
                    reward -= _cronCost;

                return reward;
            }
        } 
    }
}