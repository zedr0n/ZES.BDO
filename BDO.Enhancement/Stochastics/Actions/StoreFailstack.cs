using System.Collections.Generic;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Actions
{
    public class StoreFailstack : MarkovActionBase<EnhancementState>
    {
        private readonly int _slot;

        public StoreFailstack(int slot)
        {
            _slot = slot;
        }

        public int Slot => _slot;

        public override double this[EnhancementState @from, EnhancementState to] => 1.0;

        public override IEnumerable<EnhancementState> this[EnhancementState current] => new List<EnhancementState>
        {
           current.Clone(s =>
           {
               s.StoredFailstacks[_slot] = s.FailStack;
               s.FailStack = 0;
               s.JustFailedGrade = -1;
           }), 
        };
    }
}