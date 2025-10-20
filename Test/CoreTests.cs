using System;
using System.Collections.Generic;
using System.Linq;
using qon;
using qon.Domains;
using qon.Functions.Propagators;
using qon.Layers.VariableLayers;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.Variables;

namespace qon.Tests
{
    public class DomainLayerTests
    {
        [Fact]
        public void SetDomain_ForConstantVariable_UsesEmptyDomain()
        {
            var variable = new QVariable<int>(value: 5, name: "constant");
            var domain = new DiscreteDomain<int>(new[] { 1, 2, 3 });

            var size = DomainLayer<int>.SetDomain(variable, domain);

            Assert.Equal(0, size);
            Assert.True(ReferenceEquals(DomainLayer<int>.TryCreate(variable).Domain, EmptyDomain<int>.Instance));
            Assert.Equal(ValueState.Constant, DomainLayer<int>.TryCreate(variable).State);
        }

        [Fact]
        public void AutoCollapse_WithSingleValueDomain_DefinesVariable()
        {
            var domain = new DiscreteDomain<int>(new[] { 2 });
            var variable = new QVariable<int>("single");
            DomainLayer<int>.SetDomain(variable, domain);

            var collapsed = ConstraintLayer<int>.AutoCollapse(variable);

            Assert.True(collapsed.HasValue);
            Assert.Equal(2, collapsed.Value);
            Assert.Equal(ValueState.Defined, DomainLayer<int>.TryCreate(variable).State);
            Assert.True(variable.Value.CheckValue(2));
            Assert.True(ReferenceEquals(DomainLayer<int>.TryCreate(variable).Domain, EmptyDomain<int>.Instance));
        }
    }

    public class MachineStateTests
    {
        [Fact]
        public void CurrentState_WhenDomainEmptyAndUncertain_IsUnsolvable()
        {
            var domain = new DiscreteDomain<int>(new[] { 1, 2 });
            var variable = new QVariable<int>("v");
            DomainLayer<int>.SetDomain(variable, domain);
            DomainLayer<int>.RemoveFromDomain(variable, 1);
            DomainLayer<int>.RemoveFromDomain(variable, 2);

            var state = new MachineState<int>(new[] { variable });

            Assert.Equal(SolutionState.Unsolvable, state.CurrentState);
        }

        [Fact]
        public void AutoCollapse_ReturnsNumberOfCollapsedVariables()
        {
            var first = new QVariable<int>("first");
            DomainLayer<int>.SetDomain(first, new DiscreteDomain<int>(new[] { 5 }));
            var second = new QVariable<int>("second");
            DomainLayer<int>.SetDomain(second, new DiscreteDomain<int>(new[] { 3, 4 }));

            var state = new MachineState<int>(new[] { first, second });

            var changes = state.AutoCollapse();

            Assert.Equal(1, changes);
            Assert.Equal(ValueState.Defined, DomainLayer<int>.TryCreate(first).State);
            Assert.Equal(ValueState.Uncertain, DomainLayer<int>.TryCreate(second).State);
        }
    }

    public class DomainHelperTests
    {
        [Fact]
        public void DomainIntersection_DiscreteAndNumerical_ReturnsTheirOverlap()
        {
            var discrete = new DiscreteDomain<int>(new[] { 1, 2, 3, 5 });
            var numerical = new NumericalDomain<int>(new[] { new Interval<int>(2, 4) });

            var intersection = DomainHelper<int>.DomainIntersection(discrete, numerical);

            var result = Assert.IsType<DiscreteDomain<int>>(intersection);
            Assert.Equal(new[] { 2, 3 }, result.Domain.Keys.OrderBy(x => x));
        }

        [Fact]
        public void DomainIntersectionWithHashSet_ForNumericalDomain_CollapsesToMatchingValues()
        {
            var numerical = new NumericalDomain<int>(new[] { new Interval<int>(0, 5) });
            var variable = new QVariable<int>("num");
            DomainLayer<int>.SetDomain(variable, numerical);
            var filter = Propagators.DomainIntersection(new HashSet<int> { 2, 9 });

            var result = filter.ApplyTo(new[] { variable });

            Assert.Equal(PropagationOutcome.Converged, result.Failed);
            Assert.Equal(ValueState.Defined, DomainLayer<int>.TryCreate(variable).State);
            Assert.True(variable.Value.CheckValue(2));
            Assert.True(ReferenceEquals(DomainLayer<int>.TryCreate(variable).Domain, EmptyDomain<int>.Instance));
        }
    }

    public class PropagatorsTests
    {
        [Fact]
        public void AllDistinct_WithDuplicateDecisions_IsConflict()
        {
            var domain = new DiscreteDomain<int>(new[] { 1, 2 });
            var first = new QVariable<int>("first");
            DomainLayer<int>.SetDomain(first, domain);
            var second = new QVariable<int>("second");
            DomainLayer<int>.SetDomain(second, domain);
            var third = new QVariable<int>("third");
            DomainLayer<int>.SetDomain(third, domain);

            ConstraintLayer<int>.Collapse(first, 1, isConstant: true);
            ConstraintLayer<int>.Collapse(second, 1, isConstant: true);

            var result = Propagators.AllDistinct<int>().ApplyTo(new[] { first, second, third });

            Assert.Equal(PropagationOutcome.Conflict, result.Failed);
            Assert.Equal(0, result.ChangesAmount);
        }

        [Fact]
        public void AllDistinct_RemovesAssignedValuesFromOpenVariables()
        {
            var domain = new DiscreteDomain<int>(new[] { 1, 2 });
            var decided = new QVariable<int>("decided");
            DomainLayer<int>.SetDomain(decided, domain);
            var open = new QVariable<int>("open");
            DomainLayer<int>.SetDomain(open, new DiscreteDomain<int>(new[] { 1, 2 }));

            ConstraintLayer<int>.Collapse(decided, 1);

            var result = Propagators.AllDistinct<int>().ApplyTo(new[] { decided, open });

            Assert.Equal(PropagationOutcome.Converged, result.Failed);
            Assert.Equal(ValueState.Defined, DomainLayer<int>.TryCreate(open).State);
            Assert.True(open.Value.CheckValue(2));
            Assert.True(ReferenceEquals(DomainLayer<int>.TryCreate(open).Domain, EmptyDomain<int>.Instance));
            Assert.Equal(1, result.ChangesAmount);
        }
    }

    public class WFCMachineTests
    {
        [Fact]
        public void CreateEuclideanSpace_BuildsIndexedGrid()
        {
            var parameter = new QMachineParameter<int>
            {
                Constraints = new RuleHandler<int>(),
                Random = new Random(0)
            };
            var machine = new WFCMachine<int>(parameter);
            var domain = new DiscreteDomain<int>(new[] { 1, 2 });

            machine.CreateEuclideanSpace((2, 1, 1), domain);

            Assert.Equal(FieldType.Euclidean, machine.FieldType);
            Assert.Equal(MachineStateType.Prepared, machine.StateType);
            Assert.Equal(2, machine.State.Field.Length);

            var grid = EuclideanStateLayer<int>.With(machine.State);
            var first = grid[(0, 0, 0)];
            var second = grid[(1, 0, 0)];

            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.Null(grid[(-1, 0, 0)]);
            Assert.Null(grid[(2, 0, 0)]);

            Assert.Equal("0x0x0", first!.Name);
            Assert.Equal("1x0x0", second!.Name);

            DomainLayer<int>.RemoveFromDomain(first!, 1);
            Assert.True(DomainLayer<int>.TryCreate(second!).Domain.ContainsValue(1));
        }
    }
}
