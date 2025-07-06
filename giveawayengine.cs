using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Globalization;
using System.Diagnostics;

[System.Runtime.Serialization.DataContract]
public class GiveawaySettings
{
    [System.Runtime.Serialization.DataMember]
    public bool ClearExistingEntries { get; set; }

    [System.Runtime.Serialization.DataMember]
    public bool SubscriberOnlyMode { get; set; }

    [System.Runtime.Serialization.DataMember]
    public int TimeToOpenForEntries { get; set; }

    [System.Runtime.Serialization.DataMember]
    public string GiveawayFilesFolder { get; set; }

    [System.Runtime.Serialization.DataMember]
    public string PrizeFileName { get; set; }

    [System.Runtime.Serialization.DataMember]
    public bool NotifyWinnerByWhisper { get; set; }

    [System.Runtime.Serialization.DataMember]
    public bool NotifyWinnerByWarningApi { get; set; }

    [System.Runtime.Serialization.DataMember]
    public TwitchReward RewardName { get; set; }

    [System.Runtime.Serialization.DataMember]
    public string RewardId { get; set; }

    [System.Runtime.Serialization.DataMember]
    public int CostChange { get; set; }

    [System.Runtime.Serialization.DataMember]
    public int RewardIndex { get; set; }
    // Not saved
    public bool SaveAsDefault { get; set; }
}

