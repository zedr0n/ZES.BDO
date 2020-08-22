using BDO.Enhancement.Stochastics.Rewards;

namespace BDO.Enhancement.Stochastics
{
    public class ExpectedNumberOfItemsProcess : EnhancementProcess 
    {
        public ExpectedNumberOfItemsProcess(string item, int targetGrade, EnhancementState initialState) 
            : base(item, initialState)
        {
            Rewards.Add(new NumberOfItemsReward(targetGrade));
        }
    }
}