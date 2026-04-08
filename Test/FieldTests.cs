using qon.Machines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using qon.Exceptions;
using qon.Helpers;
using qon.QSL;
using qon.Variables.Domains;

namespace qon.Tests
{
    public class FieldTests
    {
        [Fact]
        public void BasicEqualityTest()
        {
            QMachine<char> machine = new QMachine<char>(new QMachineParameter<char>());
            machine.GenerateField((3, 3, 3), new DiscreteDomain<char>('a', 'b'));

            var original = machine.Solver.Current.Field;

            var copy = original.Copy();

            Assert.True(copy.Equals(original));
        }

        [Fact]
        public void UpdateWithValuesUpdatesExistingObjectsInPlace()
        {
            QMachine<char> machine = QMachine<char>.Create();
            machine.GenerateField(new DiscreteDomain<char>('a', 'b', 'c'), 2);

            var field = machine.State.Field;
            var first = field[0];
            var second = field[1];

            field.UpdateWithValues(new[] { Optional<char>.Of('a'), Optional<char>.Of('c') });

            Assert.Same(first, field[0]);
            Assert.Same(second, field[1]);
            Assert.True(field[0].Value.CheckValue('a'));
            Assert.True(field[1].Value.CheckValue('c'));
        }

        [Fact]
        public void UpdateWithValuesRejectsMismatchedLength()
        {
            QMachine<char> machine = QMachine<char>.Create();
            machine.GenerateField(new DiscreteDomain<char>('a', 'b'), 2);

            var field = machine.State.Field;

            Assert.Throws<ValidationException>(() => field.UpdateWithValues(new[] { Optional<char>.Of('a') }));
        }
    }
}
