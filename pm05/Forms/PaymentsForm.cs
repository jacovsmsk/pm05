using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using pm05.Data;
using pm05.Models;
using pm05.Services;

namespace pm05.Forms
{
    public class PaymentsForm : Form
    {
        private DataGridView _grid;
        private TextBox _txtSearch;
        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;

        public PaymentsForm()
        {
            InitializeComponent();
            Load += (_, __) => Reload();
        }

        private void InitializeComponent()
        {
            _txtSearch = new TextBox { Location = new Point(12, 12), Size = new Size(200, 20) };
            _txtSearch.TextChanged += (_, __) => Reload();

            _btnAdd = new Button { Text = "Добавить", Location = new Point(220, 10), Size = new Size(90, 24) };
            _btnEdit = new Button { Text = "Изменить", Location = new Point(316, 10), Size = new Size(90, 24) };
            _btnDelete = new Button { Text = "Удалить", Location = new Point(412, 10), Size = new Size(90, 24) };

            _btnAdd.Click += (_, __) => EditPayment(null);
            _btnEdit.Click += (_, __) => EditSelected();
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

            Controls.AddRange(new Control[] { _txtSearch, _btnAdd, _btnEdit, _btnDelete, _grid });

            Text = "Оплаты";
            ClientSize = new Size(640, 360);
            StartPosition = FormStartPosition.CenterParent;

            var canEdit = PermissionManager.CanEditData;
            _btnAdd.Enabled = canEdit;
            _btnEdit.Enabled = canEdit;
            _btnDelete.Enabled = canEdit;

            AppTheme.ApplyCrudForm(this, _grid);
        }

        private void Reload()
        {
            var search = _txtSearch.Text?.Trim() ?? "";
            using (var db = new ApplicationDbContext())
            {
                var query =
                    from p in db.Payments
                    join b in db.Bookings on p.BookingId equals b.Id
                    join c in db.Clients on b.ClientId equals c.Id
                    join t in db.Tours on b.TourId equals t.Id
                    select new
                    {
                        p.Id,
                        p.BookingId,
                        Клиент = c.FullName,
                        Тур = t.Title,
                        p.Amount,
                        p.PaymentDate
                    };

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        x.Клиент.Contains(search) ||
                        x.Тур.Contains(search));
                }

