using System;
using System.Drawing;
using System.Windows.Forms;
using pm05.Models;
using pm05.Services;

namespace pm05.Forms
{
    public class MainForm : Form
    {
        private readonly User _currentUser;
        private Panel _pnlMenu;
        private Label _lblWelcome;
        private Button _btnCountries;
        private Button _btnTours;
        private Button _btnClients;
        private Button _btnBookings;
        private Button _btnPayments;
        private Button _btnUsers;
        private Button _btnLogs;

        public MainForm(User currentUser)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            InitializeComponent();
            Load += MainForm_Load;
        }

        private void InitializeComponent()
        {
            var header = AppTheme.CreateHeaderPanel("Туристическое агентство", "Система учёта туров и бронирований");

            _pnlMenu = new Panel
            {
                Location = new Point(12, header.Height + 8),
                Size = new Size(256, 320),
                BackColor = AppTheme.Background
            };

            _lblWelcome = new Label
            {
                AutoSize = false,
                Location = new Point(0, 0),
                Size = new Size(256, 44),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = AppTheme.Primary
            };

            _btnCountries = CreateMenuButton("🌍  Страны", 52, BtnCountries_Click);
            _btnTours = CreateMenuButton("✈  Туры", 92, BtnTours_Click);
            _btnClients = CreateMenuButton("👤  Клиенты", 132, BtnClients_Click);
            _btnBookings = CreateMenuButton("📋  Бронирования", 172, BtnBookings_Click);
            _btnPayments = CreateMenuButton("💳  Оплаты", 212, BtnPayments_Click);
            _btnUsers = CreateMenuButton("⚙  Пользователи", 262, BtnUsers_Click);
            _btnLogs = CreateMenuButton("📜  Журнал входов", 302, BtnLogs_Click);
            var btnLogout = CreateMenuButton("Выход из системы", 352, (_, __) => Close());
            AppTheme.StyleLogoutButton(btnLogout);
            btnLogout.Font = new Font("Segoe UI", 9f, FontStyle.Italic);

            _pnlMenu.Controls.AddRange(new Control[]
            {
                _lblWelcome, _btnCountries, _btnTours, _btnClients, _btnBookings,
                _btnPayments, _btnUsers, _btnLogs, btnLogout
            });

            Controls.Add(header);
            Controls.Add(_pnlMenu);

            BackColor = AppTheme.Background;
            Font = new Font("Segoe UI", 9f);
            Text = "Туристическое агентство";
            ClientSize = new Size(280, header.Height + 340);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
        }

        private static Button CreateMenuButton(string text, int top, EventHandler click)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(0, top),
                Size = new Size(256, 34)
            };
            AppTheme.StyleMenuButton(btn);
            btn.Click += click;
            return btn;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var roleName = RoleIds.GetName(_currentUser.RoleId);
            PermissionManager.SetRole(roleName);
            _lblWelcome.Text = $"{_currentUser.FullName}\r\n{roleName} · {_currentUser.Login}";

            var isAdmin = _currentUser.RoleId == RoleIds.Admin;
            var isOperator = _currentUser.RoleId == RoleIds.Operator;
            var isUser = _currentUser.RoleId == RoleIds.User;

            _btnUsers.Visible = isAdmin;
            _btnLogs.Visible = isAdmin;
            _btnClients.Visible = isAdmin || isOperator;
            _btnBookings.Visible = isAdmin || isOperator;
            _btnPayments.Visible = isAdmin || isOperator;
            _btnCountries.Visible = isAdmin || isOperator || isUser;
            _btnTours.Visible = isAdmin || isOperator || isUser;

            AdjustMenuLayout();
        }

        private void AdjustMenuLayout()
        {
            var top = 52;
            foreach (Control control in _pnlMenu.Controls)
            {
                if (control is Button btn && btn.Visible)
                {
                    btn.Top = top;
                    top += 40;
                }
            }
        }

        private void BtnCountries_Click(object sender, EventArgs e)
        {
            using (var form = new CountriesForm())
                form.ShowDialog(this);
        }

        private void BtnTours_Click(object sender, EventArgs e)
        {
            using (var form = new ToursForm())
                form.ShowDialog(this);
        }

        private void BtnClients_Click(object sender, EventArgs e)
        {
            using (var form = new ClientsForm())
                form.ShowDialog(this);
        }

        private void BtnBookings_Click(object sender, EventArgs e)
        {
            using (var form = new BookingsForm())
                form.ShowDialog(this);
        }

        private void BtnPayments_Click(object sender, EventArgs e)
        {
            using (var form = new PaymentsForm())
                form.ShowDialog(this);
        }

        private void BtnUsers_Click(object sender, EventArgs e)
        {
            using (var form = new UsersManageForm())
                form.ShowDialog(this);
        }

        private void BtnLogs_Click(object sender, EventArgs e)
        {
            using (var form = new LoginAttemptsForm())
                form.ShowDialog(this);
        }
    }
}
