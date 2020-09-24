using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using WindGoes6.Controls;

namespace RGSS_Extractor
{
    public class Form1 : Form
    {
        public static Form1 GetForm1;

        private readonly Main_Parser parser = new Main_Parser();

        private List<Entry> entries = new List<Entry>();

        private string archive_path = string.Empty;

        private IContainer components;

        private SplitContainer splitContainer1;

        private TreeView explorer_view;

        private OpenFileDialog openFileDialog1;

        private MenuStrip menuStrip1;

        private ToolStripMenuItem fileToolStripMenuItem;

        private ToolStripMenuItem openToolStripMenuItem;

        private ToolStripMenuItem closeArchiveToolStripMenuItem;

        private ToolStripSeparator toolStripSeparator1;

        private ToolStripMenuItem exitToolStripMenuItem;

        private PictureBox pic_preview;

        private ToolStripMenuItem exportArchiveToolStripMenuItem;

        private ContextMenuStrip explorer_menu;

        private ToolStripMenuItem exportToolStripMenuItem;

        private FolderBrowserDialog folderBrowserDialog1;

        private ToolStripMenuItem githubToolStripMenuItem;

        private ImageList imageList1;

        internal ProgressBar progressBar1;

        const uint WM_SYSCOMMAND = 0x0112;
        const uint SC_MOVE = 0xF010;
        private ToolStripMenuItem project1ToolStripMenuItem;
        private ToolStripMenuItem authorYIU2ToolStripMenuItem;
        private ToolStripMenuItem authorKatyushaScarletToolStripMenuItem;
        const uint HTCAPTION = 0x0002;

        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        private static extern int SendMessage(IntPtr hwnd, uint wMsg, uint wParam, uint lParam);
        [DllImport("user32.dll")]
        private static extern int ReleaseCapture();

        public Form1(string rgssFile)
        {
            try { Thread.CurrentThread.CurrentUICulture = new CultureInfo(CultureInfo.InstalledUICulture.Name); }
            catch { }
            InitializeComponent();
            GetForm1 = this;
            if (string.IsNullOrWhiteSpace(rgssFile)) { return; }
            Read_archive(rgssFile);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
            Read_archive(array[0]);
        }

        private void Read_archive(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { return; }
            Close_archive();
            entries = parser.Parse_file(path);
            if (entries != null)
            {
                Build_file_list(entries);
                archive_path = path;
                folderBrowserDialog1.SelectedPath = Path.GetDirectoryName(path);
                exportArchiveToolStripMenuItem.Enabled = closeArchiveToolStripMenuItem.Enabled = true;
            }
            else
            {
                openFileDialog1.FileName = null;
                MessageBox.Show(this,
                    string.Format(GetResString("strMessageBoxOpenFailedContent", "The data format of {0} is not supported."), path),
                   GetResString("strMessageBoxOpenFailedTitle", "Open failed"),
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Export_archive()
        {
            if (parser == null
                || string.IsNullOrEmpty(archive_path)
                || FolderBrowserDialogHelper.ShowFolderBrowser(folderBrowserDialog1, this) != DialogResult.OK)
            { return; }
            parser.Export_archive(folderBrowserDialog1.SelectedPath);
        }

        private void Close_archive()
        {
            if (!string.IsNullOrEmpty(archive_path))
            {
                archive_path = string.Empty;
                explorer_view.Nodes.Clear();
                pic_preview.Cursor = Cursors.Default;
                pic_preview.Image = null;
                parser.Close_file();
                exportArchiveToolStripMenuItem.Enabled = closeArchiveToolStripMenuItem.Enabled = false;
            }
        }

        private void Build_file_list(List<Entry> entries)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                string[] array = entry.name.Split(new char[]
                {
                    Path.DirectorySeparatorChar
                });
                TreeNode node = Get_root(array[0]);
                Add_path(node, array, entry);
            }
        }

        private void Add_path(TreeNode node, string[] pathbits, Entry e)
        {
            for (int i = 1; i < pathbits.Length; i++)
            {
                node = Add_node(node, pathbits[i]);
            }
            node.Tag = e;
        }

        private TreeNode Get_root(string key)
        {
            return explorer_view.Nodes.ContainsKey(key) ? explorer_view.Nodes[key] : explorer_view.Nodes.Add(key, key);
        }

        private TreeNode Add_node(TreeNode node, string key)
        {
            int icon = Get_node_icon(key);
            return node.Nodes.ContainsKey(key) ? node.Nodes[key] : node.Nodes.Add(key, key, icon, icon);
        }

        private int Get_node_icon(string key)
        {
            int icon = 0;

            if (key.ExtensionContains(".rvdata"))
            { icon = 1; }
            else if (key.ExtensionContains(".png", ".jpg", "gif", "bmp", "ico"))
            { icon = 2; }
            else if (!string.IsNullOrWhiteSpace(Path.GetExtension(key)))
            { icon = 3; }

            return icon;
        }

        private void Show_image(Entry entry)
        {
            byte[] buffer = parser.Get_filedata(entry);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                Image image = Image.FromStream(stream);
                pic_preview.Image = image;
                pic_preview.Cursor = Cursors.SizeAll;
            }
        }

