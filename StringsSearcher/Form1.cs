﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Management;
using System.Web;
using MASToolBox;
using Microsoft.Win32;


namespace MASToolBox
{
    public partial class Form1 : Form
    {
        Process process;
        
        ProcessStartInfo startInfo = new ProcessStartInfo();
        
        public Form1()
        {
            InitializeComponent();
            this.textBox1.DragEnter += new DragEventHandler(txtFolderPath_DragEnter);
            this.textBox1.DragDrop += new DragEventHandler(txtFolderPath_DragDrop);
        }


        public string getSHA1()
        {
            if (textBox1.Text == "")
                return null;
            string location = textBox1.Text;
            byte[] apkByte = File.ReadAllBytes(location);
            SHA1 sha1 = new SHA1CryptoServiceProvider();//建立一個SHA1
            byte[] crypto = sha1.ComputeHash(apkByte);//進行SHA1加密
            string result = BitConverter.ToString(crypto);//把加密後的字串從Byte[]轉為字串
            result = result.Replace("-", "").ToLower();
            return result;
        }

        public string getMD5()
        {
            if (textBox1.Text == "")
                return null;
            string location = textBox1.Text;
            byte[] apkByte = File.ReadAllBytes(location);
            MD5 md5 = MD5.Create();//建立一個md5
            byte[] crypto = md5.ComputeHash(apkByte);//進行md5加密
            string result = BitConverter.ToString(crypto);//把加密後的字串從Byte[]轉為字串
            result = result.Replace("-", "").ToLower();
            return result;
        }

        private void txtFolderPath_DragDrop(object sender, DragEventArgs e)
        {
            Console.WriteLine("drop");
            String[] file = (String[])e.Data.GetData(DataFormats.FileDrop);
            String dir = System.IO.Path.GetDirectoryName(file[0]);
            String extension = System.IO.Path.GetExtension(file[0]).ToLower();
            if (extension == ".ipa")
            {
                tbOutput.AppendText("ipa無法反組譯\r\n");
                btn_decompile.Enabled = false;
                btn_sendToMobSF.Enabled = false;
                this.tbOutputDir.Enabled = false;
                this.tbOutputDir.Text = "";
            }
            else if (extension == ".apk")
            {
                btn_decompile.Enabled = true;
                btn_sendToMobSF.Enabled = true;
                this.tbOutputDir.Enabled = true;
            }
            this.textBox1.Text = file[0];
            this.tbOutputDir.Text = dir;
            tbSHA1.Text = getSHA1();
            tbMD5.Text = getMD5();
        }

        private void txtFolderPath_DragEnter(object sender, DragEventArgs e)
        {
            Console.WriteLine("enter");
            String[] file = (String[])e.Data.GetData(DataFormats.FileDrop);
            String extension = System.IO.Path.GetExtension(file[0]);
            extension = extension.ToLower();
            if (extension == ".apk" || extension ==".ipa")
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }


        private void btn_selectAPK_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            String file = openFileDialog1.FileName;
            String extension = System.IO.Path.GetExtension(file).ToLower();
            String dir = System.IO.Path.GetDirectoryName(file);
            if (result == DialogResult.OK)
            {
                if (extension == ".apk")
                {
                    this.textBox1.Text = openFileDialog1.FileName;
                    this.tbOutputDir.Text = dir;
                    btn_decompile.Enabled = true;
                    btn_sendToMobSF.Enabled = true;
                    this.tbOutputDir.Enabled = true;
                    tbSHA1.Text = getSHA1();
                    tbMD5.Text = getMD5();
                }
                else if (extension == ".ipa")
                {
                    tbOutput.AppendText("ipa無法反組譯\r\n");
                    btn_decompile.Enabled = false;
                    btn_sendToMobSF.Enabled = false;
                    this.textBox1.Text = openFileDialog1.FileName;
                    this.tbOutputDir.Enabled = false;
                    this.tbOutputDir.Text = "";
                    tbSHA1.Text = getSHA1();
                    tbMD5.Text = getMD5();
                }
                else 
                {
                    tbOutput.AppendText("不支援此檔案格式\r\n");
                }
            }
        }

        private void btn_selectOutputDir_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                tbOutputDir.Text = folderBrowserDialog1.SelectedPath;
            }
        }


        void process_Exited(object sender, EventArgs e)
        {
            MessageBox.Show("輸出完畢");
        }
        void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.tbOutput.Invoke((MethodInvoker)delegate
            {
                tbOutput.AppendText(e.Data + "\r\n");
            });
        }
        
        void proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            process.Dispose();
            process.Close();
            this.tbOutput.Invoke((MethodInvoker)delegate
            {
                tbOutput.AppendText(e.Data + "\r\n");
            });
        }

        private void decompileWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string args1 = textBox1.Text;
            string args3 = textBox1.Text + ".jar";
            string apkName = args1.Split('\\').Last<string>();
            string args4 = tbOutputDir.Text + "\\" + apkName + ".exctracted\\source code";
            string arg5 = tbOutputDir.Text + "\\" + apkName + ".exctracted\\manifest";


            process = new Process();

            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "tools\\decompiler.bat";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\"", args1, args3, args4, arg5);

            //tbOutput.Text = startInfo.Arguments;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            process.SynchronizingObject = this;


            process.Start();
            process.ErrorDataReceived += proc_ErrorDataReceived;
            process.OutputDataReceived += proc_OutputDataReceived;
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(process_Exited);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        private void btn_decompile_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || tbOutputDir.Text == "")
            {
                MessageBox.Show("請先選擇檔案與輸出資料夾");
                return;
            }
            this.btn_selectAPK.Enabled = false;
            this.btn_selectOutputDir.Enabled = false;
            this.btn_decompile.Enabled = false;
            this.lb_status.Text = "正在反組譯...";
            this.lb_status.Visible = true;
            this.toolStripProgressBar1.Visible = true;
            decompileWorker.RunWorkerAsync();
        }

        private void decompileWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.btn_selectAPK.Enabled = true;
            this.btn_selectOutputDir.Enabled = true;
            this.btn_decompile.Enabled = true;
            this.lb_status.Visible = false;
            this.toolStripProgressBar1.Visible = false;
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.ToString());
            }
        }

        private void btn_sendToMobSF_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
                return;
            tb_APKFile.Text = textBox1.Text;
            tabControl1.SelectedTab = tab_MobSF;
            tb_APKFile.Focus();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mobsf != null)
                KillProcessAndChildren(mobsf.Id);
            if (process != null)
                KillProcessAndChildren(process.Id);
        }


    }
}
