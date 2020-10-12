using System.Linq;
using BDO.Enhancement.Static;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class SellReward : ActionReward<EnhancementState, SellAction>
    {
        private double _reward;
        
        public SellReward(string item, int grade)
        {
            var info = Data.EnhancementInfos.Single(i => i.IsFor(item, grade - 1));
            var priInfo = Data.EnhancementInfos.Single(i => i.IsFor(item, 1));
            _reward = info.Price * Data.MarketYield;
            _reward -= priInfo.Price;
        }

        public override double this[EnhancementState @from, EnhancementState to, SellAction action]
        {
            get
            {
                return _reward;
            }
        }
    }
}