public static class SettingsUI
{
    public static GiveawaySettings ShowSettingsWindow(GiveawaySettings defaultSettings, List<TwitchReward> rewardList)
    {
        //Alphabetise the Channel Point List
        rewardList = rewardList.OrderBy(r => r.Title).ToList();
        //Set default channel point reward
        int rewardIndex = rewardList.FindIndex(a => a.Id == defaultSettings.RewardId);
        if (rewardIndex == -1)
            rewardIndex = 0; // set to first if not found
        var reward = rewardList.FirstOrDefault(a => a.Id == defaultSettings.RewardId);
        //Define standard sizing
        int formWidth = 400;
        int formHeight = 460;
        int buttonWidth = 80;
        int buttonHeight = 30;
        int buttonPadding = 20;
        int spacing = 10;
        int labelWidth = 180;
        //Start form layout
        var form = new Form()
        {
            Text = "Giveaway Settings",
            Size = new Size(formWidth, formHeight),
            StartPosition = FormStartPosition.CenterScreen,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };
        // Controls
        // First row: Two checkboxes
        CheckBox chkClearEntries = new CheckBox()
        {
            Text = "Clear Existing Entries",
            Left = 20,
            Top = 20,
            Width = 160
        };
        CheckBox chkSubscriberOnly = new CheckBox()
        {
            Text = "Subscriber Only Mode",
            Left = 200,
            Top = 20,
            Width = 160
        };
        // Reward Row
        Label lblRewardName = new Label()
        {
            Text = "Channel Point Reward",
            Left = 20,
            Top = 55,
            Width = labelWidth
        };
        ComboBox cmbRewardName = new ComboBox()
        {
            Left = 200,
            Top = 50,
            Width = 160,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        //Show channel point names, but store their GUIDs
        cmbRewardName.ValueMember = "Id";
        cmbRewardName.DisplayMember = "Title";
        //Populate the dropdown - using datasource is incompatible with setting defaults
        foreach (var ri in rewardList.OrderBy(r => r.Title))
            cmbRewardName.Items.Add(ri);
        cmbRewardName.SelectedIndex = rewardIndex;
        Label lblOverrideCost = new Label()
        {
            Text = "Override Reward Cost:",
            Left = 20,
            Top = 85,
            Width = labelWidth
        };
        NumericUpDown numOverrideCost = new NumericUpDown()
        {
            Left = 200,
            Top = 80,
            Width = 160,
            Minimum = 0,
            Maximum = 1000000
        };
        Label lblTime = new Label()
        {
            Text = "Time to Open for Entries (sec):",
            Left = 20,
            Top = 115,
            Width = labelWidth
        };
        NumericUpDown numTime = new NumericUpDown()
        {
            Left = 200,
            Top = 110,
            Width = 160,
            Minimum = 1,
            Maximum = 3600
        };
        //Functionality to update cost override control based on selected reward
        cmbRewardName.SelectedIndexChanged += (s, e) =>
        {
            if (cmbRewardName.SelectedItem is TwitchReward selectedReward)
            {
                numOverrideCost.Value = Math.Min(Math.Max(selectedReward.Cost, numOverrideCost.Minimum), numOverrideCost.Maximum);
            }
        };
        // After setting cmbRewardName.SelectedIndex = rewardIndex;
        void UpdateOverrideCost()
        {
            if (cmbRewardName.SelectedItem is TwitchReward selectedReward)
            {
                numOverrideCost.Value = Math.Min(Math.Max(selectedReward.Cost, numOverrideCost.Minimum), numOverrideCost.Maximum);
            }
        }

        cmbRewardName.SelectedIndex = rewardIndex;
        cmbRewardName.SelectedIndexChanged += (s, e) => UpdateOverrideCost();
        cmbRewardName.SelectedIndex = rewardIndex;
        UpdateOverrideCost();
        // Folder Path
        Label lblFolder = new Label()
        {
            Text = "Giveaway Files Folder:",
            Left = 20,
            Top = 145,
            Width = labelWidth
        };
        TextBox txtFolder = new TextBox()
        {
            Left = 20,
            Top = 167,
            Width = 230
        };
        Button btnBrowse = new Button()
        {
            Text = "Browse...",
            Left = 260,
            Top = 165,
            Width = 60
        };
        Button btnOpenFolder = new Button()
        {
            Text = "📂",
            Font = new Font("Segoe UI Emoji", 10), // Ensures good emoji rendering
            Width = 30,
            Height = 30,
            Left = btnBrowse.Right + 5,
            Top = txtFolder.Top - 5
        };
        // Prize File
        Label lblPrizeFile = new Label()
        {
            Text = "Prize File Name:",
            Left = 20,
            Top = 195,
            Width = labelWidth
        };
        TextBox txtPrizeFile = new TextBox()
        {
            Left = 20,
            Top = 217,
            Width = 340
        };
        // Notification Group
        GroupBox grpNotifications = new GroupBox()
        {
            Text = "Winner Notifications",
            Left = 20,
            Top = 255,
            Width = 350,
            Height = 80
        };
        CheckBox chkWhisper = new CheckBox()
        {
            Text = "Notify Winner by Whisper",
            Left = 10,
            Top = 20,
            Width = 250
        };
        CheckBox chkWarningAPI = new CheckBox()
        {
            Text = "Notify Winner by Warning API",
            Left = 10,
            Top = 45,
            Width = 250
        };
        grpNotifications.Controls.Add(chkWhisper);
        grpNotifications.Controls.Add(chkWarningAPI);
        // ✅ New checkbox for saving as default
        CheckBox chkSaveAsDefault = new CheckBox()
        {
            Text = "Save as default settings",
            Left = 20,
            Top = form.ClientSize.Height - buttonHeight - buttonPadding + 5,
            Width = 170,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        Button btnOK = new Button()
        {
            Text = "OK",
            Width = buttonWidth,
            Height = buttonHeight,
            Left = form.ClientSize.Width - buttonWidth * 2 - spacing - buttonPadding,
            Top = form.ClientSize.Height - buttonHeight - buttonPadding,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            DialogResult = DialogResult.OK
        };
        Button btnCancel = new Button()
        {
            Text = "Cancel",
            Width = buttonWidth,
            Height = buttonHeight,
            Left = form.ClientSize.Width - buttonWidth - buttonPadding,
            Top = form.ClientSize.Height - buttonHeight - buttonPadding,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            DialogResult = DialogResult.Cancel
        };
        // ToolTips
        ToolTip toolTip = new ToolTip();
        toolTip.AutoPopDelay = 5000;
        toolTip.InitialDelay = 500;
        toolTip.ReshowDelay = 500;
        toolTip.ShowAlways = true;
        toolTip.SetToolTip(chkClearEntries, "If checked, all previous giveaway entries will be removed.");
        toolTip.SetToolTip(chkSubscriberOnly, "Only subscribers will be allowed to enter the giveaway.");
        toolTip.SetToolTip(lblRewardName, "Choose the Channel Point Reward users will use to enter the giveaway.");
        toolTip.SetToolTip(numTime, "How many seconds the giveaway will stay open for entries.");
        toolTip.SetToolTip(txtFolder, "The folder where giveaway-related files are stored. Leave blank to use Streamer.bot folder");
        toolTip.SetToolTip(btnBrowse, "Click to choose a folder to store giveaway files.");
        toolTip.SetToolTip(txtPrizeFile, "Name of the file that contains the prizes.");
        toolTip.SetToolTip(chkWhisper, "If checked, the winner will be notified via Twitch whisper.");
        toolTip.SetToolTip(chkWarningAPI, "If checked, the winner will be notified via the Twitch warning API.");
        toolTip.SetToolTip(chkSaveAsDefault, "Check to use these settings as the default for next time.");
        toolTip.SetToolTip(btnOpenFolder, "Open the giveaway files folder in File Explorer.");
        toolTip.SetToolTip(numOverrideCost, "Change the cost of the selected channel point reward.");
        // Apply defaults
        if (defaultSettings != null)
        {
            chkClearEntries.Checked = defaultSettings.ClearExistingEntries;
            chkSubscriberOnly.Checked = defaultSettings.SubscriberOnlyMode;
            numTime.Value = Math.Min(Math.Max(defaultSettings.TimeToOpenForEntries, numTime.Minimum), numTime.Maximum);
            txtFolder.Text = defaultSettings.GiveawayFilesFolder ?? "";
            txtPrizeFile.Text = defaultSettings.PrizeFileName ?? "";
            chkWhisper.Checked = defaultSettings.NotifyWinnerByWhisper;
            chkWarningAPI.Checked = defaultSettings.NotifyWinnerByWarningApi;
            cmbRewardName.SelectedItem = reward;
        }

        // Folder browser logic
        btnBrowse.Click += (s, e) =>
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtFolder.Text = dialog.SelectedPath;
                }
            }
        };
        // Open folder button logic
        btnOpenFolder.Click += (s, e) =>
        {
            string folderPath = txtFolder.Text.Trim();
            if (string.IsNullOrEmpty(folderPath))
            {
                folderPath = AppDomain.CurrentDomain.BaseDirectory;
            }

            if (Directory.Exists(folderPath))
            {
                try
                {
                    Process.Start("explorer.exe", folderPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open folder:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("The specified folder does not exist.", "Invalid Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };
        form.Controls.AddRange(new Control[] { grpNotifications, chkClearEntries, lblOverrideCost, btnOpenFolder, numOverrideCost, chkSubscriberOnly, lblRewardName, cmbRewardName, lblTime, numTime, lblFolder, txtFolder, btnBrowse, lblPrizeFile, txtPrizeFile, /*chkWhisper, chkWarningAPI,*/ chkSaveAsDefault, btnOK, btnCancel });
        form.AcceptButton = btnOK;
        form.CancelButton = btnCancel;
        if (form.ShowDialog() == DialogResult.OK)
        {
            TwitchReward selected = cmbRewardName.SelectedItem as TwitchReward;
            return new GiveawaySettings
            {
                ClearExistingEntries = chkClearEntries.Checked,
                SubscriberOnlyMode = chkSubscriberOnly.Checked,
                RewardName = selected,
                RewardId = selected.Id,
                TimeToOpenForEntries = (int)numTime.Value,
                GiveawayFilesFolder = txtFolder.Text,
                PrizeFileName = txtPrizeFile.Text,
                NotifyWinnerByWhisper = chkWhisper.Checked,
                NotifyWinnerByWarningApi = chkWarningAPI.Checked,
                SaveAsDefault = chkSaveAsDefault.Checked, // ✅ Capture user intent to save
                RewardIndex = rewardIndex,
                CostChange = (int)numOverrideCost.Value
            };
        }

        return null; // User cancelled
    }

    public static class GiveawaySettingsStorage
    {
        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "giveaway_settings.json");
        public static void Save(GiveawaySettings settings)
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                using (var stream = new FileStream(SettingsFilePath, FileMode.Create))
                {
                    var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(GiveawaySettings));
                    serializer.WriteObject(stream, settings);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving settings: " + ex.Message);
            }
        }

        public static GiveawaySettings Load()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                    return null;
                using (var stream = new FileStream(SettingsFilePath, FileMode.Open))
                {
                    var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(GiveawaySettings));
                    return (GiveawaySettings)serializer.ReadObject(stream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading settings: " + ex.Message);
                return null;
            }
        }
    }
}

