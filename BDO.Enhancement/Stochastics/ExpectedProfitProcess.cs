using System.Linq;
using BDO.Enhancement.Static;
using BDO.Enhancement.Stochastics.Rewards;

namespace BDO.Enhancement.Stochastics
{
    public class ExpectedProfitProcess : EnhancementProcess
    {
        public ExpectedProfitProcess(string item, int targetGrade, EnhancementState initialState) 
            : base(initialState)
        {
            Rewards.Add(new ProfitReward(targetGrade, Data.EnhancementInfos.Where(i => i.Name == item).ToArray()));
            Rewards.Add(new FailstackReward());
        }
    }
}