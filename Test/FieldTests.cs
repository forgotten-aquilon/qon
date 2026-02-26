using qon.Machines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Variables.Domains;

namespace qon.Tests
{
    public class FieldTests
    {
        [Fact]
        public void BasicEqualityTest()
        {
            QMachine<char> machine = new QMachine<char>(new QMachineParameter<char>());
            machine.GenerateField(new DiscreteDomain<char>('a', 'b'), (3, 3, 3));

            var original = machine.Solver.Current.Field;

            var copy = original.Copy();

            Assert.True(copy.Equals(original));
        }
    }
}
