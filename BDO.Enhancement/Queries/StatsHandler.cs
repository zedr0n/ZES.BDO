using BDO.Enhancement.Events;
using ZES.Infrastructure.Projections;

namespace BDO.Enhancement.Queries
{
    public class StatsHandler : ProjectionHandlerBase<Stats, EnhancementFailed>
    {
        public override Stats Handle(EnhancementFailed e, Stats state)
        {
            state.Increment();
            return state;
        }
    }
}