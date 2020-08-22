using BDO.Enhancement.Stochastics.Rewards;

namespace BDO.Enhancement.Stochastics
{
    public class SuccessProbabilityProcess : EnhancementProcess 
    {
        public SuccessProbabilityProcess(string item, int targetGrade, EnhancementState initialState) 
            : base(item, initialState)
        {
            Rewards.Add(new SuccessProbabilityReward(targetGrade));
        }
    }
}