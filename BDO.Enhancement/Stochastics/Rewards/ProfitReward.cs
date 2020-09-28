using System.Collections.Generic;
using BDO.Enhancement.Static;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class ProfitReward : ActionReward<EnhancementState, EnhancementAction>
    {
        private readonly int _targetGrade;
        private readonly double _reward;
        private readonly double _repairCost;
        private readonly double _cost;

        public ProfitReward(int targetGrade, Data.EnhancementInfo[] infos)
        {
            _targetGrade = targetGrade;
            _reward = infos[targetGrade].Price * 0.85;
            _repairCost = infos[0].RepairCost;
            _cost = infos[0].Cost;
        }
        
        public override double this[EnhancementState @from, EnhancementState to, EnhancementAction action]
        {
            get
            {
                if (action[from, to] == 0)
                    return 0.0;

                var profit = -_cost;
                if (to.Items[_targetGrade] - from.Items[_targetGrade] > 0)
                    profit += _reward;
                if (_repairCost != 0 && to.Items[action.Grade] == from.Items[action.Grade])
                    profit -= _repairCost;
                return profit;
            }
        }
    }
}