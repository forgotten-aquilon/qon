using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Variables;
using qon.Variables.Domains;

namespace qon.Machines
{
    //TODO: integrate into the QSL
    public class MachineStateQuery<TQ> : IEnumerable<QVariable<TQ>> where TQ : notnull
    {
        private readonly IEnumerable<QVariable<TQ>> _query;

        internal MachineStateQuery(IEnumerable<QVariable<TQ>> source)
        {
            _query = source;
        }

        public MachineStateQuery<TQ> Where(Func<QVariable<TQ>, bool> predicate)
        {
            return new MachineStateQuery<TQ>(_query.Where(predicate));
        }

        public MachineStateQuery<TQ> WithProperty(string name, object value)
        {
            return new MachineStateQuery<TQ>(_query.Where(variable =>
                Equals(variable.GetNullOrValueProperty(name), value)));
        }

        public MachineStateQuery<TQ> WhereProperty(string name, Func<object?, bool> predicate)
        {
            return new MachineStateQuery<TQ>(_query.Where(variable =>
            {
                variable.TryGetProperty(name, out var property);
                return predicate(property);
            }));
        }

        public MachineStateQuery<TQ> WithLayer<TLayer>(Func<TLayer, bool> predicate)
        {
            return new MachineStateQuery<TQ>(_query.Where(variable =>
                variable.LayerManager.TryGetLayer<TLayer>(out var layer) && predicate(layer)));
        }

        public MachineStateQuery<TQ> WhereLayer<TLayer, TResult>(Func<TLayer, TResult> selector, Func<TResult, bool> predicate)
        {
            return new MachineStateQuery<TQ>(_query.Where(variable =>
            {
                if (!variable.LayerManager.TryGetLayer<TLayer>(out var layer))
                {
                    return false;
                }

                var result = selector(layer);
                return predicate(result);
            }));
        }

        public MachineStateQuery<TQ> WhereState(ValueState state)
        {
            return new MachineStateQuery<TQ>(_query.Where(variable => variable.State == state));
        }

        public MachineStateQuery<TQ> WhereStates(params ValueState[] states)
        {
            var stateSet = new HashSet<ValueState>(states);

            return new MachineStateQuery<TQ>(_query.Where(variable =>
                stateSet.Contains(variable.State)));
        }

        public MachineStateQuery<TQ> WhereDomain(Func<IDomain<TQ>, bool> predicate)
        {
            return new MachineStateQuery<TQ>(_query.Where(variable =>
                DomainLayer<TQ>.With(variable).MatchesDomain(predicate)));
        }

        public MachineStateQuery<TQ> WhereDomainSize(Func<int, bool> condition)
        {
            return new MachineStateQuery<TQ>(_query.Where(variable =>
                DomainLayer<TQ>.With(variable).DomainSizeSatisfies(condition)));
        }

        public MachineStateQuery<TQ> OrderByEntropy(bool ascending = true)
        {
            return new MachineStateQuery<TQ>(ascending
                ? _query.OrderBy(variable => DomainLayer<TQ>.With(variable).Entropy)
                : _query.OrderByDescending(variable => DomainLayer<TQ>.With(variable).Entropy));
        }

        public IEnumerable<QVariable<TQ>> ToEnumerable()
        {
            return _query;
        }

        public SearchResult<TQ> ToSearchResult()
        {
            return new SearchResult<TQ>(_query);
        }

        public QVariable<TQ>? FirstOrDefault()
        {
            return _query.FirstOrDefault();
        }

        public QVariable<TQ>? SingleOrDefault()
        {
            return _query.SingleOrDefault();
        }

        public void CollapseAll(TQ value, bool constant = false)
        {
            foreach (var variable in _query)
            {
                ConstraintLayer<TQ>.Collapse(variable, value, constant);
            }
        }

        public IEnumerator<QVariable<TQ>> GetEnumerator()
        {
            return _query.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
