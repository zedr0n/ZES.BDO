/// <filename>
///     ItemAdded.cs
/// </filename>

// <auto-generated/>
 namespace BDO.Core.Events
{
  public class ItemAdded : ZES.Infrastructure.Domain.Event
  {
    public ItemAdded() 
    {
    }  
    public string Name
    {
       get; 
       set;
    }  
    public ItemAdded(string name) 
    {
      Name = name;
    }
  }
}

