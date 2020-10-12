using System;
using System.Diagnostics;
using System.Linq;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement.Stochastics
{
    public class EnhancementState : IMarkovState, IEquatable<EnhancementState>
    {
        private int[] _items;
        private int[] _storedFailstacks;
        
        public EnhancementState(int failStack = 0)
        {
            _items = new[] { int.MaxValue, 0, 0, 0, 0, 0 };
            _storedFailstacks = new[] { 0, 0, 0, 0 };
            NumberOfAttempts = 0;
            FailStack = failStack;
            JustFailedGrade = -1;
        }

        public EnhancementState(ref int[] items, ref int[] storedFailstacks)
        {
            _items = new int[6];
            _storedFailstacks = new int[4];
            items.CopyTo(_items, 0);
            storedFailstacks.CopyTo(_storedFailstacks, 0);
            NumberOfAttempts = 0;
            FailStack = 0;
            JustFailedGrade = -1;
        }

        public int[] StoredFailstacks
        {
            get => _storedFailstacks;
            set => _storedFailstacks = value;
        }

        public int[] Items
        {
            get => _items;
            set => _items = value;
        }
        
        public int NumberOfAttempts { get; set; }
        public int FailStack { get; set; }
        public int JustFailedGrade { get; set; }
        public int NumberOfValks { get; set; }
        
        /// <inheritdoc/>
        public override string ToString()
        {
            var str = JustFailedGrade > 0 ? $"[{JustFailedGrade}]" : string.Empty; 
            return str + $"({string.Join(','.ToString(), Items.Select(i => i > 10000 ? "-" : i.ToString()))}) |{FailStack}| [{string.Join(','.ToString(), StoredFailstacks)}]";
        }

        public EnhancementState Clone(Action<EnhancementState> action = null)
        {
            var state = new EnhancementState(ref _items, ref _storedFailstacks)
            {
                JustFailedGrade = JustFailedGrade, 
                NumberOfAttempts = NumberOfAttempts,
                FailStack = FailStack,
                NumberOfValks = NumberOfValks,
            };

            action?.Invoke(state);

            return state;
        }

        /// <inheritdoc/>
        public bool Equals(EnhancementState other)
        {
            if (ReferenceEquals(null, other)) 
                return false;
            if (ReferenceEquals(this, other)) 
                return true;

            var b = true;
            b &= Items.Length == other.Items.Length;
            if (!b)
                return false;
            
            for (var i = 0; i < Items.Length; ++i)
                b &= Items[i] == other.Items[i];

            b &= StoredFailstacks[0] == other.StoredFailstacks[0];
            b &= StoredFailstacks[1] == other.StoredFailstacks[1];
            b &= StoredFailstacks[2] == other.StoredFailstacks[2];
            b &= StoredFailstacks[3] == other.StoredFailstacks[3];
            
            return b && NumberOfAttempts == other.NumberOfAttempts && FailStack == other.FailStack && JustFailedGrade == other.JustFailedGrade && NumberOfValks == other.NumberOfValks;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((EnhancementState)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)2166136261;
                foreach (var i in Items)
                    hashCode = (hashCode * 16777619) ^ i;
                foreach (var i in StoredFailstacks)
                    hashCode = (hashCode * 16777619) ^ i;
                hashCode = (hashCode * 16777619) ^ NumberOfAttempts;
                hashCode = (hashCode * 16777619) ^ FailStack;
                hashCode = (hashCode * 16777619) ^ JustFailedGrade;
                hashCode = (hashCode * 16777619) ^ NumberOfValks;
                return hashCode;
            }
        }
    }
}