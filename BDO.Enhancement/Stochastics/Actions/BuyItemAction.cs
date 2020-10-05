using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Actions
{
    public class BuyItemAction : MarkovActionBase<EnhancementState>
    {
        /// <inheritdoc/>
        protected override EnhancementState[] GetStates(EnhancementState current)
        {
            return new[]
            {
                current.Clone(s =>
                {
                    s.Items[0]++;
                    s.JustFailedGrade = -1;
                }),
            };
        }
    }
}