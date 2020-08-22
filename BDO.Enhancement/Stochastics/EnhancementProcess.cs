using ZES.Infrastructure.Stochastics;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement.Stochastics
{
    /// <inheritdoc />
    public class EnhancementProcess : MarkovDecisionProcessBase<EnhancementState, EnhancementProbability>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancementProcess"/> class.
        /// </summary>
        /// <param name="item">Item to enhance</param>
        /// <param name="targetGrade">Target grade</param>
        /// <param name="initialState">Starting state</param>
        public EnhancementProcess(string item, EnhancementState initialState)
            : base(initialState, new EnhancementProbability(item), 1000)
        {
        }

        /// <inheritdoc/>
        protected override IValueFunction<EnhancementState> NextFunction() => new ValueFunction<EnhancementState>();
    }
}