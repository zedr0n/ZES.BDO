using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Actions
{
    public class BookFailstack : MarkovActionBase<EnhancementState>
    {
        /// <inheritdoc/>
        public override string ToString()
        {
            return "Book";
        }

        /// <inheritdoc/>
        protected override EnhancementState[] GetStates(EnhancementState current)
        {
            if (current.FailStack > 50)
                return new EnhancementState[] { null };
            
            return new[]
            {
                current.Clone(s =>
                {
                    s.FailStack = 0;
                    s.JustFailedGrade = -1;
                }),
            };
        }
    }
}