using System;
using System.Collections.Generic;
using System.Linq;
using BDO.Enhancement.Commands;
using BDO.Enhancement.Events;
using BDO.Enhancement.Static;
using BDO.Enhancement.Stochastics;
using BDO.Enhancement.Stochastics.Actions;
using BDO.Enhancement.Stochastics.Policies;
using Stateless;
using ZES.Infrastructure.Domain;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement.Sagas
{
    /// <inheritdoc />
    public class EnhancementSaga : StatelessSaga<EnhancementSaga.State, EnhancementSaga.Trigger>
    {
        private EnhancementState _state;
        private IPolicy<EnhancementState> _policy;

        private int _numberOfFailures;
        private int _targetGrade;
        
        private IEnumerator<double> _enumerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancementSaga"/> class.
        /// </summary>
        public EnhancementSaga()
        {
            RegisterIf<EnhancementStarted>(e => e.EnchancementId, Trigger.Started, e => Data.EnhancementInfos.Any(i => i.IsFor(e.Item, e.Grade)), Initialise);
            Register<EnhancementFailed>(e => e.Id, Trigger.Attempt, e =>
            {
                _numberOfFailures++;
            });
            Register<EnhancementSucceeded>(e => e.Id, Trigger.Success);
        }
        
        public enum State
        {
            Open,
            Enhancing,
            Complete,
        }

        public enum Trigger
        {
            Started,
            Attempt,
            Success,
        }

        private double Next()
        {
            if (_numberOfFailures >= RandomGenerator.Dimension)
                throw new InvalidOperationException($"Exceeded dimensionality : {_numberOfFailures} >= {RandomGenerator.Dimension}");

            _enumerator.MoveNext();
            return _enumerator.Current;
        }

        /// <inheritdoc/>
        protected override void ConfigureStateMachine()
        {   
            StateMachine = new StateMachine<State, Trigger>(State.Open);

            StateMachine.Configure(State.Open).Permit(Trigger.Started, State.Enhancing);

            StateMachine.Configure(State.Enhancing)
                .OnEntry(() =>
                {
                    var rand = Next();
                    var action = GetNextAction();

                    var total = 0.0;
                    foreach (var state in action[_state])
                    {
                        total += action[_state, state];
                        if (rand < total)
                        {
                            _state = state.Clone();
                            break;
                        }
                    }
                    
                    var success = _state.Items[_targetGrade] > 0;
                    if (success)
                        SendCommand(new SucceedEnhancement(Id, _numberOfFailures));
                    else
                        SendCommand(new FailEnhancement(Id));
                })
                .PermitReentry(Trigger.Attempt);

            StateMachine.Configure(State.Enhancing).Permit(Trigger.Success, State.Complete);
        }

        private void Initialise(EnhancementStarted e)
        {
            _targetGrade = e.Grade + 1;
            _enumerator = RandomGenerator.Generate(Id).GetEnumerator();
            _policy = new JustEnhancePolicy(e.Item, _targetGrade);
            _state = new EnhancementState(e.Failstack);
            _state.Items[_targetGrade - 1] = int.MaxValue; 
        }

        private IMarkovAction<EnhancementState> GetNextAction()
        {
            if (_policy is IDeterministicPolicy<EnhancementState> policy)
            {
                return policy[_state];
            }
            
            if (_policy is IGeneralizedPolicy<EnhancementState> generalizedPolicy)
            {
                foreach (var action in generalizedPolicy.GetAllowedActions())
                {
                    var proba = generalizedPolicy[action, _state];
                    if (proba.Equals(1.0))
                        return action;
                }
            }

            return null;
        }
    }
}