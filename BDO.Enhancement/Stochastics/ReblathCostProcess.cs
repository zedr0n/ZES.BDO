using BDO.Enhancement.Stochastics.Rewards;

namespace BDO.Enhancement.Stochastics
{
    public class ReblathCostProcess : EnhancementProcess
    {
        public ReblathCostProcess(int targetFailstack, EnhancementState initialState) 
            : base(initialState)
        {
            Rewards.Add(new ReblathReward(targetFailstack));
            Rewards.Add(new CleanseReward());
        }
    }
}