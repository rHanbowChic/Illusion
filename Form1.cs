using IWshRuntimeLibrary;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Illusion
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public Process p = new Process();
        public string darkOrLight = "light";
        public string showNameonoff = "on";
        
        
        private void Form1_Load(object sender, EventArgs e)
        {
            Random rd = new Random();
            if (rd.Next(1, 80) == 1)
            {
                lnkBox.Font = new System.Drawing.Font("Wingdings", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));//An easter egg:3

            }
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            DirectoryInfo TheFolderU = new DirectoryInfo(userFolder+"\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs");
            ArrayList lnkList = new ArrayList();
            foreach (FileInfo NextFile in TheFolderU.GetFiles())
                lnkList.Add(NextFile.Name);
            foreach (DirectoryInfo NextFolder in TheFolderU.GetDirectories())
                foreach (FileInfo NextFile in NextFolder.GetFiles())
                    lnkList.Add(NextFolder.Name + "\\" + NextFile.Name);

            DirectoryInfo TheFolder = new DirectoryInfo("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs");
            foreach (FileInfo NextFile in TheFolder.GetFiles())
                lnkList.Add(NextFile.Name);
            foreach (DirectoryInfo NextFolder in TheFolder.GetDirectories())
                foreach (FileInfo NextFile in NextFolder.GetFiles())
                    lnkList.Add(NextFolder.Name + "\\" + NextFile.Name);
            for (int i = lnkList.Count - 1; i >= 0; i--)//.lnk files
            {
                if (!Regex.IsMatch(lnkList[i].ToString(), @"(.*)(\.lnk)"))
                {
                    lnkList.RemoveAt(i);
                }
            }
            for (int i = lnkList.Count - 1; i >= 0; i--)//remove .lnk
            {
                lnkList[i] = lnkList[i].ToString().Substring(0, lnkList[i].ToString().Length - 4);
            }
            for (int i = 0; i < lnkList.Count; i++)//To lnkBox
            {
                this.lnkBox.Items.Add(lnkList[i]);
            }
            lnkBox.SelectedIndex = 0;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();//Start a cmd.exe profile for Python


        }

        private void button1_Click(object sender, EventArgs e)
        {
            string lnkPath = "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\" + lnkBox.SelectedItem + ".lnk";
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShellClass();
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(lnkPath);
            string exePath = shortcut.TargetPath;
            if (!(exePath.Contains("C:\\Windows") || exePath.Contains("C:\\windows")))
            {
                try
                {
                    Icon exeIcon = Icon.ExtractAssociatedIcon(exePath);
                    Bitmap exeIconBmp = exeIcon.ToBitmap();
                    exeIconBmp.Save("exeicon.png", System.Drawing.Imaging.ImageFormat.Png);
                    p.StandardInput.WriteLine(".\\Python38-32\\python .\\imagep.py " + colorNote.Text);
                    Thread.Sleep(600);//Wait for Python
                    XmlDocument vManifest = new XmlDocument();
                    vManifest.Load("Template.xml");
                    XmlElement xe = (XmlElement)vManifest.SelectSingleNode("Application/VisualElements");
                    xe.SetAttribute("ForegroundText", darkOrLight);
                    xe.SetAttribute("ShowNameOnSquare150x150Logo", showNameonoff);
                    exePath = exePath.Substring(0, exePath.Length - 4);
                    vManifest.Save(exePath + ".VisualElementsManifest.xml");
                    exePath = exePath.Substring(0, exePath.LastIndexOf("\\") + 1);
                    p.StandardInput.WriteLine("xcopy /Y tile.png \"" + exePath + "\"");
                    p.StandardInput.WriteLine("xcopy /Y tileSmall.png \"" + exePath + "\"");
                    /*p.StandardInput.WriteLine("xcopy /Y \"" + lnkPath + "\" .\\");
                    p.StandardInput.WriteLine("del \"" + lnkPath + "\"");
                    Thread.Sleep(2000);//Wait for System
                    string lnkName = lnkPath.Substring(lnkPath.LastIndexOf("\\") + 1, lnkPath.Length - lnkPath.LastIndexOf("\\") - 1);
                    lnkPath = lnkPath.Substring(0, lnkPath.LastIndexOf("\\") + 1);
                    p.StandardInput.WriteLine("xcopy /Y " + lnkName + " \"" + lnkPath + "\"");*/

                    p.StandardInput.WriteLine(".\\clean");


                }
                catch
                {
                    MessageBox.Show("这个程序没有图标或无法提取.");
                }
            }
            else
            {
                MessageBox.Show("这是系统程序.");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                darkOrLight = "dark";
            }
            else
            {
                darkOrLight = "light";
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                showNameonoff = "on";
            }
            else
            {
                showNameonoff = "off";
            }
        }

        private void buttonChangeColor_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            Color colorChoosed = colorDialog1.Color;
            colorNote.Text = colorChoosed.R.ToString() + " " + colorChoosed.G.ToString() + " " + colorChoosed.B.ToString();
            colorNote.BackColor = System.Drawing.Color.FromArgb(colorChoosed.R,colorChoosed.G,colorChoosed.B);
        }
    }
}
