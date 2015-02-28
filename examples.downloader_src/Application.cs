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
// Downloader application.
//

using System;
using System.IO;
using System.Net;
using Zenfolio.Examples.Downloader.ZfApiRef;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

// resolve collision with System.Text.RegularExpressions.Group
using ZfGroup = Zenfolio.Examples.Downloader.ZfApiRef.Group;

namespace Zenfolio.Examples.Downloader
{
    /// <summary>
    /// Downloader application class
    /// </summary>
    class Application
    {
        /// <summary>
        /// Zenfolio API client
        /// </summary>
        private static ZenfolioClient _client = new ZenfolioClient();

        /// <summary>
        /// User login name
        /// </summary>
        private static string _username = "login";

        /// <summary>
        /// User password
        /// </summary>
        private static string _password = "password";

        /// <summary>
        /// Flag to overwrite image files
        /// </summary>
        private static bool _overwrite = false;

        /// <summary>
        /// Initial directory
        /// </summary>
        private static string _directory = "C:\\zfroot";

        /// <summary>
        /// Flag to be quiet
        /// </summary>
        private static bool _quiet = false;

        /// <summary>
        /// Flag to enable customization of folders through desktop.ini
        /// </summary>
        private static bool _customize = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Array of command line parameters.</param>
        /// <returns>
        /// Exit code.
        ///	0 - operation was successful.
        ///	1 - login failed.
        ///	2 - invalid command line parameters
        ///	3 - any other error
        /// </returns>
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                // Check for well-formed command line
                if(!ParseCommandLine(args))
                {
                    PrintBanner();
                    Console.Error.WriteLine("Invalid command line parameters\n");
                    PrintUsage();
                    return 2;
                }
        
                // Username and password must be present
                if(_username.Length == 0 || _password.Length == 0)
                {
                    PrintBanner();
                    Console.Error.WriteLine("Missing username or password");
                    PrintUsage();
                    return 2;
                }

                PrintBanner();

                // Try to login
                if(!_client.Login(_username, _password))
                {
                    Console.Error.WriteLine("Login failed.");
                    return 1;
                }

                // Load own profile and root Group
                User profile = _client.LoadPrivateProfile();
                ZfGroup rootGroup = _client.LoadGroupHierarchy(profile.LoginName);

