/// <filename>
///     EnhancementTestResultQuery.cs
/// </filename>

// <auto-generated/>
 namespace BDO.Enhancement.Queries
{
  public class EnhancementTestResultQuery : ZES.Infrastructure.Domain.Query<EnhancementTestResult>
  {
    public string EnhancementId
    {
       get;
    }  
    public EnhancementTestResultQuery() 
    {
    }  
    public EnhancementTestResultQuery(string enhancementId) 
    {
      EnhancementId = enhancementId;
    }
  }
}
