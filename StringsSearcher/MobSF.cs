﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using EasyHttp.Http;
using EasyHttp.Infrastructure;

namespace MASToolBox
{
    public static class MobSF
    {
        
        public static string[] filesToProcess = {"utils.py", "settings.py"};
        public static Dictionary<string, object> fileInfo = new Dictionary<string, object>();

        //備份檔案
        public static void BackupFile(string fileToBackup)
        {
            //從設定中取得MobSF的資料夾路徑
            string path = MASToolBox.Properties.MobSF.Default.MobSFPath;

            if (path == "")
                throw new Exception("請先指定MobSF路徑");
            
            string fileToCopy = path + "\\MobSF\\" + fileToBackup;
            string destinationDirectory = path + "\\backup\\";

            if (!File.Exists(fileToCopy))
                throw new Exception("要備份的檔案不存在");

            //如果備份已存在則不執行備份
            if (File.Exists(destinationDirectory + System.IO.Path.GetFileName(fileToCopy)))
                return;

            try
            {
                //如果資料夾不存在，就創一個
                if(!Directory.Exists(destinationDirectory))
                    Directory.CreateDirectory(destinationDirectory);
                
                //將檔案複製到backup資料夾底下
                File.Copy(fileToCopy, destinationDirectory + System.IO.Path.GetFileName(fileToCopy));
            }
            catch
            {
                throw new Exception("備份" + fileToBackup + "失敗!");
            }
        }

        //從備份資料夾還原檔案
        public static bool RecoveryFileFromBackup(string fileToRecovery)
        {
            //從設定中取得MobSF的資料夾路徑
            string path = MASToolBox.Properties.MobSF.Default.MobSFPath;

            if (path == "")
                throw new Exception("請先指定MobSF路徑");

            string fileToCopy = path + "\\backup\\" + fileToRecovery; 
            string destinationDirectory = path + "\\MobSF\\";

            if (!File.Exists(fileToCopy))
                return false;

            try
            {
                File.Copy(fileToCopy, destinationDirectory + System.IO.Path.GetFileName(fileToCopy), true);
            }
            catch 
            {
                throw new Exception(fileToRecovery + "還原失敗!");
            }

            return true;
        }

        //修改mobsf
        public static void PatchMobSF(string fileToBePatched)
        {
            //編輯settings.py檔
            if (fileToBePatched == "settings.py")
                PatchSettingsPy();

            //編輯utils.py檔
            else if (fileToBePatched == "utils.py")
                PatchUtilsPy();

            else
                throw new Exception("編輯檔案失敗：" + fileToBePatched);
        }

        //編輯utils.py檔
        private static void PatchUtilsPy()
        {
            //從設定中取得MobSF的資料夾路徑
            string path = MASToolBox.Properties.MobSF.Default.MobSFPath;
            List<string> lines;

            if (path == "")//已修改過utils.py了
                return;

            //讀檔
            lines = ReadFile(path + "\\MobSF\\utils.py");

            //檢查標記，確認utils.py是否已被修
            if (lines[0].Trim().StartsWith("# Modified by MASToolBox"))
                return;

            //在每一行print下面，加上sys.stdout.flush()
            for (int i = 0; i < lines.Count; i++)
            {
                //將每一行的換行字元都刪除
                lines[i] = lines[i].Replace("\r", "").Replace("\r\n", "");

                if (lines[i].Trim().StartsWith("print("))
                {
                    if (lines[i].Trim().EndsWith(")"))
                    {
                        string[] tmp = lines[i].Split('p');
                        StringBuilder s = new StringBuilder();
                        s.Append(tmp[0]);//python的縮排
                        s.Append("sys.stdout.flush()");
                        lines.Insert(i + 1, s.ToString());
                    }
                    else 
                    {
                        string[] tmp = lines[i].Split('p');
                        StringBuilder s = new StringBuilder();
                        s.Append(tmp[0]);//python的縮排
                        s.Append("sys.stdout.flush()");
                        lines.Insert(i + 2, s.ToString());
                    }
                }
            }

            //在utils.py的第一行加上被編輯過的標記
            lines.Insert(0, "# Modified by MASToolBox " + DateTime.Now.ToString("yyyy-MM-dd"));

            //寫檔
            WriteFile(path + "\\MobSF\\utils.py", lines);

        }

