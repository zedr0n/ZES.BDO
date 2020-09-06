/// <filename>
///     CookieSet.cs
/// </filename>

// <auto-generated/>
 namespace BDO.Market.Events
{
  public class CookieSet : ZES.Infrastructure.Domain.Event
  {
    public CookieSet() 
    {
    }  
    public string Cookie
    {
       get; 
       set;
    }  
    public CookieSet(string cookie) 
    {
      Cookie = cookie;
    }
  }
}

