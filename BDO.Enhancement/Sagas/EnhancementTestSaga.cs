using BDO.Enhancement.Commands;
using BDO.Enhancement.Events;
using Stateless;
using ZES.Infrastructure.Domain;

namespace BDO.Enhancement.Sagas
{
    public class EnhancementTestSaga : StatelessSaga<EnhancementTestSaga.State, EnhancementTestSaga.Trigger>
    {
        private int _numberOfPaths;
        private int _numberOfFailures;

        private string _item;
        private int _grade;

        private int _initialFilestack;
        private int _path;

        public EnhancementTestSaga()
        {
            Register<EnhancementTestCreated>(e => e.EnhancementId, Trigger.EnhancementTestCreated,  e =>
            {
                _numberOfPaths = e.NumberOfPaths;
                _item = e.ItemId;
                _grade = e.Grade;
                _initialFilestack = e.InitialFailstack;
            });
            Register<EnhancementSucceeded>(e => e.Id.Split('_')[0], Trigger.EnhancementSucceeded, e =>
            {
                _numberOfFailures += e.NumberOfFailures;
                _path++;
            });
        }
        
        public enum State
        {
            Open,
            Computing,
            Complete,
        }

        public enum Trigger
        {
           EnhancementTestCreated,    
           EnhancementSucceeded,
           Complete,
        }

        /// <inheritdoc/>
        protected override void ConfigureStateMachine()
        {
            StateMachine = new StateMachine<State, Trigger>(State.Open);

            StateMachine.Configure(State.Open)
                .Permit(Trigger.EnhancementTestCreated, State.Computing)
                .Ignore(Trigger.EnhancementSucceeded);

            StateMachine.Configure(State.Computing)
                .PermitReentryIf(Trigger.EnhancementSucceeded, () => _path < _numberOfPaths)
                .PermitIf(Trigger.EnhancementSucceeded, State.Complete, () => _path == _numberOfPaths)
                .Permit(Trigger.Complete, State.Complete)
                .OnEntry(() =>
                {
                    var test = $"{Id}_Path{_path}";
                    SendCommand(new StartEnhancement(test, _item, _grade, _initialFilestack));
                });

            StateMachine.Configure(State.Complete)
                .OnEntry(() => SendCommand(new CompleteEnhancementTest(Id, _numberOfFailures)));
        }
    }
}