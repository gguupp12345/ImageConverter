using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using MailKit.Net.Smtp;
using MimeKit;

namespace 图片转换器
{
    public class User
    {
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public partial class LoginForm : Form
    {
        private string _systemEmail = "wangwang3538453@qq.com";
        private string _systemAuthCode = "pbrqnfyxzvgfcjcj";

        private List<User> _users = new List<User>();
        private string _userFile = "users.json";

        private string _currentCode = "";
        private DateTime _codeSendTime;

        // 两个独立面板，登录和注册完全分开
        private Panel loginPanel;
        private Panel registerPanel;

        public LoginForm()
        {
            LoadUsers();
            this.Size = new Size(420, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            BuildLoginPanel();
            BuildRegisterPanel();

            loginPanel.Visible = true;
            registerPanel.Visible = false;
        }

        private void LoadUsers()
        {
            if (File.Exists(_userFile))
            {
                try
                {
                    string json = File.ReadAllText(_userFile);
                    _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                }
                catch { _users = new List<User>(); }
            }
        }

        private void SaveUsers()
        {
            // 保存时强制去除首尾空格，避免以后登录对不上
            foreach (var u in _users)
            {
                u.Email = u.Email.Trim();
                u.Username = u.Username.Trim();
                u.Password = u.Password.Trim();
            }
            File.WriteAllText(_userFile, JsonSerializer.Serialize(_users));
        }

        // ================== 登录面板 ==================
        private void BuildLoginPanel()
        {
            loginPanel = new Panel { Location = new Point(0, 0), Size = new Size(420, 520), BackColor = Color.White };

            Label lblTitle = new Label { Text = "欢迎回来", Font = new Font("微软雅黑", 20, FontStyle.Bold), Location = new Point(40, 40), AutoSize = true };
            loginPanel.Controls.Add(lblTitle);

            Label lblEmail = new Label { Text = "邮箱", Location = new Point(40, 100), AutoSize = true, ForeColor = Color.Gray };
            TextBox txtLoginEmail = new TextBox { Location = new Point(40, 125), Width = 330, Font = new Font("微软雅黑", 11) };
            loginPanel.Controls.Add(lblEmail); loginPanel.Controls.Add(txtLoginEmail);

            Label lblPass = new Label { Text = "密码", Location = new Point(40, 170), AutoSize = true, ForeColor = Color.Gray };
            TextBox txtLoginPass = new TextBox { Location = new Point(40, 195), Width = 330, Font = new Font("微软雅黑", 11), PasswordChar = '*' };
            loginPanel.Controls.Add(lblPass); loginPanel.Controls.Add(txtLoginPass);

            Button btnLogin = new Button
            {
                Text = "登 录",
                Location = new Point(40, 260),
                Size = new Size(330, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 14, FontStyle.Bold)
            };
            btnLogin.Click += (s, e) => HandleLogin(txtLoginEmail.Text, txtLoginPass.Text);
            loginPanel.Controls.Add(btnLogin);

            Label lblSwitch = new Label { Text = "没有账号？去注册", Location = new Point(130, 320), AutoSize = true, ForeColor = Color.Gray, Cursor = Cursors.Hand };
            lblSwitch.Click += (s, e) => { loginPanel.Visible = false; registerPanel.Visible = true; };
            loginPanel.Controls.Add(lblSwitch);

            Label lblTip = new Label { Text = "登录只需输入邮箱和密码", Location = new Point(40, 360), AutoSize = true, ForeColor = Color.Gray };
            loginPanel.Controls.Add(lblTip);

            this.Controls.Add(loginPanel);
        }

        // ================== 注册面板 ==================
        private void BuildRegisterPanel()
        {
            registerPanel = new Panel { Location = new Point(0, 0), Size = new Size(420, 520), BackColor = Color.White, Visible = false };

            Label lblTitle = new Label { Text = "注册账号", Font = new Font("微软雅黑", 20, FontStyle.Bold), Location = new Point(40, 40), AutoSize = true };
            registerPanel.Controls.Add(lblTitle);

            Label lblEmail = new Label { Text = "邮箱", Location = new Point(40, 100), AutoSize = true, ForeColor = Color.Gray };
            TextBox txtRegEmail = new TextBox { Location = new Point(40, 125), Width = 330, Font = new Font("微软雅黑", 11) };
            registerPanel.Controls.Add(lblEmail); registerPanel.Controls.Add(txtRegEmail);

            Label lblUser = new Label { Text = "用户名", Location = new Point(40, 170), AutoSize = true, ForeColor = Color.Gray };
            TextBox txtRegUser = new TextBox { Location = new Point(40, 195), Width = 330, Font = new Font("微软雅黑", 11) };
            registerPanel.Controls.Add(lblUser); registerPanel.Controls.Add(txtRegUser);

            Label lblPass = new Label { Text = "密码", Location = new Point(40, 240), AutoSize = true, ForeColor = Color.Gray };
            TextBox txtRegPass = new TextBox { Location = new Point(40, 265), Width = 330, Font = new Font("微软雅黑", 11), PasswordChar = '*' };
            registerPanel.Controls.Add(lblPass); registerPanel.Controls.Add(txtRegPass);

            Label lblCode = new Label { Text = "验证码", Location = new Point(40, 310), AutoSize = true, ForeColor = Color.Gray };
            TextBox txtRegCode = new TextBox { Location = new Point(40, 335), Width = 190, Font = new Font("微软雅黑", 11) };
            registerPanel.Controls.Add(lblCode); registerPanel.Controls.Add(txtRegCode);

            Button btnSendCode = new Button { Text = "发送验证码", Location = new Point(240, 333), Size = new Size(130, 32), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(230, 230, 230) };
            btnSendCode.Click += (s, e) => HandleSendCode(txtRegEmail.Text);
            registerPanel.Controls.Add(btnSendCode);

            Button btnRegister = new Button
            {
                Text = "注 册",
                Location = new Point(40, 390),
                Size = new Size(330, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 14, FontStyle.Bold)
            };
            btnRegister.Click += (s, e) => HandleRegister(txtRegEmail.Text, txtRegUser.Text, txtRegPass.Text, txtRegCode.Text);
            registerPanel.Controls.Add(btnRegister);

            Label lblSwitch = new Label { Text = "已有账号？去登录", Location = new Point(130, 450), AutoSize = true, ForeColor = Color.Gray, Cursor = Cursors.Hand };
            lblSwitch.Click += (s, e) => { loginPanel.Visible = true; registerPanel.Visible = false; };
            registerPanel.Controls.Add(lblSwitch);

            this.Controls.Add(registerPanel);
        }

        // ================== 发送验证码 ==================
        private void HandleSendCode(string email)
        {
            email = email.Trim();
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                MessageBox.Show("邮箱格式不对，请重新输入", "提示");
                return;
            }

            _currentCode = new Random().Next(100000, 999999).ToString();
            _codeSendTime = DateTime.Now;

            try
            {
                var msg = new MimeMessage();
                msg.From.Add(new MailboxAddress("大狗叫工作室", _systemEmail));
                msg.To.Add(new MailboxAddress("", email));
                msg.Subject = "【大狗叫图片转换器】注册验证码";
                msg.Body = new TextPart("html")
                {
                    Text = $@"
                        <div style='font-family:微软雅黑;'>
                            <h3 style='color:#0066cc;'>大狗叫图片转换器</h3>
                            <p>您的注册验证码为：</p>
                            <div style='font-size:32px;color:#3333ff;font-weight:bold;'>{_currentCode}</div>
                            <p>5分钟内有效。</p>
                            <hr/>
                            <p style='font-size:12px;color:#999;'>此邮件由大狗叫软件工作室自动发送</p>
                        </div>"
                };

                using (var c = new SmtpClient())
                {
                    c.Connect("smtp.qq.com", 587, false);
                    c.Authenticate(_systemEmail, _systemAuthCode);
                    c.Send(msg);
                    c.Disconnect(true);
                }

                MessageBox.Show("验证码已发送", "成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发送失败：{ex.Message}", "错误");
            }
        }

        // ================== 登录逻辑（重点修改） ==================
        private void HandleLogin(string email, string pass)
        {
            // 关键：把用户输入的两个字符串去掉前后空格
            email = email.Trim();
            pass = pass.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("请输入邮箱和密码", "提示");
                return;
            }

            // 使用遍历对比，逻辑更清晰，便于排查
            User matched = null;
            foreach (var u in _users)
            {
                if (u.Email.Trim() == email && u.Password.Trim() == pass)
                {
                    matched = u;
                    break;
                }
            }

            if (matched == null)
            {
                // 如果登录失败，自动弹出“调试信息”，把本地存的账号密码全打印出来
                string debug = $"登录失败！\n\n";
                debug += $"你输入的邮箱：[{email}]\n";
                debug += $"你输入的密码：[{pass}]\n\n";
                debug += $"当前注册的账号数量：{_users.Count}\n";
                debug += "本地已存的账号：\n";
                foreach (var u in _users)
                {
                    debug += $"[{u.Email}] / [{u.Password}]\n";
                }
                MessageBox.Show(debug, "登录失败（调试信息）");
                return;
            }

            // 登录成功
            var main = new MainForm();
            main.Show();
            this.Hide();
        }

        // ================== 注册逻辑 ==================
        private void HandleRegister(string email, string user, string pass, string code)
        {
            email = email.Trim();
            user = user.Trim();
            pass = pass.Trim();
            code = code.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(code))
            {
                MessageBox.Show("请填写完整注册信息", "提示");
                return;
            }

            if (code != _currentCode || (DateTime.Now - _codeSendTime).TotalSeconds > 300)
            {
                MessageBox.Show("验证码无效或已过期", "提示");
                return;
            }

            if (_users.Exists(u => u.Email.Trim() == email))
            {
                MessageBox.Show("该邮箱已被注册", "提示");
                return;
            }

            _users.Add(new User { Email = email, Username = user, Password = pass });
            SaveUsers();

            MessageBox.Show("注册成功！", "成功");
            loginPanel.Visible = true;
            registerPanel.Visible = false;
        }
    }
}