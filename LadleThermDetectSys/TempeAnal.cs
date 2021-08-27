using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace LadleThermDetectSys
{
    public partial class TempeAnal : Form
    {
        AutoResizeForm asc = new AutoResizeForm();
        MySqlConnection connMySql;
        //string myconnection = "user id=root;password=ARIMlab2020.07.22;server=localhost;persistsecurityinfo=True;database=ironldlthermodetectmanactrl;port=3306;SslMode=None;allowPublicKeyRetrieval=true";
        //string myconnection ="user id=tbrj;password=tbrj;server=10.99.24.144;persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true";
        string myconnection = "user id = test; password = test; server = 192.168.2.100; persistsecurityinfo = True; database = ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true";
        private int TbID;
        private int rowsCount;
        private int index;//记录正处于哪个tabControlindex
        private int id_analysis;//据路正被分析的id
        private static int TempCirRows = 310, TempCirCols = 360;
        private static int TempBotRows =190, TempBotCols=190;
        double g_dwf, g_dhf, g_bwf, g_bhf;

        Int16[] dataCir = new Int16[TempCirRows * TempCirCols];
        Int16[] dataBot = new Int16[TempBotRows * TempBotCols];

        bool[] bDrawTemp = new bool[2] { false,false };
        bool[] bPicBox = new bool[2] { false, false };

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

        struct AnalyInfo
        {
            public string ldlno;
            public int LdlSerDuty;
            public int LdlAge;
            public string Contractor;
            public int MaxTemp;
        }
        AnalyInfo analy = new AnalyInfo();

        //D:\WechatFile\WeChat Files\wxid_5wq7gywoyb0n21\FileStorage\File\2020-10
        /* int[] abc=new int[500000];
         List<string> list = new List<string>();
         public void readfile(string filepath)
         {
             if (!File.Exists(filepath))
             {
                 MessageBox.Show(" file not exits !");
                 return;
             }

             FileStream fs = new FileStream(filepath, FileMode.Open);
             using (StreamReader sr = new StreamReader(fs, Encoding.Default))
             {

             List<string> list = new List<string>();
             //List<int> list = new List<int>();
             int i = 0;

                 while (!sr.EndOfStream)
                 {
                     double a;

                     double.TryParse(sr.ReadLine(), out a);
                     //input[i] = Math.Round(a,0);
                     //list.Add(sr.ReadLine().ToString());
                     //list.Add(Convert.ToInt32(sr.ReadLine().ToString()));
                     double b = Math.Round(a, 0, MidpointRounding.AwayFromZero);

                     abc[i] = Convert.ToInt32(b);
                     i++;
                 }
                 sr.Close();
             }
         }*/
        int iDiv = 10;
        public TempeAnal()
        {
            InitializeComponent();
        }
        private void TempeAnal_Load(object sender, EventArgs e)
        {
            asc.controllInitializeSize(this);
            //readfile("D:\\ak.txt");
            connMySql = new MySqlConnection(myconnection);
            if (connMySql.State == ConnectionState.Closed)
            {
                connMySql.Open();
            }
            try
            {
                if (TabCtrlTemp.SelectedIndex == 0)
                {
                    lblNotion.Visible = false;
                    lblbarProgress.Visible = false;
                    progressBar1.Visible = false;
                    index = 0;
                    rowsCount = 0;
                    //数据表的测量时间降序排列取前10行，显示部分列到界面
                    string SQL = "select Analysis,id, LadleNo,LadleServDuty, LadleAge,LadleContractor, MeasTm, MaxTemp,MaxTempPos from heavyldl_meased where `Delete`=0 and Analysis=0 order by id desc limit 0,8000";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    dtaGridVHeavy_Meased.AutoGenerateColumns = false;
                    objDataAdpter.Fill(ds, "heavyldl_meased");
                    dtaGridVHeavy_Meased.DataSource = ds.Tables[0];
                    rowsCount = ds.Tables[0].Rows.Count;
                    if (rowsCount > 0)
                    {
                        TbID = Convert.ToInt32(ds.Tables[0].Rows[0][1]);
                    }
                    else if (rowsCount == 0)
                    {
                        lblNotion.Visible = true;
                    }
                }
                if (TabCtrlTemp.SelectedIndex == 1)
                {
                    label4.Visible = false;
                    index = 1;
                    rowsCount = 0;
                    //数据表的测量时间降序排列取前10行，显示部分列到界面
                    string SQL = "select id 序号,LadleNo 包号,LadleServDuty 包役,LadleAge 包龄,LadleContractor 砌筑商, MeasTm 测量时间, MaxTemp 最高温度 from heavyldl_meased where `Delete`=0 and Analysis=1 order by id desc limit 0,8000";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    dtaGridVHeavy_Meased.AutoGenerateColumns = false;
                    objDataAdpter.Fill(ds, "heavyldl_meased");
                    dataGridView1.DataSource = ds.Tables[0];
                    rowsCount = ds.Tables[0].Rows.Count;
                    if (rowsCount > 0)
                    {
                        TbID = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                        analy.ldlno = ds.Tables[0].Rows[0][1].ToString();
                        analy.LdlSerDuty = Convert.ToInt32(ds.Tables[0].Rows[0][2]);
                        analy.LdlAge = Convert.ToInt32(ds.Tables[0].Rows[0][3]);
                        analy.Contractor = ds.Tables[0].Rows[0][4].ToString();
                        analy.MaxTemp = Convert.ToInt32(ds.Tables[0].Rows[0][6]);
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
            ShowtempPic();
        }

        private void TabCtrlTemp_SelectedIndexChanged(object sender, EventArgs e)
        {
            connMySql = new MySqlConnection(myconnection);
            if (connMySql.State == ConnectionState.Closed)
            {
                connMySql.Open();
            }
            try
            {
                if (TabCtrlTemp.SelectedIndex == 0)
                {
                    lblNotion.Visible = false;
                    lblbarProgress.Visible = false;
                    progressBar1.Visible = false;
                    index = 0;
                    TbID = 0;
                    rowsCount = 0;
                    //数据表的测量时间降序排列取前10行，显示部分列到界面
                    string SQL = "select Analysis,id, LadleNo,LadleServDuty, LadleAge,LadleContractor, MeasTm, MaxTemp,MaxTempPos from heavyldl_meased where `Delete`=0 and Analysis=0 order by id desc limit 0,8000;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    dtaGridVHeavy_Meased.AutoGenerateColumns = false;
                    objDataAdpter.Fill(ds, "heavyldl_meased");
                    dtaGridVHeavy_Meased.DataSource = ds.Tables[0];
                    rowsCount = ds.Tables[0].Rows.Count;
                    if (rowsCount > 0)
                    {
                        TbID = Convert.ToInt32(ds.Tables[0].Rows[0][1]);
                    }
                    else if (rowsCount == 0)
                    {
                        lblNotion.Visible = true;
                    }
                }
                if (TabCtrlTemp.SelectedIndex == 1)
                {
                    comboBox1.Enabled = false;
                    label4.Visible = false;
                    index = 1;
                    TbID = 0;
                    rowsCount = 0;
                    //数据表的测量时间降序排列取前10行，显示部分列到界面
                    string SQL = "select id 序号,LadleNo 包号,LadleServDuty 包役,LadleAge 包龄,LadleContractor 砌筑商, MeasTm 测量时间, MaxTemp 最高温度 from heavyldl_meased where `Delete`=0 and Analysis=1 order by id desc limit 0,8000";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    dtaGridVHeavy_Meased.AutoGenerateColumns = false;
                    objDataAdpter.Fill(ds, "heavyldl_meased");
                    dataGridView1.DataSource = ds.Tables[0];
                    rowsCount = ds.Tables[0].Rows.Count;
                    if (rowsCount > 0)
                    {
                        TbID = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                        analy.ldlno = ds.Tables[0].Rows[0][1].ToString();
                        analy.LdlSerDuty = Convert.ToInt32(ds.Tables[0].Rows[0][2]);
                        analy.LdlAge = Convert.ToInt32(ds.Tables[0].Rows[0][3]);
                        analy.Contractor = ds.Tables[0].Rows[0][4].ToString();
                        analy.MaxTemp = Convert.ToInt32(ds.Tables[0].Rows[0][6]);
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
            ShowtempPic();
        }


        private void TempeReadBlob()
        {
            // blBotTemp = true;

            string timenow1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //label1.Text = timenow1;
            try
            {
                connMySql = new MySqlConnection(myconnection);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }
                bDrawTemp[0] = false; bDrawTemp[1] = false;
                string SQL = "SELECT  CircumExpandTemp,BottomExpandTemp  FROM heavyldl_meased where id=" + TbID + "";
                //MySqlCommand cmd = new MySqlCommand(SQL, connMySql);
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "heavyldl_meased");
                connMySql.Close();
                //MySqlDataReader rdr = cmd.ExecuteReader();

                //while (rdr.Read())
                {
                    bDrawTemp[0] = true; bDrawTemp[1] = true;
                    if (ds.Tables[0].Rows[0][0] == System.DBNull.Value)
                    {
                        bDrawTemp[0] = false;
                    }
                    if (ds.Tables[0].Rows[0][1] == System.DBNull.Value)
                    {
                        bDrawTemp[1] = false;
                    }

                    if (bDrawTemp[0]) 
                    {
                        
                        int inlen = TempCirRows * TempCirCols * 2;
                        byte[] buffer = new byte[inlen];


                        //存放获得的二进制数据，温度
                        buffer = (byte[])ds.Tables[0].Rows[0][0];

                        //FileStream fs = new FileStream("D:\\周壁温度.txt", FileMode.Append, FileAccess.Write);
                        //StreamWriter sw = new StreamWriter(fs);
                        for (int i = 0; i < inlen / 2; i++)
                        {
                            dataCir[i] = (Int16)((buffer[2 * i]) + (buffer[2 * i + 1]) * 256);
                            //获得字节数组
                           //string content = dataCir[i].ToString();
                            //开始写入
                            //sw.WriteLine(content);
                            //清空缓冲区、关闭流
                            //sw.Flush();
                            
                        }
                       // fs.Close();
                    }

                    if (bDrawTemp[1]) 
                    {
                        
                        int inlen1 = TempBotRows * TempBotCols * 2;
                        byte[] buffer1 = new byte[inlen1];


                        //存放获得的二进制数据，温度
                        buffer1 = (byte[])ds.Tables[0].Rows[0][1];
                        for (int i = 0; i < inlen1 / 2; i++)
                        {
                            dataBot[i] = (Int16)((buffer1[2 * i]) + (buffer1[2 * i + 1]) * 256);
                        }
                    }
                }

                
            }
            catch (Exception EE)
            {
                //				lblQuerry.Text = "查询数据库失败!";
                MessageBox.Show("查询数据失败！" + EE.ToString());
            }
        }


       

        private void ShowtempPic()
        {
            TempeReadBlob();
            if (TabCtrlTemp.SelectedIndex == 0) 
            {
                picBxCircumExpandTemp.Refresh();
                picBxBottomExpandTemp.Refresh();
            }
            if (TabCtrlTemp.SelectedIndex == 1) 
            {
                pictureBox1.Refresh();
                pictureBox2.Refresh();
            }
        }

        

        private void dtaGridVHeavy_Meased_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void dtaGridVHeavy_Meased_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string SerNo = dtaGridVHeavy_Meased.CurrentRow.Cells["tempid"].Value.ToString();
            if (SerNo != "")
            {
                TbID = Convert.ToInt32(SerNo);
            }
            ShowtempPic();
        }

        private void dtaGridVHeavy_Meased_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            connMySql = new MySqlConnection(myconnection);
            try
            {
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }
                DataGridViewCell cell = dtaGridVHeavy_Meased.Rows[e.RowIndex].Cells[e.ColumnIndex];
                TbID = int.Parse(dtaGridVHeavy_Meased.Rows[e.RowIndex].Cells["tempid"].Value.ToString());
                if (cell.FormattedValue.ToString() == "删除")
                {
                    if (MessageBox.Show("是否删除此次测量数据？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    {
                        string sql = "update heavyldl_meased set `Delete`=b'1' where id=" + TbID + ";";
                        MySqlCommand thisCommand = new MySqlCommand(sql, connMySql);
                        thisCommand.ExecuteNonQuery();
                        rowsCount = 0;
                        TbID = 0;
                        string SQL = "select Analysis,id, LadleNo,LadleServDuty, LadleAge,LadleContractor, MeasTm, MaxTemp,MaxTempPos from heavyldl_meased where `Delete`=0 and Analysis=0 order by id desc limit 0,8000;";
                        MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                        objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                        DataSet ds = new DataSet();
                        dtaGridVHeavy_Meased.AutoGenerateColumns = false;
                        objDataAdpter.Fill(ds, "heavyldl_meased");
                        dtaGridVHeavy_Meased.DataSource = ds.Tables[0];
                        rowsCount = ds.Tables[0].Rows.Count;
                        if (rowsCount > 0)
                        {
                            TbID = Convert.ToInt32(ds.Tables[0].Rows[0][1]);
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
                MessageBox.Show("删除数据出错！" + ee.ToString());
            }
            ShowtempPic();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label4.Visible = false;
            connMySql = new MySqlConnection(myconnection);
            try
            {
                string inputLadleNo = this.textBox1.Text.ToString();
                string Time1 = dateTimePicker2.Value.ToString();
                string Time2 = dateTimePicker1.Value.ToString();
                string SQL;
                TbID = 0;
                rowsCount = 0;
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }     //打开数据库
                if (inputLadleNo != "")
                {
                    SQL = "select id 序号, LadleNo 包号,LadleServDuty 包役,LadleAge 包龄,LadleContractor 砌筑商, MeasTm '测量时间', MaxTemp '最高温度' ,MaxTempPos '区域' from heavyldl_meased where `Delete`=0 and Analysis=1 and LadleNo='" + inputLadleNo + "' and MeasTm >='" + Time1 + "' and MeasTm <='" + Time2 + "';";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "heavyldl_meased");
                    dataGridView1.DataSource = ds.Tables[0];
                    rowsCount = ds.Tables[0].Rows.Count;
                    if (rowsCount > 0)
                    {
                        TbID = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                        analy.ldlno = ds.Tables[0].Rows[0][1].ToString();
                        analy.LdlSerDuty = Convert.ToInt32(ds.Tables[0].Rows[0][2]);
                        analy.LdlAge = Convert.ToInt32(ds.Tables[0].Rows[0][3]);
                        analy.Contractor = ds.Tables[0].Rows[0][4].ToString();
                        analy.MaxTemp = Convert.ToInt32(ds.Tables[0].Rows[0][6]);
                    }
                    else if (rowsCount == 0)
                    {
                        label4.Visible = true;
                    }
                }
                else
                {
                    SQL = "select id 序号, LadleNo 包号,LadleServDuty 包役, LadleAge 包龄,LadleContractor 砌筑商, MeasTm '测量时间', MaxTemp '最高温度' ,MaxTempPos '区域' from heavyldl_meased where `Delete`=0 and Analysis=1 and MeasTm >='" + Time1 + "' and MeasTm<='" + Time2 + "'";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "heavyldl_meased");
                    dataGridView1.DataSource = ds.Tables[0];
                    rowsCount = ds.Tables[0].Rows.Count;
                    if (rowsCount > 0)
                    {
                        TbID = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                        analy.ldlno = ds.Tables[0].Rows[0][1].ToString();
                        analy.LdlSerDuty = Convert.ToInt32(ds.Tables[0].Rows[0][2]);
                        analy.LdlAge = Convert.ToInt32(ds.Tables[0].Rows[0][3]);
                        analy.Contractor = ds.Tables[0].Rows[0][4].ToString();
                        analy.MaxTemp= Convert.ToInt32(ds.Tables[0].Rows[0][6]);
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
            ShowtempPic();
        }

        private List<int> serno = new List<int>();
        private void dtaGridVHeavy_Meased_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dtaGridVHeavy_Meased.Rows.Count > 0)
            {
                int ck_status = int.Parse(dtaGridVHeavy_Meased.CurrentRow.Cells["analysis"].Value.ToString());
                int SerNo = int.Parse(dtaGridVHeavy_Meased.CurrentRow.Cells["tempid"].Value.ToString());
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

        private void dtaGridVHeavy_Meased_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            //当前单元格是否有未提交的更改
            if (dtaGridVHeavy_Meased.IsCurrentCellDirty)
            {
                dtaGridVHeavy_Meased.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private struct storeinfo
        {
            public string LadleNo;
            public int LadleServDuty;
            public int LadleAge;
            public string Contractor;
            public Int16[] dataCircum;
            public Int16[] dataBottom;
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

        private struct temp_age
        {
            public string tablename;
            public int Startid;
            public string Ageid;
        }

        private struct temp_analy_history
        {
            public string LadleServDuty;
            public string LadleAge;
            public string Time;
        }


        private void BUTNoAnalysis_Click(object sender, EventArgs e)
        {
            BUTNoAnalysis.Enabled = false;
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
                    if (dtaGridVHeavy_Meased.Rows.Count == 0)
                    {
                        MessageBox.Show("没有测厚数据待分析！");
                    }
                    if (dtaGridVHeavy_Meased.Rows.Count > 0)
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
                        string sql1 = "select LadleNo,LadleServDuty,LadleAge,LadleContractor from heavyldl_meased where id=" + id_analysis + ";";
                        MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                        objDataAdpter.SelectCommand = new MySqlCommand(sql1, connMySql);
                        DataSet ds = new DataSet();
                        objDataAdpter.Fill(ds, "heavyldl_meased");
                        storeinfo mge = new storeinfo();
                        mge.LadleNo = ds.Tables[0].Rows[0][0].ToString();
                        mge.LadleServDuty = Convert.ToInt32(ds.Tables[0].Rows[0][1]);
                        mge.LadleAge = Convert.ToInt32(ds.Tables[0].Rows[0][2]);
                        mge.Contractor = ds.Tables[0].Rows[0][3].ToString();
                        string SQL = "SELECT  CircumExpandTemp,BottomExpandTemp  FROM heavyldl_meased where id ='" + id_analysis + "';";
                        MySqlCommand cmd = new MySqlCommand(SQL, connMySql);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            long len;
                            int inlen = TempCirRows * TempCirCols * 2;
                            byte[] buffer = new byte[inlen];

                            len = rdr.GetBytes(0, 0, buffer, 0, inlen);
                            for (int i = 0; i < len / 2; i++)
                            {
                                dataCir[i] = (Int16)((buffer[2 * i]) + (buffer[2 * i + 1]) * 256);
                            }
                            long len1;
                            int inlen1 = TempBotRows * TempBotCols * 2;
                            byte[] buffer1 = new byte[inlen1];
                            //存放获得的二进制数据，温度
                            len1 = rdr.GetBytes(1, 0, buffer1, 0, inlen1);
                            for (int i = 0; i < len1 / 2; i++)
                            {
                                dataBot[i] = (Int16)((buffer1[2 * i]) + (buffer1[2 * i + 1]) * 256);
                            }
                        }
                        rdr.Close();
                        mge.dataCircum = dataCir;
                        mge.dataBottom = dataBot;
                        progressBar1.Value = Convert.ToInt32(delta_probar * (m + 0.1));
                        //将此数据存入历史分析表中
                        parainfo para = new parainfo();
                        para.tablenameCir = $"{mge.LadleNo}_cirtemp";
                        para.tablenameBot = $"{mge.LadleNo}_bottemp";
                        int ladleServDuty_temp = mge.LadleServDuty % 3;
                        para.LadleServDuty=(ladleServDuty_temp==0)?3:ladleServDuty_temp;
                        para.LadleAge = mge.LadleAge;
                        para.startidCir = (para.LadleServDuty - 1) * 220 * TempCirRows + mge.LadleAge * TempCirRows + 1;
                        para.startidBot = (para.LadleServDuty - 1) * 220 * TempBotRows + mge.LadleAge * TempBotRows + 1;
                        para.addsql = new StringBuilder();
                        progressBar1.Value = Convert.ToInt32(delta_probar * (m + 0.2));
                        for (int i = 0; i < TempCirRows; i++)
                        {
                            for (int t = 0; t < TempCirCols; t++)
                            {
                                para.addsql.Append(",deg" + (t + 1) + "=" + mge.dataCircum[t* TempCirRows+i] + "");
                            }
                            string sql2 = "update " + para.tablenameCir + " set LadleServDuty=" + para.LadleServDuty + ",UpdateTime='" + DateTime.Now.ToString() + "'" + para.addsql + " where Id=" + (para.startidCir + i) + ";";
                            MySqlCommand thisCommand2 = new MySqlCommand(sql2, connMySql);
                            thisCommand2.ExecuteNonQuery();
                            para.addsql.Clear();
                        }
                        progressBar1.Value = Convert.ToInt32(delta_probar * (m + 0.6));
                        for (int i = 0; i < TempBotRows; i++)
                        {
                            for (int t = 0; t < TempBotCols; t++)
                            {
                                para.addsql.Append(",col" + (t + 1) + "=" + mge.dataBottom[TempBotRows * (TempBotRows - 1 - i) + t] + "");
                            }
                            string sql3 = "update " + para.tablenameBot + " set LadleServDuty=" + para.LadleServDuty + ",UpdateTime='" + DateTime.Now.ToString() + "'" + para.addsql + " where Id=" + (para.startidBot + i) + ";";
                            MySqlCommand thisCommand3 = new MySqlCommand(sql3, connMySql);
                            thisCommand3.ExecuteNonQuery();
                            para.addsql.Clear();
                        }
                        progressBar1.Value = Convert.ToInt32(delta_probar * (m + 0.85));
                        temp_age ageMeg = new temp_age();
                        ageMeg.tablename = $"{mge.LadleNo}_temp";
                        ageMeg.Startid = mge.LadleServDuty;
                        ageMeg.Ageid = $"Age{mge.LadleAge}";
                        string sql5 = "UPDATE " + ageMeg.tablename + " SET " + ageMeg.Ageid + "=" + mge.LadleAge + " WHERE Id=" + ageMeg.Startid + ";";
                        MySqlCommand thisCommand5 = new MySqlCommand(sql5, connMySql);
                        thisCommand5.ExecuteNonQuery();
                        progressBar1.Value = Convert.ToInt32(delta_probar * (m + 0.9));
                        //将此记录存入表thick_analy_history
                        temp_analy_history analy = new temp_analy_history();
                        analy.LadleServDuty = $"LadleServDuty0{para.LadleServDuty}";
                        analy.LadleAge = $"LadleAge0{para.LadleServDuty}";
                        analy.Time = $"Time0{para.LadleServDuty}";
                        string sql4 = "update temp_analy_history set LadleContractor='" + mge.Contractor + "'," + analy.LadleServDuty + "=" + mge.LadleServDuty + "," + analy.LadleAge + "=" + mge.LadleAge + "," + analy.Time + "='" + DateTime.Now.ToString() + "' where LadleNo='" + mge.LadleNo + "';";
                         MySqlCommand thisCommand4 = new MySqlCommand(sql4, connMySql);
                        thisCommand4.ExecuteNonQuery();
                        string sql = "update heavyldl_meased set `Analysis`=b'1' where id=" + id_analysis + ";";
                        MySqlCommand thisCommand = new MySqlCommand(sql, connMySql);
                        thisCommand.ExecuteNonQuery();
                        progressBar1.Value = (int)delta_probar * (m + 1);
                    }
                    serno.Clear();
                    progressBar1.Visible = false;
                    lblbarProgress.Text = "存储已完成！";
                    string SQL1 = "select Analysis,id, LadleNo,LadleServDuty, LadleAge, MeasTm, MaxTemp,MaxTempPos from heavyldl_meased where `Delete`=0 and Analysis=0 order by id desc limit 0,8000;";
                    MySqlDataAdapter objDataAdpter1 = new MySqlDataAdapter();
                    objDataAdpter1.SelectCommand = new MySqlCommand(SQL1, connMySql);
                    DataSet ds1 = new DataSet();
                    dtaGridVHeavy_Meased.AutoGenerateColumns = false;
                    objDataAdpter1.Fill(ds1, "heavyldl_meased");
                    dtaGridVHeavy_Meased.DataSource = ds1.Tables[0];
                    rowsCount = ds1.Tables[0].Rows.Count;
                    if (rowsCount > 0)
                    {
                        TbID = Convert.ToInt32(ds1.Tables[0].Rows[0][1]);
                    }
                    else if (rowsCount == 0)
                    {
                        lblNotion.Visible = true;
                    }
                    if (rowsCount > 0)
                    {
                        ShowtempPic();
                    }
                }
                connMySql.Close();
            }
            catch (Exception n)
            {
                MessageBox.Show("数据库连接出现问题!" + n.ToString());
            }
            lblbarProgress.Visible = false;
            BUTNoAnalysis.Enabled = true;
        }

        private void TempeAnal_SizeChanged(object sender, EventArgs e)
        {
            asc.controlAutoSize(this);
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
                        y = dhf * (m - X-1);
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

        
        

        private void dataGridView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string SerNo = dataGridView1.CurrentRow.Cells["序号"].Value.ToString();
            analy.ldlno = dataGridView1.CurrentRow.Cells["包号"].Value.ToString();
            analy.LdlSerDuty = Convert.ToInt32(dataGridView1.CurrentRow.Cells["包役"].Value.ToString());
            analy.LdlAge = Convert.ToInt32(dataGridView1.CurrentRow.Cells["包龄"].Value.ToString());
            analy.Contractor = dataGridView1.CurrentRow.Cells["砌筑商"].Value.ToString();
            if (SerNo != "")
            {
                TbID = Convert.ToInt32(SerNo);

            }
            ShowtempPic();
        }
        struct PosiPoint
        {
            public string cols;
            public int rowid;
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


        int col;

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
            string thickvalue = Convert.ToString(dataBot[(TempBotRows - Y -1) * TempBotRows + X]);
            
            String str = "钢包底部半径为：" + row * 19.4 + "mm，温度：" + Convert.ToString(dataBot[(TempBotRows - Y) * TempBotRows + X])+ "℃，当前角度：" + (col + 1) + "°";
            txtBxTempPosi.Text = str;
        }


        private void pictureBox1_Paint(object sender, PaintEventArgs e)
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
                textBox8.Text = $"{analy.ldlno}";
                textBox9.Text = $"{analy.LdlSerDuty}";
                textBox10.Text = $"{analy.LdlAge}";
                textBox11.Text = $"{analy.Contractor}";
                textBox15.Text = $"{analy.MaxTemp}℃";
            }
        }

       

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
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
                        y = dhf * (m - X-1);
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

        

        PosiPoint point = new PosiPoint();

       

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (bDrawTemp[0])
            {
                bPicBox[0] = true; bPicBox[1] = false;
                Point p1 = MousePosition;
                Point p2 = pictureBox1.PointToClient(p1);
                double deltaX = (double)(pictureBox1.Width * 1.0f / 360);
                int X = (int)(Math.Floor(p2.X / g_dwf));
                int Y = (int)(Math.Floor(p2.Y / g_dhf));
                point.cols = $"deg{X + 1}";
                point.rowid = Y + 1;

                int X_p = (int)(Math.Floor(p2.X / deltaX));
                if ((0 <= X_p) && (X_p < 180))
                {
                    X_p = X_p - 180;
                }
                else if (X_p >= 180)
                {
                    X_p = X_p - 179;
                }
                string tempvalue = Convert.ToString(dataCir[X * TempCirRows + Y]);
                textBox5.Text = "" + tempvalue + "℃";
                textBox6.Text = "" + Y * 19.3 + " - " + (Y + 1) * 19.3 + "mm";
                textBox7.Text = "" + X_p + "°";
                if (radioButton1.Checked == true)
                {

                }
                if (radioButton2.Checked == true)
                {
                    textBox2.Text = "" + tempvalue + "℃";
                    textBox3.Text = "" + tempvalue + "℃";
                    textBox4.Text = "" + tempvalue + "℃";
                }
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (bDrawTemp[1])
            {

                bPicBox[0] = false; bPicBox[1] = true;
                Point p1 = MousePosition;
                Point p2 = pictureBox2.PointToClient(p1);
                int X = (int)(Math.Floor(p2.X / g_bwf));
                int Y = (int)(Math.Floor(p2.Y / g_bhf));
                point.cols = $"col{X + 1}";
                point.rowid = Y + 1;
                double dg = (double)(360.0f / 360);  //角度间隔度数


                double minr = (double)(pictureBox2.Width);
                if (minr > (double)pictureBox2.Height)
                    minr = (double)pictureBox2.Height;
                double dr = (double)((minr / 2) / (TempBotCols / 2)); //半径间隔大小


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
                string tempvalue = Convert.ToString(dataBot[(TempBotRows - Y-1) * TempBotRows + X]);
                
                textBox12.Text = "" + tempvalue + "℃";
                textBox14.Text = "" + row * 19.4 + "mm";
                textBox13.Text = "" + (col + 1) + "°";
                if (radioButton1.Checked == true)
                {

                }
                if (radioButton2.Checked == true)
                {
                    textBox2.Text = "" + tempvalue + "℃";
                    textBox3.Text = "" + tempvalue + "℃";
                    textBox4.Text = "" + tempvalue + "℃";
                }

            }
        }
        struct tempInfo
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
            public List<int> temp;
        }

        
        private void button1_Click(object sender, EventArgs e)
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
                tempInfo info = new tempInfo();
                info.tableName = $"{analy.ldlno}_temp";
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
                    finfo.tablename = $"{analy.ldlno}_cirtemp";
                    finfo.LadleServDuty = analy.LdlSerDuty % 3;
                    finfo.temp = new List<int>();
                    finfo.temp.Clear();
                    foreach (int m in info.allage)
                    {
                        finfo.id = (finfo.LadleServDuty - 1) * 220 * TempCirRows + m * TempCirRows + point.rowid;
                        string sql1 = "select " + point.cols + " from " + finfo.tablename + " where Id=" + finfo.id + ";";
                        MySqlCommand thisCommand1 = new MySqlCommand(sql1, connMySql);
                        finfo.temp.Add(Convert.ToInt32(thisCommand1.ExecuteScalar()));
  
                    }
                    connMySql.Close();
                    label31.Text = $"重包{analy.ldlno}的外壳一点温度随包龄变化趋势图";
                    label31.Visible = true;
                    chart1.Series[0].Points.Clear();
                    chart1.ChartAreas[0].AxisX.Title = "包龄";
                    chart1.ChartAreas[0].AxisY.Title = "温度 （℃）";
                    chart1.Series[0].LegendText = "温度 （℃）";
                    chart1.Series[0].IsValueShownAsLabel = true;
                    chart1.Series[0].Points.DataBindXY(info.allage, finfo.temp);
                }
                catch (Exception ee)
                {
                    MessageBox.Show("查询数据失败！" + ee.ToString());
                }

            }
            //底部
            if ((radioButton2.Checked == true) && (bPicBox[0] == false) && (bPicBox[1] == true))
            {
                tempInfo info = new tempInfo();
                info.tableName = $"{analy.ldlno}_temp";
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
                    finfo.tablename = $"{analy.ldlno}_bottemp";
                    finfo.LadleServDuty = analy.LdlSerDuty % 3;
                    finfo.temp = new List<int>();
                    finfo.temp.Clear();
                    foreach (int m in info.allage)
                    {
                        finfo.id = (finfo.LadleServDuty - 1) * 220 * TempBotRows + m * TempBotRows + point.rowid;
                        string sql1 = "select " + point.cols + " from " + finfo.tablename + " where Id=" + finfo.id + ";";
                        MySqlCommand thisCommand1 = new MySqlCommand(sql1, connMySql);
                        finfo.temp.Add(Convert.ToInt32(thisCommand1.ExecuteScalar()));
                    }
                    connMySql.Close();
                    label31.Text = $"重包{analy.ldlno}的外壳一点温度随包龄变化趋势图";
                    label31.Visible = true;
                    chart1.Series[0].Points.Clear();
                    chart1.ChartAreas[0].AxisX.Title = "包龄";
                    chart1.ChartAreas[0].AxisY.Title = "温度（℃）";
                    chart1.Series[0].LegendText = "温度（℃）";
                    chart1.Series[0].IsValueShownAsLabel = true;
                    chart1.Series[0].Points.DataBindXY(info.allage, finfo.temp);
                }
                catch (Exception ee)
                {
                    MessageBox.Show("查询数据失败！" + ee.ToString());
                }
            }
        }

        struct Allladle 
        {
            public string ldlServDutyItem;
            public List<string> ladleNo;
            public List<string> ladleContractor;
        }
        struct Ageinfo 
        {
            public List<string> ladleNo;
            public List<string> ladleContractor;
        }

        struct Finalinfo
        {
            public string tablename;
            public int LadleServDuty;
            public int id;
            public List<int> temp;
            public List<int> age;
        }
        Finalinfo finalinfo = new Finalinfo();
        private void button3_Click(object sender, EventArgs e)
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
                chart2.Visible = false;
                chart3.Visible = false;
                comboBox1.Enabled = false;
                comboBox1.Items.Clear();
                Allladle colInfo = new Allladle();
                colInfo.ldlServDutyItem = $"LadleServDuty0{analy.LdlSerDuty % 3}";
                colInfo.ladleNo = new List<string>();
                colInfo.ladleContractor = new List<string>();
                connMySql = new MySqlConnection(myconnection);
                try
                {
                    if (connMySql.State == ConnectionState.Closed)
                    {
                        connMySql.Open();
                    }     //打开数据库
                    string SQL = "select LadleNo,LadleContractor from temp_analy_history where "+ colInfo.ldlServDutyItem + "=" + analy.LdlSerDuty + " ;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "temp_analy_history");
                    int rowcount = ds.Tables[0].Rows.Count;
                    if (rowcount > 0) 
                    {
                        for (int i = 0; i < rowcount; i++) 
                        {
                            colInfo.ladleNo.Add(ds.Tables[0].Rows[i][0].ToString());
                            colInfo.ladleContractor.Add(ds.Tables[0].Rows[i][1].ToString());
                        }
                    }
                    int ldlcount = colInfo.ladleNo.Count;
                    Ageinfo ageInfo = new Ageinfo();
                    ageInfo.ladleNo = new List<string>();
                    ageInfo.ladleContractor = new List<string>();
                    for (int j = 0; j < ldlcount; j++)
                    {
                        string tablename = $"{colInfo.ladleNo[j]}_temp";
                        string agecol = $"Age{analy.LdlAge}";
                        string sql1 = "select " + agecol + " from " + tablename + " where LadleServDuty=" + analy.LdlSerDuty + " ;";
                        MySqlCommand thisCommand1 = new MySqlCommand(sql1, connMySql);
                        if ((thisCommand1.ExecuteScalar() != null) || (thisCommand1.ExecuteScalar() != DBNull.Value)) 
                        {
                            if (thisCommand1.ExecuteScalar().ToString() == "" + analy.LdlAge + "") 
                            {
                                ageInfo.ladleNo.Add(thisCommand1.ExecuteScalar().ToString());
                                ageInfo.ladleContractor.Add(colInfo.ladleContractor[j]);
                            }
                        }
                    }
                    finalinfo.tablename = $"{analy.ldlno}_cirtemp";
                    finalinfo.LadleServDuty = analy.LdlSerDuty % 3;
                    finalinfo.temp = new List<int>();
                    finalinfo.temp.Clear();
                    finalinfo.age = new List<int>();
                    finalinfo.age.Clear();
                    foreach (string m in ageInfo.ladleNo)
                    {
                        finalinfo.id = (finalinfo.LadleServDuty - 1) * 220 * TempCirRows + analy.LdlAge * TempCirRows + point.rowid;
                        string sql1 = "select " + point.cols + " from " + finalinfo.tablename + " where Id=" + finalinfo.id + ";";
                        MySqlCommand thisCommand1 = new MySqlCommand(sql1, connMySql);
                        finalinfo.temp.Add(Convert.ToInt32(thisCommand1.ExecuteScalar()));
                        finalinfo.age.Add(analy.LdlAge);
                    }
                    connMySql.Close();
                    label34.Text = $"重包的外壳一点温度分布图";
                    label34.Visible = true;
                    chart2.ChartAreas[0].AxisX.Title = "包龄";
                    chart2.ChartAreas[0].AxisY.Title = "温度 （℃）";
                    chart2.Series[0].LegendText = "温度 （℃）";
                    chart2.Series[0].IsValueShownAsLabel = true;
                    chart2.Series[0].Label = "#VAL";                //设置显示X Y的值    
                    chart2.Series[0].ToolTip = "#VALX年\r#VAL";
                    chart2.Series[0].Points.Clear();
                    comboBox1.Items.Add("1");
                    for (int m = 0; m < finalinfo.age.Count; m++)
                    {
                        if (ageInfo.ladleContractor[m] == "山西昊业")
                        {
                            chart2.Series[0].Color = Color.Blue;
                            chart2.Series[0].Points.AddXY(finalinfo.age[m], finalinfo.temp[m]);
                        }
                        else if (ageInfo.ladleContractor[m] == "浙江自立")
                        {
                            chart2.Series[0].Color = Color.Green;
                            chart2.Series[0].Points.AddXY(finalinfo.age[m], finalinfo.temp[m]);
                        }
                        else if ((ageInfo.ladleContractor[m] != "山西昊业") && (ageInfo.ladleContractor[m] != "浙江自立"))
                        {
                            chart2.Series[0].Color = Color.Black;
                            chart2.Series[0].Points.AddXY(finalinfo.age[m], finalinfo.temp[m]);
                        }
                        if (m > 0) 
                        {
                            comboBox1.Items.Add(""+(m+1)+"");
                        }
                    }
                    chart2.Visible = true;
                    comboBox1.Enabled = true;
                    /*int tt = Convert.ToInt32(comboBox1.SelectedItem);
                    if (tt > 0) 
                    {
                        int[] count = new int[tt];
                        string[] tempArea = new string[tt];
                        label35.Text = $"重包的外壳一点温度及数量分布图";
                        chart3.ChartAreas[0].AxisX.Title = "温度 （℃）";
                        chart3.ChartAreas[0].AxisY.Title = "数量/个";
                        chart3.Series[0].LegendText = "数量";
                        chart3.Series[0].IsVisibleInLegend = true;  //是否显示数据说明 
                        chart3.Series[0].Points.Clear();
                        int mintemp = finalinfo.temp[0], maxtemp = finalinfo.temp[0];
                        for (int m = 0; m < finalinfo.temp.Count; m++)
                        {
                            if (mintemp > finalinfo.temp[m])
                            {
                                mintemp = finalinfo.temp[m];
                            }
                            if (maxtemp < finalinfo.temp[m])
                            {
                                maxtemp = finalinfo.temp[m];
                            }
                        }
                        for (int m = 0; m < finalinfo.temp.Count; m++)
                        {
                            for (int t = 0; t < tt; t++)
                            {
                                if (((mintemp - 1 + ((maxtemp - mintemp + 2) * t / tt)) <= finalinfo.temp[m]) && (finalinfo.temp[m] < (mintemp - 1 + ((maxtemp - mintemp + 2) * (t + 1) / tt))))
                                {
                                    count[t]++;
                                    int end0 = mintemp - 1 + ((maxtemp - mintemp + 2) * t / tt);
                                    int end1 = mintemp - 1 + ((maxtemp - mintemp + 2) * (t + 1) / tt);
                                    tempArea[t] = $"{end0}到{end1}";
                                }
                            }
                            
                        }
                        chart3.Series[0].Points.Clear();
                        for (int t = 0; t < tt; t++)
                        {
                            chart3.Series[0].Points.AddXY(tempArea[t], count[t]);
                        }
                        label35.Visible = true;
                        chart3.Visible = true;
                    }
                    finalinfo.temp.Clear();
                    finalinfo.age.Clear();*/
                }
                catch (Exception ee)
                {
                    MessageBox.Show("查询数据失败！" + ee.ToString());
                }

            }
            //底部
            if ((radioButton2.Checked == true) && (bPicBox[0] == false) && (bPicBox[1] == true))
            {
                chart2.Visible = false;
                chart3.Visible = false;
                comboBox1.Enabled = false;
                comboBox1.Items.Clear();
                Allladle colInfo = new Allladle();
                colInfo.ldlServDutyItem = $"LadleServDuty0{analy.LdlSerDuty % 3}";
                colInfo.ladleNo = new List<string>();
                colInfo.ladleContractor = new List<string>();
                connMySql = new MySqlConnection(myconnection);
                try
                {
                    if (connMySql.State == ConnectionState.Closed)
                    {
                        connMySql.Open();
                    }     //打开数据库
                    string SQL = "select LadleNo,LadleContractor from temp_analy_history where " + colInfo.ldlServDutyItem + "=" + analy.LdlSerDuty + " ;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "temp_analy_history");
                    int rowcount = ds.Tables[0].Rows.Count;
                    if (rowcount > 0)
                    {
                        for (int i = 0; i < rowcount; i++)
                        {
                            colInfo.ladleNo.Add(ds.Tables[0].Rows[i][0].ToString());
                            colInfo.ladleContractor.Add(ds.Tables[0].Rows[i][1].ToString());//得到此包役的所有重包号和砌筑商
                        }
                    }
                    int ldlcount = colInfo.ladleNo.Count;
                    Ageinfo ageInfo = new Ageinfo();
                    ageInfo.ladleNo = new List<string>();
                    ageInfo.ladleContractor = new List<string>();
                    for (int j = 0; j < ldlcount; j++)
                    {
                        string tablename = $"{colInfo.ladleNo[j]}_temp";
                        string agecol = $"Age{analy.LdlAge}";
                        string sql1 = "select " + agecol + " from " + tablename + " where LadleServDuty=" + analy.LdlSerDuty + " ;";
                        MySqlCommand thisCommand1 = new MySqlCommand(sql1, connMySql);
                        if ((thisCommand1.ExecuteScalar() != null) || (thisCommand1.ExecuteScalar() != DBNull.Value))
                        {
                            if (thisCommand1.ExecuteScalar().ToString() == "" + analy.LdlAge + "")
                            {
                                ageInfo.ladleNo.Add(thisCommand1.ExecuteScalar().ToString());
                                ageInfo.ladleContractor.Add(colInfo.ladleContractor[j]);//根据有相同包役的所有重包来查是否有相同的包龄，来得到具有相同包役和包龄的包号以及砌筑商
                            }
                        }
                    }
                    finalinfo.tablename = $"{analy.ldlno}_bottemp";
                    finalinfo.LadleServDuty = analy.LdlSerDuty % 3;
                    finalinfo.temp = new List<int>();
                    finalinfo.temp.Clear();
                    finalinfo.age = new List<int>();
                    finalinfo.age.Clear();
                    foreach (string m in ageInfo.ladleNo)
                    {
                        finalinfo.id = (finalinfo.LadleServDuty - 1) * 220 * TempBotRows + analy.LdlAge * TempBotRows + point.rowid;
                        string sql1 = "select " + point.cols + " from " + finalinfo.tablename + " where Id=" + finalinfo.id + ";";
                        MySqlCommand thisCommand1 = new MySqlCommand(sql1, connMySql);
                        finalinfo.temp.Add(Convert.ToInt32(thisCommand1.ExecuteScalar()));
                        finalinfo.age.Add(analy.LdlAge);
                    }
                    connMySql.Close();
                    label34.Text = $"重包的外壳一点温度分布图";
                    label34.Visible = true;
                    chart2.ChartAreas[0].AxisX.Title = "包龄";
                    chart2.ChartAreas[0].AxisY.Title = "温度 （℃）";
                    chart2.Series[0].LegendText = "温度 （℃）";
                    chart2.Series[0].IsValueShownAsLabel = true; //是否显示数据      
                    chart2.Series[0].IsVisibleInLegend = true;  //是否显示数据说明 
                    chart2.Series[0].Points.Clear();
                    comboBox1.Items.Add("1");
                    for (int m = 0; m < finalinfo.age.Count; m++)
                    {
                        if (ageInfo.ladleContractor[m] == "山西昊业")
                        {
                            chart2.Series[0].Color = Color.Blue;
                            chart2.Series[0].Points.AddXY(finalinfo.age[m], finalinfo.temp[m]);
                        }
                        else if (ageInfo.ladleContractor[m] == "浙江自立")
                        {
                            chart2.Series[0].Color = Color.Green;
                            chart2.Series[0].Points.AddXY(finalinfo.age[m], finalinfo.temp[m]);
                        }
                        else if ((ageInfo.ladleContractor[m] != "山西昊业") && (ageInfo.ladleContractor[m] != "浙江自立"))
                        {
                            chart2.Series[0].Color = Color.Black;
                            chart2.Series[0].Points.AddXY(finalinfo.age[m], finalinfo.temp[m]);
                        }
                        if (m > 0)
                        {
                            comboBox1.Items.Add(""+(m+1)+"");
                        }
                    }
                    chart2.Visible = true;
                    comboBox1.Enabled = true;
                    /*int tt = Convert.ToInt32(comboBox1.SelectedItem);
                    if (tt > 0)
                    {
                        int[] count = new int[tt];
                        string[] tempArea = new string[tt];
                        label35.Text = $"重包的外壳一点温度及数量分布图";
                        
                        chart3.ChartAreas[0].AxisX.Title = "温度 （℃）";
                        chart3.ChartAreas[0].AxisY.Title = "数量/个";
                        chart3.Series[0].LegendText = "数量";
                        chart3.Series[0].IsVisibleInLegend = true;  //是否显示数据说明 
                        chart3.Series[0].Points.Clear();
                        int mintemp = finalinfo.temp[0], maxtemp = finalinfo.temp[0];
                        for (int m = 0; m < finalinfo.temp.Count; m++)
                        {
                            if (mintemp > finalinfo.temp[m])
                            {
                                mintemp = finalinfo.temp[m];
                            }
                            if (maxtemp < finalinfo.temp[m])
                            {
                                maxtemp = finalinfo.temp[m];
                            }
                        }
                        for (int m = 0; m < finalinfo.temp.Count; m++)
                        {
                            for (int t = 0; t < tt; t++)
                            {
                                if (((mintemp - 1 + ((maxtemp - mintemp + 2) * t / tt)) <= finalinfo.temp[m]) && (finalinfo.temp[m] < (mintemp - 1 + ((maxtemp - mintemp + 2) * (t + 1) / tt))))
                                {
                                    count[t]++;
                                    int end0 = mintemp - 1 + ((maxtemp - mintemp + 2) * t / tt);
                                    int end1 = mintemp - 1 + ((maxtemp - mintemp + 2) * (t + 1) / tt);
                                    tempArea[t] = $"{end0}到{end1}";
                                }
                            }

                        }
                        chart3.Series[0].Points.Clear();
                        for (int t = 0; t < tt; t++)
                        {
                            chart3.Series[0].Points.AddXY(tempArea[t], count[t]);
                        }
                        label35.Visible = true;
                        chart3.Visible = true;
                    }
                    finalinfo.temp.Clear();
                    finalinfo.age.Clear();*/
                }
                catch (Exception ee)
                {
                    MessageBox.Show("查询数据失败！" + ee.ToString());
                }
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            if ((radioButton2.Checked == true) && (bPicBox[0] == true) && (bPicBox[1] == false))
            {
                int tt = Convert.ToInt32(comboBox1.SelectedItem);
                if (tt > 0)
                {
                    if (tt == 2)
                    {
                        tt = 1;
                    }
                    int[] count = new int[tt];
                    string[] tempArea = new string[tt];
                    label35.Text = $"重包的外壳一点温度及数量分布图";
                    chart3.ChartAreas[0].AxisX.Title = "温度 （℃）";
                    chart3.ChartAreas[0].AxisY.Title = "数量/个";
                    chart3.Series[0].LegendText = "数量";
                    chart3.Series[0].IsVisibleInLegend = true;  //是否显示数据说明 
                    chart3.Series[0].Points.Clear();
                    int mintemp = finalinfo.temp[0], maxtemp = finalinfo.temp[0];
                    for (int m = 0; m < finalinfo.temp.Count; m++)
                    {
                        if (mintemp > finalinfo.temp[m])
                        {
                            mintemp = finalinfo.temp[m];
                        }
                        if (maxtemp < finalinfo.temp[m])
                        {
                            maxtemp = finalinfo.temp[m];
                        }
                    }
                    for (int m = 0; m < finalinfo.temp.Count; m++)
                    {
                        for (int t = 0; t < tt; t++)
                        {
                            if (((mintemp - 1 + ((maxtemp - mintemp + 2) * t / tt)) <= finalinfo.temp[m]) && (finalinfo.temp[m] < (mintemp - 1 + ((maxtemp - mintemp + 2) * (t + 1) / tt))))
                            {
                                count[t]++;
                                int end0 = mintemp - 1 + ((maxtemp - mintemp + 2) * t / tt);
                                int end1 = mintemp - 1 + ((maxtemp - mintemp + 2) * (t + 1) / tt);
                                tempArea[t] = $"{end0}到{end1}";
                            }
                        }

                    }
                    chart3.Series[0].Points.Clear();
                    for (int t = 0; t < tt; t++)
                    {
                        chart3.Series[0].Points.AddXY(tempArea[t], count[t]);
                    }
                    label35.Visible = true;
                    chart3.Visible = true;
                }
                finalinfo.temp.Clear();
                finalinfo.age.Clear();
            }
            if ((radioButton2.Checked == true) && (bPicBox[0] == false) && (bPicBox[1] == true))
            {
                int tt = Convert.ToInt32(comboBox1.SelectedItem);
                if (tt > 0)
                {
                    if (tt == 2)
                    {
                        tt = 1;
                    }
                    int[] count = new int[tt];
                    string[] tempArea = new string[tt];
                    label35.Text = $"重包的外壳一点温度及数量分布图";

                    chart3.ChartAreas[0].AxisX.Title = "温度 （℃）";
                    chart3.ChartAreas[0].AxisY.Title = "数量/个";
                    chart3.Series[0].LegendText = "数量";
                    chart3.Series[0].IsVisibleInLegend = true;  //是否显示数据说明 
                    chart3.Series[0].Points.Clear();
                    int mintemp = finalinfo.temp[0], maxtemp = finalinfo.temp[0];
                    for (int m = 0; m < finalinfo.temp.Count; m++)
                    {
                        if (mintemp > finalinfo.temp[m])
                        {
                            mintemp = finalinfo.temp[m];
                        }
                        if (maxtemp < finalinfo.temp[m])
                        {
                            maxtemp = finalinfo.temp[m];
                        }
                    }
                    for (int m = 0; m < finalinfo.temp.Count; m++)
                    {
                        for (int t = 0; t < tt; t++)
                        {
                            if (((mintemp - 1 + ((maxtemp - mintemp + 2) * t / tt)) <= finalinfo.temp[m]) && (finalinfo.temp[m] < (mintemp - 1 + ((maxtemp - mintemp + 2) * (t + 1) / tt))))
                            {
                                count[t]++;
                                int end0 = mintemp - 1 + ((maxtemp - mintemp + 2) * t / tt);
                                int end1 = mintemp - 1 + ((maxtemp - mintemp + 2) * (t + 1) / tt);
                                tempArea[t] = $"{end0}到{end1}";
                            }
                        }

                    }
                    chart3.Series[0].Points.Clear();
                    for (int t = 0; t < tt; t++)
                    {
                        chart3.Series[0].Points.AddXY(tempArea[t], count[t]);
                    }
                    label35.Visible = true;
                    chart3.Visible = true;
                }
                finalinfo.temp.Clear();
                finalinfo.age.Clear();
            }
        }
    }
}
