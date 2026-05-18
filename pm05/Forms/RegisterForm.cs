using System;
using System.Drawing;
using System.Windows.Forms;
using pm05.Services;

namespace pm05.Forms
{
    public class RegisterForm : Form
    {
        private const int MinPasswordLength = 6;

        private readonly AuthService _auth = new AuthService();
        private TextBox _txtFullName;
        private TextBox _txtLogin;
        private TextBox _txtPassword;
        private TextBox _txtConfirm;

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var lblFullName = new Label { Text = "ФИО", Location = new Point(12, 15), AutoSize = true };
            _txtFullName = new TextBox { Location = new Point(12, 32), Size = new Size(280, 20) };

            var lblLogin = new Label { Text = "Логин", Location = new Point(12, 58), AutoSize = true };
            _txtLogin = new TextBox { Location = new Point(12, 75), Size = new Size(280, 20) };

            var lblPassword = new Label
            {
                Text = $"Пароль (мин. {MinPasswordLength} символов)",
                Location = new Point(12, 101),
                AutoSize = true
            };
            _txtPassword = new TextBox
            {
                Location = new Point(12, 118),
                Size = new Size(280, 20),
                UseSystemPasswordChar = true
            };

            var lblConfirm = new Label { Text = "Подтвердите пароль", Location = new Point(12, 144), AutoSize = true };
            _txtConfirm = new TextBox
            {
                Location = new Point(12, 161),
                Size = new Size(280, 20),
                UseSystemPasswordChar = true
            };

            var btnRegister = new Button
            {
                Text = "Зарегистрироваться",
                Location = new Point(12, 198),
                Size = new Size(140, 28)
            };
            btnRegister.Click += BtnRegister_Click;

            var btnBack = new Button
            {
                Text = "К входу",
                Location = new Point(162, 198),
                Size = new Size(130, 28)
            };
            btnBack.Click += (_, __) => Close();

            Controls.AddRange(new Control[]
            {
                lblFullName, _txtFullName, lblLogin, _txtLogin,
                lblPassword, _txtPassword, lblConfirm, _txtConfirm,
                btnRegister, btnBack
            });

            Text = "Регистрация";
            ClientSize = new Size(310, 245);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            var fullName = _txtFullName.Text?.Trim() ?? "";
            var login = _txtLogin.Text?.Trim() ?? "";
            var password = _txtPassword.Text ?? "";
            var confirm = _txtConfirm.Text ?? "";

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Заполните ФИО и логин.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password.Length < MinPasswordLength)
            {
                MessageBox.Show($"Пароль должен быть не короче {MinPasswordLength} символов.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.Equals(password, confirm, StringComparison.Ordinal))
            {
                MessageBox.Show("Пароли не совпадают.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_auth.Register(login, password, fullName, RoleIds.User))
            {
                MessageBox.Show("Такой логин уже занят.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Регистрация успешна. Теперь можно войти.", "Готово",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
