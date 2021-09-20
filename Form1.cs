using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using File = System.IO.File;

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
            if (!Directory.Exists(".\\Python38-32"))
            {
                MessageBox.Show("缺少必要文件\n请检查是否已解压", "Illusion 1.0", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }


            Random rd = new Random();
            if ((rd.Next(1, 80) == 1))
            {
                lnkBox.Font = new System.Drawing.Font("Wingdings", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));//An easter egg:3
                if (rd.Next(0, 1) == 1) { hiddenLabel.Text = "Surprise!"; } else {
                    hiddenLabel.Text = "Do you like Wingdings? It must be the best font of Windows!Emmm..."; }
            }

            ArrayList lnkList = new ArrayList();
            DirectoryInfo TheFolder = new DirectoryInfo("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs");
            foreach (FileInfo NextFile in TheFolder.GetFiles())
                lnkList.Add(NextFile.Name);
            foreach (DirectoryInfo NextFolder in TheFolder.GetDirectories())
                foreach (FileInfo NextFile in NextFolder.GetFiles())
                    lnkList.Add(NextFolder.Name + "\\" + NextFile.Name);

            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            DirectoryInfo TheFolderU = new DirectoryInfo(userFolder + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs");
            foreach (FileInfo NextFile in TheFolderU.GetFiles())
                lnkList.Add("User::" + NextFile.Name);
            foreach (DirectoryInfo NextFolder in TheFolderU.GetDirectories())
                foreach (FileInfo NextFile in NextFolder.GetFiles())
                    lnkList.Add("User::" + NextFolder.Name + "\\" + NextFile.Name);

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
            string[] bannedApps = File.ReadAllLines(@".\denylist.txt");
            for (int lnkListItem= lnkList.Count - 1; lnkListItem >= 0; lnkListItem--)
            {
                for (int bannedAppsItem = 0; bannedAppsItem < bannedApps.Length; bannedAppsItem++)
                {
                    if (lnkList[lnkListItem].ToString().Contains(bannedApps[bannedAppsItem]))
                    {
                        lnkList.RemoveAt(lnkListItem);
                    }
                }
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
            p.StandardInput.WriteLine(".\\clear");//Abnormal exit


        }

        private void button1_Click(object sender, EventArgs e)
        {
            string lnkPath = "";
            if (lnkBox.SelectedItem.ToString().Contains("User::"))
            {
                string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                lnkPath = userFolder + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\" + lnkBox.SelectedItem.ToString().Substring(6, lnkBox.SelectedItem.ToString().Length - 6) + ".lnk";
            }
            else { lnkPath = "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\" + lnkBox.SelectedItem + ".lnk"; }
            Shell32.Shell sh = new Shell32.Shell();
            Shell32.Folder fold = sh.NameSpace(Path.GetDirectoryName(lnkPath));
            Shell32.FolderItem itm = fold.Items().Item(Path.GetFileName(lnkPath));
            Shell32.ShellLinkObject linkObj = (Shell32.ShellLinkObject)itm.GetLink;


            string exePath = linkObj.Path;

            bool OKToExecute = false;
            if (!(exePath.Contains("C:\\Windows") || exePath.Contains("C:\\windows")))
            {
                OKToExecute = true;
            }
            else
            {
                if (MessageBox.Show("这是一个系统软件! 继续修改可能会破坏您的系统.\n是否继续?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    OKToExecute = true;
                }
            }
            if (OKToExecute)
            {
                //try
                //{
                OKButton.Text = "Loading...";
                OKButton.Enabled = false;
                Icon exeIcon;
                if (!File.Exists(exePath))
                {
                    exePath = exePath.Substring(0, 16) + exePath.Substring(22);
                }
                exeIcon = Icon.ExtractAssociatedIcon(exePath);


                Bitmap exeIconBmp = exeIcon.ToBitmap();
                exeIconBmp.Save("exeicon.png", System.Drawing.Imaging.ImageFormat.Png);
                string exeName = exePath.Substring(exePath.LastIndexOf("\\") + 1, exePath.Length - exePath.LastIndexOf("\\") - 1);
                p.StandardInput.WriteLine(".\\Python38-32\\python .\\imagep.py " + colorNote.Text + " " + exeName);
                Thread.Sleep(600);//Wait for Python
                XmlDocument vManifest = new XmlDocument();
                vManifest.Load("Template.xml");
                XmlElement xe = (XmlElement)vManifest.SelectSingleNode("Application/VisualElements");
                xe.SetAttribute("ForegroundText", darkOrLight);
                xe.SetAttribute("ShowNameOnSquare150x150Logo", showNameonoff);

                xe.SetAttribute("Square150x150Logo", exeName + ".tile.png");
                xe.SetAttribute("Square70x70Logo", exeName + ".tileSmall.png");
                xe.SetAttribute("Square44x44Logo", exeName + ".tileSmall.png");

                Color colorChoosed = colorDialog1.Color;
                xe.SetAttribute("BackgroundColor", "#" + colorChoosed.R.ToString("x8").Substring(6) + colorChoosed.G.ToString("x8").Substring(6) + colorChoosed.B.ToString("x8").Substring(6));

                exePath = exePath.Substring(0, exePath.Length - 4);
                vManifest.Save(exePath + ".VisualElementsManifest.xml");
                exePath = exePath.Substring(0, exePath.LastIndexOf("\\") + 1);
                if (File.Exists(exePath + exeName + ".tile.png"))
                {
                    File.Delete(exePath + exeName + ".tile.png");
                }
                if (File.Exists(exePath + exeName + ".tileSmall.png"))
                {
                    File.Delete(exePath + exeName + ".tileSmall.png");
                }
                File.Copy(exeName + ".tile.png", exePath + exeName + ".tile.png");
                File.Copy(exeName + ".tileSmall.png", exePath + exeName + ".tileSmall.png");
                PubMethods pms = new PubMethods();
                pms.resLnk(lnkPath);

                p.StandardInput.WriteLine(".\\clear");
                MessageBox.Show("已成功设置磁贴.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OKButton.Text = "应用磁贴";
                OKButton.Enabled = true;

                //}
                //catch
                //{
                //   MessageBox.Show("遇到奇怪的错误.", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                //   OKButton.Text = "应用磁贴";
                //   OKButton.Enabled = true;
                //}
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
            colorNote.BackColor = System.Drawing.Color.FromArgb(colorChoosed.R, colorChoosed.G, colorChoosed.B);
            if (colorChoosed.R + colorChoosed.G + colorChoosed.B > 384)
            {
                colorNote.ForeColor = System.Drawing.Color.Black;
                checkBox1.Checked = true;
                darkOrLight = "dark";
            }
            else
            {
                colorNote.ForeColor = System.Drawing.Color.White;
                checkBox1.Checked = false;
                darkOrLight = "light";
            }

            HEXNote.Text = "#" + colorChoosed.R.ToString("x8").Substring(6) + colorChoosed.G.ToString("x8").Substring(6) + colorChoosed.B.ToString("x8").Substring(6);


        }

        private void buttonReset_Click(object sender, EventArgs e)
        {

            buttonReset.Text = "Loading...";
            buttonReset.Enabled = false;
            string lnkPath = "";
            if (lnkBox.SelectedItem.ToString().Contains("User::"))
            {
                string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                lnkPath = userFolder + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\" + lnkBox.SelectedItem.ToString().Substring(6, lnkBox.SelectedItem.ToString().Length - 6) + ".lnk";
            }
            else { lnkPath = "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\" + lnkBox.SelectedItem + ".lnk"; }

            Shell32.Shell sh = new Shell32.Shell();
            Shell32.Folder fold = sh.NameSpace(Path.GetDirectoryName(lnkPath));
            Shell32.FolderItem itm = fold.Items().Item(Path.GetFileName(lnkPath));
            Shell32.ShellLinkObject linkObj = (Shell32.ShellLinkObject)itm.GetLink;
            string exePath = linkObj.Path;
            exePath = exePath.Substring(0, exePath.Length - 4);
            if (!File.Exists(exePath + ".exe"))
            {
                exePath = exePath.Substring(0, 16) + exePath.Substring(22);
            }
            if (File.Exists(exePath + ".VisualElementsManifest.xml")) { File.Delete(exePath + ".VisualElementsManifest.xml"); }
            if (File.Exists(exePath + ".exe.tile.png")) { File.Delete(exePath + ".exe.tile.png"); }
            if (File.Exists(exePath + ".exe.tileSmall.png")) { File.Delete(exePath + ".exe.tileSmall.png"); }
            PubMethods pms = new PubMethods();
            pms.resLnk(lnkPath);
            p.StandardInput.WriteLine(".\\clear");
            buttonReset.Text = "重置此项";
            buttonReset.Enabled = true;
            MessageBox.Show("已成功设置磁贴.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form prAbout = new About();
            prAbout.ShowDialog();
        }

        private void buttonApplyColor_Click(object sender, EventArgs e)
        {
            string fixedHexNote;
            try
            {
                if (HEXNote.Text.Substring(0, 1) == "#")
                {
                    fixedHexNote = HEXNote.Text.Substring(1, 6);
                }
                else
                {
                    fixedHexNote = HEXNote.Text;
                }
                int intHexValueR = Convert.ToInt32("0x" + fixedHexNote.Substring(0, 2), 16);
                int intHexValueG = Convert.ToInt32("0x" + fixedHexNote.Substring(2, 2), 16);
                int intHexValueB = Convert.ToInt32("0x" + fixedHexNote.Substring(4, 2), 16);
                colorNote.Text = intHexValueR.ToString() + " " + intHexValueG.ToString() + " " + intHexValueB.ToString();
                colorNote.BackColor = Color.FromArgb(intHexValueR, intHexValueG, intHexValueB);
                if (intHexValueB+intHexValueG+intHexValueR > 384)
                {
                    colorNote.ForeColor = System.Drawing.Color.Black;
                    checkBox1.Checked = true;
                    darkOrLight = "dark";
                }
                else
                {
                    colorNote.ForeColor = System.Drawing.Color.White;
                    checkBox1.Checked = false;
                    darkOrLight = "light";
                }
            }
            catch { }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            OKButton2.Text = "Loading...";
            OKButton2.Enabled = false;
            string lnkPath = "";
            if (lnkBox.SelectedItem.ToString().Contains("User::"))
            {
                string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                lnkPath = userFolder + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\" + lnkBox.SelectedItem.ToString().Substring(6, lnkBox.SelectedItem.ToString().Length - 6) + ".lnk";
            }
            else { lnkPath = "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\" + lnkBox.SelectedItem + ".lnk"; }

            Shell32.Shell sh = new Shell32.Shell();
            Shell32.Folder fold = sh.NameSpace(Path.GetDirectoryName(lnkPath));
            Shell32.FolderItem itm = fold.Items().Item(Path.GetFileName(lnkPath));
            Shell32.ShellLinkObject linkObj = (Shell32.ShellLinkObject)itm.GetLink;
            string exePath = linkObj.Path;
            exePath = exePath.Substring(0, exePath.Length - 4);
            if (!File.Exists(exePath + ".exe"))
            {
                exePath = exePath.Substring(0, 16) + exePath.Substring(22);
            }

            string exeName = exePath.Substring(exePath.LastIndexOf("\\") + 1, exePath.Length - exePath.LastIndexOf("\\") - 1);
            XmlDocument vManifest = new XmlDocument();
            vManifest.Load("Template.xml");
            XmlElement xe = (XmlElement)vManifest.SelectSingleNode("Application/VisualElements");
            xe.SetAttribute("ForegroundText", darkOrLight);
            xe.SetAttribute("ShowNameOnSquare150x150Logo", showNameonoff);

            xe.SetAttribute("Square150x150Logo", exeName + ".exe.tile.png");
            xe.SetAttribute("Square70x70Logo", exeName + ".exe.tileSmall.png");
            xe.SetAttribute("Square44x44Logo", exeName + ".exe.tileSmall.png");

            Color colorChoosed = colorDialog1.Color;
            xe.SetAttribute("BackgroundColor", "#" + colorChoosed.R.ToString("x8").Substring(6) + colorChoosed.G.ToString("x8").Substring(6) + colorChoosed.B.ToString("x8").Substring(6));

            vManifest.Save(exePath + ".VisualElementsManifest.xml");
            exePath = exePath.Substring(0, exePath.LastIndexOf("\\") + 1);
            if (File.Exists(exePath + exeName + ".exe.tile.png"))
            {
                File.Delete(exePath + exeName + ".exe.tile.png");
            }
            if (File.Exists(exePath + exeName + ".exe.tileSmall.png"))
            {
                File.Delete(exePath + exeName + ".exe.tileSmall.png");
            }
            try
            {
                File.Copy(textBoxPicPath.Text, exePath + exeName + ".exe.tile.png");
                File.Copy(textBoxPicPath.Text, exePath + exeName + ".exe.tileSmall.png");
                
                PubMethods pms = new PubMethods();
                pms.resLnk(lnkPath);
                
                p.StandardInput.WriteLine(".\\clear");
                MessageBox.Show("已成功设置磁贴.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch 
            {
                MessageBox.Show("不是标准路径.", "", MessageBoxButtons.OK);
            }
            
            OKButton2.Text = "应用";
            OKButton2.Enabled = true;
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            textBoxPicPath.Text = openFileDialog1.FileName;
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Pictures";
            openFileDialog1.Filter = "PNG文件|*.png|所有文件|*.*";
            openFileDialog1.ShowDialog();
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string drgdPath = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            textBoxPicPath.Text = drgdPath;
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            { e.Effect = DragDropEffects.All; }
            else { e.Effect = DragDropEffects.None; }
        }
    }
}
