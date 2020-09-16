using BDO.Enhancement.Stochastics.Rewards;

namespace BDO.Enhancement.Stochastics
{
    public class GrunilCostProcess : EnhancementProcess
    {
        public GrunilCostProcess(int targetFailstack, EnhancementState initialState)
            : base(initialState)
        {
            Rewards.Add(new GrunilReward(targetFailstack));
            Rewards.Add(new FailstackReward(false));
        }
    }
}