        private void Determine_action(Entry entry)
        {
            if (entry.name.ExtensionContains(".png", ".jpg", "gif", "bmp", "ico"))
            {
                Show_image(entry);
            }
            else
            {
                pic_preview.Image = null;
                pic_preview.Cursor = Cursors.Default;
            }
        }

        private void Explorer_view_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (explorer_view.SelectedNode == null || explorer_view.SelectedNode.Tag == null)
            {
                return;
            }
            Entry entry = (Entry)explorer_view.SelectedNode.Tag;
            Determine_action(entry);
        }

        private void Export_nodes(TreeNode node, string saveDir)
        {
            progressBar1.Maximum = node.Nodes.Count > 0 ? node.Nodes.Count : progressBar1.Maximum;
            progressBar1.Value = progressBar1.Value < progressBar1.Maximum ? progressBar1.Value : 0;
            if (node.Tag != null)
            {
                Entry e = (Entry)node.Tag;
                parser.Export_file(e, saveDir);
                progressBar1.Value++;
            }
            foreach (TreeNode treeNode in node.Nodes)
            {
                Export_nodes(treeNode, saveDir);
                if (treeNode.Tag != null)
                {
                    Entry e = (Entry)treeNode.Tag;
                    parser.Export_file(e, saveDir);
                }
            }
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (explorer_view.SelectedNode == null
                || FolderBrowserDialogHelper.ShowFolderBrowser(folderBrowserDialog1, this) != DialogResult.OK)
            { return; }
            progressBar1.Visible = true;
            Export_nodes(explorer_view.SelectedNode, folderBrowserDialog1.SelectedPath);
            progressBar1.Visible = false;
        }

