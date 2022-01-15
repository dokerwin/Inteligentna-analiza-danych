using System;
using System.Collections.Generic;


namespace Lab01.Analysis {
    class Frequencies
    {
        private readonly Dictionary<Place, int> _freqs;

        public Frequencies(Place[] tags)
        {
            if (tags.Length > 0)
            {
                _freqs = new Dictionary<Place, int>();
                foreach (var tag in tags)
                {
                    _freqs.Add(tag, 0);
                }
            }
            else
            {
                throw new Exception("Expected tags to be non-empty array");
            }
        }

        public void Increment(Place tag)
        {
            if (_freqs.ContainsKey(tag))
            {
                _freqs[tag] += 1;
            }
            else
            {
                throw new Exception("Invalid tag");
            }
        }

        public Dictionary<Place, int> Data
        {
            get => _freqs;
        }

        private bool Same(Frequencies frequencies)
        {
            if (_freqs.Keys.Count != frequencies.Data.Keys.Count)
            {
                return false;
            }

            foreach (var f in frequencies.Data)
            {
                if (!_freqs.ContainsKey(f.Key))
                {
                    return false;
                }
            }

            return true;
        }

        public void ReduceWith(Frequencies frequencies)
        {
            if (Same(frequencies))
            {
                foreach (var f in frequencies.Data)
                {
                    _freqs[f.Key] += f.Value;
                }
            }
            else
            {
                throw new Exception("Expected 'frequencies' to have same tags");
            }
        }
    }
}
