using BDO.Enhancement.Stochastics.Rewards;

namespace BDO.Enhancement.Stochastics
{
    public class NumberOfAttemptsProcess : EnhancementProcess
    {
        public NumberOfAttemptsProcess(int targetGrade, int failstack) 
            : this(targetGrade, new EnhancementState(failstack))
        {
        }

        public NumberOfAttemptsProcess(int targetGrade, EnhancementState initialState)
            : base(initialState)
        {
            Rewards.Add(new NumberOfAttemptsReward(targetGrade));
        }
    }
}