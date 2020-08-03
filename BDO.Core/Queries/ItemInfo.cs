/// <filename>
///     ItemInfo.cs
/// </filename>

// <auto-generated/>
 namespace BDO.Core.Queries
{
  public class ItemInfo : ZES.Interfaces.Domain.ISingleState
  {
    public ItemInfo() 
    {
    }  
    public string Name
    {
       get; 
       set;
    }  
    public int ItemId
    {
       get; 
       set;
    }  
    public int Grade
    {
       get; 
       set;
    }  
    public ItemInfo(string name, int itemId, int grade) 
    {
      Name = name; 
      ItemId = itemId; 
      Grade = grade;
    }
  }
}

