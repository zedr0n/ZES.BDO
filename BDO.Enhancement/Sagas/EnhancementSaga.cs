using System;
using BDO.Enhancement.Commands;
using BDO.Enhancement.Events;
using Stateless;
using ZES.Infrastructure.Domain;

namespace BDO.Enhancement.Sagas
{
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
        
        public EnhancementSaga()
        {
            Register<EnhancementStarted>(e => e.EnchancementId, Trigger.STARTED);
            Register<EnhancementInfoSet>(e => e.EnchancementId, Trigger.ATTEMPT, SetInfo);
            Register<EnhancementFailed>(e => e.Id, Trigger.ATTEMPT, e => _failStack++);
            Register<EnhancementSucceeded>(e => e.Id, Trigger.SUCCESS);
            
            _random = new Random();
        }
        
        public enum State
        {
            OPEN,
            ENHANCING,
            COMPLETE
        }

        public enum Trigger
        {
            STARTED,
            ATTEMPT,
            SUCCESS
        }

        private void SetInfo(EnhancementInfoSet e)
        {
            _base = e.BaseChance;
            _increase = e.BaseIncrease;
            _softCap = e.SoftCap;
            _softCapIncrease = e.SoftCapIncrease;
            _failStack = e.InitialFilestack;
            _success = GetInitial(_failStack);
        }

        protected override void ConfigureStateMachine()
        {   
            StateMachine = new StateMachine<State, Trigger>(State.OPEN);

            StateMachine.Configure(State.OPEN).Permit(Trigger.ATTEMPT, State.ENHANCING);

            StateMachine.Configure(State.ENHANCING)
                .OnEntry(() =>
                {
                    var success = _random.NextDouble() < _success;
                    if (success)
                        SendCommand(new SucceedEnhancement(Id));
                    else
                        SendCommand(new FailEnhancement(Id));

                    _success += GetIncrease(_success);
                    if (_success > _hardCap)
                        _success = _hardCap;
                })
                .PermitReentry(Trigger.ATTEMPT);

            StateMachine.Configure(State.ENHANCING).Permit(Trigger.SUCCESS, State.COMPLETE);
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