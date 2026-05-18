using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Cryptography;

namespace pm05.Services
{
    public static class CaptchaService
    {
        private static readonly char[] _chars =
            "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789".ToCharArray();

        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public static string GenerateText(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

            var result = new char[length];
            var buffer = new byte[4];

            for (int i = 0; i < length; i++)
            {
                _rng.GetBytes(buffer);
                int value = BitConverter.ToInt32(buffer, 0) & 0x7FFFFFFF;
                result[i] = _chars[value % _chars.Length];
            }

            return new string(result);
        }

        public static Bitmap GenerateImage(string text, int width, int height)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.White);

                using (var brush = new LinearGradientBrush(new Rectangle(0, 0, width, height),
                    Color.FromArgb(232, 244, 250), Color.FromArgb(200, 225, 238), 45f))
                {
                    g.FillRectangle(brush, 0, 0, width, height);
                }

                var rnd = new Random(GetSeed());

                int lines = Math.Max(3, text.Length);
                for (int i = 0; i < lines; i++)
                {
                    var penColor = Color.FromArgb(rnd.Next(50, 150), rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256));
                    using (var pen = new Pen(penColor, rnd.Next(1, 3)))
                    {
                        g.DrawLine(pen, rnd.Next(width), rnd.Next(height), rnd.Next(width), rnd.Next(height));
                    }
                }

                float charArea = width / (float)text.Length;
                float fontSize = height * 0.65f;
                var fontFamilies = new[] { "Arial", "Tahoma", "Times New Roman", "Georgia" };

                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    float x = i * charArea + (charArea - fontSize * 0.6f) / 2f;
                    float y = (height - fontSize) / 2f + rnd.Next(-5, 6);
                    float angle = rnd.Next(-30, 31);

                    using (var f = new Font(fontFamilies[rnd.Next(fontFamilies.Length)], fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
                    {
                        var state = g.Save();
                        var cx = x + charArea / 2f;
                        var cy = y + fontSize / 2f;
                        g.TranslateTransform(cx, cy);
                        g.RotateTransform(angle);

                        var textColor = Color.FromArgb(rnd.Next(30, 180), rnd.Next(0, 150), rnd.Next(0, 150), rnd.Next(0, 150));
                        using (var textBrush = new SolidBrush(textColor))
                        {
                            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                            g.DrawString(c.ToString(), f, textBrush, 0, 0, sf);
                        }

                        g.Restore(state);
                    }
                }

                int dots = (width * height) / 100;
                for (int i = 0; i < dots; i++)
                {
                    bmp.SetPixel(rnd.Next(width), rnd.Next(height),
                        Color.FromArgb(rnd.Next(100, 255), rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256)));
                }
            }

            return bmp;
        }

        private static int GetSeed()
        {
            var buffer = new byte[4];
            _rng.GetBytes(buffer);
            return BitConverter.ToInt32(buffer, 0) & 0x7FFFFFFF;
        }
    }
}
