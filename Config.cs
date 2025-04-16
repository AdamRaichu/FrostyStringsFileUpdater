using Frosty.Core;
using Frosty.Core.Controls.Editors;
using FrostySdk.Attributes;
using FrostySdk.IO;

namespace StringsFileUpdater
{
    [DisplayName("Strings File Updater")]
    public class StringsFileUpdaterConfig : OptionsExtension
    {
        [Category("General")]
        [DisplayName("Update Check?")]
        [Description("Check for updates to the strings.txt file. If false this plugin will do nothing.")]
        [Editor(typeof(FrostyBooleanEditor))]
        [EbxFieldMeta(EbxFieldType.Boolean)]
        public bool updateCheck { get; set; } = false;

        [Category("GitHub API")]
        [DisplayName("GitHub Username")]
        [Description("The username of the GitHub repository owner.")]
        [Editor(typeof(FrostyStringEditor))]
        [EbxFieldMeta(EbxFieldType.String)]
        [DependsOn("updateCheck")]
        public string username { get; set; } = "adamraichu";

        [Category("GitHub API")]
        [DisplayName("Repository Name")]
        [Description("The name of the GitHub repository.")]
        [Editor(typeof(FrostyStringEditor))]
        [EbxFieldMeta(EbxFieldType.String)]
        [DependsOn("updateCheck")]
        public string repository { get; set; } = "swbf2hashdatabase";

        [Category("GitHub API")]
        [DisplayName("Branch Name")]
        [Description("The branch to check for updates.")]
        [Editor(typeof(FrostyStringEditor))]
        [EbxFieldMeta(EbxFieldType.String)]
        [DependsOn("updateCheck")]
        public string branch { get; set; } = "new-strings";

        [Category("GitHub API")]
        [DisplayName("File Path")]
        [Description("The path to the file in the repository.")]
        [Editor(typeof(FrostyStringEditor))]
        [EbxFieldMeta(EbxFieldType.String)]
        [DependsOn("updateCheck")]
        public string filePath { get; set; } = "strings.txt";

        // YYYY.MM.DD version, 1 based index
        public string lastUpdated { get; set; } = "2025.04.01";

        public override void Load()
        {
            updateCheck = Config.Get<bool>("SFU.updateCheck", false);
            username = Config.Get<string>("SFU.username", "adamraichu");
            repository = Config.Get<string>("SFU.repository", "swbf2hashdatabase");
            branch = Config.Get<string>("SFU.branch", "new-strings");
            filePath = Config.Get<string>("SFU.filePath", "strings.txt");
            lastUpdated = Config.Get<string>("SFU.lastUpdated", "2025-04-01");
        }

        public override void Save()
        {
            Config.Add("SFU.updateCheck", updateCheck);
            Config.Add("SFU.username", username);
            Config.Add("SFU.repository", repository);
            Config.Add("SFU.branch", branch);
            Config.Add("SFU.filePath", filePath);
            Config.Add("SFU.lastUpdated", lastUpdated);
            Config.Save();
        }

        public override bool Validate()
        {
            return !string.IsNullOrWhiteSpace(username) &&
                   !string.IsNullOrWhiteSpace(repository) &&
                   !string.IsNullOrWhiteSpace(branch) &&
                   !string.IsNullOrWhiteSpace(filePath);
        }
    }
}
