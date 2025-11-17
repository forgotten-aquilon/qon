using System;
using qon.Exceptions;
using qon.Helpers;
using Xunit;

namespace qon.Tests.Exceptions
{
    public class ExceptionHelperTests
    {
        [Fact]
        public void ThrowIfArgumentIsNull_WhenNull_ThrowsArgumentNullException()
        {
            static void CallHelper(object? candidate)
            {
                ExceptionHelper.ThrowIfArgumentIsNull(candidate, nameof(candidate));
            }

            Assert.Throws<ArgumentNullException>(() => CallHelper(null));
        }

        [Fact]
        public void ThrowIfArgumentIsNull_WhenNotNull_ReturnsValue()
        {
            var instance = new object();

            var result = ExceptionHelper.ThrowIfArgumentIsNull<object>(instance, nameof(instance));

            Assert.Same(instance, result);
        }

        [Fact]
        public void ThrowIfInternalValueIsNull_WhenNull_ThrowsInternalNullException()
        {
            Assert.Throws<InternalNullException>(() => ExceptionHelper.ThrowIfInternalValueIsNull(null));
        }

        [Fact]
        public void ThrowIfInternalValueIsNull_WhenNotNull_ReturnsValue()
        {
            var instance = new object();

            var result = ExceptionHelper.ThrowIfInternalValueIsNull<object>(instance, nameof(instance));

            Assert.Same(instance, result);
        }

        [Fact]
        public void ThrowIfFieldIsNull_WhenNull_ThrowsFieldNullException()
        {
            var holder = new ExceptiontestsData.Holder();

            Assert.Throws<FieldNullException>(() => ExceptionHelper.ThrowIfFieldIsNull(holder.Value, nameof(holder.Value)));
        }

        [Fact]
        public void ThrowIfFieldIsNull_WhenNotNull_ReturnsValue()
        {
            var instance = new object();

            var result = ExceptionHelper.ThrowIfFieldIsNull<object>(instance, "field");

            Assert.Same(instance, result);
        }

        [Fact]
        public void ThrowIfPredicateFalse_WhenPredicateFails_ThrowsValidationException()
        {
            Assert.Throws<ValidationException>(() => ExceptionHelper.ThrowIfPredicateFalse(5, value => value < 0));
        }

        [Fact]
        public void ThrowIfPredicateFalse_WhenPredicatePasses_ReturnsOriginalValue()
        {
            int value = 42;

            var result = ExceptionHelper.ThrowIfPredicateFalse(value, v => v == 42);

            Assert.Equal(value, result);
        }

        [Fact]
        public void ThrowIfTypesMismatch_WhenTypeDiffers_ThrowsTypesMismatchException()
        {
            Assert.Throws<TypesMismatchException>(() => ExceptionHelper.ThrowIfTypesMismatch<string>(123));
        }

        [Fact]
        public void ThrowIfTypesMismatch_WhenTypeMatches_ReturnsValue()
        {
            const string text = "value";

            var result = ExceptionHelper.ThrowIfTypesMismatch<string>(text);

            Assert.Equal(text, result);
        }

        [Fact]
        public void CheckIfTypesMismatch_WhenTypeDiffers_ReturnsEmptyOptional()
        {
            Optional<string> result = ExceptionHelper.CheckIfTypesMismatch<string>(123);

            Assert.False(result.HasValue);
        }

        [Fact]
        public void CheckIfTypesMismatch_WhenTypeMatches_ReturnsOptionalWithValue()
        {
            const string text = "value";

            Optional<string> result = ExceptionHelper.CheckIfTypesMismatch<string>(text);

            Assert.True(result.HasValue);
            Assert.Equal(text, result.Value);
        }

    }
}
