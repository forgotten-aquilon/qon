using System;

namespace qon.Exceptions
{
    public class BaseException : Exception
    {
        public BaseException() : base() { }
        public BaseException(string? message) : base(message) { }
    }

    public class InternalLogicException : BaseException
    {
        public InternalLogicException(string? message) : base(message) { }
    }

    public class NotUniqueSequenceItemsException : InternalLogicException
    {
        public NotUniqueSequenceItemsException(int uniqueItemsAmount, int supposedUniqueItemsAmount) : 
            base($"{uniqueItemsAmount} != {supposedUniqueItemsAmount}") { }
    }

    public class FieldNullException : BaseException
    {
        public FieldNullException(string? name) : base($"{name} should not be null") { }
    }

    public class InternalNullException : BaseException
    {
        public InternalNullException(string? name) : base($"{name} should not be null") { }
    }

    public class TypesMismatchException : InternalLogicException
    {
        public TypesMismatchException(Type receivedType, Type expectedType) : 
            base($"Was expected variable of type {expectedType.Name}, instead was received variable of type {receivedType.Name}") { }
    }

    public class UnreachableException : InternalLogicException
    {
        public UnreachableException() : base("This exception is placed at the end of methods to silent static analyzer. If this exception is called, then something is really broken") { }
    }

    public class NonExhaustiveExpressionException : InternalLogicException
    {
        public NonExhaustiveExpressionException(Type type, object? obj) : base($"Value {obj} of {type} was not handled") { }
        public NonExhaustiveExpressionException(object obj) : base($"Value {obj} of {obj?.GetType()} was not handled") { }
        public NonExhaustiveExpressionException(string message) : base(message) { }
    }

    public class ValidationException : BaseException
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(object obj, object funcObj) : base($"Object \"{obj}\" of Type {obj.GetType()} has invalid state according to {funcObj}") { }
    }
}
