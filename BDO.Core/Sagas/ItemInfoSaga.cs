using System;
using System.Collections.Generic;
using System.Linq;
using BDO.Core.Commands;
using BDO.Core.Events;
using Stateless;
using ZES.Infrastructure;
using ZES.Infrastructure.Domain;
using ZES.Infrastructure.Net;
using ZES.Interfaces;
using ZES.Interfaces.Net;

namespace BDO.Core.Sagas
{
    public class ItemInfoSaga : StatelessSaga<ItemInfoSaga.State, ItemInfoSaga.Trigger>
    {
        private string _name;
        private int _itemId;
        private int _grade;
        public ItemInfoSaga()
        {
            Register<ItemAdded>( e => $"{e.Name}", Trigger.ItemAdded, e => _name = e.Name);
            RegisterIf<ItemInfoHandler.ItemInfoReceived>( 
                e => e.RequestorId, 
                Trigger.ItemInfoReceived, 
                e => e.Name.Equals(_name, StringComparison.CurrentCultureIgnoreCase),
                e =>
            {
                _itemId = e.ItemId;
                _grade = e.Grade;
            });
        }
        
        public enum State
        {
            Open,
            Active,
            Complete,
        }

        public enum Trigger
        {
            ItemAdded,
            ItemInfoReceived,
        }

        protected override void ConfigureStateMachine()
        {
            StateMachine = new StateMachine<State, Trigger>(State.Open);

            StateMachine.Configure(State.Open)
                .Permit(Trigger.ItemAdded, State.Active);

            StateMachine.Configure(State.Active)
                .OnEntry(() =>
                {
                    var url = $"https://bddatabase.net/ac.php?l=us&term={_name.Replace(' '.ToString(), "%20")}";
                    SendCommand(new RequestJson<ItemInfoHandler.SearchResults>(Id, url));
                })
                .Permit(Trigger.ItemInfoReceived, State.Complete);

            StateMachine.Configure(State.Complete)
                .OnEntry(() => SendCommand(new UpdateItemInfo(_name, _itemId, _grade)));
        }
        
        public class ItemInfoHandler : IJsonHandler<ItemInfoHandler.SearchResults>
        {
            public IEnumerable<IEvent> Handle(SearchResults response)
            {
                var events = response     
                    .Where(r => r.Object_Type == "Item")
                    .Select(r => new ItemInfoReceived(response.RequestorId, int.Parse(r.Value), r.Name, int.Parse(r.Grade)));
                return events;
            }
            
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
                public ItemInfoReceived(string requestorId, int itemId, string name, int grade)
                {
                    RequestorId = requestorId;
                    ItemId = itemId;
                    Name = name;
                    Grade = grade;
                }

                public string RequestorId { get; }
                public int ItemId { get; }
                public string Name { get; }
                public int Grade { get; }
            }
        }
    }
}