using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


public class ConfigManager
{
    private readonly string _filePath;

    public ConfigManager()
    {
        string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _filePath = Path.Combine(roamingPath, "FileMeta", "SavedState.xml");
    }

    public void EnsureConfigFile()
    {
        if (!File.Exists(_filePath))
        {
            CreateDefaultConfigFile();
        }
        else
        {
            EnsureNotesProfileExists();
        }
    }

    private void CreateDefaultConfigFile()
    {
        XDocument doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("SavedState",
                new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                new XElement("CustomProfiles",
                    new XElement("Profile",
                        new XElement("Name", "notes"),
                        new XElement("FullDetailsString", "prop:System.PropGroup.Description;System.Title;System.Subject;System.Keywords;System.Category;System.Comment;System.Rating;System.PropGroup.FileSystem;System.ItemNameDisplay;System.ItemType;System.ItemFolderPathDisplay;System.DateCreated;System.DateModified;System.Size;System.FileAttributes;System.OfflineAvailability;System.OfflineStatus;System.SharedWith;System.FileOwner;System.ComputerName"),
                        new XElement("PreviewDetailsString", "prop:*System.DateModified;System.Keywords;System.Rating;*System.Size;System.Title;System.Comment;System.Category;*System.OfflineAvailability;*System.OfflineStatus;System.Subject;*System.DateCreated;*System.SharedWith"),
                        new XElement("InfoTipString", "prop:System.ItemTypeText;System.Size;System.DateModified")
                    )
                )
            )
        );

        Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
        doc.Save(_filePath);
    }

    private void EnsureNotesProfileExists()
    {
        XDocument doc = XDocument.Load(_filePath);
        XElement customProfiles = doc.Descendants("CustomProfiles").FirstOrDefault();

        if (customProfiles != null)
        {
            XElement notesProfile = customProfiles.Descendants("Profile")
                .FirstOrDefault(p => (string)p.Element("Name") == "notes");

            if (notesProfile == null || ShouldReplaceNotesProfile(notesProfile))
            {
                notesProfile?.Remove();
                customProfiles.Add(GetNotesProfileElement());
                doc.Save(_filePath);
            }
        }
    }

    private bool ShouldReplaceNotesProfile(XElement notesProfile)
    {
        // This function can be expanded to check more details or specific conditions
        return true; // Always replace if it exists (based on your requirement)
    }

    private XElement GetNotesProfileElement()
    {
        return new XElement("Profile",
            new XElement("Name", "notes"),
            new XElement("FullDetailsString", "prop:System.PropGroup.Description;System.Title;System.Subject;System.Keywords;System.Category;System.Comment;System.Rating;System.PropGroup.FileSystem;System.ItemNameDisplay;System.ItemType;System.ItemFolderPathDisplay;System.DateCreated;System.DateModified;System.Size;System.FileAttributes;System.OfflineAvailability;System.OfflineStatus;System.SharedWith;System.FileOwner;System.ComputerName"),
            new XElement("PreviewDetailsString", "prop:*System.DateModified;System.Keywords;System.Rating;*System.Size;System.Title;System.Comment;System.Category;*System.OfflineAvailability;*System.OfflineStatus;System.Subject;*System.DateCreated;*System.SharedWith"),
            new XElement("InfoTipString", "prop:System.ItemTypeText;System.Size;System.DateModified")
        );
    }
}

