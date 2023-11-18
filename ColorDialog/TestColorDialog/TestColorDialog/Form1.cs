using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomColorDialog;

namespace TestColorDialog
{
    public partial class Form1 : Form
    {
        private PictureBox PicBox;
        private DiktorColorDialog ColorDlg;


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeConsole();


        public Form1()
        {
            InitializeComponent();
            PicBox = new PictureBox();
            ColorDlg = null;
            PicBox.BackColor = Color.Blue;
            PicBox.Click += PicBox_Click;
            Controls.Add(PicBox);
        }
        void Form1_Load(object sender, EventArgs e)
        {
            //AllocConsole();
        }


        private void PicBox_Click(object sender, EventArgs e)
        {
            /*ColorDlg = new MyColorDialog();
            ColorDlg.Color = PicBox.BackColor;
            if (ColorDlg.ShowDialog() == DialogResult.OK)
            {
                PicBox.BackColor = ColorDlg.Color;
            }
            ColorDlg = null;*/

            DiktorColorDialog colorDialog1 = new DiktorColorDialog();
            colorDialog1.CurrentColorEvent += HandleColorChange;
            //colorDialog1.ShowDialog();
            colorDialog1.ShowDialogAsync();

            /*ShowCustomAsync(colorDialog1);
            while (true)
            {
                //if (colorDialog.ShowDialog() == DialogResult.OK) { break; }
                Console.WriteLine($"R:{colorDialog1.CurrentColor.R} G:{colorDialog1.CurrentColor.G} B:{colorDialog1.CurrentColor.B}");
                System.Threading.Thread.Sleep(1000);
            }*/
        }
        void HandleColorChange(Color color)
        {
            PicBox.BackColor = color;
        }


        async void ShowAsync(ColorDialog colorDialog)
        {
            await Task.Run(() =>
            {
                colorDialog.ShowDialog();
            });
        }
        /*async void ShowCustomAsync(DiktorColorDialog colorDialog)
        {
            await Task.Run(() =>
            {
                colorDialog.ShowDialog();
            });
        }*/

        void button1_Click(object sender, EventArgs e)
        {
            /*//colorDialog.HelpRequest += ColorDialog_ColorChanged;
            colorDialog1.FullOpen = true;
            ShowAsync(colorDialog1);

            while (true)
            {
                //if (colorDialog.ShowDialog() == DialogResult.OK) { break; }
                Console.WriteLine($"R:{colorDialog1.Color.R} G:{colorDialog1.Color.G} B:{colorDialog1.Color.B}");
                System.Threading.Thread.Sleep(1000);
            }*/
        }
    }

}