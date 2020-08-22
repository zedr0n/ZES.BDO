using BDO.Enhancement.Stochastics.Rewards;

namespace BDO.Enhancement.Stochastics
{
    public class NumberOfAttemptsProcess : EnhancementProcess
    {
        public NumberOfAttemptsProcess(string item, int targetGrade, int failstack) 
            : this(item, targetGrade, new EnhancementState(failstack))
        {
        }

        public NumberOfAttemptsProcess(string item, int targetGrade, EnhancementState initialState)
            : base(item, initialState)
        {
            Rewards.Add(new NumberOfAttemptsReward(targetGrade));
        }
    }
}