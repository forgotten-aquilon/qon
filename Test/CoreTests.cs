using System;
using System.Collections.Generic;
using System.Linq;
using qon;
using qon.Constraints.Filters;
using qon.Domains;
using qon.Variables;

namespace qon.Tests
{
    public class SuperpositionVariableTests
    {
        [Fact]
        public void SetDomain_ForConstantVariable_UsesEmptyDomain()
        {
            var variable = new SuperpositionVariable<int>(value: 5, name: "constant");
            var domain = new DiscreteDomain<int>(new[] { 1, 2, 3 });

            var size = variable.SetDomain(domain);

            Assert.Equal(0, size);
            Assert.True(ReferenceEquals(variable.Domain, EmptyDomain<int>.Instance));
            Assert.Equal(SuperpositionState.Constant, variable.State);
        }

        [Fact]
        public void AutoCollapse_WithSingleValueDomain_DefinesVariable()
        {
            var domain = new DiscreteDomain<int>(new[] { 2 });
            var variable = new SuperpositionVariable<int>(domain, name: "single");

            var collapsed = variable.AutoCollapse();

            Assert.True(collapsed.HasValue);
            Assert.Equal(2, collapsed.Value);
            Assert.Equal(SuperpositionState.Defined, variable.State);
            Assert.True(variable.Value.CheckValue(2));
            Assert.True(ReferenceEquals(variable.Domain, EmptyDomain<int>.Instance));
        }
    }

    public class MachineStateTests
    {
        [Fact]
        public void CurrentState_WhenDomainEmptyAndUncertain_IsUnsolvable()
        {
            var domain = new DiscreteDomain<int>(new[] { 1, 2 });
            var variable = new SuperpositionVariable<int>(domain, name: "v");
            variable.RemoveFromDomain(1);
            variable.RemoveFromDomain(2);

            var state = new MachineState<int>(new[] { variable });

            Assert.Equal(SolutionState.Unsolvable, state.CurrentState);
        }

        [Fact]
        public void AutoCollapse_ReturnsNumberOfCollapsedVariables()
        {
            var first = new SuperpositionVariable<int>(new DiscreteDomain<int>(new[] { 5 }), "first");
            var second = new SuperpositionVariable<int>(new DiscreteDomain<int>(new[] { 3, 4 }), "second");

            var state = new MachineState<int>(new[] { first, second });

            var changes = state.AutoCollapse();

            Assert.Equal(1, changes);
            Assert.Equal(SuperpositionState.Defined, first.State);
            Assert.Equal(SuperpositionState.Uncertain, second.State);
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
            var variable = new SuperpositionVariable<int>(numerical, name: "num");
            var filter = Filters.DomainIntersectionWithHashSet(new HashSet<int> { 2, 9 });

            var result = filter.ApplyTo(new[] { variable });

            Assert.Equal(PropagationOutcome.Converged, result.Outcome);
            Assert.Equal(SuperpositionState.Defined, variable.State);
            Assert.True(variable.Value.CheckValue(2));
            Assert.True(ReferenceEquals(variable.Domain, EmptyDomain<int>.Instance));
        }
    }

    public class FiltersTests
    {
        [Fact]
        public void AllDistinct_WithDuplicateDecisions_IsConflict()
        {
            var domain = new DiscreteDomain<int>(new[] { 1, 2 });
            var first = new SuperpositionVariable<int>(domain, name: "first");
            var second = new SuperpositionVariable<int>(domain, name: "second");
            var third = new SuperpositionVariable<int>(domain, name: "third");

            first.Collapse(1, isConstant: true);
            second.Collapse(1, isConstant: true);

            var result = Filters.AllDistinct<int>().ApplyTo(new[] { first, second, third });

            Assert.Equal(PropagationOutcome.Conflict, result.Outcome);
            Assert.Equal(0, result.ChangesAmount);
        }

        [Fact]
        public void AllDistinct_RemovesAssignedValuesFromOpenVariables()
        {
            var domain = new DiscreteDomain<int>(new[] { 1, 2 });
            var decided = new SuperpositionVariable<int>(domain, name: "decided");
            var open = new SuperpositionVariable<int>(new DiscreteDomain<int>(new[] { 1, 2 }), name: "open");

            decided.Collapse(1);

            var result = Filters.AllDistinct<int>().ApplyTo(new[] { decided, open });

            Assert.Equal(PropagationOutcome.Converged, result.Outcome);
            Assert.Equal(SuperpositionState.Defined, open.State);
            Assert.True(open.Value.CheckValue(2));
            Assert.True(ReferenceEquals(open.Domain, EmptyDomain<int>.Instance));
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
                GeneralRules = new RuleHandler<int>(),
                Random = new Random(0)
            };
            var machine = new WFCMachine<int>(parameter);
            var domain = new DiscreteDomain<int>(new[] { 1, 2 });

            machine.CreateEuclideanSpace((2, 1, 1), domain);

            Assert.Equal(FieldType.Euclidean, machine.FieldType);
            Assert.Equal(MachineStateType.Prepared, machine.StateType);
            Assert.Equal(2, machine.State.Field.Length);

            var first = machine[0, 0, 0];
            var second = machine[1, 0, 0];

            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.Null(machine[-1, 0, 0]);
            Assert.Null(machine[(2, 0, 0)]);

            Assert.Equal("0x0x0", first!.Name);
            Assert.Equal("1x0x0", second!.Name);

            first.RemoveFromDomain(1);
            Assert.True(second!.Domain.ContainsValue(1));
        }
    }
}
