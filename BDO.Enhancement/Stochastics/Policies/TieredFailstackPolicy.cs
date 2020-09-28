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
        private readonly EnhancementAction[] _enhancementActions;
        private readonly FailstackAction[] _failstackActions;
        private readonly StoreFailstack[] _storeActions;
        private readonly RestoreFailstack[] _restoreActions;

        private const int MAX_FAILSTACK = 100;

        public TieredFailstackPolicy(string item, int targetGrade, Dictionary<int, int> failstacks)
            : this(item, targetGrade)
        {
            Failstacks = failstacks;
        }
            
        public TieredFailstackPolicy(string item, int targetGrade)
        {
            _targetGrade = targetGrade;
            _item = item;
            _enhancementActions = new EnhancementAction[targetGrade + 1]; 
            _failstackActions = new FailstackAction[MAX_FAILSTACK + 1];
            _storeActions = new StoreFailstack[4];
            _restoreActions = new RestoreFailstack[4];
            for (var slot = 0; slot < 4; ++slot)
                _storeActions[slot] = new StoreFailstack(slot);
            for (var slot = 0; slot < 4; ++slot)
                _restoreActions[slot] = new RestoreFailstack(slot);
            for (var i = 0; i < MAX_FAILSTACK; ++i)
                _failstackActions[i] = new FailstackAction(i);
            for (var grade = 1; grade <= targetGrade; ++grade)
            {
                var info = Data.EnhancementInfos.SingleOrDefault(i => i.IsFor(item, grade - 1)); 
                _enhancementActions[grade] = new EnhancementAction(grade, info);
            }
        }
        
        public Dictionary<int, int> Failstacks { get; set; }
        public Dictionary<int, int> MaxGradeFailstacks { get; set; } = new Dictionary<int, int>();

        public bool StopAtOnce { get; set; } = true;
        public ILog Log { get; set; }
        
        public IEnumerable<IMarkovAction<EnhancementState>> GetAllowedActions(EnhancementState state)
        {
            var list = new List<IMarkovAction<EnhancementState>>();
            for (var grade = 1; grade <= _targetGrade; ++grade)
            {
                if (state.Items[0] > 0 && state.Items[0] + state.Items[grade - 1] > 1)
                    list.Add(_enhancementActions[grade]);
            }

            for (var slot = 0; slot < 4; slot++)
            {
               if (state.StoredFailstacks[slot] == 0)
                   list.Add(_storeActions[slot]);
               if (state.StoredFailstacks[slot] > 0 && state.FailStack == 0)
                   list.Add(_restoreActions[slot]);
            }
            
            list.AddRange(_failstackActions.Skip(state.FailStack));
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
                state.FailStack > Failstacks[state.JustFailedGrade - 1] && state.FailStack > 1)
                return _targetGrade - state.JustFailedGrade;
            return -1;
        }

        private int ApplyFailstack(EnhancementState state, int toGrade)
        {
            // var toGrade = state.Items.Take(_targetGrade).ToList().FindLastIndex(i => i > 0) + 1;
            // var b = state.FailStack < Failstacks[toGrade - 1];
            return Failstacks[toGrade - 1] - state.FailStack;
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
                    // var restoreFailstack = new RestoreFailstack(restoreSlot);
                    var restoreFailstack = _restoreActions[restoreSlot];
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
                    // var storeFailstack = new StoreFailstack(storeSlot);
                    var storeFailstack = _storeActions[storeSlot];
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
                    return _failstackActions[failstack];
                    // return new FailstackAction(failstack); 

                // return new EnhancementAction(toGrade, _item);
                return _enhancementActions[toGrade];
                // return new EnhancementAction(toGrade, _info[toGrade - 1]);
            } 
            set => throw new System.NotImplementedException();
        }
    }
}