using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using Ookii.Dialogs.Wpf;
using ZenSync.ZfApiRef;
using System.Web;

using ZfGroup = ZenSync.ZfApiRef.Group;

namespace ZenSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region constructor
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            UserName = Properties.Settings.Default.UserName;
            Password = Properties.Settings.Default.Password;
            RootDirectory = Properties.Settings.Default.RootDirectory;
            SaveCredentials = Properties.Settings.Default.SaveSettings;
        }
        #endregion
        
        #region properties
        private ZenfolioClient _client;
        public string CatalogFile = "C:\\ZenfolioCatalog.xml";
        public string DefaultPhotoSetName = "Photos";

        private ObservableCollection<ProgressDialog> progressDialogs = new ObservableCollection<ProgressDialog>();


        private ObservableCollection<String> _messages = new ObservableCollection<string>();
        public ObservableCollection<String> Messages
        {
            get { return _messages; }
            set { _messages = value; RaisePropertyChanged("Messages"); }
        }

        private ObservableCollection<String> _directoryRows = new ObservableCollection<string>();
        public ObservableCollection<String> DirectoryRows
        {
            get { return _directoryRows; }
            set { _directoryRows = value; RaisePropertyChanged("DirectoryRows"); }
        }

        private ObservableCollection<String> _catalogRows = new ObservableCollection<string>();
        public ObservableCollection<String> CatalogRows
        {
            get { return _catalogRows; }
            set { _catalogRows = value; RaisePropertyChanged("CatalogRows"); }
        }
        
        private string _rootDirectory = String.Empty;
        public string RootDirectory 
        {
            get { return _rootDirectory; } 
            set 
            {
                _rootDirectory = value;
                if (_saveCredentials)
                    Properties.Settings.Default.RootDirectory = _rootDirectory;
                RaisePropertyChanged("RootDirectory");
            } 
        }
        private string _userName = String.Empty;
        public string UserName 
        { 
            get { return _userName; } 
            set 
            { 
                _userName = value;
                if (_saveCredentials)
                    Properties.Settings.Default.UserName = _userName;
                RaisePropertyChanged("UserName");
            } 
        }
        private string _password = String.Empty;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                if (_saveCredentials)
                    Properties.Settings.Default.Password = _password;
                RaisePropertyChanged("Password");
            }
        }
        private Boolean _saveCredentials = false;
        public Boolean SaveCredentials
        {
            get { return _saveCredentials; }
            set
            {
                _saveCredentials = value;
                if (_saveCredentials)
                    Properties.Settings.Default.SaveSettings = true;
                RaisePropertyChanged("SaveCredentials");
            }
        }
        #endregion

        #region functions
        private void ShowFolderBrowserDialog()
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select the root folder where .";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                MessageBox.Show(this, "Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista to see the new dialog.", "Sample folder browser dialog");
            if ((bool) dialog.ShowDialog(this))
            {
                //MessageBox.Show(this, "The selected folder was: " + dialog.SelectedPath, "Sample folder browser dialog");
                RootDirectory = dialog.SelectedPath;
            }
        }

        private void ShowCredentialDialog()
        {
            using (CredentialDialog dialog = new CredentialDialog())
            {
                // The window title will not be used on Vista and later; there the title will always be "Windows Security".
                dialog.WindowTitle = "Zensync Credentials";
                dialog.MainInstruction = "Please enter your username and password.";
                dialog.Content = "Enter your Zenfolio Username and Password.";
                dialog.ShowSaveCheckBox = true;
                dialog.ShowUIForSavedCredentials = true;
                // The target is the key under which the credentials will be stored.
                // It is recommended to set the target to something following the "Company_Application_Server" pattern.
                // Targets are per user, not per application, so using such a pattern will ensure uniqueness.
                dialog.Target = "ZenSync";
                if (dialog.ShowDialog(this))
                {
                    //MessageBox.Show(this, string.Format("You entered the following information:\nUser name: {0}\nPassword: {1}", dialog.Credentials.UserName, dialog.Credentials.Password), "Credential dialog sample");
                    // Normally, you should verify if the credentials are correct before calling ConfirmCredentials.
                    // ConfirmCredentials will save the credentials if and only if the user checked the save checkbox.
                    dialog.ConfirmCredentials(true);
                    UserName = dialog.Credentials.UserName;
                    Password = dialog.Credentials.Password;
                }
            }
        }

        private void WalkDirectoryTree(System.IO.DirectoryInfo root, ZfGroup parentGroup)
        {
            Log(Messages, String.Format("Entering Directory {0}", root));

            //
            System.IO.DirectoryInfo[] subDirs = null;
            var topId = parentGroup.Id;
            var elements = parentGroup.Elements;

            var files = GetFiles(root);
            if (files != null)
            {
                subDirs = root.GetDirectories();
            }

            //Any stray files in a directory are stored in a PhotoSet called Photos ONLY if there are no subdirs.  If there are subdirs, then the photoset
            //is named after the Directory name
            if (files != null && files.Count() > 0)
            {
                //Look for a Photoset named "Photos" - make this configurable
                PhotoSet photoSet = null;
                if (subDirs.Count() > 0)
                {
                    //photoSet = CheckPhotoSet(parentGroup, parentGroup.Title);
                    //if (photoSet != null)
                    //{
                    //    //Then we need to migrate this photoset to a new "Photos" photoset and delete the old one replacing it with a Group of the same name
                    //    MigratePhotoSetToGallery(parentGroup, photoSet, DefaultPhotoSetName);
                    //}
                    photoSet = CheckPhotoSet(parentGroup, DefaultPhotoSetName);
                    if (photoSet == null)
                    {
                        var photoSetUpdater = new PhotoSetUpdater { Title = DefaultPhotoSetName };
                       photoSet = _client.CreatePhotoSet(parentGroup.Id, PhotoSetType.Gallery, photoSetUpdater);
                    }
                }
                else
                {
                    //There are no subdirectories, so it will be named after the directory name and not the default
                    photoSet = CheckPhotoSet(parentGroup, parentGroup.Title);
                    if (photoSet == null)
                    {
                        var photoSetUpdater = new PhotoSetUpdater { Title = parentGroup.Title };
                        photoSet = _client.CreatePhotoSet(parentGroup.Id, PhotoSetType.Gallery, photoSetUpdater);
                    }
                }
                photoSet = _client.LoadPhotoSet(photoSet.Id, InformationLevel.Level2, true);
                IterateOverFiles(photoSet, files);
            }

            if (subDirs != null)
            {
                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory. 

                    Log(DirectoryRows, dirInfo.Name);
                    //If this dir has no Subdirs, we look for a PhotoSet, if it has SubDirs, we look for a ZfGroup
                    var subDirsOfCurrentDir = dirInfo.GetDirectories();
                    if (subDirsOfCurrentDir.Count() == 0)
                    {
                        //It is possible this dir HAD subdirs, so at least check for a default photoset
                        //We want to do no harm to the Zenfolio account.
                        var group = CheckGroup(parentGroup, dirInfo.Name);
                        PhotoSet photoSet = null;
                        if (group != null)
                        {
                            photoSet = CheckPhotoSet(group, DefaultPhotoSetName);
                        }
                        if (photoSet == null)
                        {
                            photoSet = CheckPhotoSet(parentGroup, dirInfo.Name);
                        }
                        
                        if (photoSet == null)
                        {
                            var photoSetUpdater = new PhotoSetUpdater {Title = dirInfo.Name};
                            photoSet = _client.CreatePhotoSet(parentGroup.Id, PhotoSetType.Gallery, photoSetUpdater);
                        }
                        else
                        {
                            photoSet = _client.LoadPhotoSet(photoSet.Id, InformationLevel.Level2, true);
                        }
                        IterateOverFiles(photoSet, dirInfo.GetFiles());
                    }
                    else
                    {
                        //First, see if there is an existing PhotoSet with this name here, if so, we have to migrate it
                        ZfGroup group = null;
                        var photoSet = CheckPhotoSet(parentGroup, dirInfo.Name);
                        if (photoSet != null)
                        {
                            //Then we need to migrate this photoset to a new "Photos" photoset and delete the old one replacing it with a Group of the same name
                            group = MigratePhotoSetToGallery(parentGroup, photoSet, DefaultPhotoSetName);
                        }

                        //Look for a Group named after this directory - make this configurable?  Make some kind of translation matrix for special characters
                        if (group == null)
                            group = CheckGroup(parentGroup, dirInfo.Name);
                        
                        if (group == null)
                        {
                            //Create the group
                            Log(CatalogRows, String.Format("Need to create Group for this directory {0}", dirInfo.Name));
                            GroupUpdater updater = new GroupUpdater { Title = dirInfo.Name };
                            group = _client.CreateGroup(parentGroup.Id, updater);
                        }
                        else
                        {
                            Log(CatalogRows, String.Format("Found Group for this directory {0}", dirInfo.Name));
                            group = _client.LoadGroup(group.Id, InformationLevel.Level2, true);
                        }

                        WalkDirectoryTree(dirInfo, group);
                    }
                }
            }
            
        }

        private ZfGroup MigratePhotoSetToGallery(ZfGroup parentGroup, PhotoSet photoSet, string defaultPhotoSetName)
        {
            var defaultPhotoSet = CheckPhotoSet(parentGroup, defaultPhotoSetName);
            //var oldPhotoSetName = photoSet.Title;
            var groupUpdater = new GroupUpdater {Title = photoSet.Title};
            var newGroup = _client.CreateGroup(parentGroup.Id, groupUpdater);
            if (defaultPhotoSet == null)
            {
                //create it - there's almost no chance it should exist
                var photoSetUpdater = new PhotoSetUpdater { Title = defaultPhotoSetName };
                defaultPhotoSet = _client.CreatePhotoSet(newGroup.Id, PhotoSetType.Gallery, photoSetUpdater);
            }
            //migrate all the old photos
            int index = 0;
            photoSet = _client.LoadPhotoSet(photoSet.Id, InformationLevel.Level2, true);
            foreach (var photo in photoSet.Photos)
            {
                // Move the photos
                //TODO: Figure out the indexing
                _client.MovePhoto(photoSet.Id, photo.Id, defaultPhotoSet.Id, index);
                index++;
            }
            
            //Do the same for videos?

            //Delete the old photoSet
            _client.DeletePhotoSet(photoSet.Id);
            return newGroup;
        }

        private PhotoSet CheckPhotoSet(ZfGroup group, String photoSetName)
        {
            //re-load all the group info
            group = _client.LoadGroup(group.Id, InformationLevel.Level2, true);
            //is there a photoset in this group?
            var photoSet = group.Elements != null ? (PhotoSet)group.Elements.FirstOrDefault(o => o.Title == photoSetName && o is PhotoSet) : null;
            return photoSet;
        }

        private ZfGroup CheckGroup(ZfGroup parentGroup, String groupName)
        {
            //re-load all the group info
            parentGroup = _client.LoadGroup(parentGroup.Id, InformationLevel.Level2, true);
            //is there a photoset in this group?
            var subGroup = parentGroup.Elements != null ? (ZfGroup)parentGroup.Elements.FirstOrDefault(o => o.Title == groupName && o is ZfGroup) : null;
            return subGroup;
        }

        private void IterateOverFiles(PhotoSet photoSet, FileInfo[] files)
        {
            foreach (System.IO.FileInfo fi in files)
            {
                // In this example, we only access the existing FileInfo object. If we 
                // want to open, delete or modify the file, then 
                // a try-catch block is required here to handle the case 
                // where the file has been deleted since the call to TraverseTree().
                //Todo: Figure out file extension filtering, etc..
                var photo = photoSet.Photos != null ? photoSet.Photos.FirstOrDefault(o => o.FileName == fi.Name) : null;
                if (photo != null)
                {
                    //upload it!
                    Log(DirectoryRows, fi.Name);
                    Log(CatalogRows, String.Format("Already Uploaded {0}", fi.Name));
                }
                else
                {
                    //Do anything?  Update the Caption?
                    Log(DirectoryRows, fi.Name);
                    Log(CatalogRows, String.Format("Need to upload {0}", fi.Name));
                    UploadProc(photoSet.UploadUrl, fi);
                }
                Log(DirectoryRows, fi.Name);
                Log(CatalogRows, fi.DirectoryName);
                //See if they exist, and if they do not, upload them

            }
        }

        private static FileInfo[] GetFiles(DirectoryInfo root)
        {
            System.IO.FileInfo[] files = null;
            // First, process all the files directly under this folder 
            try
            {
                files = root.GetFiles("*.*");
            }
                // This is thrown if even one of the files requires permissions greater 
                // than the application provides. 
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse. 
                // You may decide to do something different here. For example, you 
                // can try to elevate your privileges and access the file again.
                //log.Add(e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            return files;
        }

        private void RootDirButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowFolderBrowserDialog();
        }

        private void CredentialsButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowCredentialDialog();
        }

        private void ZenSync_OnClick(object sender, RoutedEventArgs e)
        {
            //var newDialog = CreateProgressDialog();
            //CreateUploadDialog(newDialog);
            //return;
            //await Task.Run(() => ZenSyncStart());
            Task.Factory.StartNew(() =>
                                  {
                                      ZenSyncStart();
                                  });
        }

        private void ZenSyncStart()
        {
            ZfGroup rootGroup = null;
            Log(Messages, "Getting Catalog");
            Log(Messages, String.Format("Logging into Zenfolio User: {0}", UserName));
            
            if (LogIntoZenfolio(out _client)) return;
            Log(Messages, "Successful in logging in");


            if (true)
            {

                // Load own profile and root Group
                User profile = _client.LoadPrivateProfile();
                rootGroup = _client.LoadGroupHierarchy(profile.LoginName);
                Log(Messages, "Saving Catalog to disk");
                WriteXML(rootGroup);
            }
            else
            {
                rootGroup = ReadXML();
            }
            
            System.IO.DriveInfo di = new System.IO.DriveInfo(RootDirectory);
            if (!di.IsReady)
            {
                MessageBox.Show(this, String.Format("The drive {0} could not be read", di.Name), "Error");
                return;
            }
            System.IO.DirectoryInfo rootDir = new System.IO.DirectoryInfo(RootDirectory);
            WalkDirectoryTree(rootDir, rootGroup);
        }

        public void Log(ObservableCollection<String> collection, string message)
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                collection.Add(message); 
            });
        }

        public void WriteXML(ZfGroup catalog)
        {
            Log(Messages, String.Format("Writing Catalog to disk File: {0}", CatalogFile)); 
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(ZfGroup));

            System.IO.StreamWriter file = new System.IO.StreamWriter(CatalogFile);
            writer.Serialize(file, catalog);
            file.Close();
        }

        public ZfGroup ReadXML()
        {
            Log(Messages, String.Format("Reading Catalog from disk File: {0}", CatalogFile)); 
            System.Xml.Serialization.XmlSerializer reader =
                new System.Xml.Serialization.XmlSerializer(typeof(ZfGroup));
            System.IO.StreamReader file = new System.IO.StreamReader(CatalogFile);
            ZfGroup catalog = new ZfGroup();
            catalog = (ZfGroup)reader.Deserialize(file);
            return catalog;
        }

        private bool LogIntoZenfolio(out ZenfolioClient _client)
        {
            _client = new ZenfolioClient();
            if (String.IsNullOrEmpty(UserName) || String.IsNullOrEmpty(Password))
            {
                MessageBox.Show("UserName or Password null, enter credentials and try again");
                return true;
            }

            var loggedIn = _client.Login(UserName, Password);

            if (!loggedIn)
            {
                MessageBox.Show("Zenfolio would not log in - check credentails and try again");
                return true;
            }
            return false;
        }

        private void ShowProgressDialog(ProgressDialog newDialog)
        {
            if (newDialog.IsBusy)
                MessageBox.Show(this, "The progress dialog is already displayed.", "Error");
            else
                newDialog.Show(); // Show a modeless dialog; this is the recommended mode of operation for a progress dialog.
        }
        #endregion

        #region propertychanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region dialog stuff

        private ProgressDialog CreateUploadDialog()
        {
            ProgressDialog _progressDialog = new ProgressDialog()
            {
                WindowTitle = "Zenfolio Upload Progress",
                Text = "Uploading photo/video...",
                Description = "Processing...",
                ShowTimeRemaining = true,
            };
            _progressDialog.DoWork += new System.ComponentModel.DoWorkEventHandler(_uploadFile_DoWork);

            return _progressDialog;
        }

        private void _uploadFile_DoWork(object sender, DoWorkEventArgs e)
        {
            var dialog = sender as ProgressDialog;
            if (dialog == null)
                return;

            // Implement the operation that the progress bar is showing progress of here, same as you would do with a background worker.


            
            
            
            
            
            
            
            for (int x = 0; x <= 100; ++x)
            {
                Thread.Sleep(500);
                // Periodically check CancellationPending and abort the operation if required.
                if (dialog.CancellationPending)
                    return;
                // ReportProgress can also modify the main text and description; pass null to leave them unchanged.
                // If _sampleProgressDialog.ShowTimeRemaining is set to true, the time will automatically be calculated based on
                // the frequency of the calls to ReportProgress.
                dialog.ReportProgress(x, null, string.Format(System.Globalization.CultureInfo.CurrentCulture, "Processing: {0}%", x));
            }
        }



        /// <summary>
        /// Builds upload url
        /// </summary>
        /// <returns>Url to use for image upload.</returns>
        private string BuildUrl(String uloadUrl, FileInfo fileInfo)
        {
            // append query parameters that describe the file being uploaded
            // to the base upload URL
            return String.Format("{0}?filename={1}", uloadUrl,
                                 HttpUtility.UrlEncode(fileInfo.Name));
        }

        /// <summary>
        /// Uploading procedure
        /// </summary>
        public void UploadProc(String uploadURL, FileInfo fileInfo)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(BuildUrl(uploadURL, fileInfo));
            req.AllowWriteStreamBuffering = false;
            req.Method = "POST";

            // put correct user token in request headers
            req.Headers.Add("X-Zenfolio-Token", _client.Token);
            req.ContentType = GetMimeType(fileInfo);
            req.ContentLength = fileInfo.Length;

            // Prepare to read the file and push it into request stream
            BinaryReader fileReader = new BinaryReader(
            new FileStream(fileInfo.FullName, FileMode.Open));
            Stream requestStream = req.GetRequestStream();

            // Upload the data
            try
            {
                // Create a buffer for image data
                const int bufSize = 1024;
                byte[] buffer = new byte[bufSize];
                int chunkLength = 0;

                // Transfer data
                while ((chunkLength = fileReader.Read(buffer, 0, bufSize)) > 0)
                {
                    IAsyncResult result = requestStream
                        .BeginWrite(buffer, 0, chunkLength, null, null);

                    //Enter sleep state for Thread.Interrupt() to work
                    result.AsyncWaitHandle.WaitOne();
                    requestStream.EndWrite(result);

                    //Notify UI
                    //this.Invoke(new MethodInvoker(this.OnProgress));
                }
            }
            catch (ThreadInterruptedException)
            {
                // User aborted
                return;
            }
            finally
            {
                // Make sure that streams are closed
                try
                {
                    fileReader.Close();
                    requestStream.Close();
                }
                catch
                {
                }
            }

            try
            {
                // Read image ID from the response
                WebResponse response = req.GetResponse();
                TextReader responseReader = new StreamReader(response.GetResponseStream());

                string imageId = responseReader.ReadToEnd();

                //TODO load photo and construct url for View button
                //_client.LoadPhoto(id);

                // Inform UI that upload finished
                //this.Invoke(new MethodInvoker(this.OnComplete));
            }
            catch (Exception e)
            {
                //this.Invoke(new ExceptionHandler(this.OnError), new object[] { e });
            }
        }

        private string GetMimeType(FileInfo fileInfo)
        {
            switch (fileInfo.Extension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                case ".jpe":
                case ".jfif":
                case ".jfi":
                case ".jif":
                    return "image/jpeg";

                case ".tiff":
                case ".tif":
                    return "image/tiff";

                case ".png":
                    return "image/png";

                case ".gif":
                    return "image/gif";

                default:
                    //MessageBox.Show("Unsupported file type", "Error",
                    //    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
            }
        }
        #endregion
    }
}
