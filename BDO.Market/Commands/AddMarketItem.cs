/// <filename>
///     AddMarketItem.cs
/// </filename>

// <auto-generated/>
 namespace BDO.Market.Commands
{
  public class AddMarketItem : ZES.Infrastructure.Domain.Command, ZES.Interfaces.Domain.ICreateCommand
  {
    public AddMarketItem() 
    {
    }  
    public string Item
    {
       get;
    }  
    public string FamilyName
    {
       get;
    }  
    public AddMarketItem(string target, string item, string familyName) : base(target) 
    {
      Item = item; 
      FamilyName = familyName;
    }
  }
}

