 namespace BDO.Core.Commands
{
  public class AddItemHandler : ZES.Infrastructure.Domain.CreateCommandHandlerBase<AddItem, Item>
  {
    public AddItemHandler(ZES.Interfaces.Domain.IEsRepository<ZES.Interfaces.Domain.IAggregate> repository) 
      : base(repository) 
    {
    }  
    
    protected override Item Create (AddItem command)
    {
      return new Item(command.Name, command.Grade);
    }
  }
}
