using System.Reactive.Linq;
using System.Threading.Tasks;
using ZES.Interfaces.Branching;
using ZES.Interfaces.Domain;
using ZES.Interfaces.Pipes;

namespace BDO.Enhancement.Commands
{
    public class RunEnhancementTestHandler : ICommandHandler<RunEnhancementTest>
    {
        private readonly IBranchManager _manager;
        private readonly IMessageQueue _messageQueue;

        private readonly ICommandHandler<StartEnhancement> _startHandler;
        private readonly ICommandHandler<SetEnhancementInfo> _infoHander;

        public RunEnhancementTestHandler(IBranchManager manager, IMessageQueue messageQueue, ICommandHandler<StartEnhancement> startHandler, ICommandHandler<SetEnhancementInfo> infoHander)
        {
            _manager = manager;
            _messageQueue = messageQueue;
            _startHandler = startHandler;
            _infoHander = infoHander;
        }

        public async Task Handle(RunEnhancementTest command)
        {
            var nTests = command.NumberOfTests / command.NumberOfBatches;
            for (var iBatch = 0; iBatch < command.NumberOfBatches; ++iBatch)
            {
                await _manager.Branch($"test{iBatch}");
                
                for (var iTest = 0; iTest < nTests; ++iTest)
                {
                    var test = $"Test{iTest}";
                    await _startHandler.Handle(new StartEnhancement(test));
                    await _infoHander.Handle(new SetEnhancementInfo(
                        test, 
                        command.Item,
                        string.Empty,
                        0,
                        0,
                        command.BaseChance, 
                        command.BaseIncrease, 
                        command.SoftCap,
                        command.SoftCapIncrease));

                    await _manager.Ready;
                }

                await _manager.Ready;

                // await _manager.DeleteBranch($"test{iBatch}");
            }
        }

        public async Task Handle(ICommand command) => await Handle(command as RunEnhancementTest);
    }
}