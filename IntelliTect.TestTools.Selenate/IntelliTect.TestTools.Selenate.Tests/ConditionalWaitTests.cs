using System;
using Xunit;

namespace IntelliTect.TestTools.Selenate.Tests
{
    public class ConditionalWaitTests
    {
        [Fact]
        public async void CheckActionParamsForTypeChecking()
        {
            ConditionalWait wait = new ConditionalWait();
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(() => wait.WaitFor(TestVoidDelegate, TimeSpan.FromSeconds(1), typeof(string)));
            Assert.Equal("Invalid type passed into exceptionsToIgnore paramter. Must be of type Exception.", ex.Message);
        }

        [Fact]
        public async void CheckFuncParamsForTypeChecking()
        {
            ConditionalWait wait = new ConditionalWait();
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(() => wait.WaitFor(TestReturnDelegate, TimeSpan.FromSeconds(1), typeof(string)));
            Assert.Equal("Invalid type passed into exceptionsToIgnore paramter. Must be of type Exception.", ex.Message);
        }

        [Fact]
        public async void CheckForUnexpectedException()
        {
            ConditionalWait wait = new ConditionalWait();
            await Assert.ThrowsAsync<NullReferenceException>(() => wait.WaitFor<ArgumentNullException>(() => TestNullRefException(), TimeSpan.FromSeconds(1)));
        }

        private void TestVoidDelegate()
        {
            throw new Exception();
        }

        private bool TestReturnDelegate()
        {
            throw new Exception();
        }

        private void TestNullRefException()
        {
            throw new NullReferenceException();
        }
    }
}
