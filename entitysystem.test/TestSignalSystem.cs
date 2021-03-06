using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Randomous.EntitySystem.Implementations;
using Xunit;

namespace Randomous.EntitySystem.test
{
    public class TestSignalSystem : UnitTestBase
    {
        protected SignalSystem<int> signaler;
        protected CancellationTokenSource cancelSource;

        public TestSignalSystem()
        {
            signaler = CreateService<SignalSystem<int>>();
            cancelSource = new CancellationTokenSource();
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
            var task = signaler.ListenAsync(look, (x) => x == look, listenTime ?? TimeSpan.FromMinutes(1), CancellationToken.None);
            Assert.False(task.IsCompleted); //There should be no signals yet
            return task;
        }

        protected void AssertListen(Task<List<int>> task, List<int> expected)
        {
            Assert.True(expected.OrderBy(x => x).SequenceEqual(AssertWait(task).OrderBy(x => x)));
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
            var task = signaler.ListenAsync(1, (e) => true, TimeSpan.FromMinutes(1), CancellationToken.None);
            var result = signaler.SignalItems(new[] {9,7});
            Assert.True(result.ContainsKey(9));
            Assert.Equal(1, result[9]);  //A single listener (us)
            Assert.True(result.ContainsKey(7));
            Assert.Equal(1, result[7]);  //A single listener (us)
            AssertListen(task, new List<int>() {9,7});
        }

        [Fact]
        public void Nonsignaled()
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

        [Fact]
        public void ListenersCheck()
        {
            var task9 = CreateSingleListen(9); 
            var task7 = CreateSingleListen(7, TimeSpan.FromMilliseconds(100));
            Assert.True(signaler.Listeners.Count == 2);
            var result = signaler.SignalItems(new[] {9});
            AssertListen(task9, new List<int>() {9});
            Assert.Single(signaler.Listeners);
            result = signaler.SignalItems(new[] {7});
            AssertListen(task7, new List<int>() {7});
            Assert.Empty(signaler.Listeners);
        }

        [Fact]
        public void CancelListenerCheck()
        {
            var task = signaler.ListenAsync(9, (x) => x == 9, TimeSpan.FromMinutes(1), cancelSource.Token);
            var task7 = CreateSingleListen(7);
            Assert.True(signaler.Listeners.Count == 2, "two listeners");
            cancelSource.Cancel();
            try
            {
                var result = task.Result;
                throw new InvalidOperationException("Should've thrown cancel exception!");
            }
            catch(AggregateException ex)
            {
                Assert.IsType<TaskCanceledException>(ex.InnerException);
            }
            //AssertThrows<Exception>(() => task.Wait(100));
            //AssertNotWait(task7);//.Wait(1);
            //System.Threading.Thread.Sleep(100);
            Assert.Single(signaler.Listeners);//, "listeners single");
        }
    }
}