using System.Collections.Generic;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class FailstackReward : ActionReward<EnhancementState, FailstackAction>
    {
        private Dictionary<int, double> _costs = new Dictionary<int, double>
        {
            { 0, 0 },
            { 1, 229473 },
            { 2, 235766 },
            { 3, 243194 },
            { 4, 251838 },
            { 5, 261787 },
            { 6, 273147 },
            { 7, 286037 },
            { 8, 300596 },
            { 9, 316980 },
            { 10, 335367 },
            { 11, 355961 },
            { 12, 378992 },
            { 13, 404723 },
            { 14, 433451 },
            { 15, 465516 },
            { 16, 501305 },
            { 17, 541256 },
            { 18, 585871 },
            { 19, 635720 },
            { 20, 691456 },
            { 21, 753825 },
            { 22, 823680 },
            { 23, 901999 },
            { 24, 989903 },
            { 25, 1088682 },
            { 26, 1199817 },
            { 27, 1325015 },
            { 28, 1466248 },
            { 29, 1625788 },
            { 30, 1806262 },
            { 31, 2010691 },
            { 32, 2242515 },
            { 33, 2505544 },
            { 34, 2803776 },
            { 35, 3140992 },
            { 36, 3520069 },
            { 37, 3942013 },
            { 38, 4404856 },
            { 39, 4902645 },
            { 40, 5424814 },
        };
        private const double _cost = 200000;
        
        public override double this[EnhancementState @from, EnhancementState to, FailstackAction action]
        {
            get
            {
                // if (action[from, to] == 0)
                //    return 0.0;

                var cost = 0.0;
                for (var failstack = from.FailStack + 1; failstack <= to.FailStack; failstack++)
                {
                    cost += _costs[failstack];
                }

                return -cost;
                //return _costs[from.FailStack] - _costs[to.FailStack];
            }
        }
    }
}