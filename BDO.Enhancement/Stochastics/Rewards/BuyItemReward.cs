using System;
using System.Linq;
using BDO.Enhancement.Static;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class BuyItemReward : ActionReward<EnhancementState, BuyItemAction>
    {
        private readonly double _cost;
        
        public BuyItemReward(string item)
        {
            var info = Data.EnhancementInfos.SingleOrDefault(i => i.IsFor(item, 0));
            if (info == null)
                throw new InvalidOperationException();
            _cost = info.Price;
        }

        /// <inheritdoc/>
        public override double this[EnhancementState @from, EnhancementState to, BuyItemAction action] => -_cost;
    }
}