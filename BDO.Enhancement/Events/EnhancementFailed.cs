/// <filename>
///     EnhancementFailed.cs
/// </filename>

// <auto-generated/>
 namespace BDO.Enhancement.Events
{
  public class EnhancementFailed : ZES.Infrastructure.Domain.Event
  {
    public EnhancementFailed() 
    {
    }  
    public string Id
    {
       get; 
       set;
    }  
    public EnhancementFailed(string id) 
    {
      Id = id;
    }
  }
}

