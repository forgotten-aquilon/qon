using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using qon.Rules;
using qon.Rules.Filters;
using qon.Variables;

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
        public IEnumerable<SuperpositionVariable<T>> Result { get; set; }

        public SearchResult(IEnumerable<SuperpositionVariable<T>> field)
        {
            Result = field;
        }

        public void Collapse(T value, bool isConstant = false)
        {
            foreach (var variable in Result)
            {
                variable.Collapse(value, isConstant);
            }
        }

        public SearchResult<T> this[string name, object value]
        {
            get
            {
                return Search(Result, name, value);
            }
        }

        public static SearchResult<T> Search(IEnumerable<SuperpositionVariable<T>> variables, string name, object value)
        {
            var result = variables.Where(x => object.Equals(x.GetNullOrValueProperty(name), value));
            return new SearchResult<T>(result);
        }
    }

    public class MachineState<T> : ICloneable
    {
        public SuperpositionVariable<T>[] Field { get; protected set; }

        public SolutionState CurrentState
        {
            get
            {
                foreach (var variable in Field)
                {
                    if (variable.Domain.IsEmpty() && variable.State == SuperpositionState.Uncertain)
                        return SolutionState.Unsolvable;

                    if (variable.State == SuperpositionState.Uncertain)
                        return SolutionState.NotSolved;
                }

                return SolutionState.MaybeSolved;
            }
        }

        public MachineState()
        {
            Field = Array.Empty<SuperpositionVariable<T>>();
        }

        public MachineState(SuperpositionVariable<T>[] field)
        {
            Field = field;
        }

        public void SetField(SuperpositionVariable<T>[] field)
        {
            Field = field;
        }

        public int AutoCollapse()
        {
            int changes = 0;

            foreach (var v in Field)
            {
                if (v.State != SuperpositionState.Uncertain) 
                    continue;

                if (v.AutoCollapse().HasValue)
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

        public override string ToString()
        {
            StringBuilder result = new StringBuilder("{ ");

            var fieldRepresentation = Field.Select(v =>
                v.State != SuperpositionState.Uncertain ? $"{v.Name}:[{v.Value}]" : $"{v.Name}:[{v.Domain}]");

            result.AppendJoin(" ", fieldRepresentation);
            result.Append("}");

            return result.ToString();
        }
    }
}
