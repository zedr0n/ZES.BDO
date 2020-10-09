using System;
using System.Collections.Generic;
using System.Linq;
using BDO.Enhancement.Static;
using BDO.Enhancement.Stochastics.Actions;
using ZES.Infrastructure.Stochastics;
using ZES.Interfaces;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement.Stochastics.Policies
{
    public class TieredFailstackPolicy : MarkovPolicy<EnhancementState>
    {
        private readonly string _item;
        private readonly int _targetGrade;
        private int _itemLoss = 0;
        private EnhancementAction[] _enhancementActions;
        private EnhancementAction[] _cronActions;
        private FailstackAction[] _failstackActions;
        private StoreFailstack[] _storeActions;
        private RestoreFailstack[] _restoreActions;
        private ValkAction[] _valkActions;

        private const int MAX_FAILSTACK = 45;

        public TieredFailstackPolicy(string item, int targetGrade, Dictionary<int, int> failstacks)
            : this(item, targetGrade)
        {
            Failstacks = failstacks;
        }
            
        public TieredFailstackPolicy(string item, int targetGrade)
        {
            _targetGrade = targetGrade;
            _item = item;

            if (targetGrade >= 3)
                _cronActions = new EnhancementAction[targetGrade - 3 + 1];
            _enhancementActions = new EnhancementAction[targetGrade + 1]; 
            _failstackActions = new FailstackAction[MAX_FAILSTACK];
            _storeActions = new StoreFailstack[4];
            _restoreActions = new RestoreFailstack[4];
            _valkActions = new ValkAction[10];
            for(var i = 1; i <= 10; ++i)
                _valkActions[i - 1] = new ValkAction(i);
            for (var slot = 0; slot < 4; ++slot)
                _storeActions[slot] = new StoreFailstack(slot);
            for (var slot = 0; slot < 4; ++slot)
                _restoreActions[slot] = new RestoreFailstack(slot);
            for (var i = 0; i < MAX_FAILSTACK; ++i)
                _failstackActions[i] = new FailstackAction(i + 1);
            
            for (var grade = 1; grade <= targetGrade; ++grade)
            {
                var info = Data.EnhancementInfos.SingleOrDefault(i => i.IsFor(item, grade - 1)); 
                if (info == null)
                    throw new InvalidOperationException();

                _itemLoss = info.ItemLoss;
                _enhancementActions[grade] = new EnhancementAction(grade, info);
                if (info.Cron > 0 && grade >= 3)
                    _cronActions[grade - 3] = new EnhancementAction(grade, info) { UseCron = true };                    
            }
        }
        
        public Dictionary<int, int> Failstacks { get; set; }
        public Dictionary<int, int> MaxGradeFailstacks { get; set; } = new Dictionary<int, int>();

        public int TargetFailstack { get; set; } = -1;
        public bool StopAtOnce { get; set; } = true;
        public ILog Log { get; set; }

        /// <inheritdoc/>
        protected override MarkovPolicy<EnhancementState> Copy() => new TieredFailstackPolicy(_item, _targetGrade)
        {
            StopAtOnce = StopAtOnce,
            Failstacks = Failstacks,
            TargetFailstack = TargetFailstack,
            _itemLoss = _itemLoss,
            _cronActions = _cronActions,
            _valkActions = _valkActions,
            _enhancementActions = _enhancementActions,
            _failstackActions = _failstackActions,
            _restoreActions = _restoreActions,
            _storeActions = _storeActions,
        };

        /// <inheritdoc/>
        protected override IMarkovAction<EnhancementState>[] GetAllActions(EnhancementState state)
        {
            if (TargetFailstack > 0 && state.FailStack >= TargetFailstack)
                return new IMarkovAction<EnhancementState>[] { null };
            
            if (state.Items[0] - _itemLoss < 0 || (state.Items[_targetGrade] > 0 && StopAtOnce) || state.Items.Take(_targetGrade).Sum() < 1 + _itemLoss)
                return new IMarkovAction<EnhancementState>[] { null };

            var list = new List<IMarkovAction<EnhancementState>>();
            for (var grade = 1; grade <= _targetGrade; ++grade)
            {
                if (state.Items[grade - 1] > (grade == 1 ? _itemLoss : 0) )
                    list.Add(_enhancementActions[grade]);
            }

            for (var grade = 3; grade <= _targetGrade; ++grade)
            {
                if (state.Items[grade - 1] > 0 && _cronActions != null)
                    list.Add(_cronActions[grade - 3]);
            }

            if (state.NumberOfValks > 0 && state.FailStack > 0 && state.JustFailedGrade < 0 && state.Items[_targetGrade - 1] > 0)
            {
                //for (var i = 1; i <= state.NumberOfValks; i += 3)
                list.Add(_valkActions[state.NumberOfValks - 1]);
            }

            /*var toGrade = GetToGrade(state);
            if (state.Items[toGrade - 1] > (toGrade == 1 ? 1 : 0))
                list.Add(_enhancementActions[toGrade]);*/

            var minStoreFailstack = 15;

            for (var slot = 0; slot < Math.Min(_targetGrade - 1, 2); slot++)
            {
               if (state.FailStack > minStoreFailstack && state.StoredFailstacks[slot] == 0 && state.JustFailedGrade > 0)
                   list.Add(_storeActions[slot]);
               if (state.StoredFailstacks[slot] > 0 && state.FailStack == 0)
                   list.Add(_restoreActions[slot]);
            }

            var maxFailstack = MAX_FAILSTACK;
            var toGrade = GetToGrade(state);
            if (MaxGradeFailstacks.ContainsKey(toGrade - 1))
                maxFailstack = MaxGradeFailstacks[toGrade - 1] - 1;

            if (state.FailStack == 0)
            {
                for (var i = 3; i < Math.Min(maxFailstack, 20); i += 5)
                    list.Add(_failstackActions[i]);       
                for (var i = 21; i < Math.Min(maxFailstack, 30); i += 3)
                    list.Add(_failstackActions[i]);
                for (var i = 28; i < Math.Min(maxFailstack, MAX_FAILSTACK); i++)
                    list.Add(_failstackActions[i]);
                // list.AddRange(_failstackActions.Take(Math.Max(maxFailstack - state.FailStack, 0)));
            }

            return list.ToArray();
        }
        
        /// <inheritdoc/>
        protected override IMarkovAction<EnhancementState> GetAction(EnhancementState state)
        {
            
            if (TargetFailstack > 0 && state.FailStack >= TargetFailstack)
                return null;
            
            if (state.Items[0] - _itemLoss < 0)
                return null;
            
            if (state.Items[_targetGrade] > 0 && StopAtOnce)
                return null;

            var toGrade = GetToGrade(state);
            if (toGrade == 1 && state.Items[0] < 1 + _itemLoss)
                return null;

            var restoreSlot = ApplyRestoreFailstack(state, toGrade);
            if (restoreSlot >= 0)
            {
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
                return _failstackActions[failstack - 1];

            return _enhancementActions[toGrade];
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
            if (state.JustFailedGrade > 1 && state.Items[state.JustFailedGrade - 1] == 0 &&
                state.FailStack > Failstacks[state.JustFailedGrade - 1] && state.FailStack > 1)
                return _targetGrade - state.JustFailedGrade;
            return -1;
        }

        private int ApplyFailstack(EnhancementState state, int toGrade)
        {
            return Failstacks[toGrade - 1] - state.FailStack;
        }
    }
}