public class CPHInline
{
    /*
    public List<TwitchReward> rewardList()
    {
        List<TwitchReward> rewardList = CPH.TwitchGetRewards();
        return rewardList;
    }
*/
    public string giveawayGroup = "Giveaway Entries";
    public string prizesFile()
    {
        string prizeFileName = CPH.GetGlobalVar<string?>("giveawayPrizeFileName", true) ?? "gamecodes.txt"; //Pull prize file name from Global Variables or default to gamecodes.txt
        return prizeFileName;
    }

    public string claimedPrizesFile = "claimedPrizes.txt";
    public string entriesFile = "enteredusers.txt";
    public string giveawayFolder()
    {
        //Pull in the codesFolder global - Will use streamerbot folder if not set
        string codesFolder = CPH.GetGlobalVar<string?>("giveawayPath", true) ?? AppDomain.CurrentDomain.BaseDirectory; //Pull directory path from Global Variables or default to SB directory
        //Create the folder if it doesnt exist
        if (!Directory.Exists(codesFolder))
        {
            Directory.CreateDirectory(codesFolder);
        }

        return codesFolder;
    }

    public string PrizeFilePath()
    {
        //Define the absolute paths for the files
        string gameCodesPath = Path.Combine(giveawayFolder(), prizesFile());
        //Create empty files if they are not found in the given directory
        if (!File.Exists(gameCodesPath))
        {
            using (System.IO.File.Create(gameCodesPath));
        }

        return gameCodesPath;
    }

