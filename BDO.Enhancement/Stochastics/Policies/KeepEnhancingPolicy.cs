using System.Collections.Generic;
using System.Linq;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement.Stochastics.Policies
{
    public class KeepEnhancingPolicy : IPolicy<EnhancementState>
    {
        private readonly int _targetGrade;

        public KeepEnhancingPolicy(int targetGrade)
        {
            _targetGrade = targetGrade;
        }

        /// <inheritdoc />
        public IEnumerable<IMarkovAction<EnhancementState>> GetAllowedActions()
        {
            var list = new List<IMarkovAction<EnhancementState>>();
            for (var i = 1; i <= _targetGrade; ++i)
                list.Add(new EnhancementAction(i));
            return list;
        }
        
        public double this[IMarkovAction<EnhancementState> action, EnhancementState state]
        {
            get
            {
                if (state.Items[0] <= 0)
                    return 0.0;
                
                var grade = state.Items.Take(_targetGrade).ToList().FindLastIndex(i => i > 0) + 1;

                if (action is EnhancementAction enhancementAction)
                    return enhancementAction.Grade == grade ? 1.0 : 0.0;

                return 0.0;
            }
        }
    }
}