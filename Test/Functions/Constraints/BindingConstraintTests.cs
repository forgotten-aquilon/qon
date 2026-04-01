using qon;
using qon.Machines;
using qon.QSL;

namespace qon.Tests.Functions.Constraints
{
    public class BindingConstraintTests
    {
        [Fact]
        public void BindWithVariableListChecksAssignedValues()
        {
            QMachine<int> machine = QMachine<int>.Create();

            var a = machine.Q().WithValue(1);
            var b = machine.Q().WithValue(2);
            var c = machine.Q().WithValue(3);

            var constraint = QSL.Constraints.CreateConstraint<int>()
                .Bind([a, b, c], values => values[0] + values[1] == values[2])
                .Build();

            var result = constraint.Execute(machine.State.Field);

            Assert.False(result.Failed);
        }

        [Fact]
        public void BindWithManyArgumentsUsesTypedOverload()
        {
            QMachine<int> machine = QMachine<int>.Create();

            var a = machine.Q().WithValue(1);
            var b = machine.Q().WithValue(2);
            var c = machine.Q().WithValue(3);
            var d = machine.Q().WithValue(4);
            var e = machine.Q().WithValue(5);
            var f = machine.Q().WithValue(6);
            var g = machine.Q().WithValue(7);
            var h = machine.Q().WithValue(8);

            var constraint = QSL.Constraints.CreateConstraint<int>()
                .Bind(a, b, c, d, e, f, g, h, (v1, v2, v3, v4, v5, v6, v7, v8) => v1 + v2 + v3 + v4 + v5 + v6 + v7 == v8 + 20)
                .Build();

            var result = constraint.Execute(machine.State.Field);

            Assert.False(result.Failed);
        }

        [Fact]
        public void BindSkipsValidationUntilAllVariablesHaveValues()
        {
            QMachine<int> machine = QMachine<int>.Create();

            var a = machine.Q().WithValue(10);
            var b = machine.Q();

            var constraint = QSL.Constraints.CreateConstraint<int>()
                .Bind(a, b, (left, right) => left > right)
                .Build();

            var result = constraint.Execute(machine.State.Field);

            Assert.False(result.Failed);
        }
    }
}
