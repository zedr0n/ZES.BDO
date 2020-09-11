using BDO.Enhancement.Stochastics.Rewards;

namespace BDO.Enhancement.Stochastics
{
    public class ExpectedNumberOfItemsProcess : EnhancementProcess 
    {
        public ExpectedNumberOfItemsProcess(int targetGrade, EnhancementState initialState) 
            : base(initialState)
        {
            Rewards.Add(new NumberOfItemsReward(targetGrade));
        }
    }
}