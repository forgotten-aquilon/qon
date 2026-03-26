using qon.Layers.StateLayers;
using qon.Layers.VariableLayers;
using qon.Machines;
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

            QObject<char> original = QObject<char>.Empty();
            original.Machine = machine;
            var copy = original.Copy();

            Assert.True(original == copy);
        }

        [Fact]
        public void BasicInequalityTest()
        {
            QMachine<char> machine = new QMachine<char>(new QMachineParameter<char>());

            QObject<char> original = QObject<char>.Empty();
            original.Machine = machine;

            var newVariable = QObject<char>.Empty();
            newVariable.Machine = machine;

            Assert.False(original == newVariable);
        }

        [Fact]
        public void PropertyEqualityTest()
        {
            QMachine<char> machine = new QMachine<char>(new QMachineParameter<char>());

            QObject<char> original = QObject<char>.New('a')
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
            QMachine<char> machine = new QMachine<char>(new QMachineParameter<char>());

            QObject<char> original = QObject<char>.New('a')
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
        public void EuclideanLayerEqualityTest()
        {
            QMachine<char> machine = new QMachine<char>(new QMachineParameter<char>());
            var stateLayer = EuclideanStateLayer<char>.GetOrCreate(machine.State);
            stateLayer.UpdateSize(1,0,0);

            QObject<char> original = QObject<char>.New('a');
            EuclideanLayer<char>.GetOrCreate(original);
            stateLayer.Coordinates[original.Id] = (0, 0, 0);

            original.Machine = machine;

            var copy = original.Copy();

            Assert.True(original == copy);
        }

        [Fact]
        public void EuclideanLayerInequalityTest()
        {
            QMachine<char> machine = new QMachine<char>(new QMachineParameter<char>());
            var stateLayer = EuclideanStateLayer<char>.GetOrCreate(machine.State);
            stateLayer.UpdateSize(2, 0, 0);

            QObject<char> original = QObject<char>.New('a');
            EuclideanLayer<char>.GetOrCreate(original);
            stateLayer.Coordinates[original.Id] = (0, 0, 0);

            original.Machine = machine;

            var nonCopy = QObject<char>.New('B');
            EuclideanLayer<char>.GetOrCreate(nonCopy);
            stateLayer.Coordinates[nonCopy.Id] = (1, 0, 0);


            Assert.False(original == nonCopy);
        }
    }
}
