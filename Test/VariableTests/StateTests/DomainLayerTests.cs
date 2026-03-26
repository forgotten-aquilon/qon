using qon.Layers.VariableLayers;
using qon.Machines;
using qon.Variables;
using qon.Variables.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Tests.VariableTests.StateTests
{
    public class DomainLayerTests
    {
        [Fact]
        public void DomainLayerEqualityTest()
        {
            QMachine<char> machine = new QMachine<char>(new QMachineParameter<char>());

            QObject<char> original = QObject<char>.New('a');

            DomainLayer<char> layer = DomainLayer<char>.GetOrCreate(original);
            layer.AssignDomain(new DiscreteDomain<char>('a', 'b', 'c'));

            original.Machine = machine;

            var copy = original.Copy();

            Assert.True(original == copy);
        }
    }
}
