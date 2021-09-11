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
    public partial class PubMethods
    {
        public void resLnk(string thePath)
        {
            string lnkName = thePath.Substring(thePath.LastIndexOf("\\") + 1, thePath.Length - thePath.LastIndexOf("\\") - 1);
            File.Copy(thePath, ".\\" + lnkName);
            File.Delete(thePath);
            Thread.Sleep(3500);//Wait for System
            File.Copy(lnkName, thePath);
        }
    }
}