using System.Collections.Generic;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class ProfitReward : ActionReward<EnhancementState, EnhancementAction>
    {
        private int _targetGrade = 0;
        private Dictionary<int, double> _values = new Dictionary<int, double>
        {
            {0, 400000},
            {1, 3400000},
            {2, 15400000},
            {3, 145000000},
            {4, 885000000},
        };

        public ProfitReward(int targetGrade)
        {
            _targetGrade = targetGrade;
        }
        
        public override double this[EnhancementState @from, EnhancementState to, EnhancementAction action]
        {
            get
            {
                if (action[from, to] == 0)
                    return 0.0;

                var profit = 0.0;
                //var profit = -_values[0];    // one zero grade item to enhance

                if (to.Items[action.Grade] == from.Items[action.Grade]) // failure
                    profit -= 0;
                    //profit -= action.Grade == 1 ? _values[0] : 0.0;
                    // profit -= _values[action.Grade - 1];
                else if (to.Items[_targetGrade] - from.Items[_targetGrade] == 1)
                {
                    profit += _values[_targetGrade];
                    // for (var grade = 0; grade < _targetGrade; ++grade)
                    //    profit += to.Items[grade] * _values[grade];
                }
                // else if (to.Items[0] == 0)
                //    profit += _values[action.Grade];
                //else 
                //    profit += _values[action.Grade];
                
                return profit;
            }
        }
    }
}