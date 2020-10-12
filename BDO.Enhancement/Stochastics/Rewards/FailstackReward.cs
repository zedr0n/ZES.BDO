using System.Collections.Generic;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;

namespace BDO.Enhancement.Stochastics.Rewards
{
    public class FailstackReward : ActionReward<EnhancementState, FailstackAction>
    {
        private readonly bool _invert;
        
        public FailstackReward(bool invert = true)
        {
            _invert = invert;
        }
        
        // shoes
        private static Dictionary<int, double> _shoesCosts = new Dictionary<int, double>
        {
            { 1, 223423 },
            { 2, 229567 },
            { 3, 236817 },
            { 4, 245251 },
            { 5, 254957 },
            { 6, 266037 },
            { 7, 278609 },
            { 8, 292807 },
            { 9, 308782 },
            { 10, 326710 },
            { 11, 346789 },
            { 12, 369242 },
            { 13, 394326 },
            { 14, 422332 },
            { 15, 453589 },
            { 16, 488476 },
            { 17, 527419 },
            { 18, 570907 },
            { 19, 619497 },
            { 20, 673825 },
            { 21, 734617 },
            { 22, 802705 },
            { 23, 879042 },
            { 24, 964722 },
            { 25, 1061000 },
            { 26, 1169321 },
            { 27, 1291350 },
            { 28, 1429005 },
            { 29, 1584504 },
            { 30, 1760406 },
            { 31, 1959657 },
            { 32, 2185608 },
            { 33, 2441972 },
            { 34, 2732648 },
            { 35, 3061320 },
            { 36, 3430792 },
            { 37, 3842045 },
            { 38, 4293162 },
            { 39, 4778340 },
            { 40, 5287282 },
            { 41, 6369572 },
            { 42, 7243573 },
            { 43, 8253615 },
            { 44, 9422876 },
            { 45, 10779028 },
            
            // Grunil
            { 46, 5860179 },
            { 47, 9458550 },
            { 48, 9393789 },
            { 49, 12376892 },
            { 50, 7683624 },
            { 51, 14420141 },
            { 52, 7929247 },
        };

        // armor
        private Dictionary<int, double> _costs = new Dictionary<int, double>
        {
            { 0, 0 },
            { 1, 229473 },
            { 2, 235766 },
            { 3, 243194 },
            { 4, 251838 },
            { 5, 261787 },
            { 6, 273147 },
            { 7, 286037 },
            { 8, 300596 },
            { 9, 316980 },
            { 10, 335367 },
            { 11, 355961 },
            { 12, 378992 },
            { 13, 404723 },
            { 14, 433451 },
            { 15, 465516 },
            { 16, 501305 },
            { 17, 541256 },
            { 18, 585871 },
            { 19, 635720 },
            { 20, 691456 },
            { 21, 753825 },
            { 22, 823680 },
            { 23, 901999 },
            { 24, 989903 },
            { 25, 1088682 },
            { 26, 1199817 },
            { 27, 1325015 },
            { 28, 1466248 },
            { 29, 1625788 },
            { 30, 1806262 },
            { 31, 2010691 },
            { 32, 2242515 },
            { 33, 2505544 },
            { 34, 2803776 },
            { 35, 3140992 },
            { 36, 3520069 },
            { 37, 3942013 },
            { 38, 4404856 },
            { 39, 4902645 },
            { 40, 5424814 },
        };

        public static double GetCost(int failstack)
        {
            var cost = 0.0;
            for (var i = 1; i <= failstack; i++)
                cost += _shoesCosts[i];

            return cost;
        }
        
        public override double this[EnhancementState @from, EnhancementState to, FailstackAction action]
        {
            get
            {
                var cost = 0.0;
                for (var failstack = from.FailStack + 1; failstack <= to.FailStack; failstack++)
                {
                    cost += _shoesCosts[failstack];
                }

                if (_invert)
                    cost *= -1;
                
                return cost;
            }
        }
    }
}