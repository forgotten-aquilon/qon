using qon.Events;
using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Machines;
using qon.QSL;
using qon.Helpers;
using qon.Variables;
using qon.Variables.Domains;

namespace qon.Tests.VariableTests
{
    public class QObjectTests
    {
        [Fact]
        public void BasicEqualityTest()
        {
            QMachine<char> machine = new QMachine<char>(new QMachineParameter<char>());

            var lnk = machine.Q();

            var copy = lnk.Object.Copy();

            Assert.True(lnk.Object == copy);
        }

        [Fact]
        public void BasicInequalityTest()
        {
            QMachine<char> machine = new QMachine<char>(new QMachineParameter<char>());

            var lnk1 = machine.Q();

            var lnk2 = machine.Q();

            Assert.False(lnk1 == lnk2);
        }

        [Fact]
        public void PropertyEqualityTest()
        {
            QMachine<char> machine = QMachine<char>.Create();

            var lnk = machine.Q();

            QObject<char> original = lnk.Object
                .AddProperty("int", 12)
                .AddProperty("string", "some string")
                .AddProperty("double", 12.5)
                .AddProperty("bool", true);

            original.Machine = machine;

            var copy = original.Copy();

            Assert.True(original == copy);
        }

        [Fact]
        public void PropertyInequalityTest()
        {
            QMachine<char> machine = QMachine<char>.Create();

            var lnk = machine.Q();

            QObject<char> original = lnk.Object
                .AddProperty("int", 12)
                .AddProperty("string", "some string")
                .AddProperty("double", 12.5)
                .AddProperty("bool", true);

            original.Machine = machine;

            var copy = original.Copy();
            copy["bool"] = false;

            Assert.False(original == copy);
        }

        [Fact]
        public void CartesianLayerEqualityTest()
        {
            QMachine<char> machine = new QMachine<char>(new QMachineParameter<char>());

            machine.GenerateField((1, 1, 1));

            var original = machine.At(0, 0, 0);

            var copy = original.Copy();

            Assert.True(original == copy);
        }

        [Fact]
        public void CartesianLayerInequalityTest()
        {
            QMachine<char> machine = new QMachine<char>(new QMachineParameter<char>());
            machine.GenerateField((2, 1, 1));

            var original = machine.At(0, 0, 0);

            var nonCopy = machine.At(1, 0, 0);

            Assert.False(original == nonCopy);
        }

        [Fact]
        public void ValueChangedEventIsRaisedWhenValueChanges()
        {
            var obj = QObject<char>.Empty();
            ValueChangedEventArgs<Optional<char>>? capturedArgs = null;
            var raisedCount = 0;

            obj.ValueChanged += (_, args) =>
            {
                raisedCount++;
                capturedArgs = args;
            };

            obj.Value = Optional<char>.Of('x');

            Assert.Equal(1, raisedCount);
            Assert.NotNull(capturedArgs);
            Assert.False(capturedArgs.PreviousValue.HasValue);
            Assert.True(capturedArgs.NewValue.CheckValue('x'));
        }

        [Fact]
        public void ValueChangedEventIsNotRaisedWhenValueStaysTheSame()
        {
            var obj = QObject<char>.Empty();
            obj.Value = Optional<char>.Of('x');

            var raised = false;
            obj.ValueChanged += (_, _) => raised = true;

            obj.Value = Optional<char>.Of('x');

            Assert.False(raised);
        }
    }
}
