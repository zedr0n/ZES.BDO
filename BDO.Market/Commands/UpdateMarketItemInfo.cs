/// <filename>
///     UpdateMarketItemInfo.cs
/// </filename>

// <auto-generated/>
 namespace BDO.Market.Commands
{
  public class UpdateMarketItemInfo : ZES.Infrastructure.Domain.Command
  {
    public UpdateMarketItemInfo() 
    {
    }  
    public int[] Amounts
    {
       get;
    }  
    public double[] Prices
    {
       get;
    }  
    public UpdateMarketItemInfo(string target, int[] amounts, double[] prices) : base(target) 
    {
      Amounts = amounts; 
      Prices = prices;
    }
  }
}

