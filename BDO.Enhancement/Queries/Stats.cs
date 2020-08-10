using System.Threading;
using ZES.Interfaces.Domain;

namespace BDO.Enhancement.Queries
{
    public class Stats : IState
    {
        private int _numberOfFailures;

        public int NumberOfFailures => _numberOfFailures;

        public void Increment()
        {
            Interlocked.Increment(ref _numberOfFailures);
        }
    }
}