                _grid.DataSource = query.OrderByDescending(x => x.PaymentDate).ToList();
            }
        }

        private Payment GetSelected()
        {
            if (_grid.CurrentRow == null) return null;
            var id = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
            using (var db = new ApplicationDbContext())
                return db.Payments.Find(id);
        }

        private void EditSelected()
        {
            var item = GetSelected();
            if (item == null)
            {
                MessageBox.Show("Выберите оплату.", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            EditPayment(item);
        }

        private void EditPayment(Payment payment)
        {
            using (var dlg = new PaymentEditDialog(payment))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    Reload();
            }
        }

        private void DeleteSelected()
        {
            var item = GetSelected();
            if (item == null) return;

            if (MessageBox.Show("Удалить оплату?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var bookingId = item.BookingId;
            using (var db = new ApplicationDbContext())
            {
                var entity = db.Payments.Find(item.Id);
                if (entity != null)
                {
                    db.Payments.Remove(entity);
                    db.SaveChanges();
                }
            }

            BookingPaymentService.ApplyPaymentEffects(bookingId);
            Reload();
        }
    }

    internal class PaymentEditDialog : Form
    {
        private readonly Payment _payment;
        private ComboBox _cmbBooking;
        private NumericUpDown _numAmount;
        private DateTimePicker _dtpDate;
        private Label _lblTourPrice;

        public PaymentEditDialog(Payment payment)
        {
            _payment = payment;
            InitializeComponent();
            Load += PaymentEditDialog_Load;
        }

        private void InitializeComponent()
        {
            _cmbBooking = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 28),
                Size = new Size(360, 21)
            };
            _cmbBooking.SelectedIndexChanged += (_, __) => UpdatePriceHint();

            _numAmount = new NumericUpDown
            {
                Location = new Point(12, 68),
                Size = new Size(120, 20),
                DecimalPlaces = 2,
                Maximum = 10000000,
                Minimum = 0
            };

            _dtpDate = new DateTimePicker
            {
                Location = new Point(12, 108),
                Size = new Size(200, 20),
                Format = DateTimePickerFormat.Short
            };

            _lblTourPrice = new Label
            {
                Location = new Point(12, 138),
                AutoSize = true,
                Text = "Стоимость тура по брони: —"
            };

            var btnOk = new Button { Text = "Сохранить", Location = new Point(12, 170), Size = new Size(100, 28) };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(120, 170), Size = new Size(100, 28) };
            btnOk.Click += BtnOk_Click;
            btnCancel.Click += (_, __) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[]
            {
                new Label { Text = "Бронирование", Location = new Point(12, 12), AutoSize = true }, _cmbBooking,
                new Label { Text = "Сумма", Location = new Point(12, 52), AutoSize = true }, _numAmount,
                new Label { Text = "Дата оплаты", Location = new Point(12, 92), AutoSize = true }, _dtpDate,
                _lblTourPrice, btnOk, btnCancel
            });

            Text = _payment == null ? "Новая оплата" : "Редактирование оплаты";
            ClientSize = new Size(390, 215);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
        }

        private void PaymentEditDialog_Load(object sender, EventArgs e)
        {
            using (var db = new ApplicationDbContext())
            {
                var bookings = (
                    from b in db.Bookings
                    join c in db.Clients on b.ClientId equals c.Id
                    join t in db.Tours on b.TourId equals t.Id
                    orderby b.BookingDate descending
                    select new BookingListItem
                    {
                        Id = b.Id,
                        Display = $"#{b.Id} — {c.FullName} / {t.Title} ({b.Status})",
                        TourPrice = t.Price
                    }).ToList();

                _cmbBooking.DisplayMember = "Display";
                _cmbBooking.ValueMember = "Id";
                _cmbBooking.DataSource = bookings;
            }

            if (_payment != null)
            {
                _cmbBooking.SelectedValue = _payment.BookingId;
                _numAmount.Value = _payment.Amount;
                _dtpDate.Value = _payment.PaymentDate;
            }
            else
            {
                _dtpDate.Value = DateTime.Now;
            }

            UpdatePriceHint();
        }

        private void UpdatePriceHint()
        {
            var item = _cmbBooking.SelectedItem as BookingListItem;
            _lblTourPrice.Text = item == null
                ? "Стоимость тура по брони: —"
                : $"Стоимость тура по брони: {item.TourPrice:N2} ₽";
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (_cmbBooking.SelectedValue == null)
            {
                MessageBox.Show("Выберите бронирование.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_numAmount.Value <= 0)
            {
                MessageBox.Show("Введите сумму больше нуля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var bookingId = (int)_cmbBooking.SelectedValue;
            int? oldBookingId = _payment?.BookingId;

            using (var db = new ApplicationDbContext())
            {
                if (_payment == null)
                {
                    db.Payments.Add(new Payment
                    {
                        BookingId = bookingId,
                        Amount = _numAmount.Value,
                        PaymentDate = _dtpDate.Value
                    });
                }
                else
                {
                    var entity = db.Payments.Find(_payment.Id);
                    if (entity == null) return;
                    oldBookingId = entity.BookingId;
                    entity.BookingId = bookingId;
                    entity.Amount = _numAmount.Value;
                    entity.PaymentDate = _dtpDate.Value;
                }
                db.SaveChanges();
            }

            BookingPaymentService.ApplyPaymentEffects(bookingId);
            if (oldBookingId.HasValue && oldBookingId.Value != bookingId)
                BookingPaymentService.ApplyPaymentEffects(oldBookingId.Value);
            DialogResult = DialogResult.OK;
        }

        private class BookingListItem
        {
            public int Id { get; set; }
            public string Display { get; set; }
            public decimal TourPrice { get; set; }
        }
    }
}
