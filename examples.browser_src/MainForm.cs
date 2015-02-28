// Copyright (c) 2004-2012 Zenfolio, Inc. All rights reserved.
//
// Permission is hereby granted,  free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction,  including without limitation the rights
// to use, copy, modify, merge,  publish,  distribute,  sublicense,  and/or sell
// copies of the Software,  and  to  permit  persons  to  whom  the Software  is 
// furnished to do so, subject to the following conditions:
// 
// The above  copyright notice  and this permission notice  shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE  IS PROVIDED "AS IS",  WITHOUT WARRANTY OF ANY KIND,  EXPRESS OR
// IMPLIED,  INCLUDING BUT NOT LIMITED  TO  THE WARRANTIES  OF  MERCHANTABILITY,
// FITNESS  FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// AUTHORS  OR  COPYRIGHT HOLDERS  BE LIABLE FOR  ANY CLAIM,  DAMAGES  OR  OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.


//
// Main application window.
//

using System;
using System.Globalization;
using System.Windows.Forms;

using Zenfolio.Examples.Browser.ZfApiRef;

namespace Zenfolio.Examples.Browser
{
    /// <summary>
    /// Main application window
    /// </summary>
    public class MainForm : System.Windows.Forms.Form
    {
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.TreeView _tvGroups;
        private System.Windows.Forms.ImageList _ilGroups;
        private System.Windows.Forms.Splitter _splitter;
        private AxSHDocVw.AxWebBrowser _axWebBrowser;
        private ZenfolioClient _client;

        /// <summary>
        /// Tree node types enumeration
        /// </summary>
        public enum NodeType
        {
            Root = 0,
            Group,
            Gallery,
            Collection,
            Photo
        }

