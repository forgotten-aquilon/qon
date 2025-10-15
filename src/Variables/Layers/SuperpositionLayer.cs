using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Variables.Layers
{
    public class SuperpositionLayer<T> : ILayer<T>
    {
        public ILayer<T> Copy()
        {
            throw new NotImplementedException();
        }
    }
}
