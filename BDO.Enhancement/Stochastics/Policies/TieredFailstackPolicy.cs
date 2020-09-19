using System;
using System.Collections.Generic;
using System.Linq;
using BDO.Enhancement.Static;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Interfaces;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement.Stochastics.Policies
{
    public class TieredFailstackPolicy : IPolicy<EnhancementState>, IDeterministicPolicy<EnhancementState>
    {
        private readonly string _item;
        private readonly int _targetGrade;
        private readonly Dictionary<int, int> _failstacks;
        private readonly EnhancementAction[] _enhancementActions;
        private IEnumerable<IMarkovAction<EnhancementState>> _actions;

        public TieredFailstackPolicy(string item, int targetGrade, Dictionary<int, int> failstacks)
        {
            _targetGrade = targetGrade;
            _failstacks = failstacks;
            _item = item;
            _enhancementActions = new EnhancementAction[targetGrade + 1]; 
            for (var grade = 1; grade <= targetGrade; ++grade)
            {
                var info = Data.EnhancementInfos.SingleOrDefault(i => i.IsFor(item, grade - 1)); 
                _enhancementActions[grade] = new EnhancementAction(grade, info);
            }
        }
        
        public Dictionary<int, int> MaxGradeFailstacks { get; } = new Dictionary<int, int>();

        public bool StopAtOnce { get; set; } = true;
        public ILog Log { get; set; }
        
        public IEnumerable<IMarkovAction<EnhancementState>> GetAllowedActions()
        {
            var list = new List<IMarkovAction<EnhancementState>>();
            for (var i = 1; i <= _targetGrade; ++i)
                list.Add(new EnhancementAction(i, _item));
            list.Add(new FailstackAction());
            list.Add(new StoreFailstack(0));
            list.Add(new StoreFailstack(1));
            list.Add(new RestoreFailstack(0));
            list.Add(new RestoreFailstack(1));
            _actions = list;
            return list;
        }

        private int GetToGrade(EnhancementState state)
        {
            var toGrade = 0;
            var items = state.Items;
            for (var i = 0; i < _targetGrade; ++i)
            {
                if (items[i] > 0)
                    toGrade = i + 1;
            }

            return toGrade;
        }

        private int ApplyRestoreFailstack(EnhancementState state, int toGrade)
        {
            if (state.FailStack != 0)
                return -1;

            if (toGrade == _targetGrade && state.StoredFailstacks[state.StoredFailstacks.Length - 1] > 0)
                return state.StoredFailstacks.Length - 1;
            
            if (toGrade >= 2 && state.StoredFailstacks[_targetGrade - toGrade] > 0)
                return _targetGrade - toGrade;
            return -1;
        }

        private int ApplyStoreFailstack(EnhancementState state, int toGrade)
        {
            if (MaxGradeFailstacks.TryGetValue(toGrade, out var maxFailstack))
            {
                if (state.FailStack > maxFailstack && state.StoredFailstacks[state.StoredFailstacks.Length - 1] == 0)
                    return state.StoredFailstacks.Length - 1;
            }

            if (state.JustFailedGrade > 1 && state.Items[state.JustFailedGrade - 1] == 0 &&
                state.FailStack > _failstacks[state.JustFailedGrade - 1] && state.FailStack > 1)
                return _targetGrade - state.JustFailedGrade;
            return -1;
        }

        private int ApplyFailstack(EnhancementState state, int toGrade)
        {
            // var toGrade = state.Items.Take(_targetGrade).ToList().FindLastIndex(i => i > 0) + 1;
            // var b = state.FailStack < _failstacks[toGrade - 1];
            return _failstacks[toGrade - 1] - state.FailStack;
        }

        public IMarkovAction<EnhancementState> this[EnhancementState state]
        {
            get
            {
                if (state.Items[0] <= 0)
                    return null;
                
                if (state.Items[_targetGrade] > 0 && StopAtOnce)
                    return null;

                var toGrade = GetToGrade(state);
                // var toGrade = state.Items.Take(_targetGrade).ToList().FindLastIndex(i => i > 0) + 1;
                if (toGrade == 1 && state.Items[0] < 2)
                    return null;

                var restoreSlot = ApplyRestoreFailstack(state, toGrade);
                if (restoreSlot >= 0)
                {
                    var restoreFailstack = new RestoreFailstack(restoreSlot);
                    if (Log != null)
                    {
                        Log?.Info($"[RESTORE] +{state.StoredFailstacks[restoreFailstack.Slot]} at state {state.DebuggerDisplay} after succeding an enhancement to +{toGrade - 1} from slot {restoreFailstack.Slot}");
                        var nextState = restoreFailstack[state].First();
                        var nextToGrade = nextState.Items.ToList().FindLastIndex(i => i > 0) + 1;
                        Log?.Info($"[RESTORE] New state : {nextState.DebuggerDisplay} with failstack +{nextState.FailStack} enhancing to +{nextToGrade}");
                    }

                    return restoreFailstack;
                }

                var storeSlot = ApplyStoreFailstack(state, toGrade);
                if (storeSlot >= 0)
                {
                    var storeFailstack = new StoreFailstack(storeSlot);
                    if (Log != null)
                    {
                        Log?.Info($"[STORE] +{state.FailStack} at state {state.DebuggerDisplay} after failing an enhancement to +{state.JustFailedGrade} in slot {storeFailstack.Slot}");
                        var nextState = storeFailstack[state].First();
                        var nextToGrade = nextState.Items.ToList().FindLastIndex(i => i > 0) + 1;
                        Log?.Info($"[STORE] New state : {nextState.DebuggerDisplay} with failstack +{nextState.FailStack} enhancing to +{nextToGrade}");
                    }

                    return storeFailstack;
                }

                var failstack = ApplyFailstack(state, toGrade);
                if (failstack > 0)
                    return new FailstackAction(failstack); 

                // return new EnhancementAction(toGrade, _item);
                return _enhancementActions[toGrade];
                // return new EnhancementAction(toGrade, _info[toGrade - 1]);
            } 
            set => throw new System.NotImplementedException();
        }
    }
}