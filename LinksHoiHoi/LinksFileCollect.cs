using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace LinksHoiHoi
{
    public partial class LinksFileCollect : Form
    {
        private int relativeFlag = 0;
        private bool chohukuFlag = false;

        public LinksFileCollect()
        {
            InitializeComponent();
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            button1.Enabled = false;
            button2.Enabled = false;
            button4.Enabled = false;
            button3.Enabled = false;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox2.AllowDrop = false;
            dataGridView1.AllowDrop = false;
            tabPage2.Enabled = false;

            textBox1.Text = "";
            String[] dropFiles;
            dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (String dragFile in dropFiles)
            {
                main(dragFile);
            }
            Cursor.Current = Cursors.Default;
            System.Media.SystemSounds.Asterisk.Play();

            button1.Enabled = true;
            button2.Enabled = true;
            button4.Enabled = true;
            button3.Enabled = true;
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            textBox2.AllowDrop = true;
            dataGridView1.AllowDrop = true;
            tabPage2.Enabled = true;
        }

        private void main(string dragFile)
        {
            string tmpEx = Path.GetExtension(dragFile); //拡張子を調べる
            tmpEx = tmpEx.ToLower(); //拡張子を小文字に変換

            string[] myDouble = new string[65535];
            int doubleNo = 0;
            if (tmpEx == ".indd")
            {
                ifINDDfile(dragFile, ref myDouble, ref doubleNo);
            }
            else if (tmpEx == ".ai")
            {
                ifAIfile(dragFile, ref myDouble, ref doubleNo);
            }
            else if (tmpEx == ".eps")
            {
                ifEPSfile(dragFile, ref myDouble, ref doubleNo);
            }
            else if (tmpEx == ".svg")
            {
                ifSVGfile(dragFile, ref myDouble, ref doubleNo);
            }
            else if (tmpEx == ".psd")
            {
                ifPSDfile(dragFile, ref myDouble, ref doubleNo);
            }
            while (doubleNo > -1)
            {
                Application.DoEvents();
                Cursor.Current = Cursors.WaitCursor;
                if (File.Exists(myDouble[doubleNo]))
                {
                    tmpEx = Path.GetExtension(myDouble[doubleNo]);
                    tmpEx = tmpEx.ToLower();
                    if (tmpEx == ".indd")
                    {
                        ifINDDfile(myDouble[doubleNo], ref myDouble, ref doubleNo);
                        myDouble[doubleNo] = "";
                        doubleNo--;
                    }
                    else if (tmpEx == ".ai")
                    {
                        ifAIfile(myDouble[doubleNo], ref myDouble, ref doubleNo);
                        myDouble[doubleNo] = "";
                        doubleNo--;
                    }
                    else if (tmpEx == ".eps")
                    {
                        ifEPSfile(myDouble[doubleNo], ref myDouble, ref doubleNo);
                        myDouble[doubleNo] = "";
                        doubleNo--;
                    }
                    else if (tmpEx == ".svg")
                    {
                        ifSVGfile(myDouble[doubleNo], ref myDouble, ref doubleNo);
                        myDouble[doubleNo] = "";
                        doubleNo--;
                    }
                    else if (tmpEx == ".psd")
                    {
                        ifPSDfile(myDouble[doubleNo], ref myDouble, ref doubleNo);
                        myDouble[doubleNo] = "";
                        doubleNo--;
                    }
                }
                else
                {
                    myDouble[doubleNo] = "";
                    doubleNo--;
                }
            }
        }

        //AIファイルの場合
        private void ifAIfile(string dragFile, ref string[] myDouble, ref int doubleNo)
        {
            string tmpStr;
            Regex findStr1 = new Regex(@"^%%DocumentFiles:");
            Regex findStr2 = new Regex(@"^%%\+");
            using (FileStream fs = new FileStream(dragFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, FileOptions.SequentialScan))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("Shift_JIS")))
                {
                    string strLine;
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        if (findStr1.IsMatch(strLine))
                        {
                            tmpStr = findStr1.Replace(strLine, "");
                            filePathCheck(ref tmpStr, dragFile); //相対パスチェック
                            fileWcheck(tmpStr, ref myDouble, ref doubleNo, dragFile); //重複チェック

                            while ((strLine = sr.ReadLine()) != null)
                            {
                                if (findStr2.IsMatch(strLine))
                                {
                                    tmpStr = findStr2.Replace(strLine, "");
                                    filePathCheck(ref tmpStr, dragFile); //相対パスチェック
                                    fileWcheck(tmpStr, ref myDouble, ref doubleNo, dragFile); //重複チェック
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        //EPSファイルの場合
        private void ifEPSfile(string dragFile, ref string[] myDouble, ref int doubleNo)
        {
            string tmpStr;
            Regex findStr1 = new Regex(@"^%%BeginDocument: ");
            Regex findStr2 = new Regex(@"^\\");
            Regex findStr3 = new Regex(@"^%%Creator: Adobe Illustrator");
            using (FileStream fs = new FileStream(dragFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, FileOptions.SequentialScan))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("Shift_JIS")))
                {
                    string strLine;
                    int errCheckEps = 1;
                    for (int i = 0; i < 15; i++)
                    {
                        strLine = sr.ReadLine();
                        try
                        {
                            if (findStr3.IsMatch(strLine))
                            {
                                errCheckEps = 0;
                            }
                        }
                        catch
                        {
                            return;
                        }

                    }
                    if (errCheckEps == 1)
                    {
                        return;
                    }

                    while ((strLine = sr.ReadLine()) != null)
                    {
                        if (findStr1.IsMatch(strLine))
                        {
                            tmpStr = findStr1.Replace(strLine, "");
                            if (tmpStr.IndexOf("(") > -1)
                            {
                                
                                tmpStr = tmpStr.Replace("(", "");
                                tmpStr = tmpStr.Replace(")", "");
                                string[] mySplit = {"\\\\"};
                                string[] tmpStrSplit = tmpStr.Split(mySplit, StringSplitOptions.None);
                                tmpStr = "";

                                for (int j = 0; j < tmpStrSplit.Length; j++)
                                {
                                    string src = tmpStrSplit[j];
                                    if (src.IndexOf("\\") > -1)
                                    {
                                        int srcVal = src.Length;
                                        int noOfsrc = Array.FindAll(src.ToCharArray(), delegate(char x) { return x == '\\'; }).Length;
                                        byte[] myB = new byte[srcVal - noOfsrc * 3];
                                        byte[] myTmpB = new byte[1];
                                        string tmpSrc;
                                        int tmpVal = 0;

                                        for (int i = 0; i < myB.Length; i++)
                                        {

                                            tmpSrc = src.Substring(tmpVal, 1);
                                            if (tmpSrc == "\\")
                                            {
                                                tmpSrc = src.Substring(tmpVal + 1, 3);
                                                myB[i] = (byte)Convert.ToInt32(tmpSrc, 8);
                                                tmpVal = tmpVal + 4;
                                            }
                                            else
                                            {
                                                myTmpB = Encoding.ASCII.GetBytes(tmpSrc);
                                                myB[i] = myTmpB[0];
                                                tmpVal++;
                                            }

                                        }
                                        string tmpFileName = Encoding.GetEncoding(932).GetString(myB);
                                        tmpStr = tmpStr + "\\" + tmpFileName;
                                    }
                                    else 
                                    {
                                        tmpStr = tmpStr + "\\" + tmpStrSplit[j];
                                    }

                                }
                                tmpStr = findStr2.Replace(tmpStr, "");
                            }
                            filePathCheck(ref tmpStr, dragFile); //相対パスチェック
                            fileWcheck(tmpStr, ref myDouble, ref doubleNo, dragFile); //重複チェック
                        }
                    }
                }
            }
        }

        //INDDファイルの場合
        private void ifINDDfile(string dragFile, ref string[] myDouble, ref int doubleNo)
        {
            string tmpStr;
            string strLine;
            Regex findStr1 = new Regex(@"^ +<stRef:lastURL>file:");
            Regex findStr2 = new Regex(@"<.+$");
            Regex findStr3 = new Regex(@"^///");
            //INDD解析
            int tmpNo = 0;
            int tmpLineNo = 0;
            using (FileStream fs = new FileStream(dragFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, FileOptions.SequentialScan))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("Shift_JIS")))
                {
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        tmpLineNo++;
                        if (strLine.IndexOf("<xmpMM:RenditionClass>default</xmpMM:RenditionClass>") > -1)
                        {
                            tmpNo = tmpLineNo;
                        }
                    }
                }
            }

            using (FileStream fs = new FileStream(dragFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, FileOptions.SequentialScan))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("Shift_JIS")))
                {
                    //不要な部分を読み飛ばす
                    for (int i = 0; i < tmpNo; i++)
                    {
                        sr.ReadLine();
                    }

                    while ((strLine = sr.ReadLine()) != null)
                    {
                        if (strLine.IndexOf("<stMfs:linkForm>ReferenceStream</stMfs:linkForm>") > -1)
                        {
                            while ((strLine = sr.ReadLine()) != null)
                            {
                                if (strLine.IndexOf("</stMfs:reference>") > -1)
                                {
                                    break;
                                }
                                if (findStr1.IsMatch(strLine))
                                {
                                    tmpStr = findStr1.Replace(strLine, "");
                                    tmpStr = findStr2.Replace(tmpStr, "");
                                    tmpStr = HttpUtility.UrlDecode(tmpStr, Encoding.GetEncoding("UTF-8"));
                                    tmpStr = findStr3.Replace(tmpStr, "");
                                    tmpStr = tmpStr.Replace("/", @"\");
                                    try
                                    {
                                        string fileName = Path.GetFileName(tmpStr);
                                        filePathCheck(ref tmpStr, dragFile); //相対パスチェック
                                        fileWcheck(tmpStr, ref myDouble, ref doubleNo, dragFile); //重複チェック
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //SVGファイルの場合
        private void ifSVGfile(string dragFile, ref string[] myDouble, ref int doubleNo)
        {
            string tmpStr;
            using (FileStream fs = new FileStream(dragFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, FileOptions.SequentialScan))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string strLine;
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        if (strLine == "</switch>") break;
                        if (strLine.IndexOf("<image ") > -1 && strLine.IndexOf("xlink:href=") > -1)
                        {
                            Match matche = Regex.Match(strLine, "(?<=xlink:href=\")[^\"]+");
                            if (matche.Value.IndexOf(":") > -1) continue;
                            tmpStr = matche.Value;
                            string tmpPath = Path.GetDirectoryName(dragFile);
                            while(tmpStr.IndexOf("../") > -1)
                            {
                                var re = new Regex(@"\.\./");
                                tmpStr = re.Replace(tmpStr, "", 1);
                                tmpPath = tmpPath.Substring(0, tmpPath.LastIndexOf(@"\"));
                            }
                            tmpStr = tmpPath + @"\" + tmpStr;
                            tmpStr = tmpStr.Replace("/", @"\");
                            filePathCheck(ref tmpStr, dragFile); //相対パスチェック
                            fileWcheck(tmpStr, ref myDouble, ref doubleNo, dragFile); //重複チェック
                        }
                    }
                }
            }
        }

        //PSDファイルの場合
        private void ifPSDfile(string dragFile, ref string[] myDouble, ref int doubleNo)
        {
            string tmpStr;
            using (FileStream fs = new FileStream(dragFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, FileOptions.SequentialScan))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string strLine;
                    bool verFlag = false;
                    bool embedFlag = false;
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        if (strLine == "</x:xmpmeta>") break;
                        if (strLine.IndexOf("<stEvt:softwareAgent>") > -1)
                        {
                            if (strLine.IndexOf("Illustrator") > -1) verFlag = true;
                            if (strLine.IndexOf("Photoshop CS") > -1) verFlag = true;
                        }
                        if (strLine.IndexOf("<stMfs:linkForm>EmbedByReference") > -1) embedFlag = true;
                        if (strLine.IndexOf("<stRef:filePath>") > -1)
                        {
                            if (verFlag == true) break;
                            if (embedFlag == false)
                            {
                                //if (strLine.IndexOf("<stRef:filePath>cloud-asset://") > -1) continue;
                                Match matche = Regex.Match(strLine, "(?<=<stRef:filePath>)[^<]+");
                                tmpStr = matche.Value;
                                tmpStr = tmpStr.Replace("file:///", "");
                                tmpStr = HttpUtility.UrlDecode(tmpStr, Encoding.GetEncoding("UTF-8"));
                                tmpStr = tmpStr.Replace("/", @"\");
                                filePathCheck(ref tmpStr, dragFile); //相対パスチェック
                                fileWcheck(tmpStr, ref myDouble, ref doubleNo, dragFile); //重複チェック
                            }
                            else
                            {
                                embedFlag = false;
                            }
                        }
                    }
                }
            }
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog()
            {
                SelectedPath = textBox2.Text,  // 選択されるフォルダの初期値
                RootFolder = Environment.SpecialFolder.Desktop,  // ルート
                Description = "収集先フォルダを選択してください。",   // 説明文
            };

            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                // 選択されたフォルダのパスをラベルに表示
                textBox2.Text = dialog.SelectedPath;
            }
        }

        private void textBox2_DragDrop(object sender, DragEventArgs e)
        {
            String[] dropFiles;
            dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if(Directory.Exists(dropFiles[0]))
            {
                textBox2.Text = dropFiles[0];
            }
            else {
                string tmpPath = Path.GetDirectoryName(dropFiles[0]);
                textBox2.Text = tmpPath;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (Directory.Exists(textBox2.Text))
            {
                int[] SorF = {0,0,0,0};

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = dataGridView1.Rows.Count - 1;
                    progressBar1.Value = i;
                    dataGridView1.Rows[i].Cells[1].Style.BackColor = Color.White;
                    if (Convert.ToBoolean(dataGridView1.Rows[i].Cells[0].Value) == false)
                    {
                        dataGridView1.Rows[i].Cells[1].Value = "－";
                        dataGridView1.Rows[i].Cells[1].Style.BackColor = Color.Gainsboro;
                        continue;
                    }
                    string filePath = dataGridView1.Rows[i].Cells[3].Value.ToString();
                    try
                    {
                        string fileName = Path.GetFileName(filePath);
                        string newFilePath = Path.Combine(textBox2.Text, fileName);
                        try
                        {
                            string tmpPath = Path.GetDirectoryName(filePath);
                            string oldFilePath = Path.Combine(tmpPath, fileName);
                            if (File.Exists(oldFilePath))
                            {
                                if (File.Exists(newFilePath))
                                {
                                    SorF[2]++;
                                    dataGridView1.Rows[i].Cells[1].Value = "重複";
                                    dataGridView1.Rows[i].Cells[1].Style.BackColor = Color.LightCyan;
                                }
                                else
                                {
                                    SorF[0]++;
                                    dataGridView1.Rows[i].Cells[1].Value = "成功";
                                    File.Copy(oldFilePath, newFilePath);
                                }
                            }
                            else
                            {
                                SorF[1]++;
                                dataGridView1.Rows[i].Cells[1].Value = "不明";
                                dataGridView1.Rows[i].Cells[1].Style.BackColor = Color.Khaki;
                            }
                        }
                        catch (ArgumentException)
                        {
                        }
                    }
                    catch
                    {
                        SorF[3]++;
                        dataGridView1.Rows[i].Cells[1].Value = "無効";
                        dataGridView1.Rows[i].Cells[1].Style.BackColor = Color.SandyBrown;
                    }
                }
                System.Media.SystemSounds.Beep.Play();
                myResult(SorF);
                progressBar1.Value = 0;
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show("収集先フォルダを選択してください。");
            }
            Cursor.Current = Cursors.Default;

        }

        private void myResult(int[] SorF)
        {
            //Form2クラスのインスタンスを作成する
            result f = new result(SorF);
            //Form2を表示する
            //ここではモーダルダイアログボックスとして表示する
            //オーナーウィンドウにthisを指定する
            f.ShowDialog(this);
            //フォームが必要なくなったところで、Disposeを呼び出す
            f.Dispose();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            textBox1.Text = "";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string tmpClip = Clipboard.GetText();
            if (tmpClip.IndexOf("\\") > -1)
            {
                Cursor.Current = Cursors.WaitCursor;
                string[] mySplit = { "\r\n" };
                string[] fileList = tmpClip.Split(mySplit, StringSplitOptions.None);
                for (int i = 0; i < fileList.Length; i++)
                {
                    try
                    {
                        int idx;
                        dataGridView1.Rows.Add();
                        idx = dataGridView1.Rows.Count - 1;
                        dataGridView1.Rows[idx].Cells[0].Value = true;
                        dataGridView1.Rows[idx].Cells[2].Value = Path.GetFileName(fileList[i]);
                        dataGridView1.Rows[idx].Cells[3].Value = fileList[i];

                        if (File.Exists(fileList[i]))
                        {

                        }
                        else
                        {
                            dataGridView1.Rows[idx].Cells[3].Style.BackColor = Color.Khaki;
                        }
                    }
                    catch
                    {

                    }
                }
            }
                Cursor.Current = Cursors.Default;
                System.Media.SystemSounds.Asterisk.Play();
                /*
                string tmpClip = Clipboard.GetText();
                if (tmpClip.IndexOf("\\") > -1)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    string[] mySplit = { "\r\n" };
                    string[] fileList = tmpClip.Split(mySplit, StringSplitOptions.None);
                    for (int i = 0; i < fileList.Length; i++)
                    {
                        try
                        {
                            if (File.Exists(fileList[i]))
                            {
                                main(fileList[i]);
                            }
                        }
                        catch
                        { 

                        }
                    }
                    Cursor.Current = Cursors.Default;
                    System.Media.SystemSounds.Asterisk.Play();
                }*/
            }

        //ファイル情報入れ
        private void fileStatusIn(string myFilePath, string tmpFileName, string dragFile)
        {
            int idx;
            dataGridView1.Rows.Add();
            idx = dataGridView1.Rows.Count - 1;
            if (chohukuFlag == true)
            {
                dataGridView1.Rows[idx].Cells[0].Value = false;
            }
            else
            {
                dataGridView1.Rows[idx].Cells[0].Value = true;
            }
            dataGridView1.Rows[idx].Cells[2].Value = tmpFileName;
            dataGridView1.Rows[idx].Cells[3].Value = myFilePath;
            if (relativeFlag == 1)
            {
                dataGridView1.Rows[idx].Cells[3].Style.BackColor = Color.MistyRose;
            }
            else if (relativeFlag == 2)
            {
                dataGridView1.Rows[idx].Cells[3].Style.BackColor = Color.Khaki;
            }
            dataGridView1.Rows[idx].Cells[4].Value = Path.GetFileName(dragFile);
            dataGridView1.Rows[idx].Cells[5].Value = dragFile;
            //Application.DoEvents();
        }

        //相対パスチェック
        private void filePathCheck(ref string tmpStr, string dragFile)
        {
            relativeFlag = 0;
            string tmpPath;
            string tmpDir;
            if (checkBox1.Checked == true)
            {
                if (File.Exists(tmpStr) == false)
                {
                    tmpStr = tmpStr.Replace("/", "\\");
                    string stParentName = Path.GetDirectoryName(dragFile);
                    string[] tmpSplit = stParentName.Split(new string[] { "\\" }, StringSplitOptions.None);
                    string[] splitStr = tmpStr.Split(new string[] { "\\" }, StringSplitOptions.None);
                    for (int i = 0; i < splitStr.Length; i++)
                    {
                        tmpPath = stParentName;
                        tmpDir = "";
                        for (int j = i; j < splitStr.Length; j++)
                        {
                            tmpDir = tmpDir + "\\" + splitStr[j];
                        }
                        tmpPath = tmpPath + tmpDir;
                        if (File.Exists(tmpPath) == true)
                        {
                            tmpStr = tmpPath;
                            relativeFlag = 1;
                            break;
                        }
                    }
                }
            }
            if (File.Exists(tmpStr) == false)
            {
                relativeFlag = 2;
            }
        }

        //重複チェック
        private void fileWcheck(string tmpStr, ref string[] myDouble, ref int doubleNo, string dragFile)
        {
            chohukuFlag = false;
            string tmpEx;
            if (File.Exists(tmpStr))
            {
                tmpEx = Path.GetExtension(tmpStr);
            }
            else
            {
                tmpEx = "aaa";
            }
            //重複チェック
            bool errCheck = false;
            string tmpFileName = Path.GetFileName(tmpStr);
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[2].Value.ToString() == tmpFileName)
                {
                    if (dataGridView1.Rows[i].Cells[3].Value.ToString() == tmpStr)
                    {
                        if (checkBox2.Checked == true)
                        {
                            errCheck = false;
                        }
                        else
                        {
                            errCheck = true;
                        }
                        chohukuFlag = true;
                        break;
                    }
                }
            }
            if (errCheck == false)
            {
                tmpEx = tmpEx.ToLower();
                if ((tmpEx == ".ai") || (tmpEx == ".eps") || (tmpEx == ".svg") || (tmpEx == ".psd"))
                {
                    myDouble[doubleNo] = tmpStr;
                    doubleNo++;
                }
                fileStatusIn(tmpStr, tmpFileName, dragFile);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                label18.Visible = true;
            }
            else
            {
                label18.Visible = false;
            }
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[2].Value.ToString().IndexOf(textBox1.Text) > -1)
                {
                    dataGridView1.Rows[i].Visible = true;
                }
                else
                {
                    dataGridView1.Rows[i].Visible = false;
                }
            }
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                if (e.ColumnIndex > 1)
                {
                    dataGridView1.ClearSelection();
                    DataGridViewCell cell = dataGridView1[e.ColumnIndex, e.RowIndex];
                    cell.Selected = true;
                    var relativeMousePosition = dataGridView1.PointToClient(Cursor.Position);
                    this.contextMenuStrip1.Show(dataGridView1, relativeMousePosition);
                }
            }
        }

        private void LinksFileCollect_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Check = checkBox1.Checked;
            Properties.Settings.Default.Chohuku = checkBox2.Checked;
            Properties.Settings.Default.oriPathCheck = checkBox3.Checked;
            Properties.Settings.Default.app1 = textBox_app1.Text;
            Properties.Settings.Default.app2 = textBox_app2.Text;
            Properties.Settings.Default.app3 = textBox_app3.Text;
            Properties.Settings.Default.path1 = textBox_path1.Text;
            Properties.Settings.Default.path2 = textBox_path2.Text;
            Properties.Settings.Default.path3 = textBox_path3.Text;
            Properties.Settings.Default.Save();
        }

        private void LinksFileCollect_Load(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();
            checkBox1.Checked = Properties.Settings.Default.Check;
            checkBox2.Checked = Properties.Settings.Default.Chohuku;
            checkBox3.Checked = Properties.Settings.Default.oriPathCheck;
            textBox_app1.Text = Properties.Settings.Default.app1;
            textBox_app2.Text = Properties.Settings.Default.app2;
            textBox_app3.Text = Properties.Settings.Default.app3;
            textBox_path1.Text = Properties.Settings.Default.path1;
            textBox_path2.Text = Properties.Settings.Default.path2;
            textBox_path3.Text = Properties.Settings.Default.path3;
            dataGridView1.Columns[5].Visible = checkBox3.Checked;
        }

        private void textBox_app1_TextChanged(object sender, EventArgs e)
        {
            toolStripMenuItem2.Text = textBox_app1.Text + "で開く";
        }

        private void textBox_app2_TextChanged(object sender, EventArgs e)
        {
            toolStripMenuItem3.Text = textBox_app2.Text + "で開く";
        }

        private void textBox_app3_TextChanged(object sender, EventArgs e)
        {
            toolStripMenuItem4.Text = textBox_app3.Text + "で開く";
        }

        private int ColumnNum(int selColumn)
        {
            if (selColumn == 2)
            {
                selColumn = 3;
            }
            else if (selColumn == 4)
            {
                selColumn = 5;
            }
            return selColumn;
        }

        private void エクスプローラーで表示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int selColumn = ColumnNum(dataGridView1.SelectedCells[0].ColumnIndex);
            try
            {
                Process.Start("explorer.exe", @"/select," + dataGridView1[selColumn, dataGridView1.SelectedCells[0].RowIndex].Value.ToString());
            }
            catch
            { }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            int selColumn = ColumnNum(dataGridView1.SelectedCells[0].ColumnIndex);
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = textBox_path1.Text;
                psi.Arguments = dataGridView1[selColumn, dataGridView1.SelectedCells[0].RowIndex].Value.ToString();
                Process.Start(psi);
            }
            catch
            { }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            int selColumn = ColumnNum(dataGridView1.SelectedCells[0].ColumnIndex);
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = textBox_path2.Text;
                psi.Arguments = dataGridView1[selColumn, dataGridView1.SelectedCells[0].RowIndex].Value.ToString();
                Process.Start(psi);
            }
            catch
            { }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            int selColumn = ColumnNum(dataGridView1.SelectedCells[0].ColumnIndex);
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = textBox_path3.Text;
                psi.Arguments = dataGridView1[selColumn, dataGridView1.SelectedCells[0].RowIndex].Value.ToString();
                Process.Start(psi);
            }
            catch
            { }
        }

        private void textBox_path1_DragDrop(object sender, DragEventArgs e)
        {
            String[] dropFiles;
            dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (Path.GetExtension(dropFiles[0]) == ".exe")
            {
                textBox_path1.Text = dropFiles[0];
            }
        }

        private void textBox_path2_DragDrop(object sender, DragEventArgs e)
        {
            String[] dropFiles;
            dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (Path.GetExtension(dropFiles[0]) == ".exe")
            {
                textBox_path2.Text = dropFiles[0];
            }
        }

        private void textBox_path3_DragDrop(object sender, DragEventArgs e)
        {
            String[] dropFiles;
            dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (Path.GetExtension(dropFiles[0]) == ".exe")
            {
                textBox_path3.Text = dropFiles[0];
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            dataGridView1.Columns[5].Visible = checkBox3.Checked;
        }
    }
}
