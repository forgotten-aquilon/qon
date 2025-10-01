using System;
using qon.Exceptions;
using Xunit;

namespace qon.Tests.Exceptions
{
    public class ExceptionHierarchyTests
    {
        [Fact]
        public void BaseException_PreservesProvidedMessage()
        {
            var exception = new BaseException("base message");

            Assert.Equal("base message", exception.Message);
            Assert.IsType<BaseException>(exception);
        }

        [Fact]
        public void InternalLogicException_InheritsFromBaseException()
        {
            var exception = new InternalLogicException("internal");

            Assert.Equal("internal", exception.Message);
            Assert.IsAssignableFrom<BaseException>(exception);
        }

        [Fact]
        public void FieldNullException_MessageIncludesFieldName()
        {
            var exception = new FieldNullException("Value");

            Assert.Equal("Value should not be null", exception.Message);
            Assert.IsType<FieldNullException>(exception);
            Assert.IsAssignableFrom<BaseException>(exception);
        }

        [Fact]
        public void InternalNullException_MessageIncludesMemberName()
        {
            var exception = new InternalNullException("Configuration");

            Assert.Equal("Configuration should not be null", exception.Message);
            Assert.IsType<InternalNullException>(exception);
            Assert.IsAssignableFrom<BaseException>(exception);
        }

        [Fact]
        public void NotUniqueSequenceItemsException_FormatsCounts()
        {
            var exception = new NotUniqueSequenceItemsException(uniqueItemsAmount: 2, supposedUniqueItemsAmount: 3);

            Assert.Equal("2 != 3", exception.Message);
            Assert.IsType<NotUniqueSequenceItemsException>(exception);
            Assert.IsAssignableFrom<InternalLogicException>(exception);
        }

        [Fact]
        public void TypesMismatchException_ShowsExpectedAndReceivedTypes()
        {
            var exception = new TypesMismatchException(typeof(string), typeof(int));

            Assert.Equal("Was expected variable of type Int32, instead was received variable of type String", exception.Message);
            Assert.IsType<TypesMismatchException>(exception);
            Assert.IsAssignableFrom<InternalLogicException>(exception);
        }

        [Fact]
        public void UnreachableException_UsesDiagnosticMessage()
        {
            var exception = new UnreachableException();

            Assert.Equal(
                "This exception is placed at the end of methods to silent static analyzer. If this exception is called, then something is really broken",
                exception.Message);
            Assert.IsType<UnreachableException>(exception);
            Assert.IsAssignableFrom<InternalLogicException>(exception);
        }

        [Fact]
        public void NonExhaustiveExpressionException_UsesProvidedTypeAndValue()
        {
            var exception = new NonExhaustiveExpressionException(typeof(ConsoleColor), ConsoleColor.Blue);

            Assert.Equal("Value Blue of System.ConsoleColor was not handled", exception.Message);
            Assert.IsType<NonExhaustiveExpressionException>(exception);
            Assert.IsAssignableFrom<InternalLogicException>(exception);
        }

        [Fact]
        public void NonExhaustiveExpressionException_InfersTypeFromObject()
        {
            var exception = new NonExhaustiveExpressionException(ConsoleColor.Red);

            Assert.Equal("Value Red of System.ConsoleColor was not handled", exception.Message);
            Assert.IsType<NonExhaustiveExpressionException>(exception);
            Assert.IsAssignableFrom<InternalLogicException>(exception);
        }

        [Fact]
        public void NonExhaustiveExpressionException_PreservesCustomMessage()
        {
            var exception = new NonExhaustiveExpressionException("custom message");

            Assert.Equal("custom message", exception.Message);
            Assert.IsType<NonExhaustiveExpressionException>(exception);
            Assert.IsAssignableFrom<InternalLogicException>(exception);
        }
    }
}
