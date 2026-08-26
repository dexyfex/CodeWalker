using System;
using System.Configuration;
using System.IO;
using System.Xml;

namespace CodeWalker
{

    internal static class UserConfig
    {
        private const string FileName = "user.config";
        private const string GroupName = "userSettings";
        private const string GroupType = "System.Configuration.UserSettingsGroup, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        private const string SectionType = "System.Configuration.ClientSettingsSection, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";


        public static void Repair()
        {
            Repair(ConfigurationUserLevel.PerUserRoaming);
            Repair(ConfigurationUserLevel.PerUserRoamingAndLocal);
        }


        private static void Repair(ConfigurationUserLevel level)
        {
            var path = GetConfigPath(level);
            if (string.IsNullOrEmpty(path)) return;
            if (string.Equals(Path.GetFileName(path), FileName, StringComparison.OrdinalIgnoreCase) == false) return;
            if (File.Exists(path) == false) return;

            try
            {
                var xdoc = new XmlDocument();
                xdoc.Load(path);
                if (AddSectionGroup(xdoc) == false) return;
                xdoc.Save(path);
            }
            catch
            { }
        }

        private static string GetConfigPath(ConfigurationUserLevel level)
        {
            try
            {
                return ConfigurationManager.OpenExeConfiguration(level).FilePath;
            }
            catch (ConfigurationErrorsException ex)
            {
                return GetErrorFileName(ex);
            }
            catch
            {
                return null;
            }
        }

        private static string GetErrorFileName(ConfigurationErrorsException ex)
        {
            while (ex != null)
            {
                if (string.IsNullOrEmpty(ex.Filename) == false) return ex.Filename;
                ex = ex.InnerException as ConfigurationErrorsException;
            }
            return null;
        }

        private static bool AddSectionGroup(XmlDocument xdoc)
        {
            var root = xdoc.DocumentElement;
            if (root == null) return false;

            var settings = root.SelectSingleNode(GroupName) as XmlElement;
            if (settings == null) return false;
            if (root.SelectSingleNode("configSections/sectionGroup[@name='" + GroupName + "']") != null) return false;

            var group = xdoc.CreateElement("sectionGroup");
            group.SetAttribute("name", GroupName);
            group.SetAttribute("type", GroupType);

            foreach (XmlNode node in settings.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element) continue;
                var section = xdoc.CreateElement("section");
                section.SetAttribute("name", node.Name);
                section.SetAttribute("type", SectionType);
                section.SetAttribute("allowExeDefinition", "MachineToLocalUser");
                section.SetAttribute("requirePermission", "false");
                group.AppendChild(section);
            }
            if (group.ChildNodes.Count == 0) return false;

            var sections = root.SelectSingleNode("configSections") as XmlElement;
            if (sections == null)
            {
                sections = xdoc.CreateElement("configSections");
                root.InsertBefore(sections, root.FirstChild);
            }
            sections.AppendChild(group);
            return true;
        }

    }

}
