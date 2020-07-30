using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDO.Core.Commands;
using BDO.Core.Events;
using Stateless;
using ZES.Infrastructure;
using ZES.Infrastructure.Alerts;
using ZES.Infrastructure.Domain;
using ZES.Infrastructure.Net;
using ZES.Interfaces;
using ZES.Interfaces.Net;
using ZES.Interfaces.Pipes;

namespace BDO.Core.Sagas
{
    public class SearchResults : JsonList<SearchResult>, IJsonResult
    { }
    
    public class SearchResult 
    {
        public string Value { get; set; }
        public string Name { get; set; }
        public string Grade { get; set; }
        public string Object_Type { get; set; } 
    }

    public class ItemInfoReceived : Event
    {
        public ItemInfoReceived(int itemId, string name)
        {
            ItemId = itemId;
            Name = name;
        }

        public int ItemId { get; }
        public string Name { get; }
    }
    
    public class JsonAlertHandler : AlertHandlerBase<JsonRequestCompleted<SearchResults>>
    {
        public JsonAlertHandler(IMessageQueue messageQueue) 
            : base(messageQueue)
        {
        }

        protected override async Task<IEnumerable<IEvent>> Process(JsonRequestCompleted<SearchResults> alert)
        {
            var events = alert.Data
                .Where(r => r.Object_Type == "Item")
                .Select(r => new ItemInfoReceived(int.Parse(r.Value), r.Name));
            return events;
        }
    }
    
    public class ItemInfoSaga : StatelessSaga<ItemInfoSaga.State, ItemInfoSaga.Trigger>
    {
        public ItemInfoSaga()
        {
            Register<ItemAdded>( e => e.Name, Trigger.ItemAdded);
            Register<ItemInfoReceived>( e => e.Name, Trigger.ItemInfoReceived, e => _itemId = e.ItemId);
        }

        private int _itemId;
        
        public enum State
        {
            Open,
            Active,
            Complete
        }

        public enum Trigger
        {
            ItemAdded,
            ItemInfoReceived
        }

        protected override void ConfigureStateMachine()
        {
            StateMachine = new StateMachine<State, Trigger>(State.Open);

            StateMachine.Configure(State.Open)
                //.Ignore(Trigger.ItemInfoReceived)
                .Permit(Trigger.ItemAdded, State.Active);

            StateMachine.Configure(State.Active)
                .OnEntry(() =>
                {
                    var url = $"https://bddatabase.net/ac.php?l=us&term={Id.Replace(' '.ToString(), "%20")}";
                    SendCommand(new RequestJson<SearchResults>(url));
                })
                .Permit(Trigger.ItemInfoReceived, State.Complete);

            StateMachine.Configure(State.Complete)
                .OnEntry(() =>
                {
                    SendCommand(new UpdateItemInfo(Id, _itemId));
                });
            
            base.ConfigureStateMachine();
        }
    }
}