using System.Collections.Generic;
using System.Linq;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Actions
{
    public class RestoreFailstack : MarkovActionBase<EnhancementState>
    {
        private readonly int _slot;
        
        public RestoreFailstack(int slot)
        {
            _slot = slot;
        }

        public int Slot => _slot;

        public override IEnumerable<EnhancementState> this[EnhancementState current] => new List<EnhancementState> 
        { 
            current.Clone(
            s =>
            {
                s.FailStack = s.StoredFailstacks[_slot];
                s.StoredFailstacks[_slot] = 0;
                s.JustFailedGrade = -1;
            }), 
        };
    }
}