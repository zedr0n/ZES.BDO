using System.Collections.Generic;
using System.Linq;
using BDO.Enhancement.Static;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement.Stochastics.Policies
{
    public class GrunilPolicy : IDeterministicPolicy<EnhancementState>
    {
        private readonly string _item = "Boss";
        private readonly int _targetFailstack;
        private readonly int _minFailstack;
        private readonly int _sellGrade;
        private readonly EnhancementAction[] _enhancementActions;
        
        public GrunilPolicy(int targetFailstack, int minFailstack, int sellGrade)
        {
            _sellGrade = sellGrade;
            _minFailstack = minFailstack;
            _targetFailstack = targetFailstack;
            _enhancementActions = new EnhancementAction[6];
            for (var grade = 1; grade <= 5; ++grade)
            {
                var info = Data.EnhancementInfos.SingleOrDefault(i => i.IsFor(_item, grade - 1)); 
                _enhancementActions[grade] = new EnhancementAction(grade, info);
            }
        }

        public Dictionary<int, int> FailstackToGrade { get; set; } = new Dictionary<int, int>()
        {
            { 31, 2 },
            { 43, 3 },
        };

        public IMarkovAction<EnhancementState>[] GetAllowedActions(EnhancementState state)
        {
            throw new System.NotImplementedException();
        }

        public bool IsModified { get; set; } = true;

        public EnhancementState[] Modifications { get; }

        public IMarkovAction<EnhancementState> this[EnhancementState state]
        {
            get
            {
                if (state.FailStack >= _targetFailstack)
                    return null;

                if (state.Items[_sellGrade] > 0)
                    return new SellAction(_sellGrade);

                var toGrade = GetToGrade(state);
                
                if (state.Items[toGrade - 1] == 0)
                    return new AddItemAtGradeAction(toGrade - 1);
                
                if (state.FailStack < _minFailstack)
                    return new FailstackAction(_minFailstack - state.FailStack);
                
                return _enhancementActions[toGrade];
            } 
            set => throw new System.NotImplementedException();
        }
        
        private int GetToGrade(EnhancementState state)
        {
            foreach (var x in FailstackToGrade)
            {
                if (state.FailStack < x.Key)
                    return x.Value;
            }

            return FailstackToGrade.Max(x => x.Value) + 1;
        }
    }
}