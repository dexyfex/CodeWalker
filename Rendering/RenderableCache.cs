using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using CodeWalker.World;
using System.Collections.Concurrent;
using CodeWalker.GameFiles;
using System.Threading;
using CodeWalker.Properties;

namespace CodeWalker.Rendering
{
    public class RenderableCache
    {
        public DateTime LastUpdate = DateTime.UtcNow;
        public DateTime LastUnload = DateTime.UtcNow;
        public double CacheTime = Settings.Default.GPUCacheTime;// 10.0; //seconds to keep something that's not used
        public double UnloadTime = Settings.Default.GPUCacheFlushTime;// 0.1; //seconds between running unload cycles
        public int MaxItemsPerLoop = 1; //to keep things flowing

        public long TotalGraphicsMemoryUse
        {
            get
            {
                //return (GeometryCacheUse + TextureCacheUse + BoundCompCacheUse + InstanceCacheUse);
                return (renderables.CacheUse + textures.CacheUse + boundcomps.CacheUse + instbatches.CacheUse);
            }
        }
        public int LoadedRenderableCount
        {
            get
            {
                return renderables.CurrentLoadedCount;// loadedRenderables.Count;
            }
        }
        public int LoadedTextureCount
        {
            get
            {
                return textures.CurrentLoadedCount;// loadedTextures.Count;
            }
        }
        public int MemCachedRenderableCount
        {
            get
            {
                return renderables.CurrentCacheCount;// cacheRenderables.Count;
            }
        }
        public int MemCachedTextureCount
        {
            get
            {
                return textures.CurrentCacheCount;// cacheTextures.Count;
            }
        }


        private RenderableCacheLookup<DrawableBase, Renderable> renderables = new RenderableCacheLookup<DrawableBase, Renderable>(Settings.Default.GPUGeometryCacheSize, Settings.Default.GPUCacheTime);
        private RenderableCacheLookup<Texture, RenderableTexture> textures = new RenderableCacheLookup<Texture, RenderableTexture>(Settings.Default.GPUTextureCacheSize, Settings.Default.GPUCacheTime);
        private RenderableCacheLookup<BoundComposite, RenderableBoundComposite> boundcomps = new RenderableCacheLookup<BoundComposite, RenderableBoundComposite>(Settings.Default.GPUBoundCompCacheSize, Settings.Default.GPUCacheTime);
        private RenderableCacheLookup<YmapGrassInstanceBatch, RenderableInstanceBatch> instbatches = new RenderableCacheLookup<YmapGrassInstanceBatch, RenderableInstanceBatch>(67108864, Settings.Default.GPUCacheTime); //64MB - todo: make this a setting
        private RenderableCacheLookup<YmapDistantLODLights, RenderableDistantLODLights> distlodlights = new RenderableCacheLookup<YmapDistantLODLights, RenderableDistantLODLights>(33554432, Settings.Default.GPUCacheTime); //32MB - todo: make this a setting
        private RenderableCacheLookup<BasePathData, RenderablePathBatch> pathbatches = new RenderableCacheLookup<BasePathData, RenderablePathBatch>(536870912 /*33554432*/, Settings.Default.GPUCacheTime); // 512MB /*32MB*/ - todo: make this a setting
        private RenderableCacheLookup<WaterQuad, RenderableWaterQuad> waterquads = new RenderableCacheLookup<WaterQuad, RenderableWaterQuad>(4194304, Settings.Default.GPUCacheTime); //4MB - todo: make this a setting


        private object updateSyncRoot = new object();

        private Device currentDevice;


        public void OnDeviceCreated(Device device)
        {
            currentDevice = device;
        }
        public void OnDeviceDestroyed()
        {
            currentDevice = null;

            renderables.Clear();
            textures.Clear();
            boundcomps.Clear();
            instbatches.Clear();
            distlodlights.Clear();
            pathbatches.Clear();
            waterquads.Clear();
        }

