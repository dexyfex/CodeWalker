using CodeWalker.GameFiles;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using CodeWalker.World;
using SharpDX.Direct3D;
using SharpDX;

namespace CodeWalker.Rendering
{
    public struct RenderableInst
    {
        public Renderable Renderable;
        public Vector3 CamRel;
        public Vector3 Position;
        public Vector3 BBMin;
        public Vector3 BBMax;
        public Vector3 BSCenter;
        public float Radius;
        public float Distance;
        public Quaternion Orientation;
        public Vector3 Scale;
        public uint TintPaletteIndex;
    }
    public struct RenderableGeometryInst
    {
        public RenderableGeometry Geom;
        public RenderableInst Inst;
    }

    public struct RenderableBoundCompositeInst
    {
        public RenderableBoundComposite Renderable;
        public Vector3 CamRel;
        public Vector3 Position;
        public Quaternion Orientation;
        public Vector3 Scale;
    }
    public struct RenderableBoundGeometryInst
    {
        public RenderableBoundGeometry Geom;
        public RenderableBoundCompositeInst Inst;
    }

    public struct RenderableInstanceBatchInst
    {
        public RenderableInstanceBatch Batch;
        public Renderable Renderable;
    }


    public class Renderable : RenderableCacheItem<DrawableBase>
    {
        public YtdFile[] SDtxds;
        public YtdFile[] HDtxds;
        public bool AllTexturesLoaded = false;

        public RenderableModel[] HDModels;
        public RenderableModel[] MedModels;
        public RenderableModel[] LowModels;
        public RenderableModel[] VlowModels;
        public RenderableModel[] AllModels;
        //public Dictionary<uint, Texture> TextureDict { get; private set; }
        //public long EmbeddedTextureSize { get; private set; }

        public bool HasSkeleton;
        public bool HasTransforms;

        public bool HasAnims = false;
        public double CurrentAnimTime = 0;
        public YcdFile ClipDict;
        public ClipMapEntry ClipMapEntry;
        public Dictionary<ushort, RenderableModel> ModelBoneLinks;

        public Matrix3_s[] BoneTransforms;
        public List<Bone> Bones;

        public override void Init(DrawableBase drawable)
        {
            Key = drawable;

            DataSize = 0;

            var hd = Key.DrawableModelsHigh?.data_items ?? Key.AllModels;
            var med = Key.DrawableModelsMedium?.data_items;
            var low = Key.DrawableModelsLow?.data_items;
            var vlow = Key.DrawableModelsVeryLow?.data_items;
            int totmodels = (hd?.Length ?? 0) + (med?.Length ?? 0) + (low?.Length ?? 0) + (vlow?.Length ?? 0);
            int curmodel = hd?.Length ?? 0;
            AllModels = new RenderableModel[totmodels];
            HDModels = new RenderableModel[hd.Length];
            if (hd != null)
            {
                for (int i = 0; i < hd.Length; i++)
                {
                    HDModels[i] = InitModel(hd[i]);
                    AllModels[i] = HDModels[i];
                }
            }
            if (med != null)
            {
                MedModels = new RenderableModel[med.Length];
                for (int i = 0; i < med.Length; i++)
                {
                    MedModels[i] = InitModel(med[i]);
                    AllModels[curmodel + i] = MedModels[i];
                }
                curmodel += med.Length;
            }
            if (low != null)
            {
                LowModels = new RenderableModel[low.Length];
                for (int i = 0; i < low.Length; i++)
                {
                    LowModels[i] = InitModel(low[i]);
                    AllModels[curmodel + i] = LowModels[i];
                }
                curmodel += low.Length;
            }
            if (vlow != null)
            {
                VlowModels = new RenderableModel[vlow.Length];
                for (int i = 0; i < vlow.Length; i++)
                {
                    VlowModels[i] = InitModel(vlow[i]);
                    AllModels[curmodel + i] = VlowModels[i];
                }
                curmodel += vlow.Length;
            }


            //var sg = Drawable.ShaderGroup;
            //if ((sg != null) && (sg.TextureDictionary != null))
            //{
            //    EmbeddedTextureSize = sg.TextureDictionary.MemoryUsage;
            //    TextureDict = sg.TextureDictionary.GetDictionary();
            //}





            bool hasskeleton = false;
            bool hastransforms = false;
            bool hasbones = false;
            Skeleton skeleton = drawable.Skeleton;
            Matrix[] modeltransforms = null;
            Matrix[] fragtransforms = null;
            Vector4 fragoffset = Vector4.Zero;
            int fragtransformid = 0;
            List<Bone> bones = null;
            bool usepose = false;
            if (skeleton != null)
            {
                hasskeleton = true;
                modeltransforms = skeleton.Transformations;

                //for fragments, get the default pose from the root fragment...
                var fd = drawable as FragDrawable;
                if (fd != null)
                {
                    var frag = fd.OwnerFragment;
                    var pose = frag?.BoneTransforms;
                    if ((pose != null) && (pose.Items != null)) //seems to be the default pose
                    {
                        var posebonecount = pose.Items.Length;
                        if ((modeltransforms == null))// || (modeltransforms.Length != posebonecount))
                        {
                            modeltransforms = new Matrix[posebonecount];
                        }
                        var modelbonecount = modeltransforms.Length;
                        var maxbonecount = Math.Min(posebonecount, modelbonecount);
                        for (int i = 0; i < maxbonecount; i++)
                        {
                            var p = pose.Items[i];
                            Vector4 r1 = p.Row1;
                            Vector4 r2 = p.Row2;
                            Vector4 r3 = p.Row3;
                            modeltransforms[i] = new Matrix(r1.X, r2.X, r3.X, 0.0f, r1.Y, r2.Y, r3.Y, 0.0f, r1.Z, r2.Z, r3.Z, 0.0f, r1.W, r2.W, r3.W, 1.0f);
                        }
                        usepose = true;
                    }

                    var phys = fd.OwnerFragmentPhys;
                    if (phys != null)
                    {
                        if (phys.OwnerFragPhysLod != null)
                        {
                            fragtransforms = phys.OwnerFragPhysLod.FragTransforms?.Data;
                            fragtransformid = phys.OwnerFragPhysIndex;
                            fragoffset = phys.OwnerFragPhysLod.Unknown_30h;
                            fragoffset.W = 0.0f;


                            switch (phys.BoneTag) //right hand side wheel flip!
                            {
                                //case 27922: //wheel_lf
                                //case 29921: //wheel_lm1
                                //case 29922: //wheel_lm2
                                //case 29923: //wheel_lm3
                                //case 27902: //wheel_lr
                                case 26418: //wheel_rf
                                case 5857:  //wheel_rm1
                                case 5858:  //wheel_rm2
                                case 5859:  //wheel_rm3
                                case 26398: //wheel_rr
                                    fragtransforms[fragtransformid].M11 = -1;
                                    fragtransforms[fragtransformid].M12 = 0;
                                    fragtransforms[fragtransformid].M13 = 0;
                                    fragtransforms[fragtransformid].M21 = 0;
                                    fragtransforms[fragtransformid].M22 = 1;
                                    fragtransforms[fragtransformid].M23 = 0;
                                    fragtransforms[fragtransformid].M31 = 0;
                                    fragtransforms[fragtransformid].M32 = 0;
                                    fragtransforms[fragtransformid].M33 = -1;
                                    break;
                                default:
                                    break;
                            }

                        }
                    }
                    else if (frag != null)
                    {
                    }
                }

                hastransforms = (modeltransforms != null) || (fragtransforms != null);
                hasbones = ((skeleton.Bones != null) && (skeleton.Bones.Data != null));
                bones = hasbones ? skeleton.Bones.Data : null;
            }

            HasSkeleton = hasskeleton;
            HasTransforms = hastransforms;

            Bones = skeleton?.Bones?.Data;


            //calculate transforms for the models if there are any. (TODO: move this to a method for re-use...)
            for (int mi = 0; mi < AllModels.Length; mi++)
            {
                var model = AllModels[mi];

                model.UseTransform = hastransforms;
                if (hastransforms)
                {

                    int boneidx = model.BoneIndex;

                    Matrix trans = (boneidx < modeltransforms.Length) ? modeltransforms[boneidx] : Matrix.Identity;
                    Bone bone = (hasbones && (boneidx < bones.Count)) ? bones[boneidx] : null;

                    if (mi < HDModels.Length) //populate bone links map for hd models
                    {
                        if (bone != null)
                        {
                            if (ModelBoneLinks == null) ModelBoneLinks = new Dictionary<ushort, RenderableModel>();
                            ModelBoneLinks[bone.Tag] = model;
                        }
                    }



                    if ((fragtransforms != null))// && (fragtransformid < fragtransforms.Length))
                    {
                        if (fragtransformid < fragtransforms.Length)
                        {
                            trans = fragtransforms[fragtransformid];
                            trans.Row4 += fragoffset;
                        }
                        else
                        { }
                    }
                    else if (!usepose) //when using the skeleton's matrices, they need to be transformed by parent
                    {
                        trans.Column4 = Vector4.UnitW;
                        short[] pinds = skeleton.ParentIndices;
                        short parentind = ((pinds != null) && (boneidx < pinds.Length)) ? pinds[boneidx] : (short)-1;
                        while ((parentind >= 0) && (parentind < pinds.Length))
                        {
                            Matrix ptrans = (parentind < modeltransforms.Length) ? modeltransforms[parentind] : Matrix.Identity;
                            ptrans.Column4 = Vector4.UnitW;
                            trans = Matrix.Multiply(ptrans, trans);
                            parentind = ((pinds != null) && (parentind < pinds.Length)) ? pinds[parentind] : (short)-1;
                        }
                    }

                    if (model.IsSkinMesh)
                    {
                        model.Transform = Matrix.Identity;
                    }
                    else
                    {
                        model.Transform = trans;
                    }
                }
            }





            UpdateBoneTransforms();

        }

