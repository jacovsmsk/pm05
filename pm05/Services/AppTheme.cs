using System.Drawing;
using System.Windows.Forms;

namespace pm05.Services
{
    public static class AppTheme
    {
        public static readonly Color Primary = Color.FromArgb(11, 95, 125);
        public static readonly Color PrimaryLight = Color.FromArgb(0, 142, 178);
        public static readonly Color Accent = Color.FromArgb(245, 158, 66);
        public static readonly Color Background = Color.FromArgb(236, 243, 247);
        public static readonly Color Card = Color.White;
        public static readonly Color TextOnPrimary = Color.White;
        public static readonly Color TextMuted = Color.FromArgb(90, 110, 120);

        public static Font TitleFont => new Font("Segoe UI", 14f, FontStyle.Bold);
        public static Font SubtitleFont => new Font("Segoe UI", 9f, FontStyle.Regular);
        public static Font MenuFont => new Font("Segoe UI", 9.5f, FontStyle.Regular);

        public static void StylePrimaryButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = PrimaryLight;
            button.ForeColor = TextOnPrimary;
            button.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }

        public static void StyleSecondaryButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = PrimaryLight;
            button.BackColor = Card;
            button.ForeColor = Primary;
            button.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }

        public static void StyleMenuButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = Card;
            button.ForeColor = Primary;
            button.Font = MenuFont;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(12, 0, 0, 0);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }

        public static void StyleLogoutButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = Color.FromArgb(220, 235, 242);
            button.ForeColor = Primary;
            button.Font = MenuFont;
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }

        public static void StyleDataGridView(DataGridView grid)
        {
            grid.BackgroundColor = Card;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Primary;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextOnPrimary;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(4);
            grid.ColumnHeadersHeight = 32;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(180, 220, 235);
            grid.DefaultCellStyle.SelectionForeColor = Primary;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 251, 253);
            grid.GridColor = Color.FromArgb(220, 230, 236);
            grid.RowTemplate.Height = 28;
        }

        public static void ApplyCrudForm(Form form, DataGridView grid)
        {
            form.BackColor = Background;
            form.Font = new Font("Segoe UI", 9f);
            if (grid != null)
                StyleDataGridView(grid);

            foreach (Control control in form.Controls)
            {
                if (control is Button btn && btn.Text != null)
                {
                    if (btn.Text.Contains("Удалить"))
                        continue;
                    if (btn.Text.Contains("Добавить") || btn.Text.Contains("Оформить") || btn.Text.Contains("Создать"))
                        StylePrimaryButton(btn);
                    else if (btn.Text.Contains("Изменить") || btn.Text.Contains("Сохранить"))
                        StyleSecondaryButton(btn);
                }
            }
        }

        public static Panel CreateHeaderPanel(string title, string subtitle = null)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = subtitle == null ? 52 : 64,
                BackColor = Primary
            };

            var lblTitle = new Label
            {
                Text = title,
                ForeColor = TextOnPrimary,
                Font = TitleFont,
                AutoSize = true,
                Location = new Point(14, subtitle == null ? 14 : 10)
            };
            panel.Controls.Add(lblTitle);

            if (!string.IsNullOrEmpty(subtitle))
            {
                panel.Controls.Add(new Label
                {
                    Text = subtitle,
                    ForeColor = Color.FromArgb(200, 225, 235),
                    Font = SubtitleFont,
                    AutoSize = true,
                    Location = new Point(16, 36)
                });
            }

            return panel;
        }
    }
}
