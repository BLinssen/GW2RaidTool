using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics; //For process check
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net; //For download
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp;

namespace ArcUpdater
{



    public partial class RaidToolForm : Form
    {
        private string InstallPath = Application.StartupPath;
        private string GW2Path;
        private string LogPath;
        private string ArcPath;
        private string ConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\GW2BLinssenRaidTool";
        private string ConfigFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\GW2BLinssenRaidTool\\config.txt";
        private string Main = "\\d3d9.dll";
        private string BuildTemplates = "\\d3d9_arcdps_buildtemplates.dll";
        private string Chainloader = "\\d3d3_chainload.dll";
        private string MechanicsLog = "\\d3d9_arcdps_mechanics.dll";
        private string ArcMain;
        private string ArcBuildTemplates;
        private string ArcChainloader;
        private string ArcMechanicsLog;
        private string ArcBackup;
        private string ArcBackupMain;
        private string ArcBackupBuildTemplates;
        private string ArcBackupChainloader;
        private string ArcBackupMechanicsLog;
        private string GW2Process = "Gw2-64";
        private string[,] ComponentInfo;
        private string[] Settings;
        private string[] CategoriesArray;
        private string updatestring;
        private string startdate;
        private string enddate;
        private bool initial = true;

        List<string> ToUploadFileName = new List<string>();
        List<string> ToUploadFileFull = new List<string>();

        private DateTime startdatedt;
        private DateTime enddatedt;

        private string bossNaam;
        private string bestandNaam;
        private string datum;
        private DateTime datumdt;
        private string bestandType;
        private string pad;

        private string token;

        public RaidToolForm()
        {
            InitializeComponent();

            #region declarations

            InstallPath = Application.StartupPath;
            textBoxGW2Folder.Text = GW2Path;
            textBoxLogFolder.Text = LogPath;
            ArcPath = GW2Path + "\\bin64";
            ArcBackup = GW2Path + "\\addons\\arcdps\\arcdps.backups";
            ArcMain = ArcPath + Main;
            ArcBuildTemplates = ArcPath + BuildTemplates;
            ArcChainloader = ArcPath + Chainloader;
            ArcMechanicsLog = ArcPath + MechanicsLog;
            ArcBackupMain = ArcBackup + Main;
            ArcBackupBuildTemplates = ArcBackup + BuildTemplates;
            ArcBackupChainloader = ArcBackup + Chainloader;
            ArcBackupMechanicsLog = ArcBackup + MechanicsLog;
            Settings = new string[] { "", "", "", "", "", "", "", "", "", "", "", "", "" };
            Config();
            UpdatePaths();
            ComponentInfo = new string[4, 4] {
                { ArcMain, "Main Comp", ArcBackupMain, "https://www.deltaconnected.com/arcdps/x64/d3d9.dll" },
                { ArcBuildTemplates, "Build Templates", ArcBackupBuildTemplates, "https://www.deltaconnected.com/arcdps/x64/buildtemplates/d3d9_arcdps_buildtemplates.dll" },
                { ArcChainloader, "Chainloader", ArcBackupChainloader, "https://www.deltaconnected.com/arcdps/x64/reshade_loader/d3d9_chainload.dll" },
                { ArcMechanicsLog, "Mechanics Log", ArcBackupMechanicsLog, "http://martionlabs.com/wp-content/uploads/d3d9_arcdps_mechanics.dll" } };
            SelectedWeek();
            UpdateList();
            FillConfig();
            CategoriesArray = new string[] { "Log Category" ,"Guild / Static", "Training", "PUG", "Low Man / Sells" };
            comboBoxTag.DataSource = CategoriesArray;
            StatusLabel.Text = "Idle";
            #endregion
        }

