using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Actions
{
    public class SellAction : MarkovActionBase<EnhancementState>
    {
        private readonly int _sellGrade;
        
        public SellAction(int grade)
        {
            _sellGrade = grade;
        }
        
        protected override EnhancementState[] GetStates(EnhancementState current)
        {
            return new[]
            {
                current.Clone(s =>
                {
                    s.Items[_sellGrade]--;
                    s.Items[1]++;
                }),
            };
        }
    }
}