using System.Collections.Generic;
using System.Linq;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement.Stochastics.Policies
{
    public class JustEnhancePolicy : MarkovPolicy<EnhancementState>
    {
        private readonly string _item;
        private readonly int _targetGrade;

        public JustEnhancePolicy(string item, int targetGrade)
        {
            _targetGrade = targetGrade;
            _item = item;
        }

        public bool StopAtOnce { get; set; } = true;
        public bool TrackNumberOfAttempts { get; set; } = false;

        public double this[IMarkovAction<EnhancementState> action, EnhancementState state]
        {
            get
            {
                if (state.Items[0] <= 0)
                    return 0.0;
                
                if (state.Items[_targetGrade] > 0 && StopAtOnce)
                    return 0.0;
                
                var grade = state.Items.Take(_targetGrade).ToList().FindLastIndex(i => i > 0) + 1;

                if (action is EnhancementAction enhancementAction) // && state.FailStack > 0)
                    return enhancementAction.Grade == grade ? 1.0 : 0.0;
                
                return 0.0;
            }
        }
        
        protected override MarkovPolicy<EnhancementState> Copy() => new JustEnhancePolicy(_item, _targetGrade)
        {
            StopAtOnce = StopAtOnce,
            TrackNumberOfAttempts = TrackNumberOfAttempts, 
        };

        protected override IMarkovAction<EnhancementState>[] GetAllActions(EnhancementState state)
        {
            var list = new List<IMarkovAction<EnhancementState>>();
            for (var i = 1; i <= _targetGrade; ++i)
                list.Add(new EnhancementAction(i, _item));
            return list.ToArray();
        }

        /// <inheritdoc/>
        protected override IMarkovAction<EnhancementState> GetAction(EnhancementState state)
        {
            if (state.Items[0] <= 0 || (state.Items[_targetGrade] > 0 && StopAtOnce))
                return null;
            
            var grade = state.Items.Take(_targetGrade).ToList().FindLastIndex(i => i > 0) + 1;
            
            return new EnhancementAction(grade, _item) { TrackNumberOfAttempts = TrackNumberOfAttempts };
        }
    }
}