using qon.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace qon.Exceptions
{
    //FUTURE: Update using CallerArgumentExpression attribute when possible
    public static class ExceptionHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfArgumentIsNull([NotNull] object? obj, string? name = null)
        {
            _ = obj ?? throw new ArgumentNullException(name ?? nameof(obj));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ThrowIfArgumentIsNull<T>([NotNull] T? obj, string? name = null) where T : class
        {
            return obj ?? throw new ArgumentNullException(name ?? nameof(obj));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ThrowIfArgumentIsNull<T>([NotNull] T? obj, string? name = null) where T : struct
        {
            return obj ?? throw new ArgumentNullException(name ?? nameof(obj));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfInternalValueIsNull([NotNull] object? obj, string? name = null)
        {
            _ = obj ?? throw new InternalNullException(name ?? nameof(obj));
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InternalNullException"></exception>
        public static T ThrowIfInternalValueIsNull<T>([NotNull] T? obj, string? name = null) where T: class
        {
            return obj ?? throw new InternalNullException(name ?? nameof(obj));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ThrowIfInternalValueIsNull<T>([NotNull] T? obj, string? name = null) where T : struct
        {
            return obj ?? throw new InternalNullException(name ?? nameof(obj));
        }

        //public static void ThrowIfFieldIsNull([NotNull] object? obj, string? name = null)
        //{
        //    _ = obj ?? throw new FieldNullException(name ?? nameof(obj));
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ThrowIfFieldIsNull<T>([NotNull] T? obj, string? name = null) where T : class
        {
            return obj ?? throw new FieldNullException(name ?? nameof(obj));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ThrowIfFieldIsNull<T>([NotNull] T? obj, string? name = null) where T : struct
        {
            return obj ?? throw new FieldNullException(name ?? nameof(obj));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ThrowIfPredicateFalse<T>(T obj, Func<T, bool> predicate, string? message = null)
        {
            if (!predicate(obj))
            {
                throw message is null
                    ? new ValidationException(obj!, predicate)
                    : new ValidationException(message);
            }

            return obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ThrowIfPredicateTrue<T>(T obj, Func<T, bool> predicate, string? message = null)
        {
            if (predicate(obj))
            {
                throw message is null
                    ? new ValidationException(obj!, predicate)
                    : new ValidationException(message);
            }

            return obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TOut ThrowIfTypesMismatch<TOut>(object? variable)
        {
            Type expectedType = typeof(TOut);

            ThrowIfArgumentIsNull(variable, nameof(variable));

            if (!expectedType.IsInstanceOfType(variable))
            {
                throw new TypesMismatchException(variable.GetType(), expectedType);
            }

            return (TOut)variable;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<TOut> CheckIfTypesMismatch<TOut>(object variable)
        {
            Type expectedType = typeof(TOut);

            if (!expectedType.IsInstanceOfType(variable))
            {
                return Optional<TOut>.Empty;
            }

            return Optional<TOut>.Of((TOut)variable);
        }
    }
}