        private void UpdateArc()
        {
            StatusLabel.Text = "Check if GW2 is running";
            if (IsProcessOpen(GW2Process) == true)
            {
                StatusLabel.Text = "Error, Guild Wars 2 is running";
                MessageBox.Show("Please close Guild Wars 2 before updating.", "Guild Wars 2 is currently running", MessageBoxButtons.OK);
            }
            else
            {
                UpdatePaths();
                StatusLabel.Text = "Started update";
                updatestring = "Updated: ";
                if (!System.IO.Directory.Exists(ArcBackup))
                {
                    System.IO.Directory.CreateDirectory(ArcBackup);
                }
                if (!System.IO.Directory.Exists(GW2Path + "\\bin64"))
                {
                    MessageBox.Show("The selected directory does not look like it contains Guild Wars 2\nPlease select the correct directory.", "Unknown folder selected", MessageBoxButtons.OK);
                    StatusLabel.Text = "Error, wrong directory";
                }
                else
                {
                    if (checkBoxMain.Checked == true)
                    {
                        UpdateComponent(0);
                        updatestring += ComponentInfo[0, 1] + ", ";
                    }
                    if (checkBoxBuildTemplates.Checked == true)
                    {
                        UpdateComponent(1);
                        updatestring += ComponentInfo[1, 1] + ", ";
                    }
                    if (checkBoxChainloader.Checked == true)
                    {
                        UpdateComponent(2);
                        updatestring += ComponentInfo[2, 1] + ", ";
                    }
                    if (checkBoxMechanicsLog.Checked == true)
                    {
                        UpdateComponent(3);
                        updatestring += ComponentInfo[3, 1] + ", ";
                    }
                    updatestring = updatestring.Substring(0, updatestring.Length - 2);
                    if (updatestring == "Updated: Main Comp, Build Templates, Chainloader, Mechanics Log")
                    {
                        updatestring = "Updated: All components";
                    }
                    StatusLabel.Text = updatestring;
                }
            }
        }
        private void UpdateComponent(int comp)
        {
            StatusLabel.Text = "Making Backup of " + ComponentInfo[comp, 1];
            if (System.IO.File.Exists(ComponentInfo[comp, 0]))
            {
                System.IO.File.Copy(ComponentInfo[comp, 0], ComponentInfo[comp, 2], true);
            }
            StatusLabel.Text = "Downloading " + ComponentInfo[comp, 1];
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    webClient.DownloadFile(ComponentInfo[comp, 3], ComponentInfo[comp, 0]);
                    StatusLabel.Text = "Updated " + ComponentInfo[comp, 1];

                }
                catch (WebException)
                {
                    StatusLabel.Text = "Error, download failed";
                    MessageBox.Show("One or more download links are unavailable.", "Download failed", MessageBoxButtons.OK);
                }
            }
        }
        public bool IsProcessOpen(string GW2Process)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(GW2Process))
                {
                    return true;
                }
            }
            return false;
        }
        private void ButtonDeleteArc_Click(object sender, EventArgs e)
        {
            string deletestring = "Deleted all ArcDps files";
            if (System.IO.File.Exists(ArcMain))
            {
                try
                {
                    System.IO.File.Delete(ArcMain);
                }
                catch (System.IO.IOException)
                {
                    deletestring = "Error, could not delete all files";
                }
            }
            if (System.IO.File.Exists(ArcBuildTemplates))
            {
                try
                {
                    System.IO.File.Delete(ArcBuildTemplates);
                }
                catch (System.IO.IOException)
                {
                    deletestring = "Error, could not delete all files";
                }
            }
            if (System.IO.File.Exists(ArcChainloader))
            {
                try
                {
                    System.IO.File.Delete(ArcChainloader);
                }
                catch (System.IO.IOException)
                {
                    deletestring = "Error, could not delete all files";
                }
            }
            if (System.IO.File.Exists(ArcMechanicsLog))
            {
                try
                {
                    System.IO.File.Delete(ArcMechanicsLog);
                }
                catch (System.IO.IOException)
                {
                    deletestring = "Error, could not delete all files";
                }
            }
            StatusLabel.Text = deletestring;
        }
        private void Config()
        {
            StatusLabel.Text = "Checking config folder";
            if (System.IO.Directory.Exists(ConfigPath) == false)
            {
                System.IO.Directory.CreateDirectory(ConfigPath);
                StatusLabel.Text = "Created config folder";
            }
            StatusLabel.Text = "Checked config folder";
            StatusLabel.Text = "Checking config file";
            if (System.IO.File.Exists(ConfigFile) == false)
            {
                System.IO.File.Create(ConfigFile).Close();
                StatusLabel.Text = "Created config file";
            }
            StatusLabel.Text = "Reading config file";
            int counter = 0;
            string line;

            // Read the file and display it line by line.  
            System.IO.StreamReader file = new System.IO.StreamReader(ConfigFile);
            while ((line = file.ReadLine()) != null)
            {
                if (counter == 0)
                {
                    GW2Path = line;
                }
                else if (counter == 1)
                {
                    LogPath = line;
                }
                else if (counter == 2)
                {
                    if (line == "True")
                    {
                        checkBoxZip.Checked = true;
                    }
                    else
                    {
                        checkBoxZip.Checked = false;
                    }
                }
                else if (counter == 3)
                {
                    if (line == "True")
                    {
                        checkBoxEvtc.Checked = true;
                    }
                    else
                    {
                        checkBoxEvtc.Checked = false;
                    }
                }
                else if (counter == 4)
                {
                    if (line == "True")
                    {
                        checkBoxRememberPassword.Checked = true;
                    }
                    else
                    {
                        checkBoxRememberPassword.Checked = false;
                    }
                }
                else if (counter == 5)
                {
                    maskedTextBoxGW2RaidarPassword.Text = Base64Decode(line);
                }
                else if (counter == 6)
                {
                    textBoxGW2RaidarUsername.Text = line;
                }
                else if (counter == 7)
                {
                    if (line == "True")
                    {
                        radioButtonGW2RaidarLogs.Checked = true;
                        radioButtonLocalLogs.Checked = true;
                    }
                    else
                    {
                        radioButtonGW2RaidarLogs.Checked = false;
                        radioButtonLocalLogs.Checked = false;
                    }
                }
                else if (counter == 8)
                {
                    if (line == "True")
                    {
                        checkBoxRaid.Checked = true;
                    }
                    else
                    {
                        checkBoxRaid.Checked = false;
                    }
                }
                else if (counter == 9)
                {
                    if (line == "True")
                    {
                        checkBoxFractal.Checked = true;
                    }
                    else
                    {
                        checkBoxFractal.Checked = false;
                    }
                }
                else if (counter == 10)
                {
                    if (line == "True")
                    {
                        checkBoxOnlyWeek.Checked = true;
                    }
                    else
                    {
                        checkBoxOnlyWeek.Checked = false;
                    }
                }
                else if (counter == 11)
                {
                    if (line == "True")
                    {
                        checkBoxKitty.Checked = true;
                    }
                    else
                    {
                        checkBoxKitty.Checked = false;
                    }
                }
                else if (counter == 12)
                {
                    if (line == "True")
                    {
                        checkBoxMini.Checked = true;
                    }
                    else
                    {
                        checkBoxMini.Checked = false;
                    }
                }
                counter++;
            }

            file.Close();
            textBoxGW2Folder.Text = GW2Path;
            textBoxLogFolder.Text = LogPath;
            /*if (textBoxGW2Folder.Text == "")
            {
                //MessageBox.Show("No GW2 install folder has been found.\nPlease select one by clicking 'Set GW2 Folder'.", "GW2 Install folder not set", MessageBoxButtons.OK);
            }*/
            StatusLabel.Text = "Idle";
        }
        private void FillConfig()
        {
            Settings[0] = textBoxGW2Folder.Text;
            Settings[1] = textBoxLogFolder.Text;
            Settings[2] = checkBoxZip.Checked.ToString();
            Settings[3] = checkBoxEvtc.Checked.ToString();
            Settings[4] = checkBoxRememberPassword.Checked.ToString();
            if (Settings[4].ToLower() == "true")
            {
                Settings[5] = Base64Encode(maskedTextBoxGW2RaidarPassword.Text);
            }
            else
            {
                Settings[5] = "";
            }
            Settings[6] = textBoxGW2RaidarUsername.Text;
            Settings[7] = radioButtonLocalLogs.Checked.ToString();
            Settings[8] = checkBoxRaid.Checked.ToString();
            Settings[9] = checkBoxFractal.Checked.ToString();
            Settings[10] = checkBoxOnlyWeek.Checked.ToString();
            Settings[11] = checkBoxMini.Checked.ToString();
            Settings[12] = checkBoxKitty.Checked.ToString();
            System.IO.File.WriteAllLines(ConfigFile, Settings);
            StatusLabel.Text = "Settings saved succesfully";
        }
        public static string Base64Encode(string plainText)
        {
            try
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                return System.Convert.ToBase64String(plainTextBytes);
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static string Base64Decode(string base64EncodedData)
        {
            try
            {
                var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
                return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception)
            {
                return "";
            }

        }
        private void UpdatePaths()
        {
            GW2Path = textBoxGW2Folder.Text;
            LogPath = textBoxLogFolder.Text;
            ArcPath = GW2Path + "\\bin64";
            ArcBackup = GW2Path + "\\addons\\arcdps\\arcdps.backups";
            ArcMain = ArcPath + Main;
            ArcBuildTemplates = ArcPath + BuildTemplates;
            ArcChainloader = ArcPath + Chainloader;
            ArcMechanicsLog = ArcPath + MechanicsLog;
            ArcBackupMain = ArcBackup + Main;
            ArcBackupBuildTemplates = ArcBackup + BuildTemplates;
            ArcBackupChainloader = ArcBackup + Chainloader;
            ArcBackupMechanicsLog = ArcBackup + MechanicsLog;
        }
        private void ButtonListLogs_Click(object sender, EventArgs e)
        {
            FillConfig();
            UpdateList();
        }

        private void EmptyGrid()
        {
            dataGridViewLocalLogs.Rows.Clear();
            dataGridViewLocalLogs.Refresh();
        }

        private void GetFiles(string directory)
        {
            try
            {
                string[] fileEntries = Directory.GetFiles(directory);
                foreach (string fileName in fileEntries)
                {
                    bossNaam = Path.GetFileName(Path.GetDirectoryName(fileName));
                    bestandNaam = System.IO.Path.GetFileName(fileName);
                    datumdt = System.IO.File.GetCreationTime(fileName);
                    datum = System.IO.File.GetCreationTime(fileName).ToShortDateString();
                    bestandType = System.IO.Path.GetExtension(fileName);
                    pad = System.IO.Path.GetFullPath(fileName);

                    string[] rij0 = { bossNaam, datum, pad, bestandType };

                    if (checkBoxOnlyWeek.Checked == true)
                    {
                        if (datumdt > startdatedt && datumdt < enddatedt)
                        {
                            if (checkBoxZip.Checked == true && bestandType == ".zip" || checkBoxEvtc.Checked == true && bestandType == ".evtc")
                            {
                                ToUploadFileFull.Add(pad);
                                ToUploadFileName.Add(bestandNaam);
                                dataGridViewLocalLogs.Rows.Add(rij0);
                            }
                        }
                    }
                    else
                    {
                        if (checkBoxZip.Checked == true && bestandType == ".zip" || checkBoxEvtc.Checked == true && bestandType == ".evtc")
                        {
                            ToUploadFileFull.Add(pad);
                            ToUploadFileName.Add(bestandNaam);
                            dataGridViewLocalLogs.Rows.Add(rij0);
                        }
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                StatusLabel.Text = "Missing one or more directories";
            }

        }

        private void ButtonUpdatePathGW2_Click(object sender, EventArgs e)
        {
            folderBrowserDialogGW2Folder.SelectedPath = GW2Path;
            DialogResult result = folderBrowserDialogGW2Folder.ShowDialog();
            if (result == DialogResult.OK)
            {
                GW2Path = folderBrowserDialogGW2Folder.SelectedPath.ToString();
                textBoxGW2Folder.Text = GW2Path;
                //FillConfig();
                StatusLabel.Text = "Updated GW2 Install Directory";
            }
            else if (result == DialogResult.Cancel)
            {
                return;
            }
        }

        private void ButtonUpdatePathLog_Click_1(object sender, EventArgs e)
        {
            folderBrowserDialogLog.SelectedPath = LogPath;
            DialogResult result = folderBrowserDialogLog.ShowDialog();
            if (result == DialogResult.OK)
            {
                LogPath = folderBrowserDialogLog.SelectedPath.ToString();
                textBoxLogFolder.Text = LogPath;
                //FillConfig();
                StatusLabel.Text = "Updated ArcDps Log Directory";
            }
            else if (result == DialogResult.Cancel)
            {
                return;
            }
        }

        private void CheckBoxDisplayPassword_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDisplayPassword.Checked)
            {
                maskedTextBoxGW2RaidarPassword.UseSystemPasswordChar = false;
            }
            else
            {
                maskedTextBoxGW2RaidarPassword.UseSystemPasswordChar = true;
            }
        }

        private void ButtonSaveSettings_Click(object sender, EventArgs e)
        {
            FillConfig();
        }

        private void ButtonGW2RaidarLoginTest_Click(object sender, EventArgs e)
        {
            Login();
        }

        private void Login()
        {
            string username = textBoxGW2RaidarUsername.Text;
            string password = maskedTextBoxGW2RaidarPassword.Text;
            string link = "https://www.gw2raidar.com/api/v2/token";
            PostLogin(username, password, link);
        }

        private void PostLogin(string username, string password, string link)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                string postData = string.Format("username={0}&password={1}", username, password);
                byte[] bytes = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = bytes.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);

                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);

                var result = reader.ReadToEnd();
                stream.Dispose();
                reader.Dispose();
                string resultaat = result.ToString();
                if (resultaat.Contains("token"))
                {
                    token = resultaat.Substring(10, 40);
                    StatusLabel.Text = "Login test succesfull";
                    labelLogin.Text = "Login success";
                    pictureBoxLogin.BackgroundImage = Properties.Resources.LoginGreen;
                    //MessageBox.Show("Login was succesfull.", "Login succesfull", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    StatusLabel.Text = "Login test failed";
                    labelLogin.Text = "Login failed";
                    pictureBoxLogin.BackgroundImage = Properties.Resources.LoginRed;
                    //MessageBox.Show("Login failed, double check your username and password. If they are correct there might be an issue with the GW2Raidar API. Try again later.", "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception)
            {
                //MessageBox.Show("Login failed, double check your username and password. If they are correct there might be an issue with the GW2Raidar API. Try again later.", "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                labelLogin.Text = "Login failed";
                StatusLabel.Text = "Login test failed";
                pictureBoxLogin.BackgroundImage = Properties.Resources.LoginRed;
            }
        }

        private void ButtonInstallArc_Click(object sender, EventArgs e)
        {
            UpdateArc();
        }

        private void ButtonUpdateArc_Click_1(object sender, EventArgs e)
        {
            UpdateArc();
        }

        public List<string> FetchWeeks(int year)
        {
            List<string> weeks = new List<string>();
            DateTime startDate = new DateTime(year, 1, 1);
            DateTime currentday = DateTime.Now.Date;
            int curweek = GetIso8601WeekOfYear(currentday) - 1;
            startDate = startDate.AddDays(1 - (int)startDate.DayOfWeek);
            DateTime endDate = startDate.AddDays(6);
            int w = 0;
            while (startDate.Year < 1 + year && w <= curweek)
            {
                weeks.Add(string.Format("{0:dd/MM/yy} to {1:dd/MM/yy}", startDate, endDate));
                startDate = startDate.AddDays(7);
                endDate = endDate.AddDays(7);
                w += 1;
            }
            string startdate = startDate.ToString();
            string enddate = endDate.ToString();
            comboBoxWeek.DataSource = weeks;
            return weeks;
        }

        public static int GetIso8601WeekOfYear(DateTime time)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        private void SelectedWeek()
        {
            int currentyear = DateTime.Now.Year;
            DateTime currentday = DateTime.Now.Date;
            FetchWeeks(currentyear);
            if (initial == true)
            {
                comboBoxWeek.SelectedIndex = GetIso8601WeekOfYear(currentday) - 1;
                initial = false;
            }
            string tempstart = comboBoxWeek.SelectedValue.ToString().Substring(0, 8);
            startdatedt = DateTime.ParseExact(tempstart, "dd-MM-yy", System.Globalization.CultureInfo.InvariantCulture);
            string tempend = comboBoxWeek.SelectedValue.ToString().Substring(12, 8);
            enddatedt = DateTime.ParseExact(tempend, "dd-MM-yy", System.Globalization.CultureInfo.InvariantCulture);
        }

        private void UpdateList()
        {
            EmptyGrid();
            string tempstart = comboBoxWeek.SelectedValue.ToString().Substring(0, 8);
            startdatedt = DateTime.ParseExact(tempstart, "dd-MM-yy", System.Globalization.CultureInfo.InvariantCulture);
            string tempend = comboBoxWeek.SelectedValue.ToString().Substring(12, 8);
            enddatedt = DateTime.ParseExact(tempend, "dd-MM-yy", System.Globalization.CultureInfo.InvariantCulture);
            ToUploadFileFull.Clear();
            ToUploadFileName.Clear();
            if (checkBoxRaid.Checked)
            {
                GetFiles(LogPath + "\\Vale Guardian");
                GetFiles(LogPath + "\\Gorseval the Multifarious");
                GetFiles(LogPath + "\\Sabetha the Saboteur");
                GetFiles(LogPath + "\\Slothasor");
                GetFiles(LogPath + "\\Matthias Gabrel");
                GetFiles(LogPath + "\\Keep Construct");
                GetFiles(LogPath + "\\Xera");
                GetFiles(LogPath + "\\Cairn the Indomitable");
                GetFiles(LogPath + "\\Mursaat Overseer");
                GetFiles(LogPath + "\\Samarog");
                GetFiles(LogPath + "\\Deimos");
                GetFiles(LogPath + "\\Soulless Horror");
                GetFiles(LogPath + "\\Dhuum");
                StatusLabel.Text = "Showing Raid logs";
            }
            if (checkBoxFractal.Checked)
            {
                GetFiles(LogPath + "\\MAMA");
                GetFiles(LogPath + "\\Nightmare Oratuss");
                GetFiles(LogPath + "\\Ensolyss of the Endless Torment");
                GetFiles(LogPath + "\\Skorvald the Shattered");
                GetFiles(LogPath + "\\Artsariiv");
                GetFiles(LogPath + "\\Arkk");
                StatusLabel.Text = "Showing Fractal logs";
            }
            if (checkBoxMini.Checked)
            {
                GetFiles(LogPath + "\\Berg");
                GetFiles(LogPath + "\\Zane");
                GetFiles(LogPath + "\\Narella");
                StatusLabel.Text = "Showing Mini Raid logs";
            }
            if (checkBoxKitty.Checked)
            {
                GetFiles(LogPath + "\\Average Kitty Golem");
                GetFiles(LogPath + "\\Standard Kitty Golem");
                GetFiles(LogPath + "\\Tough Kitty Golem");
                GetFiles(LogPath + "\\Vital Kitty Golem");
                GetFiles(LogPath + "\\Massive Vital Kitty Golem");
                GetFiles(LogPath + "\\Massive Standard Golem");
                GetFiles(LogPath + "\\Massive Average Kitty Golem");
                StatusLabel.Text = "Showing Kitty golem logs";
            }
            if (checkBoxFractal.Checked && checkBoxRaid.Checked)
            {
                StatusLabel.Text = "Showing Fractal and Raid logs";
            }
            if (checkBoxRaid.Checked && checkBoxOnlyWeek.Checked || checkBoxFractal.Checked && checkBoxOnlyWeek.Checked)
            {
                StatusLabel.Text += " (Selected week)";
            }
            else if (checkBoxRaid.Checked || checkBoxFractal.Checked)
            {
                StatusLabel.Text += " (All time)";
            }
        }

        private void ButtonUploadLog_Click(object sender, EventArgs e)
        {
            Login();
            ToUpload();
        }

        private void ToUpload()
        {
            string contentType = "";
            string url = "https://www.gw2raidar.com/api/v2/encounters/new";
            int i = 0;
            int p = 0;
            foreach(string FIleFull in ToUploadFileFull)
            {
                p += 1;
            }
            progressBarLogs.Value = 0;
            progressBarLogs.Maximum = p;


            foreach(string FileFull in ToUploadFileFull)
            {
                byte[] bytes = File.ReadAllBytes(ToUploadFileFull[i]);
                string filename = ToUploadFileName[i];
                StatusLabel.Text = "Uploading: " + filename;

                UploadMultipart(bytes, filename, contentType, url);
                i += 1;
                progressBarLogs.PerformStep();
            }
            StatusLabel.Text = "Finished Uploading " + i + " logs";
        }


        public void UploadMultipart(byte[] file, string filename, string contentType, string url)
        {
            var webClient = new WebClient();
            string boundary = "------------------------" + DateTime.Now.Ticks.ToString("x");
            webClient.Headers.Add("Authorization", "token " + token);
            webClient.Headers.Add("Content-Type", "multipart/form-data; boundary=" + boundary);
            var fileData = webClient.Encoding.GetString(file);
            var package = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n{3}\r\n--{0}--\r\n", boundary, filename, contentType, fileData);

            var nfile = webClient.Encoding.GetBytes(package);

            byte[] resp = webClient.UploadData(url, "PUT", nfile);
        }
        
        private void ComboBoxWeek_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeDate();
        }

        public void ChangeDate()
        {
            startdate = comboBoxWeek.SelectedValue.ToString().Substring(0, 8);
            enddate = comboBoxWeek.SelectedValue.ToString().Substring(12, 8);
        }

        public void GotoSite(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        #region Links
        private void LinkLabelArcDps_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GotoSite("https://www.deltaconnected.com/arcdps/");
        }

        private void LinkLabelGW2Hook_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GotoSite("https://04348.github.io/Gw2Hook/");
        }

        private void LinkLabelMechanicsLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GotoSite("http://martionlabs.com/arcdps-mechanics-log-plugin/");
        }

        private void LinkLabelGW2Raidar_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GotoSite("https://www.gw2raidar.com/");
        }

        private void LinkLabelGW2TacO_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GotoSite("http://www.gw2taco.com/");
        }

        private void LinkLabelReShade_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GotoSite("https://reshade.me/");
        }

        private void LinkLabelGW2Navi_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GotoSite("http://forum.renaka.com/topic/5546166/1/");
        }

        private void LinkLabelGW2PAO_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GotoSite("https://samhurne.github.io/gw2pao/");
        }

        private void LinkLabelGW2Timer_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GotoSite("https://gw2timer.com/");
        }

        private void LinkLabelDpsReport_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GotoSite("https://dps.report/");
        }

        private void RadioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }
        #endregion

        public string Get(string URI, string token)
        {
            WebClient client = new WebClient();

            client.Headers.Add("Authorization", "token " + token);

            Stream data = client.OpenRead(URI);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();

            return s;
        }

        private void Areas()
        {
            if(token == "")
            {
                Login();
            }
            //string AreasLink = "https://www.gw2raidar.com/api/v2/areas";
            //textBoxGetTest.Text = Get(AreasLink, token);
        }

        private void Categories()
        {
            if (token == "")
            {
                Login();
            }
            //string CategoriesLink = "https://www.gw2raidar.com/api/v2/categories";
            //textBoxGetTest.Text = Get(CategoriesLink, token);
            CategoriesArray = new string[] { "Guild / Static", "Training", "PUG", "Low Man / Sells" };
            comboBoxTag.DataSource = CategoriesArray;
        }

        private void Encounters()
        {
            if (token == "")
            {
                Login();
            }
            //string EncountersLink = "https://www.gw2raidar.com/api/v2/encounters";
            //textBoxGetTest.Text = Get(EncountersLink, token);
        }

    }
}
