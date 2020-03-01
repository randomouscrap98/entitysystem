using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace entitysystem
{
    public class Listener<T> : IDisposable // <E> where E : EntityBase
    {
        public DateTime CreateDate = DateTime.Now;
        public List<T> Signalers = new List<T>();
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

        public Dictionary<T, int> SignalItems(IEnumerable<T> items)
        {
            logger.LogTrace($"SignalItems called for {items.Count()} items");

            var result = items.ToDictionary(x => x, y => 0); //new Dictionary<T, int>();

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
                        if(listener.Signalers.Count > 0)
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
                listener.Filter = filter; //(e) => e is E && e.id > lastId && filter((E)e);

                lock(listenLock)
                    listeners.Add(listener);

                try
                {
                    //IF we got signaled, go ahead and return the signalers
                    if (await WaitForSignalAsync(listener, maxWait))
                        return listener.Signalers; //new List<E>() { (E)listener.SignaledEntity };
                    else
                        throw new TimeoutException($"Did not get signaled in time ({maxWait})");
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
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//
//namespace stream
//{
//    //A single stream, usually associated with a room.
//    /*public class StreamData
//    {
//        public DateTime CreateDate = DateTime.Now;
//        public DateTime UpdateDate = DateTime.Now;
//        public DateTime SaveDate = new DateTime(0);
//
//        public StringBuilder Data = new StringBuilder();
//        public readonly object Lock = new object();
//
//        //The signaler is used to wake up waiting listeners
//        public ManualResetEvent Signal = new ManualResetEvent(false);
//
//        //The list of listeners should all get flushed every time a signal comes
//        public List<StreamListener> Listeners = new List<StreamListener>();
//    }*/
//
//    //Data sent from a signal to waiters through the listener object. Should
//    //be the same data for every listener!
//    public class SignalData
//    {
//        //The amount of listeners at signal time.
//        public int ListenersBeforeSignal = 0;
//    }
//
//    //The stream and signal data retrieved from waiting. Signal data will be null
//    //if no waiting was involved.
//    public class ReadyData
//    {
//        public string Data = "";
//        public SignalData SignalData = null;
//    }
//
//    //Someone waiting for a stream. Signalled by data additions. The signal data is added
//    //through this class.
//    //Configuration for the stream system
//    public class SignalConfig
//    {
//        //Stuff I don't want to set in the json configs.
//        public TimeSpan ListenTimeout = TimeSpan.FromSeconds(300); //This length PROBABLY doesn't matter....???
//        public TimeSpan SignalTimeout = TimeSpan.FromSeconds(30);
//        public TimeSpan SignalWaitInterval = TimeSpan.FromMilliseconds(20);
//    }
//
//    public class SignalListener 
//    {
//        public Task Waiter = null;
//        public SignalData SignalData = new SignalData();
//    }
//
//    public class SignalGroup 
//    {
//        public DateTime CreateDate = DateTime.Now;
//        public readonly object Lock = new object();
//
//        //The signaler is used to wake up waiting listeners
//        public ManualResetEvent Signal = new ManualResetEvent(false);
//
//        //The list of listeners should all get flushed every time a signal comes
//        public List<SignalListener> Listeners = new List<SignalListener>();
//    }
//
//    //A group of streams, categorized by key.
//    public class SignalSystem<T>
//    {
//        protected readonly ILogger<SignalSystem<T>> logger;
//        protected readonly object Lock = new object();
//
//        public SignalConfig Config;
//        public Dictionary<string, SignalGroup> Groups = new Dictionary<string, SignalGroup>();
//
//        public SignalSystem(ILogger<SignalSystem<T>> logger, SignalConfig config)
//        {
//            this.logger = logger;
//            this.Config = config;
//        }
//
//        public async Task<ReadyData> GetDataWhenReady(StreamData stream, int start, int count = -1)
//        {
//            //JUST IN CASE we need it later (can't make it in the lock section, needed outside!)
//            bool completed = false;
//            StreamListener listener = null; 
//            ReadyData result = new ReadyData();
//
//            lock(stream.Lock)
//            {
//                if(start < stream.Data.Length)
//                {
//                    //No waiting! We're already done! The stream can never get smaller!
//                    completed = true;
//                }
//                else
//                {
//                    //Oh, waiting... we're a new listener so add it!
//                    listener = new StreamListener();
//                    listener.Waiter = Task.Run(() => completed = stream.Signal.WaitOne(Config.ListenTimeout));
//                    stream.Listeners.Add(listener);
//                }
//            }
//
//            //Don't need to listen if there's no listener!
//            if(listener != null)
//            {
//                //CANNOT wait in the lock! We're just waiting to see if we get data. If we DOOOO, "completed" will be true!
//                try
//                {
//                    await listener.Waiter;
//                }
//                finally
//                {
//                    //We're done. Doesn't matter what happened, whether it finished or we threw an exception,
//                    //we are NO LONGER listening!
//                    result.SignalData = listener.SignalData; //this might be nothing
//                    stream.Listeners.Remove(listener);
//                }
//            }
//
//            if(completed)
//                result.Data = GetData(stream, start, count);
//
//            return result;
//        }
//
//        public void RaiseSignal(string key, T item)
//        {
//
//        }
//
//        public async Task<T> WaitOnSignalAsync(string key)
//        {
//            //Make sure we have a signal group for this key
//            lock(Lock)
//            {
//                if(!Groups.ContainsKey(key))
//                    Groups.Add(key, new SignalGroup());
//            }
//
//            //
//        }
//
//        public void AddData(StreamData stream, string data)
//        {
//            lock(stream.Lock)
//            {
//                if(data.Length == 0)
//                    throw new InvalidOperationException("Can't add 0 length data!");
//
//                if(data.Length > Config.SingleDataLimit)
//                    throw new InvalidOperationException($"Too much data at once!: {Config.SingleDataLimit}");
//
//                //Don't allow data additions that would allow the limit to go beyond thingy!
//                if(stream.Data.Length >= Config.StreamDataLimit)
//                    throw new InvalidOperationException($"Stream at data limit: {Config.StreamDataLimit}");
//
//                stream.Data.Append(data);
//                stream.UpdateDate = DateTime.Now;
//
//                var signalData = new SignalData()
//                {
//                    ListenersBeforeSignal = stream.Listeners.Count
//                };
//
//                //Set the signal data before signalling so they have "communication" from us. NOT SAFE
//                //since someone could... you know, add a listener... or could they? We hold the lock for
//                //the stream and you can't add a listener without the lock! So probably safe...
//                stream.Listeners.ForEach(x => x.SignalData = signalData);
//
//                //Set the signal so all the listeners know they have data!
//                stream.Signal.Set();
//
//                try
//                {
//                    var signalStart = DateTime.Now;
//
//                    //Wait for OUR listeners to clear out! Notice that the listener wait and removal is NOT 
//                    //in a lock: this allows US to hold the lock (since it's probably safer...? we're doing the signalling).
//                    while (stream.Listeners.Count > 0)
//                    {
//                        System.Threading.Thread.Sleep(Config.SignalWaitInterval);
//
//                        if (DateTime.Now - signalStart > Config.SignalTimeout)
//                        {
//                            logger.LogWarning("Timed out while waiting for listeners to process signal!");
//                            break;
//                        }
//                    }
//                }
//                finally
//                {
//                    //ALWAYS get rid of listeners and reset the signal! we don't want to be left in an unknown state!
//                    stream.Listeners.Clear(); //This might be dangerous? IDK, we don't want to wait forever!
//                    stream.Signal.Reset();
//                }
//            }
//        }
//    }
//
//}