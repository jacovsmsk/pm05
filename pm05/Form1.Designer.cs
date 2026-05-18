namespace pm05
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlSide = new System.Windows.Forms.Panel();
            this.lblBrand = new System.Windows.Forms.Label();
            this.lblTagline = new System.Windows.Forms.Label();
            this.pnlCard = new System.Windows.Forms.Panel();
            this.lblFormTitle = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLogin = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.picCaptcha = new System.Windows.Forms.PictureBox();
            this.txtCaptchaInput = new System.Windows.Forms.TextBox();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.pnlSide.SuspendLayout();
            this.pnlCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCaptcha)).BeginInit();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSide
            // 
            this.pnlSide.Controls.Add(this.lblTagline);
            this.pnlSide.Controls.Add(this.lblBrand);
            this.pnlSide.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSide.Location = new System.Drawing.Point(0, 0);
            this.pnlSide.Name = "pnlSide";
            this.pnlSide.Size = new System.Drawing.Size(200, 400);
            this.pnlSide.TabIndex = 0;
            // 
            // lblBrand
            // 
            this.lblBrand.AutoSize = true;
            this.lblBrand.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblBrand.ForeColor = System.Drawing.Color.White;
            this.lblBrand.Location = new System.Drawing.Point(16, 48);
            this.lblBrand.MaximumSize = new System.Drawing.Size(170, 0);
            this.lblBrand.Name = "lblBrand";
            this.lblBrand.Size = new System.Drawing.Size(163, 50);
            this.lblBrand.TabIndex = 0;
            this.lblBrand.Text = "Туристическое\r\nагентство";
            // 
            // lblTagline
            // 
            this.lblTagline.AutoSize = true;
            this.lblTagline.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblTagline.ForeColor = System.Drawing.Color.FromArgb(200, 225, 235);
            this.lblTagline.Location = new System.Drawing.Point(18, 108);
            this.lblTagline.MaximumSize = new System.Drawing.Size(165, 0);
            this.lblTagline.Name = "lblTagline";
            this.lblTagline.Size = new System.Drawing.Size(155, 45);
            this.lblTagline.TabIndex = 1;
            this.lblTagline.Text = "Учёт туров, бронирований\r\nи оплат клиентов";
            // 
            // pnlCard
            // 
            this.pnlCard.Controls.Add(this.pnlButtons);
            this.pnlCard.Controls.Add(this.txtCaptchaInput);
            this.pnlCard.Controls.Add(this.picCaptcha);
            this.pnlCard.Controls.Add(this.txtPassword);
            this.pnlCard.Controls.Add(this.label2);
            this.pnlCard.Controls.Add(this.txtLogin);
            this.pnlCard.Controls.Add(this.label1);
            this.pnlCard.Controls.Add(this.lblFormTitle);
            this.pnlCard.Location = new System.Drawing.Point(228, 24);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(300, 352);
            this.pnlCard.TabIndex = 1;
            // 
            // lblFormTitle
            // 
            this.lblFormTitle.AutoSize = true;
            this.lblFormTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblFormTitle.ForeColor = System.Drawing.Color.FromArgb(11, 95, 125);
            this.lblFormTitle.Location = new System.Drawing.Point(20, 16);
            this.lblFormTitle.Name = "lblFormTitle";
            this.lblFormTitle.Size = new System.Drawing.Size(95, 20);
            this.lblFormTitle.TabIndex = 0;
            this.lblFormTitle.Text = "Вход в систему";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(90, 110, 120);
            this.label1.Location = new System.Drawing.Point(22, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Логин";
            // 
            // txtLogin
            // 
            this.txtLogin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLogin.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.txtLogin.Location = new System.Drawing.Point(22, 68);
            this.txtLogin.Name = "txtLogin";
            this.txtLogin.Size = new System.Drawing.Size(256, 25);
            this.txtLogin.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.FromArgb(90, 110, 120);
            this.label2.Location = new System.Drawing.Point(22, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Пароль";
            // 
            // txtPassword
            // 
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.txtPassword.Location = new System.Drawing.Point(22, 116);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(256, 25);
            this.txtPassword.TabIndex = 4;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // picCaptcha
            // 
            this.picCaptcha.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picCaptcha.Location = new System.Drawing.Point(22, 156);
            this.picCaptcha.Name = "picCaptcha";
            this.picCaptcha.Size = new System.Drawing.Size(256, 54);
            this.picCaptcha.TabIndex = 5;
            this.picCaptcha.TabStop = false;
            // 
            // txtCaptchaInput
            // 
            this.txtCaptchaInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCaptchaInput.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.txtCaptchaInput.Location = new System.Drawing.Point(22, 216);
            this.txtCaptchaInput.Name = "txtCaptchaInput";
            this.txtCaptchaInput.Size = new System.Drawing.Size(256, 25);
            this.txtCaptchaInput.TabIndex = 6;
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnRefresh);
            this.pnlButtons.Controls.Add(this.btnLogin);
            this.pnlButtons.Location = new System.Drawing.Point(22, 256);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(256, 36);
            this.pnlButtons.TabIndex = 7;
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(0, 0);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(120, 32);
            this.btnLogin.TabIndex = 0;
            this.btnLogin.Text = "Войти";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(128, 0);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(128, 32);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "Обновить CAPTCHA";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(236, 243, 247);
            this.ClientSize = new System.Drawing.Size(548, 400);
            this.Controls.Add(this.pnlCard);
            this.Controls.Add(this.pnlSide);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Туристическое агентство — вход";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.pnlSide.ResumeLayout(false);
            this.pnlSide.PerformLayout();
            this.pnlCard.ResumeLayout(false);
            this.pnlCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCaptcha)).EndInit();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlSide;
        private System.Windows.Forms.Label lblBrand;
        private System.Windows.Forms.Label lblTagline;
        private System.Windows.Forms.Panel pnlCard;
        private System.Windows.Forms.Label lblFormTitle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLogin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.PictureBox picCaptcha;
        private System.Windows.Forms.TextBox txtCaptchaInput;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnRefresh;
    }
}
