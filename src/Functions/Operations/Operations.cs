using qon.Exceptions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Functions.Operations
{
    public static class Operations
    {
        public static Operation<int, bool> Comparison(int compareValue, COperator condition)
        {
            return new Operation<int, bool>(number =>
            {
                return condition switch
                {
                    COperator.EQ => number == compareValue,
                    COperator.NE => number != compareValue,
                    COperator.LT => number < compareValue,
                    COperator.LE => number <= compareValue,
                    COperator.GT => number > compareValue,
                    COperator.GE => number >= compareValue,
                    _ => throw new InternalLogicException("Passed nonexistent enum value"),
                };
            });
        }
    }
}
