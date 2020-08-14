using System.Collections.Concurrent;

namespace BDO.Enhancement.Queries
{
    public class EnhancementTestResultState : ZES.Interfaces.Domain.IState
    {
        public ConcurrentDictionary<string, int> NumberOfFailures { get; } = new ConcurrentDictionary<string, int>();
    }
}