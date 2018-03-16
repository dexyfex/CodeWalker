using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace CodeWalker.Explorer
{
    public partial class OrganizeFavorites : Form
    {
        public ExploreForm ExploreForm { get; set; }

        private XmlDocument xDoc = new XmlDocument();
        private XmlNodeList FavoriteNodes;
        private XmlNode Root;

        public OrganizeFavorites(ExploreForm exploreform)
        {
            ExploreForm = exploreform;
            InitializeComponent();
            Init();
        }
        
        private void Init()
        {
            xDoc.Load(@"C:\Users\Skyler\Documents\GitHub\CodeWalker\Resources\Favorites.xml");
            FavoriteNodes = xDoc.DocumentElement.SelectNodes("Favorite");
            Root = xDoc.DocumentElement;
            foreach (XmlNode FavNode in FavoriteNodes)
            {
                FavoritesTreeView.Nodes[0].Nodes.Add(FavNode.InnerText);
            }

            FavoritesTreeView.ExpandAll();
        }

        private void ClearAllFavoritesButton_Click(object sender, EventArgs e)
        {
            FavoritesTreeView.Nodes[0].Nodes.Clear();
            Root.RemoveAll();
        }

        private void RemoveFavoriteButton_Click(object sender, EventArgs e)
        {
            if (FavoritesTreeView.SelectedNode == FavoritesTreeView.Nodes[0]) return;
            string FavoriteToDelete = FavoritesTreeView.SelectedNode.Text;
            FavoritesTreeView.SelectedNode.Remove();

            foreach (XmlNode FavNode in FavoriteNodes)
            {
                if(FavNode.InnerText == FavoriteToDelete)
                {
                    Root.RemoveChild(FavNode);
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            xDoc.Save(@"C:\Users\Skyler\Documents\GitHub\CodeWalker\Resources\Favorites.xml");
            ExploreForm.LoadFavorites();
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
