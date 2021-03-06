/// <filename>
///     CompleteEnhancementTestHandler.cs
/// </filename>

// <auto-generated/>
 namespace BDO.Enhancement.Commands
{
  public class CompleteEnhancementTestHandler : ZES.Infrastructure.Domain.CommandHandlerBase<CompleteEnhancementTest, EnhancementTest>
  {
    public CompleteEnhancementTestHandler(ZES.Interfaces.Domain.IEsRepository<ZES.Interfaces.Domain.IAggregate> repository) : base(repository) 
    {
    }  
    protected override void Act (EnhancementTest enhancementTest, CompleteEnhancementTest command)
    {
      enhancementTest.Complete(command.NumberOfFailures);
    }
  }
}

