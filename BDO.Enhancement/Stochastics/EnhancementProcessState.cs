using System;
using System.Diagnostics;
using System.Linq;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement.Stochastics
{
    [DebuggerDisplay("{DebuggerDisplay}, {NumberOfAttempts}, {FailStack}")]
    public class EnhancementState : IMarkovState, IEquatable<EnhancementState>
    {
        public EnhancementState(int failStack = 0)
        {
            Items = new[] { int.MaxValue, 0, 0, 0, 0 };
            NumberOfAttempts = 0;
            FailStack = failStack;
        }

        private string DebuggerDisplay
        {
            get
            {
                return $"[{Items.Aggregate(string.Empty, (s, i) => s + ( i >= 10000 ? "-" : i.ToString() ) + ",")}]";
            }
        }

        public int[] Items { get; set; }
        public int NumberOfAttempts { get; set; }
        public int FailStack { get; set; }

        public EnhancementState Clone(Action<EnhancementState> action = null)
        {
            var state = new EnhancementState();
            for (var grade = 0; grade < Items.Length; grade++)
                state.Items[grade] = Items[grade];

            state.NumberOfAttempts = NumberOfAttempts;
            state.FailStack = FailStack;
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
            for (var i = 0; i < Items.Length; ++i)
                b &= Items[i] == other.Items[i];
            
            return b && NumberOfAttempts == other.NumberOfAttempts && FailStack == other.FailStack;
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
                hashCode = (hashCode * 397) ^ NumberOfAttempts;
                hashCode = (hashCode * 397) ^ FailStack;
                return hashCode;
            }
        }
    }
}