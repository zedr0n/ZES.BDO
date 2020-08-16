/// <filename>
///     EnhancementInfoSet.cs
/// </filename>

// <auto-generated/>
 namespace BDO.Enhancement.Events
{
  public class EnhancementInfoSet : ZES.Infrastructure.Domain.Event
  {
    public EnhancementInfoSet() 
    {
    }  
    public string EnchancementId
    {
       get; 
       set;
    }  
    public string ItemId
    {
       get; 
       set;
    }  
    public string EnhancementItemId
    {
       get; 
       set;
    }  
    public int EnhancementItemAmount
    {
       get; 
       set;
    }  
    public int InitialFilestack
    {
       get; 
       set;
    }  
    public double BaseChance
    {
       get; 
       set;
    }  
    public double BaseIncrease
    {
       get; 
       set;
    }  
    public double SoftCap
    {
       get; 
       set;
    }  
    public double SoftCapIncrease
    {
       get; 
       set;
    }  
    public EnhancementInfoSet(string enchancementId, string itemId, string enhancementItemId, int enhancementItemAmount, int initialFilestack, double baseChance, double baseIncrease, double softCap, double softCapIncrease) 
    {
      EnchancementId = enchancementId; 
      ItemId = itemId; 
      EnhancementItemId = enhancementItemId; 
      EnhancementItemAmount = enhancementItemAmount; 
      InitialFilestack = initialFilestack; 
      BaseChance = baseChance; 
      BaseIncrease = baseIncrease; 
      SoftCap = softCap; 
      SoftCapIncrease = softCapIncrease;
    }
  }
}
