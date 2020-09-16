using System;
using System.Collections.Generic;

#pragma warning disable SA1600

namespace BDO.Enhancement.Static
{
    public static class Data
    {
        public static int EnhancementBonus = 1;
        
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
            new EnhancementInfo("Silver Embroidered", 2)
            {
                BaseChance = 0.075,
                BaseIncrease = 0.0075,
                SoftCap = 0.4,
                SoftCapIncrease = 0.0015,
            },
            new EnhancementInfo("Silver Embroidered", 3)
            {
                BaseChance = 0.025,
                BaseIncrease = 0.0025,
                SoftCap = 0.3,
                SoftCapIncrease = 0.0005,
            },
            new EnhancementInfo("Silver Embroidered", 4)
            {
                BaseChance = 0.005,
                BaseIncrease = 0.0005,
                SoftCap = 0.2,
                SoftCapIncrease = 0.0002,
            },
            new EnhancementInfo("Gold accessory", 0)
            {
                BaseChance = 0.25,
                BaseIncrease = 0.025,
                SoftCap = 0.7,
                SoftCapIncrease = 0.005,
            },
            new EnhancementInfo("Gold accessory", 1)
            {
                BaseChance = 0.1,
                BaseIncrease = 0.01,
                SoftCap = 0.5,
                SoftCapIncrease = 0.002,
            },
            new EnhancementInfo("Gold accessory", 2)
            {
                BaseChance = 0.075,
                BaseIncrease = 0.0075,
                SoftCap = 0.4,
                SoftCapIncrease = 0.0015,
            },
            new EnhancementInfo("Gold accessory", 3)
            {
                BaseChance = 0.025,
                BaseIncrease = 0.0025,
                SoftCap = 0.3,
                SoftCapIncrease = 0.0005,
            },
            new EnhancementInfo("Dummy", 0)
            {
                BaseChance = 1,
                BaseIncrease = 0,
                SoftCap = 1,
                SoftCapIncrease = 0,
            },
            new EnhancementInfo("Armor", 4)
            {
                ItemLoss = 0,
                BaseChance = 0.02,
                BaseIncrease = 0.002,
                SoftCap = 0.3,
                SoftCapIncrease = 0.0002,
            },
            new EnhancementInfo("Boss", 0)
            {
                ItemLoss = 0,
                BaseChance = 0.1176,
                BaseIncrease = 0.0118,
                SoftCap = 0.7,
                SoftCapIncrease = 0.0024,
                NumberOfStacks = 2,
            },
            new EnhancementInfo("Boss", 1)
            {
                ItemLoss = 0,
                BaseChance = 0.0769,
                BaseIncrease = 0.0077,
                SoftCap = 0.7,
                SoftCapIncrease = 0.0016,
                NumberOfStacks = 3,
            },
            new EnhancementInfo("Boss", 2)
            {
                ItemLoss = 0,
                BaseChance = 0.0625,
                BaseIncrease = 0.0063,
                SoftCap = 0.7,
                SoftCapIncrease = 0.0012,
                NumberOfStacks = 4,
                DropEnhancementGrade = true,
            },
            new EnhancementInfo("Boss", 3)
            {
                ItemLoss = 0,
                BaseChance = 0.02,
                BaseIncrease = 0.0020,
                SoftCap = 0.3,
                SoftCapIncrease = 0.0002,
                NumberOfStacks = 5,
                DropEnhancementGrade = true,
            },
            new EnhancementInfo("Boss", 4)
            {
                ItemLoss = 0,
                BaseChance = 0.003,
                BaseIncrease = 0.0003,
                SoftCap = 0.3,
                SoftCapIncrease = 0.00003,
                NumberOfStacks = 6,
                DropEnhancementGrade = true,
            },
        };
        
        public class EnhancementInfo
        {
            public EnhancementInfo(string name, int grade)
            {
                Name = name;
                Grade = grade;
            }

            public bool DropEnhancementGrade { get; set; } = false;
            public int NumberOfStacks { get; set; } = 1;
            public string Name { get; }
            public int Grade { get; }
            public int ItemLoss { get; set; } = 1;
            public double BaseChance { get; set; }
            public double BaseIncrease { get; set; }
            public double SoftCap { get; set; }
            public double SoftCapIncrease { get; set; }
            
            public bool IsFor(string item, int grade) => Grade == grade && item.StartsWith(Name, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}