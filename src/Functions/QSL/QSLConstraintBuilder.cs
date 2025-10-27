using System;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Variables;

namespace qon.Functions.QSL
{
    public class QSLConstraintBuilder<T>
    {
        private QPredicate<T>? _guard;
        private Filter<T>? _grouping;
        private QPredicate<T>? _selector;
        private Func<QVariable<T>, QPredicate<T>>? _neighbourAggregation;
        private Func<QVariable<T>, Result>? _neighbourConstraint;
        private Propagator<T>? _propagator;
        private Func<QVariable<T>[], Result>? _customExecutor;

        public QSLConstraintBuilder<T> When(QPredicate<T> guard)
        {
            _guard = guard;
            return this;
        }

        public QSLConstraintBuilder<T> GroupBy(Filter<T> grouping)
        {
            _grouping = grouping;
            return this;
        }

        public QSLConstraintBuilder<T> Select(QPredicate<T> selector)
        {
            _selector = selector;
            return this;
        }

        public QSLConstraintBuilder<T> Where(Func<QVariable<T>, QPredicate<T>> aggregationFactory)
        {
            _neighbourAggregation = aggregationFactory;
            return this;
        }

        public QSLConstraintBuilder<T> Where(Func<QVariable<T>, Result> neighbourConstraint)
        {
            _neighbourConstraint = neighbourConstraint;
            return this;
        }

        public QSLConstraintBuilder<T> Propagate(Propagator<T> propagator)
        {
            _propagator = propagator;
            return this;
        }

        public QSLConstraintBuilder<T> Execute(Func<QVariable<T>[], Result> executor)   
        {
            _customExecutor = executor;
            return this;
        }

        public IPreparation<T> Build()
        {
            if (_customExecutor is not null)
            {
                return new ConstraintBuilder<T>(_customExecutor);
            }

            if (_grouping is not null)
            {
                EnsurePropagatorProvided();
                return new Constraint<T>(_grouping, _propagator!);
            }

            if (_selector is not null)
            {
                EnsurePropagatorProvided();
                return new Constraint<T>(_selector, _propagator!);
            }

            if (_guard is not null && _neighbourConstraint is not null)
            {
                return new RelativeConstraintBuilder<T>(_guard, _neighbourConstraint);
            }

            if (_guard is not null && _neighbourAggregation is not null)
            {
                EnsurePropagatorProvided();
                return new RelativeConstraint<T>(_guard, _propagator!, _neighbourAggregation);
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