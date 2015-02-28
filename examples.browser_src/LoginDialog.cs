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
// Login dialog. Asks for username and password.
//

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Security;
using System.Windows.Forms;

namespace Zenfolio.Examples.Browser
{
    /// <summary>
    /// Login dialog to retrieve username and password
    /// </summary>
    public class LoginDialog : System.Windows.Forms.Form
    {
        private System.Windows.Forms.TextBox _txtLogin;
        private System.Windows.Forms.TextBox _txtPassword;

	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

        /// <summary>
        /// Constructor, initializes the object.
        /// </summary>
	public LoginDialog()
	{
	    InitializeComponent();
	}

        /// <summary>
        /// Returns the user name entered by the user.
        /// </summary>
        public string UserName
        {
            get { return _txtLogin.Text; }
        }

        /// <summary>
        /// Returns the password entered by the user.
        /// </summary>
        public string Password
        {
            get { return _txtPassword.Text; }
        }

	/// <summary>
	/// Cleans up any used resources.
	/// </summary>
	protected override void Dispose(
            bool disposing
            )
	{
	    if (disposing)
	    {
		if (components != null)
		    components.Dispose();
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
            System.Windows.Forms.Label _lblLogin;
            System.Windows.Forms.Label _lblPassword;
            System.Windows.Forms.Button _btnOk;
            System.Windows.Forms.Button _btnCancel;
            System.Windows.Forms.PictureBox _icon;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginDialog));
            this._txtLogin = new System.Windows.Forms.TextBox();
            this._txtPassword = new System.Windows.Forms.TextBox();
            _lblLogin = new System.Windows.Forms.Label();
            _lblPassword = new System.Windows.Forms.Label();
            _btnOk = new System.Windows.Forms.Button();
            _btnCancel = new System.Windows.Forms.Button();
            _icon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(_icon)).BeginInit();
            this.SuspendLayout();
            // 
            // _lblLogin
            // 
            _lblLogin.Location = new System.Drawing.Point(88, 16);
            _lblLogin.Name = "_lblLogin";
            _lblLogin.Size = new System.Drawing.Size(72, 23);
            _lblLogin.TabIndex = 0;
            _lblLogin.Text = "&Login Name:";
            _lblLogin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _lblPassword
            // 
            _lblPassword.Location = new System.Drawing.Point(96, 56);
            _lblPassword.Name = "_lblPassword";
            _lblPassword.Size = new System.Drawing.Size(64, 23);
            _lblPassword.TabIndex = 2;
            _lblPassword.Text = "&Password:";
            _lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _btnOk
            // 
            _btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            _btnOk.Location = new System.Drawing.Point(136, 96);
            _btnOk.Name = "_btnOk";
            _btnOk.Size = new System.Drawing.Size(75, 23);
            _btnOk.TabIndex = 4;
            _btnOk.Text = "&OK";
            // 
            // _btnCancel
            // 
            _btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            _btnCancel.Location = new System.Drawing.Point(224, 96);
            _btnCancel.Name = "_btnCancel";
            _btnCancel.Size = new System.Drawing.Size(75, 23);
            _btnCancel.TabIndex = 5;
            _btnCancel.Text = "&Cancel";
            // 
            // _icon
            // 
            _icon.Image = ((System.Drawing.Image)(resources.GetObject("_icon.Image")));
            _icon.Location = new System.Drawing.Point(16, 16);
            _icon.Name = "_icon";
            _icon.Size = new System.Drawing.Size(64, 64);
            _icon.TabIndex = 6;
            _icon.TabStop = false;
            // 
            // _txtLogin
            // 
            this._txtLogin.Location = new System.Drawing.Point(168, 16);
            this._txtLogin.Name = "_txtLogin";
            this._txtLogin.Size = new System.Drawing.Size(128, 20);
            this._txtLogin.TabIndex = 1;
            // 
            // _txtPassword
            // 
            this._txtPassword.Location = new System.Drawing.Point(168, 56);
            this._txtPassword.Name = "_txtPassword";
            this._txtPassword.PasswordChar = '*';
            this._txtPassword.Size = new System.Drawing.Size(128, 20);
            this._txtPassword.TabIndex = 3;
            // 
            // LoginDialog
            // 
            this.AcceptButton = _btnOk;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = _btnCancel;
            this.ClientSize = new System.Drawing.Size(312, 133);
            this.Controls.Add(_icon);
            this.Controls.Add(_btnCancel);
            this.Controls.Add(_btnOk);
            this.Controls.Add(this._txtPassword);
            this.Controls.Add(_lblPassword);
            this.Controls.Add(this._txtLogin);
            this.Controls.Add(_lblLogin);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(_icon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

	}
	#endregion
    }
}