        /// <summary>
        /// Constructor, initializes the object.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(
            bool disposing
            )
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
            this._tvGroups = new System.Windows.Forms.TreeView();
            this._ilGroups = new System.Windows.Forms.ImageList(this.components);
            this._splitter = new System.Windows.Forms.Splitter();
            this._axWebBrowser = new AxSHDocVw.AxWebBrowser();
            ((System.ComponentModel.ISupportInitialize)(this._axWebBrowser)).BeginInit();
            this.SuspendLayout();
            // 
            // _tvGroups
            // 
            this._tvGroups.Dock = System.Windows.Forms.DockStyle.Left;
            this._tvGroups.ImageList = this._ilGroups;
            this._tvGroups.ItemHeight = 18;
            this._tvGroups.Location = new System.Drawing.Point(0, 0);
            this._tvGroups.Name = "_tvGroups";
            this._tvGroups.Size = new System.Drawing.Size(180, 668);
            this._tvGroups.TabIndex = 1;
            this._tvGroups.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnGroupsSelect);
            this._tvGroups.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.OnGroupsExpand);
            // 
            // _ilGroups
            // 
            this._ilGroups.ImageSize = new System.Drawing.Size(16, 16);
            this._ilGroups.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_ilGroups.ImageStream")));
            this._ilGroups.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // _splitter
            // 
            this._splitter.Location = new System.Drawing.Point(180, 0);
            this._splitter.Name = "_splitter";
            this._splitter.Size = new System.Drawing.Size(4, 668);
            this._splitter.TabIndex = 2;
            this._splitter.TabStop = false;
            // 
            // _axWebBrowser
            // 
            this._axWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this._axWebBrowser.Enabled = true;
            this._axWebBrowser.Location = new System.Drawing.Point(184, 0);
            this._axWebBrowser.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("_axWebBrowser.OcxState")));
            this._axWebBrowser.Size = new System.Drawing.Size(843, 668);
            this._axWebBrowser.TabIndex = 3;
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1027, 668);
            this.Controls.Add(this._axWebBrowser);
            this.Controls.Add(this._splitter);
            this.Controls.Add(this._tvGroups);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Zenfolio Browser";
            this.Load += new System.EventHandler(this.OnLoad);
            ((System.ComponentModel.ISupportInitialize)(this._axWebBrowser)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception e)
            {
                MessageBoxOptions options = (MessageBoxOptions)0;
                if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
                {
                    options |= MessageBoxOptions.RightAlign |
                               MessageBoxOptions.RtlReading;
                }

                MessageBox.Show(e.ToString(), "Exception", MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                                options);
            }
        }

        /// <summary>
        /// Occurs before a form is displayed for the first time.
        /// Makes all necessary initializations.
        /// </summary>
        private void OnLoad(object sender, EventArgs e)
        {
            _client = new ZenfolioClient();

            LoginDialog loginDialog = new LoginDialog();

            // try to login in a loop until successfully logged in or 
            // cancelled by the user
            bool loggedIn = false;
            while (!loggedIn)
            {
                // ask for username and password
                DialogResult res = loginDialog.ShowDialog(this);

                // exit if the user doesn't want to enter credentials
                if (res != DialogResult.OK)
                {
                    Application.Exit();
                    return;
                }

                // try to login
                loggedIn = _client.Login(loginDialog.UserName,
                                         loginDialog.Password);
            }

            // load own profile
            User user = _client.LoadPrivateProfile();

            // load and wrap Groups hierarchy
            Group rootGroup = _client.LoadGroupHierarchy(user.LoginName);
            TreeNode rootNode = CreateGroupNode(rootGroup);

            // fix-up the root node of the Group hierarchy
            rootNode.ImageIndex = (int)NodeType.Root;
            rootNode.SelectedImageIndex = rootNode.ImageIndex;
            rootNode.Text = user.DisplayName;
            _tvGroups.Nodes.Add(rootNode);

            // initialize browser pane
            _axWebBrowser.Navigate("about:blank");
            _axWebBrowser.TheaterMode = true;
        }

        /// <summary>
        /// Creates a TreeNode representing Group element.
        /// </summary>
        /// <param name="element">Group element to wrap.</param>
        /// <returns>Constructed tree node.</returns>
        private TreeNode CreateGroupNode(
            GroupElement element
            )
        {
            TreeNode ret = new TreeNode(element.Title);
            ret.Tag = element;

            Group group = element as Group;
            PhotoSet photoSet = element as PhotoSet;

            if (group != null)
            {
                ret.ImageIndex = (int)NodeType.Group;
                foreach (GroupElement child in group.Elements)
                    ret.Nodes.Add(CreateGroupNode(child));
            }
            else if (photoSet != null)
            {
                ret.ImageIndex = photoSet.Type == PhotoSetType.Gallery
                                 ? (int)NodeType.Gallery 
                                 : (int)NodeType.Collection;

                // photoSets contain photos which are not loaded yet; add
                // a dummy node for lazy expansion
                if (photoSet.PhotoCount > 0)
                    ret.Nodes.Add("_Dummy_");
            }

            ret.SelectedImageIndex = ret.ImageIndex;
            return ret;
        }

        /// <summary>
        /// Called when user tries to expand the tree node. Checks if this is a gallery node and 
        /// loads photos if they not loaded yet.
        /// </summary>
        private void OnGroupsExpand(object sender, TreeViewCancelEventArgs e)
        {
            NodeType nt = (NodeType)e.Node.ImageIndex;
            if (nt == NodeType.Gallery || nt == NodeType.Collection)
            {
                PhotoSet ps = (PhotoSet)e.Node.Tag;
                //Load photo set on demand
                if (ps.Photos == null || ps.Photos.Length == 0)
                {
                    // clear dummy nodes
                    e.Node.Nodes.Clear();
                    ps = _client.LoadPhotoSet(ps.Id, ZfApiRef.InformationLevel.Level1, true);
                    e.Node.Tag = ps;

                    // add nodes for each of the loaded photos
                    foreach (Photo p in ps.Photos)
                    {
                        string name = p.Title;
                        if (name==null || name.Length == 0) name = p.FileName;
                        TreeNode n = new TreeNode(name, (int)NodeType.Photo, (int)NodeType.Photo);
                        n.Tag = p;
                        e.Node.Nodes.Add(n);
                    }
                }
            }
        }


        /// <summary>
        /// Called when user selects a tree node in the left pane. Checks if user selected a 
        /// node representing a photo. Displays photo in the browser pane if necessary.
        /// </summary>
        private void OnGroupsSelect(object sender, TreeViewEventArgs e)
        {
            // display the selected photo in the browser pane
            NodeType nt = (NodeType)e.Node.ImageIndex;
            if (nt == NodeType.Photo)
            {
                Photo p = (Photo)e.Node.Tag;

                // construct large photo URL
                string url = p.UrlHost + p.UrlCore + "-4.jpg"
                             + "?sn=" + p.Sequence
                             + "&tk=" + p.UrlToken;

                // this gives access to protected photos
                if (p.AccessDescriptor.AccessType != AccessType.Public)
                    url += "&token=" + _client.Token;

                // make the browser load this image
                _axWebBrowser.Navigate(url);
            }
        }
    }
}
