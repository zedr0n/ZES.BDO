using BDO.Core.Events;
using ZES.Infrastructure.Domain;
using ZES.Interfaces.Domain;

namespace BDO.Core
{
    public class Item : EventSourced, IAggregate 
    {
        public Item()
        {
            Register<ItemAdded>(e => Id = e.Name);
            Register<ItemInfoUpdated>(e => ItemId = e.ItemId);
        }
        
        public Item(string name) 
            : this()
        {
            When(new ItemAdded(name));
        }

        public int ItemId { get; set; }

        public void UpdateInfo(int id)
        {
            When(new ItemInfoUpdated(id));
        }
    }
}