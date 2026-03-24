using System;
using System.Collections.Generic;
using qon.Exceptions;
using qon.Machines;
using qon.Variables;

namespace qon.Functions.Constraints
{
    public class BindingConstraint<TQ> : IPreparation<TQ> where TQ : notnull
    {
        private readonly Guid[] _variableIds;
        private readonly Func<IReadOnlyList<TQ>, bool> _func;

        public BindingConstraint(IEnumerable<QLink<TQ>> qLinks, Func<IReadOnlyList<TQ>, bool> func)
        {
            ExceptionHelper.ThrowIfArgumentIsNull(qLinks, nameof(qLinks));
            ExceptionHelper.ThrowIfArgumentIsNull(func, nameof(func));

            List<Guid> variableIds = new List<Guid>();

            foreach (var qLink in qLinks)
            {
                ExceptionHelper.ThrowIfArgumentIsNull(qLink, nameof(qLinks));
                variableIds.Add(qLink.Variable.Id);
            }

            if (variableIds.Count == 0)
            {
                throw new InternalLogicException("Binding constraint should contain non-zero amount of variables");
            }

            _variableIds = variableIds.ToArray();
            _func = func;
        }

        public Result Execute(Field<TQ> field)
        {
            TQ[] values = new TQ[_variableIds.Length];

            for (int i = 0; i < _variableIds.Length; i++)
            {
                var value = field[_variableIds[i]].Value;

                if (!value.HasValue)
                {
                    return Result.Success(0);
                }

                values[i] = value.Value;
            }

            return _func(values) ? Result.Success(0) : Result.HasErrors();
        }
    }
}