        private RenderableModel InitModel(DrawableModel dm)
        {
            var rmodel = new RenderableModel();
            rmodel.Owner = this;
            rmodel.Init(dm);
            DataSize += rmodel.GeometrySize;
            return rmodel;
        }

        public override void Load(Device device)
        {
            if (AllModels != null)
            {
                foreach (var model in AllModels)
                {
                    if (model.Geometries == null) continue;
                    foreach (var geom in model.Geometries)
                    {
                        geom.Load(device);
                    }
                }
            }
            //LastUseTime = DateTime.Now; //reset usage timer
            IsLoaded = true;
        }

        public override void Unload()
        {
            IsLoaded = false;
            if (AllModels != null)
            {
                foreach (var model in AllModels)
                {
                    if (model.Geometries == null) continue;
                    foreach (var geom in model.Geometries)
                    {
                        geom.Unload();
                    }
                }
            }
            LoadQueued = false;
        }

        public override string ToString()
        {
            return Key.ToString();
        }



        private void UpdateBoneTransforms()
        {
            if (Bones == null) return;
            if ((BoneTransforms == null) || (BoneTransforms.Length != Bones.Count))
            {
                BoneTransforms = new Matrix3_s[Bones.Count];
            }
            for (int i = 0; i < Bones.Count; i++)
            {
                var bone = Bones[i];
                Matrix b = bone.SkinTransform;
                Matrix3_s bt = new Matrix3_s();
                bt.Row1 = b.Column1;
                bt.Row2 = b.Column2;
                bt.Row3 = b.Column3;
                BoneTransforms[i] = bt;
            }
        }



        public void UpdateAnims(double realTime)
        {
            if (CurrentAnimTime == realTime) return;//already updated this!
            CurrentAnimTime = realTime;

            if (ClipMapEntry != null)
            {
                UpdateAnim(ClipMapEntry); //animate skeleton/models
            }

            foreach (var model in HDModels)
            {
                if (model == null) continue;
                foreach (var geom in model.Geometries)
                {
                    if (geom == null) continue;
                    if (geom.ClipMapEntryUV != null)
                    {
                        UpdateAnimUV(geom.ClipMapEntryUV, geom); //animate UVs
                    }
                }
            }

        }
        private void UpdateAnim(ClipMapEntry cme)
        {
            if (cme.Next != null)
            {
                UpdateAnim(cme.Next);
            }

            var clipanim = cme.Clip as ClipAnimation;
            if (clipanim?.Animation != null)
            {
                UpdateAnim(clipanim.Animation, clipanim.GetPlaybackTime(CurrentAnimTime));
            }

            var clipanimlist = cme.Clip as ClipAnimationList;
            if (clipanimlist?.Animations != null)
            {
                float t = clipanimlist.GetPlaybackTime(CurrentAnimTime);
                foreach (var canim in clipanimlist.Animations)
                {
                    if (canim?.Animation == null) continue;
                    UpdateAnim(canim.Animation, t*canim.Rate + canim.StartTime);
                }
            }

        }
        private void UpdateAnim(Animation anim, float t)
        { 
            if (anim == null)
            { return; }
            if (anim.BoneIds?.data_items == null)
            { return; }
            if (anim.Sequences?.data_items == null)
            { return; }

            bool interpolate = true; //how to know? eg. cs4_14_hickbar_anim shouldn't
            bool ignoreLastFrame = true;//if last frame is equivalent to the first one, eg rollercoaster small light "globes" don't

            var duration = anim.Duration;
            var frames = anim.Frames;
            var nframes = (ignoreLastFrame) ? (frames - 1) : frames;

            var curPos = (t/duration) * nframes;
            var frame0 = ((ushort)curPos) % frames;
            var frame1 = (frame0 + 1) % frames;
            var falpha = (float)(curPos - Math.Floor(curPos));
            var ialpha = 1.0f - falpha;


            var dwbl = this.Key;
            var skel = dwbl?.Skeleton;
            var bones = skel?.Bones;
            if (bones == null)
            { return; }

            Vector4 v0, v1, v;
            Quaternion q0, q1, q;

            for (int i = 0; i < anim.BoneIds.data_items.Length; i++)
            {
                var boneiditem = anim.BoneIds.data_items[i];
                var track = boneiditem.Track;

                Bone bone = null;
                skel?.BonesMap?.TryGetValue(boneiditem.BoneId, out bone);
                if (bone == null)
                { continue; }


                for (int s = 0; s < anim.Sequences.data_items.Length; s++)
                {
                    var seq = anim.Sequences.data_items[s];
                    var aseq = seq.Sequences[i];
                    switch (track)
                    {
                        case 0: //bone position
                            v0 = aseq.EvaluateVector(frame0);
                            v1 = aseq.EvaluateVector(frame1);
                            v = interpolate ? (v0 * ialpha) + (v1 * falpha) : v0;
                            bone.AnimTranslation = v.XYZ();
                            break;
                        case 1: //bone orientation
                            q0 = new Quaternion(aseq.EvaluateVector(frame0));
                            q1 = new Quaternion(aseq.EvaluateVector(frame1));
                            q = interpolate ? Quaternion.Slerp(q0, q1, falpha) : q0;
                            bone.AnimRotation = q;
                            break;
                        case 2: //scale?
                            break;
                        case 5://vector3...
                            //v0 = aseq.EvaluateVector(frame0);
                            //v1 = aseq.EvaluateVector(frame1);
                            //v = interpolate ? (v0 * ialpha) + (v1 * falpha) : v0;
                            //bone.AnimScale = v.XYZ();
                            break;
                        case 6://quaternion...
                            break;
                        case 134://single float?
                        case 136:
                        case 137:
                        case 138:
                        case 139:
                        case 140:
                            break;
                        default:
                            break;
                    }
                }
            }

            for (int i = 0; i < bones.Count; i++)
            {
                var bone = bones[i];
                bone.UpdateAnimTransform();
                bone.UpdateSkinTransform();

                //update model's transform from animated bone
                RenderableModel bmodel = null;
                ModelBoneLinks?.TryGetValue(bone.Tag, out bmodel);


                if (bmodel == null)
                { continue; }
                if (bmodel.IsSkinMesh) //don't transform model for skin mesh
                { continue; }

                bmodel.Transform = bone.AnimTransform;

            }


            UpdateBoneTransforms();

        }
        private void UpdateAnimUV(ClipMapEntry cme, RenderableGeometry rgeom = null)
        {
            if (cme.Next != null)
            {
                UpdateAnimUV(cme.Next, rgeom);
            }

            var clipanim = cme.Clip as ClipAnimation;
            if (clipanim?.Animation != null)
            {
                UpdateAnimUV(clipanim.Animation, clipanim.GetPlaybackTime(CurrentAnimTime), rgeom);
            }

            var clipanimlist = cme.Clip as ClipAnimationList;
            if (clipanimlist?.Animations != null)
            {
                float t = clipanimlist.GetPlaybackTime(CurrentAnimTime);
                foreach (var canim in clipanimlist.Animations)
                {
                    if (canim?.Animation == null) continue;
                    UpdateAnimUV(canim.Animation, t*canim.Rate + canim.StartTime, rgeom);
                }
            }

        }
        private void UpdateAnimUV(Animation anim, float t, RenderableGeometry rgeom = null)
        {
            if (anim == null)
            { return; }
            if (anim.BoneIds?.data_items == null)
            { return; }
            if (anim.Sequences?.data_items == null)
            { return; }

            bool interpolate = true; //how to know? eg. cs4_14_hickbar_anim shouldn't
            bool ignoreLastFrame = true;//if last frame is equivalent to the first one, eg rollercoaster small light "globes" don't

            var duration = anim.Duration;
            var frames = anim.Frames;
            var nframes = (ignoreLastFrame) ? (frames - 1) : frames;

            var curPos = (t / duration) * nframes;
            var frame0 = ((ushort)curPos) % frames;
            var frame1 = (frame0 + 1) % frames;
            var falpha = (float)(curPos - Math.Floor(curPos));
            var ialpha = 1.0f - falpha;

            var globalAnimUV0 = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
            var globalAnimUV1 = new Vector4(0.0f, 1.0f, 0.0f, 0.0f);


            for (int i = 0; i < anim.BoneIds.data_items.Length; i++)
            {
                var boneiditem = anim.BoneIds.data_items[i];
                var track = boneiditem.Track;
                if ((track != 17) && (track != 18))
                { continue; }//17 and 18 would be UV0 and UV1

                for (int s = 0; s < anim.Sequences.data_items.Length; s++)
                {
                    var seq = anim.Sequences.data_items[s];
                    var aseq = seq.Sequences[i];
                    var v0 = aseq.EvaluateVector(frame0);
                    var v1 = aseq.EvaluateVector(frame1);
                    var v = interpolate ? (v0 * ialpha) + (v1 * falpha) : v0;
                    switch (track)
                    {
                        case 17: globalAnimUV0 = v; break; //could be overwriting values here...
                        case 18: globalAnimUV1 = v; break;
                    }
                }
            }

            if (rgeom != null)
            {
                rgeom.globalAnimUV0 = globalAnimUV0;
                rgeom.globalAnimUV1 = globalAnimUV1;
            }
            else
            {
                foreach (var model in HDModels) //TODO: figure out which models/geometries this should be applying to!
                {
                    if (model == null) continue;
                    foreach (var geom in model.Geometries)
                    {
                        if (geom == null) continue;
                        if (geom.globalAnimUVEnable)
                        {
                            geom.globalAnimUV0 = globalAnimUV0;
                            geom.globalAnimUV1 = globalAnimUV1;
                        }
                    }
                }
            }

        }

    }

