using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem
{
    public class Listener<T> : IDisposable
    {
        public DateTime CreateDate = DateTime.Now;
        public List<T> Signalers = new List<T>();
        //public bool Cancelled = false;
        public Func<T, bool> Filter;
        public ManualResetEvent Signal = new ManualResetEvent(false);

        //I don't CARE if this is "not the right way to do dispose", the right way is GARBAGE and I am
        //the only one calling dispose so... I think I'll be fine.
        public void Dispose()
        {
            Signal.Dispose();
        }
    }

    public class SignalSystem<T> : ISignaler<T>
    {
        protected ILogger logger;

        protected readonly object listenLock = new object();
        protected List<Listener<T>> listeners = new List<Listener<T>>();

        public SignalSystem(ILogger<SignalSystem<T>> logger)
        {
            this.logger = logger;
        }

        public Dictionary<T, int> SignalItems(IEnumerable<T> items) //, bool cancel)
        {
            logger.LogTrace($"SignalItems called for {items.Count()} items");

            var result = items.ToDictionary(x => x, y => 0);

            //Assuming saving actually works, let's go alert everyone
            lock(listenLock)
            {
                foreach(var listener in listeners)
                {
                    //Don't let this one listener wreck everyone else's fun
                    try
                    {
                        //Find out which items would signal this listener
                        var signalItems = items.Where(x => listener.Filter(x)).ToList();

                        //Signal this listener if any did. Increase signal count for each item 
                        if(signalItems.Count > 0)
                        {
                            //listener.Cancelled = cancel;
                            listener.Signalers = signalItems;
                            listener.Signal.Set();
                            listener.Signalers.ForEach(x => result[x]++);
                        }
                    }
                    catch(Exception ex)
                    {
                        logger.LogError($"Error while processing listener {listener}: {ex}");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Given a listener, wait for the given amount of time to be signaled.
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="maxWait"></param>
        /// <returns>Whether we got signaled or not</returns>
        public async Task<bool> WaitForSignalAsync(Listener<T> listener, TimeSpan maxWait)
        {
            bool signaled = false;
            await Task.Run(() => signaled = listener.Signal.WaitOne(maxWait));
            return signaled;
        }

        public async Task<List<T>> ListenAsync(Func<T, bool> filter, TimeSpan maxWait)
        {
            logger.LogTrace($"ListenAsync called with maxWait {maxWait}");

            using(var listener = new Listener<T>())
            {
                listener.Filter = filter;

                lock(listenLock)
                    listeners.Add(listener);

                try
                {
                    //IF we got signaled, go ahead and return the signalers
                    if (await WaitForSignalAsync(listener, maxWait))
                    {
                        //if(listener.Cancelled)
                        //    throw new TaskCanceledException("The item");
                        return listener.Signalers;
                    }
                    else
                    {
                        throw new TimeoutException($"Did not get signaled in time ({maxWait})");
                    }
                }
                finally
                {
                    //WE have to remove it because we added it
                    lock(listenLock)
                        listeners.Remove(listener);
                }
            }
        }
    }
}