/// <filename>
///     CompleteEnhancementTest.cs
/// </filename>

// <auto-generated/>
 namespace BDO.Enhancement.Commands
{
  public class CompleteEnhancementTest : ZES.Infrastructure.Domain.Command
  {
    public CompleteEnhancementTest() 
    {
    }  
    public int NumberOfFailures
    {
       get;
    }  
    public CompleteEnhancementTest(string target, int numberOfFailures) : base(target) 
    {
      NumberOfFailures = numberOfFailures;
    }
  }
}

