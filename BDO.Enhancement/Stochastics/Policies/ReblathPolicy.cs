using System.Collections.Generic;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement.Stochastics.Policies
{
    public class ReblathPolicy : MarkovPolicy<EnhancementState>
    {
        private readonly string _item = "Reblath";
        private readonly int _targetFailstack;

        public ReblathPolicy(int targetFailstack)
        {
            _targetFailstack = targetFailstack;
        }
        
        public double this[IMarkovAction<EnhancementState> action, EnhancementState state]
        {
            get
            {
                if (state.FailStack >= _targetFailstack)
                    return 0.0;

                if (state.Items[5] > 0)
                    return action is CleanseAction ? 1.0 : 0.0;

                return action is EnhancementAction ? 1.0 : 0.0;
            }
        }

        protected override MarkovPolicy<EnhancementState> Copy() => new ReblathPolicy(_targetFailstack);

        protected override IMarkovAction<EnhancementState>[] GetAllActions(EnhancementState state)
        {
            return new List<IMarkovAction<EnhancementState>>
            {
                new EnhancementAction(5, _item),
                new CleanseAction(),
            }.ToArray();
        }

        protected override IMarkovAction<EnhancementState> GetAction(EnhancementState state)
        {
            if (state.FailStack >= _targetFailstack)
                return null;
            
            if (state.Items[5] > 0)
                return new CleanseAction();
            
            return new EnhancementAction(5, _item);
        }
    }
}