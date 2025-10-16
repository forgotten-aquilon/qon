using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using qon.Variables;
using qon.Variables.Layers;

namespace qon
{
    public enum SolutionState
    {
        NotSolved,
        MaybeSolved,
        Unsolvable
    }

    /// <summary>
    /// "Hack" for searching for variables in the field by properties. Working without collection-expressions is not possible in Unity for now.
    /// I don't like recursive allocation, but by design solution machine is not supposed to run too often.
    /// TODO: Remove this as soon Unity supports recent .NET versions
    /// Or maybe keep, because public API looks much better this way.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SearchResult<T>
    {
        public IEnumerable<QVariable<T>> Result { get; set; }

        public SearchResult(IEnumerable<QVariable<T>> field)
        {
            Result = field;
        }

        public void Collapse(T value, bool isConstant = false)
        {
            foreach (var variable in Result)
            {
                SuperpositionLayer<T>.Collapse(variable, value, isConstant);
            }
        }

        public SearchResult<T> this[string name, object value]
        {
            get
            {
                return Search(Result, name, value);
            }
        }

        public static SearchResult<T> Search(IEnumerable<QVariable<T>> variables, string name, object value)
        {
            var result = variables.Where(x => object.Equals(x.GetNullOrValueProperty(name), value));
            return new SearchResult<T>(result);
        }

        public static SearchResult<T> Search(IEnumerable<QVariable<T>> variables, Func<QVariable<T>, bool> predicate)
        {
            var result = variables.Where(predicate);

            return new SearchResult<T>(result);
        }
    }

    public class MachineState<T> : ICloneable
    {
        public QVariable<T>[] Field { get; protected set; }

        public SolutionState CurrentState
        {
            get
            {
                foreach (var variable in Field)
                {
                    if (SuperpositionLayer<T>.With(variable).Domain.IsEmpty() && SuperpositionLayer<T>.With(variable).State == SuperpositionState.Uncertain)
                        return SolutionState.Unsolvable;

                    if (SuperpositionLayer<T>.With(variable).State == SuperpositionState.Uncertain)
                        return SolutionState.NotSolved;
                }

                return SolutionState.MaybeSolved;
            }
        }

        public MachineState()
        {
            Field = Array.Empty<QVariable<T>>();
        }

        public MachineState(QVariable<T>[] field)
        {
            Field = field;
        }

        public void SetField(QVariable<T>[] field)
        {
            Field = field;
        }

        public int AutoCollapse()
        {
            int changes = 0;

            foreach (var v in Field)
            {
                if (SuperpositionLayer<T>.With(v).State != SuperpositionState.Uncertain)
                    continue;

                if (SuperpositionLayer<T>.AutoCollapse(v).HasValue)
                {
                    changes++;
                }
            }

            return changes;
        }

        public object Clone()
        {
            var clone = new MachineState<T>(Field.Select(x => x.Copy()).ToArray());
            return clone;
        }

        public SearchResult<T> this[string name, object value]
        {
            get
            {
                return SearchResult<T>.Search(Field, name, value);
            }
        }

        public SearchResult<T> this[Func<QVariable<T>, bool> predicate]
        {
            get
            {
                return SearchResult<T>.Search(Field, predicate);
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder("{ ");

            var fieldRepresentation = Field.Select(v =>
                SuperpositionLayer<T>.With(v).State != SuperpositionState.Uncertain ? $"{v.Name}:[{v.Value}]" : $"{v.Name}:[{SuperpositionLayer<T>.With(v).Domain}]");

            result.AppendJoin(" ", fieldRepresentation);
            result.Append("}");

            return result.ToString();
        }
    }
}
