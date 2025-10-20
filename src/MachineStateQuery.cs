using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using qon.Domains;
using qon.Exceptions;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;

namespace qon
{
    public class MachineStateQuery<T> : IEnumerable<QVariable<T>>
    {
        private readonly IEnumerable<QVariable<T>> _query;

        internal MachineStateQuery(IEnumerable<QVariable<T>> source)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(source, nameof(source));
            _query = source;
        }

        public MachineStateQuery<T> Where(Func<QVariable<T>, bool> predicate)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(predicate, nameof(predicate));
            return new MachineStateQuery<T>(_query.Where(predicate));
        }

        public MachineStateQuery<T> WithProperty(string name, object value)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(name, nameof(name));
            return new MachineStateQuery<T>(_query.Where(variable =>
                Equals(variable.GetNullOrValueProperty(name), value)));
        }

        public MachineStateQuery<T> WhereProperty(string name, Func<object?, bool> predicate)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(name, nameof(name));
            ExceptionHelper.ThrowIfArgumentIsNull(predicate, nameof(predicate));

            return new MachineStateQuery<T>(_query.Where(variable =>
            {
                variable.TryGetProperty(name, out var property);
                return predicate(property);
            }));
        }

        public MachineStateQuery<T> WithLayer<TLayer>(Func<TLayer, bool> predicate)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(predicate, nameof(predicate));

            return new MachineStateQuery<T>(_query.Where(variable =>
                variable.Layers.TryGetLayer<TLayer>(out var layer) && predicate(layer)));
        }

        public MachineStateQuery<T> WhereLayer<TLayer, TResult>(Func<TLayer, TResult> selector, Func<TResult, bool> predicate)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(selector, nameof(selector));
            ExceptionHelper.ThrowIfArgumentIsNull(predicate, nameof(predicate));

            return new MachineStateQuery<T>(_query.Where(variable =>
            {
                if (!variable.Layers.TryGetLayer<TLayer>(out var layer))
                {
                    return false;
                }

                var result = selector(layer);
                return predicate(result);
            }));
        }

        public MachineStateQuery<T> WhereState(ValueState state)
        {
            return new MachineStateQuery<T>(_query.Where(variable => variable.State == state));
        }

        public MachineStateQuery<T> WhereStates(params ValueState[] states)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(states, nameof(states));

            var stateSet = new HashSet<ValueState>(states);

            return new MachineStateQuery<T>(_query.Where(variable =>
                stateSet.Contains(variable.State)));
        }

        public MachineStateQuery<T> WhereDomain(Func<IDomain<T>, bool> predicate)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(predicate, nameof(predicate));

            return new MachineStateQuery<T>(_query.Where(variable =>
                DomainLayer<T>.With(variable).MatchesDomain(predicate)));
        }

        public MachineStateQuery<T> WhereDomainSize(Func<int, bool> condition)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(condition, nameof(condition));

            return new MachineStateQuery<T>(_query.Where(variable =>
                DomainLayer<T>.With(variable).DomainSizeSatisfies(condition)));
        }

        public MachineStateQuery<T> OrderByEntropy(bool ascending = true)
        {
            return new MachineStateQuery<T>(ascending
                ? _query.OrderBy(variable => DomainLayer<T>.With(variable).Entropy)
                : _query.OrderByDescending(variable => DomainLayer<T>.With(variable).Entropy));
        }

        public IEnumerable<QVariable<T>> ToEnumerable()
        {
            return _query;
        }

        public SearchResult<T> ToSearchResult()
        {
            return new SearchResult<T>(_query);
        }

        public QVariable<T>? FirstOrDefault()
        {
            return _query.FirstOrDefault();
        }

        public QVariable<T>? SingleOrDefault()
        {
            return _query.SingleOrDefault();
        }

        public void CollapseAll(T value, bool constant = false)
        {
            foreach (var variable in _query)
            {
                ConstraintLayer<T>.Collapse(variable, value, constant);
            }
        }

        public IEnumerator<QVariable<T>> GetEnumerator()
        {
            return _query.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
