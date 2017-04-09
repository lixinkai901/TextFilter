using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;

namespace TextFilter
{
    public partial class MainForm : Form
    {
        const int WM_NCHITTEST = 0x0084;
        const int HT_LEFT = 10;
        const int HT_RIGHT = 11;
        const int HT_TOP = 12;
        const int HT_TOPLEFT = 13;
        const int HT_TOPRIGHT = 14;
        const int HT_BOTTOM = 15;
        const int HT_BOTTOMLEFT = 16;
        const int HT_BOTTOMRIGHT = 17;
        const int HT_CAPTION = 2;

        List<List<string>> content = null;
        HashSet<string> areaItem = null;
        HashSet<string> genderItem = null;

        public MainForm()
        {
            InitializeComponent();
            this.MinimumSize = new System.Drawing.Size(800,400);
        }

        public void SetWindowRegion()
        {
            System.Drawing.Drawing2D.GraphicsPath FormPath;
            FormPath = new System.Drawing.Drawing2D.GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            FormPath = GetRoundedRectPath(rect, 10);
            this.Region = new Region(FormPath);

        }
        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            int diameter = radius;
            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));
            GraphicsPath path = new GraphicsPath();

            // 左上角
            path.AddArc(arcRect, 180, 90);

            // 右上角
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);

            // 右下角
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);

            // 左下角
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);
            path.CloseFigure();//闭合曲线
            return path;
        }

        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void min_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void max_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.MaximumSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
                this.WindowState = FormWindowState.Maximized;
            }            
        }

        protected override void WndProc(ref Message Msg)
        {
            if (Msg.Msg == WM_NCHITTEST)
            {
                //获取鼠标位置
                int nPosX = (Msg.LParam.ToInt32() & 65535);
                int nPosY = (Msg.LParam.ToInt32() >> 16);
                //右下角
                if (nPosX >= this.Right - 6 && nPosY >= this.Bottom - 6)
                {
                    Msg.Result = new IntPtr(HT_BOTTOMRIGHT);
                    return;
                }
                //左上角
                else if (nPosX <= this.Left + 6 && nPosY <= this.Top + 6)
                {
                    Msg.Result = new IntPtr(HT_TOPLEFT);
                    return;
                }
                //左下角
                else if (nPosX <= this.Left + 6 && nPosY >= this.Bottom - 6)
                {
                    Msg.Result = new IntPtr(HT_BOTTOMLEFT);
                    return;
                }
                //右上角
                else if (nPosX >= this.Right - 6 && nPosY <= this.Top + 6)
                {
                    Msg.Result = new IntPtr(HT_TOPRIGHT);
                    return;
                }
                else if (nPosX >= this.Right - 2)
                {
                    Msg.Result = new IntPtr(HT_RIGHT);
                    return;
                }
                else if (nPosY >= this.Bottom - 2)
                {
                    Msg.Result = new IntPtr(HT_BOTTOM);
                    return;
                }
                else if (nPosX <= this.Left + 2)
                {
                    Msg.Result = new IntPtr(HT_LEFT);
                    return;
                }
                else if (nPosY <= this.Top + 2)
                {
                    Msg.Result = new IntPtr(HT_TOP);
                    return;
                }
                else
                {
                    Msg.Result = new IntPtr(HT_CAPTION);
                    return;
                }
            }
            base.WndProc(ref Msg);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            this.Refresh();
            SetWindowRegion();
        }

        private void Upload_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                try
                {
                    uploadDocument(fileName);
                    dataGridView1.Rows.Clear();
                    areaItem = new HashSet<string>();
                    areaItem.Add("");
                    genderItem = new HashSet<string>();
                    genderItem.Add("");
                    foreach (List<string> str in content)
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        row.CreateCells(dataGridView1);
                        int index = 0;
                        foreach (string s in str)
                        {
                            row.Cells[index].Value = s;
                            index++;
                        }
                        if(index >= 2)
                            areaItem.Add(str[2]);
                        if(index >= 4)
                            genderItem.Add(str[4]);
                        dataGridView1.Rows.Add(row);
                    }

                    foreach (string s in areaItem)
                    {
                        Area.Items.Add(s);
                    }
                    foreach (string s in genderItem)
                    {
                        Gender.Items.Add(s);
                    }
                }
                catch (Exception e1)
                {
                    MessageBox.Show("Upload Process error: " + e1.Message.ToString());
                }

            }
        }

        private void uploadDocument(string fileName)
        {
            Cursor.Hide();
            content = new List<List<string>>();
            foreach (string line in File.ReadLines(fileName))
            {
                string[] words = line.Split(',');
                List<string> word = new List<string>();
                foreach (string s in words)
                {
                    word.Add(s.Trim());
                }
                content.Add(word);
            }
            Cursor.Show();
        }

        private void Area_TextChanged(object sender, EventArgs e)
        {
            Cursor.Hide();
            try
            {
                string text = Area.Text;
                if (Gender.Text == "")
                {
                    if (text == "")
                    {
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            row.Visible = true;
                        }
                    }
                    else
                    {
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                if (row.Cells[2].Value.ToString().ToUpper() != text.ToUpper())
                                    row.Visible = false;
                                else
                                    row.Visible = true;
                            }

                        }
                    }
                }
                else
                {
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            if ((text == "" || row.Cells[2].Value.ToString().ToUpper() == text.ToUpper()) && row.Cells[4].Value.ToString().ToUpper() == Gender.Text.ToString().ToUpper() )
                                row.Visible = true;
                            else
                                row.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Cursor.Show();
                MessageBox.Show("Filter errors: " + ex.Message.ToString());
            }
            Cursor.Show();
        }

        private void Gender_TextChanged(object sender, EventArgs e)
        {
            Cursor.Hide();
            try
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        if ((Area.Text == "" || row.Cells[2].Value.ToString().ToUpper() == Area.Text.ToUpper()) && ((Gender.Text == "" || row.Cells[4].Value.ToString().ToUpper() == Gender.Text.ToString().ToUpper())))
                            row.Visible = true;
                        else
                            row.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Cursor.Show();
                MessageBox.Show(ex.Message.ToString());
            }
            Cursor.Show();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            string name = saveFileDialog1.FileName;
            saveContent(name);
        }

        private void saveContent(string fileName)
        {
            Cursor.Hide();
            var objWriter = new System.IO.StreamWriter(fileName);
            try
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow && row.Visible == true)
                    {
                        int colCount = row.Cells.Count;
                        for (int col = 0; col < colCount; col++)
                        {
                            objWriter.Write(row.Cells[col].Value.ToString());
                            if (col != colCount - 1)
                                objWriter.Write(", ");
                        }
                        objWriter.Write(Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                Cursor.Show();
                MessageBox.Show(ex.Message.ToString());
                objWriter.Close();
            }
            objWriter.Close();
            Cursor.Show();
        }

        private void Start_Click(object sender, EventArgs e)
        {
            Cursor.Hide();
            try
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        if ((Area.Text == "" || row.Cells[2].Value.ToString().ToUpper() == Area.Text.ToUpper()) && ((Gender.Text == "" || row.Cells[4].Value.ToString().ToUpper() == Gender.Text.ToString().ToUpper())))
                            row.Visible = true;
                        else
                            row.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Cursor.Show();
                MessageBox.Show(ex.Message.ToString());                
            }
            Cursor.Show();
        }
    }
}
