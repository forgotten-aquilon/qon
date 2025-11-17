using qon.Machines;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Layers.StateLayers
{
    public class EuclideanStateLayer<TQ> : BaseLayer<TQ, EuclideanStateLayer<TQ>, MachineState<TQ>>, ILayer<TQ, MachineState<TQ>> where TQ : notnull
    {
        public string[,,] FieldGrid { get; set; } = new string[0, 0, 0];

        public QVariable<TQ>? this[(int x, int y, int z) coordinate]
        {
            get
            {
                if (coordinate.x < 0 || coordinate.y < 0 || coordinate.z < 0)
                {
                    return null;
                }

                if (coordinate.x >= FieldGrid.GetLength(0) || coordinate.y >= FieldGrid.GetLength(1) || coordinate.z >= FieldGrid.GetLength(2))
                {
                    return null;
                }

                return Machine?[FieldGrid[coordinate.x, coordinate.y, coordinate.z]];
            }
        }

        public override ILayer<TQ, MachineState<TQ>> Copy()
        {
            throw new NotImplementedException();
        }
    }
}
