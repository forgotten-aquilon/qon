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
            QMachine<char> machine = QSL.Machine<char>();

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
            QMachine<char> machine = QSL.Machine<char>();

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
        public void EuclideanLayerEqualityTest()
        {
            QMachine<char> machine = new QMachine<char>(new QMachineParameter<char>());

            machine.GenerateField(null, (1, 1, 1));

            var original = machine.At(0, 0, 0);

            var copy = original.Copy();

            Assert.True(original == copy);
        }

        [Fact]
        public void EuclideanLayerInequalityTest()
        {
            QMachine<char> machine = new QMachine<char>(new QMachineParameter<char>());
            machine.GenerateField(null, (2, 1, 1));

            var original = machine.At(0, 0, 0);

            var nonCopy = machine.At(1, 0, 0);

            Assert.False(original == nonCopy);
        }
    }
}
