using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Randomous.EntitySystem.Implementations
{
    public class Listener<T> : IDisposable
    {
        public object ListenerId;
        public DateTime CreateDate = DateTime.Now;
        public List<T> Signalers = new List<T>();
        public Func<IQueryable<T>, IQueryable<T>> Filter;
        public ManualResetEvent Signal = new ManualResetEvent(false);

        //I don't CARE if this is "not the right way to do dispose", the right way is GARBAGE and I am
        //the only one calling dispose so... I think I'll be fine.
        public void Dispose()
        {
            Signal.Dispose();
        }

        public override string ToString() 
        {
            return $"{ListenerId} ({CreateDate})";
        }
    }

    public class SignalSystem<T> : ISignaler<T>
    {
        protected ILogger<SignalSystem<T>> logger;

        protected readonly object listenLock = new object();
        protected List<Listener<T>> listeners = new List<Listener<T>>();

        public List<ListenerData> Listeners 
        {
            get
            {
                lock(listenLock)
                {
                    return listeners.Select(x => new ListenerData()
                    {
                        ListenerId = x.ListenerId,
                        StartedListening = x.CreateDate
                    }).ToList();
                }
            }
        }

        public SignalSystem(ILogger<SignalSystem<T>> logger)
        {
            this.logger = logger;
        }

        public Dictionary<T, int> SignalItems(IEnumerable<T> items) //, bool cancel)
        {
            logger.LogTrace($"SignalItems called for {items.Count()} items");

            var result = items.ToDictionary(x => x, y => 0);

            //Items don't change, so compute the queryable now so it doesn't happen for every listener
            var itemsQueryable = items.AsQueryable();

            //Assuming saving actually works, let's go alert everyone
            lock(listenLock)
            {
                foreach(var listener in listeners)
                {
                    //Don't let this one listener wreck everyone else's fun
                    try
                    {
                        //Find out which items would signal this listener
                        var signalItems = listener.Filter(itemsQueryable).ToList();

                        //Signal this listener if any did. Increase signal count for each item 
                        if(signalItems.Count > 0)
                        {
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
        public Task WaitForSignalThrowAsync(Listener<T> listener, TimeSpan maxWait, CancellationToken token)
        {
            return Task.Run(() => 
            {
                var handlers = new List<WaitHandle>() { listener.Signal };

                if(token != null)
                    handlers.Add(token.WaitHandle);

                int signalIndex = WaitHandle.WaitAny(handlers.ToArray(), maxWait); 
                logger.LogDebug($"WaitForSignalAsync finished, index: {signalIndex}");

                token.ThrowIfCancellationRequested();

                if(signalIndex == WaitHandle.WaitTimeout)
                    throw new TimeoutException($"Listener timed out {maxWait}");

            }, token);
        }

        public Task<List<T>> ListenAsync(object listenId, Func<T, bool> filter, TimeSpan maxWait, CancellationToken token)
        {
            return ListenAsync(listenId, (q) => q.Where(x => filter(x)), maxWait, token);
        }

        public async Task<List<T>> ListenAsync(object listenId, Func<IQueryable<T>, IQueryable<T>> filter, TimeSpan maxWait, CancellationToken token)
        {
            logger.LogTrace($"ListenAsync called with id {listenId}, maxWait {maxWait}");

            using(var listener = new Listener<T>() { ListenerId = listenId })
            {
                listener.Filter = filter;

                lock(listenLock)
                {
                    logger.LogDebug($"Adding listener {listener}");
                    listeners.Add(listener);
                }

                try
                {
                    await WaitForSignalThrowAsync(listener, maxWait, token);
                    return listener.Signalers;
                }
                finally
                {
                    //WE have to remove it because we added it
                    lock(listenLock)
                    {
                        logger.LogDebug($"Removing listener {listener}");
                        listeners.Remove(listener);
                    }
                }
            }
        }
    }
}