    public class RenderableModel
    {
        public Renderable Owner;
        public DrawableModel DrawableModel;
        public RenderableGeometry[] Geometries;
        public AABB_s[] GeometryBounds;
        public long GeometrySize { get; private set; }

        public uint SkeletonBinding;
        public uint RenderMaskFlags; //flags.......

        public bool UseTransform;
        public Matrix Transform;

        public int BoneIndex = 0;
        public bool IsSkinMesh = false;

        public void Init(DrawableModel dmodel)
        {
            SkeletonBinding = dmodel.SkeletonBinding;//4th byte is bone index, 2nd byte for skin meshes
            RenderMaskFlags = dmodel.RenderMaskFlags; //only the first byte seems be related to this

            IsSkinMesh = ((SkeletonBinding >> 8) & 0xFF) > 0;
            BoneIndex = (int)((SkeletonBinding >> 24) & 0xFF);

            DrawableModel = dmodel;
            long geomcount = dmodel.Geometries.data_items.Length;
            Geometries = new RenderableGeometry[geomcount];
            GeometryBounds = new AABB_s[geomcount];

            GeometrySize = 0;
            for (int i = 0; i < geomcount; i++)
            {
                var dgeom = dmodel.Geometries.data_items[i];
                var rgeom = new RenderableGeometry();
                rgeom.Init(dgeom);
                rgeom.Owner = this;
                Geometries[i] = rgeom;
                GeometrySize += rgeom.TotalDataSize;

                if ((dmodel.BoundsData != null) && (i < dmodel.BoundsData.Length))
                {
                    GeometryBounds[i] = dmodel.BoundsData[i];
                }
                else
                {
                    //GeometryBounds[i] = new AABB_s();//what to default to?
                }

                if (Owner.Key is FragDrawable)
                {
                    rgeom.IsFragment = true;
                }
            }


        }

    }

    public class RenderableGeometry
    {
        public RenderableModel Owner;
        public Buffer VertexBuffer { get; set; }
        public Buffer IndexBuffer { get; set; }
        public VertexBufferBinding VBBinding;
        public DrawableGeometry DrawableGeom;
        public VertexType VertexType { get; set; }
        public int VertexStride { get; set; }
        public int VertexCount { get; set; }
        public int IndexCount { get; set; }
        public uint VertexDataSize { get; set; }
        public uint IndexDataSize { get; set; }
        public uint TotalDataSize { get; set; }
        public TextureBase[] Textures;
        public Texture[] TexturesHD;
        public RenderableTexture[] RenderableTextures;
        public RenderableTexture[] RenderableTexturesHD;
        public ShaderParamNames[] TextureParamHashes;
        public PrimitiveTopology Topology { get; set; }
        public bool IsFragment = false;
        public bool IsEmissive { get; set; } = false;
        public bool EnableWind { get; set; } = false;
        public float HardAlphaBlend { get; set; } = 0.0f;
        public float useTessellation { get; set; } = 0.0f;
        public float wetnessMultiplier { get; set; } = 0.0f;
        public float bumpiness { get; set; } = 1.0f;
        public Vector4 detailSettings { get; set; } = Vector4.Zero;
        public Vector3 specMapIntMask { get; set; } = Vector3.Zero;
        public float specularIntensityMult { get; set; } = 0.0f;
        public float specularFalloffMult { get; set; } = 0.0f;
        public float specularFresnel { get; set; } = 0.0f;
        public float RippleSpeed { get; set; } = 1.0f;
        public float RippleScale { get; set; } = 1.0f;
        public float RippleBumpiness { get; set; } = 1.0f;
        public Vector4 WindGlobalParams { get; set; } = Vector4.Zero;
        public Vector4 WindOverrideParams { get; set; } = Vector4.One;
        public Vector4 globalAnimUV0 { get; set; } = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
        public Vector4 globalAnimUV1 { get; set; } = new Vector4(0.0f, 1.0f, 0.0f, 0.0f);
        public Vector4 DirtDecalMask { get; set; } = Vector4.Zero;
        public bool SpecOnly { get; set; } = false;
        public float WaveOffset { get; set; } = 0; //for terrainfoam
        public float WaterHeight { get; set; } = 0; //for terrainfoam
        public float WaveMovement { get; set; } = 0; //for terrainfoam
        public float HeightOpacity { get; set; } = 0; //for terrainfoam
        public bool HDTextureEnable = true;
        public bool globalAnimUVEnable = false;
        public ClipMapEntry ClipMapEntryUV = null;
        public bool isHair = false;
        public bool disableRendering = false;

