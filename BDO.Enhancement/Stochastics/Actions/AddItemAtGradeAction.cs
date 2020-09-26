using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Actions
{
    public class AddItemAtGradeAction : MarkovActionBase<EnhancementState>
    {
        private readonly int _grade;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddItemAtGradeAction"/> class.
        /// </summary>
        /// <param name="grade">Target grade</param>
        public AddItemAtGradeAction(int grade)
        {
            _grade = grade;
        }

        public int Grade => _grade;

        /// <inheritdoc/>
        protected override EnhancementState[] GetStates(EnhancementState current)
        {
            return new[] { current.Clone(s => s.Items[_grade]++) };
        }
    }
}