using System;
using System.Linq;
using qon.Exceptions;
using qon.Variables.Domains;

namespace qon.Tests.VariableTests.Domains
{
    public class NumericalDomainTests
    {
        [Fact]
        public void IntervalLengthIncludesBothBoundsForSignedValues()
        {
            var interval = new Interval<int>(-2, 2);

            Assert.Equal((ulong)5, interval.Length);
        }

        [Fact]
        public void IntervalLengthIncludesBothBoundsForUnsignedValues()
        {
            var interval = new Interval<ulong>(ulong.MaxValue - 1, ulong.MaxValue);

            Assert.Equal((ulong)2, interval.Length);
        }

        [Fact]
        public void RemoveSplitsIntervalWhenRemovingMiddleValue()
        {
            var domain = new NumericalDomain<int>(new[] { new Interval<int>(1, 5) });

            var removed = domain.Remove(3);

            Assert.Equal(1, removed);
            Assert.Collection(
                domain.Domain,
                interval =>
                {
                    Assert.Equal(1, interval.Start);
                    Assert.Equal(2, interval.End);
                },
                interval =>
                {
                    Assert.Equal(4, interval.Start);
                    Assert.Equal(5, interval.End);
                });
            Assert.Equal(new[] { 1, 2, 4, 5 }, domain.GetValues().ToArray());
        }

        [Fact]
        public void RemoveEnumerableIgnoresDuplicatesAndOnlyCountsAppliedChanges()
        {
            var domain = new NumericalDomain<int>(new[] { new Interval<int>(1, 6) });

            var removed = domain.Remove(new[] { 5, 2, 2, 9 });

            Assert.Equal(2, removed);
            Assert.Equal(new[] { 1, 3, 4, 6 }, domain.GetValues().ToArray());
        }

        [Fact]
        public void SingleOrEmptyValueReturnsRemainingValueAfterShrinkingInterval()
        {
            var domain = new NumericalDomain<int>(new[] { new Interval<int>(1, 3) });

            domain.Remove(1);
            domain.Remove(3);

            var single = domain.SingleOrEmptyValue();

            Assert.True(single.HasValue);
            Assert.Equal(2, single.Value);
        }

        [Fact]
        public void CopyCreatesIndependentSnapshot()
        {
            var original = new NumericalDomain<int>(new[] { new Interval<int>(1, 3) });
            var copy = (NumericalDomain<int>)original.Copy();

            original.Remove(2);

            Assert.Equal(new[] { 1, 3 }, original.GetValues().ToArray());
            Assert.Equal(new[] { 1, 2, 3 }, copy.GetValues().ToArray());
        }

        [Fact]
        public void SizeReturnsIntMaxValueWhenTrueSizeExceedsIntRange()
        {
            var domain = new NumericalDomain<long>(new[] { new Interval<long>(0, int.MaxValue) });

            Assert.Equal(int.MaxValue, domain.Size());
            Assert.Equal((ulong)int.MaxValue + 1, domain.TrueSize());
        }

        [Fact]
        public void GetEntropyUsesTrueSizeAcrossAllIntervals()
        {
            var domain = new NumericalDomain<int>(
                new[]
                {
                    new Interval<int>(1, 1),
                    new Interval<int>(4, 5),
                });

            Assert.Equal(Math.Log(3, 2), domain.GetEntropy(), 10);
        }

        [Fact]
        public void GetRandomValueCanSelectValueFromLaterInterval()
        {
            var domain = new NumericalDomain<int>(
                new[]
                {
                    new Interval<int>(1, 3),
                    new Interval<int>(10, 10),
                });

            var value = domain.GetRandomValue(new FixedBytesRandom(3));

            Assert.Equal(10, value);
        }

        [Fact]
        public void GetEntropyThrowsForEmptyDomain()
        {
            var domain = new NumericalDomain<int>(Array.Empty<Interval<int>>());

            Assert.Throws<InternalLogicException>(() => domain.GetEntropy());
        }

        private sealed class FixedBytesRandom : Random
        {
            private readonly byte[] _bytes;

            public FixedBytesRandom(ulong value)
            {
                _bytes = BitConverter.GetBytes(value);
            }

            public override void NextBytes(byte[] buffer)
            {
                Fill(buffer);
            }

            public override void NextBytes(Span<byte> buffer)
            {
                Fill(buffer);
            }

            private void Fill(Span<byte> buffer)
            {
                buffer.Clear();
                _bytes.AsSpan(0, Math.Min(_bytes.Length, buffer.Length)).CopyTo(buffer);
            }
        }
    }
}
