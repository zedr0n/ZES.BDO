using System;
using System.Collections.Generic;

#pragma warning disable SA1600

namespace BDO.Enhancement.Static
{
    public static class Data
    {
        public static int EnhancementBonus = 1;
        public static double CronCost = 2000000;
        public static double ArmorStonePrice = 210000;
        public static double BlackGemPrice = 740000;
        public static double ConcentratedWeaponStonePrice = 2100000;
        
        public static List<EnhancementInfo> EnhancementInfos { get; } = new List<EnhancementInfo>
        {
            new EnhancementInfo("Matchlock", 0)
            {
              BaseChance = 0.72,
              BaseIncrease = 0.0134,
              SoftCap = 0.7,
              SoftCapIncrease = 0.0134,
              ItemLoss = 0,
            },
            new EnhancementInfo("Matchlock", 1)
            {
              BaseChance = 0.4445,
              BaseIncrease = 0.0444,
              SoftCap = 0.7,
              SoftCapIncrease = 0.00888,
              ItemLoss = 0,
            },
            new EnhancementInfo("Matchlock", 2)
            {
              BaseChance = 0.2962,
              BaseIncrease = 0.0297,
              SoftCap = 0.7,
              SoftCapIncrease = 0.00594,
              ItemLoss = 0,
            },
            new EnhancementInfo("Matchlock", 3)
            {
              BaseChance = 0.1967,
              BaseIncrease = 0.0197,
              SoftCap = 0.7,
              SoftCapIncrease = 0.00394,
              ItemLoss = 0,
            },
            new EnhancementInfo("Matchlock", 4)
            {
              BaseChance = 0.1318,
              BaseIncrease = 0.0131,
              SoftCap = 0.7,
              SoftCapIncrease = 0.00262,
              ItemLoss = 0,
            },
            new EnhancementInfo("Silver Embroidered", 0)
            {
                BaseChance = 0.3,
                BaseIncrease = 0.03,
                SoftCap = 0.7,
                SoftCapIncrease = 0.006,
                Price = 400000,
                Cost = 80 * 1240,
                ItemLoss = 1,
            },
            new EnhancementInfo("Silver Embroidered", 1)
            {
                BaseChance = 0.1,
                BaseIncrease = 0.01,
                SoftCap = 0.5,
                SoftCapIncrease = 0.002,
                Price = 3400000, 
                Cost = 80 * 1240,
                ItemLoss = 1,
            },
            new EnhancementInfo("Silver Embroidered", 2)
            {
                BaseChance = 0.075,
                BaseIncrease = 0.0075,
                SoftCap = 0.4,
                SoftCapIncrease = 0.0015,
                Price = 15400000, 
                Cost = 80 * 1240,
                ItemLoss = 1,
            },
            new EnhancementInfo("Silver Embroidered", 3)
            {
                BaseChance = 0.025,
                BaseIncrease = 0.0025,
                SoftCap = 0.3,
                SoftCapIncrease = 0.0005,
                Price = 157000000,
                Cost = 80 * 1240,
                ItemLoss = 1,
            },
            new EnhancementInfo("Silver Embroidered", 4)
            {
                BaseChance = 0.005,
                BaseIncrease = 0.0005,
                SoftCap = 0.2,
                SoftCapIncrease = 0.0002,
                Price = 885000000, 
                Cost = 80 * 1240,
                ItemLoss = 1,
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
            new EnhancementInfo("Reblath", 0)
            {
                ArmorStone = 1,
            },
            new EnhancementInfo("Reblath", 1)
            {
                ArmorStone = 1,
            },
            new EnhancementInfo("Reblath", 2)
            {
                ArmorStone = 1,
            },
            new EnhancementInfo("Reblath", 3)
            {
                ArmorStone = 1,
            },
            new EnhancementInfo("Reblath", 4)
            {
                ItemLoss = 0,
                BaseChance = 0.02,
                BaseIncrease = 0.002,
                SoftCap = 0.3,
                SoftCapIncrease = 0.0002,
                ArmorStone = 1,
                RepairCost = 12900 / 10 * 5,
            },
            new EnhancementInfo("Reblath", 5)
            {
                ArmorStone = 1,
            },
            new EnhancementInfo("Boss", 0)
            {
                ItemLoss = 0,
                BaseChance = 0.1176,
                BaseIncrease = 0.0118,
                SoftCap = 0.7,
                SoftCapIncrease = 0.0024,
                NumberOfStacks = 2,
                ConcentratedWeaponStone = 1,
            },
            new EnhancementInfo("Boss", 1)
            {
                ItemLoss = 0,
                BaseChance = 0.0769,
                BaseIncrease = 0.0077,
                SoftCap = 0.7,
                SoftCapIncrease = 0.0016,
                NumberOfStacks = 3,
                ConcentratedWeaponStone = 1,
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
                Cron = 19,
                ConcentratedWeaponStone = 1,
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
                ConcentratedWeaponStone = 1,
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
                ConcentratedWeaponStone = 1,
            },
            new EnhancementInfo("Loggia", 0)
            {
                ItemLoss = 0,
                BaseChance = 0.1176,
                BaseIncrease = 0.0118,
                SoftCap = 0.7,
                SoftCapIncrease = 0.0024,
                NumberOfStacks = 2,
                ConcentratedWeaponStone = 1,
                RepairCost = 1000000,
            },
            new EnhancementInfo("Loggia", 1)
            {
                ItemLoss = 0,
                BaseChance = 0.0769,
                BaseIncrease = 0.0077,
                SoftCap = 0.7,
                SoftCapIncrease = 0.0016,
                NumberOfStacks = 3,
                ConcentratedWeaponStone = 1,
                RepairCost = 1000000,
            },
            new EnhancementInfo("Loggia", 2)
            {
                ItemLoss = 0,
                BaseChance = 0.0625,
                BaseIncrease = 0.0063,
                SoftCap = 0.7,
                SoftCapIncrease = 0.0012,
                NumberOfStacks = 4,
                DropEnhancementGrade = true,
                Cron = 19,
                ConcentratedWeaponStone = 1,
                RepairCost = 1000000,
            },
            new EnhancementInfo("Loggia", 3)
            {
                ItemLoss = 0,
                BaseChance = 0.02,
                BaseIncrease = 0.0020,
                SoftCap = 0.3,
                SoftCapIncrease = 0.0002,
                NumberOfStacks = 5,
                DropEnhancementGrade = true,
                ConcentratedWeaponStone = 1,
                RepairCost = 1000000,
            },
            new EnhancementInfo("Loggia", 4)
            {
                ItemLoss = 0,
                BaseChance = 0.003,
                BaseIncrease = 0.0003,
                SoftCap = 0.3,
                SoftCapIncrease = 0.00003,
                NumberOfStacks = 6,
                DropEnhancementGrade = true,
                ConcentratedWeaponStone = 1,
                RepairCost = 1000000,
            },
            new EnhancementInfo("Loggia Accessory", 0)
            {
                ItemLoss = 1,
                BaseChance = 0.7,
                BaseIncrease = 0,
                SoftCap = 1,
                SoftCapIncrease = 0,
                BlackGem = 1,
            },
            new EnhancementInfo("Loggia Accessory", 1)
            {
                ItemLoss = 1,
                BaseChance = 0.4,
                BaseIncrease = 0,
                SoftCap = 1,
                SoftCapIncrease = 0,
                BlackGem = 2,
            },
            new EnhancementInfo("Loggia Accessory", 2)
            {
                ItemLoss = 1,
                BaseChance = 0.3,
                BaseIncrease = 0,
                SoftCap = 1,
                SoftCapIncrease = 0,
                BlackGem = 3,
            },
            new EnhancementInfo("Loggia Accessory", 3)
            {
                ItemLoss = 1,
                BaseChance = 0.2,
                BaseIncrease = 0,
                SoftCap = 1,
                SoftCapIncrease = 0,
                BlackGem = 4,
            },
            new EnhancementInfo("Loggia Accessory", 4)
            {
                ItemLoss = 1,
                BaseChance = 0.1,
                BaseIncrease = 0,
                SoftCap = 1,
                SoftCapIncrease = 0,
                BlackGem = 5,
            },
            new EnhancementInfo("Grunil", 0)
            {
                ItemLoss = 0,
                BaseChance = 0.1176,
                BaseIncrease = 0.0118,
                SoftCap = 0.7,
                SoftCapIncrease = 0.0024,
                NumberOfStacks = 2,
                ConcentratedWeaponStone = 1,
                RepairCost = 121000,
            },
            new EnhancementInfo("Grunil", 1)
            {
                ItemLoss = 0,
                BaseChance = 0.0769,
                BaseIncrease = 0.0077,
                SoftCap = 0.7,
                SoftCapIncrease = 0.0016,
                NumberOfStacks = 3,
                ConcentratedWeaponStone = 1,
                RepairCost = 121000,
            },
            new EnhancementInfo("Grunil", 2)
            {
                ItemLoss = 0,
                BaseChance = 0.0625,
                BaseIncrease = 0.0063,
                SoftCap = 0.7,
                SoftCapIncrease = 0.0012,
                NumberOfStacks = 4,
                DropEnhancementGrade = true,
                ConcentratedWeaponStone = 1,
                RepairCost = 121000,
            },
            new EnhancementInfo("Grunil", 3)
            {
                ItemLoss = 0,
                BaseChance = 0.02,
                BaseIncrease = 0.0020,
                SoftCap = 0.3,
                SoftCapIncrease = 0.0002,
                NumberOfStacks = 5,
                DropEnhancementGrade = true,
                ConcentratedWeaponStone = 1,
                RepairCost = 1000000,
            },
            new EnhancementInfo("Grunil", 4)
            {
                ItemLoss = 0,
                BaseChance = 0.003,
                BaseIncrease = 0.0003,
                SoftCap = 0.3,
                SoftCapIncrease = 0.00003,
                NumberOfStacks = 6,
                DropEnhancementGrade = true,
                ConcentratedWeaponStone = 1,
                RepairCost = 1000000,
            },
        };
        
        public class EnhancementInfo
        {
            public EnhancementInfo(string name, int grade)
            {
                Name = name;
                Grade = grade;
            }

            public int Cron { get; set; }
            public int ConcentratedWeaponStone { get; set; }
            public int ArmorStone { get; set; }
            public int BlackGem { get; set; }
            public double RepairCost { get; set; }
            public double Cost { get; set; }
            public double Price { get; set; }
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