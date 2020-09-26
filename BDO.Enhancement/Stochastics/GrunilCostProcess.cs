using BDO.Enhancement.Stochastics.Rewards;

namespace BDO.Enhancement.Stochastics
{
    public class GrunilCostProcess : EnhancementProcess
    {
        public GrunilCostProcess(EnhancementState initialState)
            : base(initialState)
        {
            Rewards.Add(new GetGrunilReward());
            Rewards.Add(new GrunilReward());
            Rewards.Add(new FailstackReward(false));
        }
    }
}