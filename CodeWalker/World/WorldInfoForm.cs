using CodeWalker.GameFiles;
using CodeWalker.Rendering;
using CodeWalker.Utils;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.World
{
    public partial class WorldInfoForm : Form
    {
        WorldForm WorldForm;
        MapSelection Selection;
        string SelectionMode = "";
        bool MouseSelectEnable = false;
        Texture currentTex; // Used by save button

        public WorldInfoForm(WorldForm worldForm)
        {
            WorldForm = worldForm;
            InitializeComponent();

            //SelectionModeComboBox.SelectedIndex = 0;
        }

        private void MouseSelectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (MouseSelectCheckBox.Checked != MouseSelectEnable)
            {
                MouseSelectEnable = MouseSelectCheckBox.Checked;
                WorldForm.OnInfoFormSelectionModeChanged(SelectionMode, MouseSelectEnable);
            }
        }

        private void SelectionModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectionModeComboBox.Text != SelectionMode)
            {
                SelectionMode = SelectionModeComboBox.Text;
                WorldForm.OnInfoFormSelectionModeChanged(SelectionMode, MouseSelectEnable);
            }
        }

        public void SetSelectionMode(string mode, bool enable)
        {
            SelectionMode = mode;
            MouseSelectEnable = enable;
            SelectionModeComboBox.Text = mode;
            MouseSelectCheckBox.Checked = enable;
        }

        public void SetSelection(MapSelection item)
        {
            Selection = item;

            SelectionNameTextBox.Text = item.GetNameString("Nothing selected");
            //SelEntityPropertyGrid.SelectedObject = item.EntityDef;
            SelArchetypePropertyGrid.SelectedObject = item.Archetype;
            SelDrawablePropertyGrid.SelectedObject = item.Drawable;
            SelDrawableModelPropertyGrid.SelectedObject = null;
            SelDrawableModelsTreeView.Nodes.Clear();
            SelDrawableTexturesTreeView.Nodes.Clear();
            SelDrawableTexturePropertyGrid.SelectedObject = null;
            SelDrawableTexturePictureBox.Image = null;
            HierarchyTreeView.Nodes.Clear();
            if (item.Drawable != null)
            {
                AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModels?.High, "High Detail", true);
                AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModels?.Med, "Medium Detail", false);
                AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModels?.Low, "Low Detail", false);
                AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModels?.VLow, "Very Low Detail", false);
                //AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModels?.Extra, "X Detail", false);
            }

            if (item.EntityDef != null)
            {
                AddSelectionEntityHierarchyNodes(item.EntityDef);
            }

            if (item.MultipleSelectionItems != null)
            {
                SelectionEntityTabPage.Text = "Multiple items";
                SelEntityPropertyGrid.SelectedObject = item.MultipleSelectionItems;
            }
            else if (item.TimeCycleModifier != null)
            {
                SelectionEntityTabPage.Text = "Time Cycle Modifier";
                SelEntityPropertyGrid.SelectedObject = item.TimeCycleModifier;
            }
            else if (item.CarGenerator != null)
            {
                SelectionEntityTabPage.Text = "Car Generator";
                SelEntityPropertyGrid.SelectedObject = item.CarGenerator;
            }
            else if (item.LodLight!= null)
            {
                SelectionEntityTabPage.Text = "LOD Light";
                SelEntityPropertyGrid.SelectedObject = item.LodLight;
            }
            else if (item.GrassBatch != null)
            {
                SelectionEntityTabPage.Text = "Grass";
                SelEntityPropertyGrid.SelectedObject = item.GrassBatch;
            }
            else if (item.BoxOccluder != null)
            {
                SelectionEntityTabPage.Text = "Box Occluder";
                SelEntityPropertyGrid.SelectedObject = item.BoxOccluder;
            }
            else if (item.OccludeModelTri != null)
            {
                SelectionEntityTabPage.Text = "Occlude Model Triangle";
                SelEntityPropertyGrid.SelectedObject = item.OccludeModelTri;
            }
            else if (item.WaterQuad != null)
            {
                SelectionEntityTabPage.Text = "Water Quad";
                SelEntityPropertyGrid.SelectedObject = item.WaterQuad;
            }
            else if (item.NavPoly != null)
            {
                SelectionEntityTabPage.Text = "Nav Poly";
                SelEntityPropertyGrid.SelectedObject = item.NavPoly;
            }
            else if (item.NavPoint != null)
            {
                SelectionEntityTabPage.Text = "Nav Point";
                SelEntityPropertyGrid.SelectedObject = item.NavPoint;
            }
            else if (item.NavPortal != null)
            {
                SelectionEntityTabPage.Text = "Nav Portal";
                SelEntityPropertyGrid.SelectedObject = item.NavPortal;
            }
            else if (item.PathNode != null)
            {
                SelectionEntityTabPage.Text = "Path Node";
                SelEntityPropertyGrid.SelectedObject = item.PathNode;
            }
            else if (item.TrainTrackNode != null)
            {
                SelectionEntityTabPage.Text = "Train Track Node";
                SelEntityPropertyGrid.SelectedObject = item.TrainTrackNode;
            }
            else if (item.ScenarioNode != null)
            {
                SelectionEntityTabPage.Text = item.ScenarioNode.FullTypeName;
                SelEntityPropertyGrid.SelectedObject = item.ScenarioNode;
            }
            else if (item.Audio != null)
            {
                SelectionEntityTabPage.Text = item.Audio.FullTypeName;
                SelEntityPropertyGrid.SelectedObject = item.Audio;
            }
            else
            {
                SelectionEntityTabPage.Text = "Entity";
                SelEntityPropertyGrid.SelectedObject = item.EntityDef;
            }


            if (item.EntityExtension != null)
            {
                SelectionExtensionTabPage.Text = "Entity Extension";
                SelExtensionPropertyGrid.SelectedObject = item.EntityExtension;
            }
            else if (item.ArchetypeExtension != null)
            {
                SelectionExtensionTabPage.Text = "Archetype Extension";
                SelExtensionPropertyGrid.SelectedObject = item.ArchetypeExtension;
            }
            else if (item.CollisionVertex != null)
            {
                SelectionExtensionTabPage.Text = "Collision Vertex";
                SelExtensionPropertyGrid.SelectedObject = item.CollisionVertex;
            }
            else if (item.CollisionPoly != null)
            {
                SelectionExtensionTabPage.Text = "Collision Polygon";
                SelExtensionPropertyGrid.SelectedObject = item.CollisionPoly;
            }
            else if (item.CollisionBounds != null)
            {
                SelectionExtensionTabPage.Text = "Collision Bounds";
                SelExtensionPropertyGrid.SelectedObject = item.CollisionBounds;
            }
            else
            {
                SelectionExtensionTabPage.Text = "Extension";
                SelExtensionPropertyGrid.SelectedObject = null;
            }

        }
        private void AddSelectionDrawableModelsTreeNodes(DrawableModel[] models, string prefix, bool check)
        {
            if (models == null) return;

            for (int mi = 0; mi < models.Length; mi++)
            {
                var model = models[mi];
                string mprefix = prefix + " " + (mi + 1).ToString();
                var mnode = SelDrawableModelsTreeView.Nodes.Add(mprefix + " " + model.ToString());
                mnode.Tag = model;
                mnode.Checked = check;

                var tmnode = SelDrawableTexturesTreeView.Nodes.Add(mprefix + " " + model.ToString());
                tmnode.Tag = model;

                if (model.Geometries == null) continue;

                foreach (var geom in model.Geometries)
                {
                    var gname = geom.ToString();
                    var gnode = mnode.Nodes.Add(gname);
                    gnode.Tag = geom;
                    gnode.Checked = true;// check;

                    var tgnode = tmnode.Nodes.Add(gname);
                    tgnode.Tag = geom;

                    if ((geom.Shader != null) && (geom.Shader.ParametersList != null) && (geom.Shader.ParametersList.Hashes != null))
                    {
                        var pl = geom.Shader.ParametersList;
                        var h = pl.Hashes;
                        var p = pl.Parameters;
                        for (int ip = 0; ip < h.Length; ip++)
                        {
                            var hash = pl.Hashes[ip];
                            var parm = pl.Parameters[ip];
                            var tex = parm.Data as TextureBase;
                            if (tex != null)
                            {
                                var t = tex as Texture;
                                var tstr = tex.Name.Trim();
                                if (t != null)
                                {
                                    tstr = string.Format("{0} ({1}x{2}, embedded)", tex.Name, t.Width, t.Height);
                                }
                                var tnode = tgnode.Nodes.Add(hash.ToString().Trim() + ": " + tstr);
                                tnode.Tag = tex;
                            }
                        }
                        tgnode.Expand();
                    }

                }

                mnode.Expand();
                tmnode.Expand();
            }
        }

        private void AddSelectionEntityHierarchyNodes(YmapEntityDef entity)
        {
            if (entity == null) return;

            var e = entity;
            TreeNode tn = null;
            TreeNode seltn = null;

            while (e != null)
            {
                var newtn = new TreeNode(e.Name);
                newtn.Tag = e;
                if (tn != null)
                {
                    newtn.Nodes.Add(tn);
                }
                else
                {
                    seltn = newtn;
                }
                if (e.Children != null)
                {
                    foreach (var c in e.Children)
                    {
                        if ((tn != null) && (c == tn.Tag)) continue;
                        var ctn = new TreeNode(c.Name);
                        ctn.Tag = c;
                        newtn.Nodes.Add(ctn);
                    }
                }

                tn = newtn;
                e = e.Parent;
            }

            if (tn != null)
            {
                HierarchyTreeView.Nodes.Add(tn);
                tn.ExpandAll();
            }
            if (seltn != null)
            {
                HierarchyTreeView.SelectedNode = seltn;
            }

        }


        private void DisplayTexture(Texture tex, int mip)
        {
            try
            {
                int cmip = Math.Min(Math.Max(mip, 0), tex.Levels - 1);
                byte[] pixels = DDSIO.GetPixels(tex, cmip);
                int w = tex.Width >> cmip;
                int h = tex.Height >> cmip;
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);

                if (pixels != null)
                {
                    var BoundsRect = new System.Drawing.Rectangle(0, 0, w, h);
                    BitmapData bmpData = bmp.LockBits(BoundsRect, ImageLockMode.WriteOnly, bmp.PixelFormat);
                    IntPtr ptr = bmpData.Scan0;
                    int bytes = bmpData.Stride * bmp.Height;
                    Marshal.Copy(pixels, 0, ptr, bytes);
                    bmp.UnlockBits(bmpData);
                }

                SelDrawableTexturePictureBox.Image = bmp;
                SelTextureDimensionsLabel.Text = w.ToString() + " x " + h.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading texture mip:\n" + ex.ToString());
                SelDrawableTexturePictureBox.Image = null;
            }
        }

        private void SelectTexture(TextureBase texbase, bool mipchange)
        {
            Texture tex = texbase as Texture;
            YtdFile ytd = null;
            string errstr = string.Empty;
            if ((tex == null)&&(texbase!=null))
            {
                //need to load from txd.
                var arch = Selection.Archetype;
                uint texhash = texbase.NameHash;
                uint txdHash = (arch != null) ? arch.TextureDict.Hash : 0;
                tex = TryGetTextureFromYtd(texhash, txdHash, out ytd);
                if (tex == null)
                { //search parent ytds...
                    uint ptxdhash = WorldForm.GameFileCache.TryGetParentYtdHash(txdHash);
                    while ((ptxdhash != 0) && (tex == null))
                    {
                        tex = TryGetTextureFromYtd(texhash, ptxdhash, out ytd);
                        if (tex == null)
                        {
                            ptxdhash = WorldForm.GameFileCache.TryGetParentYtdHash(ptxdhash);
                        }
                        else
                        { }
                    }
                    if (tex == null)
                    {
                        ytd = WorldForm.GameFileCache.TryGetTextureDictForTexture(texhash);
                        if (ytd != null)
                        {
                            int tries = 0;
                            while (!ytd.Loaded && (tries < 500)) //wait upto ~5 sec
                            {
                                System.Threading.Thread.Sleep(10);
                                tries++;
                            }
                            if (ytd.Loaded)
                            {
                                tex = ytd.TextureDict.Lookup(texhash);
                            }
                        }
                        if (tex == null)
                        {
                            ytd = null;
                            errstr = "<Couldn't find texture!>";
                        }
                    }
                }
            }
            if (tex != null)
            {
                currentTex = tex;
                int mip = 0;
                if (mipchange)
                {
                    mip = SelTextureMipTrackBar.Value;
                    if (mip >= tex.Levels) mip = tex.Levels - 1;
                }
                else
                {
                    SelTextureMipTrackBar.Maximum = tex.Levels - 1;
                }
                DisplayTexture(tex, mip);


                //try get owner drawable to get the name for the dictionary textbox...
                object owner = null;
                if (Selection.Drawable != null)
                {
                    owner = Selection.Drawable.Owner;
                }
                YdrFile ydr = owner as YdrFile;
                YddFile ydd = owner as YddFile;
                YftFile yft = owner as YftFile;

                SelTextureNameTextBox.Text = tex.Name;
                SelTextureDictionaryTextBox.Text = (ytd != null) ? ytd.Name : (ydr != null) ? ydr.Name : (ydd != null) ? ydd.Name : (yft != null) ? yft.Name : string.Empty;
            }
            else
            {
                SelDrawableTexturePictureBox.Image = null;
                SelTextureNameTextBox.Text = errstr;
                SelTextureDictionaryTextBox.Text = string.Empty;
                SelTextureMipTrackBar.Value = 0;
                SelTextureMipTrackBar.Maximum = 0;
                SelTextureDimensionsLabel.Text = "-";
                currentTex = null;
            }
        }

        private Texture TryGetTextureFromYtd(uint texHash, uint txdHash, out YtdFile ytd)
        {
            if (txdHash != 0)
            {
                ytd = WorldForm.GameFileCache.GetYtd(txdHash);
                if (ytd != null)
                {
                    int tries = 0;
                    while (!ytd.Loaded && (tries < 500)) //wait upto ~5 sec
                    {
                        System.Threading.Thread.Sleep(10);
                        tries++;
                    }
                    if (ytd.Loaded)
                    {
                        return ytd.TextureDict.Lookup(texHash);
                    }
                }
            }
            ytd = null;
            return null;
        }


        private void WorldInfoForm_Load(object sender, EventArgs e)
        {
            SetSelection(Selection);
        }

        private void WorldInfoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            WorldForm.OnInfoFormClosed();
        }

        public void SyncSelDrawableModelsTreeNode(TreeNode node)
        {
            //called by the world form when a selection treeview node is checked/unchecked.
            foreach (TreeNode mnode in SelDrawableModelsTreeView.Nodes)
            {
                if (mnode.Tag == node.Tag)
                {
                    if (mnode.Checked != node.Checked)
                    {
                        mnode.Checked = node.Checked;
                    }
                }
                foreach (TreeNode gnode in mnode.Nodes)
                {
                    if (gnode.Tag == node.Tag)
                    {
                        if (gnode.Checked != node.Checked)
                        {
                            gnode.Checked = node.Checked;
                        }
                    }
                }
            }
        }

        private void SelDrawableModelsTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            WorldForm.SyncSelDrawableModelsTreeNode(e.Node);
        }

        private void SelDrawableModelsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelDrawableModelPropertyGrid.SelectedObject = e.Node?.Tag;
        }

        private void SelDrawableTexturesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelDrawableTexturePropertyGrid.SelectedObject = e.Node?.Tag;

            TextureBase texbase = e.Node?.Tag as TextureBase;

            SelTextureMipTrackBar.Value = 0;
            SelTextureMipLabel.Text = "0";

            SelectTexture(texbase, false);
        }

        private void SelTextureMipTrackBar_Scroll(object sender, EventArgs e)
        {
            var node = SelDrawableTexturesTreeView.SelectedNode;

            TextureBase texbase = node?.Tag as TextureBase;

            SelTextureMipLabel.Text = SelTextureMipTrackBar.Value.ToString();

            SelectTexture(texbase, true);
        }

        private void HierarchyTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var sele = HierarchyTreeView.SelectedNode?.Tag as YmapEntityDef;
            HierarchyPropertyGrid.SelectedObject = sele;
        }

        private void SaveTextureButton_Click(object sender, EventArgs e)
        {
            if (currentTex == null) return;
            string fname = currentTex.Name + ".dds";
            SaveFileDialog.FileName = fname;
            if (SaveFileDialog.ShowDialog() != DialogResult.OK) return;
            string fpath = SaveFileDialog.FileName;
            byte[] dds = DDSIO.GetDDSFile(currentTex);
            File.WriteAllBytes(fpath, dds);
        }
    }
}
