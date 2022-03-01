using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Renci.SshNet;
using System.Windows.Threading;
using Ookii.Dialogs;
using System.IO;
using GoogleMapsApi;
using Geocoding.Google;
using Geocoding;
using BingMapsRESTToolkit;
using System.Collections.Specialized;
using System.Net;


namespace Grotto_Information_2022_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int i, intChildCount, intAdultCount;
        DispatcherTimer displayTimer = new DispatcherTimer();
        DispatcherTimer familyTimer = new DispatcherTimer();
        DispatcherTimer overrideTimer = new DispatcherTimer();
        DispatcherTimer nameTimer = new DispatcherTimer();
        DispatcherTimer mapTimer = new DispatcherTimer();
        DispatcherTimer photoTimer = new DispatcherTimer();
        List<string> ftp_list = new List<string>();
        string strStatus, strread;
        int intStatus, intTab, intRef;
        string intUnique, strFamily, strAddress, strPostcode;
        List<string> lstImages = new List<string>();
        Window1 win2 = new Window1();
        BitmapImage image = new BitmapImage();
        List<string> lstQuestions = new List<string>();
        int intZoom;
        FileInfo file;

        public MainWindow()
        {
            InitializeComponent();
            loadSettings();
            clearFamily();
            DownloadandLoadQuestions();
            hideInfoGB();
            loadSecondScreen();
        }

        private void loadSecondScreen()
        {
            win2.Show();
            hideElements();
            clearElements();
        }

        private void clearElements()
        {
            win2.lbAName.Items.Clear();
            win2.lbARelationship.Items.Clear();
            win2.lbCGender.Items.Clear();
            win2.lbCName.Items.Clear();
            win2.lbCPresent.Items.Clear();
        }

        private void hideElements()
        {
            win2.lblAName.Visibility = Visibility.Hidden;
            win2.lblARelationship.Visibility = Visibility.Hidden;
            win2.lblCGender.Visibility = Visibility.Hidden;
            win2.lblCName.Visibility = Visibility.Hidden;
            win2.lblCPresent.Visibility = Visibility.Hidden;

            win2.lbAName.Visibility=Visibility.Hidden;
            win2.lbARelationship.Visibility=Visibility.Hidden;
            win2.lbCName.Visibility=Visibility.Hidden;
            win2.lbCPresent.Visibility=Visibility.Hidden;
            win2.lbCGender.Visibility=Visibility.Hidden;
        }

        private void hideInfoGB()
        {
            gbAdult.Visibility = Visibility.Hidden;
            cmdShowAdult.Content = "Show Adult Info";
            gbChild.Visibility = Visibility.Hidden;
            cmdShowChild.Content = "Show Child Info";
            gbFamily.Visibility = Visibility.Hidden;
            cmdShowFamily.Content = "Show Family Info";
            gbHidden.Visibility = Visibility.Hidden;
            cmdHidden.Content = "Show Hidden Info";

            cmdHidden.IsEnabled = true;
            cmdMap.IsEnabled = true;
        }

        private void DownloadandLoadQuestions()
        {
            var list = new List<string>();

            //attempt to download questions
            string Host = txtFTPHost.Text;
            int Port = Int32.Parse(txtFTPPort.Text);
            string Username = txtFTPUsername.Text;
            string Password = txtFTPPassword.Text;

            bool bSuccess = false;

            try
            {
                using (var sftp = new SftpClient(Host, Port, Username, Password))
                {
                    sftp.Connect(); //connect to server

                    //download questions
                    string strRemoteFolder = Properties.Settings.Default.giFTPFolder.ToString() + "//Questions.txt";
                    string strLocalFolder = Properties.Settings.Default.giSaveLocal.ToString() + "//Questions.txt";

                    using (var file = File.OpenWrite(strLocalFolder))
                    {
                        sftp.DownloadFile(strRemoteFolder, file);//download file
                    }

                    sftp.Disconnect();
                    bSuccess = true;
                    distributeQuestions();
                }
            }
            catch
            {
                MessageBox.Show("Failed to Download from server");
                bSuccess = false;
                //attempt to download from local server
                try
                {
                    string fileToCopy = Properties.Settings.Default.giSaveServer.ToString() + "//Questions.txt";
                    string strLocalFolder = Properties.Settings.Default.giSaveLocal.ToString() + "//Questions.txt";

                    File.Copy(fileToCopy, strLocalFolder, true);
                    bSuccess = true;
                    distributeQuestions();
                }
                catch
                {
                    MessageBox.Show("Failed to Download from local server");
                    bSuccess = false;
                    //check to see if file already exists and use this
                    string strLocalFolder = Properties.Settings.Default.giSaveLocal.ToString() + "//Questions.txt";
                    if (File.Exists(strLocalFolder))
                    {
                        bSuccess = true;
                        distributeQuestions();
                    }
                    else
                    {
                        string strMessage = "Questions.txt cannot be downloaded, found on local server or exists on this machine. Please place Questions.txt at " + strLocalFolder + " and reload program to add questions";
                        MessageBox.Show(strMessage, "Missing Questions.txt file", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void distributeQuestions()
        {
            var list = new List<string>();
            //if successful open questions and split to correct boxes
            strread = Properties.Settings.Default.giSaveLocal.ToString();
            strread = strread + "\\Questions.txt";
            // MessageBox.Show(strread.ToString());
            var fileStream = new FileStream(strread, FileMode.Open, System.IO.FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    list.Add(line);
                }
            }
            fileStream.Close();
            i = 0;
            lstChildHideQuestions.Items.Clear();
            lstChildShowQuestions.Items.Clear();
            lstAdultHideQuestions.Items.Clear();
            lstAdultShowQuestions.Items.Clear();
            lstFamilyHideQuestions.Items.Clear();
            lstFamilyShowQuestions.Items.Clear();
            while (i < list.Count)
            {
                List<string> questionsSplit = list[i].ToString().Split(',').ToList<string>();
                if (string.Join("", questionsSplit[4].Split('"')) == "Child")
                {
                    if (string.Join("", questionsSplit[5].Split('"')) == "Show")
                    {
                        lstChildShowQuestions.Items.Add(questionsSplit[1]);
                    }
                    else
                    {
                        lstChildHideQuestions.Items.Add(questionsSplit[1]);
                    }
                }
                if (string.Join("", questionsSplit[4].Split('"')) == "Adult")
                {
                    if (string.Join("", questionsSplit[5].Split('"')) == "Show")
                    {
                        lstAdultShowQuestions.Items.Add(questionsSplit[1]);
                    }
                    else
                    {
                        lstAdultHideQuestions.Items.Add(questionsSplit[1]);
                    }
                }
                if (string.Join("", questionsSplit[4].Split('"')) == "Family")
                {
                    if (string.Join("", questionsSplit[5].Split('"')) == "Show")
                    {
                        lstFamilyShowQuestions.Items.Add(questionsSplit[1]);
                    }
                    else
                    {
                        lstFamilyHideQuestions.Items.Add(questionsSplit[1]);
                    }
                }
                lstQuestions.Add(list[i].ToString());
                i = i + 1;
            }


            //add children questions to screen
            i = 0;
            int xloc = 0;
            int yloc = 0;
            //MessageBox.Show(lstChildShowQuestions.Items.Count.ToString());
            while (i < lstChildShowQuestions.Items.Count)
            {
                TextBlock childTB = new TextBlock();
                childTB.Height = 50;
                childTB.Width = 150;
                childTB.FontSize = 12;
                childTB.HorizontalAlignment = HorizontalAlignment.Left;
                childTB.VerticalAlignment = VerticalAlignment.Top;
                childTB.Text = lstChildShowQuestions.Items[i].ToString();
                childTB.Margin = new Thickness(xloc, yloc, 0, 0);
                childTB.TextWrapping = TextWrapping.WrapWithOverflow;
                childTB.Name = "childTB" + i.ToString();
                gridChild.Children.Add(childTB);

                ListBox childLB = new ListBox();
                childLB.Height = 100;
                childLB.Width = 150;
                childLB.FontSize = 12;
                childLB.HorizontalAlignment = HorizontalAlignment.Left;
                childLB.VerticalAlignment = VerticalAlignment.Top;
                childLB.Name = "childLB" + i.ToString();
                childLB.Margin = new Thickness(xloc, yloc + 50, 0, 0);
                gridChild.Children.Add(childLB);

                xloc = xloc + 175;
                if (xloc > 800)
                {
                    yloc = yloc + 150;
                    xloc = 0;
                }

                i = i + 1;
            }

            //add adults questions to adult group box
            i = 0;
            xloc = 0;
            yloc = 0;
            //MessageBox.Show(lstAdultShowQuestions.Items.Count.ToString());
            while (i < lstAdultShowQuestions.Items.Count)
            {
                TextBlock adultTB = new TextBlock();
                adultTB.Height = 50;
                adultTB.Width = 150;
                adultTB.FontSize = 12;
                adultTB.HorizontalAlignment = HorizontalAlignment.Left;
                adultTB.VerticalAlignment = VerticalAlignment.Top;
                adultTB.Text = lstAdultShowQuestions.Items[i].ToString();
                adultTB.Margin = new Thickness(xloc, yloc, 0, 0);
                adultTB.TextWrapping = TextWrapping.WrapWithOverflow;
                adultTB.Name = "adultTB" + i.ToString();
                gridAdult.Children.Add(adultTB);

                ListBox adultLB = new ListBox();
                adultLB.Height = 100;
                adultLB.Width = 150;
                adultLB.FontSize = 12;
                adultLB.HorizontalAlignment = HorizontalAlignment.Left;
                adultLB.VerticalAlignment = VerticalAlignment.Top;
                adultLB.Name = "adultLB" + i.ToString();
                adultLB.Margin = new Thickness(xloc, yloc + 50, 0, 0);
                gridAdult.Children.Add(adultLB);

                xloc = xloc + 175;
                if (xloc > 800)
                {
                    yloc = yloc + 150;
                    xloc = 0;
                }

                i = i + 1;
            }

            //add family questions to adult group box
            i = 0;
            xloc = 0;
            yloc = 0;
            //MessageBox.Show(lstFamilyShowQuestions.Items.Count.ToString());
            while (i < lstFamilyShowQuestions.Items.Count)
            {
                TextBlock familyTB = new TextBlock();
                familyTB.Height = 50;
                familyTB.Width = 150;
                familyTB.FontSize = 12;
                familyTB.HorizontalAlignment = HorizontalAlignment.Left;
                familyTB.VerticalAlignment = VerticalAlignment.Top;
                familyTB.Text = lstFamilyShowQuestions.Items[i].ToString();
                familyTB.Margin = new Thickness(xloc, yloc, 0, 0);
                familyTB.TextWrapping = TextWrapping.WrapWithOverflow;
                familyTB.Name = "familyTB" + i.ToString();
                gridFamily.Children.Add(familyTB);

                ListBox familyLB = new ListBox();
                familyLB.Height = 100;
                familyLB.Width = 150;
                familyLB.FontSize = 12;
                familyLB.HorizontalAlignment = HorizontalAlignment.Left;
                familyLB.VerticalAlignment = VerticalAlignment.Top;
                familyLB.Name = "familyLB" + i.ToString();
                familyLB.Margin = new Thickness(xloc, yloc + 50, 0, 0);
                gridFamily.Children.Add(familyLB);

                xloc = xloc + 175;
                if (xloc > 800)
                {
                    yloc = yloc + 150;
                    xloc = 0;
                }

                i = i + 1;
            }

            //add all hidden information to hidden groupbox
            i = 0;
            xloc = 0;
            yloc = 0;

            //hidden child info
            while (i < lstChildHideQuestions.Items.Count)
            {
                TextBlock hidechildTB = new TextBlock();
                hidechildTB.Height = 50;
                hidechildTB.Width = 150;
                hidechildTB.FontSize = 12;
                hidechildTB.HorizontalAlignment = HorizontalAlignment.Left;
                hidechildTB.VerticalAlignment = VerticalAlignment.Top;
                hidechildTB.Text = lstChildHideQuestions.Items[i].ToString();
                hidechildTB.Margin = new Thickness(xloc, yloc, 0, 0);
                hidechildTB.TextWrapping = TextWrapping.WrapWithOverflow;
                hidechildTB.Name = "hidechildTB" + i.ToString();
                gridHidden.Children.Add(hidechildTB);

                ListBox hiddenchildLB = new ListBox();
                hiddenchildLB.Height = 100;
                hiddenchildLB.Width = 150;
                hiddenchildLB.FontSize = 12;
                hiddenchildLB.HorizontalAlignment = HorizontalAlignment.Left;
                hiddenchildLB.VerticalAlignment = VerticalAlignment.Top;
                hiddenchildLB.Name = "hiddenchildLB" + i.ToString();
                hiddenchildLB.Margin = new Thickness(xloc, yloc + 50, 0, 0);
                gridHidden.Children.Add(hiddenchildLB);

                xloc = xloc + 175;
                if (xloc > 800)
                {
                    yloc = yloc + 150;
                    xloc = 0;
                }
                i = i + 1;
            }

            yloc = yloc + 150;
            xloc = 0;
            i = 0;

            //hidden adult info
            while (i < lstAdultHideQuestions.Items.Count)
            {
                TextBlock hideadultTB = new TextBlock();
                hideadultTB.Height = 50;
                hideadultTB.Width = 150;
                hideadultTB.FontSize = 12;
                hideadultTB.HorizontalAlignment = HorizontalAlignment.Left;
                hideadultTB.VerticalAlignment = VerticalAlignment.Top;
                hideadultTB.Text = lstAdultHideQuestions.Items[i].ToString();
                hideadultTB.Margin = new Thickness(xloc, yloc, 0, 0);
                hideadultTB.TextWrapping = TextWrapping.WrapWithOverflow;
                hideadultTB.Name = "hideadultTB" + i.ToString();
                gridHidden.Children.Add(hideadultTB);

                ListBox hiddenadultLB = new ListBox();
                hiddenadultLB.Height = 100;
                hiddenadultLB.Width = 150;
                hiddenadultLB.FontSize = 12;
                hiddenadultLB.HorizontalAlignment = HorizontalAlignment.Left;
                hiddenadultLB.VerticalAlignment = VerticalAlignment.Top;
                hiddenadultLB.Name = "hiddenadultLB" + i.ToString();
                hiddenadultLB.Margin = new Thickness(xloc, yloc + 50, 0, 0);
                gridHidden.Children.Add(hiddenadultLB);

                xloc = xloc + 175;
                if (xloc > 800)
                {
                    yloc = yloc + 150;
                    xloc = 0;
                }
                i = i + 1;
            }


            yloc = yloc + 150;
            xloc = 0;
            i = 0;

            //hidden family info
            while (i < lstFamilyHideQuestions.Items.Count)
            {
                TextBlock hidefamilyTB = new TextBlock();
                hidefamilyTB.Height = 50;
                hidefamilyTB.Width = 150;
                hidefamilyTB.FontSize = 12;
                hidefamilyTB.HorizontalAlignment = HorizontalAlignment.Left;
                hidefamilyTB.VerticalAlignment = VerticalAlignment.Top;
                hidefamilyTB.Text = lstFamilyShowQuestions.Items[i].ToString();
                hidefamilyTB.Margin = new Thickness(xloc, yloc, 0, 0);
                hidefamilyTB.TextWrapping = TextWrapping.WrapWithOverflow;
                hidefamilyTB.Name = "hidefamilyTB" + i.ToString();
                gridHidden.Children.Add(hidefamilyTB);

                ListBox hiddenfamilyLB = new ListBox();
                hiddenfamilyLB.Height = 100;
                hiddenfamilyLB.Width = 150;
                hiddenfamilyLB.FontSize = 12;
                hiddenfamilyLB.HorizontalAlignment = HorizontalAlignment.Left;
                hiddenfamilyLB.VerticalAlignment = VerticalAlignment.Top;
                hiddenfamilyLB.Name = "hiddenfamilyLB" + i.ToString();
                hiddenfamilyLB.Margin = new Thickness(xloc, yloc + 50, 0, 0);
                gridHidden.Children.Add(hiddenfamilyLB);

                xloc = xloc + 175;
                if (xloc > 800)
                {
                    yloc = yloc + 150;
                    xloc = 0;
                }
                i = i + 1;
            }
            //MessageBox.Show(lstFamilyShowQuestions.Items.Count.ToString());
        }

        private void loadSettings()
        {
            loadFTP();
            loadGrottoInfo();
            loadIntervals();
            loadTimers();
            loadLocations();
            loadBackups();
            txtDisplaySantaPic.Text = Properties.Settings.Default.giDisplayInterval.ToString();
            txtGoogleAPI.Text = Properties.Settings.Default.giGoogleAPI.ToString();
            txtLocalSave.Text = Properties.Settings.Default.giSaveLocal.ToString();
            txtServerSave.Text = Properties.Settings.Default.giSaveServer.ToString();
            txtMainDisplay.Text = Properties.Settings.Default.giMainMonitor.ToString();
            txtSecondDisplay.Text = Properties.Settings.Default.giSecondMonitor.ToString();
            txtMonitor.Text = Properties.Settings.Default.giMonitor.ToString();
            txtSavedXmas.Text = Properties.Settings.Default.giSavedXmas.ToString();
            txtPicsGreenscreen.Text = Properties.Settings.Default.giPicsGreenscreen.ToString();
            txtPhotoTimer.Text = Properties.Settings.Default.giPhotoInterval.ToString();
            cmdBreak.IsEnabled = false;
            intUnique = Properties.Settings.Default.giUnique.ToString();
            if (Properties.Settings.Default.giGSsetting == "FTP")
            {
                rbGSFTP.IsChecked = true;
            }
            else
            {
                rbGSLocal.IsChecked = true;
            }
            win2.myMap.Visibility = Visibility.Hidden;
            loadDates();
        }

        private void loadDates()
        {
            //add dates to combo boxes
            i = 1;
            while (i < 32)
            {
                cboDay.Items.Add(i.ToString());
                i = i + 1;
            }
            i = 1;
            while (i < 13)
            {
                cboMonth.Items.Add(i.ToString());
                i = i + 1;
            }
            i = 2021;
            while (i < 2025)
            {
                cboYear.Items.Add(i.ToString());
                i = i + 1;
            }
            cboDay.Text = DateTime.Now.ToString("dd");
            cboMonth.Text = DateTime.Now.ToString("MM");
            cboYear.Text = DateTime.Now.ToString("yyyy");
        }

        private void loadBackups()
        {
            List<string> lstBackups = new List<string>();
            lstBackups = Properties.Settings.Default.giBackupLocations.Split(",").ToList();
            txtNearby1.Text = lstBackups[0].ToString();
            txtNearby2.Text = lstBackups[1].ToString();
            txtNearby3.Text = lstBackups[2].ToString();
            txtNearby4.Text = lstBackups[3].ToString();
            txtNearby5.Text = lstBackups[4].ToString();
            txtNearby6.Text = lstBackups[5].ToString();
            txtNearby7.Text = lstBackups[6].ToString();
            txtNearby8.Text = lstBackups[7].ToString();
            txtNearby9.Text = lstBackups[8].ToString();
            txtNearby10.Text = lstBackups[9].ToString();
        }

        private void loadLocations()
        {
            txtBackup.Text = Properties.Settings.Default.giBackupLocation.ToString();
        }

       void loadTimers()
        {
            familyTimer.Stop();
            familyTimer.Interval = TimeSpan.FromSeconds(Properties.Settings.Default.giTimerFamily);
            familyTimer.Tick += familyTick;

            overrideTimer.Stop();
            overrideTimer.Interval = TimeSpan.FromSeconds(Properties.Settings.Default.giTimerOverride);
            overrideTimer.Tick += overrideTick;
            overrideTimer.Start();

            nameTimer.Stop();
            nameTimer.Interval = TimeSpan.FromSeconds(Properties.Settings.Default.giNameTimer);
            nameTimer.Tick += nameTick;

            mapTimer.Stop();
            mapTimer.Interval = TimeSpan.FromMilliseconds(Properties.Settings.Default.giMapSpeed);
            mapTimer.Tick += mapZoom;

            photoTimer.Stop();
            photoTimer.Interval = TimeSpan.FromSeconds(Properties.Settings.Default.giPhotoInterval);
            photoTimer.Tick += photoTick;
            photoTimer.Start();

            displayTimer.Stop();
            displayTimer.Interval = TimeSpan.FromSeconds(Properties.Settings.Default.giDisplayInterval);
            displayTimer.Tick += displayTick;
        }
      
        void displayTick(object sender, EventArgs e)
        {
            displayTimer.Stop();
            win2.imageBox.Source = null;
            File.Delete(file.ToString());
            photoTimer.Start();
        }

        void photoTick(object sender, EventArgs e)
        {
            DirectoryInfo dir = new DirectoryInfo(@Properties.Settings.Default.giPicsSanta.ToString());
            int count = dir.GetFiles().Length;
            string strExe;
            if (count > 0)
            {
                file = dir.GetFiles()[0];
                //move file
                string path = file.ToString();
                string dt = DateTime.Now.ToString("hhmmss");
                string path2 = Properties.Settings.Default.giPicsSanta.ToString() + "\\Backup\\" + dt.ToString() + "" + file.Name;
                File.Copy(path, path2);

                //get extension of file
                strExe = System.IO.Path.GetExtension(path2.ToString());
                
                if (strExe == ".txt")//display if it is a picture
                { }
                else
                {
                    win2.myMap.Visibility = Visibility.Hidden;
                    hideElements();

                    image = new BitmapImage(new Uri(path2.ToString(), UriKind.Absolute));
                    win2.imageBox.Source = image;
                    photoTimer.Stop();
                    displayTimer.Start();
                }
            }
        }

        void mapZoom(object sender, EventArgs e)
        {
            intZoom = intZoom + 1;
            if (intZoom >18)
            {
                mapTimer.Stop();
            }
            else
            {
                intZoom = intZoom + 1;
                win2.myMap.ZoomLevel = intZoom; 
            }
        }

        void nameTick(object sender, EventArgs e)
        {
            hideElements();
            nameTimer.Stop();
        }

        async void familyTick(object sender, EventArgs e)
        {
            //check for a new family
            var list = new List<string>();
            strread = Properties.Settings.Default.giMonitor.ToString();
            strread = strread + "\\Info"+Properties.Settings.Default.giGrottoNumber.ToString()+".txt";
            //track success
            bool bSuccess = false;

            try
            {
                var fileStream = new FileStream(strread, FileMode.Open, System.IO.FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        list.Add(line);
                    }
                }
                fileStream.Close();
                bSuccess = true;
          }
            catch 
            {
                bSuccess = false;
                MessageBox.Show("Can't read the monitor file");
            }

            if (bSuccess==true)
            {
                //if the unique number is different to saved unique number then load new family information
                if (list[0].ToString() == intUnique.ToString())
                { }
                else
                {
                    string b, c;

                    //stop timer
                    familyTimer.Stop();

                    //update unique number
                    Properties.Settings.Default.giUnique = list[0].ToString();
                    Properties.Settings.Default.Save();
                    intUnique = list[0].ToString();

                    //Store new variables
                    strFamily = list[2].ToString();
                    intRef = Int32.Parse(list[1].ToString());
                    strAddress = list[3].ToString();
                    strPostcode = list[4].ToString();

                    intChildCount = Int32.Parse(list[5]);
                    intAdultCount = Int32.Parse(list[6]);

                    lblAddress.Content=strAddress.ToString();
                    lblPostcode.Content=strPostcode.ToString();


                    //get long and latitude
                    try
                    {
                        var request = new GeocodeRequest();
                        request.BingMapsKey = "AmOe20x8v9Yuqsaot-wUNASTJaP4UW_zCKG9aVp4KCREWELFUQ-g13hwGBMZ9rkU";

                        request.Query = lblAddress.Content + ", " + lblPostcode.Content;

                        var result = await request.Execute();
                        if (result.StatusCode == 200)
                        {
                            var toolkitLocation = (result?.ResourceSets?.FirstOrDefault())
                                    ?.Resources?.FirstOrDefault()
                                    as BingMapsRESTToolkit.Location;
                            var latitude = toolkitLocation.Point.Coordinates[0];
                            var longitude = toolkitLocation.Point.Coordinates[1];
                            var mapLocation = new Microsoft.Maps.MapControl.WPF.Location(latitude, longitude);
                            win2.myMap.SetView(mapLocation, 1);
                            intZoom = 1;
                            cmdMap.IsEnabled = true;
                            win2.myMap.Visibility = Visibility.Hidden;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Can't get Longitude and Latitude for this families address, the map will not work", "No map for this family", MessageBoxButton.OK, MessageBoxImage.Stop);
                        cmdMap.IsEnabled = false;
                    }

                    //Display new variables
                    lblFamilyName.Content = strFamily;
                    lblRef.Content = intRef.ToString();
                    lblRef.Visibility = Visibility.Visible;
                    
                    //enable buttons
                    cmdFamilyLeft.IsEnabled = true;
                    cmdMap.IsEnabled = true;
                    gbPics.IsEnabled = true;
                    cmdShowAdult.IsEnabled = true;
                    cmdShowChild.IsEnabled = true;
                    cmdShowFamily.IsEnabled = true;
                    cmdHidden.IsEnabled = true;

                    //attempt to download information from server
                    string Host = txtFTPHost.Text;
                    int Port = Int32.Parse(txtFTPPort.Text);
                    string Username = txtFTPUsername.Text;
                    string Password = txtFTPPassword.Text;

                    //set status's
                    bool greenStatus = false;
                    bool childStatus = false;
                    bool adultStatus = false;
                    bool familyStatus = false;
                    bool savedStatus = false;

                    //try to download family information from server
                    try
                    {
                        ftp_list.Clear();
                        using (var sftp = new SftpClient(Host, Port, Username, Password))
                        {
                            sftp.Connect(); //connect to server

                            //server variables
                            string strLocalFolder = "";
                            string strFileName;
                            string strRemoteFolder = "";

                            //count files in different folders
                            string strRemoteFolderC = Properties.Settings.Default.giFTPChild.ToString();
                            List<string> child_list = new List<string>();

                            string strRemoteFolderA = Properties.Settings.Default.giFTPAdult.ToString();
                            List<string> adult_list = new List<string>();

                            string strRemoteFolderF = Properties.Settings.Default.giFTPFamily.ToString();
                            List<string> family_list = new List<string>();

                            string strSavedFolder = Properties.Settings.Default.giFTPSaved.ToString();
                            List<string> saved_list = new List<string>();

                            string strGreenFolder = Properties.Settings.Default.giPicsGreenscreen.ToString();
                            List<string> green_list = new List<string>();

                            child_list = sftp.ListDirectory(strRemoteFolderC).Where(f => !f.IsDirectory).Select(f => f.Name).ToList();
                            adult_list = sftp.ListDirectory(strRemoteFolderA).Where(f => !f.IsDirectory).Select(f => f.Name).ToList();
                            family_list = sftp.ListDirectory(strRemoteFolderF).Where(f => !f.IsDirectory).Select(f => f.Name).ToList();
                            saved_list = sftp.ListDirectory(strSavedFolder).Where(f => !f.IsDirectory).Select(f => f.Name).ToList();
                            green_list = sftp.ListDirectory(strGreenFolder).Where(f => !f.IsDirectory).Select(f => f.Name).ToList();

                            //download child info and pictures from FTP
                            try
                            {
                                i = 0;
                                while (i < child_list.Count) //cycle through list of files
                                {
                                    if (child_list[i].ToString().Contains(intRef.ToString()))
                                    {
                                        c = Properties.Settings.Default.giFTPChild + "/" + child_list[i]; //update download file from sftp
                                        b = Properties.Settings.Default.giSaveLocal + "\\" + child_list[i];//update download folder to pc 
                                        using (var file = File.OpenWrite(b))
                                        {
                                            sftp.DownloadFile(c, file);//download file
                                        }
                                    }
                                    i = i + 1;
                                }
                                childStatus = true;
                            }
                            catch
                            {
                                MessageBox.Show("Failed to download child information from server, checking backups", "FTP contained no information");
                                childStatus = false;
                            }

                            //download Adult info and pictures from FTP
                            try
                            {
                                i = 0;
                                while (i < adult_list.Count) //cycle through list of files
                                {
                                    if (child_list[i].ToString().Contains(intRef.ToString()))
                                    {
                                        c = Properties.Settings.Default.giFTPAdult + "/" + adult_list[i]; //update download file from sftp
                                        b = Properties.Settings.Default.giSaveLocal + "\\" + adult_list[i];//update download folder to pc 
                                        using (var file = File.OpenWrite(b))
                                        {
                                            sftp.DownloadFile(c, file);//download file
                                        }
                                    }
                                    i = i + 1;
                                }
                                adultStatus = true;
                            }
                            catch
                            {
                                MessageBox.Show("Failed to download adult information from server, checking backups", "FTP contained no information");
                                adultStatus=false;
                            }

                            //download family info and pictures from FTP
                            try
                            {
                                i = 0;
                                while (i < family_list.Count) //cycle through list of files
                                {
                                    if (family_list[i].ToString().Contains(intRef.ToString()))
                                    {
                                        c = Properties.Settings.Default.giFTPFamily + "/" + family_list[i]; //update download file from sftp
                                        b = Properties.Settings.Default.giSaveLocal + "\\" + family_list[i];//update download folder to pc 
                                        using (var file = File.OpenWrite(b))
                                        {
                                            sftp.DownloadFile(c, file);//download file
                                        }
                                    }
                                    i = i + 1;
                                }
                                familyStatus = true;
                            }
                            catch
                            {
                                MessageBox.Show("Failed to download family information from server, checking backups", "FTP contained no information");
                                familyStatus=false;
                            }

                            //check to see if they have saved Christmas
                            try
                            {
                                i = 0;
                                while (i < saved_list.Count) //cycle through list of files
                                {
                                    if (saved_list[i].ToString().Contains(intRef.ToString()))
                                    {
                                        lblSavedChristmas.Content = "Saved";
                                        lblSavedChristmas.Visibility= Visibility.Visible;
                                    }
                                    i = i + 1;
                                }
                                savedStatus = true;
                            }
                            catch
                            {
                                MessageBox.Show("Failed to connect to saved xmas folder on server, checking backups", "FTP contained no information");
                                savedStatus=false;
                            }

                            //download any greenscreen pictures
                            //if photos stored on ftp then download
                            if (Properties.Settings.Default.giGSsetting == "FTP")
                            {
                                try
                                {
                                    i = 0;
                                    while (i < green_list.Count)
                                    {
                                        if (green_list[i].Contains(intRef.ToString()))
                                        {
                                            c = Properties.Settings.Default.giPicsGreenscreen + "/" + green_list[i]; //update download file from sftp
                                            b = Properties.Settings.Default.giSaveLocal + "\\" + green_list[i];//update download folder to pc 
                                            using (var file = File.OpenWrite(b))
                                            {
                                                sftp.DownloadFile(c, file);//download file
                                            }
                                        }
                                        i = i + 1;
                                    }
                                    greenStatus = true; 
                                }
                                catch
                                {
                                    MessageBox.Show("Failed to connect to green screen folder on the FTP site, checking local storage", "FTP contection error - Greenscreen", MessageBoxButton.OK);
                                    greenStatus=false;
                                }
                            }
                            //if stored locally copy from here
                            else
                            {
                                try
                                {
                                    string strExe1;
                                    DirectoryInfo dir1 = new DirectoryInfo(@Properties.Settings.Default.giPicsGreenscreen.ToString());
                                    foreach (FileInfo file in dir1.GetFiles())
                                    {
                                        if (file.FullName.ToString().Contains(intRef.ToString()))
                                        {
                                            string path = file.ToString();
                                            string dt = DateTime.Now.ToString("hhmmss");
                                            string path2 = Properties.Settings.Default.giPicsSanta.ToString() + "\\Backup\\" + dt.ToString() + "" + file.Name;
                                            File.Copy(path, path2);
                                        }
                                    }
                                }
                                catch
                                {
                                    MessageBox.Show("Couldn't connect to local storage of Green Screen pictures, please check settings and try again", "Green screen error", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    greenStatus = false;
                                }
                            }
                        }
                    }
                    catch
                    {
                        bSuccess = false;
                        MessageBox.Show("Failed to connect to ftp site, checking backups");
                    }

                    //if status's have failed or can't connect to ftp then check local backups
                    if (childStatus==false || adultStatus == false || familyStatus == false || savedStatus == false || greenStatus == false )
                    {
                        try
                        {
                            DirectoryInfo dirChild = new DirectoryInfo(@Properties.Settings.Default.giBackupLocation.ToString());
                            foreach (FileInfo file in dirChild.GetFiles())
                            {
                                if (file.FullName.ToString().Contains(intRef.ToString()))
                                {
                                    string path = file.ToString();
                                    string path2 = Properties.Settings.Default.giSaveLocal.ToString() + "\\" + file.FullName.ToString();
                                }
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Couldn't connect to the backup folder on this device, please; check the folder", "Backup location error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    //display pictures in listbox
                    string strExe;
                    DirectoryInfo dir = new DirectoryInfo(@Properties.Settings.Default.giSaveLocal.ToString());
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        strExe = System.IO.Path.GetExtension(file.ToString());
                        if (strExe == ".txt")
                        { }
                        else
                        {
                            lstImages.Add(file.FullName);
                            ListViewItem myItem = new ListViewItem();
                            Image img = new Image();
                            img.Source = new BitmapImage(new Uri(file.FullName));
                            img.Width = 150;
                            img.Height = 150;
                            myItem.Content = img;
                            lstPics.Items.Add(myItem);
                            Label lbl = new Label();
                            string[] words = file.Name.Split('_');
                            if (file.FullName.Contains("Childhood")) {lbl.Content = words[0]+" Childhood Photo";}
                            if (file.FullName.Contains("ChristmasToy")) { lbl.Content = words[0] + " Christmas Toy Photo"; }
                            if (file.FullName.Contains("Achieve")) { lbl.Content = words[0] + " Achievement Photo"; }
                            if (file.FullName.Contains("Famous")) { lbl.Content = words[0] + " with Famous Person Photo"; }
                            if (file.FullName.Contains("Port")) { lbl.Content = words[0] + " Portrait"; }
                            if (file.FullName.Contains("Pres")) { lbl.Content = words[0] + " Present wanting"; }
                            if (file.FullName.Contains("Group")) { lbl.Content ="Family Photo"; }
                            if (file.FullName.Contains("Holiday")) { lbl.Content="Holiday Photo"; }
                            if (file.FullName.Contains("GS")) { lbl.Content = "Greenscreen Photo"; }
                            lstPics.Items.Add(lbl);
                        }
                    }
                }
            }

        }

        void overrideTick(object sender, EventArgs e)
        {

        }

        private void loadIntervals()
        {
            txtFamilyTimerInterval.Text = Properties.Settings.Default.giTimerFamily.ToString();
            txtOverrideTimerInterval.Text = Properties.Settings.Default.giTimerOverride.ToString();
            txtNameTimerInterval.Text = Properties.Settings.Default.giNameTimer.ToString();
        }
        private void loadGrottoInfo()
        {
            //load names
            txtElfActor.Text = Properties.Settings.Default.giElfActor.ToString();
            txtElfName.Text = Properties.Settings.Default.giElfName.ToString();
            txtSantaActor.Text = Properties.Settings.Default.giSantaActor.ToString();
            txtGrottoName.Text = Properties.Settings.Default.giGrottoName.ToString();
            loadGrottos();
            loadGrottoLabels();
        }

        private void loadGrottoLabels()
        {
            lblGrottoName.Content = Properties.Settings.Default.giGrottoName.ToString();
            lblSantaName.Content = Properties.Settings.Default.giSantaActor.ToString();
            lblElfActor.Content = Properties.Settings.Default.giElfActor.ToString();
            lblElfName.Content = Properties.Settings.Default.giElfName.ToString();
            lblGrottoNumber.Content = Properties.Settings.Default.giGrottoNumber.ToString();
        }

        private void loadGrottos()
        {
            //load grottos into combobox
            txtGrottoCount.Text = Properties.Settings.Default.giGrottoCount.ToString();
            cboGrottoNumber.Items.Clear();
            i = 1;
            while (i < Properties.Settings.Default.giGrottoCount + 1)
            {
                cboGrottoNumber.Items.Add(i);
                i = i + 1;
            }
            cboGrottoNumber.SelectedIndex = Properties.Settings.Default.giGrottoNumber - 1;
        }

        private void loadFTP()
        {
            txtFTPHost.Text = Properties.Settings.Default.giFTPHost.ToString();
            txtFTPUsername.Text = Properties.Settings.Default.giFTPUsername.ToString();
            txtFTPPassword.Text = Properties.Settings.Default.giFTPPassword.ToString();
            txtFTPPort.Text = Properties.Settings.Default.giFTPPort.ToString();
            txtFTPFamily.Text = Properties.Settings.Default.giFTPFamily.ToString();
            txtFTPChild.Text = Properties.Settings.Default.giFTPChild.ToString();
            txtFTPAdult.Text = Properties.Settings.Default.giFTPAdult.ToString();
            txtFTPFolder.Text=Properties.Settings.Default.giFTPFolder.ToString();
            txtFTPSaved.Text = Properties.Settings.Default.giFTPSaved.ToString();
        }

        private void cmdCancelFTP_Click(object sender, RoutedEventArgs e)
        {
            loadFTP();
        }

        private void cmdSaveFTP_Click(object sender, RoutedEventArgs e)
        {
            //check ftp details by connecting to remote server
            //access server
            string Host =txtFTPHost.Text;
            int Port = Int32.Parse(txtFTPPort.Text);
            string Username = txtFTPUsername.Text;
            string Password = txtFTPPassword.Text;

            bool bSuccess = false;

            try
            {
                using (var sftp = new SftpClient(Host, Port, Username, Password))
                {
                    sftp.Connect(); //connect to server
                    sftp.Disconnect();
                    bSuccess= true;
                }
            }
            catch
            {
                MessageBox.Show("Couldn't connect to server with provided FTP details, please reenter and try again. These have not been saved", "FTP connection failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                bSuccess = false;
            }

            if (bSuccess==true)
            {
                Properties.Settings.Default.giFTPHost = txtFTPHost.Text;
                Properties.Settings.Default.giFTPPassword = txtFTPPassword.Text;
                Properties.Settings.Default.giFTPPort = Int32.Parse(txtFTPPort.Text);
                Properties.Settings.Default.giFTPUsername= txtFTPUsername.Text;
                Properties.Settings.Default.giFTPChild = txtFTPChild.Text;
                Properties.Settings.Default.giFTPAdult = txtFTPAdult.Text;
                Properties.Settings.Default.giFTPFamily = txtFTPFamily.Text;
                Properties.Settings.Default.giFTPFolder = txtFTPFolder.Text;
                Properties.Settings.Default.giFTPSaved = txtFTPSaved.Text;
                Properties.Settings.Default.Save();
                MessageBox.Show("FTP details test successful. FTP details stored", "FTP Details", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public async Task downloadBackup()
        {
            gbProgress.Visibility = Visibility.Visible;
            tbSettings.Visibility = Visibility.Hidden;
            cmdCheckPassword.IsEnabled = true;
            txtPassword.IsEnabled = true;
            txtPassword.Password = "";
            bool bsuccess,bsuccess1;
            string c, b;
            //access server
            string Host = Properties.Settings.Default.giFTPHost.ToString();
            int Port = Int32.Parse(Properties.Settings.Default.giFTPPort.ToString());
            string Username = Properties.Settings.Default.giFTPUsername.ToString();
            string Password = Properties.Settings.Default.giFTPPassword.ToString();
            string strAdult = Properties.Settings.Default.giFTPAdult.ToString();
            string strChild = Properties.Settings.Default.giFTPChild.ToString();
            string strFamily = Properties.Settings.Default.giFTPFamily.ToString();
            string strFolder = Properties.Settings.Default.giFTPFolder.ToString();

            ftp_list.Clear();
            using (var sftp = new SftpClient(Host, Port, Username, Password))
            {
                sftp.Connect(); //connect to server

                //download questions
                string strRemoteFolder = Properties.Settings.Default.giFTPFolder.ToString() + "//Questions.txt";
                string strLocalFolder = Properties.Settings.Default.giBackupLocation.ToString() + "//Questions.txt";

                using (var file = File.OpenWrite(strLocalFolder))
                {
                    sftp.DownloadFile(strRemoteFolder, file);//download file
                }

                //count files in different folders
                string strRemoteFolderC = Properties.Settings.Default.giFTPChild.ToString();
                List<string> child_list = new List<string>();
                var toReturn = sftp.ListDirectory(strRemoteFolderC).ToList(); //download a list of files on server

                string strRemoteFolderA = Properties.Settings.Default.giFTPAdult.ToString();
                List<string> adult_list = new List<string>();
                var toReturn1 = sftp.ListDirectory(strRemoteFolderA).ToList(); //download a list of files on server

                string strRemoteFolderF = Properties.Settings.Default.giFTPFamily.ToString();
                List<string> family_list = new List<string>();
                var toReturn2 = sftp.ListDirectory(strRemoteFolderF).ToList(); //download a list of files on server

                //progress bar maths

                int pTotal = toReturn.Count + toReturn1.Count + toReturn2.Count;
                int pAmount = Convert.ToInt32(pTotal / 100);
                pbDownload.Minimum = 0;
                pbDownload.Maximum = pTotal;

                child_list = sftp.ListDirectory(strRemoteFolderC).Where(f => !f.IsDirectory).Select(f => f.Name).ToList();
                adult_list = sftp.ListDirectory(strRemoteFolderA).Where(f => !f.IsDirectory).Select(f => f.Name).ToList();
                family_list = sftp.ListDirectory(strRemoteFolderF).Where(f => !f.IsDirectory).Select(f => f.Name).ToList();

                try
                {
                    //download child folders
                    i = 0;
                    while (i < child_list.Count) //cycle through list of files
                    {
                        c = Properties.Settings.Default.giFTPChild + "/" + child_list[i]; //update download file from sftp
                        b = Properties.Settings.Default.giBackupLocation + "\\" + child_list[i];//update download folder to pc 
                        using (var file = File.OpenWrite(b))
                        {
                            sftp.DownloadFile(c, file);//download file
                        }

                        i = i + 1;//next
                    }

                    //download adult folders
                    i = 0;
                    while (i < adult_list.Count) //cycle through list of files
                    {
                        c = Properties.Settings.Default.giFTPChild + "/" + adult_list[i]; //update download file from sftp
                        b = Properties.Settings.Default.giBackupLocation + "\\" + adult_list[i];//update download folder to pc 
                        try
                        {
                            using (var file = File.OpenWrite(b))
                            {
                                sftp.DownloadFile(c, file);//download file
                            }
                        }
                        catch
                        { }
                        i = i + 1;//next
                        pbDownload.Value = pbDownload.Value + 1;
                    }

                    //download family folders
                    i = 0;
                    while (i < family_list.Count) //cycle through list of files
                    {
                        c = Properties.Settings.Default.giFTPFamily + "/" + family_list[i]; //update download file from sftp
                        b = Properties.Settings.Default.giBackupLocation + "\\" + family_list[i];//update download folder to pc 
                        try
                        {
                            using (var file = File.OpenWrite(b))
                            {
                                sftp.DownloadFile(c, file);//download file
                            }
                        }
                        catch
                        { }
                        i = i + 1;//next
                        pbDownload.Value = pbDownload.Value + 1;
                    }

                    MessageBox.Show("Download completed successfully", "Download finished", MessageBoxButton.OK);
                    bsuccess = true;
                }
                catch
                {
                    bsuccess = false;
                    MessageBox.Show("Download Failed, please check the setup folders and try again", "Failed to download", MessageBoxButton.OK);
                }

                //connect to fusemetrix and create a backupfile of all the bookings for today
                string sDate = cboDay.Text + "/" + cboMonth.Text + "/" + cboYear.Text;
                NameValueCollection nv = new NameValueCollection();
                nv.Add("QDate", sDate);

                WebClient wc = new WebClient();
                byte[] ret = wc.UploadValues("https://4k-photos.co.uk/Bart.php", nv);

                var url = "https://4k-photos.co.uk/downloadAllBookings22.php";

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";

                httpRequest.ContentType = "application/x-www-form-urlencoded";

                var data = "QDate=" + sDate;

                //this sends the request for date and time to a php script on the server which in turn 
                //creates a new file with the username date time and bookings1.txt
                //eg 23/12/2021 10:00 would be 231220211000Bookings1.txt
                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    string result = streamReader.ReadToEnd();
                }

                //download the backup file
                try
                {
                    strRemoteFolder = cboDay.Text + cboMonth.Text + cboYear.Text + "BookingsBart.txt";

                    strLocalFolder = Properties.Settings.Default.giBackupLocation.ToString() + "//" + strRemoteFolder;
                    using (var sftp1 = new SftpClient(Host, Port, Username, Password))
                    {
                        sftp1.Connect();

                        //download new booking file
                        using (var file = File.OpenWrite(strLocalFolder))
                        {
                            sftp1.DownloadFile(strRemoteFolder, file);//download file
                        }

                        sftp1.Disconnect();
                    }
                    bsuccess1 = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("error");
                    bsuccess1 = false;
                }

                if (bsuccess == true && bsuccess1 == true)
                {
                    MessageBox.Show("Download completed successfully", "Download finished", MessageBoxButton.OK);
                }
            }
        }

        private async void cmdDownload_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to download all information from server? This may take some time and is only advised before or after opening hours or during longer breaks.", "Download all information?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                pbDownload.Visibility=Visibility.Visible;
                downloadBackup();
                await Task.Delay(TimeSpan.FromMilliseconds(5));
                pbDownload.Value = pbDownload.Value + 1;
            }
        }

        private void cmdSaveGrottoCount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.giGrottoCount = Int32.Parse(txtGrottoCount.Text);
                Properties.Settings.Default.Save();
                loadGrottos();
            }
            catch
            {
                MessageBox.Show("Please ensure the grotto count is a whole number between 1 and 30. Grotto Count has not been saved", "Grotto Count Error");
            }
        }

        private void cmdCancelDetails_Click(object sender, RoutedEventArgs e)
        {
            loadGrottoInfo();
        }

        private void cmdSaveDetails_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.giGrottoName = txtGrottoName.Text;
            Properties.Settings.Default.giSantaActor = txtSantaActor.Text;
            Properties.Settings.Default.giElfActor = txtElfActor.Text;
            Properties.Settings.Default.giElfName = txtElfName.Text;
            Properties.Settings.Default.giGrottoNumber = cboGrottoNumber.SelectedIndex + 1;
            Properties.Settings.Default.Save();
            MessageBox.Show("Grotto Information has been saved and updated", "Save Successful");
            loadGrottoLabels();
        }

        private void cmdCancelFamilyFinderInterval_Click(object sender, RoutedEventArgs e)
        {
            loadIntervals();
        }

        private void cmdCancelOverrideTimer_Click(object sender, RoutedEventArgs e)
        {
            loadIntervals();
        }

        private void cmdSaveFamilyTimerInterval_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.giTimerFamily = Int32.Parse(txtFamilyTimerInterval.Text);
                Properties.Settings.Default.Save();
                MessageBox.Show("Family Timer Interval Updated Successfully", "Timer Updated Succesfully");
                loadTimers();
            }
            catch
            {
                MessageBox.Show("Please ensure the interval is a valid integer and try again, timer has not been updated", "Timer not updated", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void cmdSaveOverrideTimer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.giTimerOverride = Int32.Parse(txtOverrideTimerInterval.Text);
                Properties.Settings.Default.Save();
                MessageBox.Show("Override Timer Interval Updated Successfully", "Timer Updated Succesfully");
                loadTimers();
            }
            catch
            {
                MessageBox.Show("Please ensure the interval is a valid integer and try again, timer has not been updated", "Timer not updated", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void cmdCancelPassword_Click(object sender, RoutedEventArgs e)
        {
            txtNewPassword1.Text = "";
            txtNewPassword2.Text = "";
            txtOldPassword.Text = "";
        }

        private void cmdSavePassword_Click(object sender, RoutedEventArgs e)
        {
            if (txtOldPassword.Text == "" || txtNewPassword2.Text == "" || txtNewPassword1.Text =="")
            {
                MessageBox.Show ("Please ensure you have filled in all the passwords above","Missing Passwords",MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                if (txtOldPassword.Text == Properties.Settings.Default.giPassword)
                {
                    if (txtNewPassword1.Text == txtNewPassword2.Text)
                    {
                        Properties.Settings.Default.giPassword = txtNewPassword1.Text;
                        Properties.Settings.Default.Save();
                        MessageBox.Show("Password has been updated successfully","Password Saved!",MessageBoxButton.OK,MessageBoxImage.None);
                        txtNewPassword1.Text = "";
                        txtNewPassword2.Text = "";
                        txtOldPassword.Text = "";
                    }
                    else
                    {
                        MessageBox.Show("New passwords don't match, please try again", "Password error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Old password is not correct, please try again", "Password Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtOldPassword.Text = "";
                    txtOldPassword.Focus();
                }
            }
        }

        private void tbMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cmdCheckPassword_Click(object sender, RoutedEventArgs e)
        {
            checkPassword();
        }

        private void checkPassword()
        {
            if (txtPassword.Password == Properties.Settings.Default.giPassword)
            {
                tbSettings.Visibility = Visibility.Visible;
                txtPassword.IsEnabled = false;
                cmdCheckPassword.IsEnabled = false;
                intTab = 1;
            }
            else
            {
                MessageBox.Show("Password is incorrect, please try again", "Password wrong!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            txtPassword.Password = "";
            txtPassword.Focus();
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                checkPassword();
            }
        }

        private void cmdStartTimer_Click(object sender, RoutedEventArgs e)
        {
            if (cmdStartTimer.Content == "Start Search Timer")
            {
                familyTimer.Start();
                cmdStartTimer.Content = "Stop Search Timer";
            }
            else
            {
                familyTimer.Stop();
                cmdStartTimer.Content = "Start Search Timer";
            }
            
        }

        private void cmdBackupBrowse_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the Backup Location folder";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtBackup.Text = newSelectedFolderPath;
                Properties.Settings.Default.giBackupLocation = newSelectedFolderPath;
                Properties.Settings.Default.Save();
            }
        }

        private void tbMain_SelectionChanged(object sender, MouseButtonEventArgs e)
        {
            intTab = 0;
        }

        private void cmdPicDownload_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the folder that stores downloaded photos";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtPicDownload.Text = newSelectedFolderPath;
                Properties.Settings.Default.giPicsDownloaded = newSelectedFolderPath;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdPicsSanta_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the folder that has Santa's pictures stored to";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtPicsSanta.Text = newSelectedFolderPath;
                Properties.Settings.Default.giPicsSanta = newSelectedFolderPath;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdPicsGreenscreen_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the folder where greenscreen pictures are located";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtPicsGreenscreen.Text = newSelectedFolderPath;
                Properties.Settings.Default.giPicsGreenscreen = newSelectedFolderPath;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdGreenscreenLoc_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the folder where greenscreen pictures are located";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtPicsGreenscreen.Text = newSelectedFolderPath;
                Properties.Settings.Default.giPicsGreenscreen = newSelectedFolderPath;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdGrottocontrollerLoc_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the folder where greenscreen pictures are located";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtGrottocontrollerLoc.Text = newSelectedFolderPath;
                Properties.Settings.Default.giGrottoControllerLoc = newSelectedFolderPath;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdFamilyLeft_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure this family has left?", "Family Left?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                strStatus = "Empty";
                intStatus = 0;
                updateGrottoController();
                clearFamily();
                clearElements();
                hideElements();
                win2.imageBox.Source= null;
                familyTimer.Start();
            }
        }

        private void updateGrottoController()
        {

        }

        private void clearFamily()
        {
            lblSavedChristmas.Visibility = Visibility.Hidden;
            lblFamilyName.Content = "Awaiting Next Family";
            lblRef.Content = "";
            lblRef.Visibility=Visibility.Hidden;
            gbPics.IsEnabled= false;
            lstPics.Items.Clear();
            cmdFamilyLeft.IsEnabled = false;
            cmdHidden.IsEnabled = false;
            cmdHidden.Content = "Show Hidden Info";
            gbHidden.Visibility = Visibility.Hidden;
            cmdMap.IsEnabled = false;
            cmdMap.Content = "Display Map";

            string[] filePaths = Directory.GetFiles(Properties.Settings.Default.giSaveLocal.ToString());
            foreach (string filePath in filePaths)
            {
                var name = new FileInfo(filePath).Name;
                name = name.ToLower();
                if (name != "Questions.txt")
                {
                    File.Delete(filePath);
                }
            }

        }

        private void cmdHidden_Click(object sender, RoutedEventArgs e)
        {
            hideInfoGB();
            gbHidden.Visibility = Visibility.Visible;
            if (cmdHidden.Content=="Show Hidden Info")
                {
                    cmdHidden.Content = "Hide Hidden Info";
                }
            else
                {
                    cmdHidden.Content = "Show Hidden Info";
                    hideInfoGB();
                }
        }

        private void cmdGoogleAPI_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.giGoogleAPI = txtGoogleAPI.Text;
            Properties.Settings.Default.Save();
        }

        private void cmdLocalSave_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the folder where you will save locally";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtLocalSave.Text = newSelectedFolderPath;
                Properties.Settings.Default.giSaveLocal = newSelectedFolderPath;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdServerSave_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the folder where greenscreen pictures are located";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtServerSave.Text = newSelectedFolderPath;
                Properties.Settings.Default.giSaveServer = newSelectedFolderPath;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdSecondScreen_Click(object sender, RoutedEventArgs e)
        {
            if (cmdSecondScreen.Content == "Turn on Second Screen")
            {
                
                cmdSecondScreen.Content = "Close second screen";
            }
            else
            {
                cmdSecondScreen.Content = "Turn on Second Screen";

            }

        }

        private void cmdOpenGrotto_Click(object sender, RoutedEventArgs e)
        {
            if (cmdOpenGrotto.Content=="Open")
            {
                intStatus = 0;
                strStatus = "Empty";
                cmdBreak.IsEnabled = true;
                cmdOpenGrotto.Content = "Closed";
            }
           else
            {
                intStatus = 2;
                strStatus = "Closed";
                cmdBreak.IsEnabled = false;
                cmdOpenGrotto.Content = "Open";
            }
            updateGrottoController();
        }

        private void cmdBreak_Click(object sender, RoutedEventArgs e)
        {
            intStatus = 3;
            strStatus = "Break";
            updateGrottoController();
        }

        private void cmdMap_Click(object sender, RoutedEventArgs e)
        {
            if (cmdMap.Content=="Display Map")
            {
                cmdMap.Content = "Hide Map";
                intZoom = 1;
                win2.myMap.Visibility = Visibility.Visible;
                hideElements();
                win2.imageBox.Source = null;
                mapTimer.Start();
            }
            else
            {
                cmdMap.Content = "Display Map";
                

                win2.myMap.Visibility = Visibility.Hidden;
            }
        }

        private void cmdShowChild_Click(object sender, RoutedEventArgs e)
        {
            hideInfoGB();
            if (cmdShowChild.Content == "Show Child Info")
            {
                cmdShowChild.Content = "Hide Child Info";
                gbChild.Visibility = Visibility.Visible;
            }
            else
            {
                cmdShowChild.Content = "Show Child Info";
                gbChild.Visibility = Visibility.Hidden;
            }
        }

        private void cmdShowAdult_Click(object sender, RoutedEventArgs e)
        {
            hideInfoGB();
            gbAdult.Visibility = Visibility.Visible;
            if (cmdShowAdult.Content == "Show Adult Info")
            {
                cmdShowAdult.Content = "Hide Adult Info";
            }
            else
            {
                cmdShowAdult.Content = "Show Adult Info";
                hideInfoGB();
            }
        }

        private void cmdDisplayFlip_Click(object sender, RoutedEventArgs e)
        {

            hideElements();
            win2.myMap.Visibility = Visibility.Hidden;
            var biOriginal = (BitmapImage)image;

            var biRotated = new BitmapImage();
            biRotated.BeginInit();
            biRotated.UriSource = biOriginal.UriSource;
            biRotated.Rotation = Rotation.Rotate180;
            biRotated.EndInit();

            win2.imageBox.Source = biRotated;
        }

        private void cmdPicsDisplay_Click(object sender, RoutedEventArgs e)
        {
            hideElements();
            win2.myMap.Visibility = Visibility.Hidden;
            win2.imageBox.Source = image;
        }

        private void cmdPicsFlipLef_Click(object sender, RoutedEventArgs e)
        {

            hideElements();
            win2.myMap.Visibility = Visibility.Hidden;

            var biOriginal = (BitmapImage)image;

            var biRotated = new BitmapImage();
            biRotated.BeginInit();
            biRotated.UriSource = biOriginal.UriSource;
            biRotated.Rotation = Rotation.Rotate270;
            biRotated.EndInit();

            win2.imageBox.Source = biRotated;
        }

        private void cmdPicsFlipRight_Click(object sender, RoutedEventArgs e)
        {

            hideElements();
            win2.myMap.Visibility = Visibility.Hidden;

            var biOriginal = (BitmapImage)image;

            var biRotated = new BitmapImage();
            biRotated.BeginInit();
            biRotated.UriSource = biOriginal.UriSource;
            biRotated.Rotation = Rotation.Rotate90;
            biRotated.EndInit();

            win2.imageBox.Source = biRotated;
        }

        private void cmdPicsHide_Click(object sender, RoutedEventArgs e)
        {
            win2.imageBox.Source = null;
        }

        private void cmdCancelNameTimer_Click(object sender, RoutedEventArgs e)
        {
            txtNameTimerInterval.Text = Properties.Settings.Default.giNameTimer.ToString();
        }

        private void cmdSaveNameTimer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.giNameTimer = Int32.Parse(txtNameTimerInterval.Text);
                Properties.Settings.Default.Save();
                nameTimer.Interval = TimeSpan.FromSeconds(Properties.Settings.Default.giNameTimer);
            }
            catch
            {
                MessageBox.Show("Please only use whole numbers for timerspan interval, this has not been saved. Please try again","Timespan error",MessageBoxButton.OK,MessageBoxImage.Error);
                txtNameTimerInterval.Text = Properties.Settings.Default.giNameTimer.ToString();
            }
        }

        private void cmdShowNames_Click(object sender, RoutedEventArgs e)
        {
            if (cmdShowNames.Content == "Show Names")
            {
                win2.imageBox.Source = null;
                win2.myMap.Visibility = Visibility.Hidden;

                win2.lblAName.Visibility = Visibility.Visible;
                win2.lblARelationship.Visibility = Visibility.Visible;
                win2.lblCGender.Visibility = Visibility.Visible;
                win2.lblCName.Visibility = Visibility.Visible;
                win2.lblCPresent.Visibility = Visibility.Visible;

                win2.lbAName.Visibility = Visibility.Visible;
                win2.lbARelationship.Visibility = Visibility.Visible;
                win2.lbCGender.Visibility = Visibility.Visible;
                win2.lbCName.Visibility = Visibility.Visible;
                win2.lbCPresent.Visibility = Visibility.Visible;

                cmdShowNames.Content = "Hide Names";

                nameTimer.Start();
            }
            else
            {
                cmdShowNames.Content = "Show Names";
                nameTimer.Stop();
                hideElements();
            }
        }

        private void txtNameTimerInterval_Copy_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void cmdSaveSantaPicInterval_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.giDisplayInterval = Int32.Parse(txtDisplaySantaPic.Text);
                Properties.Settings.Default.Save();
                displayTimer.Interval = TimeSpan.FromSeconds(Properties.Settings.Default.giDisplayInterval);
            }
            catch
            {
                MessageBox.Show("Please ensure the time is a valid integer (whole number) this has not been saved", "Interval error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDisplaySantaPic.Text = Properties.Settings.Default.giDisplayInterval.ToString();
            }
        }

        private void cmdCancelSantaPicInterval_Click(object sender, RoutedEventArgs e)
        {
            txtDisplaySantaPic.Text = Properties.Settings.Default.giDisplayInterval.ToString();
        }

        private void cmdCancelPhotoInterval_Click(object sender, RoutedEventArgs e)
        {
            txtPhotoTimer.Text = Properties.Settings.Default.giPhotoInterval.ToString();
        }

        private void cmdSavePhotoInterval_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.giPhotoInterval = Int32.Parse(txtPhotoTimer.Text);
                Properties.Settings.Default.Save();
                photoTimer.Interval = TimeSpan.FromSeconds(Properties.Settings.Default.giPhotoInterval);
            }
            catch
            {
                MessageBox.Show("Please ensure the timespan is an whole number, interval has not been saved","Interval not saved",MessageBoxButton.OK,MessageBoxImage.Error);
                txtPhotoTimer.Text = Properties.Settings.Default.giPhotoInterval.ToString();
            }
        }

        private void cmdShowFamily_Click(object sender, RoutedEventArgs e)
        {
            hideInfoGB();
            gbFamily.Visibility = Visibility.Visible;
            if (cmdShowFamily.Content == "Show Family Info")
            {
                cmdShowFamily.Content = "Hide Family Info";
            }
            else
            {
                cmdShowFamily.Content = "Show Family Info";
                hideInfoGB();
            }
        }

        private void cmdMonitor_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the Location folder to monitor for new family";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtMonitor.Text = newSelectedFolderPath;
                Properties.Settings.Default.giMonitor = newSelectedFolderPath;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdSavedXmas_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the folder where the saved Christmas files are located";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtSavedXmas.Text = newSelectedFolderPath;
                Properties.Settings.Default.giSavedXmas = newSelectedFolderPath;
                Properties.Settings.Default.Save();
            }
        }

        private void rbGSFTP_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.giGSsetting = "FTP";
            Properties.Settings.Default.Save();
        }

        private void rbGSLocal_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.giGSsetting = "Local";
            Properties.Settings.Default.Save();
        }

        private void cmdSaveGS_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.giPicsGreenscreen = txtPicsGreenscreen.Text;
            Properties.Settings.Default.Save();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void cmdBackup1_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the location for the first backup";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtNearby1.Text = newSelectedFolderPath;
                string strBackups = "";
                List<string> lstbackups = Properties.Settings.Default.giBackupLocations.Split(",").ToList();
                lstbackups[0] = newSelectedFolderPath;
                i = 0;
                while (i<lstbackups.Count)
                {
                    strBackups = strBackups + lstbackups[i];
                    i = i + 1;
                    if (i==lstbackups.Count)
                    { }
                    else
                    { 
                        strBackups = strBackups + ",";
                    }
                }
                Properties.Settings.Default.giBackupLocations = strBackups;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdBackup2_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the location for the first backup";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtNearby2.Text = newSelectedFolderPath;
                string strBackups = "";
                List<string> lstbackups = Properties.Settings.Default.giBackupLocations.Split(",").ToList();
                lstbackups[1] = newSelectedFolderPath;
                i = 0;
                while (i < lstbackups.Count)
                {
                    strBackups = strBackups + lstbackups[i];
                    i = i + 1;
                    if (i == lstbackups.Count)
                    { }
                    else
                    {
                        strBackups = strBackups + ",";
                    }
                }
                Properties.Settings.Default.giBackupLocations = strBackups;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdBackup3_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the location for the first backup";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtNearby3.Text = newSelectedFolderPath;
                string strBackups = "";
                List<string> lstbackups = Properties.Settings.Default.giBackupLocations.Split(",").ToList();
                lstbackups[2] = newSelectedFolderPath;
                i = 0;
                while (i < lstbackups.Count)
                {
                    strBackups = strBackups + lstbackups[i];
                    i = i + 1;
                    if (i == lstbackups.Count)
                    { }
                    else
                    {
                        strBackups = strBackups + ",";
                    }
                }
                Properties.Settings.Default.giBackupLocations = strBackups;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdBackup4_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the location for the first backup";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtNearby4.Text = newSelectedFolderPath;
                string strBackups = "";
                List<string> lstbackups = Properties.Settings.Default.giBackupLocations.Split(",").ToList();
                lstbackups[3] = newSelectedFolderPath;
                i = 0;
                while (i < lstbackups.Count)
                {
                    strBackups = strBackups + lstbackups[i];
                    i = i + 1;
                    if (i == lstbackups.Count)
                    { }
                    else
                    {
                        strBackups = strBackups + ",";
                    }
                }
                Properties.Settings.Default.giBackupLocations = strBackups;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdBackup5_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the location for the first backup";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtNearby5.Text = newSelectedFolderPath;
                string strBackups = "";
                List<string> lstbackups = Properties.Settings.Default.giBackupLocations.Split(",").ToList();
                lstbackups[4] = newSelectedFolderPath;
                i = 0;
                while (i < lstbackups.Count)
                {
                    strBackups = strBackups + lstbackups[i];
                    i = i + 1;
                    if (i == lstbackups.Count)
                    { }
                    else
                    {
                        strBackups = strBackups + ",";
                    }
                }
                Properties.Settings.Default.giBackupLocations = strBackups;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdBackup6_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the location for the first backup";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtNearby6.Text = newSelectedFolderPath;
                string strBackups = "";
                List<string> lstbackups = Properties.Settings.Default.giBackupLocations.Split(",").ToList();
                lstbackups[5] = newSelectedFolderPath;
                i = 0;
                while (i < lstbackups.Count)
                {
                    strBackups = strBackups + lstbackups[i];
                    i = i + 1;
                    if (i == lstbackups.Count)
                    { }
                    else
                    {
                        strBackups = strBackups + ",";
                    }
                }
                Properties.Settings.Default.giBackupLocations = strBackups;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdBackup7_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the location for the first backup";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtNearby7.Text = newSelectedFolderPath;
                string strBackups = "";
                List<string> lstbackups = Properties.Settings.Default.giBackupLocations.Split(",").ToList();
                lstbackups[6] = newSelectedFolderPath;
                i = 0;
                while (i < lstbackups.Count)
                {
                    strBackups = strBackups + lstbackups[i];
                    i = i + 1;
                    if (i == lstbackups.Count)
                    { }
                    else
                    {
                        strBackups = strBackups + ",";
                    }
                }
                Properties.Settings.Default.giBackupLocations = strBackups;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdBackup8_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the location for the first backup";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtNearby8.Text = newSelectedFolderPath;
                string strBackups = "";
                List<string> lstbackups = Properties.Settings.Default.giBackupLocations.Split(",").ToList();
                lstbackups[7] = newSelectedFolderPath;
                i = 0;
                while (i < lstbackups.Count)
                {
                    strBackups = strBackups + lstbackups[i];
                    i = i + 1;
                    if (i == lstbackups.Count)
                    { }
                    else
                    {
                        strBackups = strBackups + ",";
                    }
                }
                Properties.Settings.Default.giBackupLocations = strBackups;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdBackup9_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the location for the first backup";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtNearby9.Text = newSelectedFolderPath;
                string strBackups = "";
                List<string> lstbackups = Properties.Settings.Default.giBackupLocations.Split(",").ToList();
                lstbackups[8] = newSelectedFolderPath;
                i = 0;
                while (i < lstbackups.Count)
                {
                    strBackups = strBackups + lstbackups[i];
                    i = i + 1;
                    if (i == lstbackups.Count)
                    { }
                    else
                    {
                        strBackups = strBackups + ",";
                    }
                }
                Properties.Settings.Default.giBackupLocations = strBackups;
                Properties.Settings.Default.Save();
            }
        }

        private void cmdBackup10_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderDialog.Description = "Please select the location for the first backup";
            folderDialog.UseDescriptionForTitle = true;
            if ((bool)folderDialog.ShowDialog(this))
            {
                string newSelectedFolderPath = folderDialog.SelectedPath;
                txtNearby10.Text = newSelectedFolderPath;
                string strBackups = "";
                List<string> lstbackups = Properties.Settings.Default.giBackupLocations.Split(",").ToList();
                lstbackups[9] = newSelectedFolderPath;
                i = 0;
                while (i < lstbackups.Count)
                {
                    strBackups = strBackups + lstbackups[i];
                    i = i + 1;
                    if (i == lstbackups.Count)
                    { }
                    else
                    {
                        strBackups = strBackups + ",";
                    }
                }
                Properties.Settings.Default.giBackupLocations = strBackups;
                Properties.Settings.Default.Save();
            }
        }

        private void txtBookingRef_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                checkRef();
            }
        }

        private void lstPics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double x = lstPics.SelectedIndex / 2;
            x = Math.Round(x);
            i = Convert.ToInt32(x);

            image = new BitmapImage(new Uri(lstImages[i].ToString(), UriKind.Absolute)); 
        }

        private void checkRef()
        {

        }

        private void cmdCheckRef_Click(object sender, RoutedEventArgs e)
        {
            checkRef();
        }

        private void cmdMainDisplay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.giMainMonitor = Int32.Parse(txtMainDisplay.Text.ToString());
                Properties.Settings.Default.Save();
            }
            catch
            {
                MessageBox.Show("Please ensure that the display number is a valid integer, nothing has been saved.","Monitor Invalid",MessageBoxButton.OK,MessageBoxImage.Warning);
            }
        }

        private void cmdSecondDisplay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.giSecondMonitor = Int32.Parse(txtSecondDisplay.Text.ToString());
                Properties.Settings.Default.Save();
            }
            catch
            {
                MessageBox.Show("Please ensure that the display number is a valid integer, nothing has been saved.", "Monitor Invalid", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void tbSettings_KeyDown(object sender, KeyEventArgs e)
        {
            intTab = 1;
        }

        private void tbMain_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (intTab == 0)
            {
                if (e.Source is TabControl)
                {
                    tbSettings.Visibility = Visibility.Hidden;
                    gbProgress.Visibility = Visibility.Hidden;
                    txtPassword.IsEnabled = true;
                    cmdCheckPassword.IsEnabled = true;
                }
            }
        }
    }
}
