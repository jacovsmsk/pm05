using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using pm05.Data;
using pm05.Models;
using pm05.Services;

namespace pm05.Forms
{
    public class BookingsForm : Form
    {
        private DataGridView _grid;
        private TextBox _txtSearch;
        private ComboBox _cmbStatus;
        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;

        public BookingsForm()
        {
            InitializeComponent();
            Load += (_, __) => Reload();
        }

        private void InitializeComponent()
        {
            _txtSearch = new TextBox { Location = new Point(12, 12), Size = new Size(160, 20) };
            _txtSearch.TextChanged += (_, __) => Reload();

            _cmbStatus = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(180, 12),
                Size = new Size(150, 21)
            };
            _cmbStatus.Items.AddRange(new object[] { "Все", BookingStatuses.AwaitingPayment, BookingStatuses.Paid, BookingStatuses.Cancelled });
            _cmbStatus.SelectedIndex = 0;
            _cmbStatus.SelectedIndexChanged += (_, __) => Reload();

            _btnAdd = new Button { Text = "Оформить", Location = new Point(340, 10), Size = new Size(90, 24) };
            _btnEdit = new Button { Text = "Изменить", Location = new Point(436, 10), Size = new Size(90, 24) };
            _btnDelete = new Button { Text = "Удалить", Location = new Point(532, 10), Size = new Size(90, 24) };

            _btnAdd.Click += (_, __) => OpenEditor(null);
            _btnEdit.Click += (_, __) => OpenSelected();
            _btnDelete.Click += (_, __) => DeleteSelected();

            _grid = new DataGridView
            {
                Location = new Point(12, 42),
                Size = new Size(610, 300),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _grid.CellFormatting += Grid_CellFormatting;

            Controls.AddRange(new Control[]
            {
                new Label { Text = "Поиск", Location = new Point(12, 0), AutoSize = true },
                _txtSearch, _cmbStatus, _btnAdd, _btnEdit, _btnDelete, _grid
            });

            Text = "Бронирования";
            ClientSize = new Size(640, 360);
            StartPosition = FormStartPosition.CenterParent;

            var canEdit = PermissionManager.CanEditData;
            _btnAdd.Enabled = canEdit;
            _btnEdit.Enabled = canEdit;
            _btnDelete.Enabled = canEdit;
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var statusCell = _grid.Rows[e.RowIndex].Cells["Status"];
            if (statusCell?.Value == null) return;

            var status = statusCell.Value.ToString();
            if (status == BookingStatuses.Paid)
                e.CellStyle.BackColor = Color.Honeydew;
            else if (status == BookingStatuses.AwaitingPayment)
                e.CellStyle.BackColor = Color.LemonChiffon;
            else if (status == BookingStatuses.Cancelled)
                e.CellStyle.BackColor = Color.Gainsboro;
        }

        private void Reload()
        {
            var search = _txtSearch.Text?.Trim() ?? "";
            var statusFilter = _cmbStatus.SelectedItem?.ToString() ?? "Все";

            using (var db = new ApplicationDbContext())
            {
                var query =
                    from b in db.Bookings
                    join c in db.Clients on b.ClientId equals c.Id
                    join t in db.Tours on b.TourId equals t.Id
                    select new
                    {
                        b.Id,
                        b.BookingDate,
                        Клиент = c.FullName,
                        Тур = t.Title,
                        Цена = t.Price,
                        b.Status
                    };

                if (statusFilter != "Все")
                    query = query.Where(x => x.Status == statusFilter);

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        x.Клиент.Contains(search) ||
                        x.Тур.Contains(search) ||
                        x.Status.Contains(search));
                }

