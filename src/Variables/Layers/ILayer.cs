using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Variables.Layers
{
    public interface ILayer<T>
    {
        ILayer<T> Copy();

        //TODO: Later introduce Base class with covarian return type for methods like For/With/From
    }
}

