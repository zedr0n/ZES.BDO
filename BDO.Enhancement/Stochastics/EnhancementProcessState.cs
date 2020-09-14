using System;
using System.Diagnostics;
using System.Linq;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement.Stochastics
{
    [DebuggerDisplay("{DebuggerDisplay}, {NumberOfAttempts}, {FailStack}")]
    public class EnhancementState : IMarkovState, IEquatable<EnhancementState>
    {
        private static int _itemsSize = sizeof(int) * 5;
        private static int _storedFailstacksSize = sizeof(int) * 2;

        private int[] _items;
        private int[] _storedFailstacks;
        
        public EnhancementState(int failStack = 0)
        {
            _items = new[] { int.MaxValue, 0, 0, 0, 0, 0, 0};
            _storedFailstacks = new[] { 0, 0, 0 };
            NumberOfAttempts = 0;
            FailStack = failStack;
            JustFailedGrade = -1;
        }

        public EnhancementState(ref int[] items, ref int[] storedFailstacks)
        {
            _items = new int[6];
            _storedFailstacks = new int[3];
            items.CopyTo(_items, 0);
            storedFailstacks.CopyTo(_storedFailstacks, 0);
            // Buffer.BlockCopy(items, 0, _items, 0, _itemsSize);
            // Buffer.BlockCopy(storedFailstacks, 0, _storedFailstacks, 0, _storedFailstacksSize);
            NumberOfAttempts = 0;
            FailStack = 0;
            JustFailedGrade = -1;
        }

        public string DebuggerDisplay
        {
            get
            {
                return $"[{Items.Aggregate(string.Empty, (s, i) => s + ( i >= 10000 ? "-" : i.ToString() ) + ",")}]";
            }
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

        public EnhancementState Clone(Action<EnhancementState> action = null)
        {
            var state = new EnhancementState(ref _items, ref _storedFailstacks)
            {
                JustFailedGrade = JustFailedGrade, 
                NumberOfAttempts = NumberOfAttempts,
                FailStack = FailStack,
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
            
            return b && NumberOfAttempts == other.NumberOfAttempts && FailStack == other.FailStack && JustFailedGrade == other.JustFailedGrade;
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
                var hashCode = 0;
                foreach (var i in Items)
                    hashCode = (hashCode * 397) ^ i.GetHashCode();
                hashCode = (hashCode * 397) ^ StoredFailstacks[0].GetHashCode();
                hashCode = (hashCode * 397) ^ StoredFailstacks[1].GetHashCode();
                hashCode = (hashCode * 397) ^ StoredFailstacks[2].GetHashCode();
                hashCode = (hashCode * 397) ^ NumberOfAttempts;
                hashCode = (hashCode * 397) ^ FailStack;
                hashCode = (hashCode * 397) ^ JustFailedGrade;
                return hashCode;
            }
        }
    }
}