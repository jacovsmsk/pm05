using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using pm05.Data;
using pm05.Models;
using pm05.Services;

namespace pm05.Forms
{
    public class ToursForm : Form
    {
        private DataGridView _grid;
        private TextBox _txtSearch;
        private CheckBox _chkActiveOnly;
        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;

        public ToursForm()
        {
            InitializeComponent();
            Load += (_, __) => Reload();
        }

        private void InitializeComponent()
        {
            _txtSearch = new TextBox { Location = new Point(12, 12), Size = new Size(180, 20) };
            _txtSearch.TextChanged += (_, __) => Reload();

            _chkActiveOnly = new CheckBox
            {
                Text = "Только активные",
                Location = new Point(12, 36),
                AutoSize = true,
                Checked = true
            };
            _chkActiveOnly.CheckedChanged += (_, __) => Reload();

            _btnAdd = new Button { Text = "Добавить", Location = new Point(220, 10), Size = new Size(90, 24) };
            _btnEdit = new Button { Text = "Изменить", Location = new Point(316, 10), Size = new Size(90, 24) };
            _btnDelete = new Button { Text = "Удалить", Location = new Point(412, 10), Size = new Size(90, 24) };

            _btnAdd.Click += (_, __) => EditTour(null);
            _btnEdit.Click += (_, __) => EditSelected();
            _btnDelete.Click += (_, __) => DeleteSelected();

            _grid = new DataGridView
            {
                Location = new Point(12, 58),
                Size = new Size(560, 280),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Controls.AddRange(new Control[] { _txtSearch, _chkActiveOnly, _btnAdd, _btnEdit, _btnDelete, _grid });

            Text = "Туры";
            ClientSize = new Size(590, 355);
            StartPosition = FormStartPosition.CenterParent;

            var canEdit = PermissionManager.CanEditData;
            _btnAdd.Enabled = canEdit;
            _btnEdit.Enabled = canEdit;
            _btnDelete.Enabled = canEdit;
        }

        private void Reload()
        {
            var search = _txtSearch.Text?.Trim() ?? "";
            using (var db = new ApplicationDbContext())
            {
                var query =
                    from t in db.Tours
                    join c in db.Countries on t.CountryId equals c.Id
                    select new { t, CountryName = c.Name };

                if (_chkActiveOnly.Checked)
                    query = query.Where(x => x.t.IsActive);

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(x => x.t.Title.Contains(search));

                _grid.DataSource = query
                    .OrderBy(x => x.t.Title)
                    .Select(x => new
                    {
                        x.t.Id,
                        x.t.Title,
                        Страна = x.CountryName,
                        x.t.Price,
                        Активен = x.t.IsActive ? "Да" : "Нет"
                    })
                    .ToList();
            }
        }

        private Tour GetSelected()
        {
            if (_grid.CurrentRow == null) return null;
            var id = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
            using (var db = new ApplicationDbContext())
                return db.Tours.Find(id);
        }

        private void EditSelected()
        {
            var item = GetSelected();
            if (item == null)
            {
                MessageBox.Show("Выберите тур.", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            EditTour(item);
        }

        private void EditTour(Tour tour)
        {
            using (var dlg = new TourEditDialog(tour))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    Reload();
            }
        }

        private void DeleteSelected()
        {
            var item = GetSelected();
            if (item == null) return;

            if (MessageBox.Show($"Снять с продажи тур «{item.Title}»?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            using (var db = new ApplicationDbContext())
            {
                var entity = db.Tours.Find(item.Id);
                if (entity != null)
                {
                    entity.IsActive = false;
                    db.SaveChanges();
                }
            }
            Reload();
        }
    }

    internal class TourEditDialog : Form
    {
        private readonly Tour _tour;
        private TextBox _txtTitle;
        private ComboBox _cmbCountry;
        private NumericUpDown _numPrice;
        private CheckBox _chkActive;

        public TourEditDialog(Tour tour)
        {
            _tour = tour;
            InitializeComponent();
            Load += TourEditDialog_Load;
        }

        private void InitializeComponent()
        {
            _txtTitle = new TextBox { Location = new Point(12, 28), Size = new Size(320, 20) };
            _cmbCountry = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 68),
                Size = new Size(320, 21)
            };
            _numPrice = new NumericUpDown
            {
                Location = new Point(12, 108),
                Size = new Size(120, 20),
                DecimalPlaces = 2,
                Maximum = 10000000,
                Minimum = 0
            };
            _chkActive = new CheckBox
            {
                Text = "Активен",
                Location = new Point(12, 138),
                AutoSize = true,
                Checked = true
            };

            var btnOk = new Button { Text = "Сохранить", Location = new Point(12, 170), Size = new Size(100, 28) };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(120, 170), Size = new Size(100, 28) };
            btnOk.Click += BtnOk_Click;
            btnCancel.Click += (_, __) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[]
            {
                new Label { Text = "Название", Location = new Point(12, 12), AutoSize = true }, _txtTitle,
                new Label { Text = "Страна", Location = new Point(12, 52), AutoSize = true }, _cmbCountry,
                new Label { Text = "Цена", Location = new Point(12, 92), AutoSize = true }, _numPrice,
                _chkActive, btnOk, btnCancel
            });

            Text = _tour == null ? "Новый тур" : "Редактирование тура";
            ClientSize = new Size(350, 215);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
        }

        private void TourEditDialog_Load(object sender, EventArgs e)
        {
            using (var db = new ApplicationDbContext())
            {
                _cmbCountry.DisplayMember = "Name";
                _cmbCountry.ValueMember = "Id";
                _cmbCountry.DataSource = db.Countries.OrderBy(c => c.Name).ToList();
            }

            if (_tour != null)
            {
                _txtTitle.Text = _tour.Title;
                _cmbCountry.SelectedValue = _tour.CountryId;
                _numPrice.Value = _tour.Price;
                _chkActive.Checked = _tour.IsActive;
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var title = _txtTitle.Text?.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Введите название тура.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_cmbCountry.SelectedValue == null)
            {
                MessageBox.Show("Выберите страну.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var countryId = (int)_cmbCountry.SelectedValue;

            using (var db = new ApplicationDbContext())
            {
                if (_tour == null)
                {
                    db.Tours.Add(new Tour
                    {
                        Title = title,
                        CountryId = countryId,
                        Price = _numPrice.Value,
                        IsActive = _chkActive.Checked
                    });
                }
                else
                {
                    var entity = db.Tours.Find(_tour.Id);
                    if (entity == null) return;
                    entity.Title = title;
                    entity.CountryId = countryId;
                    entity.Price = _numPrice.Value;
                    entity.IsActive = _chkActive.Checked;
                }
                db.SaveChanges();
            }

            DialogResult = DialogResult.OK;
        }
    }
}
