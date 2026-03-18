using System;
using qon.Functions;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Variables;

namespace qon
{
    public class QSLConstraintsBuilder<TQ> where TQ : notnull
    {
        private QPredicate<TQ>? _guard;
        private Filter<TQ>? _grouping;
        private QPredicate<TQ>? _selector;
        private Func<QVariable<TQ>, QPredicate<TQ>>? _neighbourAggregation;
        private Func<QVariable<TQ>, Result>? _neighbourConstraint;
        private Propagator<TQ>? _propagator;
        private Func<QVariable<TQ>[], Result>? _customExecutor;

        public QSLConstraintsBuilder<TQ> When(QPredicate<TQ> guard)
        {
            _guard = guard;
            return this;
        }

        public QSLConstraintsBuilder<TQ> GroupBy(Filter<TQ> grouping)
        {
            _grouping = grouping;
            return this;
        }

        public QSLConstraintsBuilder<TQ> Select(QPredicate<TQ> selector)
        {
            _selector = selector;
            return this;
        }

        public QSLConstraintsBuilder<TQ> Where(Func<QVariable<TQ>, QPredicate<TQ>> aggregationFactory)
        {
            _neighbourAggregation = aggregationFactory;
            return this;
        }

        public QSLConstraintsBuilder<TQ> Where(Func<QVariable<TQ>, Result> neighbourConstraint)
        {
            _neighbourConstraint = neighbourConstraint;
            return this;
        }

        public QSLConstraintsBuilder<TQ> Propagate(Propagator<TQ> propagator)
        {
            _propagator = propagator;
            return this;
        }

        //TODO: Rename
        public QSLConstraintsBuilder<TQ> Execute(Func<QVariable<TQ>[], Result> executor)   
        {
            _customExecutor = executor;
            return this;
        }

        public IPreparation<TQ> Build()
        {
            if (_customExecutor is not null)
            {
                return new ConstraintBuilder<TQ>(_customExecutor);
            }

            if (_grouping is not null)
            {
                EnsurePropagatorProvided();
                return new Constraint<TQ>(_grouping, _propagator!);
            }

            if (_selector is not null)
            {
                EnsurePropagatorProvided();
                return new Constraint<TQ>(_selector, _propagator!);
            }

            if (_guard is not null && _neighbourConstraint is not null)
            {
                return new RelativeConstraintBuilder<TQ>(_guard, _neighbourConstraint);
            }

            if (_guard is not null && _neighbourAggregation is not null)
            {
                EnsurePropagatorProvided();
                return new RelativeConstraint<TQ>(_guard, _propagator!, _neighbourAggregation);
            }

            throw new InvalidOperationException("Insufficient data to build constraint. Specify grouping/selecting or guard with neighbourhood details.");
        }

        private void EnsurePropagatorProvided()
        {
            if (_propagator is null)
            {
                throw new InvalidOperationException("Propagator should be provided before building this constraint.");
            }
        }
    }
}