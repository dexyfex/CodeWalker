using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Forms
{
    public partial class ModelMatForm : Form
    {
        private ModelForm ModelForm;
        private DrawableBase Drawable;
        private Dictionary<uint, Drawable> DrawableDict;


        public ModelMatForm(ModelForm modelForm)
        {
            InitializeComponent();

            ModelForm = modelForm;

        }


        public void LoadModel(DrawableBase drawable)
        {
            Drawable = drawable;

            ModelsTreeView.Nodes.Clear();
            ModelsTreeView.ShowRootLines = false;
            if (drawable != null)
            {
                AddDrawableModelsTreeNodes(drawable.DrawableModelsHigh, "High Detail");
                AddDrawableModelsTreeNodes(drawable.DrawableModelsMedium, "Medium Detail");
                AddDrawableModelsTreeNodes(drawable.DrawableModelsLow, "Low Detail");
                AddDrawableModelsTreeNodes(drawable.DrawableModelsVeryLow, "Very Low Detail");

                var fdrawable = drawable as FragDrawable;
                if (fdrawable != null)
                {
                    var plod1 = fdrawable.OwnerFragment?.PhysicsLODGroup?.PhysicsLOD1;
                    if ((plod1 != null) && (plod1.Children?.data_items != null))
                    {
                        foreach (var child in plod1.Children.data_items)
                        {
                            var cdrwbl = child.Drawable1;
                            if ((cdrwbl != null) && (cdrwbl.AllModels?.Length > 0))
                            {
                                if (cdrwbl.Owner is FragDrawable) continue; //it's a copied drawable... eg a wheel

                                var dname = child.GroupNameHash.ToString();
                                AddDrawableModelsTreeNodes(cdrwbl.DrawableModelsHigh, dname + " - High Detail");
                                AddDrawableModelsTreeNodes(cdrwbl.DrawableModelsMedium, dname + " - Medium Detail");
                                AddDrawableModelsTreeNodes(cdrwbl.DrawableModelsLow, dname + " - Low Detail");
                                AddDrawableModelsTreeNodes(cdrwbl.DrawableModelsVeryLow, dname + " - Very Low Detail");
                            }
                        }
                    }
                }
            }
        }
        public void LoadModels(Dictionary<uint, Drawable> dict)
        {
            DrawableDict = dict;

            ModelsTreeView.Nodes.Clear();
            //ModelsTreeView.ShowRootLines = true;

            if (dict != null)
            {
                foreach (var kvp in dict)
                {
                    MetaHash mhash = new MetaHash(kvp.Key);
                    var drawable = kvp.Value;

                    var dnode = ModelsTreeView.Nodes.Add(mhash.ToString());
                    dnode.Tag = drawable;

                    AddDrawableModelsTreeNodes(drawable.DrawableModelsHigh, "High Detail", dnode);
                    AddDrawableModelsTreeNodes(drawable.DrawableModelsMedium, "Medium Detail", dnode);
                    AddDrawableModelsTreeNodes(drawable.DrawableModelsLow, "Low Detail", dnode);
                    AddDrawableModelsTreeNodes(drawable.DrawableModelsVeryLow, "Very Low Detail", dnode);

                    dnode.Expand();
                }
            }
        }

        private void AddDrawableModelsTreeNodes(ResourcePointerList64<DrawableModel> models, string prefix, TreeNode parentDrawableNode = null)
        {
            if (models == null) return;
            if (models.data_items == null) return;

            for (int mi = 0; mi < models.data_items.Length; mi++)
            {
                var tnc = (parentDrawableNode != null) ? parentDrawableNode.Nodes : ModelsTreeView.Nodes;

                var model = models.data_items[mi];
                string mprefix = prefix + " " + (mi + 1).ToString();
                var mnode = tnc.Add(mprefix + " " + model.ToString());
                mnode.Tag = model;

                if ((model.Geometries == null) || (model.Geometries.data_items == null)) continue;

                foreach (var geom in model.Geometries.data_items)
                {
                    var gname = geom.ToString();
                    var gnode = mnode.Nodes.Add(gname);
                    gnode.Tag = geom;
                }

                mnode.Expand();
            }
        }


        private void SelectGeometry(DrawableGeometry geom)
        {
            MaterialPropertiesPanel.Controls.Clear();

            var pl = geom?.Shader?.ParametersList;

            if (pl == null) return;

            var tmpPanel = new Panel(); //need this so textboxes resize correctly in case of scrollbar
            tmpPanel.Size = new Size(MaterialPropertiesPanel.Width, 10);
            tmpPanel.Location = new System.Drawing.Point(0, 0);
            tmpPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            var w = MaterialPropertiesPanel.Width - 140;

            var h = pl.Hashes;
            var p = pl.Parameters;
            for (int ip = 0; ip < h.Length; ip++)
            {
                var hash = pl.Hashes[ip];
                var parm = pl.Parameters[ip];
                var data = parm?.Data;

                tmpPanel.Height += 25;

                var l = new Label();
                l.AutoSize = true;
                l.Location = new System.Drawing.Point(5, 5 + ip * 25);
                l.Text = hash.ToString();
                tmpPanel.Controls.Add(l);

                var t = new TextBox();
                t.Size = new Size(w, 20);
                t.Location = new System.Drawing.Point(130, 2 + ip * 25);
                t.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                t.Tag = parm;
                tmpPanel.Controls.Add(t);

                if (data is Vector4)
                {
                    t.Text = FloatUtil.GetVector4String((Vector4)data);
                }
                else if (data is Vector4[])
                {
                    var txt = "";
                    var vecs = (Vector4[])data;
                    foreach (var vec in vecs)
                    {
                        if (txt.Length > 0) txt += "; ";
                        txt += FloatUtil.GetVector4String(vec);
                    }
                    t.Text = txt;
                }
                else if (data is TextureBase)
                {
                    var tex = (TextureBase)data;
                    t.Text = tex.Name;
                    if(tex is Texture)
                    {
                        t.Text += " (embedded)";
                        t.ReadOnly = true;
                    }
                }

                t.TextChanged += ParamTextBox_TextChanged;

            }


            MaterialPropertiesPanel.Controls.Add(tmpPanel);
        }

        private void ParamTextBox_TextChanged(object sender, EventArgs e)
        {
            var tb = sender as TextBox;
            var parm = tb?.Tag as ShaderParameter;
            var txt = tb?.Text;

            if (parm == null) return;

            if (parm.DataType == 0)//texture
            {
                var tex = parm.Data as TextureBase;
                var ttex = tex as Texture;
                if (ttex == null)//don't do this for embedded textures!
                {
                    tex.Name = txt;
                    tex.NameHash = JenkHash.GenHash(txt.ToLowerInvariant());
                }
                else
                {
                    //TODO: modify embedded textures!
                }
            }
            else if (parm.DataType == 1)//Vector4
            {
                parm.Data = FloatUtil.ParseVector4String(txt);
            }
            else //Vector4 array
            {
                var strs = txt.Split(';');
                var vecs = new Vector4[parm.DataType];
                for (int i = 0; i < parm.DataType; i++)
                {
                    var vec = Vector4.Zero;
                    if (i < strs.Length)
                    {
                        vec = FloatUtil.ParseVector4String(strs[i].Trim());
                    }
                    vecs[i] = vec;
                }
                parm.Data = vecs;
            }


            var geom = ModelsTreeView.SelectedNode?.Tag as DrawableGeometry;
            if (geom != null)
            {
                if (Drawable != null)
                {
                    UpdateRenderableParams(Drawable, geom.Shader);
                }
                if (DrawableDict != null)
                {
                    foreach (var dwbl in DrawableDict.Values)
                    {
                        UpdateRenderableParams(dwbl, geom.Shader);
                    }
                }
            }

            ModelForm.OnModelModified();

        }

        private void UpdateRenderableParams(DrawableBase dwbl, ShaderFX shader)
        {
            foreach (var model in dwbl.AllModels)
            {
                if (model?.Geometries?.data_items == null) continue;
                foreach (var geom in model.Geometries.data_items)
                {
                    if (geom.Shader == shader)
                    {
                        geom.UpdateRenderableParameters = true;
                    }
                }
            }
        }


        private void ModelMatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ModelForm.OnMaterialFormClosed();
        }

        private void ModelsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectGeometry(e.Node.Tag as DrawableGeometry);
        }
    }
}
