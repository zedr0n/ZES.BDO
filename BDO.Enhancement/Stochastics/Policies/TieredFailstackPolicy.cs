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
        private IEnumerable<IMarkovAction<EnhancementState>> _actions;

        public TieredFailstackPolicy(string item, int targetGrade, Dictionary<int, int> failstacks)
        {
            _targetGrade = targetGrade;
            _failstacks = failstacks;
            _item = item;
        }

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

        private bool ApplyRestoreFailstack(EnhancementState state)
        {
            var toGrade = state.Items.Take(_targetGrade).ToList().FindLastIndex(i => i > 0) + 1;
            var b = state.FailStack == 0 && toGrade >= _targetGrade - 1 && state.StoredFailstacks[_targetGrade - toGrade] > 0;
            return b;
        }

        private bool ApplyStoreFailstack(EnhancementState state)
        {
            var b = state.JustFailedGrade > 1 && state.Items[state.JustFailedGrade - 1] == 0 && state.FailStack > _failstacks[state.JustFailedGrade - 1] && state.FailStack > 1;
            return b;
        }

        private bool ApplyFailstack(EnhancementState state)
        {
            var toGrade = state.Items.Take(_targetGrade).ToList().FindLastIndex(i => i > 0) + 1;
            var b = state.FailStack < _failstacks[toGrade - 1];
            return b;
        }

        public double this[IMarkovAction<EnhancementState> action, EnhancementState state]
        {
            get
            {
                if (state.Items[0] <= 0)
                    return 0.0;
                
                if (state.Items[_targetGrade] > 0 && StopAtOnce)
                    return 0.0;
                
                var toGrade = state.Items.Take(_targetGrade).ToList().FindLastIndex(i => i > 0) + 1;
                if (toGrade == 1 && state.Items[0] < 2)
                    return 0.0;

                if (ApplyRestoreFailstack(state) && _actions.Any(a => a is RestoreFailstack))
                {
                    if (action is RestoreFailstack restoreFailstack && restoreFailstack.Slot == _targetGrade - toGrade)
                    {
                        Log?.Info($"[RESTORE] +{state.StoredFailstacks[restoreFailstack.Slot]} at state {state.DebuggerDisplay} after succeding an enhancement to +{toGrade - 1} from slot {restoreFailstack.Slot}");
                        if (Log != null)
                        {
                            var nextState = restoreFailstack[state].First();
                            var nextToGrade = nextState.Items.ToList().FindLastIndex(i => i > 0) + 1;
                            Log?.Info($"[RESTORE] New state : {nextState.DebuggerDisplay} with failstack +{nextState.FailStack} enhancing to +{nextToGrade}");
                        }
                        
                        return 1.0;
                    }

                    return 0.0;
                }

                if (ApplyStoreFailstack(state) && _actions.Any(a => a is StoreFailstack))
                {
                    if (action is StoreFailstack storeFailstack &&
                        storeFailstack.Slot == _targetGrade - state.JustFailedGrade)
                    {
                        Log?.Info($"[STORE] +{state.FailStack} at state {state.DebuggerDisplay} after failing an enhancement to +{state.JustFailedGrade} in slot {storeFailstack.Slot}");
                        if (Log != null)
                        {
                            var nextState = storeFailstack[state].First();
                            var nextToGrade = nextState.Items.ToList().FindLastIndex(i => i > 0) + 1;
                            Log?.Info($"[STORE] New state : {nextState.DebuggerDisplay} with failstack +{nextState.FailStack} enhancing to +{nextToGrade}");
                        }
                        
                        return 1.0;
                    }

                    return 0.0;
                }

                if (ApplyFailstack(state))
                    return action is FailstackAction ? 1.0 : 0.0;

                if (action is EnhancementAction enhancementAction) // && state.FailStack > 0)
                    return enhancementAction.Grade == toGrade ? 1.0 : 0.0;

                return 0.0;
            }
        }

        public IMarkovAction<EnhancementState> this[EnhancementState state]
        {
            get
            {
                if (state.Items[0] <= 0)
                    return null;
                
                if (state.Items[_targetGrade] > 0 && StopAtOnce)
                    return null;
                
                var toGrade = state.Items.Take(_targetGrade).ToList().FindLastIndex(i => i > 0) + 1;
                if (toGrade == 1 && state.Items[0] < 2)
                    return null;

                if (ApplyRestoreFailstack(state))
                {
                    var restoreFailstack = new RestoreFailstack(_targetGrade - toGrade);
                    if (Log != null)
                    {
                        Log?.Info($"[RESTORE] +{state.StoredFailstacks[restoreFailstack.Slot]} at state {state.DebuggerDisplay} after succeding an enhancement to +{toGrade - 1} from slot {restoreFailstack.Slot}");
                        var nextState = restoreFailstack[state].First();
                        var nextToGrade = nextState.Items.ToList().FindLastIndex(i => i > 0) + 1;
                        Log?.Info($"[RESTORE] New state : {nextState.DebuggerDisplay} with failstack +{nextState.FailStack} enhancing to +{nextToGrade}");
                    }

                    return restoreFailstack;
                }

                if (ApplyStoreFailstack(state))
                {
                    var storeFailstack = new StoreFailstack(_targetGrade - state.JustFailedGrade);
                    if (Log != null)
                    {
                        Log?.Info($"[STORE] +{state.FailStack} at state {state.DebuggerDisplay} after failing an enhancement to +{state.JustFailedGrade} in slot {storeFailstack.Slot}");
                        var nextState = storeFailstack[state].First();
                        var nextToGrade = nextState.Items.ToList().FindLastIndex(i => i > 0) + 1;
                        Log?.Info($"[STORE] New state : {nextState.DebuggerDisplay} with failstack +{nextState.FailStack} enhancing to +{nextToGrade}");
                    }

                    return storeFailstack;
                }

                if (ApplyFailstack(state))
                    return new FailstackAction(); 

                return new EnhancementAction(toGrade, _item);
            } 
            set => throw new System.NotImplementedException();
        }
    }
}