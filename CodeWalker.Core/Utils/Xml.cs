using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker
{
    public static class Xml
    {

        public static string GetStringAttribute(XmlNode node, string attribute)
        {
            if (node == null) return null;
            return node.Attributes[attribute]?.InnerText;
        }
        public static bool GetBoolAttribute(XmlNode node, string attribute)
        {
            if (node == null) return false;
            string val = node.Attributes[attribute]?.InnerText;
            bool b;
            bool.TryParse(val, out b);
            return b;
        }
        public static int GetIntAttribute(XmlNode node, string attribute)
        {
            if (node == null) return 0;
            string val = node.Attributes[attribute]?.InnerText;
            int i;
            int.TryParse(val, out i);
            return i;
        }
        public static float GetFloatAttribute(XmlNode node, string attribute)
        {
            if (node == null) return 0;
            string val = node.Attributes[attribute]?.InnerText;
            float f;
            FloatUtil.TryParse(val, out f);
            return f;
        }

        public static string GetChildInnerText(XmlNode node, string name)
        {
            if (node == null) return null;
            return node.SelectSingleNode(name)?.InnerText;
        }
        public static bool GetChildBoolInnerText(XmlNode node, string name)
        {
            if (node == null) return false;
            string val = node.SelectSingleNode(name)?.InnerText;
            bool b;
            bool.TryParse(val, out b);
            return b;
        }
        public static int GetChildIntInnerText(XmlNode node, string name)
        {
            if (node == null) return 0;
            string val = node.SelectSingleNode(name)?.InnerText;
            int i;
            int.TryParse(val, out i);
            return i;
        }
        public static float GetChildFloatInnerText(XmlNode node, string name)
        {
            if (node == null) return 0;
            string val = node.SelectSingleNode(name)?.InnerText;
            float f;
            FloatUtil.TryParse(val, out f);
            return f;
        }

        public static bool GetChildBoolAttribute(XmlNode node, string name, string attribute)
        {
            if (node == null) return false;
            string val = node.SelectSingleNode(name)?.Attributes[attribute]?.InnerText;
            bool b;
            bool.TryParse(val, out b);
            return b;
        }
        public static int GetChildIntAttribute(XmlNode node, string name, string attribute)
        {
            if (node == null) return 0;
            string val = node.SelectSingleNode(name)?.Attributes[attribute]?.InnerText;
            int i;
            int.TryParse(val, out i);
            return i;
        }
        public static float GetChildFloatAttribute(XmlNode node, string name, string attribute)
        {
            if (node == null) return 0;
            string val = node.SelectSingleNode(name)?.Attributes[attribute]?.InnerText;
            float f;
            FloatUtil.TryParse(val, out f);
            return f;
        }
        public static string GetChildStringAttribute(XmlNode node, string name, string attribute)
        {
            if (node == null) return string.Empty;
            string val = node.SelectSingleNode(name)?.Attributes[attribute]?.InnerText;
            return val;
        }
        public static Vector3 GetChildVector3Attributes(XmlNode node, string name, string x, string y, string z)
        {
            float fx = GetChildFloatAttribute(node, name, x);
            float fy = GetChildFloatAttribute(node, name, y);
            float fz = GetChildFloatAttribute(node, name, z);
            return new Vector3(fx, fy, fz);
        }

        public static XmlElement GetChild(XmlElement element, string name)
        {
            return element.SelectSingleNode(name) as XmlElement;
        }

        public static XmlElement AddChild(XmlDocument doc, XmlNode node, string name)
        {
            XmlElement child = doc.CreateElement(name);
            node.AppendChild(child);
            return child;
        }
        public static XmlElement AddChildWithInnerText(XmlDocument doc, XmlNode node, string name, string innerText)
        {
            XmlElement child = AddChild(doc, node, name);
            child.InnerText = innerText;
            return child;
        }
        public static XmlElement AddChildWithAttribute(XmlDocument doc, XmlNode node, string name, string attributeName, string attributeValue)
        {
            XmlElement child = AddChild(doc, node, name);
            child.SetAttribute(attributeName, attributeValue);
            return child;
        }

    }
}
