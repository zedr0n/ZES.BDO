using BDO.Core.Events;
using ZES.Interfaces;
using ZES.Interfaces.Domain;

namespace BDO.Core.Queries
{
    public class ItemInfoQueryHandler : IProjectionHandler<ItemInfo, ItemInfoUpdated>, IProjectionHandler<ItemInfo, ItemAdded>
    {
        public ItemInfo Handle(IEvent e, ItemInfo state) => Handle((dynamic)e, state);

        public ItemInfo Handle(ItemAdded e, ItemInfo state)
        {
            state.Name = e.Name;
            state.Grade = e.Grade;
            return state;
        }

        public ItemInfo Handle(ItemInfoUpdated e, ItemInfo state)
        {
            state.ItemId = e.ItemId;
            return state;
        }
    }
}