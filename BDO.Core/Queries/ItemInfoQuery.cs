/// <filename>
///     ItemInfoQuery.cs
/// </filename>

// <auto-generated/>
 namespace BDO.Core.Queries
{
  public class ItemInfoQuery : ZES.Infrastructure.Domain.SingleQuery<ItemInfo>
  {
    public string Name
    {
       get;
    }  
    public ItemInfoQuery() 
    {
    }  
    public ItemInfoQuery(string name) : base(name) 
    {
      Name = name;
    }
  }
}

