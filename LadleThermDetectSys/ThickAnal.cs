using AnyCAD.Forms;
using AnyCAD.Foundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.IO;

namespace LadleThermDetectSys
{
    public partial class ThickAnal : Form
    {
        AutoResizeForm asc = new AutoResizeForm();
        MySqlConnection connMySql;
        //string myconnection = "user id=root;password=ARIMlab2020.07.22;server=localhost;persistsecurityinfo=True;database=ironldlthermodetectmanactrl;port=3306;SslMode=None;allowPublicKeyRetrieval=true ";
        //string myconnection ="user id=tbrj;password=tbrj;server=10.99.24.144;persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true";
        string myconnection = "user id = test; password = test; server = 192.168.2.100; persistsecurityinfo = True; database = ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true";
        RenderControl mRenderThickCirExpand, mRenderThickBotExpand;
        static Dictionary<uint, Dictionary<uint, float>> mData = null;
        static uint mMinX = uint.MaxValue;
        static uint mMaxX = uint.MinValue;
        static uint mMinY = uint.MaxValue;
        static uint mMaxY = uint.MinValue;
        static Dictionary<uint, Dictionary<uint, float>> mData1 = null;
        static uint mMinX1 = uint.MaxValue;
        static uint mMaxX1 = uint.MinValue;
        static uint mMinY1 = uint.MaxValue;
        static uint mMaxY1 = uint.MinValue;
        private int ElID;//记录已测空包表读到的最大的序号
        private int rowsCount;
        private int index;//记录正处于哪个tabControlindex
        private int id_analysis;//据路正被分析的id

        private static int ThickCirRows = 360;
        private static int ThickCirCols = 720;
        private static int ThickBotRows = 380;
        private static int ThickBotCols = 380;
        Int16[] dataThickCir = new Int16[ThickCirRows * ThickCirCols];
        Int16[] dataThickBot = new Int16[ThickBotRows * ThickBotCols];

        bool[] bDrawThickAnaly = new bool[2] {false,false};
        bool[] bPicBox = new bool[2] { false, false };
        public ThickAnal()
        {
            InitializeComponent();
            mRenderThickCirExpand = new RenderControl();
            mRenderThickBotExpand = new RenderControl();
            this.pnlThickLdlCirExpand.Controls.Add(mRenderThickCirExpand);
            this.pnlThickBottom3D.Controls.Add(mRenderThickBotExpand);
        }
       
        private void ThickAnal_Load(object sender, EventArgs e)
        {
            asc.controllInitializeSize(this);
            TabCtrlThick.SelectedIndex = 1;
            connMySql = new MySqlConnection(myconnection);
            if (connMySql.State == ConnectionState.Closed)
            {
                connMySql.Open();
            }
            try
            {
                if (TabCtrlThick.SelectedIndex == 0) 
                {
                    index = 0;
                    //数据表的测量时间降序排列取前10行，显示部分列到界面
                    string SQL = "select id 序号, LadleNo 包号,LadleServDuty 包役,LadleAge 包龄,LadleContractor 砌筑商,MeasTm 测量时间,MinThick 最大侵蚀厚度 ,MinThickPos 区域  from emptyldl_meased where `Delete`=0 order by id desc limit 0,100;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "emptyldl_meased");
                    dtaGridVEmpty_Meased.DataSource = ds.Tables[0];
                }
                if (TabCtrlThick.SelectedIndex == 1) 
                {
                    lblNotion.Visible = false;
                    lblbarProgress.Visible = false;
                    progressBar1.Visible = false;
                    index = 1;
                    rowsCount = 0;
                    //数据表的测量时间降序排列取前10行，显示部分列到界面
                    string SQL = "select Analysis,id, LadleNo,LadleServDuty,LadleAge,LadleContractor,MeasTm,MinThick,MinThickPos from emptyldl_meased where `Delete`=0 and Analysis=0 order by id desc limit 0,100;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    dtGridViewEmptyed_Meased.AutoGenerateColumns = false;
                    objDataAdpter.Fill(ds, "emptyldl_meased");
                    dtGridViewEmptyed_Meased.DataSource = ds.Tables[0];
                    rowsCount = ds.Tables[0].Rows.Count;
                    if (rowsCount > 0)
                    {
                        ElID = Convert.ToInt32(ds.Tables[0].Rows[0][1]);
                    }
                    else if (rowsCount == 0) 
                    {
                        lblNotion.Visible = true;
                    }
                }
                if (TabCtrlThick.SelectedIndex == 2) 
                {
                    index = 2;
                    label4.Visible = false;
                    rowsCount = 0;
                    //数据表的测量时间降序排列取前10行，显示部分列到界面
                    string SQL = "select id 序号, LadleNo 包号,LadleServDuty 包役,LadleAge 包龄,LadleContractor 砌筑商,MeasTm 测量时间,MinThick 最大侵蚀厚度 ,MinThickPos 区域  from emptyldl_meased where `Delete`=0 and Analysis=1 and LadleAge>0 order by id desc limit 0,100;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "emptyldl_meased");
                    dataGridView1.DataSource = ds.Tables[0];
                    rowsCount = ds.Tables[0].Rows.Count;
                    if (rowsCount > 0)
                    {

                        ElID = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                        analy.ldlno = ds.Tables[0].Rows[0][1].ToString();
                        analy.LdlSerDuty = Convert.ToInt32(ds.Tables[0].Rows[0][2]);
                        analy.LdlAge = Convert.ToInt32(ds.Tables[0].Rows[0][3]);
                        analy.Contractor = ds.Tables[0].Rows[0][4].ToString();
                    }
                    else if (rowsCount == 0)
                    {
                        label4.Visible = true;
                    }
                }
                connMySql.Close();
            }
            catch (Exception EE)
            {

                MessageBox.Show("查询数据失败！" + EE.ToString());
            }
            if ((TabCtrlThick.SelectedIndex == 1) || (TabCtrlThick.SelectedIndex == 2))
            {
                ShowthickImage();
            }
        }
        private void TabCtrlThick_SelectedIndexChanged(object sender, EventArgs e)
        {
            connMySql = new MySqlConnection(myconnection);
            if (connMySql.State == ConnectionState.Closed)
            {
                connMySql.Open();
            }
            try
            {
                if (TabCtrlThick.SelectedIndex == 0)
                {
                    index = 0;
                    //数据表的测量时间降序排列取前10行，显示部分列到界面
                    string SQL = "select id 序号, LadleNo 包号,LadleServDuty 包役,LadleAge 包龄,LadleContractor 砌筑商,MeasTm 测量时间,MinThick 最大侵蚀厚度 ,MinThickPos 区域  from emptyldl_meased order by id desc limit 0,100;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "emptyldl_meased");
                    dtaGridVEmpty_Meased.DataSource = ds.Tables[0];
                }
                if (TabCtrlThick.SelectedIndex == 1)
                {
                    lblNotion.Visible = false;
                    lblbarProgress.Visible = false;
                    progressBar1.Visible = false;
                    index = 1;
                    ElID = 0;
                    rowsCount = 0;
                    //数据表的测量时间降序排列取前10行，显示部分列到界面
                    string SQL = "select Analysis,id, LadleNo,LadleServDuty,LadleAge,LadleContractor,MeasTm,MinThick,MinThickPos from emptyldl_meased where `Delete`=0 and Analysis=0  order by id desc limit 0,100;;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    dtGridViewEmptyed_Meased.AutoGenerateColumns = false;
                    objDataAdpter.Fill(ds, "emptyldl_meased");
                    dtGridViewEmptyed_Meased.DataSource = ds.Tables[0];
                    rowsCount = ds.Tables[0].Rows.Count;
                    if (rowsCount > 0)
                    {
                        ElID = Convert.ToInt32(ds.Tables[0].Rows[0][1]);
                    }
                    else if(rowsCount == 0)
                    {
                        lblNotion.Visible = true;
                    }
                }
                if (TabCtrlThick.SelectedIndex == 2)
                {
                    index = 2;
                    ElID = 0;
                    rowsCount = 0;
                    label4.Visible = false;
                    //数据表的测量时间降序排列取前10行，显示部分列到界面
                    string SQL = "select id 序号, LadleNo 包号,LadleServDuty 包役,LadleAge 包龄,LadleContractor 砌筑商,MeasTm 测量时间,MinThick 最大侵蚀厚度 ,MinThickPos 区域  from emptyldl_meased where `Delete`=0 and Analysis=1 and LadleAge>0 order by id desc limit 0,100;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "emptyldl_meased");
                    dataGridView1.DataSource = ds.Tables[0];
                    rowsCount = ds.Tables[0].Rows.Count;
                    if (rowsCount > 0)
                    {
                        ElID = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                        analy.ldlno = ds.Tables[0].Rows[0][1].ToString();
                        analy.LdlSerDuty = Convert.ToInt32(ds.Tables[0].Rows[0][2]);
                        analy.LdlAge = Convert.ToInt32(ds.Tables[0].Rows[0][3]);
                        analy.Contractor = ds.Tables[0].Rows[0][4].ToString();
                    }
                    else if (rowsCount == 0)
                    {
                        label4.Visible = true;
                    }
                }
                connMySql.Close();
            }
            catch (Exception EE)
            {
                MessageBox.Show("查询数据失败！" + EE.ToString());
            }
            if ((TabCtrlThick.SelectedIndex == 1) || (TabCtrlThick.SelectedIndex == 2))
            {
                 ShowthickImage();
            }
        }

