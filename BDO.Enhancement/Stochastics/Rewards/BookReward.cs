using System.Collections.Generic;
using System.Linq;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class BookReward : ActionReward<EnhancementState, BookFailstack>
    {
        public static Dictionary<int, double> Costs = new Dictionary<int, double>
        {
            {20, -1000000},
            {30, -1500000},
            {40, -2000000},
            {50, -3000000},
        };

        public override double this[EnhancementState @from, EnhancementState to, BookFailstack action]
        {
            get
            {
                var failstack = from.FailStack;
                if (failstack > 50)
                    return 0.0;
                var cost = Costs.First(c => c.Key >= failstack).Value;
                var reward = FailstackReward.GetCost(from.FailStack) + cost;
                return reward;
            }
        }
    }
}