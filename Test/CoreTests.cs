using System;
using System.Collections.Generic;
using System.Linq;
using qon;
using qon.Domains;
using qon.Functions.Propagators;
using qon.Variables;
using qon.Variables.Layers;

namespace qon.Tests
{
    public class SuperpositionLayerTests
    {
        [Fact]
        public void SetDomain_ForConstantVariable_UsesEmptyDomain()
        {
            var variable = new QVariable<int>(value: 5, name: "constant");
            var domain = new DiscreteDomain<int>(new[] { 1, 2, 3 });

            var size = SuperpositionLayer<int>.SetDomain(variable, domain);

            Assert.Equal(0, size);
            Assert.True(ReferenceEquals(SuperpositionLayer<int>.For(variable).Domain, EmptyDomain<int>.Instance));
            Assert.Equal(SuperpositionState.Constant, SuperpositionLayer<int>.For(variable).State);
        }

        [Fact]
        public void AutoCollapse_WithSingleValueDomain_DefinesVariable()
        {
            var domain = new DiscreteDomain<int>(new[] { 2 });
            var variable = new QVariable<int>("single");
            SuperpositionLayer<int>.SetDomain(variable, domain);

            var collapsed = SuperpositionLayer<int>.AutoCollapse(variable);

            Assert.True(collapsed.HasValue);
            Assert.Equal(2, collapsed.Value);
            Assert.Equal(SuperpositionState.Defined, SuperpositionLayer<int>.For(variable).State);
            Assert.True(variable.Value.CheckValue(2));
            Assert.True(ReferenceEquals(SuperpositionLayer<int>.For(variable).Domain, EmptyDomain<int>.Instance));
        }
    }

    public class MachineStateTests
    {
        [Fact]
        public void CurrentState_WhenDomainEmptyAndUncertain_IsUnsolvable()
        {
            var domain = new DiscreteDomain<int>(new[] { 1, 2 });
            var variable = new QVariable<int>("v");
            SuperpositionLayer<int>.SetDomain(variable, domain);
            SuperpositionLayer<int>.RemoveFromDomain(variable, 1);
            SuperpositionLayer<int>.RemoveFromDomain(variable, 2);

            var state = new MachineState<int>(new[] { variable });

            Assert.Equal(SolutionState.Unsolvable, state.CurrentState);
        }

        [Fact]
        public void AutoCollapse_ReturnsNumberOfCollapsedVariables()
        {
            var first = new QVariable<int>("first");
            SuperpositionLayer<int>.SetDomain(first, new DiscreteDomain<int>(new[] { 5 }));
            var second = new QVariable<int>("second");
            SuperpositionLayer<int>.SetDomain(second, new DiscreteDomain<int>(new[] { 3, 4 }));

            var state = new MachineState<int>(new[] { first, second });

            var changes = state.AutoCollapse();

            Assert.Equal(1, changes);
            Assert.Equal(SuperpositionState.Defined, SuperpositionLayer<int>.For(first).State);
            Assert.Equal(SuperpositionState.Uncertain, SuperpositionLayer<int>.For(second).State);
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
            SuperpositionLayer<int>.SetDomain(variable, numerical);
            var filter = Propagators.DomainIntersectionWithHashSet(new HashSet<int> { 2, 9 });

            var result = filter.ApplyTo(new[] { variable });

            Assert.Equal(PropagationOutcome.Converged, result.Failed);
            Assert.Equal(SuperpositionState.Defined, SuperpositionLayer<int>.For(variable).State);
            Assert.True(variable.Value.CheckValue(2));
            Assert.True(ReferenceEquals(SuperpositionLayer<int>.For(variable).Domain, EmptyDomain<int>.Instance));
        }
    }

    public class PropagatorsTests
    {
        [Fact]
        public void AllDistinct_WithDuplicateDecisions_IsConflict()
        {
            var domain = new DiscreteDomain<int>(new[] { 1, 2 });
            var first = new QVariable<int>("first");
            SuperpositionLayer<int>.SetDomain(first, domain);
            var second = new QVariable<int>("second");
            SuperpositionLayer<int>.SetDomain(second, domain);
            var third = new QVariable<int>("third");
            SuperpositionLayer<int>.SetDomain(third, domain);

            SuperpositionLayer<int>.Collapse(first, 1, isConstant: true);
            SuperpositionLayer<int>.Collapse(second, 1, isConstant: true);

            var result = Propagators.AllDistinct<int>().ApplyTo(new[] { first, second, third });

            Assert.Equal(PropagationOutcome.Conflict, result.Failed);
            Assert.Equal(0, result.ChangesAmount);
        }

        [Fact]
        public void AllDistinct_RemovesAssignedValuesFromOpenVariables()
        {
            var domain = new DiscreteDomain<int>(new[] { 1, 2 });
            var decided = new QVariable<int>("decided");
            SuperpositionLayer<int>.SetDomain(decided, domain);
            var open = new QVariable<int>("open");
            SuperpositionLayer<int>.SetDomain(open, new DiscreteDomain<int>(new[] { 1, 2 }));

            SuperpositionLayer<int>.Collapse(decided, 1);

            var result = Propagators.AllDistinct<int>().ApplyTo(new[] { decided, open });

            Assert.Equal(PropagationOutcome.Converged, result.Failed);
            Assert.Equal(SuperpositionState.Defined, SuperpositionLayer<int>.For(open).State);
            Assert.True(open.Value.CheckValue(2));
            Assert.True(ReferenceEquals(SuperpositionLayer<int>.For(open).Domain, EmptyDomain<int>.Instance));
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

            var first = machine[0, 0, 0];
            var second = machine[1, 0, 0];

            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.Null(machine[-1, 0, 0]);
            Assert.Null(machine[(2, 0, 0)]);

            Assert.Equal("0x0x0", first!.Name);
            Assert.Equal("1x0x0", second!.Name);

            SuperpositionLayer<int>.RemoveFromDomain(first!, 1);
            Assert.True(SuperpositionLayer<int>.For(second!).Domain.ContainsValue(1));
        }
    }
}
