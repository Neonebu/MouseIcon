using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseIcon
{
    public partial class Form1 : Form
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct CursorInfo
        {
            public int Size;
            public int Flags;
            public IntPtr Handle;
            public Point Position;
        }
        public class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern bool GetCursorInfo(out CursorInfo info);
        }
        public bool check = true;
        OpenFileDialog ofd;
        public SaveFileDialog sfd;
        Cursor cursor;
        Bitmap bitmap, bitmap2;
        CursorInfo info;
        StreamReader reader;
        String name = "icon";
        int count = 1;
        private IKeyboardMouseEvents m_GlobalHook;
        public Form1()
        {
            InitializeComponent();

        }
        public enum CompareResult
        {
            ciCompareOk,
            ciPixelMismatch,
            ciSizeMismatch
        };

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!bgWorker.CancellationPending)
            {
                while (check)
                {
                    info = new CursorInfo();
                    info.Size = Marshal.SizeOf(info.GetType());
                    if (NativeMethods.GetCursorInfo(out info))
                    {
                        // info.Handle contains the global cursor handle.
                        cursor = new Cursor(info.Handle);
                    }
                    bitmap = new Bitmap(cursor.Size.Width, cursor.Size.Height);
                    using (Graphics gBmp = Graphics.FromImage(bitmap))
                    {
                        cursor.Draw(gBmp, new Rectangle(0, 0, cursor.Size.Width, cursor.Size.Height));
                        pictureBox1.Invoke((MethodInvoker)delegate
                        {
                            pictureBox1.Image = bitmap;
                            pictureBox1.Refresh();
                            pictureBox1.Update();
                            panelLeft.Refresh();
                            panelLeft.Update();

                        });
                    }
                    Application.DoEvents();
                    Thread.Sleep(100);
                }
                Thread.Sleep(100);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bgWorker.RunWorkerAsync();
            Subscribe();
   
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            bgWorker.CancelAsync();
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            name += count.ToString();
            count++;
            pictureBox1.Image.Save(@"C:\Users\Public\Pictures\"+name+".png", ImageFormat.Png);
            name = "icon";
            
        }
        public void Subscribe()
        {
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress += GlobalHookKeyPress;
        }

        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            //Console.WriteLine("KeyPress: \t{0}", e.KeyChar);
        }

        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            //Console.WriteLine("MouseDown: \t{0}; \t System Timestamp: \t{1}", e.Button, e.Timestamp);
            // uncommenting the following line will suppress the middle mouse button click
            // if (e.Buttons == MouseButtons.Middle) { e.Handled = true; }
        }

        public void Unsubscribe()
        {
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress -= GlobalHookKeyPress;

            //It is recommened to dispose it
            m_GlobalHook.Dispose();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            var fileContent = String.Empty;
            var filePath = String.Empty;
            ofd = new OpenFileDialog();
            ofd.Title = "Open Icon";
            ofd.Filter = "PNG Image|*.png*";
            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filePath = ofd.FileName;
                var fileStream = ofd.OpenFile();
                using (reader = new StreamReader(fileStream))
                {
                    fileContent = reader.ReadToEnd();
                }
                bitmap2 = new Bitmap(filePath);
                pictureBox2.Image = bitmap2;
            }
        }

        private void buttonCompare_Click(object sender, EventArgs e)
        {
            if(pictureBox2.Image == null)
            {
                label3.Text = "No Image in picturebox.";
            }
            else
            {
                //Console.WriteLine(Compare(bitmap,bitmap2)); 
                if(Compare(bitmap,bitmap2) == CompareResult.ciCompareOk)
                {
                    label3.Text = "Same Icon";
                }
                else
                {
                    label3.Text = "Different Icon";
                }
            }
    }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.S && e.Shift && e.Control)
            {
                check = false;
                sfd = new SaveFileDialog();
                sfd.Title = "Save Icon";
                sfd.Filter = "PNG Image|*.png";
                sfd.ShowDialog();
                if (sfd.FileName != "")
                {
                    // Saves the Image via a FileStream created by the OpenFile method.
                    System.IO.FileStream fs =(System.IO.FileStream)sfd.OpenFile();
                    this.pictureBox1.Image.Save(fs,System.Drawing.Imaging.ImageFormat.Png);
                    fs.Close();
                
                }
                check = true;
            }
        }
        public static CompareResult Compare(Bitmap bmp1, Bitmap bmp2)
        {
            CompareResult cr = CompareResult.ciCompareOk;

            //Test to see if we have the same size of image
            if (bmp1.Size != bmp2.Size)
            {
                cr = CompareResult.ciSizeMismatch;
            }
            else
            {
                //Sizes are the same so start comparing pixels
                for (int x = 0; x < bmp1.Width
                     && cr == CompareResult.ciCompareOk; x++)
                {
                    for (int y = 0; y < bmp1.Height
                                 && cr == CompareResult.ciCompareOk; y++)
                    {
                        if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                            cr = CompareResult.ciPixelMismatch;
                    }
                }
            }
            return cr;
        }
    }
}