                _grid.DataSource = query.OrderByDescending(x => x.BookingDate).ToList();
            }
        }

        private int? GetSelectedId()
        {
            if (_grid.CurrentRow == null) return null;
            return Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        }

        private void OpenSelected()
        {
            var id = GetSelectedId();
            if (!id.HasValue)
            {
                MessageBox.Show("Выберите бронь.", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            OpenEditor(id);
        }

        private void OpenEditor(int? id)
        {
            using (var form = new BookingEditForm(id))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                    Reload();
            }
        }

        private void DeleteSelected()
        {
            var id = GetSelectedId();
            if (!id.HasValue) return;

            if (MessageBox.Show("Удалить бронирование?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            using (var db = new ApplicationDbContext())
            {
                var payments = db.Payments.Where(p => p.BookingId == id.Value).ToList();
                var booking = db.Bookings.Find(id.Value);
                if (booking == null) return;

                var clientId = booking.ClientId;
                db.Payments.RemoveRange(payments);
                db.Bookings.Remove(booking);
                db.SaveChanges();

                var totalSpent = (
                    from p in db.Payments
                    join b in db.Bookings on p.BookingId equals b.Id
                    where b.ClientId == clientId
                    select p.Amount).DefaultIfEmpty(0m).Sum();

                var client = db.Clients.Find(clientId);
                if (client != null)
                {
                    client.TotalSpent = totalSpent;
                    db.SaveChanges();
                }
            }
            Reload();
        }
    }

    public class BookingEditForm : Form
    {
        private readonly int? _bookingId;
        private ComboBox _cmbClients;
        private ComboBox _cmbTours;
        private DateTimePicker _dtpDate;
        private ComboBox _cmbStatus;
        private Label _lblPrice;

        public BookingEditForm(int? bookingId = null)
        {
            _bookingId = bookingId;
            InitializeComponent();
            Load += BookingEditForm_Load;
        }

        private void InitializeComponent()
        {
            _cmbClients = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 28),
                Size = new Size(320, 21)
            };
            _cmbTours = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 68),
                Size = new Size(320, 21)
            };
            _cmbTours.SelectedIndexChanged += (_, __) => UpdatePriceLabel();

            _dtpDate = new DateTimePicker
            {
                Location = new Point(12, 108),
                Size = new Size(200, 20),
                Format = DateTimePickerFormat.Short
            };

            _cmbStatus = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 148),
                Size = new Size(200, 21)
            };
            _cmbStatus.Items.AddRange(new object[]
            {
                BookingStatuses.AwaitingPayment,
                BookingStatuses.Paid,
                BookingStatuses.Cancelled
            });

            _lblPrice = new Label
            {
                Location = new Point(12, 178),
                AutoSize = true,
                Text = "Стоимость тура: —"
            };

            var btnSave = new Button { Text = "Сохранить", Location = new Point(12, 210), Size = new Size(100, 28) };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(120, 210), Size = new Size(100, 28) };
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, __) => Close();

            Controls.AddRange(new Control[]
            {
                new Label { Text = "Клиент", Location = new Point(12, 12), AutoSize = true }, _cmbClients,
                new Label { Text = "Тур", Location = new Point(12, 52), AutoSize = true }, _cmbTours,
                new Label { Text = "Дата брони", Location = new Point(12, 92), AutoSize = true }, _dtpDate,
                new Label { Text = "Статус", Location = new Point(12, 132), AutoSize = true }, _cmbStatus,
                _lblPrice, btnSave, btnCancel
            });

            Text = _bookingId.HasValue ? "Редактирование брони" : "Новая бронь";
            ClientSize = new Size(350, 255);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            if (!PermissionManager.CanEditData)
            {
                btnSave.Enabled = false;
                _cmbClients.Enabled = false;
                _cmbTours.Enabled = false;
                _dtpDate.Enabled = false;
                _cmbStatus.Enabled = false;
            }
        }

        private void BookingEditForm_Load(object sender, EventArgs e)
        {
            using (var db = new ApplicationDbContext())
            {
                _cmbClients.DisplayMember = "FullName";
                _cmbClients.ValueMember = "Id";
                _cmbClients.DataSource = db.Clients.OrderBy(c => c.FullName).ToList();

                _cmbTours.DisplayMember = "Title";
                _cmbTours.ValueMember = "Id";
                _cmbTours.DataSource = db.Tours.Where(t => t.IsActive).OrderBy(t => t.Title).ToList();
            }

            if (_bookingId.HasValue)
            {
                using (var db = new ApplicationDbContext())
                {
                    var booking = db.Bookings.Find(_bookingId.Value);
                    if (booking == null) return;
                    _cmbClients.SelectedValue = booking.ClientId;
                    _cmbTours.SelectedValue = booking.TourId;
                    _dtpDate.Value = booking.BookingDate;
                    _cmbStatus.SelectedItem = booking.Status;
                }
            }
            else
            {
                _dtpDate.Value = DateTime.Now;
                _cmbStatus.SelectedItem = BookingStatuses.AwaitingPayment;
            }

            UpdatePriceLabel();
        }

        private void UpdatePriceLabel()
        {
            var tour = _cmbTours.SelectedItem as Tour;
            _lblPrice.Text = tour == null
                ? "Стоимость тура: —"
                : $"Стоимость тура: {tour.Price:N2} ₽";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_cmbClients.SelectedValue == null || _cmbTours.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента и тур.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var clientId = (int)_cmbClients.SelectedValue;
            var tourId = (int)_cmbTours.SelectedValue;
            var status = _cmbStatus.SelectedItem?.ToString() ?? BookingStatuses.AwaitingPayment;

            using (var db = new ApplicationDbContext())
            {
                if (_bookingId.HasValue)
                {
                    var booking = db.Bookings.Find(_bookingId.Value);
                    if (booking == null) return;
                    booking.ClientId = clientId;
                    booking.TourId = tourId;
                    booking.BookingDate = _dtpDate.Value;
                    booking.Status = status;
                }
                else
                {
                    db.Bookings.Add(new Booking
                    {
                        ClientId = clientId,
                        TourId = tourId,
                        BookingDate = _dtpDate.Value,
                        Status = status
                    });
                }
                db.SaveChanges();
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
