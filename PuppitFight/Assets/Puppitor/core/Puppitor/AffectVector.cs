using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Puppitor
{
    /// <summary>
    ///     Wrapper class for the affect vector
    /// </summary>
    public class AffectVector : IEnumerable<KeyValuePair<string, double>>
    {
        public double this[string key]
        {
            get => _affectVector[key];
            set => _affectVector[key] = value;
        }

        private readonly Dictionary<string, double> _affectVector;

        /// <summary>
        ///     Creates an affect vector from a dictionary of affect values
        /// </summary>
        /// <param name="affectVector"></param>
        public AffectVector(Dictionary<string, double> affectVector)
        {
            _affectVector = new Dictionary<string, double>(affectVector);
        }

        public AffectVector()
        {
            _affectVector = new Dictionary<string, double>();
        }

        public AffectVector(AffectVector affectVector)
        {
            _affectVector = new Dictionary<string, double>(affectVector._affectVector);
        }

        public IEnumerator<KeyValuePair<string, double>> GetEnumerator()
        {
            return _affectVector.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void ResetValues()
        {
            foreach (string affect in _affectVector.Keys.ToList())
            {
                _affectVector[affect] = 0;
            }
        }

        public bool TryAdd(string key, double value)
        {
            return _affectVector.TryAdd(key, value);
        }

        public void Remove(string key)
        {
            _affectVector.Remove(key);
        }

        /// <summary>
        /// </summary>
        /// <returns>List of the affect(s) with the highest strength of expression in the given affectVector</returns>
        public List<string> GetAllPrevailingAffects()
        {
            var possibleAffects = new List<string>();

            double max = _affectVector.Values.Max();
            foreach (KeyValuePair<string, double> affectEntry in _affectVector)
            {
                if (affectEntry.Value == max)
                {
                    possibleAffects.Add(affectEntry.Key);
                }
            }

            return possibleAffects;
        }

        public List<string> GetAllAffectNames()
        {
            return _affectVector.Keys.ToList();
        }

        /// <summary>
        ///     Helper function to do arithmetic with affect values. Modifies the affect value and clamps the result between
        ///     floorValue and ceilValue.
        /// </summary>
        public void UpdateAndClampValues(string affectName, double affectUpdateValue, double floorValue,
            double ceilValue)
        {
            double currentValue = _affectVector[affectName];
            double newValue = currentValue + affectUpdateValue;
            _affectVector[affectName] = Math.Clamp(newValue, floorValue, ceilValue);
        }

        /// <summary>
        ///     evaluates a given affectVector based on the difference in values between the goalEmotion and the highest valued
        ///     affects
        /// </summary>
        public double EvaluateAffectVector(
            string currentAffect,
            string goalEmotion)
        {
            double score = 0;
            double goalEmotionValue = _affectVector[goalEmotion];

            List<string> maxAffects = GetAllPrevailingAffects();

            if (currentAffect.CompareTo(goalEmotion) == 0)
            {
                score += 1;
            }
            else if (maxAffects.Count > 1 && maxAffects.Contains(goalEmotion) &&
                     currentAffect.CompareTo(goalEmotion) != 0)
            {
                score -= 1;
            }
            else
            {
                foreach (KeyValuePair<string, double> affectEntry in _affectVector)
                {
                    if (affectEntry.Key.CompareTo(goalEmotion) != 0)
                    {
                        score += goalEmotionValue - _affectVector[affectEntry.Key];
                    }
                }
            }

            return score;
        }

        public double DotProduct(AffectVector other)
        {
            double dotProduct = 0;
            foreach (string affect in _affectVector.Keys)
            {
                dotProduct += _affectVector[affect] * other[affect];
            }

            return dotProduct;
        }

        public void Normalize()
        {
            double sum = _affectVector.Values.Sum();

            if (sum == 0)
            {
                return;
            }

            var keys = new List<string>(_affectVector.Keys);

            foreach (string affect in keys)
            {
                _affectVector[affect] /= sum;
            }
        }
    }
}