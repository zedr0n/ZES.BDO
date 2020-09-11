using BDO.Enhancement.Stochastics.Rewards;

namespace BDO.Enhancement.Stochastics
{
    public class ExpectedProfitProcess : EnhancementProcess
    {
        public ExpectedProfitProcess(int targetGrade, EnhancementState initialState) 
            : base(initialState)
        {
            Rewards.Add(new ProfitReward(targetGrade));
            Rewards.Add(new FailstackReward());
        }
    }
}