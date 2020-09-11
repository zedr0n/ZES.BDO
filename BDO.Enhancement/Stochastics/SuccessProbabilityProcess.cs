using BDO.Enhancement.Stochastics.Rewards;

namespace BDO.Enhancement.Stochastics
{
    public class SuccessProbabilityProcess : EnhancementProcess 
    {
        public SuccessProbabilityProcess(int targetGrade, EnhancementState initialState) 
            : base(initialState)
        {
            Rewards.Add(new SuccessProbabilityReward(targetGrade));
        }
    }
}