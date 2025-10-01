using System;
using qon.Exceptions;
using qon.Helpers;
using Xunit;

namespace qon.Tests.Exceptions
{
    public class ExceptionHelperTests
    {
        [Fact]
        public void ThrowIfArgumentIsNull_ValueProvided_DoesNotThrow()
        {
            var exception = Record.Exception(() => ExceptionHelper.ThrowIfArgumentIsNull(string.Empty));

            Assert.Null(exception);
        }

        [Fact]
        public void ThrowIfArgumentIsNull_Null_UsesDefaultParameterName()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => ExceptionHelper.ThrowIfArgumentIsNull(null));

            Assert.Equal("obj", exception.ParamName);
        }

        [Fact]
        public void ThrowIfArgumentIsNull_Null_UsesProvidedParameterName()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => ExceptionHelper.ThrowIfArgumentIsNull(null, "value"));

            Assert.Equal("value", exception.ParamName);
        }

        [Fact]
        public void ThrowIfInternalValueIsNull_WithValue_DoesNotThrow()
        {
            var exception = Record.Exception(() => ExceptionHelper.ThrowIfInternalValueIsNull(new object()));

            Assert.Null(exception);
        }

        [Fact]
        public void ThrowIfInternalValueIsNull_Null_UsesDefaultNameInMessage()
        {
            var exception = Assert.Throws<InternalNullException>(() => ExceptionHelper.ThrowIfInternalValueIsNull(null));

            Assert.Equal("obj should not be null", exception.Message);
        }

        [Fact]
        public void ThrowIfInternalValueIsNull_Null_UsesProvidedNameInMessage()
        {
            var exception = Assert.Throws<InternalNullException>(() => ExceptionHelper.ThrowIfInternalValueIsNull(null, "configuration"));

            Assert.Equal("configuration should not be null", exception.Message);
        }

        [Fact]
        public void ThrowIfFieldIsNull_WithValue_DoesNotThrow()
        {
            var exception = Record.Exception(() => ExceptionHelper.ThrowIfFieldIsNull(Guid.Empty));

            Assert.Null(exception);
        }

        [Fact]
        public void ThrowIfFieldIsNull_Null_UsesDefaultNameInMessage()
        {
            var exception = Assert.Throws<FieldNullException>(() => ExceptionHelper.ThrowIfFieldIsNull(null));

            Assert.Equal("obj should not be null", exception.Message);
        }

        [Fact]
        public void ThrowIfFieldIsNull_Null_UsesProvidedNameInMessage()
        {
            var exception = Assert.Throws<FieldNullException>(() => ExceptionHelper.ThrowIfFieldIsNull(null, "field"));

            Assert.Equal("field should not be null", exception.Message);
        }

        [Fact]
        public void ThrowIfTypesMismatch_NullVariable_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => ExceptionHelper.ThrowIfTypesMismatch<int>(null));

            Assert.Equal("variable", exception.ParamName);
        }

        [Fact]
        public void ThrowIfTypesMismatch_TypeMismatch_ThrowsTypesMismatchException()
        {
            var exception = Assert.Throws<TypesMismatchException>(() => ExceptionHelper.ThrowIfTypesMismatch<int>("sample"));

            Assert.Equal("Was expected variable of type Int32, instead was received variable of type String", exception.Message);
        }

        [Fact]
        public void ThrowIfTypesMismatch_TypeMatches_ReturnsCastValue()
        {
            var result = ExceptionHelper.ThrowIfTypesMismatch<int>(12);

            Assert.Equal(12, result);
        }

        [Fact]
        public void CheckIfTypesMismatch_TypeMismatch_ReturnsEmptyOptional()
        {
            Optional<int> optional = ExceptionHelper.CheckIfTypesMismatch<int>("value");

            Assert.False(optional.HasValue);
        }

        [Fact]
        public void CheckIfTypesMismatch_TypeMatches_ReturnsOptionalWithValue()
        {
            Optional<int> optional = ExceptionHelper.CheckIfTypesMismatch<int>(42);

            Assert.True(optional.HasValue);
            Assert.Equal(42, optional.Value);
        }
    }
}
