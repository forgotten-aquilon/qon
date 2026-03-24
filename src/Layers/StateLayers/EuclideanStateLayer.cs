using qon.Machines;
using qon.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qon.Layers.StateLayers
{
    //TODO: Add support for compact fields
    public class EuclideanStateLayer<TQ> : BaseLayer<TQ, EuclideanStateLayer<TQ>, MachineState<TQ>>, ILayer<TQ, MachineState<TQ>> where TQ : notnull
    {
        public Guid[,,] FieldGrid { get; private set; } = new Guid[0, 0, 0];

        public Dictionary<Guid, (int X, int Y, int Z)> Coordinates { get; private set; } = new();

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

                return Machine[FieldGrid[coordinate.x, coordinate.y, coordinate.z]];
            }
        }

        public QVariable<TQ>? this[int x, int y, int z]
        {
            get
            {
                if (x < 0 || y < 0 || z < 0)
                {
                    return null;
                }

                if (x >= FieldGrid.GetLength(0) || y >= FieldGrid.GetLength(1) || z >= FieldGrid.GetLength(2))
                {
                    return null;
                }

                return Machine[FieldGrid[x, y, z]];
            }
        }

        public void UpdateSize(int x, int y, int z)
        {
            FieldGrid = new Guid[x, y, z];
        }

        public override ILayer<TQ, MachineState<TQ>> Copy()
        {
            throw new NotImplementedException();
        }
    }
}
