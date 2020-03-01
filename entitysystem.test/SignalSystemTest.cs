using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace entitysystem.test
{
    public class SignalSystemTest : UnitTestBase
    {
        protected SignalSystem<int> signaler;

        public SignalSystemTest()
        {
            signaler = CreateService<SignalSystem<int>>();
        }

        [Fact]
        public void SimpleSignal()
        {
            var result = signaler.SignalItems(new[] {1});
            Assert.True(result.ContainsKey(1));
            Assert.Equal(0, result[1]); //There should be no signals.
            //The signal items should not throw an exception.
        }

        protected Task<List<int>> CreateSingleListen(int look, TimeSpan? listenTime = null)
        {
            var task = signaler.ListenAsync((x) => x == look, listenTime ?? TimeSpan.FromMinutes(1));
            Assert.False(task.IsCompleted); //There should be no signals yet
            return task;
        }

        protected void AssertListen(Task<List<int>> task, List<int> expected)
        {
            Assert.True(task.Wait(1));   //We should've gotten signaled
            var retrieved = task.Result; //This won't wait at all if the previous came through
            Assert.True(expected.OrderBy(x => x).SequenceEqual(retrieved.OrderBy(x => x)));
        }

        [Fact]
        public void SimpleSignalListen()
        {
            var task = CreateSingleListen(9);
            var result = signaler.SignalItems(new[] {9});
            Assert.True(result.ContainsKey(9));
            Assert.Equal(1, result[9]);  //A single listener (us)
            AssertListen(task, new List<int>() {9});
        }

        [Fact]
        public void DoubleSignal()
        {
            var task9 = CreateSingleListen(9); 
            var task7 = CreateSingleListen(7);
            var result = signaler.SignalItems(new[] {9,7}); //Signal both
            Assert.True(result.ContainsKey(9));
            Assert.Equal(1, result[9]);  //A single listener (us)
            Assert.True(result.ContainsKey(7));
            Assert.Equal(1, result[7]);  //A single listener (us)
            AssertListen(task9, new List<int>() {9});
            AssertListen(task7, new List<int>() {7});
        }

        [Fact]
        public void MultiSignal()
        {
            var task = signaler.ListenAsync((e) => true, TimeSpan.FromMinutes(1));
            var result = signaler.SignalItems(new[] {9,7});
            Assert.True(result.ContainsKey(9));
            Assert.Equal(1, result[9]);  //A single listener (us)
            Assert.True(result.ContainsKey(7));
            Assert.Equal(1, result[7]);  //A single listener (us)
            AssertListen(task, new List<int>() {9,7});
        }

        [Fact]
        public async Task Nonsignaled()
        {
            var task9 = CreateSingleListen(9); 
            var task7 = CreateSingleListen(7, TimeSpan.FromMilliseconds(100));
            var result = signaler.SignalItems(new[] {9});
            Assert.True(result.ContainsKey(9));
            Assert.Equal(1, result[9]);  //A single listener (us)
            Assert.False(result.ContainsKey(7)); //No signalled entities
            AssertListen(task9, new List<int>() {9});
            try
            {
                var fakeResult = task7.Result;
                throw new InvalidOperationException("This should've thrown an exception!");
            }
            catch(Exception ex)
            {
                Assert.IsType<TimeoutException>(ex.InnerException);
            }
        }
    }
}