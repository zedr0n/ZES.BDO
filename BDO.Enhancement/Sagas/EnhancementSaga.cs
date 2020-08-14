using System;
using System.Linq;
using BDO.Enhancement.Commands;
using BDO.Enhancement.Events;
using BDO.Enhancement.Static;
using Stateless;
using ZES.Infrastructure.Domain;

namespace BDO.Enhancement.Sagas
{
    /// <inheritdoc />
    public class EnhancementSaga : StatelessSaga<EnhancementSaga.State, EnhancementSaga.Trigger>
    {
        private readonly Random _random;
        private readonly double _hardCap = Config.HardCap;
        
        private int _failStack;
        private double _success;

        private double _base;
        private double _softCap;

        private double _increase;
        private double _softCapIncrease;
        private int _numberOfFailures;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancementSaga"/> class.
        /// </summary>
        public EnhancementSaga()
        {
            RegisterIf<EnhancementStarted>(e => e.EnchancementId, Trigger.Started, e => Data.EnhancementInfos.Any(i => i.IsFor(e.Item, e.Grade)), Initialise);
            Register<EnhancementFailed>(e => e.Id, Trigger.Attempt, e =>
            {
                _failStack++;
                _numberOfFailures++;
            });
            Register<EnhancementSucceeded>(e => e.Id, Trigger.Success);
            
            _random = new Random();
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
        
        protected override void ConfigureStateMachine()
        {   
            StateMachine = new StateMachine<State, Trigger>(State.Open);

            StateMachine.Configure(State.Open).Permit(Trigger.Started, State.Enhancing);

            StateMachine.Configure(State.Enhancing)
                .OnEntry(() =>
                {
                    var success = _random.NextDouble() < _success;
                    if (success)
                        SendCommand(new SucceedEnhancement(Id, _numberOfFailures));
                    else
                        SendCommand(new FailEnhancement(Id));

                    _success += GetIncrease(_success);
                    if (_success > _hardCap)
                        _success = _hardCap;
                })
                .PermitReentry(Trigger.Attempt);

            StateMachine.Configure(State.Enhancing).Permit(Trigger.Success, State.Complete);
        }

        private void Initialise(EnhancementStarted e)
        {
            var info = Data.EnhancementInfos.SingleOrDefault(i => i.IsFor(e.Item, e.Grade));
            if (info == null)
                return;
            
            _base = info.BaseChance;
            _increase = info.BaseIncrease;
            _softCap = info.SoftCap;
            _softCapIncrease = info.SoftCapIncrease;
            _failStack = e.Failstack;
            _success = GetInitial(_failStack);
        }

        private double GetIncrease(double current)
        {
            return current > _softCap ? _softCapIncrease : _increase;
        }

        private double GetInitial(int failStack)
        {
            var success = _base;
            while (failStack > 0)
            {
                success += GetIncrease(success);
                failStack--;
            }

            if (success > _hardCap)
                success = _hardCap;

            return success;
        }
    }
}