using System;
using System.Collections.Generic;
using System.Linq;
using qon.Exceptions;
using qon.Functions;
using qon.Functions.Constraints;
using qon.Functions.Filters;
using qon.Functions.Propagators;
using qon.Variables;

namespace qon.QSL
{
    public class ConstraintsBuilder<TQ> where TQ : notnull
    {
        private QPredicate<TQ>? _guard;
        private Filter<TQ>? _grouping;
        private QPredicate<TQ>? _selector;
        private Func<QObject<TQ>, QPredicate<TQ>>? _neighbourAggregation;
        private Func<QObject<TQ>, Result>? _neighbourConstraint;
        private Propagator<TQ>? _propagator;
        private Func<QObject<TQ>[], Result>? _customExecutor;
        private BindingConstraint<TQ>? _bindingConstraint;
        private QLink<TQ>[]? _qLinks;

        public ConstraintsBuilder<TQ> When(QPredicate<TQ> guard)
        {
            _guard = guard;
            return this;
        }

        public ConstraintsBuilder<TQ> GroupBy(Filter<TQ> grouping)
        {
            _grouping = grouping;
            return this;
        }

        public ConstraintsBuilder<TQ> Select(QPredicate<TQ> selector)
        {
            _selector = selector;
            return this;
        }

        public ConstraintsBuilder<TQ> Select(params QLink<TQ>[] links)
        {
            _qLinks = links;
            return this;
        }

        public ConstraintsBuilder<TQ> Where(Func<QObject<TQ>, QPredicate<TQ>> aggregationFactory)
        {
            _neighbourAggregation = aggregationFactory;
            return this;
        }

        public ConstraintsBuilder<TQ> Where(Func<QObject<TQ>, Result> neighbourConstraint)
        {
            _neighbourConstraint = neighbourConstraint;
            return this;
        }

        public ConstraintsBuilder<TQ> Propagate(Propagator<TQ> propagator)
        {
            _propagator = propagator;
            return this;
        }

        public ConstraintsBuilder<TQ> Constraint(Func<QObject<TQ>[], Result> executor)
        {
            _customExecutor = executor;
            return this;
        }

        //TODO: Add QObject Function binding
        public ConstraintsBuilder<TQ> Bind(IReadOnlyList<QLink<TQ>> qLinks, Func<IReadOnlyList<TQ>, bool> bindingFunction)
        {
            _bindingConstraint = new BindingConstraint<TQ>(qLinks, bindingFunction);
            return this;
        }
        
        public ConstraintsBuilder<TQ> Bind(QLink<TQ> a, Func<TQ, bool> bindingFunction)
        {
            return Bind(new[] { a }, values => bindingFunction(values[0]));
        }

        public ConstraintsBuilder<TQ> Bind(QLink<TQ> a, QLink<TQ> b, Func<TQ, TQ, bool> bindingFunction)
        {
            return Bind(new[] { a, b }, values => bindingFunction(values[0], values[1]));
        }

        public ConstraintsBuilder<TQ> Bind(QLink<TQ> a, QLink<TQ> b, QLink<TQ> c, Func<TQ, TQ, TQ, bool> bindingFunction)
        {
            return Bind(new[] { a, b, c }, values => bindingFunction(values[0], values[1], values[2]));
        }

        public ConstraintsBuilder<TQ> Bind(QLink<TQ> a, QLink<TQ> b, QLink<TQ> c, QLink<TQ> d, Func<TQ, TQ, TQ, TQ, bool> bindingFunction)
        {
            return Bind(new[] { a, b, c, d }, values => bindingFunction(values[0], values[1], values[2], values[3]));
        }

        public ConstraintsBuilder<TQ> Bind(QLink<TQ> a, QLink<TQ> b, QLink<TQ> c, QLink<TQ> d, QLink<TQ> e, Func<TQ, TQ, TQ, TQ, TQ, bool> bindingFunction)
        {
            return Bind(new[] { a, b, c, d, e }, values => bindingFunction(values[0], values[1], values[2], values[3], values[4]));
        }

        public ConstraintsBuilder<TQ> Bind(QLink<TQ> a, QLink<TQ> b, QLink<TQ> c, QLink<TQ> d, QLink<TQ> e, QLink<TQ> f, Func<TQ, TQ, TQ, TQ, TQ, TQ, bool> bindingFunction)
        {
            return Bind(new[] { a, b, c, d, e, f }, values => bindingFunction(values[0], values[1], values[2], values[3], values[4], values[5]));
        }

        public ConstraintsBuilder<TQ> Bind(QLink<TQ> a, QLink<TQ> b, QLink<TQ> c, QLink<TQ> d, QLink<TQ> e, QLink<TQ> f, QLink<TQ> g, Func<TQ, TQ, TQ, TQ, TQ, TQ, TQ, bool> bindingFunction)
        {
            return Bind(new[] { a, b, c, d, e, f, g }, values => bindingFunction(values[0], values[1], values[2], values[3], values[4], values[5], values[6]));
        }

        public ConstraintsBuilder<TQ> Bind(QLink<TQ> a, QLink<TQ> b, QLink<TQ> c, QLink<TQ> d, QLink<TQ> e, QLink<TQ> f, QLink<TQ> g, QLink<TQ> h, Func<TQ, TQ, TQ, TQ, TQ, TQ, TQ, TQ, bool> bindingFunction)
        {
            return Bind(new[] { a, b, c, d, e, f, g, h }, values => bindingFunction(values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7]));
        }

        public ConstraintsBuilder<TQ> Bind(QLink<TQ> a, QLink<TQ> b, QLink<TQ> c, QLink<TQ> d, QLink<TQ> e, QLink<TQ> f, QLink<TQ> g, QLink<TQ> h, QLink<TQ> i, Func<TQ, TQ, TQ, TQ, TQ, TQ, TQ, TQ, TQ, bool> bindingFunction)
        {
            return Bind(new[] { a, b, c, d, e, f, g, h, i }, values => bindingFunction(values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8]));
        }

        public IPreparation<TQ> Build()
        {
            if (_customExecutor is not null)
            {
                return new CustomConstraint<TQ>(_customExecutor);
            }

            if (_bindingConstraint is not null)
            {
                return _bindingConstraint;
            }

            if (_grouping is not null)
            {
                ExceptionHelper.ThrowIfFieldIsNull(_propagator);
                return new GroupingConstraint<TQ>(_grouping, _propagator);
            }

            if (_selector is not null)
            {
                ExceptionHelper.ThrowIfFieldIsNull(_propagator);
                return new SelectingConstraint<TQ>(_selector, _propagator);
            }

            if (_qLinks is not null)
            {
                ExceptionHelper.ThrowIfFieldIsNull(_propagator);
                return new Constraint<TQ>(_qLinks, _propagator);
            }

            if (_guard is not null && _neighbourConstraint is not null)
            {
                return new RelativeConstraintBuilder<TQ>(_guard, _neighbourConstraint);
            }

            if (_guard is not null && _neighbourAggregation is not null)
            {
                ExceptionHelper.ThrowIfFieldIsNull(_propagator);
                return new RelativeConstraint<TQ>(_guard, _propagator, _neighbourAggregation);
            }

            throw new InternalLogicException("Insufficient data to build constraint. Specify grouping/selecting or guard with neighbourhood details.");
        }
    }
}
