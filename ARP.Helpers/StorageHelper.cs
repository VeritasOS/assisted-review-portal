/*
Copyright (c) 2019 Veritas Technologies LLC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace garb.Helpers
{
    public class StorageHelper
    {
        public static string StorageRootFolder { get; set; }

        public string StoreScreen(string projectName, string language, string build, string screenName, byte[] content)
        {
            string screenPath = GetScreenPath(projectName, language, build, screenName);
            string targetPath = Path.Combine(StorageRootFolder, screenPath);

            try
            {
                var fi = new FileInfo(targetPath);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }

                // TODO validate if content is PNG - if not convert to PNG

                File.WriteAllBytes(targetPath, content);
            }
            catch (Exception)
            {
                return null;
            }

            return screenPath;
        }

        public static string GetScreenPath(string projectName, string language, string build, string screenName, char? separator = null)
        {
            string path = Path.Combine(projectName, language, build, screenName + ".png");

            if (separator != null)
                path = path.Replace(Path.DirectorySeparatorChar, (char)separator);
            
            return path;
        }
        public static IConfigurationRoot Configuration { set; get; }
  
        //public static string GetScreenAbsPath(string projectName, string language, string build, string screenName, char? separator = null)
        //{
        //    var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
        //    Configuration = configurationBuilder.Build();
        //    string basePath = Configuration.GetSection("StorageHelperSettings").GetValue<string>("StorageRootFolder");

        //    string path = Path.Combine(basePath, projectName, language, build, screenName + ".png");            

        //    if (separator != null)
        //        path = path.Replace(Path.DirectorySeparatorChar, (char)separator);
            
        //    return path;
        //}
        public static string GetDiffImagePath(Guid screenFromId, Guid screenToId, char? separator = null)
        {
            string path = Path.Combine("DIFF", screenFromId.ToString() + "_" + screenToId.ToString() + ".png");

            if (separator != null)
                path = path.Replace(Path.DirectorySeparatorChar, (char)separator);

            return path;
        }
  
        public static string GetScreenAbsPath(string relativePath)
        {           
            return Path.Combine(StorageRootFolder, relativePath);            
        }

        public static bool GenerateDiffImageFolder(string path)
        {
            try
            {               
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