        public static ShaderParamNames[] GetTextureSamplerList()
        {
            return new ShaderParamNames[]
            {
                ShaderParamNames.DiffuseSampler, //base diffuse
                ShaderParamNames.SpecSampler, //base specular
                ShaderParamNames.BumpSampler, //base normal
                ShaderParamNames.TintPaletteSampler, // _pal
                ShaderParamNames.DetailSampler, // ENV_
                ShaderParamNames.FlowSampler, //river _flow
                ShaderParamNames.FogSampler, //river _fog , water slod
                ShaderParamNames.TextureSampler_layer0, //CS_RSN_SL_Road_0007
                ShaderParamNames.BumpSampler_layer0, //CS_RSN_SL_Road_0007_n
                ShaderParamNames.heightMapSamplerLayer0, //nxg_cs_rsn_sl_road_0007_h
                ShaderParamNames.TextureSampler_layer1, //IM_Road_009b
                ShaderParamNames.BumpSampler_layer1, //IM_Road_010b_N
                ShaderParamNames.heightMapSamplerLayer1, //nxg_im_road_010b_h
                ShaderParamNames.TextureSampler_layer2, //IM_Concrete10
                ShaderParamNames.BumpSampler_layer2, //IM_Concrete13_N
                ShaderParamNames.heightMapSamplerLayer2, //nxg_im_concrete13_h
                ShaderParamNames.TextureSampler_layer3, //SC1_RSN_NS_ground_0009
                ShaderParamNames.BumpSampler_layer3, //sc1_rsn_ns_ground_0010_n
                ShaderParamNames.heightMapSamplerLayer3, //nxg_sc1_rsn_ns_ground_0010_b_h
                ShaderParamNames.lookupSampler, //TF_RSN_Msk_CS1_DesHill1, bh1_43_golf_blendmap_04_LOD
                ShaderParamNames.heightSampler, //nxg_prop_tree_palm2_displ_l
                ShaderParamNames.FoamSampler, //bj_beachfoam01_lod, CS_RSN_SL_RiverFoam_01_A_lodCS_RSN_SL_RiverFoam_01_A
                ShaderParamNames.DirtSampler,
                ShaderParamNames.DirtBumpSampler,
                ShaderParamNames.DiffuseSampler2,
                ShaderParamNames.DiffuseSampler3,
                ShaderParamNames.DiffuseHfSampler,
                ShaderParamNames.ComboHeightSamplerFur01,
                ShaderParamNames.ComboHeightSamplerFur23,
                ShaderParamNames.ComboHeightSamplerFur45,
                ShaderParamNames.ComboHeightSamplerFur67,
                ShaderParamNames.StippleSampler,
                ShaderParamNames.FurMaskSampler,
                ShaderParamNames.EnvironmentSampler,
                ShaderParamNames.distanceMapSampler,
                ShaderParamNames.textureSamp,
            };
        }

        public void Init(DrawableGeometry dgeom)
        {
            DrawableGeom = dgeom;
            VertexType = dgeom.VertexData.VertexType;
            VertexStride = dgeom.VertexStride;
            VertexCount = dgeom.VerticesCount;
            IndexCount = (int)dgeom.IndicesCount;
            VertexDataSize = (uint)(VertexCount * VertexStride);
            IndexDataSize = (uint)(IndexCount * 2); //ushort indices...
            TotalDataSize = VertexDataSize + IndexDataSize;
            Topology = PrimitiveTopology.TriangleList;

            var shader = DrawableGeom.Shader;
            if ((shader != null) && (shader.ParametersList != null))
            {
                if (shader.FileName == 3854885487)//{cable.sps}
                {
                    Topology = PrimitiveTopology.LineList;
                }


                var shaderName = shader.Name;
                var shaderFile = shader.FileName;
                switch (shaderFile.Hash)
                {
                    case 2245870123: //trees_normal_diffspec_tnt.sps
                    case 3334613197: //trees_tnt.sps
                    case 1229591973://{trees_normal_spec_tnt.sps}
                    case 2322653400://{trees.sps}
                    case 3192134330://{trees_normal.sps}
                    case 1224713457://{trees_normal_spec.sps}
                    case 4265705004://{trees_normal_diffspec.sps}
                    case 1581835696://{default_um.sps}
                    case 3326705511://{normal_um.sps}
                    case 3085209681://{normal_spec_um.sps}
                    case 3190732435://{cutout_um.sps}
                    case 748520668://{normal_cutout_um.sps}
                        EnableWind = true;
                        break;
                    case 1332909972://{normal_spec_emissive.sps}
                    case 2072061694://{normal_spec_reflect_emissivenight.sps}
                    case 2635608835://{emissive.sps}
                    case 443538781://{emissive_clip.sps}
                    case 2049580179://{emissive_speclum.sps}
                    case 1193295596://{emissive_tnt.sps}
                    case 1434302180://{emissivenight.sps}
                    case 1897917258://{emissivenight_geomnightonly.sps}
                    case 140448747://{emissivestrong.sps}
                    case 1436689415://{normal_spec_reflect_emissivenight_alpha.sps}
                    case 179247185://{emissive_alpha.sps}
                    case 1314864030://{emissive_alpha_tnt.sps}
                    case 1478174766://{emissive_additive_alpha.sps}
                    case 3733846327://{emissivenight_alpha.sps}
                    case 3174327089://{emissivestrong_alpha.sps}
                    case 3924045432://{glass_emissive.sps}
                    case 837003310://{glass_emissivenight.sps}
                    case 485710087://{glass_emissivenight_alpha.sps}
                    case 2055615352://{glass_emissive_alpha.sps}
                    case 2918136469://{decal_emissive_only.sps}
                    case 2698880237://{decal_emissivenight_only.sps}
                        IsEmissive = true;
                        break;
                    case 3880384844://{decal_spec_only.sps}
                    case 341123999://{decal_normal_only.sps}
                    case 600733812://{decal_amb_only.sps}
                        SpecOnly = true; //this needs more work.
                        break;
                    case 100720695://{ped_hair_spiked.sps}
                        isHair = true;
                        break;
                }



                var pl = shader.ParametersList.Parameters;
                var hl = shader.ParametersList.Hashes;
                List<TextureBase> texs = new List<TextureBase>();
                List<ShaderParamNames> phashes = new List<ShaderParamNames>();
                if ((pl != null) && (hl != null))
                {
                    for (int i = 0; (i < pl.Length) && (i < hl.Length); i++)
                    {
                        ShaderParamNames pName = (ShaderParamNames)hl[i];
                        var param = pl[i];
                        if (param.Data is TextureBase)
                        {
                            texs.Add(param.Data as TextureBase);
                            phashes.Add(pName);
                        }

                        switch (pName)
                        {
                            case ShaderParamNames.HardAlphaBlend:
                                HardAlphaBlend = ((Vector4)param.Data).X;
                                break;
                            case ShaderParamNames.useTessellation:
                                useTessellation = ((Vector4)param.Data).X;
                                break;
                            case ShaderParamNames.wetnessMultiplier:
                                wetnessMultiplier = ((Vector4)param.Data).X;
                                break;
                            case ShaderParamNames.bumpiness: //float
                                bumpiness = ((Vector4)param.Data).X;
                                break;
                            case ShaderParamNames.detailSettings: //float4
                                detailSettings = (Vector4)param.Data;
                                break;
                            case ShaderParamNames.specMapIntMask: //float3
                                specMapIntMask = ((Vector4)param.Data).XYZ();
                                break;
                            case ShaderParamNames.specularIntensityMult: //float
                                specularIntensityMult = ((Vector4)param.Data).X;
                                break;
                            case ShaderParamNames.specularFalloffMult: //float
                                specularFalloffMult = ((Vector4)param.Data).X;
                                break;
                            case ShaderParamNames.specularFresnel: //float
                                specularFresnel= ((Vector4)param.Data).X;
                                break;
                            case ShaderParamNames.WindGlobalParams:
                            case ShaderParamNames.umGlobalOverrideParams:
                                //WindOverrideParams = ((Vector4)param.Data); //todo...
                                break;
                            case ShaderParamNames.umGlobalParams:
                                WindGlobalParams = ((Vector4)param.Data);
                                break;
                            case ShaderParamNames.RippleSpeed:
                                RippleSpeed = ((Vector4)param.Data).X;
                                break;
                            case ShaderParamNames.RippleScale:
                                RippleScale = ((Vector4)param.Data).X;
                                break;
                            case ShaderParamNames.RippleBumpiness:
                                RippleBumpiness = ((Vector4)param.Data).X;
                                break;
                            case ShaderParamNames.globalAnimUV0:
                                globalAnimUV0 = (Vector4)param.Data;
                                globalAnimUVEnable = true;
                                break;
                            case ShaderParamNames.globalAnimUV1:
                                globalAnimUV1 = (Vector4)param.Data;
                                globalAnimUVEnable = true;
                                break;
                            case ShaderParamNames.WaveOffset:
                                WaveOffset = ((Vector4)param.Data).X;
                                break;
                            case ShaderParamNames.WaterHeight:
                                WaterHeight = ((Vector4)param.Data).X;
                                break;
                            case ShaderParamNames.WaveMovement:
                                WaveMovement = ((Vector4)param.Data).X;
                                break;
                            case ShaderParamNames.HeightOpacity:
                                HeightOpacity = ((Vector4)param.Data).X;
                                break;
                            case ShaderParamNames.DirtDecalMask:
                                DirtDecalMask = ((Vector4)param.Data);
                                break;
                            case ShaderParamNames.orderNumber:
                                //stops drawing hair geoms that apparently shouldn't be rendered... any better way to do this?
                                if (isHair && (((Vector4)param.Data).X > 0.0f)) disableRendering = true;
                                break;
                        }

                    }
                }
                if (texs.Count > 0)
                {
                    TextureParamHashes = phashes.ToArray();
                    Textures = texs.ToArray();
                    TexturesHD = new Texture[texs.Count];
                    RenderableTextures = new RenderableTexture[texs.Count]; //these will get populated at render time.
                    RenderableTexturesHD = new RenderableTexture[texs.Count]; //these will get populated at render time.
                }
            }


        }

