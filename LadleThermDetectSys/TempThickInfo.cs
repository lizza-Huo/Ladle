using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace LadleThermDetectSys
{
    public partial class TempThickInfo : Form
    {
        AutoResizeForm asc = new AutoResizeForm();
        MySqlConnection connMySql;
        //string myconnection = "user id=root;password=ARIMlab2020.07.22;server=localhost;persistsecurityinfo=True;database=ironldlthermodetectmanactrl;port=3306;SslMode=None;allowPublicKeyRetrieval=true";
        //string myconnection ="user id=tbrj;password=tbrj;server=10.99.24.144;persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true";
        string myconnection = "user id = test; password = test; server = 192.168.2.100; persistsecurityinfo = True; database = ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true"; 
        private string ladleno;
        private int ladleServDuty;
        private int ladleAge;

        private static int TempCirRows = 310, TempCirCols = 360;
        private static int TempBotRows = 190, TempBotCols = 190;
        double g_dwf, g_dhf, g_bwf, g_bhf;

        Int16[] dataCir = new Int16[TempCirRows * TempCirCols];
        Int16[] dataBot = new Int16[TempBotRows * TempBotCols];
        bool[] bDrawTemp = new bool[2] { false, false };
        int iDiv = 10;
        Color[] cr = new Color[] {Color.FromArgb(0, 0, 5),
         Color.FromArgb(0, 0, 37),
         Color.FromArgb(10, 0, 119),
         Color.FromArgb(34, 0, 123),
         Color.FromArgb(79, 0, 123),
         Color.FromArgb(84, 0, 133),
         Color.FromArgb(166, 0, 149),
         Color.FromArgb(172, 0, 150),
         Color.FromArgb(200, 15, 136),
         Color.FromArgb(227, 51, 91),
         Color.FromArgb(231, 58, 73),
         Color.FromArgb(241, 77, 24),
         Color.FromArgb(250, 93, 15),
         Color.FromArgb(255, 118, 15),
         Color.FromArgb(255, 127, 15),
         Color.FromArgb(255, 134, 15),
         Color.FromArgb(255, 144, 15),
         Color.FromArgb(255, 177, 15),
         Color.FromArgb(255, 218, 15),
         Color.FromArgb(255, 229, 37),
         Color.FromArgb(255, 239, 82),
         Color.FromArgb(255, 255, 165),
         Color.FromArgb(255, 255, 255)};

        private static int ThickCirRows = 360;
        private static int ThickCirCols = 720;
        private static int ThickBotRows = 380;
        private static int ThickBotCols = 380;
        Int16[] dataThickCir = new Int16[ThickCirRows * ThickCirCols];
        Int16[] dataThickBot = new Int16[ThickBotRows * ThickBotCols];

        bool[] bDrawThick = new bool[3] { false, false, false };
        
        int iDiv1 = 15;
        //色阶段
        Color[] cr_thick = new Color[] {
         Color.FromArgb(0, 0, 255),
         Color.FromArgb(0, 102, 255),
         Color.FromArgb(0, 204, 255),
         Color.FromArgb(0, 255, 204),
         Color.FromArgb(0, 255, 102),
         Color.FromArgb(0, 255, 0),
         Color.FromArgb(102, 255, 0),
         Color.FromArgb(204, 255, 0),
         Color.FromArgb(255, 204, 0),
         Color.FromArgb(255, 102, 0),
         Color.FromArgb(255, 0, 0),
         Color.Black};
        public TempThickInfo(string Ladleno,int LadleServDuty,int LadleAge)
        {
            InitializeComponent();
            ladleno = Ladleno;
            ladleServDuty = LadleServDuty;
            ladleAge = LadleAge;
        }

        private void TempThickInfo_Load(object sender, EventArgs e)
        {
            asc.controllInitializeSize(this);
            ShowAllPic();
        }

        private void TempThickInfo_SizeChanged(object sender, EventArgs e)
        {
            asc.controlAutoSize(this);
        }

        //显示测厚测温周壁数据

        void ShowAllPic()
        {
            ReadAllBlob();
        }
        Image image_rote;
        private void ReadAllBlob()
        {
            //相关数据显示
            connMySql = new MySqlConnection(myconnection);
            if (connMySql.State == ConnectionState.Closed)
            {
                connMySql.Open();
            }
            try
            {
                string SQL = "select LadleContractor, MeasTm,CircumExpandTemp,BottomExpandTemp from heavyldl_meased where LadleNo='" + ladleno + "' ORDER BY id DESC LIMIT 0,1;";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "heavyldl_meased");
                if (ds.Tables[0].Rows[0][2] != DBNull.Value)
                {
                    int inlenCir = TempCirRows * TempCirCols * 2;
                    byte[] bufferCir = new byte[inlenCir];
                    bufferCir = (byte[])ds.Tables[0].Rows[0][2];
                    for (int i = 0; i < inlenCir / 2; i++)
                    {
                        dataCir[i] = (Int16)((bufferCir[2 * i]) + (bufferCir[2 * i + 1]) * 256);
                        //获得字节数组
                    }
                    bDrawTemp[0] = true;
                }
                else
                {
                    bDrawTemp[0] = false;
                }
                if (ds.Tables[0].Rows[0][3] != DBNull.Value)
                {
                    int inlenBot = TempBotRows * TempBotCols * 2;
                    byte[] bufferBot = new byte[inlenBot];
                    bufferBot = (byte[])ds.Tables[0].Rows[0][3];
                    for (int i = 0; i < inlenBot / 2; i++)
                    {
                        dataBot[i] = (Int16)((bufferBot[2 * i]) + (bufferBot[2 * i + 1]) * 256);
                        //获得字节数组
                    }
                    bDrawTemp[1] = true;
                }
                else
                {
                    bDrawTemp[1] = false;
                }
                label3.Text = "" + ladleno + "铁包，" + ladleServDuty + "包役," + ladleAge + "包龄外壳温度相关数据";
                //数据表的测量时间降序排列取前10行，显示部分列到界面
                string SQL1 = "SELECT  LadleAge,CircumThick,BottomThick,ThickPhoto  FROM emptyldl_meased where LadleNo='" + ladleno + "' and LadleServDuty="+ladleServDuty+" and LadleAge<="+ladleAge+" order by id desc limit 0,1;";
                //MySqlCommand cmd = new MySqlCommand(SQL, connMySql);
                MySqlDataAdapter objDataAdpter1 = new MySqlDataAdapter();
                objDataAdpter1.SelectCommand = new MySqlCommand(SQL1, connMySql);
                DataSet ds1= new DataSet();
                objDataAdpter1.Fill(ds1, "emptyldl_meased");
                int rowsCount = ds1.Tables[0].Rows.Count;
                if (rowsCount > 0)
                {
                    int ladleAgeThick = Convert.ToInt32(ds1.Tables[0].Rows[0][0]);
                    if (ds1.Tables[0].Rows[0][1] != DBNull.Value)
                    {
                        int inlenThickCir = ThickCirRows * ThickCirCols * 2;
                        byte[] bufThickCir = new byte[inlenThickCir];
                        bufThickCir = (byte[])ds1.Tables[0].Rows[0][1];
                        for (int i = 0; i < inlenThickCir / 2; i++)
                        {
                            dataThickCir[i] = (Int16)((bufThickCir[2 * i]) + (bufThickCir[2 * i + 1]) * 256);
                        }
                        bDrawThick[0] = true;
                    }
                    else
                    {
                        bDrawThick[0] = false;
                    }
                    if (ds1.Tables[0].Rows[0][2]!=DBNull.Value)
                    {
                        int inlenThickBot = ThickBotRows * ThickBotCols * 2;
                        byte[] bufThickBot = new byte[inlenThickBot];
                        bufThickBot = (byte[])ds1.Tables[0].Rows[0][2];
                        for (int i = 0; i < inlenThickBot / 2; i++)
                        {
                            dataThickBot[i] = (Int16)((bufThickBot[2 * i]) + (bufThickBot[2 * i + 1]) * 256);
                        }
                        bDrawThick[1] = true;
                    }
                    else
                    {
                        bDrawThick[1] = false;
                    }
                    if (ds1.Tables[0].Rows[0][3] == DBNull.Value)
                    {
                        Image image = LadleThermDetectSys.Properties.Resources.内衬照片11;
                        picBoxBottomPic.Image = image;
                        image_rote = image;
                    }
                    else
                    {
                        byte[] Thick = (byte[])ds1.Tables[0].Rows[0][3];
                        MemoryStream stream = new MemoryStream(Thick, true);
                        Image image = Image.FromStream(stream, true);
                        stream.Close();
                        picBoxBottomPic.Image = image;
                        image_rote = image;
                    }
                    image_rote.RotateFlip(RotateFlipType.Rotate270FlipXY);
                    label1.Text = "已查询到有关" + ladleno + "包，" + ladleServDuty + "包役，相近" + ladleAgeThick + "包龄的测厚数据！";
                }
                else if (rowsCount == 0)
                {
                    label1.Text = "未查询到有关"+ladleno+"包，"+ladleServDuty+"包役，相近"+ladleAge+"包龄的测厚数据！";
                    groupBox2.Visible = false;
                    bDrawThick[0] = false; bDrawThick[1] = false; bDrawThick[2] = false;
                }
                
                connMySql.Close();
            }
            catch (Exception EE)
            {

                MessageBox.Show("获得blob数据块失败！" + EE.ToString());
            }
        }

        private void picBxCircumExpandTemp_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            if (bDrawTemp[0])
            {
                int m = TempCirRows, n = TempCirCols;

                Rectangle rt = e.ClipRectangle;
                double dwf = (double)(rt.Size.Width * 1.0f / n);
                double dhf = (double)(rt.Size.Height * 1.0f / m);

                g_dwf = dwf;
                g_dhf = dhf;
                SizeF Sf = new SizeF((float)dwf, (float)dhf);
                PointF PTf;

                RectangleF tmprtf;
                Brush bsh;

                double x, y;
                // Color color;
                //UInt16 a;
                int a;
                int b;
                int maxCir = -10000;
                int iMaxTempYbyPixelCir = 0;
                int iMaxTempXinPixelCir = 0;
                for (int X = 0; X < m; X++)  //  310行
                {
                    for (int Y = 0; Y < n; Y++)  //360列
                    {
                        //x = (double)(rt.Location.X + j * dwf);
                        //y = (double)(rt.Location.Y + i * dhf);
                        x = dwf * Y;
                        y = dhf * X;
                        PTf = new PointF((float)x, (float)y);
                        tmprtf = new RectangleF(PTf, Sf);
                        a = dataCir[Y * 310 + X];
                        if (maxCir < a)
                        {
                            maxCir = a;
                            iMaxTempYbyPixelCir = Y;
                            iMaxTempXinPixelCir = X;
                        }

                        b = (a - 30) / iDiv;
                        if (b <= 0)
                        {
                            b = 0;
                        }
                        else if (b > 21)
                        {
                            b = 22;
                        }
                        using (bsh = new SolidBrush(cr[b]))
                        {
                            g.FillRectangle(bsh, tmprtf);
                        }
                    }
                }
                Point LTPntofRt;
                Pen PenofRt;
                Size SzofRt;
                Rectangle Rt;
                float Penwid = (float)4.1;
                LTPntofRt = new Point((int)((iMaxTempYbyPixelCir) * dwf) - 10, (int)(iMaxTempXinPixelCir * dhf) - 10);
                SzofRt = new Size(20, 20);//定义画红框的大小，宽20高20
                Rt = new Rectangle(LTPntofRt, SzofRt);
                using (PenofRt = new Pen(Color.Green, Penwid))
                {
                    g.DrawRectangle(PenofRt, Rt);
                }
            }

        }

        private void picBxBottomExpandTemp_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            if (bDrawTemp[1])
            {
                int m = TempBotRows, n = TempBotCols;

                Rectangle rt = e.ClipRectangle;
                double dwf = (double)(rt.Size.Width * 1.0f / n);
                double dhf = (double)(rt.Size.Height * 1.0f / m);

                g_bwf = dwf;
                g_bhf = dhf;
                SizeF Sf = new SizeF((float)dwf, (float)dhf);
                PointF PTf;

                RectangleF tmprtf;
                Brush bsh;

                double x, y;
                // Color color;
                Int16 a;
                int b;
                int maxBot = -10000;
                int iMaxTempYbyPixelBot = 0;
                int iMaxTempXinPixelBot = 0;
                for (int X = 0; X < m; X++)  //  390行
                {
                    for (int Y = 0; Y < n; Y++)  //360列
                    {
                        x = dwf * Y;
                        y = dhf * (m - X - 1);
                        PTf = new PointF((float)x, (float)y);
                        tmprtf = new RectangleF(PTf, Sf);

                        a = dataBot[X * m + Y];
                        if (maxBot < a)
                        {
                            maxBot = a;
                            iMaxTempYbyPixelBot = Y;
                            iMaxTempXinPixelBot = X;
                        }
                        b = (a - 30) / iDiv;
                        if (b <= 0)
                        {
                            b = 0;
                        }
                        else if (b > 21)
                        {
                            b = 22;
                        }
                        using (bsh = new SolidBrush(cr[b]))
                        {
                            g.FillRectangle(bsh, tmprtf);
                        }

                    }
                }
                Point LTPntofRt;
                Pen PenofRt;
                Size SzofRt;
                Rectangle Rt;
                float Penwid = (float)4.1;
                LTPntofRt = new Point((int)((iMaxTempYbyPixelBot) * dwf) - 10, (int)(iMaxTempXinPixelBot * dhf) - 10);
                SzofRt = new Size(20, 20);//定义画红框的大小，宽20高20
                Rt = new Rectangle(LTPntofRt, SzofRt);
                using (PenofRt = new Pen(Color.Green, Penwid))
                {
                    g.DrawRectangle(PenofRt, Rt);
                }

            }

        }

        private void picBxCircumExpandTemp_MouseClick(object sender, MouseEventArgs e)
        {
            //鼠标点击该控件上绘制的图像出现坐标值            
            if (bDrawTemp[0])
            {
                Point p1 = MousePosition;
                Point p2 = picBxCircumExpandTemp.PointToClient(p1);

                double deltaX = (double)(picBxCircumExpandTemp.Width * 1.0f / 360);
                int X = (int)(Math.Floor(p2.X / g_dwf));
                int Y = (int)(Math.Floor(p2.Y / g_dhf));
                int X_p = (int)(Math.Floor(p2.X / deltaX));
                if ((0 <= X_p) && (X_p < 180))
                {
                    X_p = X_p - 180;
                }
                else if (X_p >= 180)
                {
                    X_p = X_p - 179;
                }
                String str = "温度：" + Convert.ToString(dataCir[X * 310 + Y] + "℃，当前深度为" + (Y - 1) * 19.3 + "mm-" + (Y * 19.3) + "mm，当前角度：" + X_p + "°");
                txtBxTempPosi.Text = str;
            }

        }


        //int col;

        private void picBxBottomExpandTemp_MouseClick(object sender, MouseEventArgs e)
        {
            Point p1 = MousePosition;
            Point p2 = picBxBottomExpandTemp.PointToClient(p1);

            int X = (int)(Math.Floor(p2.X / g_bwf));
            int Y = (int)(Math.Floor(p2.Y / g_bwf));
            double dg = (double)(360.0f / 360);  //角度间隔度数


            double minr = (double)(picBxBottomExpandTemp.Width);
            if (minr > (double)picBxBottomExpandTemp.Height)
                minr = (double)picBxBottomExpandTemp.Height;
            double dr = (double)((minr / 2) / (TempBotCols / 2)); //半径间隔大小


            int row = (int)Math.Floor((Math.Sqrt((p2.X - (picBxBottomExpandTemp.Width / 2.0)) * (p2.X - (picBxBottomExpandTemp.Width / 2.0)) + (p2.Y - (picBxBottomExpandTemp.Height / 2.0)) * (p2.Y - (picBxBottomExpandTemp.Height / 2.0)))) / dr);
            double x, y;
            y = p2.Y - picBxBottomExpandTemp.Height / 2.0;
            x = p2.X - picBxBottomExpandTemp.Width / 2.0;
            if (y >= 0)
            {
                double a = Math.Atan2(y, x);
                double b = a * (180.0 / Math.PI);
                col = (int)Math.Floor((b) / dg) + 90;
                if (col > 180)
                {
                    col = -(360 - col);
                }
            }
            else
            {
                double a = Math.Atan2(-y, x);
                double b = 360.0 - a * (180.0 / Math.PI);
                col = (int)Math.Floor((b) / dg) - 270;
            }
            string thickvalue = Convert.ToString(dataBot[(TempBotRows - Y - 1) * TempBotRows + X]);

            String str = "钢包底部半径为：" + row * 19.4 + "mm，温度：" + Convert.ToString(dataBot[(TempBotRows - Y) * TempBotRows + X]) + "℃，当前角度：" + (col + 1) + "°";
            txtBxTempPosi.Text = str;
        }

        private double f_dwf = 0, f_dhf = 0, s_dwf = 0, s_dhf = 0;

        

        private void PicBoxBottomPic_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ThickBotPic fmThickBotPic = new ThickBotPic(image_rote);
            fmThickBotPic.Show();
        }

        private void picBoxThickCir_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            if (bDrawThick[0])
            {
                Rectangle rt = e.ClipRectangle;
                double dwf = (double)(rt.Size.Width * 1.0f / ThickCirCols);
                double dhf = (double)(rt.Size.Height * 1.0f / ThickCirRows);

                f_dwf = dwf;
                f_dhf = dhf;
                SizeF Sf = new SizeF((float)dwf, (float)dhf);
                PointF PTf;

                RectangleF tmprtf;
                Brush bsh;
                double x, y;
                Int16 a;
                int b;
                int[] max = new int[4] { -10000, -10000, -10000, -10000 };
                int[] iMaxThickYbyPixel = new int[4] { 0, 0, 0, 0 };
                int[] iMaxThickXinPixel = new int[4] { 0, 0, 0, 0 };
                for (int X = 0; X < ThickCirRows; X++)  //  270行
                {
                    for (int Y = 0; Y < ThickCirCols; Y++)  //360列
                    {
                        x = dwf * Y;
                        y = dhf * X;
                        PTf = new PointF((float)x, (float)y);
                        tmprtf = new RectangleF(PTf, Sf);

                        a = dataThickCir[X * ThickCirCols + Y];
                        if (Y < (ThickCirCols / 4) && (X >= 100) && (a != 404))
                        {
                            if (max[0] < a)
                            {
                                max[0] = a;
                                iMaxThickYbyPixel[0] = Y;
                                iMaxThickXinPixel[0] = X;
                            }
                        }
                        if (((ThickCirCols / 4) <= Y) && (Y < (ThickCirCols / 4) * 2) && (X >= 100) && (a != 404))
                        {
                            if (max[1] < a)
                            {
                                max[1] = a;
                                iMaxThickYbyPixel[1] = Y;
                                iMaxThickXinPixel[1] = X;
                            }
                        }
                        if (((ThickCirCols / 4) * 2 <= Y) && (Y < (ThickCirCols / 4) * 3) && (X >= 100) && (a != 404))
                        {
                            if (max[2] < a)
                            {
                                max[2] = a;
                                iMaxThickYbyPixel[2] = Y;
                                iMaxThickXinPixel[2] = X;
                            }
                        }
                        if (((ThickCirCols / 4) * 3 <= Y) && (Y < (ThickCirCols)) && (X >= 100) && (a != 404))
                        {
                            if (max[3] < a)
                            {
                                max[3] = a;
                                iMaxThickYbyPixel[3] = Y;
                                iMaxThickXinPixel[3] = X;
                            }
                        }

                        b = a / iDiv1;
                        if (b < 0)
                        {
                            b = 0;
                        }
                        else if ((b >= 0) && (b < 9))
                        {
                            b = b + 1;
                        }
                        else if ((b >= 9) && (b < 12))
                        {
                            b = 10;
                        }
                        else if (b >= 12)
                        {
                            b = 11;
                        }

                        using (bsh = new SolidBrush(cr_thick[b]))
                        {
                            g.FillRectangle(bsh, tmprtf);
                        }
                    }
                }
                Color c = Color.Black;
                double deltaX = (double)(picBoxThickCir.Width * 1.0f / 360);
                for (int i = 0; i < 4; i++)
                {
                    Point LTPntofRt, LTPtoSignal;
                    Pen PenofRt;
                    Size SzofRt;
                    Rectangle Rt;
                    Font f;
                    float Penwid = (float)4.1;
                    LTPntofRt = new Point((int)((iMaxThickYbyPixel[i]) * dwf) - 10, (int)(iMaxThickXinPixel[i] * dhf) - 10);
                    //Position of annotations
                    LTPtoSignal = new Point((int)((iMaxThickYbyPixel[i]) * dwf), (int)(iMaxThickXinPixel[i] * dhf) - 30);
                    if (iMaxThickYbyPixel[i] * dwf > picBoxThickCir.Width * 0.75)
                    {
                        LTPtoSignal = new Point((int)((iMaxThickYbyPixel[i]) * dwf) - 170, (int)(iMaxThickXinPixel[i] * dhf) - 30);
                    }
                    //LTPtoSignal = new Point((int)((iMaxThickYbyPixel[i]) * dwf), (int)(iMaxThickXinPixel[i] * dhf) + 12);
                    SzofRt = new Size(20, 20);//定义画红框的大小，宽20高20
                    Rt = new Rectangle(LTPntofRt, SzofRt);
                    if (max[i] >= 90)
                    {
                        c = Color.Red;
                    }
                    else
                    {
                        c = Color.Green;
                    }
                    using (PenofRt = new Pen(c, Penwid))
                    {
                        g.DrawRectangle(PenofRt, Rt);
                    }
                    int X_p = (int)Math.Floor(((iMaxThickYbyPixel[i]) * dwf) / deltaX);
                    if ((0 <= X_p) && (X_p <= 180))
                    {
                        X_p = X_p - 180;
                    }
                    else if (180 < X_p)
                    {
                        X_p = X_p - 179;
                    }
                    using (f = new Font("黑体", 12))
                    {
                        g.DrawString("(" + Math.Round((iMaxThickXinPixel[i] + 0.5) * 16.6, 1) + "mm," + max[i] + "mm," + X_p + "°)", f, Brushes.White, LTPtoSignal);
                    }
                }
            }

        }

        private void picBoxThickBottom_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            if (bDrawThick[1])
            {
                Rectangle rt = e.ClipRectangle;
                double dwf = (double)(rt.Size.Width * 1.0f / ThickBotCols);
                double dhf = (double)(rt.Size.Height * 1.0f / ThickBotRows);

                s_dwf = dwf;
                s_dhf = dhf;
                SizeF Sf = new SizeF((float)dwf, (float)dhf);
                PointF PTf;

                RectangleF tmprtf;
                Brush bsh;
                double x, y;
                Int16 a;
                int b = 0;
                int max = -10000;
                int iMaxThickYbyPixel = 0;
                int iMaxThickXinPixel = 0;
                for (int X = 0; X < ThickBotRows; X++)  //  220行
                {
                    for (int Y = 0; Y < ThickBotCols; Y++)  //220列
                    {
                        x = dwf * Y;
                        y = dhf * X;
                        PTf = new PointF((float)x, (float)y);
                        tmprtf = new RectangleF(PTf, Sf);

                        a = dataThickBot[X * ThickBotCols + Y];
                        if ((max < a) && (a != 404))
                        {
                            max = a;
                            iMaxThickYbyPixel = Y;
                            iMaxThickXinPixel = X;
                        }
                        b = a / iDiv1;
                        if (b < 0)
                        {
                            b = 0;
                        }
                        else if ((b >= 0) && (b < 9))
                        {
                            b = b + 1;
                        }
                        else if ((b >= 9) && (b < 12))
                        {
                            b = 10;
                        }
                        else if (b >= 12)
                        {
                            b = 11;
                        }
                        using (bsh = new SolidBrush(cr_thick[b]))
                        {
                            g.FillRectangle(bsh, tmprtf);
                        }
                    }
                }
                Point LTPntofRt, LTPtoSignal;
                Pen PenofRt;
                Size SzofRt;
                Rectangle Rt;
                Font f;
                Color c = Color.Black;
                float Penwid = (float)4.1;
                LTPntofRt = new Point((int)((iMaxThickYbyPixel) * dwf) - 10, (int)(iMaxThickXinPixel * dhf) - 10);
                LTPtoSignal = new Point((int)((iMaxThickYbyPixel) * dwf), (int)(iMaxThickXinPixel * dhf) + 12);
                if ((iMaxThickYbyPixel * dwf > picBoxThickBottom.Width * 0.65) && (iMaxThickXinPixel * dhf < picBoxThickBottom.Height * 0.65))
                {
                    LTPtoSignal = new Point((int)((iMaxThickYbyPixel) * dwf - 150), (int)(iMaxThickXinPixel * dhf) + 12);
                }
                else if (iMaxThickXinPixel * dhf > picBoxThickBottom.Height * 0.75)
                {
                    LTPtoSignal = new Point((int)((iMaxThickYbyPixel) * dwf - 150), (int)(iMaxThickXinPixel * dhf) - 30);
                }
                SzofRt = new Size(20, 20);//定义画红框的大小，宽20高20
                Rt = new Rectangle(LTPntofRt, SzofRt);
                c = Color.Green;
                if (max >= 90)
                {
                    c = Color.Red;
                }
                using (PenofRt = new Pen(c, Penwid))
                {
                    g.DrawRectangle(PenofRt, Rt);
                }
                //radiums
                double dg = (double)(360.0f / 360);  //角度间隔度数
                double rad = (double)Math.Sqrt(((iMaxThickYbyPixel - 160) * (iMaxThickYbyPixel - 160) + (iMaxThickXinPixel - 160) * (iMaxThickXinPixel - 160)) / ((dwf + dhf) / 2));
                //angles
                double ang = iMaxThickXinPixel - 160;
                double _x = iMaxThickYbyPixel - 160;
                if (ang >= 0)
                {
                    double _a = Math.Atan2(ang, _x);
                    double _b = _a * (180.0 / Math.PI);
                    col = (int)Math.Floor((_b) / dg) + 90;
                    if (col > 180)
                    {
                        col = -(360 - col);
                    }
                }
                else
                {
                    double _a = Math.Atan2(-ang, _x);
                    double _b = 360.0 - _a * (180.0 / Math.PI);
                    col = (int)Math.Floor((_b) / dg) - 270;
                }
                using (f = new Font("黑体", 12))
                {
                    g.DrawString("(" + Math.Round((rad + 0.5) * 9.7, 1) + "mm," + max + "mm," + (col + 1) + "°)", f, Brushes.White, LTPtoSignal);
                }
            }

        }
        private void picBoxThickCir_MouseClick(object sender, MouseEventArgs e)
        {
            if (bDrawThick[0])
            {
                Point p1 = MousePosition;
                Point p2 = picBoxThickCir.PointToClient(p1);

                double deltaX = (double)(picBoxThickCir.Width * 1.0f / 360);
                int X = (int)(Math.Floor(p2.X / f_dwf));
                int Y = (int)(Math.Floor(p2.Y / f_dhf));

                int X_p = (int)(Math.Floor(p2.X / deltaX));
                if ((0 <= X_p) && (X_p < 180))
                {
                    X_p = X_p - 180;
                }
                else if (X_p >= 180)
                {
                    X_p = X_p - 179;
                }
                string thickvalue = Convert.ToString(dataThickCir[Y * ThickCirCols + X]);
                if (thickvalue == "404")
                {
                    thickvalue = null;
                }
                String str = "侵蚀：" + thickvalue + "mm；当前深度：" + Y * 16.6 + " - " + (Y + 1) * 16.6 + " mm，当前角度：" + X_p + "°";
                txtBxThickPosiInform.Text = str;
                // this.toolTip1.Show(str,this.picBoxThickCir);
            }
        }
        int col;
        private void picBoxThickBottom_MouseClick(object sender, MouseEventArgs e)
        {
            if (bDrawThick[1])
            {
                Point p1 = MousePosition;
                Point p2 = picBoxThickBottom.PointToClient(p1);

                int X = (int)(Math.Floor(p2.X / s_dwf));
                int Y = (int)(Math.Floor(p2.Y / s_dhf));
                double dg = (double)(360.0f / 360);  //角度间隔度数


                double minr = (double)(picBoxThickBottom.Width);
                if (minr > (double)picBoxThickBottom.Height)
                    minr = (double)picBoxThickBottom.Height;
                double dr = (double)(minr / ThickBotCols); //半径间隔大小


                int row = (int)Math.Floor((Math.Sqrt((p2.X - (minr / 2.0)) * (p2.X - (minr / 2.0)) + (p2.Y - (minr / 2.0)) * (p2.Y - (minr / 2.0)))) / dr);
                double x, y;
                y = p2.Y - minr / 2.0;
                x = p2.X - minr / 2.0;
                if (y >= 0)
                {
                    double a = Math.Atan2(y, x);
                    double b = a * (180.0 / Math.PI);
                    col = (int)Math.Floor((b) / dg) + 90;
                    if (col > 180)
                    {
                        col = -(360 - col);
                    }
                }
                else
                {
                    double a = Math.Atan2(-y, x);
                    double b = 360.0 - a * (180.0 / Math.PI);
                    col = (int)Math.Floor((b) / dg) - 270;
                }
                string thickvalue = Convert.ToString(dataThickBot[Y * ThickBotCols + X]);
                if (thickvalue == "404")
                {
                    thickvalue = null;
                }
                String str = "钢包底部半径为：" + row * 9.7 + "mm，侵蚀：" + thickvalue + "mm；当前角度：" + (col + 1) + "°";
                txtBxThickPosiInform.Text = str;
                // this.toolTip1.Show(str, this.picBoxThickBottom);
            }

        }
    }
}
