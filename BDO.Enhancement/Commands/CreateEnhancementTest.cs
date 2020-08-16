/// <filename>
///     CreateEnhancementTest.cs
/// </filename>

// <auto-generated/>
 namespace BDO.Enhancement.Commands
{
  public class CreateEnhancementTest : ZES.Infrastructure.Domain.Command, ZES.Interfaces.Domain.ICreateCommand
  {
    public CreateEnhancementTest() 
    {
    }  
    public int NumberOfPaths
    {
       get;
    }  
    public string ItemId
    {
       get;
    }  
    public int Grade
    {
       get;
    }  
    public int InitialFailstack
    {
       get;
    }  
    public CreateEnhancementTest(string target, int numberOfPaths, string itemId, int grade, int initialFailstack) : base(target) 
    {
      NumberOfPaths = numberOfPaths; 
      ItemId = itemId; 
      Grade = grade; 
      InitialFailstack = initialFailstack;
    }
  }
}
