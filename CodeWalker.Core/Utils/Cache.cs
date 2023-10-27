using CodeWalker.GameFiles;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeWalker
{
    public class Cache<TKey, TVal> where TVal : Cacheable<TKey>
    {
        public long MaxMemoryUsage = 536870912; //512mb
        public long CurrentMemoryUsage = 0;
        public double CacheTime = 10.0; //seconds to keep something that's not used
        public DateTime CurrentTime = DateTime.Now;

        private LinkedList<TVal> loadedList = new LinkedList<TVal>();
        private object loadedListLock = new object();
        private Dictionary<TKey, LinkedListNode<TVal>> loadedListDict = new Dictionary<TKey, LinkedListNode<TVal>>();

        public int Count
        {
            get
            {
                return loadedList.Count;
            }
        }

        public Cache()
        {
        }
        public Cache(long maxMemoryUsage, double cacheTime)
        {
            MaxMemoryUsage = maxMemoryUsage;
            CacheTime = cacheTime;
        }

        public void BeginFrame()
        {
            CurrentTime = DateTime.Now;
            Compact();
        }

        public TVal TryGet(TKey key)
        {
            LinkedListNode<TVal> lln = null;
            lock (loadedListLock)
            {
                if (loadedListDict.TryGetValue(key, out lln))
                {

                    loadedList.Remove(lln);
                    loadedList.AddLast(lln);

                    lln.Value.LastUseTime = CurrentTime;
                }
            }
            return (lln != null) ? lln.Value : null;
        }
        public bool TryAdd(TKey key, TVal item)
        {
            item.Key = key;
            if (CanAdd())
            {
                lock(loadedListLock)
                {
                    var lln = loadedList.AddLast(item);
                    loadedListDict.Add(key, lln);
                    Interlocked.Add(ref CurrentMemoryUsage, item.MemoryUsage);
                }
                return true;
            }
            else
            {
                //cache full, check the front of the list for oldest..
                var oldlln = loadedList.First;
                var cachetime = CacheTime;
                int iter = 0, maxiter = 2;
                while (!CanAdd() && (iter<maxiter))
                {
                    while ((!CanAdd()) && (oldlln != null) && ((CurrentTime - oldlln.Value.LastUseTime).TotalSeconds > cachetime))
                    {
                        lock(loadedListLock)
                        {
                            Interlocked.Add(ref CurrentMemoryUsage, -oldlln.Value.MemoryUsage);
                            loadedListDict.Remove(oldlln.Value.Key);
                            loadedList.Remove(oldlln); //gc should free up memory later..
                        }

                        oldlln.Value = null;
                        oldlln = null;
                        //GC.Collect();
                        oldlln = loadedList.First;
                    }
                    cachetime *= 0.5;
                    iter++;
                }
                if (CanAdd()) //see if there's enough memory now...
                {
                    lock(loadedListLock)
                    {
                        var newlln = loadedList.AddLast(item);
                        loadedListDict.Add(key, newlln);
                        Interlocked.Add(ref CurrentMemoryUsage, item.MemoryUsage);
                    }
                    return true;
                }
                else
                {
                    //really shouldn't get here, but it's possible under stress.
                }
            }
            return false;
        }

        public bool CanAdd()
        {
            return Interlocked.Read(ref CurrentMemoryUsage) < MaxMemoryUsage;
        }


        public void Clear()
        {
            lock(loadedList)
            {
                loadedList.Clear();
                loadedListDict.Clear();
                CurrentMemoryUsage = 0;
            }
        }

        public void Remove(TKey key)
        {
            if (!loadedListDict.ContainsKey(key))
            {
                return;
            }
            lock(loadedListLock)
            {
                LinkedListNode<TVal> n;
                if (loadedListDict.TryGetValue(key, out n))
                {
                    loadedListDict.Remove(key);
                    loadedList.Remove(n);
                    Interlocked.Add(ref CurrentMemoryUsage, -n.Value.MemoryUsage);
                }
            }
        }


        public void Compact()
        {
            lock(loadedList)
            {
                var oldlln = loadedList.First;
                while (oldlln != null)
                {
                    if ((CurrentTime - oldlln.Value.LastUseTime).TotalSeconds < CacheTime) break;
                    var nextln = oldlln.Next;
                    Interlocked.Add(ref CurrentMemoryUsage, -oldlln.Value.MemoryUsage);
                    loadedListDict.Remove(oldlln.Value.Key);
                    loadedList.Remove(oldlln); //gc should free up memory later..
                    
                    oldlln.Value = null;
                    oldlln = nextln;
                }
            }
        }


    }

    public abstract class Cacheable<TKey>
    {
        public TKey Key;
        public DateTime LastUseTime;
        public long MemoryUsage;
    }

}