        private void ThickAnal_SizeChanged(object sender, EventArgs e)
        {
            asc.controlAutoSize(this);
        }

        private void btnThickSelectQuery_Click(object sender, EventArgs e)
        {
            connMySql = new MySqlConnection(myconnection);
            try
            {
                string inputLadleNo = this.txtBxThickLadleNo.Text.ToString();
                string Time1 = dtTimPkThickStartTime.Value.ToString();
                string Time2 = dtTimPkThickEndTime.Value.ToString();
                string SQL;
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }     //打开数据库
                if (inputLadleNo != "")
                {
                    SQL = "select id 序号, LadleNo 包号,LadleServDuty 包役,LadleAge 包龄,LadleContractor 砌筑商,MeasTm 测量时间,MinThick 最大侵蚀厚度 ,MinThickPos 区域  from emptyldl_meased where LadleNo='" + inputLadleNo + "' and MeasTm >='" + Time1 + "' and MeasTm <='" + Time2 + "';";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "emptyldl_meased");
                    dtaGridVEmpty_Meased.DataSource = ds.Tables[0];
                }
                else
                {
                    SQL = "select id 序号, LadleNo 包号,LadleServDuty 包役,LadleAge 包龄,LadleContractor 砌筑商,MeasTm 测量时间,MinThick 最大侵蚀厚度 ,MinThickPos 区域  from emptyldl_meased where MeasTm >='" + Time1 + "' and MeasTm<='" + Time2 + "'";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "emptyldl_meased");
                    dtaGridVEmpty_Meased.DataSource = ds.Tables[0];
                }
                connMySql.Close();
            }
            catch (Exception ee)
            {
                MessageBox.Show("查询数据失败！" + ee.ToString());
            }
        }
        public void RunWall(RenderControl render, string txtname)
        {
            mRenderThickCirExpand.ClearAll();

            if (!ReadWallData(txtname))
                return;
            var matplot = Matplot.Create("MyMatlab 2020");
            var xRange = new PlotRange(mMinX, mMaxX - 1, 1);
            var yRange = new PlotRange(mMinY, mMaxY - 1, 1);
            matplot.AddSurface(xRange, yRange, (idxU, idxV, u, v) =>
            {
                double x = u;
                double y = v;
                double z = mData[idxU + 1][idxV + 1];
                return new GPnt(x, y, z);
            });
            var ctx = render.GetContext();
            ctx.ClearDisplayFilter(EnumShapeFilter.All);
            ctx.AddDisplayFilter(EnumShapeFilter.Face);
            var node = matplot.Build(ColorMapKeyword.Create(EnumSystemColorMap.Cooltowarm));
            node.SetPickable(false);
            render.GetContext().GetCamera().LookAt(new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(-1, 0, 0));

            var pw = new PaletteWidget();
            pw.Update(matplot.GetColorTable());

            render.ShowSceneNode(pw);
            render.ShowSceneNode(node);
            render.ZoomAll();
        }


        bool bShowTooltip = false;
        private void toolTipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mRenderThickCirExpand.SetHilightingCallback((PickedItem item) =>
            {
                var text = item.GetPoint().GetPosition().ToString();
                this.mRenderThickCirExpand.SetToolTip(text);
                return true;
            });
            bShowTooltip = !bShowTooltip;
        }


        public void RunBottom(RenderControl render, string txtname)
        {
            mRenderThickBotExpand.ClearAll();

            if (!ReadBotData(txtname))
                return;

            var matplot = Matplot.Create("MyMatlab 2020");

            var xRange1 = new PlotRange(mMinX1, mMaxX1 - 1, 1);
            var yRange1 = new PlotRange(mMinY1, mMaxY1 - 1, 1);
            matplot.AddSurface(xRange1, yRange1, (idxU, idxV, u, v) =>
            {
                double x = u;
                double y = v;
                double z = mData1[idxU + 1][idxV + 1];

                return new GPnt(x, y, z);
            });
            var ctx = render.GetContext();
            ctx.ClearDisplayFilter(EnumShapeFilter.All);
            ctx.AddDisplayFilter(EnumShapeFilter.Face);
            var node = matplot.Build(ColorMapKeyword.Create(EnumSystemColorMap.Cooltowarm));
            node.SetPickable(false);
            render.GetContext().GetCamera().LookAt(new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(-1, 0, 0));

            var pw = new PaletteWidget();
            pw.Update(matplot.GetColorTable());

            render.ShowSceneNode(pw);
            render.ShowSceneNode(node);
            render.ZoomAll();
        }
        public static string GetResourcePath(string fileName)
        {
            return AppDomain.CurrentDomain.BaseDirectory + @"\..\..\data\" + fileName;
        }
        
        private void dtaGridVEmpty_Meased_Click(object sender, EventArgs e)
        {
            if (rowsCount > 0) 
            {
                ElID = Convert.ToInt32(dtaGridVEmpty_Meased.CurrentRow.Cells["序号"].Value.ToString());
                ShowthickPic();
            }
        }
       /*protected override void OnLoad(EventArgs e)
        {

            RunWall(mRenderThickCirExpand, "XYZ.txt");
            RunBottom(mRenderThickBotExpand, "XOY.txt");

            base.OnLoad(e);
        }*/
        private void ShowthickPic() 
        {
            
            ThickReadBlob();
            RunWall(mRenderThickCirExpand, "XYZ.txt");
            RunBottom(mRenderThickBotExpand, "XOY.txt");

        }
        
        private void ThickReadBlob()
        {
            connMySql = new MySqlConnection(myconnection);
            try
            {
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }
                string SQL = "SELECT  CircumThick,BottomThick,ThickPhoto  FROM emptyldl_meased where id ='" + ElID + "';";
                //MySqlCommand cmd = new MySqlCommand(SQL, connMySql);
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "emptyldl_meased");
                connMySql.Close();
                //MySqlDataReader rdr = cmd.ExecuteReader();
                bDrawThickAnaly[0] = false; bDrawThickAnaly[1] = false;
                //while (rdr.Read())
                {
                    bDrawThickAnaly[0] = true; bDrawThickAnaly[1] = true;
                    if ((ds.Tables[0].Rows[0][0] == System.DBNull.Value)||(ds.Tables[0].Rows[0][0] ==null))
                    {
                        bDrawThickAnaly[0] = false;
                    }
                    if ((ds.Tables[0].Rows[0][1] == System.DBNull.Value)||(ds.Tables[0].Rows[0][1] == null))
                    {
                        bDrawThickAnaly[1] = false;
                    }

                    if (bDrawThickAnaly[0]) 
                    {
                        
                        int inlen = ThickCirRows * ThickCirCols * 2;
                        byte[] buffer = new byte[inlen];
                        buffer = (byte[])ds.Tables[0].Rows[0][0];
                        for (int i = 0; i < inlen / 2; i++)
                        {
                            dataThickCir[i] = (Int16)((buffer[2 * i]) + (buffer[2 * i + 1]) * 256);

                            //存入XOY

                        }
                        if (index == 0)
                        {
                            writeData("XYZ.txt", dataThickCir, ThickCirRows, ThickCirCols, 200);
                        }
                    }

                    if (bDrawThickAnaly[1])
                    {
                        
                        int inlen1 = ThickBotRows * ThickBotCols * 2;
                        byte[] buffer1 = new byte[inlen1];

                        //存放获得的二进制数据，温度
                        buffer1 = (byte[])ds.Tables[0].Rows[0][1];
                        for (int i = 0; i < inlen1 / 2; i++)
                        {
                            dataThickBot[i] = (Int16)((buffer1[2 * i]) + (buffer1[2 * i + 1]) * 256);
                        }
                        if (index == 0)
                        {
                            writeData("XOY.txt", dataThickBot, ThickBotRows, ThickBotCols, 400);
                        }
                    }
                    if (ds.Tables[0].Rows[0][2] == DBNull.Value)
                    {
                        System.Drawing.Image image = LadleThermDetectSys.Properties.Resources.内衬照片11;
                        if (index == 1)
                        {
                            picBoxBottomPic.Image = image;
                        }
                        if (index == 2)
                        {
                            pictureBox3.Image = image;
                        }
                        image_rote = image;
                    }
                    else
                    {
                        byte[] Thick = (byte[])ds.Tables[0].Rows[0][2];
                        MemoryStream stream = new MemoryStream(Thick, true);
                        System.Drawing.Image image = System.Drawing.Image.FromStream(stream, true);
                        stream.Close();
                        if (index == 1)
                        {
                            picBoxBottomPic.Image = image;
                        }
                        if (index == 2)
                        {
                            pictureBox3.Image = image;
                        }
                        image_rote = image;
                    }
                    image_rote.RotateFlip(RotateFlipType.Rotate270FlipXY);
                }
                //rdr.Close();
                
            }
            catch (Exception EE)
            {
                MessageBox.Show("查询数据失败！" + EE.ToString());
            }
        }
        private void writeData(string filepath,Int16[] arry, int rows, int cols, int error) 
        {
            string filePath = GetResourcePath(filepath);
            FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

           // Int16[] arry = new Int16[rows * cols];
            

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (!System.IO.File.Exists(filePath))
                    {
                        fs = new FileStream(filePath, FileMode.Append, FileAccess.Write);
                    }
                    else
                    {
                        if (Math.Abs(arry[(i) * cols + (j)]) < error)
                        {
                            string content = (i + 1).ToString() + "\t" + (j + 1).ToString() + "\t" + (Convert.ToSingle(-arry[(i) * cols + (j)])).ToString();
                            sw.WriteLine(content);
                            sw.Flush();
                        }
                        else
                        {
                            arry[(i) * cols + (j)] = 0;
                            string content = (i + 1).ToString() + "\t" + (j + 1).ToString() + "\t" + (Convert.ToSingle(-arry[(i) * cols + (j)])).ToString();
                            sw.WriteLine(content);
                            sw.Flush();
                        }
                        
                    }
                }
            }
            sw.Close();
            fs.Close();
        }
        bool ReadWallData(string txtname)
        {
            //if (mData != null) 
            {
              //  mData=new Dictionary<uint, Dictionary<uint, float>>();
            }//

           string fileName = GetResourcePath(txtname);
           using (StreamReader reader = File.OpenText(fileName))
            {
                mData = new Dictionary<uint, Dictionary<uint, float>>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var items = line.Split('\t');
                    if (items.Length != 3)
                    {
                       continue;
                    }

                    uint x = uint.Parse(items[0]);
                    uint y = uint.Parse(items[1]);
                    float z = float.Parse(items[2]);

                    if (mMinX > x) mMinX = x;
                    if (mMaxX < x) mMaxX = x;
                    if (mMinY > y) mMinY = y;
                    if (mMaxY < y) mMaxY = y;

                    Dictionary<uint, float> yData = null;
                    if (!mData.TryGetValue(x, out yData))
                    {
                        yData = new Dictionary<uint, float>();
                        mData[x] = yData;
                    }

                    yData[y] = z;
                }
            }

            return true;
        }

        private void dtGridViewEmptyed_Meased_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            connMySql = new MySqlConnection(myconnection);
            try
            {
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }
                DataGridViewCell cell = dtGridViewEmptyed_Meased.Rows[e.RowIndex].Cells[e.ColumnIndex];
                ElID = int.Parse(dtGridViewEmptyed_Meased.Rows[e.RowIndex].Cells["id"].Value.ToString());
                if (cell.FormattedValue.ToString() == "删除")
                {
                    if (MessageBox.Show("是否删除此次测量数据？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    {
                        string sql = "update emptyldl_meased set `Delete`=b'1' where id="+ ElID + ";";
                        MySqlCommand thisCommand = new MySqlCommand(sql, connMySql);
                        thisCommand.ExecuteNonQuery();
                        ElID = 0;
                        rowsCount = 0;
                        //数据表的测量时间降序排列取前10行，显示部分列到界面
                        string SQL = "select Analysis,id, LadleNo,LadleServDuty,LadleAge,LadleContractor,MeasTm,MinThick,MinThickPos from emptyldl_meased where `Delete`=0 and Analysis=0  order by id desc limit 0,100;;";
                        MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                        objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                        DataSet ds = new DataSet();
                        dtGridViewEmptyed_Meased.AutoGenerateColumns = false;
                        objDataAdpter.Fill(ds, "emptyldl_meased");
                        dtGridViewEmptyed_Meased.DataSource = ds.Tables[0];
                        rowsCount = ds.Tables[0].Rows.Count;
                        if (rowsCount > 0)
                        {
                            ElID = Convert.ToInt32(ds.Tables[0].Rows[0][1]);
                        }
                        else if (rowsCount == 0)
                        {
                            lblNotion.Visible = true;
                        }
                    }
                }
                connMySql.Close();
            }
            catch (Exception ee) 
            {
                MessageBox.Show("删除数据出错！"+ee.ToString());
            }
            ShowthickImage();
        }

       
        private void ShowthickImage()
        {
            ThickReadBlob();
            if (index == 1) 
            {
                picBxThickCirExpand.Refresh();
                picBxThickBot.Refresh();
                ShowthickBottomImage();
                              
            }
            if (index == 2) 
            {
                pictureBox1.Refresh();
                pictureBox2.Refresh();
                ShowthickBottomImage();
            }
        }

        System.Drawing.Image image_rote;
        private void ShowthickBottomImage()
        {
            connMySql = new MySqlConnection(myconnection);
            try
            {
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }
                string SQL = "SELECT  ThickPhoto  FROM emptyldl_meased where id ='" + ElID + "';";
                MySqlCommand thickcmd = new MySqlCommand(SQL, connMySql);
                if ((thickcmd.ExecuteScalar() == DBNull.Value)||(thickcmd.ExecuteScalar() == null))
                {
                    connMySql.Close();
                    System.Drawing.Image image = LadleThermDetectSys.Properties.Resources.内衬照片11;
                    if (index == 1)
                    {
                        picBoxBottomPic.Image = image;
                    }
                    if (index == 2)
                    {
                        pictureBox3.Image = image;
                    }
                    image_rote = image;
                }
                else
                {
                    byte[] thickpic = (byte[])thickcmd.ExecuteScalar();
                    if (thickpic.Length > 0) 
                    {
                        MemoryStream stream = new MemoryStream(thickpic, true);
                        System.Drawing.Image image = System.Drawing.Image.FromStream(stream, true);
                        connMySql.Close();
                        stream.Close();
                        //stream.Write(b, 0, b.Length);
                        //picBoxBottomPic.Image = new Bitmap(stream);
                        if (index == 1) 
                        {
                            picBoxBottomPic.Image = image;
                        }
                        if (index == 2) 
                        {
                            pictureBox3.Image = image;
                        }
                        image_rote = image;
                    }
                }
                image_rote.RotateFlip(RotateFlipType.Rotate270FlipXY);
                
            }
            catch (Exception EE)
            {
                MessageBox.Show("数据库连接出现问题!"+EE.ToString());
            }
        }
        private void picBoxBottomPic_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {
            ThickBotPic fmThickBotPic = new ThickBotPic(image_rote);

            // fmThickBotPic.ThickBotPic_ChangePicture += new ThickBotPic.ChangePictureHandler(Form1_Change_Picture);//使用委托，关闭子窗体时，图片回传
            fmThickBotPic.Show();
        }
        private void pictureBox3_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {
            ThickBotPic fmThickBotPic = new ThickBotPic(image_rote);

            // fmThickBotPic.ThickBotPic_ChangePicture += new ThickBotPic.ChangePictureHandler(Form1_Change_Picture);//使用委托，关闭子窗体时，图片回传
            fmThickBotPic.Show();
        }
        int iDiv1 = 15;

        Color[] cr_thick = new Color[] {
         Color.FromArgb(84, 0, 133),
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
         Color.Black};

        private double f_dwf = 0, f_dhf = 0, s_dwf = 0, s_dhf = 0;
        int col;
        private void picBxThickBot_Click(object sender, EventArgs e)
        {
            Point p1 = MousePosition;
            Point p2 = picBxThickBot.PointToClient(p1);

            int X = (int)(Math.Floor(p2.X / s_dwf));
            int Y = (int)(Math.Floor(p2.Y / s_dhf));
            double dg = (double)(360.0f / 360);  //角度间隔度数


            double minr = (double)(picBxThickBot.Width);
            if (minr > (double)picBxThickBot.Height)
                minr = (double)picBxThickBot.Height;
            double dr = (double)((minr / 2) / (ThickBotCols / 2)); //半径间隔大小


            int row = (int)Math.Floor((Math.Sqrt((p2.X - (picBxThickBot.Width / 2.0)) * (p2.X - (picBxThickBot.Width / 2.0)) + (p2.Y - (picBxThickBot.Height / 2.0)) * (p2.Y - (picBxThickBot.Height / 2.0)))) / dr);
            double x, y;
            y = p2.Y - picBxThickBot.Height / 2.0;
            x = p2.X - picBxThickBot.Width / 2.0;
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
        }

        private void dtGridViewEmptyed_Meased_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string SerNo = dtGridViewEmptyed_Meased.CurrentRow.Cells["id"].Value.ToString();
            if (SerNo != "")
            {
                ElID = Convert.ToInt32(SerNo);
            }
            ShowthickImage();
        }
        private List<int> serno = new List<int>();
        private void dtGridViewEmptyed_Meased_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dtGridViewEmptyed_Meased.Rows.Count > 0) 
            {
                int ck_status = int.Parse(dtGridViewEmptyed_Meased.CurrentRow.Cells["ck"].Value.ToString());
                int SerNo = int.Parse(dtGridViewEmptyed_Meased.CurrentRow.Cells["id"].Value.ToString());
                if (ck_status == 1)
                {
                    serno.Add(SerNo);
                }
                else if (ck_status == 0) 
                {
                    serno.Remove(SerNo);
                }
            }
        }

        //即时触发
        private void dtGridViewEmptyed_Meased_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            //当前单元格是否有未提交的更改
            if (dtGridViewEmptyed_Meased.IsCurrentCellDirty) 
            {
                dtGridViewEmptyed_Meased.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private struct storeinfo 
        {
            public string LadleNo;
            public int LadleServDuty;
            public int LadleAge;
            public string Contractor;
            public Int16[] dataThickCircum;
            public Int16[] dataThickBottom;
        }

        private struct parainfo
        {
            public string tablenameCir;
            public string tablenameBot;
            public int LadleServDuty;
            public int LadleAge;
            public int startidCir;
            public int startidBot;
            public StringBuilder addsql;
        }

        private struct thick_age
        {
            public string tablename;
            public int Startid;
            public string Ageid;
        }

        private struct thick_analy_history 
        {
            public string LadleServDuty;
            public string LadleAge;
            public string Time;
        }
        private void butYes_Click(object sender, EventArgs e)
        {
            butYes.Enabled = false;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            connMySql = new MySqlConnection(myconnection);
            try
            {
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }
                int serno_count = serno.Count;
                if (serno_count == 0) 
                {
                    if (dtaGridVEmpty_Meased.Rows.Count == 0) 
                    {
                        MessageBox.Show("没有测厚数据待分析！");
                    }
                    if (dtaGridVEmpty_Meased.Rows.Count > 0) 
                    {
                        MessageBox.Show("请选择需要分析的数据！");
                    }
                }
                else if (serno_count > 0)
                {
                    double delta_probar = (double)progressBar1.Maximum / serno_count;
                    lblbarProgress.Visible = true;
                    progressBar1.Visible = true;

                    for (int m = 0; m < serno_count; m++)
                    {
                        id_analysis = serno[m];
                        string sql1 = "select LadleNo,LadleServDuty,LadleAge,LadleContractor,CircumThick,BottomThick from emptyldl_meased where id=" + id_analysis + ";";
                        MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                        objDataAdpter.SelectCommand = new MySqlCommand(sql1, connMySql);
                        DataSet ds = new DataSet();
                        objDataAdpter.Fill(ds, "emptyldl_meased");
                        storeinfo mge = new storeinfo();
                        mge.LadleNo = ds.Tables[0].Rows[0][0].ToString();
                        mge.LadleServDuty = Convert.ToInt32(ds.Tables[0].Rows[0][1]);
                        mge.LadleAge = Convert.ToInt32(ds.Tables[0].Rows[0][2]);
                        mge.Contractor = ds.Tables[0].Rows[0][3].ToString();
                        string SQL = "SELECT  CircumThick,BottomThick  FROM emptyldl_meased where id ='" + id_analysis + "';";
                        MySqlCommand cmd = new MySqlCommand(SQL, connMySql);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            long len;
                            int inlen = ThickCirRows * ThickCirCols * 2;
                            byte[] buffer = new byte[inlen];

                            len = rdr.GetBytes(0, 0, buffer, 0, inlen);
                            for (int i = 0; i < len / 2; i++)
                            {
                                dataThickCir[i] = (Int16)((buffer[2 * i]) + (buffer[2 * i + 1]) * 256);
                            }
                            long len1;
                            int inlen1 = ThickBotRows * ThickBotCols * 2;
                            byte[] buffer1 = new byte[inlen1];
                            //存放获得的二进制数据，温度
                            len1 = rdr.GetBytes(1, 0, buffer1, 0, inlen1);
                            for (int i = 0; i < len1 / 2; i++)
                            {
                                dataThickBot[i] = (Int16)((buffer1[2 * i]) + (buffer1[2 * i + 1]) * 256);
                            }
                        }
                        rdr.Close();
                        mge.dataThickCircum = dataThickCir;
                        mge.dataThickBottom = dataThickBot;
                        progressBar1.Value = Convert.ToInt32(delta_probar * (m + 0.1));
                        //将此数据存入历史分析表中
                        parainfo para = new parainfo();
                        para.tablenameCir = $"{mge.LadleNo}_cirthick";
                        para.tablenameBot = $"{mge.LadleNo}_botthick";
                        para.LadleServDuty = mge.LadleServDuty % 3;
                        para.LadleAge = mge.LadleAge;
                        para.startidCir = (para.LadleServDuty - 1) * 220 * ThickCirRows + mge.LadleAge  * ThickCirRows + 1;
                        para.startidBot = (para.LadleServDuty - 1) * 220 * ThickBotRows + mge.LadleAge  * ThickBotRows + 1;
                        para.addsql = new StringBuilder();
                        progressBar1.Value = Convert.ToInt32(delta_probar * (m + 0.2));
                        for (int i = 0; i < ThickCirRows; i++)
                        {
                            for (int t = 0; t < ThickCirCols; t++)
                            {
                                para.addsql.Append(",col" + (t + 1) + "=" + mge.dataThickCircum[i * ThickCirCols + t] + "");
                            }
                            string sql2 = "update " + para.tablenameCir + " set LadleServDuty=" + para.LadleServDuty + ",UpdateTime='" + DateTime.Now.ToString() + "'" + para.addsql + " where Id=" + (para.startidCir + i )+ ";";
                            MySqlCommand thisCommand2 = new MySqlCommand(sql2, connMySql);
                            thisCommand2.ExecuteNonQuery();
                            para.addsql.Clear();
                        }
                        progressBar1.Value = Convert.ToInt32(delta_probar * (m + 0.6));
                        for (int i = 0; i < ThickBotRows; i++)
                        {
                            for (int t = 0; t < ThickBotCols; t++)
                            {
                                para.addsql.Append(",col" + (t + 1 )+ "=" + mge.dataThickBottom[i * ThickBotCols + t] + "");
                            }
                            string sql3 = "update " + para.tablenameBot + " set LadleServDuty=" + para.LadleServDuty + ",UpdateTime='" + DateTime.Now.ToString() + "'" + para.addsql + " where Id=" + (para.startidBot + i) + ";";
                            MySqlCommand thisCommand3 = new MySqlCommand(sql3, connMySql);
                            thisCommand3.ExecuteNonQuery();
                            para.addsql.Clear();
                        }
                        progressBar1.Value = Convert.ToInt32(delta_probar * (m + 0.85));
                        thick_age ageMeg = new thick_age();
                        ageMeg.tablename = $"{mge.LadleNo}_thick";
                        ageMeg.Startid = mge.LadleServDuty;
                        ageMeg.Ageid = $"Age{mge.LadleAge}";
                        string sql5 = "UPDATE "+ageMeg.tablename+" SET "+ageMeg.Ageid+"="+mge.LadleAge+" WHERE Id="+ageMeg.Startid+";";
                        MySqlCommand thisCommand5 = new MySqlCommand(sql5, connMySql);
                        thisCommand5.ExecuteNonQuery();
                        progressBar1.Value = Convert.ToInt32(delta_probar * (m + 0.9));
                        //将此记录存入表thick_analy_history
                        thick_analy_history analy = new thick_analy_history();
                        analy.LadleServDuty = $"LadleServDuty0{para.LadleServDuty}";
                        analy.LadleAge = $"LadleAge0{para.LadleServDuty}";
                        analy.Time = $"Time0{para.LadleServDuty}";
                        string sql4 = "update thick_analy_history set LadleContractor='"+ mge.Contractor + "'," + analy.LadleServDuty + "=" + mge.LadleServDuty + "," + analy.LadleAge + "=" + mge.LadleAge + "," + analy.Time + "='" + DateTime.Now.ToString() + "' where LadleNo='" + mge.LadleNo + "';";
                        MySqlCommand thisCommand4 = new MySqlCommand(sql4, connMySql);
                        thisCommand4.ExecuteNonQuery();
                        string sql = "update emptyldl_meased set `Analysis`=b'1' where id=" + id_analysis + ";";
                        MySqlCommand thisCommand = new MySqlCommand(sql, connMySql);
                        thisCommand.ExecuteNonQuery();
                        progressBar1.Value = (int)delta_probar * (m + 1);
                    }
                    serno.Clear();
                    progressBar1.Visible = false;
                    lblbarProgress.Text="存储已完成！";
                    string SQL1 = "select Analysis,id, LadleNo,LadleServDuty,LadleAge,LadleContractor,MeasTm,MinThick,MinThickPos from emptyldl_meased where `Delete`=0 and Analysis=0 order by id desc limit 0,100;";
                    MySqlDataAdapter objDataAdpter1 = new MySqlDataAdapter();
                    objDataAdpter1.SelectCommand = new MySqlCommand(SQL1, connMySql);
                    DataSet ds1 = new DataSet();
                    dtGridViewEmptyed_Meased.AutoGenerateColumns = false;
                    objDataAdpter1.Fill(ds1, "emptyldl_meased");
                    dtGridViewEmptyed_Meased.DataSource = ds1.Tables[0];
                    rowsCount = ds1.Tables[0].Rows.Count;
                    if (rowsCount > 0)
                    {
                        ElID = Convert.ToInt32(ds1.Tables[0].Rows[0][1]);
                    }
                    else if (rowsCount == 0)
                    {
                        lblNotion.Visible = true;
                    }
                    if (rowsCount > 0)
                    {
                        ShowthickImage();
                    }
                }
                connMySql.Close();
            }
            catch (Exception n)
            {
                MessageBox.Show("数据库连接出现问题!" + n.ToString());
            }
            lblbarProgress.Visible = false;
            butYes.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label4.Visible = false;
            connMySql = new MySqlConnection(myconnection);
            try
            {
                string inputLadleNo = this.textBox1.Text.ToString();
                string Time1 = dateTimePicker2.Value.ToString();
                string Time2 = dateTimePicker1.Value.ToString();
                string SQL;
                rowsCount = 0;
                ElID = 0;
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }     //打开数据库
                if (inputLadleNo != "")
                {
                    SQL = "select id 序号, LadleNo 包号,LadleServDuty 包役,LadleAge 包龄,LadleContractor 砌筑商,MeasTm 测量时间,MinThick 最大侵蚀厚度 ,MinThickPos 区域  from emptyldl_meased where `Delete`=0 and Analysis=1 and LadleNo='" + inputLadleNo + "' and MeasTm >='" + Time1 + "' and MeasTm <='" + Time2 + "' order by id desc limit 0,100;;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "emptyldl_meased");
                    dataGridView1.DataSource = ds.Tables[0];
                    rowsCount = ds.Tables[0].Rows.Count;
                    if (rowsCount > 0)
                    {
                        ElID = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                        analy.ldlno = ds.Tables[0].Rows[0][1].ToString();
                        analy.LdlSerDuty = Convert.ToInt32(ds.Tables[0].Rows[0][2]);
                        analy.LdlAge = Convert.ToInt32(ds.Tables[0].Rows[0][3]);
                        analy.Contractor = ds.Tables[0].Rows[0][4].ToString();
                    }
                    else if (rowsCount == 0)
                    {
                        label4.Visible = true;
                    }
                }
                else
                {
                    SQL = "select id 序号, LadleNo 包号,LadleServDuty 包役,LadleAge 包龄,LadleContractor 砌筑商,MeasTm 测量时间,MinThick 最大侵蚀厚度 ,MinThickPos 区域  from emptyldl_meased where `Delete`=0 and Analysis=1 and MeasTm >='" + Time1 + "' and MeasTm<='" + Time2 + "' order by id desc limit 0,100;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "emptyldl_meased");
                    dataGridView1.DataSource = ds.Tables[0];
                    rowsCount = ds.Tables[0].Rows.Count;
                    if (rowsCount > 0)
                    {
                        ElID = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                        analy.ldlno = ds.Tables[0].Rows[0][1].ToString();
                        analy.LdlSerDuty = Convert.ToInt32(ds.Tables[0].Rows[0][2]);
                        analy.LdlAge = Convert.ToInt32(ds.Tables[0].Rows[0][3]);
                        analy.Contractor = ds.Tables[0].Rows[0][4].ToString();
                    }
                    else if (rowsCount == 0)
                    {
                        label4.Visible = true;
                    }
                }
                connMySql.Close();
            }
            catch (Exception ee)
            {
                MessageBox.Show("查询数据失败！" + ee.ToString());
            }
            ShowthickImage();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            if (bDrawThickAnaly[0])
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
                        else if ((b >= 9) && (b < 26))
                        {
                            b = 15;
                        }
                        else if (b >= 26)
                        {
                            b = 16;
                        }

                        using (bsh = new SolidBrush(cr_thick[b])) 
                        {
                            g.FillRectangle(bsh, tmprtf);
                        }     
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    Point LTPntofRt;
                    Pen PenofRt;
                    Size SzofRt;
                    Rectangle Rt;
                    float Penwid = (float)4.1;
                    LTPntofRt = new Point((int)((iMaxThickYbyPixel[i]) * dwf) - 10, (int)(iMaxThickXinPixel[i] * dhf) - 10);
                    SzofRt = new Size(20, 20);//定义画红框的大小，宽20高20
                    Rt = new Rectangle(LTPntofRt, SzofRt);
                    using (PenofRt = new Pen(Color.Green, Penwid)) 
                    {
                        g.DrawRectangle(PenofRt, Rt);
                    }
                        
                }
                textBox8.Text = $"{analy.ldlno}";
                textBox9.Text = $"{analy.LdlSerDuty}";
                textBox10.Text = $"{analy.LdlAge}";
                textBox11.Text = $"{analy.Contractor}";
            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            if (bDrawThickAnaly[1])
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
                        else if ((b >= 9) && (b < 26))
                        {
                            b = 15;
                        }
                        else if (b >= 26)
                        {
                            b = 16;
                        }
                        using (bsh = new SolidBrush(cr_thick[b])) 
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
                LTPntofRt = new Point((int)((iMaxThickYbyPixel) * dwf) - 10, (int)(iMaxThickXinPixel * dhf) - 10);
                SzofRt = new Size(20, 20);//定义画红框的大小，宽20高20
                Rt = new Rectangle(LTPntofRt, SzofRt);
                using (PenofRt = new Pen(Color.Green, Penwid)) 
                {
                    g.DrawRectangle(PenofRt, Rt);
                }
            }
        }

        private void picBxThickCirExpand_Click(object sender, EventArgs e)
        {
            if (bDrawThickAnaly[0]) 
            {
                Point p1 = MousePosition;
                Point p2 = picBxThickCirExpand.PointToClient(p1);

                double deltaX = (double)(picBxThickCirExpand.Width * 1.0f / 360);
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
            }
            
        }

        private void picBxThickBot_Paint(object sender, PaintEventArgs e)
        {
            if (bDrawThickAnaly[1]) 
            {
                Graphics g = e.Graphics;
                g.Clear(Color.White);
                if (bDrawThickAnaly[1])
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
                            else if ((b >= 9) && (b < 26))
                            {
                                b = 15;
                            }
                            else if (b >= 26)
                            {
                                b = 16;
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
                    float Penwid = (float)4.1;
                    LTPntofRt = new Point((int)((iMaxThickYbyPixel) * dwf) - 10, (int)(iMaxThickXinPixel * dhf) - 10);
                    LTPtoSignal = new Point((int)((iMaxThickYbyPixel) * dwf), (int)(iMaxThickXinPixel * dhf) + 12);
                    SzofRt = new Size(20, 20);//定义画红框的大小，宽20高20
                    Rt = new Rectangle(LTPntofRt, SzofRt);
                    using (PenofRt = new Pen(Color.Green, Penwid))
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
                    using (f = new Font("黑体", 8))
                    {
                        g.DrawString("(" + Math.Round((rad + 0.5) * 9.7, 1) + "mm," + max + "mm," + (col+1) + "°)", f, Brushes.White, LTPtoSignal);
                    }
                }
            }
        }
        PosiPoint point = new PosiPoint();
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (bDrawThickAnaly[0]) 
            {
                bPicBox[0] = true; bPicBox[1] = false; 
                Point p1 = MousePosition;
                Point p2 = pictureBox1.PointToClient(p1);
                double deltaX = (double)(pictureBox1.Width * 1.0f / 360);
                int X = (int)(Math.Floor(p2.X / f_dwf));
                int Y = (int)(Math.Floor(p2.Y / f_dhf));
                point.cols = $"col{X+1}";
                point.rowid = Y+1;

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
                textBox5.Text = "" + thickvalue + "mm";
                textBox6.Text = "" + Y * 16.6 + " - " + (Y + 1) * 16.6 + "mm";
                textBox7.Text = "" + X_p + "°";
                if (radioButton1.Checked == true)
                {

                }
                if (radioButton2.Checked == true)
                {
                    textBox2.Text = "" + thickvalue + "mm";
                    textBox3.Text = "" + thickvalue + "mm";
                    textBox4.Text = "" + thickvalue + "mm";
                }
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (bDrawThickAnaly[1]) 
            {

                bPicBox[0] = false; bPicBox[1] = true;
                Point p1 = MousePosition;
                Point p2 = pictureBox2.PointToClient(p1);
                int X = (int)(Math.Floor(p2.X / s_dwf));
                int Y = (int)(Math.Floor(p2.Y / s_dhf));
                point.cols = $"col{X+1}";
                point.rowid = Y+1;
                double dg = (double)(360.0f / 360);  //角度间隔度数


                double minr = (double)(pictureBox2.Width);
                if (minr > (double)pictureBox2.Height)
                    minr = (double)pictureBox2.Height;
                double dr = (double)((minr / 2) / (ThickBotCols / 2)); //半径间隔大小


                int row = (int)Math.Floor((Math.Sqrt((p2.X - (pictureBox2.Width / 2.0)) * (p2.X - (pictureBox2.Width / 2.0)) + (p2.Y - (pictureBox2.Height / 2.0)) * (p2.Y - (pictureBox2.Height / 2.0)))) / dr);
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
                textBox12.Text = "" + thickvalue + "mm";
                textBox14.Text = "" + row * 9.7 + "mm";
                textBox13.Text = "" + (col + 1) + "°";
                if (radioButton1.Checked == true)
                {

                }
                if (radioButton2.Checked == true)
                {
                    textBox2.Text = "" + thickvalue + "mm";
                    textBox3.Text = "" + thickvalue + "mm";
                    textBox4.Text = "" + thickvalue + "mm";
                }
                
            }
        }

        struct thickInfo 
        {
            public string tableName;
            public int id;
            public List<int> allage;
        }
        struct findinfo 
        {
            public string tablename;
            public int LadleServDuty;
            public int id;
            public List<double> erode;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            //区域统计周壁
            if ((radioButton1.Checked == true) && (bPicBox[0] == true) && (bPicBox[1] == false))
            {

            }
            //底部
            if ((radioButton1.Checked == true) && (bPicBox[0] == false) && (bPicBox[1] == true))
            {

            }
            //单点统计周壁
            if ((radioButton2.Checked == true) && (bPicBox[0] == true) && (bPicBox[1] == false)) 
            {
                thickInfo info = new thickInfo();
                info.tableName = $"{analy.ldlno}_thick";
                info.id = analy.LdlSerDuty;
                info.allage = new List<int>();
                info.allage.Clear();
                connMySql = new MySqlConnection(myconnection);
                try
                {
                    if (connMySql.State == ConnectionState.Closed)
                    {
                        connMySql.Open();
                    }     //打开数据库
                    string SQL = "select * from "+ info.tableName + " where Id="+info.id+" ;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds,""+ info.tableName +"");
                    for (int i = 3; i <= 213; i++) 
                    {
                        if (ds.Tables[0].Rows[0][i] != DBNull.Value) 
                        {
                            info.allage.Add(Convert.ToInt32(ds.Tables[0].Rows[0][i]));
                        }
                    }
                    findinfo finfo = new findinfo();
                    finfo.tablename= $"{analy.ldlno}_cirthick";
                    finfo.LadleServDuty= analy.LdlSerDuty % 3;
                    finfo.erode = new List<double>();
                    finfo.erode.Clear();
                    foreach (int m in info.allage) 
                    {
                        finfo.id = (finfo.LadleServDuty - 1) * 220 * ThickCirRows + m * ThickCirRows + point.rowid;
                        string sql1 = "select "+point.cols+" from "+ finfo.tablename + " where Id="+finfo.id+";";
                        MySqlCommand thisCommand1 = new MySqlCommand(sql1, connMySql);
                        if (Convert.ToInt32(thisCommand1.ExecuteScalar()) != 404)
                        {
                            finfo.erode.Add(Math.Round(Convert.ToInt32(thisCommand1.ExecuteScalar())*1.00/m,2));
                        }
                        else if (Convert.ToInt32(thisCommand1.ExecuteScalar()) == 404) 
                        {
                            finfo.erode.Add(0);
                        }
                    }
                    connMySql.Close();
                    label12.Text= $"空包{analy.ldlno}的侵蚀量速率随包龄变化趋势图";
                    label12.Visible = true;
                    chart1.ChartAreas[0].AxisX.Title = "包龄";
                    chart1.ChartAreas[0].AxisY.Title = "侵蚀速率 （mm/包龄）";
                    chart1.Series[0].LegendText = "侵蚀速率（mm/包龄）";
                    chart1.Series[0].IsValueShownAsLabel = true;
                    chart1.Series[0].Points.DataBindXY(info.allage,finfo.erode);
                }
                catch (Exception ee)
                {
                    MessageBox.Show("查询数据失败！" + ee.ToString());
                }

            }
            //底部
            if ((radioButton2.Checked == true) && (bPicBox[0] == false) && (bPicBox[1] == true))
            {
                thickInfo info = new thickInfo();
                info.tableName = $"{analy.ldlno}_thick";
                info.id = analy.LdlSerDuty;
                info.allage = new List<int>();
                info.allage.Clear();
                connMySql = new MySqlConnection(myconnection);
                try
                {
                    if (connMySql.State == ConnectionState.Closed)
                    {
                        connMySql.Open();
                    }     //打开数据库
                    string SQL = "select * from " + info.tableName + " where Id=" + info.id + " ;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "" + info.tableName + "");
                    for (int i = 3; i <= 213; i++)
                    {
                        if (ds.Tables[0].Rows[0][i] != DBNull.Value)
                        {
                            info.allage.Add(Convert.ToInt32(ds.Tables[0].Rows[0][i]));
                        }
                    }
                    findinfo finfo = new findinfo();
                    finfo.tablename = $"{analy.ldlno}_botthick";
                    finfo.LadleServDuty = analy.LdlSerDuty % 3;
                    finfo.erode = new List<double>();
                    finfo.erode.Clear();
                    foreach (int m in info.allage)
                    {
                        finfo.id = (finfo.LadleServDuty - 1) * 220 * ThickBotRows + m * ThickBotRows + point.rowid;
                        string sql1 = "select " + point.cols + " from " + finfo.tablename + " where Id=" + finfo.id + ";";
                        MySqlCommand thisCommand1 = new MySqlCommand(sql1, connMySql);
                        if (Convert.ToInt32(thisCommand1.ExecuteScalar()) != 404)
                        {
                            finfo.erode.Add(Math.Round(Convert.ToInt32(thisCommand1.ExecuteScalar())*1.00/m,2));
                        }
                        else if (Convert.ToInt32(thisCommand1.ExecuteScalar()) == 404)
                        {
                            finfo.erode.Add(0);
                        }
                    }
                    connMySql.Close();
                    label12.Text = $"空包{analy.ldlno}的侵蚀速率随包龄变化趋势图";
                    label12.Visible = true;
                    chart1.ChartAreas[0].AxisX.Title = "包龄";
                    chart1.ChartAreas[0].AxisY.Title = "侵蚀速率（mm/包龄）";
                    chart1.Series[0].LegendText = "侵蚀速率（mm/包龄）";
                    chart1.Series[0].IsValueShownAsLabel = true;
                    chart1.Series[0].Points.DataBindXY(info.allage, finfo.erode);
                }
                catch (Exception ee)
                {
                    MessageBox.Show("查询数据失败！" + ee.ToString());
                }
            }
        }

        private void picBxThickCirExpand_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            if (bDrawThickAnaly[0]) 
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
                        else if ((b >= 9) && (b < 26))
                        {
                            b = 15;
                        }
                        else if (b >= 26)
                        {
                            b = 16;
                        }

                        bsh = new SolidBrush(cr_thick[b]);

                        g.FillRectangle(bsh, tmprtf);
                    }
                }
                double deltaX = (double)(picBxThickCirExpand.Width * 1.0f / 360);
                for (int i = 0; i < 4; i++)
                {
                    Point LTPntofRt,LTPtoSignal;
                    Pen PenofRt;
                    Size SzofRt;
                    Rectangle Rt;
                    Font f;
                    float Penwid = (float)4.1;
                    LTPntofRt = new Point((int)((iMaxThickYbyPixel[i]) * dwf) - 10, (int)(iMaxThickXinPixel[i] * dhf) - 10);
                    LTPtoSignal = new Point((int)((iMaxThickYbyPixel[i]) * dwf), (int)(iMaxThickXinPixel[i] * dhf) + 12);
                    SzofRt = new Size(20, 20);//定义画红框的大小，宽20高20
                    Rt = new Rectangle(LTPntofRt, SzofRt);
                    using (PenofRt = new Pen(Color.Green, Penwid))
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
                    using (f = new Font("黑体", 10))
                    {
                        g.DrawString("(" + Math.Round((iMaxThickXinPixel[i] + 0.5) * 16.6, 1) + "mm," + max[i] + "mm," + X_p + "°)", f, Brushes.White, LTPtoSignal);
                    }
                }
            }
        }

        struct AnalyInfo 
        {
            public string ldlno;
            public int LdlSerDuty;
            public int LdlAge;
            public string  Contractor;
        }
        AnalyInfo analy = new AnalyInfo();
        private void dataGridView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string SerNo = dataGridView1.CurrentRow.Cells["序号"].Value.ToString();
            analy.ldlno = dataGridView1.CurrentRow.Cells["包号"].Value.ToString();
            analy.LdlSerDuty = Convert.ToInt32(dataGridView1.CurrentRow.Cells["包役"].Value.ToString());
            analy.LdlAge= Convert.ToInt32(dataGridView1.CurrentRow.Cells["包龄"].Value.ToString());
            analy.Contractor = dataGridView1.CurrentRow.Cells["砌筑商"].Value.ToString();
            if (SerNo != "")
            {
                ElID = Convert.ToInt32(SerNo);

            }
            ShowthickImage();
        }

        bool ReadBotData(string txtname)
        {
            //if (mData != null) 
            {
                //  mData=new Dictionary<uint, Dictionary<uint, float>>();
            }//

            string fileName = GetResourcePath(txtname);
            using (StreamReader reader = File.OpenText(fileName))
            {
                mData1 = new Dictionary<uint, Dictionary<uint, float>>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var items = line.Split('\t');
                    if (items.Length != 3)
                    {
                        continue;
                    }

                    uint x1 = uint.Parse(items[0]);
                    uint y1 = uint.Parse(items[1]);
                    float z1 = float.Parse(items[2]);

                    if (mMinX1 > x1) mMinX1 = x1;
                    if (mMaxX1 < x1) mMaxX1 = x1;
                    if (mMinY1 > y1) mMinY1 = y1;
                    if (mMaxY1 < y1) mMaxY1 = y1;

                    Dictionary<uint, float> yData1 = null;
                    if (!mData1.TryGetValue(x1, out yData1))
                    {
                        yData1 = new Dictionary<uint, float>();
                        mData1[x1] = yData1;
                    }

                    yData1[y1] = z1;
                }
            }

            return true;
        }
       
        struct PosiPoint 
        {
            public string cols;
            public int rowid;
        }
        
    }
}
