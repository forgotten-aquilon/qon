using System;

namespace qon
{
    public class BaseException : Exception
    {
        public BaseException() : base() { }
        public BaseException(string? message) : base(message) { }
    }

    public class NotUniqueSequenceItemsException : BaseException
    {
        public NotUniqueSequenceItemsException(int uniqueItemsAmount, int supposedUniqueItemsAmount) : 
            base($"{uniqueItemsAmount} != {supposedUniqueItemsAmount}") { }
    }

    public class InternalLogicException : BaseException
    {
        public InternalLogicException(string? message) : base(message) { }
    }

    public class FieldNullException : BaseException
    {
        public FieldNullException(string? name) : base($"{name} should not be null") { }
    }

    public class InternalNullException : BaseException
    {
        public InternalNullException(string? name) : base($"{name} should not be null") { }
    }
}