    public string ClaimedFilePath()
    {
        //Define the absolute paths for the files
        string claimedPath = Path.Combine(giveawayFolder(), claimedPrizesFile);
        //Create empty files if they are not found in the given directory
        if (!File.Exists(claimedPath))
        {
            using (System.IO.File.Create(claimedPath));
        }

        return claimedPath;
    }

    public string EntriesFilePath()
    {
        //Define the absolute paths for the files
        string enteredUsersPath = Path.Combine(giveawayFolder(), entriesFile);
        //Create empty files if they are not found in the given directory
        if (!File.Exists(enteredUsersPath))
        {
            using (System.IO.File.Create(enteredUsersPath));
        }

        return enteredUsersPath;
    }

    public bool ModAddToGiveaway()
    {
        //For Moderators to add people to giveaway manually using a command
        CPH.TryGetArg("targetUser", out string userName);
        CPH.AddUserToGroup(userName, Platform.Twitch, giveawayGroup);
        return true;
    }

    public bool RemoveFromEntries(string target) //This function is probably redundant, but its here for modular expansion
    {
        CPH.RemoveUserFromGroup(target, Platform.Twitch, giveawayGroup);
        return true;
    }

    public bool RemoveFromEntriesManual() //This function is probably redundant, but its here for modular expansion
    {
        CPH.TryGetArg("targetUserName", out string targetUserName);
        CPH.RemoveUserFromGroup(targetUserName, Platform.Twitch, giveawayGroup);
        return true;
    }

    public bool ClearEntries(bool clearCurrent)
    {
        if (clearCurrent)
        {
            //do something 
            CPH.ClearUsersFromGroup(giveawayGroup);
            File.WriteAllText(EntriesFilePath(), String.Empty);
        }

        return true;
    }

