// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System;
using System.Text;
using System.Diagnostics;

namespace Azure.ResourceManager.Samples.Common
{
    public static class Utilities
    {
        public static bool IsRunningMocked { get; set; }
        public static Action<string> LoggerMethod { get; set; }
        public static Func<string> PauseMethod { get; set; }

        public static string ProjectPath { get; set; }
        private static Random _random => new Random();

        public static string ReadLine() => PauseMethod.Invoke();
        public static string CreateRandomName(string namePrefix) => $"{namePrefix}{_random.Next(9999)}";
        public static string CreatePassword() => "azure12345QWE!";
        static Utilities()
        {
            LoggerMethod = Console.WriteLine;
            PauseMethod = Console.ReadLine;
            ProjectPath = ".";
        }

        public static void Log(string message)
        {
            LoggerMethod.Invoke(message);
        }

        public static void Log(object obj)
        {
            if (obj != null)
            {
                LoggerMethod.Invoke(obj.ToString());
            }
            else
            {
                LoggerMethod.Invoke("(null)");
            }
        }

        public static void Log()
        {
            Utilities.Log("");
        }

        public static void Print(ArmResource resource)
        {
            StringBuilder builder = new StringBuilder()
                    .Append("Service bus topic authorization rule: ").Append(resource.Id)
                    .Append("\n\tName: ").Append(resource.Id.Name)
                    .Append("\n\tResourceGroupName: ").Append(resource.Id.ResourceGroupName)
                    .Append("\n\tNamespace Name: ").Append(resource.Id.ResourceType.Namespace);

            Log(builder.ToString());
        }

        public static string PostAddress(string url, string body, IDictionary<string, string> headers = null)
        {
            if (!IsRunningMocked)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        if (headers != null)
                        {
                            foreach (var header in headers)
                            {
                                client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            }
                        }
                        return client.PostAsync(url, new StringContent(body)).Result.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Utilities.Log(ex);
                }
            }

            return "[Running in PlaybackMode]";
        }

        public static string CheckAddress(string url, IDictionary<string, string> headers = null)
        {
            if (!IsRunningMocked)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(300);
                        if (headers != null)
                        {
                            foreach (var header in headers)
                            {
                                client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            }
                        }
                        return client.GetAsync(url).Result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    }
                }
                catch (Exception ex)
                {
                    Utilities.Log(ex);
                }
            }

            return "[Running in PlaybackMode]";
        }

        public static void UploadFileToWebApp(Stream profile, string filePath, string fileName = null)
        {
            if (!IsRunningMocked)
            {
                {
                    var fileinfo = new FileInfo(filePath);

                    if (fileName == null)
                    {
                        fileName = Path.GetFileName(filePath);
                    }
                    while (fileName.Contains("/"))
                    {
                        int slash = fileName.IndexOf("/");
                        string subDir = fileName.Substring(0, slash);
                        fileName = fileName.Substring(slash + 1);
                    }

                    using (var writeStream = profile)
                    {
                        var fileReadStream = fileinfo.OpenRead();
                        fileReadStream.CopyToAsync(writeStream).GetAwaiter().GetResult();
                    }
                }
            }
        }
        public static void UploadFileToFunctionApp(Stream profile, string filePath, string fileName = null)
        {
            if (!IsRunningMocked)
            {
                var fileinfo = new FileInfo(filePath);

                if (fileName == null)
                {
                    fileName = Path.GetFileName(filePath);
                }
                while (fileName.Contains("/"))
                {
                    int slash = fileName.IndexOf("/");
                    string subDir = fileName.Substring(0, slash);
                    fileName = fileName.Substring(slash + 1);
                }

                using (var writeStream = profile)
                {
                    var fileReadStream = fileinfo.OpenRead();
                    fileReadStream.CopyToAsync(writeStream).GetAwaiter().GetResult();
                }
            }
        }

        public static void CreateCertificate(string domainName, string pfxPath, string password)
        {
            if (!IsRunningMocked)
            {
                string args = string.Format(
                    @".\createCert.ps1 -pfxFileName {0} -pfxPassword ""{1}"" -domainName ""{2}""",
                    pfxPath,
                    password,
                    domainName);
                ProcessStartInfo info = new ProcessStartInfo("powershell", args);
                string assetPath = Path.Combine(ProjectPath, "Asset");
                info.WorkingDirectory = assetPath;
                Process process = Process.Start(info);
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    // call "Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Bypass" in powershell if you fail here

                    Utilities.Log("powershell createCert.ps1 script failed");
                }
            }
            else
            {
                //File.Copy(
                //    Path.Combine(Utilities.ProjectPath, "Asset", "SampleTestCertificate.pfx"),
                //    Path.Combine(Utilities.ProjectPath, "Asset", pfxPath),
                //    overwrite: true);
            }
        }

        public static void CreateCertificate(string domainName, string pfxName, string cerName, string password)
        {
            if (!IsRunningMocked)
            {
                string args = string.Format(
                    @".\createCert1.ps1 -pfxFileName {0} -cerFileName {1} -pfxPassword ""{2}"" -domainName ""{3}""",
                    pfxName,
                    cerName,
                    password,
                    domainName);
                ProcessStartInfo info = new ProcessStartInfo("powershell", args);
                string assetPath = Path.Combine(ProjectPath, "Asset");
                info.WorkingDirectory = assetPath;
                Process.Start(info).WaitForExit();
            }
            else
            {
                //File.Copy(
                //    Path.Combine(Utilities.ProjectPath, "Asset", "SampleTestCertificate.pfx"),
                //    Path.Combine(Utilities.ProjectPath, "Asset", pfxName),
                //    overwrite: true);
            }
        }
    }
}