                // Start processing
                ProcessGroup(rootGroup, _directory);
                return 0;
            }
            catch(Exception e)
            {
                // Report error and exit
                    Console.Error.WriteLine("Unexpected error:");
                Console.Error.WriteLine(e.ToString());
                return 3;
            }	    
        }

        /// <summary>
        /// Parses command line parameters. Parameters can be passed in any order.
        /// </summary>
        /// <param name="args">Array of command line parameters.</param>
        /// <returns>True if command line was successfully parsed, false otherwise.</returns>
        private static bool ParseCommandLine(string[] args)
        {
            // Expression that matches single command line option. 
            // Both '-' and '/' prefixes allowed.
            Regex r = new Regex(@"^(-|/)(?<1>[a-z])(:(?<2>.*))?$",
                RegexOptions.Compiled | RegexOptions.ExplicitCapture );

            foreach(string option in args)
            {
                Match match = r.Match(option);
                if(!match.Success)
                    return false;

                string name = match.Groups[1].Value;
                string val = match.Groups[2].Value;

                switch(name)
                {
                    case "u":
                        _username = val;
                        break;

                    case "p":
                        _password = val;
                        break;

                    case "x":
                        if (val.Length > 0)
                            return false;
                        _overwrite = true;
                        break;

                    case "d":
                        _directory = val;
                        break;

                    case "c":
                        if (val.Length > 0)
                            return false;
                        _customize = true;
                        break;

                    case "q":
                        if (val.Length > 0)
                            return false;
                        _quiet = true;
                        break;

                    default:
                        // unknown option
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Prints the banner
        /// </summary>
        private static void PrintBanner()
        {
            // Be silent if requested
            if(_quiet)
            return;

            Console.Out.WriteLine("Zenfolio (R) Batch Image Downloader  Version 1.0");
            Console.Out.WriteLine("Copyright (C) 2006-2012 Zenfolio, Inc. All rights reserved.");
            Console.Out.WriteLine("Downloads original image files from your Zenfolio account.");
            Console.Out.WriteLine("");
        }

        /// <summary>
        /// Prints usage information
        /// </summary>
        private static void PrintUsage()
        {
            // be silent if requested
            if (_quiet)
                return;

            Console.Out.WriteLine("USAGE:");
            Console.Out.WriteLine("    Zenfolio.Downloader.exe [options]");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("OPTIONS:");
            Console.Out.WriteLine("    -u:<username>    Login name of the user");
            Console.Out.WriteLine("    -p:<password>    Account password");
            Console.Out.WriteLine("    -d:<directory>   Directory where downloaded files are placed");
            Console.Out.WriteLine("    -x               Overwrite existing files");
            Console.Out.WriteLine("    -q               Quiet mode");
            Console.Out.WriteLine("    -c               Customize downloaded directories");
        }

        /// <summary>
        /// Recursively processes specified Group. Downloads galleries and sub-Groups 
        /// and creates parallel folder hierarchy locally.
        /// </summary>
        /// <param name="group">Group to process.</param>
        /// <param name="path">Path of the directory used as local storage.</param>
        private static void ProcessGroup(ZfGroup group, string path)
        {
            if (_customize)
            {
                // load the group caption in a separate call since it is not loaded
                // with the hierarchy
                ZfGroup g = _client.LoadGroup(group.Id, InformatonLevel.Level2, false);
                CustomizeFolder(path, g.Caption);
            }

            foreach (GroupElement element in group.Elements)
            {
                string folderName = Regex.Replace(element.Title, @"[/\\:*?""<>|]", "_");
                string newPath = Path.Combine(path, folderName);
                if (element is ZfGroup)
                {
                    Directory.CreateDirectory(newPath);
                    ProcessGroup(element as ZfGroup, newPath);
                    continue;
                }

                PhotoSet ps = element as PhotoSet;

                // Download physical galleries only. Collections are skipped.
                if (ps.Type == PhotoSetType.Gallery)
                {
                    Directory.CreateDirectory(newPath);
                    ProcessGallery(ps, newPath);
                }
            }
        }

        /// <summary>
        /// Processes specified gallery. 
        /// Downloads all images in specified gallery and puts them at specified directory.
        /// </summary>
        /// <param name="gallery">Gallery to process.</param>
        /// <param name="path">Path of the directory used to store image files.</param>
        private static void ProcessGallery(PhotoSet gallery, String path)
        {
            // Load gallery data first
            gallery = _client.LoadPhotoSet(gallery.Id, InformatonLevel.Level2, true);

            if (_customize)
                CustomizeFolder(path, gallery.Caption);

            foreach (Photo photo in gallery.Photos)
            {
                // Only photos can be downloaded; Zenfolio does not keep the originals
                // for video files
                if (photo.IsVideo)
                    continue;

                // Do not touch existing files unless user asked for overwrites
                string filePath = Path.Combine(path, photo.FileName);
                if (System.IO.File.Exists(filePath) && !_overwrite)
                {
                    // Report progress
                    if (!_quiet)
                        Console.Out.WriteLine(String.Format("Skipped:     {0}", filePath));
                    continue;
                }

                // Prepare GET request to download image file
                WebRequest req = WebRequest.Create(photo.OriginalUrl);
                req.Method = "GET";
        
                // Put correct user token in request headers
                req.Headers.Add("X-Zenfolio-Token", _client.Token);
                WebResponse response = req.GetResponse();

                // Prepare to read the response stream and store it to file
                using (BinaryWriter writer = new BinaryWriter(new FileStream( filePath, FileMode.Create)))
                using (BinaryReader reader = new BinaryReader(response.GetResponseStream()))
                {
                    // Create a buffer for image data
                    const int bufSize = 65536;
                    byte[] buffer = new byte[bufSize];
                    int chunkLength = 0;

                    // transfer data
                    while ((chunkLength = reader.Read(buffer, 0, bufSize)) > 0)
                        writer.Write(buffer, 0, chunkLength);

                    // report progress
                    if (!_quiet)
                        Console.Out.WriteLine(String.Format("Downloaded:  {0}", filePath));
                }
            }
        }

        /// <summary>
        /// Creates desktop.ini file at specified directory to provide some customization features.
        /// </summary>
        /// <param name="path">Path of directory to customize.</param>
        /// <param name="caption">Directory caption to be included in InfoTip parameter.</param>
        private static void CustomizeFolder(string path, string caption)
        {
            // Set normal attributes if file already exists
            path = Path.Combine(path, "desktop.ini");
            if (System.IO.File.Exists(path))
                System.IO.File.SetAttributes(path, FileAttributes.Normal);

            // Create file and write required info inside
            using(TextWriter writer = new StreamWriter( 
                  new FileStream(path, FileMode.Create, 
                  FileAccess.Write, FileShare.Read),
                  Encoding.ASCII))
            {
                writer.WriteLine("[.ShellClassInfo]");
                writer.WriteLine(String.Format("InfoTip={0}", caption));
            }

            // Set hidden+system attributes, as required
            System.IO.File.SetAttributes(path, FileAttributes.Hidden | FileAttributes.System);
        }
    }
}