    public bool AddToGiveaway()
    {
        //Collect Twitch Reward arguments - this allows for automatic refunds on failure
        CPH.TryGetArg("rewardId", out string rewardId);
        CPH.TryGetArg("redemptionId", out string redemptionId);
        //Compare to reward global fail out if incorrect reward
        string storedRewardId = CPH.GetGlobalVar<string>("giveawayRewardId", true);
        if (rewardId == storedRewardId)
        {
        }
        else
            return false;
        //When users use the channel point, add them to the Giveaway Entries group
        CPH.TryGetArg("userName", out string userName);
        CPH.TryGetArg("userId", out string userID);
        CPH.TryGetArg("isSubscribed", out bool isSubscribed);
        // if group does not exist, create it
        if (!CPH.GroupExists(giveawayGroup))
        {
            CPH.AddGroup(giveawayGroup);
        }

        bool userCanEnter = true;
        bool userEntered = CPH.UserInGroup(userName, Platform.Twitch, giveawayGroup);
        bool subOnly = CPH.GetGlobalVar<bool>("subOnlyGiveaway", false);
        CPH.SetArgument("subMode", subOnly);
        //Deny entry if user has already entered or if is not subscribed in a sub only giveaway
        if ((subOnly && !isSubscribed) || userEntered)
        {
            userCanEnter = false;
        }

        if (!userCanEnter)
        {
            CPH.TwitchRedemptionCancel(rewardId, redemptionId); //Auto refund points if user inelligible - Only works if Streamerbot created the Channel Point Reward
            return false;
        }

        CPH.AddUserToGroup(userName, Platform.Twitch, giveawayGroup);
        //Output Giveaway group to text file so OBS can easily read it as a source
        List<GroupUser> entries = CPH.UsersInGroup(giveawayGroup);
        List<string> users = [];
        foreach (GroupUser userInfo in entries)
        {
            users.Add(userInfo.Username);
        }

        System.IO.File.WriteAllLines(EntriesFilePath(), users); //Save new prize pool to file
        return true;
    }

    public bool OpenGiveaway()
    {
        //Prepopulate list of Channel Point Rewards
        List<TwitchReward> fullRewardList = CPH.TwitchGetRewards();
        List<TwitchReward> filteredRewards = fullRewardList.Where(r => r.IsOurs).ToList();
        //Load settings from JSON file or default to these settings if no file found
        GiveawaySettings defaultSettings = SettingsUI.GiveawaySettingsStorage.Load() ?? new GiveawaySettings
        {
            ClearExistingEntries = true,
            SubscriberOnlyMode = false,
            TimeToOpenForEntries = 60,
            GiveawayFilesFolder = @"",
            PrizeFileName = "prizes.txt",
            NotifyWinnerByWhisper = false,
            NotifyWinnerByWarningApi = true,
            RewardId = ""
        };
        GiveawaySettings userSettings = SettingsUI.ShowSettingsWindow(defaultSettings, filteredRewards);
        // Save new defaults if the user checked the box
        if (userSettings != null && userSettings.SaveAsDefault)
        {
            SettingsUI.GiveawaySettingsStorage.Save(userSettings);
        }

        if (userSettings != null)
        {
            // User clicked OK, now read values
            bool clearEntries = userSettings.ClearExistingEntries;
            bool subscriberOnly = userSettings.SubscriberOnlyMode;
            int timeToOpen = userSettings.TimeToOpenForEntries;
            string giveawayFolder = userSettings.GiveawayFilesFolder;
            string prizeFile = userSettings.PrizeFileName;
            bool notifyWhisper = userSettings.NotifyWinnerByWhisper;
            bool notifyWarningApi = userSettings.NotifyWinnerByWarningApi;
            bool saveDefaults = userSettings.SaveAsDefault;
            string rewardId = userSettings.RewardName.Id;
            string rewardName = userSettings.RewardName.Title;
            int rewardIndex = userSettings.RewardIndex;
            int costOverride = userSettings.CostChange;
            //If Subscriber only, set global to reflect this
            CPH.SetGlobalVar("subOnlyGiveaway", subscriberOnly, false);
            //If Clear current true, blank group and clear entrants file
            ClearEntries(clearEntries);
            //Set Globals for Giveaway files
            if (giveawayFolder == @"")
            {
                CPH.SetGlobalVar("giveawayPath", AppDomain.CurrentDomain.BaseDirectory, true);
            }
            else
            {
                CPH.SetGlobalVar("giveawayPath", giveawayFolder, true);
            }

            CPH.SetGlobalVar("giveawayPrizeFileName", prizeFile, true);
            CPH.SetGlobalVar("giveawayWhisperMode", notifyWhisper, true);
            CPH.SetGlobalVar("giveawayWarningMode", notifyWarningApi, true);
            CPH.SetArgument("rewardIndex", rewardIndex);
            //PLACEHOLDER FOR CONFIGURING CHANNEL POINT
            CPH.SetGlobalVar("giveawayRewardId", rewardId, true);
            CPH.UpdateRewardCost(rewardId, costOverride, false);
            CPH.EnableReward(rewardId);
            //Announce giveaway is open
            CPH.TwitchAnnounce($"A giveaway has started, you have {timeToOpen} seconds to enter. Please use channel point reward {rewardName} for a chance to win!");
            //Delay for set time
            CPH.SetArgument("giveawayTimer", timeToOpen * 1000);
            CPH.Wait(timeToOpen * 1000);
            //Disable Channel Point Reward
            CPH.DisableReward(rewardId);
        }
        else
        {
            CPH.LogDebug("Giveaway Settings window was cancelled.");
        }

        return true;
    }

