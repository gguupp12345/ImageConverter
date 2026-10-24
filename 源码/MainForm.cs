using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace 图片转换器
{
    public partial class MainForm : Form
    {
        private Bitmap _currentBitmap;
        private string _sourceFilePath;

        // 把控件都定义为全局变量，解决 "CS0103" 找不到名称的报错
        private Panel dropZone;
        private Label lblDropHint, lblFileName, lblStatus;
        private ComboBox cmbFormat;
        private Button btnConvert, btnSelectFile;

        public MainForm()
        {
            this.Text = "图片转换器";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(248, 249, 250);

            InitUI();
            SetupDragDrop();
        }

        private void InitUI()
        {
            Label lblTitle = new Label
            {
                Text = "图片转换器",
                Font = new Font("微软雅黑", 20, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            dropZone = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(440, 120),
                BackColor = Color.White
            };
            this.Controls.Add(dropZone);

            lblDropHint = new Label
            {
                Text = "📁 将图片拖拽到这里",
                Location = new Point(0, 40),
                Size = new Size(440, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(120, 120, 120),
                Font = new Font("微软雅黑", 10)
            };
            dropZone.Controls.Add(lblDropHint);

            lblFileName = new Label
            {
                Text = "",
                Location = new Point(0, 80),
                Size = new Size(440, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(30, 30, 30),
                Font = new Font("微软雅黑", 11, FontStyle.Bold),
                Visible = false
            };
            dropZone.Controls.Add(lblFileName);

            btnSelectFile = new Button
            {
                Text = "选择图片文件",
                Location = new Point(20, 210),
                Size = new Size(440, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 242, 245),
                Font = new Font("微软雅黑", 10)
            };
            btnSelectFile.Click += (s, e) => SelectFile();
            this.Controls.Add(btnSelectFile);

            Label lblFmt = new Label
            {
                Text = "目标格式：",
                Location = new Point(20, 265),
                AutoSize = true,
                Font = new Font("微软雅黑", 10)
            };
            this.Controls.Add(lblFmt);

            cmbFormat = new ComboBox
            {
                Location = new Point(100, 262),
                Size = new Size(100, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("微软雅黑", 10)
            };
            cmbFormat.Items.AddRange(new[] { ".jpg", ".png", ".bmp", ".gif", ".webp" });
            cmbFormat.SelectedIndex = 0;
            this.Controls.Add(cmbFormat);

            btnConvert = new Button
            {
                Text = "开始转换",
                Location = new Point(220, 260),
                Size = new Size(240, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                Enabled = false
            };
            btnConvert.Click += BtnConvert_Click;
            this.Controls.Add(btnConvert);

            lblStatus = new Label
            {
                Text = "就绪，请选择或拖入图片",
                Location = new Point(20, 315),
                AutoSize = true,
                ForeColor = Color.FromArgb(150, 150, 150),
                Font = new Font("微软雅黑", 9)
            };
            this.Controls.Add(lblStatus);
        }

        private void SetupDragDrop()
        {
            dropZone.AllowDrop = true;
            dropZone.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            };
            dropZone.DragDrop += (s, e) =>
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0) LoadImage(files[0]);
            };
        }

        private void SelectFile()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp";
                if (ofd.ShowDialog() == DialogResult.OK) LoadImage(ofd.FileName);
            }
        }

        private void LoadImage(string path)
        {
            try
            {
                _currentBitmap?.Dispose();
                _currentBitmap = new Bitmap(path);
                _sourceFilePath = path;

                lblFileName.Text = Path.GetFileName(path);
                lblFileName.Visible = true;
                lblDropHint.Visible = false;
                btnConvert.Enabled = true;
                lblStatus.Text = $"已加载: {Path.GetFileName(path)}";
                lblStatus.ForeColor = Color.Green;
            }
            catch
            {
                lblStatus.Text = "无法加载此图片";
                lblStatus.ForeColor = Color.Red;
                btnConvert.Enabled = false;
            }
        }

        private void BtnConvert_Click(object s, EventArgs e)
        {
            if (_currentBitmap == null || string.IsNullOrEmpty(_sourceFilePath)) return;

            string targetExt = cmbFormat.Text;
            string output = Path.ChangeExtension(_sourceFilePath, targetExt);
            output = GetUniqueFileName(output);

            try
            {
                ImageFormat format = targetExt.ToLower() switch
                {
                    ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                    ".png" => ImageFormat.Png,
                    ".bmp" => ImageFormat.Bmp,
                    ".gif" => ImageFormat.Gif,
                    ".webp" => ImageFormat.Png,
                    _ => ImageFormat.Jpeg
                };
                _currentBitmap.Save(output, format);

                lblStatus.Text = $"✅ 转换成功! 已保存为: {Path.GetFileName(output)}";
                lblStatus.ForeColor = Color.Green;
                MessageBox.Show($"转换完成！\n\n文件已保存至：\n{output}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ 转换失败";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"转换出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetUniqueFileName(string path)
        {
            if (!File.Exists(path)) return path;
            string dir = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);
            int count = 1;
            string newPath;
            do
            {
                newPath = Path.Combine(dir, $"{name}_{count}{ext}");
                count++;
            } while (File.Exists(newPath));
            return newPath;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _currentBitmap?.Dispose();
            base.OnFormClosing(e);
        }
    }
}