        public void Load(Device device)
        {

            VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, DrawableGeom.VertexData.VertexBytes);

            //object v = DrawableGeom.VertexData.Vertices;
            //switch (VertexType)
            //{
            //    case VertexType.Default:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypeDefault[]);
            //        break; //P,N,C,T
            //    case VertexType.DefaultEx:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypeDefaultEx[]);
            //        break; //P,N,C,T,Ext
            //    case VertexType.PNCCT:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePNCCT[]);
            //        break;
            //    case VertexType.PNCCTTTT:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePNCCTTTT[]);
            //        break;
            //    case VertexType.PCCNCCTTX:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePCCNCCTTX[]);
            //        break;
            //    case VertexType.PCCNCCT:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePCCNCCT[]);
            //        break;
            //    case VertexType.PNCTTTX:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePNCTTTX[]);
            //        break;
            //    case VertexType.PNCTTTX_2:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePNCTTTX_2[]);
            //        break;
            //    case VertexType.PNCTTTX_3:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePNCTTTX_3[]);
            //        break;
            //    case VertexType.PNCTTX:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePNCTTX[]);
            //        break;
            //    case VertexType.PNCCTTX:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePNCCTTX[]);
            //        break;
            //    case VertexType.PNCCTTX_2:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePNCCTTX_2[]);
            //        break;
            //    case VertexType.PNCCTTTX:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePNCCTTTX[]);
            //        break;
            //    case VertexType.PCCNCCTX:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePCCNCCTX[]);
            //        break;
            //    case VertexType.PCCNCTX:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePCCNCTX[]);
            //        break;
            //    case VertexType.PCCNCT:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePCCNCT[]);
            //        break;
            //    case VertexType.PNCCTT:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePNCCTT[]);
            //        break;
            //    case VertexType.PNCCTX:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePNCCTX[]);
            //        break;
            //    case VertexType.PTT:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePTT[]);
            //        break;
            //    case VertexType.PNC:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePNC[]);
            //        break;
            //    case VertexType.PCT:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePCT[]);
            //        break;
            //    case VertexType.PT:
            //        VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, v as VertexTypePT[]);
            //        break;
            //    default:
            //        break;
            //}
            if (VertexBuffer != null)
            {
                VBBinding = new VertexBufferBinding(VertexBuffer, VertexStride, 0);
            }

