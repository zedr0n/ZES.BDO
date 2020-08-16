// #define USE_THIRD_PARTY_RANDOM_GENERATORS

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#if USE_THIRD_PARTY_RANDOM_GENERATORS
using CenterSpace.NMath.Core;
using Extreme.Mathematics;
using Extreme.Mathematics.Random;
#endif

namespace BDO.Enhancement
{
    public class RandomGenerator
    {
        private static readonly ConcurrentDictionary<string, IEnumerable<double>> _sequences = new ConcurrentDictionary<string, IEnumerable<double>>();
        private static readonly ConcurrentDictionary<string, int> _indices = new ConcurrentDictionary<string, int>();
        private static IEnumerator<List<double>> _defaultEnumerator;
        private static int _index;

        private static bool UseAntithetic => true;
#if USE_THIRD_PARTY_RANDOM_GENERATORS
        private static bool UseExtreme => false;
        private static bool UseQuasiRandom => true;
        private static IEnumerator<Vector<double>> _enumerator;
        private static DoubleMatrix _generatedRandoms;
#endif
        
        public static int Dimension => 500;
        private static int _nPaths = 0;
        
        public static void Initialise(int numberOfPaths)
        {
            _nPaths = numberOfPaths;
#if USE_THIRD_PARTY_RANDOM_GENERATORS
            if (UseQuasiRandom)
            {
                if (UseExtreme)
                {
                    _enumerator = QuasiRandom.HaltonSequence(Dimension, numberOfPaths).GetEnumerator();
                }
                else
                {
                    var generator = new SobolQuasiRandomGenerator(Dimension);
                    _generatedRandoms = generator.Next(new DoubleRandomUniformDistribution(), numberOfPaths);

                }
                
                return;
            }
#endif
            var random = new Random();
            var useAntithetic = UseAntithetic && _nPaths % 2 == 0;
            var nPaths = useAntithetic ? _nPaths / 2 : _nPaths;
            var defaultRandoms = new List<List<double>>();
            for (var i = 0; i < nPaths; ++i)
            {
                var list = new List<double>(); 
                for (var j = 0; j < Dimension; ++j)
                    list.Add(random.NextDouble());
                
                defaultRandoms.Add(list);
                if (useAntithetic)
                    defaultRandoms.Add(list.Select(x => 1.0 - x).ToList());
            }

            _defaultEnumerator = defaultRandoms.GetEnumerator();
        }

        public static IEnumerable<double> Generate(string id)
        {
            return _sequences.GetOrAdd(id, s =>
            {
                _defaultEnumerator.MoveNext();
                return _defaultEnumerator.Current;
            });
        }
        
#if USE_THIRD_PARTY_RANDOM_GENERATORS
        public static IEnumerable<double> Generate(string id)
        {
            var index = _indices.GetOrAdd(id, s =>
            {
                var i = _index;
                Interlocked.Increment(ref _index);
                return i;
            });
            return _sequences.GetOrAdd(id, s =>
            {
                if (UseExtreme && UseQuasiRandom)
                {
                    _enumerator.MoveNext();
                    return _enumerator.Current;
                }
                else if (UseQuasiRandom)
                {
                    return _generatedRandoms.Col(index).ToList();
                }
                _defaultEnumerator.MoveNext();
                return _defaultEnumerator.Current;
            });
        }
#endif
    }
}