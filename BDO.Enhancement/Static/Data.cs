using System;
using System.Collections.Generic;

#pragma warning disable SA1600

namespace BDO.Enhancement.Static
{
    public static class Data
    {
        public static List<EnhancementInfo> EnhancementInfos { get; } = new List<EnhancementInfo>
        {
            new EnhancementInfo("Silver Embroidered", 0)
            {
                BaseChance = 0.3,
                BaseIncrease = 0.03,
                SoftCap = 0.7,
                SoftCapIncrease = 0.006,
            },
            new EnhancementInfo("Silver Embroidered", 1)
            {
                BaseChance = 0.1,
                BaseIncrease = 0.01,
                SoftCap = 0.5,
                SoftCapIncrease = 0.002,
            },
            new EnhancementInfo("Silver Embroidered", 4)
            {
                BaseChance = 0.005,
                BaseIncrease = 0.0005,
                SoftCap = 0.2,
                SoftCapIncrease = 0.0002,
            },
            new EnhancementInfo("Dummy", 0)
            {
                BaseChance = 1,
                BaseIncrease = 0,
                SoftCap = 1,
                SoftCapIncrease = 0,
            },
        };
        
        public class EnhancementInfo
        {
            public EnhancementInfo(string name, int grade)
            {
                Name = name;
                Grade = grade;
            }

            public string Name { get; }
            public int Grade { get; }
            public double BaseChance { get; set; }
            public double BaseIncrease { get; set; }
            public double SoftCap { get; set; }
            public double SoftCapIncrease { get; set; }
            
            public bool IsFor(string item, int grade) => item.StartsWith(Name, StringComparison.InvariantCultureIgnoreCase) && Grade == grade;
        }
    }
}