            if (DrawableGeom.IndexBuffer != null)
            {
                IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, DrawableGeom.IndexBuffer.Indices);
            }
            else if (DrawableGeom.BoneIds != null)
            {
                IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, DrawableGeom.BoneIds);
            }

        }

        public void Unload()
        {
            if (VertexBuffer != null)
            {
                VBBinding.Buffer = null;
                VertexBuffer.Dispose();
                VertexBuffer = null;
            }
            if (IndexBuffer != null)
            {
                IndexBuffer.Dispose();
                IndexBuffer = null;
            }
            //DrawableGeom = null;

            if (RenderableTextures != null)
            {
                for (int i = 0; i < RenderableTextures.Length; i++)
                {
                    RenderableTextures[i] = null;
                }
                RenderableTextures = null;
            }
            if (RenderableTexturesHD != null)
            {
                for (int i = 0; i < RenderableTexturesHD.Length; i++)
                {
                    RenderableTexturesHD[i] = null;
                }
                RenderableTexturesHD = null;
            }

        }

        public void Render(DeviceContext context)
        {
            if ((VertexBuffer == null) || (IndexBuffer == null))
            {
                return;
            }

            context.InputAssembler.PrimitiveTopology = Topology;
            context.InputAssembler.SetVertexBuffers(0, VBBinding);
            context.InputAssembler.SetIndexBuffer(IndexBuffer, SharpDX.DXGI.Format.R16_UInt, 0);

            context.DrawIndexed(IndexCount, 0, 0);
        }

        public void RenderInstanced(DeviceContext context, int instCount)
        {
            if ((VertexBuffer == null) || (IndexBuffer == null))
            {
                return;
            }

            context.InputAssembler.PrimitiveTopology = Topology;
            context.InputAssembler.SetVertexBuffers(0, VBBinding);
            context.InputAssembler.SetIndexBuffer(IndexBuffer, SharpDX.DXGI.Format.R16_UInt, 0);

            context.DrawIndexedInstanced(IndexCount, instCount, 0, 0, 0);
        }

    }

    public class RenderableTexture : RenderableCacheItem<Texture>
    {
        public uint Hash { get; private set; }
        public string Name { get; private set; }
        public Texture2D Texture2D { get; set; }
        public ShaderResourceView ShaderResourceView { get; set; }


        public override void Init(Texture tex)
        {
            Key = tex;

            if ((Key != null) && (Key.Data != null) && (Key.Data.FullData != null))
            {
                DataSize = Key.Data.FullData.Length;
            }

        }

        public override void Load(Device device)
        {
            if ((Key != null) && (Key.Data != null) && (Key.Data.FullData != null))
            {
                using (var stream = DataStream.Create(Key.Data.FullData, true, false))
                {

                    var format = TextureFormats.GetDXGIFormat(Key.Format);
                    var width = Key.Width;
                    var height = Key.Height;
                    int mips = Key.Levels;
                    int rowpitch, slicepitch;
                    var totlength = Key.Data.FullData.Length;
                    int pxsize = TextureFormats.ByteSize(Key.Format); // SharpDX.DXGI.FormatHelper.SizeOfInBytes(desc.Format);

                    //get databoxes for mips
                    int offset = 0;
                    int level = 1;
                    List<DataBox> boxes = new List<DataBox>();
                    for (int i = 0; i < mips; i++)
                    {
                        if (offset >= totlength) break; //only load as many mips as there are..

                        var mipw = width / level;
                        var miph = height / level;

                        TextureFormats.ComputePitch(format, mipw, miph, out rowpitch, out slicepitch, 0);
                        var mipbox = new DataBox(stream.DataPointer + offset, rowpitch, slicepitch);
                        boxes.Add(mipbox);

                        offset += slicepitch;
                        level *= 2;
                    }
                    mips = boxes.Count;


                    //single mip..
                    //TextureFormats.ComputePitch(format, width, height, out rowpitch, out slicepitch, 0);
                    //var box = new DataBox(stream.DataPointer, rowpitch, slicepitch);


                    var desc = new Texture2DDescription()
                    {
                        ArraySize = 1,
                        BindFlags = BindFlags.ShaderResource,
                        CpuAccessFlags = CpuAccessFlags.None,
                        Format = format,
                        Height = Key.Height,
                        MipLevels = mips,//Texture.Levels,
                        OptionFlags = ResourceOptionFlags.None,
                        SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                        Usage = ResourceUsage.Default,
                        Width = Key.Width
                    };


                    try
                    {
                        //Texture2D = new Texture2D(device, desc, new[] { box }); //single mip
                        Texture2D = new Texture2D(device, desc, boxes.ToArray()); //multiple mips
                        ShaderResourceView = new ShaderResourceView(device, Texture2D);
                    }
                    catch //(Exception ex)
                    {
                        //string str = ex.ToString(); //todo: don't fail silently..
                    }
                }
            }


            //LastUseTime = DateTime.Now; //reset usage timer
            IsLoaded = true;
        }

        public void SetVSResource(DeviceContext context, int slot)
        {
            context.VertexShader.SetShaderResource(slot, ShaderResourceView);
            //LastUseTime = DateTime.Now;
        }
        public void SetPSResource(DeviceContext context, int slot)
        {
            context.PixelShader.SetShaderResource(slot, ShaderResourceView);
            //LastUseTime = DateTime.Now;
        }

        public override void Unload()
        {
            IsLoaded = false;
            if (ShaderResourceView != null)
            {
                ShaderResourceView.Dispose();
                ShaderResourceView = null;
            }
            if (Texture2D != null)
            {
                Texture2D.Dispose();
                Texture2D = null;
            }
            LoadQueued = false;
        }

        public override string ToString()
        {
            return (Key != null) ? Key.ToString() : base.ToString();
        }
    }


    public class RenderableInstanceBatch : RenderableCacheItem<YmapGrassInstanceBatch>
    {
        public rage__fwGrassInstanceListDef__InstanceData[] GrassInstanceData { get; set; }
        public GpuSBuffer<rage__fwGrassInstanceListDef__InstanceData> GrassInstanceBuffer { get; set; }
        public int InstanceCount { get; set; }
        public Vector3 AABBMin { get; set; }
        public Vector3 AABBMax { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 CamRel { get; set; }

        public override void Init(YmapGrassInstanceBatch batch)
        {
            Key = batch;
            if (batch.Instances == null)
            {
                return;
            }

            InstanceCount = batch.Instances.Length;

            DataSize = (InstanceCount * 16);

            GrassInstanceData = batch.Instances;

        }

        public override void Load(Device device)
        {
            if (Key != null)
            {
                AABBMin = Key.AABBMin;
                AABBMax = Key.AABBMax;
                Position = Key.Position;
            }
            if ((GrassInstanceData != null) && (GrassInstanceData.Length > 0))
            {
                GrassInstanceBuffer = new GpuSBuffer<rage__fwGrassInstanceListDef__InstanceData>(device, GrassInstanceData);
            }
            //LastUseTime = DateTime.Now; //reset usage timer
            IsLoaded = true;
        }

        public override void Unload()
        {
            IsLoaded = false;
            if (GrassInstanceBuffer != null)
            {
                GrassInstanceBuffer.Dispose();
                GrassInstanceBuffer = null;
            }
            LoadQueued = false;
        }
    }

    public class RenderableDistantLODLights : RenderableCacheItem<YmapDistantLODLights>
    {
        public struct DistLODLight
        {
            public Vector3 Position;
            public uint Colour;
        }

        private DistLODLight[] InstanceData { get; set; } 
        public GpuSBuffer<DistLODLight> InstanceBuffer { get; set; }
        public int InstanceCount { get; set; }
        public ushort Category { get; set; }
        public ushort NumStreetLights { get; set; }
        public RenderableTexture Texture { get; set; }

        public override void Init(YmapDistantLODLights key)
        {
            Key = key;
            if ((key.positions == null) || (key.colours == null))
            {
                return;
            }

            InstanceCount = Math.Min(key.positions.Length, key.colours.Length);

            DataSize = InstanceCount * 16;

            InstanceData = new DistLODLight[InstanceCount];
            for (int i = 0; i < InstanceCount; i++)
            {
                InstanceData[i].Position = key.positions[i].ToVector3();
                InstanceData[i].Colour = key.colours[i];
            }

            Category = key.CDistantLODLight.category;
            NumStreetLights = key.CDistantLODLight.numStreetLights;

        }

        public override void Load(Device device)
        {
            if ((InstanceData != null) && (InstanceData.Length > 0))
            {
                InstanceBuffer = new GpuSBuffer<DistLODLight>(device, InstanceData);
            }
            //LastUseTime = DateTime.Now; //reset usage timer
            IsLoaded = true;
        }

        public override void Unload()
        {
            IsLoaded = false;
            if (InstanceBuffer != null)
            {
                InstanceBuffer.Dispose();
                InstanceBuffer = null;
            }
        }

    }


    public class RenderablePathBatch : RenderableCacheItem<BasePathData>
    {
        public int VertexStride { get { return 16; } }

        public EditorVertex[] PathVertices;
        public int PathVertexCount { get; set; }
        public Buffer PathVertexBuffer { get; set; }
        public VertexBufferBinding PathVBBinding;

        public EditorVertex[] TriangleVertices;
        public int TriangleVertexCount { get; set; }
        public Buffer TriangleVertexBuffer { get; set; }
        public VertexBufferBinding TriangleVBBinding;

        public Vector4[] Nodes;
        public GpuSBuffer<Vector4> NodeBuffer { get; set; }

        public override void Init(BasePathData key)
        {
            Key = key;

            DataSize = 0;
            PathVertices = key.GetPathVertices();
            if (PathVertices != null)
            {
                PathVertexCount = PathVertices.Length;
                DataSize = PathVertices.Length * VertexStride;
            }

            TriangleVertices = key.GetTriangleVertices();
            if (TriangleVertices != null)
            {
                TriangleVertexCount = TriangleVertices.Length;
                DataSize += TriangleVertices.Length * VertexStride;
            }

            Nodes = key.GetNodePositions();
            if (Nodes != null)
            {
                DataSize += Nodes.Length * 16;//sizeof(Vector4)
            }

        }

        public override void Load(Device device)
        {
            if ((PathVertices != null) && (PathVertices.Length > 0))
            {
                PathVertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, PathVertices);
                if (PathVertexBuffer != null)
                {
                    PathVBBinding = new VertexBufferBinding(PathVertexBuffer, VertexStride, 0);
                }
            }

            if ((TriangleVertices != null) && (TriangleVertices.Length > 0))
            {
                TriangleVertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, TriangleVertices);
                if (TriangleVertexBuffer != null)
                {
                    TriangleVBBinding = new VertexBufferBinding(TriangleVertexBuffer, VertexStride, 0);
                }
            }

            if ((Nodes != null) && (Nodes.Length > 0))
            {
                NodeBuffer = new GpuSBuffer<Vector4>(device, Nodes);
            }

            //LastUseTime = DateTime.Now; //reset usage timer
            IsLoaded = true;
        }

        public override void Unload()
        {
            IsLoaded = false;
            if (PathVertexBuffer != null)
            {
                PathVBBinding.Buffer = null;
                PathVertexBuffer.Dispose();
                PathVertexBuffer = null;
            }
            if (TriangleVertexBuffer != null)
            {
                TriangleVBBinding.Buffer = null;
                TriangleVertexBuffer.Dispose();
                TriangleVertexBuffer = null;
            }
            if (NodeBuffer != null)
            {
                NodeBuffer.Dispose();
                NodeBuffer = null;
            }
        }
    }


    public class RenderableWaterQuad : RenderableCacheItem<WaterQuad>
    {
        public VertexTypePCT[] Vertices;
        public uint[] Indices;
        public int IndexCount { get; set; }
        public int VertexCount { get; set; }
        public int VertexStride { get; set; } = 24;
        public Buffer VertexBuffer { get; set; }
        public Buffer IndexBuffer { get; set; }
        public VertexBufferBinding VBBinding;
        public Vector3 CamRel { get; set; } //verts are in world space, so camrel should just be -campos

        public override void Init(WaterQuad key)
        {
            Key = key;

            float sx = key.maxX - key.minX;
            float sy = key.maxY - key.minY;

            VertexCount = 4;
            Vertices = new VertexTypePCT[4];
            Vertices[0].Position = new Vector3(key.minX, key.minY, key.z);
            Vertices[0].Texcoord = new Vector2(0.0f, 0.0f);
            Vertices[0].Colour = (uint)new Color4(key.a1 / 255.0f).ToRgba();
            Vertices[1].Position = new Vector3(key.maxX, key.minY, key.z);
            Vertices[1].Texcoord = new Vector2(sx, 0.0f);
            Vertices[1].Colour = (uint)new Color4(key.a2 / 255.0f).ToRgba();
            Vertices[2].Position = new Vector3(key.minX, key.maxY, key.z);
            Vertices[2].Texcoord = new Vector2(0.0f, sy);
            Vertices[2].Colour = (uint)new Color4(key.a3 / 255.0f).ToRgba();
            Vertices[3].Position = new Vector3(key.maxX, key.maxY, key.z);
            Vertices[3].Texcoord = new Vector2(sx, sy);
            Vertices[3].Colour = (uint)new Color4(key.a4 / 255.0f).ToRgba();

            if (key.Type == 0)
            {
                IndexCount = 6;
                Indices = new uint[6];
                Indices[0] = 0;
                Indices[1] = 2;
                Indices[2] = 1;
                Indices[3] = 1;
                Indices[4] = 2;
                Indices[5] = 3;
            }
            else
            {
                IndexCount = 3;
                Indices = new uint[3];
                switch (key.Type)
                {
                    case 1:
                        Indices[0] = 0;
                        Indices[1] = 1;
                        Indices[2] = 2;
                        break;
                    case 2:
                        Indices[0] = 0;
                        Indices[1] = 3;
                        Indices[2] = 2;
                        break;
                    case 3:
                        Indices[0] = 1;
                        Indices[1] = 3;
                        Indices[2] = 2;
                        break;
                    case 4:
                        Indices[0] = 0;
                        Indices[1] = 1;
                        Indices[2] = 3;
                        break;
                    default:
                        break;//shouldn't ever get here...
                }
            }

            DataSize = VertexCount * VertexStride + IndexCount * 4;
        }

        public override void Load(Device device)
        {
            if ((Vertices != null) && (Vertices.Length > 0))
            {
                VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, Vertices);
                if (VertexBuffer != null)
                {
                    VBBinding = new VertexBufferBinding(VertexBuffer, VertexStride, 0);
                }
            }
            if ((Indices != null) && (Indices.Length > 0))
            {
                IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, Indices);
            }
            //LastUseTime = DateTime.Now; //reset usage timer
            IsLoaded = true;
        }

        public override void Unload()
        {
            IsLoaded = false;
            if (VertexBuffer != null)
            {
                VBBinding.Buffer = null;
                VertexBuffer.Dispose();
                VertexBuffer = null;
            }
            if (IndexBuffer != null)
            {
                IndexBuffer.Dispose();
                IndexBuffer = null;
            }
            LoadQueued = false;
        }

        public void Render(DeviceContext context)
        {
            if ((VertexBuffer == null) || (IndexBuffer == null))
            {
                return;
            }

            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, VBBinding);
            context.InputAssembler.SetIndexBuffer(IndexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            context.DrawIndexed(IndexCount, 0, 0);
        }

    }


    public class RenderableBoundComposite : RenderableCacheItem<BoundComposite>
    {
        public RenderableBoundGeometry[] Geometries;


        public override void Init(BoundComposite bound)
        {
            Key = bound;

            if (bound.Children == null)
            {
                return;
            }

            RenderableBoundGeometry[] geoms = new RenderableBoundGeometry[bound.Children.data_items.Length];
            long dsize = 0;
            for (int i = 0; i < bound.Children.data_items.Length; i++)
            {
                var child = bound.Children.data_items[i];
                if (child is BoundGeometry)
                {
                    var rgeom = new RenderableBoundGeometry();
                    rgeom.Init(child as BoundGeometry);
                    rgeom.Owner = this;
                    geoms[i] = rgeom;
                    dsize += rgeom.TotalDataSize;
                }
                else
                {
                    //other types of bound might be here, eg BoundBox
                    geoms[i] = null;//not really necessary
                }
            }

            Geometries = geoms;

            DataSize = dsize;

        }

        public override void Load(Device device)
        {
            if (Geometries == null) return;
            foreach (var geom in Geometries)
            {
                if (geom == null) continue;
                geom.Load(device);
            }
            //LastUseTime = DateTime.Now; //reset usage timer
            IsLoaded = true;
        }

        public override void Unload()
        {
            IsLoaded = false;
            if (Geometries == null) return;
            foreach (var geom in Geometries)
            {
                if (geom == null) continue;
                geom.Unload();
            }
            LoadQueued = false;
        }

        public override string ToString()
        {
            return Key.ToString();
        }
    }

    public class RenderableBoundGeometry
    {
        public RenderableBoundComposite Owner;
        public Buffer VertexBuffer { get; set; }
        //public Buffer IndexBuffer { get; set; }
        public VertexBufferBinding VBBinding;
        public BoundGeometry BoundGeom;
        public VertexType VertexType { get; set; } = VertexType.Default;
        public int VertexStride { get; set; } = 36;
        public int VertexCount { get; set; } = 0;
        public uint VertexDataSize { get; set; } = 0;
        public uint TotalDataSize { get; set; } = 0;
        public VertexTypeDefault[] Vertices { get; set; }

        public RenderableBox[] Boxes { get; set; }
        public RenderableSphere[] Spheres { get; set; }
        public RenderableCapsule[] Capsules { get; set; }
        public RenderableCylinder[] Cylinders { get; set; }
        public GpuSBuffer<RenderableBox> BoxBuffer { get; set; }
        public GpuSBuffer<RenderableSphere> SphereBuffer { get; set; }
        public GpuSBuffer<RenderableCapsule> CapsuleBuffer { get; set; }
        public GpuSBuffer<RenderableCylinder> CylinderBuffer { get; set; }

        public void Init(BoundGeometry bgeom)
        {
            BoundGeom = bgeom;

            if ((bgeom.Polygons == null) || (bgeom.Vertices == null))
            {
                return;
            }

            Vector3 vbox = (bgeom.BoundingBoxMax - bgeom.BoundingBoxMin);
            //var verts = bgeom.Vertices;
            //int vertcount = bgeom.Vertices.Length;

            int rvertcount = 0, curvert = 0;
            int rboxcount = 0, curbox = 0;
            int rspherecount = 0, cursphere = 0;
            int rcapsulecount = 0, curcapsule = 0;
            int rcylindercount = 0, curcylinder = 0;
            for (int i = 0; i < bgeom.Polygons.Length; i++)
            {
                if (bgeom.Polygons[i] == null) continue;
                var type = bgeom.Polygons[i].Type;
                switch(type)
                {
                    case BoundPolygonType.Triangle: rvertcount += 3;
                        break;
                    case BoundPolygonType.Sphere: rspherecount++;
                        break;
                    case BoundPolygonType.Capsule: rcapsulecount++;
                        break;
                    case BoundPolygonType.Box: rboxcount++;
                        break;
                    case BoundPolygonType.Cylinder: rcylindercount++;
                        break;
                }
            }

            VertexTypeDefault[] rverts = (rvertcount > 0) ? new VertexTypeDefault[rvertcount] : null;
            RenderableBox[] rboxes = (rboxcount > 0) ? new RenderableBox[rboxcount] : null;
            RenderableSphere[] rspheres = (rspherecount > 0) ? new RenderableSphere[rspherecount] : null;
            RenderableCapsule[] rcapsules = (rcapsulecount > 0) ? new RenderableCapsule[rcapsulecount] : null;
            RenderableCylinder[] rcylinders = (rcylindercount > 0) ? new RenderableCylinder[rcylindercount] : null;
            for (int i = 0; i < bgeom.Polygons.Length; i++)
            {
                var poly = bgeom.Polygons[i];
                if (poly == null) continue;
                byte matind = ((bgeom.PolygonMaterialIndices != null) && (i < bgeom.PolygonMaterialIndices.Length)) ? bgeom.PolygonMaterialIndices[i] : (byte)0;
                BoundMaterial_s mat = ((bgeom.Materials != null) && (matind < bgeom.Materials.Length)) ? bgeom.Materials[matind] : new BoundMaterial_s();
                Color color = BoundsMaterialTypes.GetMaterialColour(mat.Type);
                Vector3 p1, p2, p3, p4, a1, n1;//, n2, n3, p5, p7, p8;
                Vector3 norm = Vector3.Zero;
                uint colour = (uint)color.ToRgba();
                switch (poly.Type)
                {
                    case BoundPolygonType.Triangle:
                        var ptri = poly as BoundPolygonTriangle;
                        p1 = bgeom.GetVertex(ptri.vertIndex1);
                        p2 = bgeom.GetVertex(ptri.vertIndex2);
                        p3 = bgeom.GetVertex(ptri.vertIndex3);
                        n1 = Vector3.Normalize(Vector3.Cross(p2 - p1, p3 - p1));
                        AddVertex(p1, n1, colour, rverts, ref curvert);
                        AddVertex(p2, n1, colour, rverts, ref curvert);
                        AddVertex(p3, n1, colour, rverts, ref curvert);
                        break;
                    case BoundPolygonType.Sphere:
                        var psph = poly as BoundPolygonSphere;
                        rspheres[cursphere].Center = bgeom.GetVertex(psph.sphereIndex);
                        rspheres[cursphere].Radius = psph.sphereRadius;// * 0.5f;//diameter?
                        rspheres[cursphere].Colour = colour;
                        cursphere++;
                        break;
                    case BoundPolygonType.Capsule:
                        var bcap = poly as BoundPolygonCapsule;
                        p1 = bgeom.GetVertex(bcap.capsuleIndex1);
                        p2 = bgeom.GetVertex(bcap.capsuleIndex2);
                        a1 = p2 - p1;
                        n1 = Vector3.Normalize(a1);
                        p3 = Vector3.Normalize(GetPerpVec(n1));
                        //p4 = Vector3.Normalize(Vector3.Cross(n1, p3));
                        Quaternion q1 = Quaternion.Invert(Quaternion.LookAtRH(Vector3.Zero, p3, n1));
                        rcapsules[curcapsule].Point1 = p1;
                        rcapsules[curcapsule].Orientation = q1;
                        rcapsules[curcapsule].Length = a1.Length();
                        rcapsules[curcapsule].Radius = bcap.capsuleRadius;// * 0.5f;//diameter?
                        rcapsules[curcapsule].Colour = colour;
                        curcapsule++;
                        break;
                    case BoundPolygonType.Box:  //(...only 4 inds... = diagonal corners)
                        var pbox = poly as BoundPolygonBox;
                        p1 = bgeom.GetVertex(pbox.boxIndex1);
                        p2 = bgeom.GetVertex(pbox.boxIndex2);
                        p3 = bgeom.GetVertex(pbox.boxIndex3);
                        p4 = bgeom.GetVertex(pbox.boxIndex4);
                        a1 = ((p3 + p4) - (p1 + p2)) * 0.5f;
                        p2 = p1 + a1;
                        p3 = p3 - a1;
                        p4 = p4 - a1;
                        rboxes[curbox].Corner = p1;
                        rboxes[curbox].Edge1 = (p2 - p1);
                        rboxes[curbox].Edge2 = (p3 - p1);
                        rboxes[curbox].Edge3 = (p4 - p1);
                        rboxes[curbox].Colour = colour;
                        curbox++;
                        break;
                    case BoundPolygonType.Cylinder:
                        var pcyl = poly as BoundPolygonCylinder;
                        p1 = bgeom.GetVertex(pcyl.cylinderIndex1);
                        p2 = bgeom.GetVertex(pcyl.cylinderIndex2);
                        a1 = p2 - p1;
                        n1 = Vector3.Normalize(a1);
                        p3 = Vector3.Normalize(GetPerpVec(n1));
                        //p4 = Vector3.Normalize(Vector3.Cross(n1, p3));
                        Quaternion q2 = Quaternion.Invert(Quaternion.LookAtRH(Vector3.Zero, p3, n1));
                        rcylinders[curcylinder].Point1 = p1;
                        rcylinders[curcylinder].Orientation = q2;
                        rcylinders[curcylinder].Length = a1.Length();
                        rcylinders[curcylinder].Radius = pcyl.cylinderRadius;
                        rcylinders[curcylinder].Colour = colour;
                        curcylinder++;
                        break;
                    default:
                        break;
                }


            }

            Vertices = rverts;
            VertexCount = (rverts!=null) ? rverts.Length : 0;

            Boxes = rboxes;
            Spheres = rspheres;
            Capsules = rcapsules;
            Cylinders = rcylinders;

            VertexDataSize = (uint)(VertexCount * VertexStride);
            TotalDataSize = VertexDataSize;

        }

        private ushort AddVertex(Vector3 pos, Vector3 norm, uint colour, List<VertexTypeDefault> list)
        {
            VertexTypeDefault v = new VertexTypeDefault();
            v.Position = pos;
            v.Normal = norm;
            v.Colour = colour;
            v.Texcoord = Vector2.Zero;
            var rv = list.Count;
            list.Add(v);
            return (ushort)rv;
        }
        private void AddVertex(Vector3 pos, Vector3 norm, uint colour, VertexTypeDefault[] arr, ref int index)
        {
            arr[index].Position = pos;
            arr[index].Normal = norm;
            arr[index].Colour = colour;
            arr[index].Texcoord = Vector2.Zero;
            index++;
        }

        private Vector3 GetPerpVec(Vector3 n)
        {
            //make a vector perpendicular to the given one
            float nx = Math.Abs(n.X);
            float ny = Math.Abs(n.Y);
            float nz = Math.Abs(n.Z);
            if ((nx < ny) && (nx < nz))
            {
                return Vector3.Cross(n, Vector3.Right);
            }
            else if (ny < nz)
            {
                return Vector3.Cross(n, Vector3.Up);
            }
            else
            {
                return Vector3.Cross(n, Vector3.ForwardLH);
            }
        }


        public void Load(Device device)
        {
            //if (Vertices.Length == 0) return; //nothing to see here..

            if ((Vertices != null) && (Vertices.Length > 0))
            {
                VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, Vertices);
                if (VertexBuffer != null)
                {
                    VBBinding = new VertexBufferBinding(VertexBuffer, VertexStride, 0);
                }
                //IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, Indices);
            }

            if ((Boxes != null) && (Boxes.Length > 0))
            {
                BoxBuffer = new GpuSBuffer<RenderableBox>(device, Boxes);
            }
            if ((Spheres != null) && (Spheres.Length > 0))
            {
                SphereBuffer = new GpuSBuffer<RenderableSphere>(device, Spheres);
            }
            if ((Capsules != null) && (Capsules.Length > 0))
            {
                CapsuleBuffer = new GpuSBuffer<RenderableCapsule>(device, Capsules);
            }
            if ((Cylinders != null) && (Cylinders.Length > 0))
            {
                CylinderBuffer = new GpuSBuffer<RenderableCylinder>(device, Cylinders);
            }

        }

        public void Unload()
        {


            if (VertexBuffer != null)
            {
                VBBinding.Buffer = null;
                VertexBuffer.Dispose();
                VertexBuffer = null;
            }
            //if (IndexBuffer != null)
            //{
            //    IndexBuffer.Dispose();
            //    IndexBuffer = null;
            //}
            //BoundGeom = null;

            if (BoxBuffer != null)
            {
                BoxBuffer.Dispose();
                BoxBuffer = null;
            }
            if (SphereBuffer != null)
            {
                SphereBuffer.Dispose();
                SphereBuffer = null;
            }
            if (CapsuleBuffer != null)
            {
                CapsuleBuffer.Dispose();
                CapsuleBuffer = null;
            }
            if (CylinderBuffer != null)
            {
                CylinderBuffer.Dispose();
                CylinderBuffer = null;
            }
        }

        public void RenderTriangles(DeviceContext context)
        {
            if ((VertexBuffer == null))// || (IndexBuffer == null))
            {
                return;
            }

            //Owner.LastUseTime = DateTime.Now; //cache timer reset


            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, VBBinding);
            context.InputAssembler.SetIndexBuffer(null, SharpDX.DXGI.Format.R16_UInt, 0);
            //context.InputAssembler.SetIndexBuffer(IndexBuffer, SharpDX.DXGI.Format.R16_UInt, 0);

            context.Draw(VertexCount, 0);

        }

    }

    public struct RenderableBox
    {
        public Vector3 Corner { get; set; }
        public uint Colour { get; set; }
        public Vector3 Edge1 { get; set; }
        public float Pad1 { get; set; }
        public Vector3 Edge2 { get; set; }
        public float Pad2 { get; set; }
        public Vector3 Edge3 { get; set; }
        public float Pad3 { get; set; }
    }
    public struct RenderableSphere
    {
        public Vector3 Center { get; set; }
        public float Radius { get; set; }
        public Vector3 Pad0 { get; set; }
        public uint Colour { get; set; }
    }
    public struct RenderableCapsule
    {
        public Vector3 Point1 { get; set; }
        public float Radius { get; set; }
        public Quaternion Orientation { get; set; }
        public float Length { get; set; }
        public uint Colour { get; set; }
        public float Pad0 { get; set; }
        public float Pad1 { get; set; }
    }
    public struct RenderableCylinder
    {
        public Vector3 Point1 { get; set; }
        public float Radius { get; set; }
        public Quaternion Orientation { get; set; }
        public float Length { get; set; }
        public uint Colour { get; set; }
        public float Pad0 { get; set; }
        public float Pad1 { get; set; }
    }


    public struct RenderableEntity
    {
        public YmapEntityDef Entity;
        public Renderable Renderable;
    }


}
