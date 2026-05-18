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
            _lblWelcome = new Label
            {
                AutoSize = true,
                Location = new Point(12, 12),
                Font = new Font(Font.FontFamily, 10f, FontStyle.Bold)
            };

            _btnCountries = CreateMenuButton("Страны", 48, BtnCountries_Click);
            _btnTours = CreateMenuButton("Туры", 88, BtnTours_Click);
            _btnClients = CreateMenuButton("Клиенты", 128, BtnClients_Click);
            _btnBookings = CreateMenuButton("Бронирования", 168, BtnBookings_Click);
            _btnPayments = CreateMenuButton("Оплаты", 208, BtnPayments_Click);
            _btnUsers = CreateMenuButton("Пользователи", 260, BtnUsers_Click);
            _btnLogs = CreateMenuButton("Журнал входов", 300, BtnLogs_Click);
            var btnLogout = CreateMenuButton("Выход", 348, (_, __) => Close());

            Controls.AddRange(new Control[]
            {
                _lblWelcome, _btnCountries, _btnTours, _btnClients, _btnBookings,
                _btnPayments, _btnUsers, _btnLogs, btnLogout
            });

            Text = "Туристическое агентство — главное меню";
            ClientSize = new Size(240, 390);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
        }

        private static Button CreateMenuButton(string text, int top, EventHandler click)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(12, top),
                Size = new Size(210, 32)
            };
            btn.Click += click;
            return btn;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var roleName = RoleIds.GetName(_currentUser.RoleId);
            PermissionManager.SetRole(roleName);
            _lblWelcome.Text = $"{_currentUser.FullName}\r\n({_currentUser.Login}, {roleName})";

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
