using BDO.Enhancement.Events;
using ZES.Interfaces;
using ZES.Interfaces.Domain;

namespace BDO.Enhancement.Queries
{
    public class EnhancementTestResultProjectionHandler : IProjectionHandler<EnhancementTestResultState, EnhancementTestCompleted>
    {
        public EnhancementTestResultState Handle(IEvent e, EnhancementTestResultState state) =>
            Handle(e as EnhancementTestCompleted, state);

        public EnhancementTestResultState Handle(EnhancementTestCompleted e, EnhancementTestResultState state)
        {
            state.NumberOfFailures[e.Id] = e.NumberOfFailures;
            return state;
        }
    }
}