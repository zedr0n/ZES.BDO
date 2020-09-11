using System.Collections.Generic;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement.Stochastics.Policies
{
    public class ReblathPolicy : IPolicy<EnhancementState>, IDeterministicPolicy<EnhancementState>
    {
        private readonly string _item = "Armor";
        private readonly int _targetFailstack;

        public ReblathPolicy(int targetFailstack)
        {
            _targetFailstack = targetFailstack;
        }
        
        public IEnumerable<IMarkovAction<EnhancementState>> GetAllowedActions()
        {
            return new List<IMarkovAction<EnhancementState>>
            {
                new EnhancementAction(5, _item),
                new CleanseAction(),
            };
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

        public IMarkovAction<EnhancementState> this[EnhancementState state]
        {
            get
            {
                if (state.FailStack >= _targetFailstack)
                    return null;
                
                if (state.Items[5] > 0)
                    return new CleanseAction();
                
                return new EnhancementAction(5, _item);
            }
            set => throw new System.NotImplementedException();
        }
    }
}