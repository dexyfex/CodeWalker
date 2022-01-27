using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ST.Library.UI.NodeEditor
{
    /// <summary>
    /// STNode node characteristics
    /// Used to describe STNode developer information and some behaviors
    /// </summary>
    public class STNodeAttribute : Attribute
    {
        private string _Path;
        /// <summary>
        /// Get the path that the STNode node expects in the tree control
        /// </summary>
        public string Path {
            get { return _Path; }
        }

        private string _Author;
        /// <summary>
        /// Get the author name of the STNode node
        /// </summary>
        public string Author {
            get { return _Author; }
        }

        private string _Mail;
        /// <summary>
        /// Get the author mailbox of the STNode node
        /// </summary>
        public string Mail {
            get { return _Mail; }
        }

        private string _Link;
        /// <summary>
        /// Get the author link of the STNode node
        /// </summary>
        public string Link {
            get { return _Link; }
        }

        private string _Description;
        /// <summary>
        /// Get the description information of the STNode node
        /// </summary>
        public string Description {
            get { return _Description; }
        }

        private static char[] m_ch_splitter = new char[] { '/', '\\' };
        private static Regex m_reg = new Regex(@"^https?://", RegexOptions.IgnoreCase);
        /// <summary>
        /// Constructs an STNode property
        /// </summary>
        /// <param name="strPath">expected path</param>
        public STNodeAttribute(string strPath) : this(strPath, null, null, null, null) { }
        /// <summary>
        /// Constructs an STNode property
        /// </summary>
        /// <param name="strPath">expected path</param>
        /// <param name="strDescription">Description</param>
        public STNodeAttribute(string strPath, string strDescription) : this(strPath, null, null, null, strDescription) { }
        /// <summary>
        /// Constructs an STNode property
        /// </summary>
        /// <param name="strPath">expected path</param>
        /// <param name="strAuthor">STNode author name</param>
        /// <param name="strMail">STNode author mailbox</param>
        /// <param name="strLink">STNode author link</param>
        /// <param name="strDescription">STNode node description information</param>
        public STNodeAttribute(string strPath, string strAuthor, string strMail, string strLink, string strDescription) {
            if (!string.IsNullOrEmpty(strPath))
                strPath = strPath.Trim().Trim(m_ch_splitter).Trim();

            this._Path = strPath;

            this._Author = strAuthor;
            this._Mail = strMail;
            this._Description = strDescription;
            if (string.IsNullOrEmpty(strLink) || strLink.Trim() == string.Empty) return;
            strLink = strLink.Trim();
            if (m_reg.IsMatch(strLink))
                this._Link = strLink;
            else
                this._Link = "http://" + strLink;
        }

        private static Dictionary<Type, MethodInfo> m_dic = new Dictionary<Type, MethodInfo>();
        /// <summary>
        /// Get type helper function
        /// </summary>
        /// <param name="stNodeType">Node Type</param>
        /// <returns>Function information</returns>
        public static MethodInfo GetHelpMethod(Type stNodeType) {
            if (m_dic.ContainsKey(stNodeType)) return m_dic[stNodeType];
            var mi = stNodeType.GetMethod("ShowHelpInfo");
            if (mi == null) return null;
            if (!mi.IsStatic) return null;
            var ps = mi.GetParameters ();
            if (ps.Length != 1) return null;
            if (ps[0].ParameterType != typeof(string)) return null;
            m_dic.Add(stNodeType, mi);
            return mi;
        }
        /// <summary>
        /// Execute the helper function for the corresponding node type
        /// </summary>
        /// <param name="stNodeType">Node Type</param>
        public static void ShowHelp(Type stNodeType) {
            var mi = STNodeAttribute.GetHelpMethod (stNodeType);
            if (mi == null) return;
            mi.Invoke(null, new object[] { stNodeType.Module.FullyQualifiedName });
        }
    }
}