        public bool ContentThreadProc()
        {
            if (currentDevice == null) return false; //can't do anything with no device

            Monitor.Enter(updateSyncRoot);


            //load the queued items if possible
            int renderablecount = renderables.LoadProc(currentDevice, MaxItemsPerLoop);
            int texturecount = textures.LoadProc(currentDevice, MaxItemsPerLoop);
            int boundcompcount = boundcomps.LoadProc(currentDevice, MaxItemsPerLoop);
            int instbatchcount = instbatches.LoadProc(currentDevice, MaxItemsPerLoop);
            int distlodlightcount = distlodlights.LoadProc(currentDevice, MaxItemsPerLoop);
            int pathbatchcount = pathbatches.LoadProc(currentDevice, MaxItemsPerLoop);
            int waterquadcount = waterquads.LoadProc(currentDevice, MaxItemsPerLoop);


            bool itemsStillPending = 
                (renderablecount >= MaxItemsPerLoop) ||
                (texturecount >= MaxItemsPerLoop) ||
                (boundcompcount >= MaxItemsPerLoop) ||
                (instbatchcount >= MaxItemsPerLoop) ||
                (distlodlightcount >= MaxItemsPerLoop) ||
                (pathbatchcount >= MaxItemsPerLoop) ||
                (waterquadcount >= MaxItemsPerLoop);


            //todo: change this to unload only when necessary (ie when something is loaded)
            var now = DateTime.UtcNow;
            var deltat = (now - LastUpdate).TotalSeconds;
            var unloadt = (now - LastUnload).TotalSeconds;
            if ((unloadt > UnloadTime) && (deltat < 0.25)) //don't try the unload on every loop... or when really busy
            {

                //unload items that haven't been used in longer than the cache period.
                renderables.UnloadProc();
                textures.UnloadProc();
                boundcomps.UnloadProc();
                instbatches.UnloadProc();
                distlodlights.UnloadProc();
                pathbatches.UnloadProc();
                waterquads.UnloadProc();

                LastUnload = DateTime.UtcNow;
            }


            LastUpdate = DateTime.UtcNow;

            Monitor.Exit(updateSyncRoot);

            return itemsStillPending;
        }

        public void RenderThreadSync()
        {
            renderables.RenderThreadSync();
            textures.RenderThreadSync();
            boundcomps.RenderThreadSync();
            instbatches.RenderThreadSync();
            distlodlights.RenderThreadSync();
            pathbatches.RenderThreadSync();
            waterquads.RenderThreadSync();
        }

        public Renderable GetRenderable(DrawableBase drawable)
        {
            return renderables.Get(drawable);
        }
        public RenderableTexture GetRenderableTexture(Texture texture)
        {
            return textures.Get(texture);
        }
        public RenderableBoundComposite GetRenderableBoundComp(BoundComposite boundcomp)
        {
            return boundcomps.Get(boundcomp);
        }
        public RenderableInstanceBatch GetRenderableInstanceBatch(YmapGrassInstanceBatch batch)
        {
            return instbatches.Get(batch);
        }
        public RenderableDistantLODLights GetRenderableDistantLODLights(YmapDistantLODLights lights)
        {
            return distlodlights.Get(lights);
        }
        public RenderablePathBatch GetRenderablePathBatch(BasePathData pathdata)
        {
            return pathbatches.Get(pathdata);
        }
        public RenderableWaterQuad GetRenderableWaterQuad(WaterQuad quad)
        {
            return waterquads.Get(quad);
        }



        public void Invalidate(BasePathData path)
        {
            lock (updateSyncRoot)
            {
                pathbatches.Invalidate(path);
            }
        }

        public void Invalidate(YmapGrassInstanceBatch batch)
        {
            lock (updateSyncRoot)
            {
                instbatches.Invalidate(batch);
            }
        }


    }


    public abstract class RenderableCacheItem<TKey>
    {
        public TKey Key;
        public volatile bool IsLoaded = false;
        public volatile bool LoadQueued = false;
        public long LastUseTime = 0;
        //public DateTime LastUseTime { get; set; }
        public long DataSize { get; set; }

        public abstract void Init(TKey key);
        public abstract void Load(Device device);
        public abstract void Unload();
    }

