using System.Linq;
using BDO.Enhancement.Static;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class EnhancementReward : ActionReward<EnhancementState, EnhancementAction>
    {
        private readonly double[] _repairCost;
        private readonly double[] _cost;
        private readonly double[] _cronCost;
        
        public EnhancementReward(string item, int targetGrade)
        {
            _repairCost = new double[targetGrade];
            _cost = new double[targetGrade];
            _cronCost = new double[targetGrade];
            for (var grade = 0; grade < targetGrade; ++grade)
            {
                var info = Data.EnhancementInfos.Single(i => i.IsFor(item, grade));
                _repairCost[grade] = info.RepairCost;
                _cost[grade] = (info.ArmorStone * Data.ArmorStonePrice) 
                        + (info.ConcentratedWeaponStone * Data.ConcentratedWeaponStonePrice)
                        + (info.BlackGem * Data.BlackGemPrice);
                _cronCost[grade] = info.Cron * Data.CronCost;
            }
        }

        public override double this[EnhancementState from, EnhancementState to, EnhancementAction action]
        {
            get
            {
                var grade = action.Grade;
                var isSuccess = to.Items[action.Grade] - from.Items[action.Grade] == 1;

                var reward = -_cost[grade - 1];
                if (!isSuccess)
                    reward -= _repairCost[grade - 1];

                if (action.UseCron)
                    reward -= _cronCost[grade - 1];

                return reward;
            }
        } 
    }
}