        private void Explorer_view_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            explorer_view.SelectedNode = e.Node;
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            { openFileDialog1.FileName = null; return; }
            Read_archive(openFileDialog1.FileName);
        }

        private void ExportArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Export_archive();
        }

        private void CloseArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close_archive();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close_archive();
            Application.Exit();
        }

        private void Pic_preview_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage((sender as Control).Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Project1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/usaginya/RGSS-Extractor");
        }

        private void AuthorYIU2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/usaginya");
        }

        private void AuthorKatyushaScarletToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/KatyushaScarlet");
        }

        private string GetResString(string name, string defaultStr = "")
        {
            string resString = Extensions.ApplyResource(GetType(), name);
            return string.IsNullOrWhiteSpace(resString) ? defaultStr : resString;
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.explorer_view = new System.Windows.Forms.TreeView();
            this.explorer_menu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.pic_preview = new System.Windows.Forms.PictureBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportArchiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeArchiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.githubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.project1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.authorYIU2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.authorKatyushaScarletToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.explorer_menu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pic_preview)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.progressBar1);
            this.splitContainer1.Panel1.Controls.Add(this.explorer_view);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(235)))), ((int)(((byte)(220)))));
            this.splitContainer1.Panel2.Controls.Add(this.pic_preview);
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Step = 1;
            // 
            // explorer_view
            // 
            this.explorer_view.ContextMenuStrip = this.explorer_menu;
            resources.ApplyResources(this.explorer_view, "explorer_view");
            this.explorer_view.ImageList = this.imageList1;
            this.explorer_view.Name = "explorer_view";
            this.explorer_view.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.Explorer_view_AfterSelect);
            this.explorer_view.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.Explorer_view_NodeMouseClick);
            // 
            // explorer_menu
            // 
            this.explorer_menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToolStripMenuItem});
            this.explorer_menu.Name = "contextMenuStrip1";
            resources.ApplyResources(this.explorer_menu, "explorer_menu");
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            resources.ApplyResources(this.exportToolStripMenuItem, "exportToolStripMenuItem");
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.ExportToolStripMenuItem_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Dir.ico");
            this.imageList1.Images.SetKeyName(1, "Data.ico");
            this.imageList1.Images.SetKeyName(2, "Graphics.ico");
            this.imageList1.Images.SetKeyName(3, "unFile.ico");
            // 
            // pic_preview
            // 
            resources.ApplyResources(this.pic_preview, "pic_preview");
            this.pic_preview.Name = "pic_preview";
            this.pic_preview.TabStop = false;
            this.pic_preview.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Pic_preview_MouseDown);
            // 
            // openFileDialog1
            // 
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.githubToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.exportArchiveToolStripMenuItem,
            this.closeArchiveToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // exportArchiveToolStripMenuItem
            // 
            resources.ApplyResources(this.exportArchiveToolStripMenuItem, "exportArchiveToolStripMenuItem");
            this.exportArchiveToolStripMenuItem.Name = "exportArchiveToolStripMenuItem";
            this.exportArchiveToolStripMenuItem.Click += new System.EventHandler(this.ExportArchiveToolStripMenuItem_Click);
            // 
            // closeArchiveToolStripMenuItem
            // 
            resources.ApplyResources(this.closeArchiveToolStripMenuItem, "closeArchiveToolStripMenuItem");
            this.closeArchiveToolStripMenuItem.Name = "closeArchiveToolStripMenuItem";
            this.closeArchiveToolStripMenuItem.Click += new System.EventHandler(this.CloseArchiveToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // githubToolStripMenuItem
            // 
            this.githubToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.project1ToolStripMenuItem,
            this.authorYIU2ToolStripMenuItem,
            this.authorKatyushaScarletToolStripMenuItem});
            this.githubToolStripMenuItem.Name = "githubToolStripMenuItem";
            resources.ApplyResources(this.githubToolStripMenuItem, "githubToolStripMenuItem");
            // 
            // project1ToolStripMenuItem
            // 
            this.project1ToolStripMenuItem.Name = "project1ToolStripMenuItem";
            resources.ApplyResources(this.project1ToolStripMenuItem, "project1ToolStripMenuItem");
            this.project1ToolStripMenuItem.Click += new System.EventHandler(this.Project1ToolStripMenuItem_Click);
            // 
            // authorYIU2ToolStripMenuItem
            // 
            this.authorYIU2ToolStripMenuItem.Name = "authorYIU2ToolStripMenuItem";
            resources.ApplyResources(this.authorYIU2ToolStripMenuItem, "authorYIU2ToolStripMenuItem");
            this.authorYIU2ToolStripMenuItem.Click += new System.EventHandler(this.AuthorYIU2ToolStripMenuItem_Click);
            // 
            // authorKatyushaScarletToolStripMenuItem
            // 
            this.authorKatyushaScarletToolStripMenuItem.Name = "authorKatyushaScarletToolStripMenuItem";
            resources.ApplyResources(this.authorKatyushaScarletToolStripMenuItem, "authorKatyushaScarletToolStripMenuItem");
            this.authorKatyushaScarletToolStripMenuItem.Click += new System.EventHandler(this.AuthorKatyushaScarletToolStripMenuItem_Click);
            // 
            // folderBrowserDialog1
            // 
            resources.ApplyResources(this.folderBrowserDialog1, "folderBrowserDialog1");
            // 
            // Form1
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.explorer_menu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pic_preview)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

    }
}