        //編輯settings.py檔
        private static void PatchSettingsPy()
        {
            //從設定中取得MobSF的資料夾路徑
            string path = MASToolBox.Properties.MobSF.Default.MobSFPath;
            List<string> lines;

            if (path == "")//已修改過settings.py了
                return;

            //讀檔
            lines = ReadFile(path + "\\MobSF\\settings.py");

            //檢查標記，確認utils.py是否已被修改過
            if (lines[0].Trim().StartsWith("# Modified by MASToolBox"))
                return;

            //逐行搜尋VirusTotal相關設定
            //VirusTotal設定在檔案結尾附近，故從最後一行搜尋回來
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                //將每一行的換行字元都刪除
                lines[i] = lines[i].Replace("\r", "").Replace("\r\n", "");

                //將VT_ENABLED設成True
                if (lines[i].Trim().StartsWith("VT_ENABLED"))
                {
                    string[] tmp = lines[i].Split('V');
                    StringBuilder s = new StringBuilder();
                    s.Append(tmp[0]);//python的縮排
                    s.Append("VT_ENABLED = True");
                    lines[i] = s.ToString() ;
                }

                //設定API Key
                else if (lines[i].Trim().StartsWith("VT_API_KEY"))
                {
                    string[] tmp = lines[i].Split('V');
                    StringBuilder s = new StringBuilder();
                    s.Append(tmp[0]);//python的縮排
                    s.Append("VT_API_KEY = \'2692b5e4a40f79fdc35d1a422ef5664f463e797fc33192925eb7f43ed3a54f21\'");
                    lines[i] = s.ToString();
                }

                //將上傳功能設成True
                else if (lines[i].Trim().StartsWith("VT_UPLOAD"))
                {
                    string[] tmp = lines[i].Split('V');
                    StringBuilder s = new StringBuilder();
                    s.Append(tmp[0]);//python的縮排
                    s.Append("VT_UPLOAD = True");
                    lines[i] = s.ToString();
                }

            }

            //在settings.py的第一行加上被編輯過的標記
            lines.Insert(0, "# Modified by MASToolBox " + DateTime.Now.ToString("yyyy-MM-dd"));

            //寫檔
            WriteFile(path + "\\MobSF\\settings.py", lines);

        }

        //讀取python檔，將每一行放入list
        private static List<string> ReadFile(string fileToBeRead)
        {
            try
            {
                using (StreamReader reader = new StreamReader(fileToBeRead))
                {
                    return reader.ReadToEnd().Split('\n').ToList<string>();
                }
            }
            catch
            {
                throw new Exception("無法讀取檔案：" + fileToBeRead);
            }
        }

        //將list內容寫入至檔案
        private static void WriteFile(string fileToWrite, List<string> content)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(fileToWrite))
                {
                    for (int i = 0; i < content.Count; i++)
                    {
                        writer.WriteLine(content[i]);
                    }
                }
            }
            catch
            {
                throw new Exception("無法寫入檔案：" + fileToWrite);
            }
        }

        //上傳並掃描APK
        public static string UploadScan(string file)
        {
            var apiKey = Properties.MobSF.Default.APIKey;
            string fullPath = file;
            FileData myFile = new FileData
            {
                Filename = fullPath,
                FieldName = "file",
                ContentType = "application/vnd.android.package-archive"
            };
            List<FileData> fileList = new List<FileData>() { myFile };
            var http2 = new HttpClient();
            http2.Request.RawHeaders.Add("Authorization", apiKey);
            http2.Request.KeepAlive = true;
            http2.Request.Timeout = 3600000;
            var response = http2.Post("http://127.0.0.1:8000/api/v1/upload", null, fileList, HttpContentTypes.ApplicationJson);
            JObject json = JObject.Parse(response.RawText);

            fileInfo.Add("scan_type", json["scan_type"]);
            fileInfo.Add("file_name", json["file_name"]);
            fileInfo.Add("hash", json["hash"]);
            
            
            var http = new HttpClient();
            http.Request.RawHeaders.Add("Authorization", apiKey);
            http.Request.KeepAlive = true;
            http.Request.Timeout = 3600000;
            http.Post("http://127.0.0.1:8000/api/v1/scan", fileInfo, null, HttpContentTypes.ApplicationJson);
            
            return http.Response.RawText;



        }
    }
}
