using System.Collections.Generic;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class GetGrunilReward : ActionReward<EnhancementState, AddItemAtGradeAction>
    {
        private readonly Dictionary<int, double> _costs = new Dictionary<int, double>
        {
            { 1, 9803836.225624446 }, // ( 7, -, - )
            { 2, 21719193.04303671 }, // ( 7, 7, - )
            { 3, 72621625.56344400 }, // ( 7, 7, 24 )
        };

        public override double this[EnhancementState @from, EnhancementState to, AddItemAtGradeAction action]
        {
            get
            {
                if (_costs.TryGetValue(action.Grade, out var cost))
                    return cost;
                return 0.0;
            }
        }
    }
}