    public class RenderableCacheLookup<TKey, TVal> where TVal: RenderableCacheItem<TKey>, new()
    {
        private ConcurrentStack<TVal> itemsToLoad = new ConcurrentStack<TVal>();
        private ConcurrentStack<TVal> itemsToUnload = new ConcurrentStack<TVal>();
        private LinkedList<TVal> loadeditems = new LinkedList<TVal>();
        private Dictionary<TKey, TVal> cacheitems = new Dictionary<TKey, TVal>();
        public long CacheLimit;
        public long CacheUse = 0;
        public double CacheTime;
        public int LoadedCount = 0;//temporary, per loop
        private long LastFrameTime = 0;

        public RenderableCacheLookup(long limit, double time)
        {
            CacheLimit = limit;
            CacheTime = time;
            LastFrameTime = DateTime.UtcNow.ToBinary();
        }

        public int CurrentLoadedCount
        {
            get
            {
                return loadeditems.Count;
            }
        }
        public int CurrentCacheCount
        {
            get
            {
                return cacheitems.Count;
            }
        }

        public void Clear()
        {
            TVal item;
            while (itemsToLoad.TryPop(out item))
            { }
            foreach (TVal rnd in loadeditems)
            {
                rnd.Unload();
            }
            loadeditems.Clear();
            cacheitems.Clear();
            while (itemsToUnload.TryPop(out item))
            { }
        }


        public int LoadProc(Device device, int maxitemsperloop)
        {
            TVal item;
            LoadedCount = 0;
            while (itemsToLoad.TryPop(out item))
            {
                if (item.IsLoaded) continue; //don't load it again...
                LoadedCount++;
                long gcachefree = CacheLimit - CacheUse;
                if (gcachefree > item.DataSize)
                {
                    try
                    {
                        item.Load(device);
                        loadeditems.AddLast(item);
                        Interlocked.Add(ref CacheUse, item.DataSize);
                    }
                    catch //(Exception ex)
                    {
                        //todo: error handling...
                    }
                }
                else
                {
                    item.LoadQueued = false; //can try load it again later..
                }
                if (LoadedCount >= maxitemsperloop) break;
            }
            return LoadedCount;
        }

        public void UnloadProc()
        {
            //unload items that haven't been used in longer than the cache period.
            var now = DateTime.UtcNow;
            var rnode = loadeditems.First;
            while (rnode != null)
            {
                var lu = DateTime.FromBinary(Interlocked.Read(ref rnode.Value.LastUseTime));
                if ((now - lu).TotalSeconds > CacheTime)
                {
                    var nextnode = rnode.Next;
                    itemsToUnload.Push(rnode.Value);
                    loadeditems.Remove(rnode);
                    rnode = nextnode;
                }
                else
                {
                    rnode = rnode.Next;
                }
            }

        }

        public void RenderThreadSync()
        {
            LastFrameTime = DateTime.UtcNow.ToBinary();
            TVal item;
            while (itemsToUnload.TryPop(out item))
            {
                if ((item.Key != null) && (cacheitems.ContainsKey(item.Key)))
                {
                    cacheitems.Remove(item.Key);
                }
                item.Unload();
                item.LoadQueued = false;
                Interlocked.Add(ref CacheUse, -item.DataSize);
            }

        }

        public TVal Get(TKey key)
        {
            if (key == null) return null;
            TVal item = null;
            if (!cacheitems.TryGetValue(key, out item))
            {
                item = new TVal();
                item.Init(key);
                cacheitems.Add(key, item);
            }
            Interlocked.Exchange(ref item.LastUseTime, LastFrameTime);
            if ((!item.IsLoaded) && (!item.LoadQueued))// || 
            {
                item.LoadQueued = true;
                itemsToLoad.Push(item);
            }
            return item;
        }


        public void Invalidate(TKey key)
        {
            if (key == null) return;
            TVal item = null;
            if (!cacheitems.TryGetValue(key, out item)) return;
            if (item == null) return;

            if ((item.Key != null) && (cacheitems.ContainsKey(item.Key)))
            {

                cacheitems.Remove(item.Key);

                item.Unload();
                item.LoadQueued = false;
                Interlocked.Add(ref CacheUse, -item.DataSize);
                Interlocked.Exchange(ref item.LastUseTime, DateTime.UtcNow.AddHours(-1).ToBinary());

                loadeditems.Remove(item);//slow...
            }

        }

    }

}
