using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using Frosty.Controls;
using Frosty.Core;
using Frosty.Core.Controls;
using Frosty.Core.Windows;
using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Newtonsoft.Json;

namespace StringsFileUpdater
{
    public class StringsFileUpdateCheckerAction : StartupAction
    {
        WebClient webClient = new WebClient();

        public override Action<ILogger> Action => taskLogger =>
        {
            webClient.Headers.Add(HttpRequestHeader.UserAgent, $"FrostyEditor/1.0.6.3 (version uncertain) StringsFileUpdater/1.0.0.0 (contact at GitHub @ AdamRaichu with concerns)");

            bool updateCheck = Config.Get<bool>("SFU.updateCheck", false);
            if (!updateCheck)
            {
                taskLogger.Log("strings.txt update check is disabled. Skipping...");
                return;
            }
            taskLogger.Log("Checking for updates to strings.txt file...");

            // Retrieve configuration properties
            string username = Config.Get<string>("SFU.username", "adamraichu");
            string repository = Config.Get<string>("SFU.repository", "swbf2hashdatabase");
            string branch = Config.Get<string>("SFU.branch", "new-strings");
            string filePath = Config.Get<string>("SFU.filePath", "strings.txt");
            string lastUpdated = Config.Get<string>("SFU.lastUpdated", "2025-04-01");

            // Construct the API URL dynamically
            string fileApiPath = $"https://api.github.com/repos/{username}/{repository}/commits?sha={branch}&page=1&per_page=1&path={filePath}";

            string fullRemoteVersion = readApi(fileApiPath)[0].Commits.Committer.DateString;
            string remoteVersion = fullRemoteVersion.Split("T".ToCharArray())[0];

            if (needUpdate(remoteVersion))
            {
                MessageBoxResult shouldDownloadUpdate = FrostyMessageBox.Show("A new strings.txt file is available." + Environment.NewLine + "Download the new version automagically?", "Frosty Editor", MessageBoxButton.YesNo);
                if (shouldDownloadUpdate == MessageBoxResult.Yes)
                {
                    // Download the new version and save it to the local directory
                    string localPath = Path.Combine(AppContext.BaseDirectory, "strings.txt");
                    string remotePath = $"https://raw.githubusercontent.com/{username}/{repository}/{branch}/{filePath}";
                    downloadStringsTxt(remotePath, localPath);
                    Config.Add("SFU.lastUpdated", remoteVersion);
                }
                else
                {
                    MessageBoxResult skipThisVersion = FrostyMessageBox.Show("Ignore this version?", "Frosty Editor", MessageBoxButton.YesNo);
                    if (skipThisVersion == MessageBoxResult.Yes)
                    {
                        Config.Add("SFU.lastUpdated", remoteVersion);
                    }
                }
            }
            else
            {
                taskLogger.Log("No new version of strings.txt is available.");
            }
        };

        public List<PartialCommitDetails> readApi(string apiPath)
        {
            return JsonConvert.DeserializeObject<List<PartialCommitDetails>>(webClient.DownloadString(apiPath));
        }

        public bool needUpdate(string remoteVersion)
        {
            // Get the local version from the config
            string localVersion = Config.Get<string>("SFU.lastUpdated", "2025-04-01");

            string[] _splitRemoteVersion = remoteVersion.Split("-".ToCharArray());
            string[] _splitLocalVersion = localVersion.Split("-".ToCharArray());
            int[] splitRemoteVersion = new int[3];
            int[] splitLocalVersion = new int[3];

            if (_splitRemoteVersion.Length != 3)
            {
                return true;
            }
            if (_splitLocalVersion.Length != 3)
            {
                return true;
            }

            // convert to numbers
            for (int i = 0; i < _splitRemoteVersion.Length; i++)
            {
                splitRemoteVersion[i] = int.Parse(_splitRemoteVersion[i]);
                splitLocalVersion[i] = int.Parse(_splitLocalVersion[i]);
            }

            // compare remote version and local version
            if (splitRemoteVersion[0] > splitLocalVersion[0])
            {
                return true;
            }
            if (splitRemoteVersion[0] < splitLocalVersion[0])
            {
                return false;
            }
            if (splitRemoteVersion[1] > splitLocalVersion[1])
            {
                return true;
            }
            if (splitRemoteVersion[1] < splitLocalVersion[1])
            {
                return false;
            }
            if (splitRemoteVersion[2] > splitLocalVersion[2])
            {
                return true;
            }
            // last check is unnecessary

            return false;
        }

        public void downloadStringsTxt(string remotePath, string localPath)
        {
            webClient.DownloadFile(remotePath, localPath);
        }
    }

    public class PartialCommitDetails
    {
        [JsonProperty("commit")]
        public CommitMetadata Commits { get; set; }
    }

    public class CommitMetadata
    {
        [JsonProperty("committer")]
        public CommitterInformation Committer { get; set; }
    }

    public class CommitterInformation
    {
        [JsonProperty("date")]
        public string DateString { get; set; }
    }
}
