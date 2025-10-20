using qon.Functions;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Layers.StateLayers
{
    public interface IStateLayer<T>
    {
        public Result Execute(QVariable<T>[] field);

        public bool Validate(QVariable<T>[] field);

    }
}