    public bool PickWinner()
    {
        CPH.TryGetArg("testMode", out bool testMode);
        List<GroupUser> entries = CPH.UsersInGroup(giveawayGroup);
        //Generate prize array
        List<string> prizes = File.ReadAllLines(PrizeFilePath()).ToList();
        Random rand = new Random();
        int idx = rand.Next(0, prizes.Count);
        object code = prizes[idx];
        string codeString = code.ToString(); //Save prize code as string
        //Do not remove prizes in test mode
        if (!testMode)
        {
            prizes.RemoveAt(idx); //Remove code from Prize pool
            System.IO.File.WriteAllLines(PrizeFilePath(), prizes); //Save new prize pool to file
        }

        //Pick Winner
        Random random = new Random();
        var winner = entries.OrderBy(s => random.NextDouble()).First();
        CPH.TwitchAnnounce($"{winner.Username} is the winner");
        //If running in test mode, log the result but do not send an actual message
        if (!testMode)
        {
            //Also log prize and winner to the claimedPrizes file 
            using (StreamWriter sw = File.AppendText(ClaimedFilePath()))
            {
                sw.WriteLine($"{DateTime.Now} :: {winner.Username} :: {codeString}");
                //Send prize to selected announcement channels
                CPH.TwitchWarnUser(winner.Username, $"You have won {codeString} please press acknowledge once claimed. Any problems please contact a moderator.");
                CPH.SendWhisper(winner.Username, $"You have won {codeString} Any problems please contact a moderator.", true); //Sends code via whisper also
            }
        }
        else
        {
            CPH.LogDebug($"Givaway DEBUG: {winner.Username}, You have won {codeString} Any problems please contact a moderator.");
        }

        //Popup to remove winner from pool
        DialogResult dialogResult = MessageBox.Show($"Remove {winner.Username} from entry pool?", "Remove Winner?", MessageBoxButtons.YesNo);
        if (dialogResult == DialogResult.Yes)
        {
            //Remove user from the pool
            RemoveFromEntries(winner.Username);
        }
        else if (dialogResult == DialogResult.No)
        {
        //do nothing
        }

        //Pick another winner?
        DialogResult rerunResult = MessageBox.Show($"Pick another winner?", "Draw again?", MessageBoxButtons.YesNo);
        if (rerunResult == DialogResult.Yes)
        {
            PickWinner();
        }
        else
        {
            return true;
        }

        return true;
    }
}