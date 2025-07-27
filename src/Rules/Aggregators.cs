using System;

namespace qon.Rules
{
    public class GroupingAggregator<T>
    {
        public Func<SuperpositionVariable<T>, object> AggregationFunction { get; }

        public GroupingAggregator(Func<SuperpositionVariable<T>, object> aggregationFunction)
        {
            AggregationFunction = aggregationFunction;
        }

        public object ApplyTo(SuperpositionVariable<T> variable)
        {
            return AggregationFunction(variable);
        }
    }

    public class SelectingAggregator<T>
    {
        public Func<SuperpositionVariable<T>, bool> AggregationFunction { get; }

        public SelectingAggregator(Func<SuperpositionVariable<T>, bool> aggregationFunction)
        {
            AggregationFunction = aggregationFunction;
        }

        public bool ApplyTo(SuperpositionVariable<T> variable)
        {
            return AggregationFunction(variable);
        }
    }

    public static class Aggregators
    {
        public static GroupingAggregator<T> GroupByTag<T>(string s)
        {
            return new GroupingAggregator<T>(v => v.Properties[s]);
        }

        public static SelectingAggregator<T> SelectByTagValue<T>(string s, object value)
        {
            return new SelectingAggregator<T>(v => v.Properties[s].Item2.Equals(value));
        }

        public static SelectingAggregator<T> All<T>()
        {
            return new SelectingAggregator<T>(v => true);
        }

        public static SelectingAggregator<T> Unassigned<T>()
        {
            return new SelectingAggregator<T>(v => v.State == SuperpositionState.Uncertain);
        }

        public static SelectingAggregator<T> Assigned<T>()
        {
            return new SelectingAggregator<T>(v => v.State != SuperpositionState.Uncertain);
        }

        public static SelectingAggregator<T> DomainContains<T>(T value)
        {
            return new SelectingAggregator<T>(v => v.Domain.ContainsValue(value));
        }
    }

    public static class EuclideanAggregators
    {
        public static Func<SuperpositionVariable<T>, SelectingAggregator<T>> SelectRegion<T>((int x, int y, int z) l1, (int x, int y, int z) l2)
        {
            return referenceVar =>
            {
                if (referenceVar == null)
                    throw new ArgumentNullException(nameof(referenceVar));

                return new SelectingAggregator<T>(candidateVar =>
                {
                    int rx = (int)referenceVar.Properties["x"].Item2;
                    int ry = (int)referenceVar.Properties["y"].Item2;
                    int rz = (int)referenceVar.Properties["z"].Item2;

                    int cx = (int)candidateVar.Properties["x"].Item2;
                    int cy = (int)candidateVar.Properties["y"].Item2;
                    int cz = (int)candidateVar.Properties["z"].Item2;

                    return cx >= rx - l1.x && cx <= rx + l2.x &&
                           cy >= ry - l1.y && cy <= ry + l2.y &&
                           cz >= rz - l1.z && cz <= rz + l2.z;
                });
            };
        }

        public static GroupingAggregator<T> GroupByRectangle<T>(int width, int height)
        {
            return new GroupingAggregator<T>(
                v => $"{(int)v.Properties["x"].Item2 / width}x{(int)v.Properties["y"].Item2 / height}"
            );
        }
    }
}
