using BDO.Core.Events;
using ZES.Infrastructure.Projections;

namespace BDO.Core.Queries
{
    public class ItemInfoQueryHandler : ProjectionHandlerBase<ItemInfo, ItemInfoUpdated>
    {
        public override ItemInfo Handle(ItemInfoUpdated e, ItemInfo state)
        {
            state.ItemId = e.ItemId;
            return state;
        }
    }
}