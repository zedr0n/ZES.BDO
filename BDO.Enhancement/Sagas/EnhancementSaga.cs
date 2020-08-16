using System;
using System.Collections.Generic;
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
        private readonly double _hardCap = Config.HardCap;
        
        private int _failStack;
        private double _success;

        private double _base;
        private double _softCap;

        private double _increase;
        private double _softCapIncrease;
        private int _numberOfFailures;
        
        private List<double> _randoms = new List<double>();

        private IEnumerator<double> _enumerator;

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
                    _randoms.Add(rand);
                    var success = rand < _success;
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

            _enumerator = RandomGenerator.Generate(Id).GetEnumerator();
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