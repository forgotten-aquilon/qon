using qon.Machines;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace qon.Helpers
{
    public class Cache<T>
    {
        private T _value;

        private bool _hasChanged = false;

        public T Value
        {
            get
            {
                if (_hasChanged)
                {
                    _value = UpdateFunction();
                }

                return _value;
            }

            private set
            {
                _value = value;
                _hasChanged = false;
            }
        }

        public Func<T> UpdateFunction { get; private set; }

        public Cache(T value, Func<T> updateFunction)
        {
            _value = value;
            UpdateFunction = updateFunction;
        }

        public Cache(Func<T> updateFunction)
        {
            UpdateFunction = updateFunction;
            _value = updateFunction();
        }

        public void Changed()
        {
            _hasChanged = true;
        }

        public static implicit operator T(Cache<T> cache)
        {
            return cache.Value;
        }
    }
}
