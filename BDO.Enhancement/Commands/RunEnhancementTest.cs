using System;
using ZES.Infrastructure.Domain;

namespace BDO.Enhancement.Commands
{
    public class RunEnhancementTest : Command
    {
        public RunEnhancementTest(string item)
        {
            Item = item;
        }

        public string Item { get; }

        public int NumberOfTests { get; } = 10000;
        public int PathsPerBatch { get; } = 2000;
        public int NumberOfBatches => (int)Math.Ceiling((double)NumberOfTests / NumberOfBatches);
        
        public double BaseChance { get; set; }
        public double BaseIncrease { get; set; }
        
        public double SoftCap { get; set; }
        public double SoftCapIncrease { get; set; }
        
        public int InitialFailstack { get; set; }
    }
}