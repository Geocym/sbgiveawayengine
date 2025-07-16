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
using StreamUP;

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
    public string RewardString { get; set; }

    [System.Runtime.Serialization.DataMember]
    public int CostChange { get; set; }

    [System.Runtime.Serialization.DataMember]
    public int RewardIndex { get; set; }
    // Not saved
    public bool SaveAsDefault { get; set; }
}

public class WeightedUser
{
    public GroupUser User { get; set; }
    public int Weight { get; set; }
}



public static class SettingsUI
{
    
    
    public static GiveawaySettings ShowSettingsWindow(GiveawaySettings defaultSettings, List<TwitchReward> rewardList)
    {
        CPHInline ge = new CPHInline();
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
        Button btnClear = new Button()
        {
            Text = "Clear Entries",
            Width = buttonWidth,
            Height = buttonHeight,
            Top = 20,
            Left = 20
            //Left = form.ClientSize.Width - buttonWidth * 2 - spacing - buttonPadding,
            //Top = form.ClientSize.Height - buttonHeight - buttonPadding,
            //Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            //DialogResult = DialogResult.OK
        };
        CheckBox chkClearEntries = new CheckBox()
        {
            Text = "Clear Existing Entries",
            Left = 200,
            Top = 200,
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
            Minimum = 1,
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

        Button btnAdvancedSettings = new Button()
{
    Text = "Advanced Settings",
    Width = buttonWidth,
    Height = buttonHeight,
    Left = form.ClientSize.Width - buttonWidth * 3 - spacing - buttonPadding,
    Top = form.ClientSize.Height - buttonHeight - buttonPadding,
    Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
    //Location = new Point(20, 300), // Adjust as needed
    //Size = new Size(150, 30)
};
//btnAdvancedSettings.Click += BtnAdvancedSettings_Click;
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
        toolTip.SetToolTip(btnClear, "Clear all previous giveaway entries.");
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
        toolTip.SetToolTip(btnAdvancedSettings, "Additional settings for weighting winner draws");
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

        btnAdvancedSettings.Click += (s, e) =>
        {
            
        };


        //Clear Entries button logic
        btnClear.Click += (s, e) =>
        {
            ge.ClearEntries(true);
        };

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
        
        form.Controls.AddRange(new Control[] { grpNotifications,btnAdvancedSettings,btnClear, chkClearEntries, lblOverrideCost, btnOpenFolder, numOverrideCost, chkSubscriberOnly, lblRewardName, cmbRewardName, lblTime, numTime, lblFolder, txtFolder, btnBrowse, lblPrizeFile, txtPrizeFile, /*chkWhisper, chkWarningAPI,*/ chkSaveAsDefault, btnOK, btnCancel });
        
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

    
    // Initialise StreamUP Library
    public StreamUpLib SUP;
    public void Init()
    {
        SUP = new StreamUpLib(CPH, "clb001");
    }

    // Variable initialisation
    private string settingsMenuName = "StreamUP Settings | Giveaway Engine";
    public ProductInfo productInfo;
    public string actionName;
    public List<(string fontName, string fontFile, string fontUrl)> requiredFonts;
    public bool LoadProductInfo()
    {
        productInfo = new ProductInfo
        {
            ProductName = "Giveaway Engine",
            ProductNumber = "clb001",
            ProductVersionNumber = new Version(2, 0, 0),
            RequiredLibraryVersion = new Version(2, 0, 0, 0),
            SceneName = "OBSSceneNameHere",
            SourceNameVersionCheck = "OBSSourceNameHere",
            SourceNameVersionNumber = new Version(1, 0, 0, 0),
            SettingsAction = "Giveaway Engine",
        };
        CPH.SetGlobalVar($"{productInfo.ProductNumber}_ProductInfo", productInfo, false);
        return true;
    }

    public List<(string fontName, string fontFile, string fontUrl)> GetRequiredFonts()
    {
        requiredFonts = new List<(string fontName, string fontFile, string fontUrl)>
        {
        //("Digital-7 Mono", "digital-7 (mono).ttf", "https://www.dafont.com/digital-7.font")   
        };
        return requiredFonts;
    }

    public List<Control> AddSettingsToUI(GiveawaySettings defaultSettings)
    {
        string tabName = "Giveaway Settings";
        List<string> listExample = new List<string>
        {
            "item1",
            "item2",
            "item3"
        };
        List<Control> settings = 
        [
        SUP.AddRunMethod("Clear Current Entries", "Clear Entries", "Giveaway Engine v5 (SUP)", "ClearEntriesWrapper", tabName), 
        SUP.AddYesNo("Subscriber Only Mode", defaultSettings.SubscriberOnlyMode, "SaveName", tabName),
        //Consider using custom dropdown as this shows an unfiltered list
        //Also feedback to TD that this needs a new method for owned rewards only
        SUP.AddRewardDrop("Channel Point Reward", "CHANGE ME", "SaveName", tabName),
        SUP.AddInt("Override Cost", 10, 1, 1000000, "SaveName", tabName), 
        SUP.AddLine(tabName), 
        SUP.AddInt("Time until givaway closes (secs)", defaultSettings.TimeToOpenForEntries, 10, 172800, tabName), 
        SUP.AddLine(tabName), 
        SUP.AddFolder("Giveaway Files Folder", "SaveName", tabName), 
        SUP.AddFile("Prize List File", "SaveName", tabName), 
        SUP.AddSpace(tabName), 
        SUP.AddYesNo("Notify Winner by Whisper", defaultSettings.NotifyWinnerByWhisper, "SaveName", tabName), 
        SUP.AddYesNo("Notify Winner by Warning API", defaultSettings.NotifyWinnerByWarningApi, "SaveName", tabName), 
        ];
        return settings;
    }

    public GiveawaySettings ShowSettingsSUP(GiveawaySettings defaultSettings)
    {
        // Load product info from above
        LoadProductInfo();
        // Set save file name
        CPH.SetArgument("saveFile", productInfo.ProductNumber);
        // Get current running actionName
        if (!CPH.TryGetArg("actionName", out actionName))
        {
            SUP.LogError("Unable to retrieve actionName args");
        }

        // Get and check required fonts
        GetRequiredFonts();
        SUP.CheckInstalledFonts(requiredFonts);
        // Load settings Menu
        CPH.Wait(600);
        List<Control> controls = AddSettingsToUI(defaultSettings);
        Application.Run(SUP.BuildForm(settingsMenuName, controls, productInfo, 2));
        // Check settings have initialised correctly
        if (!SUP.InitialiseProduct(actionName, productInfo.ProductNumber, ProductType.Obs))
        {
            SUP.LogError("Product settings are not loaded correctly");
            //return false;
            return null;
        }

        //return true;
        /*
        if (form.ShowDialog() == DialogResult.OK)
        {
            //TwitchReward selected = cmbRewardName.SelectedItem as TwitchReward;
            return new GiveawaySettings
            {/*
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
        }*/
        return null; // User cancelled
    }

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
            using (System.IO.File.Create(gameCodesPath))
                ;
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
            using (System.IO.File.Create(claimedPath))
                ;
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
            using (System.IO.File.Create(enteredUsersPath))
                ;
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

    public bool ClearEntriesWrapper()
    {
        ClearEntries(true);
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
        ShowSettingsSUP(defaultSettings);
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
            //Probably want to move this out to UI subactions so UI thread is not locked.
            //Alternatively look into asynchronous operaton
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
        List<WeightedUser> weightedEntries = entries.Select(u => new WeightedUser
{
    User = u,
    Weight = GetWeightForUser(u) // your custom logic here
}).ToList();


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
        //var winner = entries.OrderBy(s => random.NextDouble()).First();
        var winner = WeightedRandomSelection(weightedEntries, random).User;
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

private WeightedUser WeightedRandomSelection(List<WeightedUser> users, Random rand)
{
    int totalWeight = users.Sum(u => u.Weight);
    int randomValue = rand.Next(0, totalWeight);
    int cumulative = 0;
    foreach (var user in users)
    {
        cumulative += user.Weight;
        if (randomValue < cumulative)
        {
            return user;
        }
    }
    return users.Last(); // Fallback
}

private int GetWeightForUser(GroupUser user)
{
    TwitchUserInfoEx info = CPH.TwitchGetExtendedUserInfoByLogin(user.ToString());
    bool weightSystem = CPH.GetGlobalVar<bool>("weightedGiveaway",false);

    int weight = 100; // base weight for all users

    //Allow weight system to be disabled entirely
    if(!weightSystem)
        return weight;
        
    // Boost for active supporters
    if (info.IsSubscribed)
    {
        int tier1Bonus = CPH.GetGlobalVar<int>("t1Bonus", false);
        int tier2Bonus = CPH.GetGlobalVar<int>("t2Bonus", false);
        int tier3Bonus = CPH.GetGlobalVar<int>("t3Bonus", false);
        switch (info.SubscriptionTier)
        {
            //Replace fixed weights with variables from UI later
            case "1000": weight += tier1Bonus; break; // Tier 1
            case "2000": weight += tier2Bonus; break; // Tier 2
            case "3000": weight += tier3Bonus; break; // Tier 3
        }
    }
    //Boost VIPs
    int vipBonus = CPH.GetGlobalVar<int>("vipBonus", false);
    if (info.IsVip) weight += vipBonus;

    //Boost Moderators
    int modBonus = CPH.GetGlobalVar<int>("modBonus", false);
    if (info.IsModerator) weight += modBonus;

    // Loyalty bonus for following
    int followerBonus = CPH.GetGlobalVar<int>("followerBonus", false);
    if (info.IsFollowing) weight += followerBonus;

    // Suppress if user was recently created 
    int youngAccount = CPH.GetGlobalVar<int>("minAccountAgeDays",false);
    int youngAccountPenalty = CPH.GetGlobalVar<int>("youngAccountPenalty",false);
    if ((DateTime.Now - info.CreatedAt).TotalDays <= youngAccount)
        weight -= youngAccountPenalty;

    //Suppress if user won last giveaway
    string lastWinner = CPH.GetGlobalVar<string>("lastGiveawayWinner", false);
    bool consecutiveWins = CPH.GetGlobalVar<bool>("lastWinFairness",false);
    int consecutiveWinPenalty = CPH.GetGlobalVar<int>("lastWinPenalty",false);
    if(info.UserLogin == lastWinner && consecutiveWins)
        weight -= consecutiveWinPenalty;

    
    return weight;
}


}
