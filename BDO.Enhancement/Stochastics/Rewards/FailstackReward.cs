using System.Collections.Generic;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class FailstackReward : ActionReward<EnhancementState, FailstackAction>
    {
        private Dictionary<int, double> _costs = new Dictionary<int, double>
        {
            { 1, 228824 },
            { 2, 234619 },
            { 3, 241519 },
            { 4, 249585 },
            { 5, 258926 },
            { 6, 269613 },
            { 7, 281772 },
            { 8, 295519 },
            { 9, 311012 },
            { 10, 328410 },
            { 11, 347901 },
            { 12, 369698 },
            { 13, 394043 },
            { 14, 421224 },
            { 15, 451533 },
            { 16, 485338 },
            { 17, 523053 },
            { 18, 565139 },
            { 19, 612096 },
            { 20, 664560 },
            { 21, 723188 },
            { 22, 788782 },
            { 23, 862238 },
            { 24, 944564 },
            { 25, 1036966 },
            { 26, 1140777 },
            { 27, 1257543 },
            { 28, 1389097 },
            { 29, 1537476 },
            { 30, 1705070 },
            { 31, 1894660 },
        };
        private const double _cost = 200000;
        
        public override double this[EnhancementState @from, EnhancementState to, FailstackAction action]
        {
            get
            {
                if (action[from, to] == 0)
                    return 0.0;

                return -_costs[to.FailStack];
            }
        }
    }
}