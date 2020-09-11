using BDO.Enhancement.Stochastics.Rewards;

namespace BDO.Enhancement.Stochastics
{
    public class ExpectedCostProcess : EnhancementProcess
    {
        public ExpectedCostProcess(EnhancementState initialState) 
            : base(initialState)
        {
            Rewards.Add(new FailstackReward());
        }
    }
}