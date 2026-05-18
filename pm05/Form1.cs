using System;
using System.Drawing;
using System.Windows.Forms;
using pm05.Forms;
using pm05.Services;

namespace pm05
{
    public partial class Form1 : Form
    {
        private readonly AuthService _auth = new AuthService();
        private bool _captchaVisible;
        private string _currentCaptchaText;
        private System.Windows.Forms.Timer _lockoutTimer;
        private string _lockedLogin;
        private Label _lblLockout;
        private Button _btnRegister;

        public Form1()
        {
            InitializeComponent();

            _btnRegister = new Button
            {
                Text = "Регистрация",
                Location = new Point(348, 196),
                Size = new Size(90, 23)
            };
            _btnRegister.Click += BtnRegister_Click;
            Controls.Add(_btnRegister);

            _lblLockout = new Label
            {
                AutoSize = true,
                ForeColor = Color.DarkRed,
                Font = new Font(Font.FontFamily, 9f, FontStyle.Bold),
                Visible = false
            };
            Controls.Add(_lblLockout);
            PositionLockoutLabel();
        }

        private void PositionLockoutLabel()
        {
            _lblLockout.Location = new Point(btnLogin.Left, btnLogin.Bottom + 6);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtCaptchaInput.Visible = false;
            picCaptcha.Visible = false;

            txtLogin.TextChanged += TxtLogin_TextChanged;
            txtLogin.Enter += TxtLogin_Enter;

            _lockoutTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _lockoutTimer.Tick += LockoutTimer_Tick;
            SyncLockoutUiWithCurrentLogin();
        }

        private void TxtLogin_Enter(object sender, EventArgs e)
        {
            if (txtLogin.Text == "Вводить сюда")
            {
                txtLogin.Text = string.Empty;
                txtLogin.ForeColor = SystemColors.WindowText;
            }
        }

        private void TxtLogin_TextChanged(object sender, EventArgs e)
        {
            SyncLockoutUiWithCurrentLogin();
        }

        private string GetLoginFromField()
        {
            var text = txtLogin.Text?.Trim() ?? string.Empty;
            if (string.Equals(text, "Вводить сюда", StringComparison.OrdinalIgnoreCase))
                return string.Empty;
            return text;
        }

        private void SyncLockoutUiWithCurrentLogin()
        {
            var login = GetLoginFromField();
            if (string.IsNullOrEmpty(login))
            {
                ClearLockoutUi();
                return;
            }

            if (_auth.IsLockedOut(login))
                ApplyLockoutUi(login);
            else
                ClearLockoutUi();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var login = GetLoginFromField();
            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_auth.IsLockedOut(login))
            {
                ApplyLockoutUi(login);
                MessageBox.Show(
                    $"Учётная запись «{login}» заблокирована.\n{_lblLockout.Text}",
                    "Блокировка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (_captchaVisible)
            {
                var input = txtCaptchaInput.Text ?? string.Empty;
                if (!string.Equals(input, _currentCaptchaText, StringComparison.OrdinalIgnoreCase))
                {
                    _auth.RecordLoginAttempt(login, false);
                    GenerateAndShowCaptcha();
                    MessageBox.Show("Неверная CAPTCHA", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    SyncLockoutUiWithCurrentLogin();
                    return;
                }
            }

            var password = txtPassword.Text ?? string.Empty;
            var result = _auth.Login(login, password);

            if (result == LoginResult.Success)
            {
                var user = _auth.GetUserByLogin(login);
                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден в базе.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ClearLockoutUi();

                var main = new MainForm(user);
                main.FormClosed += MainForm_FormClosed;
                main.Show();
                Hide();
                return;
            }

            if (result == LoginResult.LockedOut)
            {
                ApplyLockoutUi(login);
                MessageBox.Show(
                    $"Учётная запись «{login}» заблокирована.\n{_lblLockout.Text}",
                    "Блокировка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            if (!_captchaVisible)
            {
                _captchaVisible = true;
                txtCaptchaInput.Visible = true;
                picCaptcha.Visible = true;
            }

            GenerateAndShowCaptcha();
            SyncLockoutUiWithCurrentLogin();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            txtPassword.Clear();
            txtCaptchaInput.Clear();
            _captchaVisible = false;
            txtCaptchaInput.Visible = false;
            picCaptcha.Visible = false;
            Show();
            SyncLockoutUiWithCurrentLogin();
        }

        private void ApplyLockoutUi(string login)
        {
            _lockedLogin = login;
            btnLogin.Enabled = false;
            _lblLockout.Visible = true;
            PositionLockoutLabel();

            if (!_lockoutTimer.Enabled)
                _lockoutTimer.Start();

            UpdateLockoutDisplay();
        }

        private void ClearLockoutUi()
        {
            _lockedLogin = null;
            _lockoutTimer?.Stop();
            btnLogin.Enabled = true;
            btnLogin.Text = "Войти";
            _lblLockout.Visible = false;
            _lblLockout.Text = string.Empty;
        }

        private void UpdateLockoutDisplay()
        {
            if (string.IsNullOrEmpty(_lockedLogin))
            {
                ClearLockoutUi();
                return;
            }

            var remaining = _auth.GetLockoutRemaining(_lockedLogin);
            var totalSeconds = Math.Max(0, (int)Math.Ceiling(remaining.TotalSeconds));

            if (totalSeconds <= 0)
            {
                ClearLockoutUi();
                return;
            }

            if (totalSeconds >= 60)
                _lblLockout.Text = $"Повторите вход через {totalSeconds / 60}:{totalSeconds % 60:D2}";
            else
                _lblLockout.Text = $"Повторите вход через {totalSeconds} сек.";

            btnLogin.Text = "Заблокировано";
        }

        private void GenerateAndShowCaptcha()
        {
            try
            {
                if (picCaptcha.Image != null)
                {
                    var old = picCaptcha.Image;
                    picCaptcha.Image = null;
                    old.Dispose();
                }
            }
            catch { }

            _currentCaptchaText = CaptchaService.GenerateText(5);
            var bmp = CaptchaService.GenerateImage(_currentCaptchaText,
                picCaptcha.Width > 0 ? picCaptcha.Width : 200,
                picCaptcha.Height > 0 ? picCaptcha.Height : 60);
            picCaptcha.Image = bmp;
            txtCaptchaInput.Text = string.Empty;
        }

        private void LockoutTimer_Tick(object sender, EventArgs e)
        {
            var login = GetLoginFromField();
            if (!string.IsNullOrEmpty(login) && !string.Equals(login, _lockedLogin, StringComparison.OrdinalIgnoreCase))
            {
                SyncLockoutUiWithCurrentLogin();
                return;
            }

            if (string.IsNullOrEmpty(_lockedLogin))
            {
                ClearLockoutUi();
                return;
            }

            if (!_auth.IsLockedOut(_lockedLogin))
            {
                ClearLockoutUi();
                return;
            }

            UpdateLockoutDisplay();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            GenerateAndShowCaptcha();
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            using (var form = new RegisterForm())
            {
                form.ShowDialog(this);
            }
        }

        private void label1_Click(object sender, EventArgs e) { }

        private void label2_Click(object sender, EventArgs e) { }
    }
}
