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
            QMachine<char> machine = QSL.Machine<char>();

            var lnk = machine.Q().WithDomain(new DiscreteDomain<char>('a', 'b', 'c'));


            var copy = lnk.Object.Copy();

            Assert.True(lnk.Object == copy);
        }
    }
}
