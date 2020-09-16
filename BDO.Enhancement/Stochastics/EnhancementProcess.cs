using ZES.Infrastructure.Stochastics;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement.Stochastics
{
    /// <inheritdoc />
    public class EnhancementProcess : MarkovDecisionProcessBase<EnhancementState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancementProcess"/> class.
        /// </summary>
        /// <param name="initialState">Starting state</param>
        public EnhancementProcess(EnhancementState initialState)
            : base(initialState, 1000000)
        {
        }

        /// <inheritdoc/>
        protected override IValueFunction<EnhancementState> NextFunction() => new ValueFunction<EnhancementState>();
    }
}