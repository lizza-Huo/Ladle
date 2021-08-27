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
using System.Threading;
using System.Text.RegularExpressions;

namespace LadleThermDetectSys
{
    public partial class LaddleSelfCheckSys : Form
    {
        
        AutoResizeForm asc = new AutoResizeForm();

        //string SQLServerUser = "user id=root;password=ARIMlab2020.07.22;server=localhost;persistsecurityinfo=True;database=ironldlthermodetectmanactrl;port=3306;SslMode=None;allowPublicKeyRetrieval=true";
        //string SQLServerUser = "user id=tbrj;password=tbrj;server=10.99.24.144;persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true";
        string SQLServerUser = "user id=test;password=test;server=192.168.2.100;persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true";
        //int TbID = 1;
        //int TbID;
        /*string UserID = "tbrj";
        string Password="tbrj";        
        string Server = "10.99.24.144";*/

        /*string UserID = "root";
        string Password = "ARIMlab2020.07.22";
        string Server = "localhost";*/

        string UserID = "test";
        string Password = "test";
        string Server = "192.168.2.100";



        int iMaxTempImagerNo = 0, iMaxTempXinPixel = 0, iMaxTempYbyPixel = 0;
        bool[] bDrawMaxTempScope = new bool[5] { false, false, false, false, false };
        bool[] bDrawTemp = new bool[5] {false,false,false,false,false};
        bool[] bDrawThick = new bool[2] { false, false};

        private static byte StandardAnswer = 33;

        public bool IsNumber(String strNumber)
        {
            Regex objNotNumberPattern = new Regex("[^0-9.-]");
            Regex objTwoDotPattern = new Regex("[0-9]*[.][0-9]*[.][0-9]*");
            Regex objTwoMinusPattern = new Regex("[0-9]*[-][0-9]*[-][0-9]*");
            String strValidRealPattern = "^([-]|[.]|[-.]|[0-9])[0-9]*[.]*[0-9]+$";
            String strValidIntegerPattern = "^([-]|[0-9])[0-9]*$";
            Regex objNumberPattern = new Regex("(" + strValidRealPattern + ")|(" + strValidIntegerPattern + ")");

            return !objNotNumberPattern.IsMatch(strNumber) &&
            !objTwoDotPattern.IsMatch(strNumber) &&
            !objTwoMinusPattern.IsMatch(strNumber) &&
            objNumberPattern.IsMatch(strNumber);
        }
        public LaddleSelfCheckSys()
        {
            InitializeComponent();
            //测温
            //104
            label_skytrain104.Parent = panelShowTemp;
            label1_skytrain104.Parent = panelShowTemp;
            label_HeavyLaddle104.Parent = panelShowTemp;
            //105
            label_skytrain105.Parent = panelShowTemp;
            label1_skytrain105.Parent = panelShowTemp;
            label_HeavyLaddle105.Parent = panelShowTemp;
            //106
            label_skytrain106.Parent = panelShowTemp;
            label1_skytrain106.Parent = panelShowTemp;
            label_HeavyLaddle106.Parent = panelShowTemp;

            //车架
            label_jiaziE1.Parent = panelShowTemp;
            label_jiaziE2.Parent = panelShowTemp;
            label_jiaziE3.Parent = panelShowTemp;
            label_jiaziE4.Parent = panelShowTemp;
            //车架上的重包
            label_jiaziE1_heavyLadle.Parent = panelShowTemp;
            label_jiaziE2_heavyLadle.Parent = panelShowTemp;
            label_jiaziE3_heavyLadle.Parent = panelShowTemp;
            label_jiaziE4_heavyLadle.Parent = panelShowTemp;

            //置于顶层显示
            label_HeavyLaddle104.BringToFront();
            label_HeavyLaddle105.BringToFront();
            label_HeavyLaddle106.BringToFront();

            //112
            label_skytrain112.Parent = panelShowThick;
            label1_skytrain112.Parent = panelShowThick;
            label_EmptyLaddle112.Parent = panelShowThick;
            //111
            label_skytrain111.Parent = panelShowThick;
            label1_skytrain111.Parent = panelShowThick;
            label_EmptyLaddle111.Parent = panelShowThick;

            //127
            label_skytrain127.Parent = panelShowThick;
            label1_skytrain127.Parent = panelShowThick;
            label_EmptyLaddle127.Parent = panelShowThick;
            //小车
            label_thicktrain.Parent = panelShowThick;
            label1_thicktrain.Parent = panelShowThick;
            label1_xiaoche.Parent = panelShowThick;
            //车架
            label_jiaziE.Parent = panelShowThick;
            label_jiaziW.Parent = panelShowThick;
            //车架上的重包
            label_jiaziE_emptyLadle.Parent = panelShowThick;
            label_jiaziW_emptyLadle.Parent = panelShowThick;
            //置于顶层显示
            label_EmptyLaddle112.BringToFront();
            label_EmptyLaddle111.BringToFront();
            label_EmptyLaddle127.BringToFront();
            label1_xiaoche.BringToFront();
            label_jiaziE_emptyLadle.BringToFront();
            label_jiaziW_emptyLadle.BringToFront();
        }
        public static int TbID;
        string ladleno104last;
        string ladleno105last;
        int rowslastHeavyldl_Meased;
        int ElID;
        int rowslastdtaGrdViewEmtyMeased;
        private static int MeaThickRecordCount = 30;//每次存入txt的记录条数+1
        private int ThickStatusCode = -1, ThickStatusCodeEast = -1, ThickStatusCodeWest = -1;
        private DateTime ThickMeasTm, ThickMeasTmEast, ThickMeasTmWest;
        private static int drawStatusFlag = 0;
        private void LaddleSelfCheckSys_Load(object sender, EventArgs e)
        {
            asc.controllInitializeSize(this);
            tabCtrlMeas.SelectedIndex = 0;
            
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            //打开数据库
            try
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();
                }
                if ((tabCtrlMeas.SelectedIndex == 0) && (myconnection.State == ConnectionState.Open))
                {
                    MySQLConnection1.Image = Properties.Resources.green2;
                }
                if ((tabCtrlMeas.SelectedIndex == 1) && (myconnection.State == ConnectionState.Open))
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                if (tabCtrlMeas.SelectedIndex == 0)
                {
                    string SQL = "select SerNo 序号,CrnBlkNo 位置,LadleNo 包号,LadleAge 包龄 from heavyldl_tomeas";
                    DataSet ds = new DataSet();
                    using (MySqlDataAdapter objDataAdpter = new MySqlDataAdapter())
                    {
                        objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection);
                        objDataAdpter.Fill(ds, "heavyldl_tomeas");
                    } 
                     Heavyldl_ToMeas.DataSource = ds.Tables[0];
                     string ladleno104 = ds.Tables[0].Rows[0][2].ToString();
                     string ladleno105 = ds.Tables[0].Rows[1][2].ToString();
                     ladleno104last = ladleno104;
                     ladleno105last = ladleno105;
                     

                     //数据表的测量时间降序排列取前10行，显示部分列到界面
                    string SQL2 = "select id, LadleNo,LadleServDuty, LadleAge, MeasTm, MaxTemp,MaxTempPos  from heavyldl_meased where `Delete`=0 order by id desc limit 0,50;";
                    DataSet ds2 = new DataSet();
                    using (MySqlDataAdapter objDataAdpter2 = new MySqlDataAdapter())
                    {
                        objDataAdpter2.SelectCommand = new MySqlCommand(SQL2, myconnection);
                        objDataAdpter2.Fill(ds2, "heavyldl_meased");
                    }
                    Heavyldl_Meased.AutoGenerateColumns = false;
                    Heavyldl_Meased.DataSource = ds2.Tables[0];
                    
                    //int TbID = Convert.ToInt32(ds2.Tables[0].Rows[0][0]);
                    TbID = Convert.ToInt32(ds2.Tables[0].Rows[0][0]);
                    rowslastHeavyldl_Meased = TbID;
                    string SQLMaxTempImager = "select MaxTempImagerNo,MaxTempXbyPixel,MaxTempYbyPixel  from heavyldl_meased where id=" + TbID + "";
                    DataSet dsMaxTempImager = new DataSet();
                    using (MySqlDataAdapter objDataAdpterMaxTempImager = new MySqlDataAdapter())
                    {
                        objDataAdpterMaxTempImager.SelectCommand = new MySqlCommand(SQLMaxTempImager, myconnection);
                        objDataAdpterMaxTempImager.Fill(dsMaxTempImager, "heavyldl_meased");
                    }
                    iMaxTempImagerNo = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][0]);
                    iMaxTempXinPixel = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][1]);
                    iMaxTempYbyPixel = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][2]);
                    bDrawMaxTempScope = new bool[5] { false, false, false, false, false };
                    if (iMaxTempImagerNo != 0)
                    {
                        bDrawMaxTempScope[iMaxTempImagerNo - 1] = true;
                    }

                    string SQL6 = "SELECT controlheart FROM sysinteractinstr_copy1";
                    MySqlDataAdapter objDataAdpter6 = new MySqlDataAdapter();
                    DataSet ds6 = new DataSet();
                    using (objDataAdpter6.SelectCommand = new MySqlCommand(SQL6, myconnection))
                    {
                        objDataAdpter6.Fill(ds6, "sysinteractinstr_copy1");
                    }
                    controlheartlast = Convert.ToInt32(ds6.Tables[0].Rows[0][0]);//后台心跳
                    
                }

                if (tabCtrlMeas.SelectedIndex == 1)
                {
                    txtBoxShowStatus.Text = "请先选择操作模式！";
                    XYCarSemiAutoEastPos.Enabled = false;
                    XYCarSemiAutoWestPos.Enabled = false;
                    XYCarSemiAutoEastWestPos.Enabled = false;
                    comboBoxSelectAimPos.Enabled = false;
                    textBoxLadleNoEast.Enabled = false;
                    textBoxLadleNoWest.Enabled = false;
                    XYCarsToTarget.Enabled = false;
                    XYCarsTakePhoto.Enabled = false;
                    XYCarsToHome.Enabled = false;
                    XCarsToTarget.Enabled = false;
                    YCarToTarget.Enabled = false;
                    MeasThick.Enabled = false;
                    YCarToHome.Enabled = false;
                    XCarToHome.Enabled = false;

                    string SQL4 = "select id, LadleNo,LadleServDuty,LadleAge, MeasTm,MinThick,MinThickPos,StatusCode,ModeType  from emptyldl_meased where `Delete`=0 order by id desc limit 0,50;";
                    MySqlDataAdapter objDataAdpter4 = new MySqlDataAdapter();
                    objDataAdpter4.SelectCommand = new MySqlCommand(SQL4, myconnection);
                    DataSet ds4 = new DataSet();
                    objDataAdpter4.Fill(ds4, "emptyldl_meased");
                    dtaGrdViewEmtyMeased.AutoGenerateColumns = false;
                    dtaGrdViewEmtyMeased.DataSource = ds4.Tables[0];
                    ElID = Convert.ToInt32(ds4.Tables[0].Rows[0][0]);
                    ThickMeasTm = Convert.ToDateTime(ds4.Tables[0].Rows[0][4]);
                    ThickStatusCode = Convert.ToInt32(ds4.Tables[0].Rows[0][7]);
                    rowslastdtaGrdViewEmtyMeased = ElID;
                }
                if (myconnection.State == ConnectionState.Closed)
                {
                    tiancheAvoid.Image = Properties.Resources.black;
                    tiancheBroken.Image = Properties.Resources.black;
                    tiancheRomotrMode.Image = Properties.Resources.black;
                    tiancheHurryStop.Image = Properties.Resources.black;
                    tiancheDoorOpen.Image = Properties.Resources.black;
                    headTotiancheHeartCount.Image = Properties.Resources.black;
                    tiancheHeart.Image = Properties.Resources.black;
                    thickHeart.Image = Properties.Resources.black;
                }
                if ((tabCtrlMeas.SelectedIndex == 1))
                {
                    string SQL5 = "SELECT controlheart,tianchestatus01,tianchestatus02,tianchestatus03,tianchestatus04,tianchestatus05,tianchestatus06,tianchestatus07,tianchestatus08,tianchestatus11,tianchestatus12,tiancheheart,thickheartstatus,thickTmpOut,thickHumid,thickwarning,gbgzstatus FROM sysinteractinstr_copy1";
                    MySqlDataAdapter objDataAdpter5 = new MySqlDataAdapter();
                    objDataAdpter5.SelectCommand = new MySqlCommand(SQL5, myconnection);
                    DataSet ds5 = new DataSet();
                    objDataAdpter5.Fill(ds5, "sysinteractinstr_copy1");
                    controlheartlast= Convert.ToInt32(ds5.Tables[0].Rows[0][0]);//后台心跳
                    Status12t_1 = Convert.ToInt32(ds5.Tables[0].Rows[0][1]);//紧急避让行车
                    Status12t_2 = Convert.ToInt32(ds5.Tables[0].Rows[0][2]);//行车故障
                    Status12t_3 = Convert.ToInt32(ds5.Tables[0].Rows[0][3]);//本地操作模式
                    Status12t_4 = Convert.ToInt32(ds5.Tables[0].Rows[0][4]);//紧急停车
                    Status12t_5 = Convert.ToInt32(ds5.Tables[0].Rows[0][5]);//行车上车门未关
                    Status12t_6 = Convert.ToInt32(ds5.Tables[0].Rows[0][6]);//热检通讯中断
                    Status12t_7 = Convert.ToInt32(ds5.Tables[0].Rows[0][7]);//紧急箱通讯故障
                    Status12t_8 = Convert.ToInt32(ds5.Tables[0].Rows[0][8]);//变频器使能状态
                    Status12t_11 = Convert.ToInt32(ds5.Tables[0].Rows[0][9]);//车架状态状态
                    Status12t_12 = Convert.ToInt32(ds5.Tables[0].Rows[0][10]);//遥控接收器使能状态
                    tiancheheart = Convert.ToInt32(ds5.Tables[0].Rows[0][11]);//天车通讯状态
                    thickheartstatus = Convert.ToInt32(ds5.Tables[0].Rows[0][12]);//测厚设备心跳状态
                    thickTmpOut = Convert.ToSingle(ds5.Tables[0].Rows[0][13]);//测厚温度报警
                    thickHumid = Convert.ToSingle(ds5.Tables[0].Rows[0][14]);//测厚设备湿度报警
                    thickwarning = Convert.ToInt32(ds5.Tables[0].Rows[0][15]);//测厚设备报警
                    gbgzStatus= Convert.ToInt32(ds5.Tables[0].Rows[0][16]);//钢包跟踪视图报警
                    if (Status12t_1 == 0)
                    {
                        tiancheAvoid.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_1 == 1)
                    {
                        tiancheAvoid.Image = Properties.Resources.red;
                    }
                    if (Status12t_2 == 0)
                    {
                        tiancheBroken.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_2 == 1)
                    {
                        tiancheBroken.Image = Properties.Resources.red;
                    }
                    if (Status12t_3 == 0)
                    {
                        tiancheRomotrMode.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_3 == 1)
                    {
                        tiancheRomotrMode.Image = Properties.Resources.red;
                    }
                    if (Status12t_4 == 0)
                    {
                        tiancheHurryStop.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_4 == 1)
                    {
                        tiancheHurryStop.Image = Properties.Resources.red;
                    }
                    if (Status12t_5 == 0)
                    {
                        tiancheDoorOpen.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_5 == 1)
                    {
                        tiancheDoorOpen.Image = Properties.Resources.red;
                    }
                    if (Status12t_6 == 0)
                    {
                        headTotiancheHeartCount.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_6 == 1)
                    {
                        headTotiancheHeartCount.Image = Properties.Resources.red;
                    }
                    if (Status12t_7 == 0)
                    {
                        WarningBxComunication.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_7 == 1)
                    {
                        WarningBxComunication.Image = Properties.Resources.red;
                    }
                    if (Status12t_8 == 0)
                    {
                        InverterEn.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_8 == 1)
                    {
                        InverterEn.Image = Properties.Resources.red;
                    }
                    if (Status12t_11 == 0)
                    {
                        RackPosBl.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_11 == 1)
                    {
                        RackPosBl.Image = Properties.Resources.red;
                    }
                    if (Status12t_12 == 0)
                    {
                        RemoteEn.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_12 == 1)
                    {
                        RemoteEn.Image = Properties.Resources.red;
                    }
                    if (tiancheheart == 0)
                    {
                        tiancheHeart.Image = Properties.Resources.green2;
                    }
                    else if (tiancheheart == 1)
                    {
                        tiancheHeart.Image = Properties.Resources.red;
                    }
                    if (thickheartstatus == 0)
                    {
                        thickHeart.Image = Properties.Resources.green2;
                    }
                    else if (thickheartstatus == 1)
                    {
                        thickHeart.Image = Properties.Resources.red;
                    }
                    if (thickTmpOut <= 40)
                    {
                        lblthickTempOut.Text = "温度(℃):" + thickTmpOut.ToString();
                    }
                    else if (thickTmpOut >40)
                    {
                        lblthickTempOut.Text = "温度(℃):" + thickTmpOut.ToString();
                    }
                    if (thickHumid <=40)
                    {
                        lblHumidOut.Text = "湿度(%):" + thickHumid.ToString();
                    }
                    else if (thickHumid >40)
                    {
                        lblHumidOut.Text = "湿度(%):" + thickHumid.ToString();
                    }
                    switch (thickwarning) 
                    {
                        case 0: 
                            {
                                ThickWorkStatus.Image= Properties.Resources.green2;
                                lblThickWorkStatus.Text = "正常";
                            }
                            break;
                        case 1:
                            {
                                ThickWorkStatus.Image = Properties.Resources.purple;
                                lblThickWorkStatus.Text = "温度预警";
                            }
                            break;
                        case 2:
                            {
                                ThickWorkStatus.Image = Properties.Resources.red;
                                lblThickWorkStatus.Text = "离线状态";
                            }
                            break;
                        case 3:
                            {
                                ThickWorkStatus.Image = Properties.Resources.yellow;
                                lblThickWorkStatus.Text = "空闲状态";
                            }
                            break;
                        case 4:
                            {
                                ThickWorkStatus.Image = Properties.Resources.green2;
                                lblThickWorkStatus.Text = "扫描状态";
                            }
                            break;
                        case 5:
                            {
                                ThickWorkStatus.Image = Properties.Resources.purple;
                                lblThickWorkStatus.Text = "数据传输";
                            }
                            break;
                    }
                    if (gbgzStatus == 0)
                    {
                        gbgzheart.Image = Properties.Resources.green2;
                    }
                    else if (gbgzStatus == 1)
                    {
                        gbgzheart.Image= Properties.Resources.red;
                    }

                }
                myconnection.Close();
            }
            catch (Exception EE)
            {
                if ((tabCtrlMeas.SelectedIndex == 0) && (myconnection.State == ConnectionState.Closed))
                {
                    MySQLConnection1.Image = Properties.Resources.redshine;
                }
                if ((tabCtrlMeas.SelectedIndex == 1) && (myconnection.State == ConnectionState.Closed))
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
            
            if (tabCtrlMeas.SelectedIndex == 0)
            {
                ShowtempPic();
            }
            if (tabCtrlMeas.SelectedIndex == 1)
            {
                if (dtaGrdViewEmtyMeased.Rows.Count > 0)
                {
                    ShowthickImage();
                }
                
            }
            
        }

        //画图所用参数
        int skytrain111_x;
        int skytrain112_x;
        int skytrain127_x;
        string emptyLaddle111_exsit_num;
        string emptyLaddle112_exsit_num;
        string emptyLaddle127_exsit_num;
        string label_jiaziE_exsit_num;
        string label_jiaziW_exsit_num;
        string label_jiaziE_emptyLaddle_exsit_num;
        string label_jiaziW_emptyLaddle_exsit_num;
        int label_thicktrain_x;
        int label_thicktrain_y;
        //四线柱的真实x坐标
        int Wall4_x_real = 468;
        int Wall1_x_real = 525;
        //一个像素值对应的实际距离
        double pix_distance_real;
        double pix_distance_realy;

        int skytrain104_x;
        int skytrain105_x;
        int skytrain106_x;
        string heavyLadle104_exsit_num;
        string heavyLadle105_exsit_num;
        string heavyLadle106_exsit_num;
        string label_jiaziE1_exsit_num;
        string label_jiaziE2_exsit_num;
        string label_jiaziE3_exsit_num;
        string label_jiaziE4_exsit_num;
        string label_jiaziE1_heavyLadle_exsit_num;
        string label_jiaziE2_heavyLadle_exsit_num;
        string label_jiaziE3_heavyLadle_exsit_num;
        string label_jiaziE4_heavyLadle_exsit_num;
        
        double panelShowThick_Start_Point_x = 0;
        double pix_distance_real2 = 0;
        //测厚线的天车矩形y值
        int skyreain_y_Posi;
        //测温线的天车矩形y值
        int skyreain_y_Posi_temp;
        int div_num = 16;
        double start_n = 5.0;
        double temp_n = 3.0;
        int Wall4_x;
        int Wall1_x;
        double tempoffset = 9.7;
        double thickoffsset = 5.1;


        //测厚全局变量定义
        int m_iOperation = 0;

        private void LaddleSelfCheckSys_SizeChanged(object sender, EventArgs e)
        {
            asc.controlAutoSize(this);
        }
        private void tabCtrlMeas_SelectedIndexChanged(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            //打开数据库
            try
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();
                }
                if ((tabCtrlMeas.SelectedIndex == 0) && (myconnection.State == ConnectionState.Open))
                {
                    MySQLConnection1.Image = Properties.Resources.green2;
                }
                if ((tabCtrlMeas.SelectedIndex == 1) && (myconnection.State == ConnectionState.Open))
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                if (tabCtrlMeas.SelectedIndex == 0)
                {
                    textBox1.Text = "";
                    string SQL = "select SerNo 序号,CrnBlkNo 位置,LadleNo 包号,LadleAge 包龄 from heavyldl_tomeas";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    DataSet ds = new DataSet();
                    using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                    {
                        objDataAdpter.Fill(ds, "heavyldl_tomeas");
                    }
                    Heavyldl_ToMeas.DataSource = ds.Tables[0];
                    string ladleno104 = ds.Tables[0].Rows[0][2].ToString();
                    string ladleno105 = ds.Tables[0].Rows[1][2].ToString();
                    ladleno104last = ladleno104;
                    ladleno105last = ladleno105;

                    //数据表的测量时间降序排列取前10行，显示部分列到界面
                    string SQL2 = "select id, LadleNo,LadleServDuty, LadleAge, MeasTm , MaxTemp ,MaxTempPos  from heavyldl_meased where `Delete`=0 order by id desc limit 0,50";
                    MySqlDataAdapter objDataAdpter2 = new MySqlDataAdapter();
                    DataSet ds2 = new DataSet();
                    using (objDataAdpter2.SelectCommand = new MySqlCommand(SQL2, myconnection))
                    {
                        objDataAdpter2.Fill(ds2, "heavyldl_meased");
                    }
                    
                    Heavyldl_Meased.AutoGenerateColumns = false;
                    Heavyldl_Meased.DataSource = ds2.Tables[0];
                    
                    TbID = Convert.ToInt32(ds2.Tables[0].Rows[0][0]);
                    rowslastHeavyldl_Meased = TbID;
                    string SQLMaxTempImager = "select MaxTempImagerNo,MaxTempXbyPixel,MaxTempYbyPixel  from heavyldl_meased where id=" + TbID + "";
                    MySqlDataAdapter objDataAdpterMaxTempImager = new MySqlDataAdapter();
                    DataSet dsMaxTempImager = new DataSet();
                    using (objDataAdpterMaxTempImager.SelectCommand = new MySqlCommand(SQLMaxTempImager, myconnection))
                    {
                        objDataAdpterMaxTempImager.Fill(dsMaxTempImager, "heavyldl_meased");
                    }    
                    iMaxTempImagerNo = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][0]);
                    iMaxTempXinPixel = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][1]);
                    iMaxTempYbyPixel = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][2]);
                    bDrawMaxTempScope = new bool[5] { false, false, false, false, false };
                    if (iMaxTempImagerNo != 0)
                    {
                        bDrawMaxTempScope[iMaxTempImagerNo - 1] = true;
                    }
                    string SQL6 = "SELECT controlheart FROM sysinteractinstr_copy1";
                    MySqlDataAdapter objDataAdpter6 = new MySqlDataAdapter();
                    DataSet ds6 = new DataSet();
                    using (objDataAdpter6.SelectCommand = new MySqlCommand(SQL6, myconnection))
                    {
                        objDataAdpter6.Fill(ds6, "sysinteractinstr_copy1");
                    }
                    controlheartnow = Convert.ToInt32(ds6.Tables[0].Rows[0][0]);//后台心跳
                    if (controlheartnow != controlheartlast)
                    {
                        houtaiHeart1.Image = Properties.Resources.green2;
                        controlheartlast = controlheartnow;
                    }
                    else if (controlheartnow == controlheartlast)
                    {
                        houtaiHeart1.Image = Properties.Resources.redshine;
                    }
                }

                if (tabCtrlMeas.SelectedIndex == 1) 
                {
                    txtBoxShowStatus.Text = "请先选择操作模式！";
                    comboBoxSelectMode.Enabled = true;
                    comboBoxSelectMode.Text = "选择操作模式";
                    XYCarSemiAutoEastPos.Enabled = false;
                    XYCarSemiAutoWestPos.Enabled = false;
                    XYCarSemiAutoEastWestPos.Enabled = false;
                    comboBoxSelectAimPos.Enabled = false;
                    comboBoxSelectAimPos.Text = "选择目标位";
                    textBoxLadleNoEast.Enabled = false;
                    textBoxLadleNoWest.Enabled = false;
                    XYCarsToTarget.Enabled = false;
                    XYCarsTakePhoto.Enabled = false;
                    XYCarsToHome.Enabled = false;
                    XCarsToTarget.Enabled = false;
                    YCarToTarget.Enabled = false;
                    MeasThick.Enabled = false;
                    YCarToHome.Enabled = false;
                    XCarToHome.Enabled = false;
                    butSurePara.Enabled = false;
                    butStop.Enabled = true;
                    string SQL4 = "select id,LadleNo,LadleServDuty,LadleAge,MeasTm,MinThick,MinThickPos,StatusCode,ModeType  from emptyldl_meased  where `Delete`=0 order by id desc limit 0,50;";
                    MySqlDataAdapter objDataAdpter4 = new MySqlDataAdapter();
                    DataSet ds4 = new DataSet();
                    using (objDataAdpter4.SelectCommand = new MySqlCommand(SQL4, myconnection))
                    {
                        objDataAdpter4.Fill(ds4, "emptyldl_meased");
                    }
                    dtaGrdViewEmtyMeased.AutoGenerateColumns = false;
                    dtaGrdViewEmtyMeased.DataSource = ds4.Tables[0];
                    ElID = Convert.ToInt32(ds4.Tables[0].Rows[0][0]);
                    ThickMeasTm = Convert.ToDateTime(ds4.Tables[0].Rows[0][4]);
                    ThickStatusCode = Convert.ToInt32(ds4.Tables[0].Rows[0][7]);
                    rowslastdtaGrdViewEmtyMeased = ElID;
                }
                if (myconnection.State == ConnectionState.Closed)
                {
                    tiancheAvoid.Image = Properties.Resources.black;
                    tiancheBroken.Image = Properties.Resources.black;
                    tiancheRomotrMode.Image = Properties.Resources.black;
                    tiancheHurryStop.Image = Properties.Resources.black;
                    tiancheDoorOpen.Image = Properties.Resources.black;
                    headTotiancheHeartCount.Image = Properties.Resources.black;
                    tiancheHeart.Image = Properties.Resources.black;
                    thickHeart.Image = Properties.Resources.black;
                }
                if ((tabCtrlMeas.SelectedIndex == 1))
                {
                    string SQL5 = "SELECT controlheart,tianchestatus01,tianchestatus02,tianchestatus03,tianchestatus04,tianchestatus05,tianchestatus06,tianchestatus07,tianchestatus08,tianchestatus11,tianchestatus12,tiancheheart,thickheartstatus,thickTmpOut,thickHumid,thickwarning,gbgzstatus FROM sysinteractinstr_copy1";
                    MySqlDataAdapter objDataAdpter5 = new MySqlDataAdapter();
                    DataSet ds5 = new DataSet();
                    using (objDataAdpter5.SelectCommand = new MySqlCommand(SQL5, myconnection))
                    {
                        objDataAdpter5.Fill(ds5, "sysinteractinstr_copy1");
                    }
                    controlheartnow= Convert.ToInt32(ds5.Tables[0].Rows[0][0]);//后台心跳
                    Status12t_1 = Convert.ToInt32(ds5.Tables[0].Rows[0][1]);//紧急避让行车
                    Status12t_2 = Convert.ToInt32(ds5.Tables[0].Rows[0][2]);//行车故障
                    Status12t_3 = Convert.ToInt32(ds5.Tables[0].Rows[0][3]);//本地操作模式
                    Status12t_4 = Convert.ToInt32(ds5.Tables[0].Rows[0][4]);//紧急停车
                    Status12t_5 = Convert.ToInt32(ds5.Tables[0].Rows[0][5]);//行车上车门未关
                    Status12t_6 = Convert.ToInt32(ds5.Tables[0].Rows[0][6]);//心跳计数器故障
                    Status12t_7 = Convert.ToInt32(ds5.Tables[0].Rows[0][7]);//紧急箱通讯故障
                    Status12t_8 = Convert.ToInt32(ds5.Tables[0].Rows[0][8]);//变频器使能状态
                    Status12t_11 = Convert.ToInt32(ds5.Tables[0].Rows[0][9]);//变频器使能状态
                    Status12t_12 = Convert.ToInt32(ds5.Tables[0].Rows[0][10]);//变频器使能状态
                    tiancheheart = Convert.ToInt32(ds5.Tables[0].Rows[0][11]);//天车通讯状态
                    thickheartstatus = Convert.ToInt32(ds5.Tables[0].Rows[0][12]);//测厚设备心跳状态
                    thickTmpOut = Convert.ToSingle(ds5.Tables[0].Rows[0][13]);//测厚温度报警
                    thickHumid = Convert.ToSingle(ds5.Tables[0].Rows[0][14]);//测厚设备湿度报警
                    thickwarning = Convert.ToInt32(ds5.Tables[0].Rows[0][15]);//测厚设备报警
                    gbgzStatus= Convert.ToInt32(ds5.Tables[0].Rows[0][16]);//钢包跟踪视图报警
                    if (controlheartnow != controlheartlast)
                    {
                        houtaiHeart.Image = Properties.Resources.green2;
                        controlheartlast = controlheartnow;
                    }
                    else if (controlheartnow == controlheartlast) 
                    {
                        houtaiHeart.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_1 == 0)
                    {
                        tiancheAvoid.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_1 == 1)
                    {
                        tiancheAvoid.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_2 == 0)
                    {
                        tiancheBroken.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_2 == 1)
                    {
                        tiancheBroken.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_3 == 0)
                    {
                        tiancheRomotrMode.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_3 == 1)
                    {
                        tiancheRomotrMode.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_4 == 0)
                    {
                        tiancheHurryStop.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_4 == 1)
                    {
                        tiancheHurryStop.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_5 == 0)
                    {
                        tiancheDoorOpen.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_5 == 1)
                    {
                        tiancheDoorOpen.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_6 == 0)
                    {
                        headTotiancheHeartCount.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_6 == 1)
                    {
                        headTotiancheHeartCount.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_7 == 0)
                    {
                        WarningBxComunication.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_7 == 1)
                    {
                        WarningBxComunication.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_8 == 0)
                    {
                        InverterEn.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_8 == 1)
                    {
                        InverterEn.Image = Properties.Resources.red;
                    }
                    if (Status12t_11 == 0)
                    {
                        RackPosBl.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_11 == 1)
                    {
                        RackPosBl.Image = Properties.Resources.red;
                    }
                    if (Status12t_12 == 0)
                    {
                        RemoteEn.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_12 == 1)
                    {
                        RemoteEn.Image = Properties.Resources.red;
                    }
                    if (tiancheheart == 0)
                    {
                        tiancheHeart.Image = Properties.Resources.green2;
                    }
                    else if (tiancheheart == 1)
                    {
                        tiancheHeart.Image = Properties.Resources.redshine;
                    }
                    if (thickheartstatus == 0)
                    {
                        thickHeart.Image = Properties.Resources.green2;
                    }
                    else if (thickheartstatus == 1)
                    {
                        thickHeart.Image = Properties.Resources.redshine;
                    }
                    if (thickTmpOut <= 40)
                    {
                        lblthickTempOut.Text = "温度(℃):" + thickTmpOut.ToString();
                    }
                    else if (thickTmpOut > 40)
                    {
                        lblthickTempOut.Text = "温度(℃):" + thickTmpOut.ToString();
                    }
                    if (thickHumid <= 40)
                    {
                        lblHumidOut.Text = "湿度(%):" + thickHumid.ToString();
                    }
                    else if (thickHumid > 40)
                    {
                        lblHumidOut.Text = "湿度(%):" + thickHumid.ToString();
                    }
                    switch (thickwarning)
                    {
                        case 0:
                            {
                                ThickWorkStatus.Image = Properties.Resources.green2;
                                lblThickWorkStatus.Text = "正常";
                            }
                            break;
                        case 1:
                            {
                                ThickWorkStatus.Image = Properties.Resources.purple;
                                lblThickWorkStatus.Text = "温度预警";
                            }
                            break;
                        case 2:
                            {
                                ThickWorkStatus.Image = Properties.Resources.red;
                                lblThickWorkStatus.Text = "离线状态";
                            }
                            break;
                        case 3:
                            {
                                ThickWorkStatus.Image = Properties.Resources.yellow;
                                lblThickWorkStatus.Text = "空闲状态";
                            }
                            break;
                        case 4:
                            {
                                ThickWorkStatus.Image = Properties.Resources.green2;
                                lblThickWorkStatus.Text = "扫描状态";
                            }
                            break;
                        case 5:
                            {
                                ThickWorkStatus.Image = Properties.Resources.purple;
                                lblThickWorkStatus.Text = "数据传输";
                            }
                            break;
                    }
                    if (gbgzStatus == 0)
                    {
                        gbgzheart.Image = Properties.Resources.green2;
                    }
                    else if (gbgzStatus == 1)
                    {
                        gbgzheart.Image = Properties.Resources.red;
                    }

                }
                if (tabCtrlMeas.SelectedIndex == 2)
                {
                    string SQL6 = "select LadleNo_Ball,StatusCode,MeasTime  from ladleaimball  where `DataExist`=1 or StatusCode!=0;";
                    MySqlDataAdapter objDataAdpter6 = new MySqlDataAdapter();
                    DataSet ds6 = new DataSet();
                    using (objDataAdpter6.SelectCommand = new MySqlCommand(SQL6, myconnection))
                    {
                        objDataAdpter6.Fill(ds6, "ladleaimball");
                    }
                    dtGridViewBall.AutoGenerateColumns = false;
                    dtGridViewBall.DataSource = ds6.Tables[0];
                }
                myconnection.Close();
            }
            catch (Exception EE)
            {
                if ((tabCtrlMeas.SelectedIndex == 0) && (myconnection.State == ConnectionState.Closed))
                {
                    MySQLConnection1.Image = Properties.Resources.redshine;
                }
                if ((tabCtrlMeas.SelectedIndex == 1) && (myconnection.State == ConnectionState.Closed))
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
                MessageBox.Show("查询数据失败！" + EE.ToString());
            }
            
            if (tabCtrlMeas.SelectedIndex == 0)
            {
                ShowtempPic();
            }
            if (tabCtrlMeas.SelectedIndex == 1)
            {
                if (dtaGrdViewEmtyMeased.Rows.Count > 0)
                {
                    ShowthickImage();
                }
                
            }
        }
        private void panelShowThick_Paint(object sender, PaintEventArgs e)
        {
            Wall4_x = 0;
            Wall1_x = 0;
            Graphics g1 = e.Graphics;//画布
            g1.Clear(Color.FromArgb(192, 192, 192));
            int width = panelShowThick.Size.Width;
            int height = panelShowThick.Size.Height;
            skyreain_y_Posi = Convert.ToInt32(Math.Round((height / 5) * 0.9));
            int skytrainheight = Convert.ToInt32(Math.Round((height / 5) * 3.7)) - Convert.ToInt32(Math.Round((height / 5) * 0.9));
            label1_skytrain112.Height = skytrainheight;
            label1_skytrain111.Height = skytrainheight;
            label1_skytrain127.Height = skytrainheight;
            label1_thicktrain.Height = skytrainheight;
            int xiaocheheight = label1_thicktrain.Size.Height;
            
            //四线柱
            Wall4_x = Convert.ToInt32(Math.Round((width / div_num) * start_n));
            Wall1_x = Convert.ToInt32(Math.Round((width / div_num) * (start_n + temp_n * 1.4 + temp_n * 2)));
            g1.FillRectangle(Brushes.LightGray, new Rectangle(Wall4_x, 8, 5, Convert.ToInt32(Math.Round((height / 5) * 3.5))));
            g1.FillRectangle(Brushes.LightGray, new Rectangle(Convert.ToInt32(Math.Round((width / div_num) * (start_n + temp_n))), 8, 5, Convert.ToInt32(Math.Round((height / 5) * 3.5))));
            g1.FillRectangle(Brushes.LightGray, new Rectangle(Convert.ToInt32(Math.Round((width / div_num) * (start_n + temp_n * 1.4 + temp_n))), 8, 5, Convert.ToInt32(Math.Round((height / 5) * 3.5))));
            g1.FillRectangle(Brushes.LightGray, new Rectangle(Wall1_x, 8, 5, Convert.ToInt32(Math.Round((height / 5) * 3.5))));
            //天车轨道
            g1.FillRectangle(new SolidBrush(Color.FromArgb(238, 233, 233)), new Rectangle(0, Convert.ToInt32(Math.Round((height / 5) * 0.9)), width, 7));
            g1.FillRectangle(new SolidBrush(Color.FromArgb(238, 233, 233)), new Rectangle(0, Convert.ToInt32(Math.Round((height / 5) * 3.5)), width, 7));
            
            //钢三线
            Pen p1 = new Pen(Color.Black, 1);
            g1.DrawLine(p1, Wall4_x, Convert.ToInt32(Math.Round((height / 5) * 3.0)), width, Convert.ToInt32(Math.Round((height / 5) * 3.0)));
            //g1.FillRectangle(new SolidBrush(Color.FromArgb(224, 238, 238)), new Rectangle(Wall4_x, Convert.ToInt32(Math.Round((height / 2.5) * 1.0)), width-Wall4_x, 7));
            //方向标识
            Font f = new Font("黑体", 8);//字体

            p1.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            p1.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            g1.DrawLine(p1, width - 20, 30, width - 20, 8);
            g1.DrawLine(p1, width - 30, 20, width - 10, 20);
            g1.DrawString("北", f, Brushes.Black, width - 35, 5);
            g1.DrawString("东", f, Brushes.Black, width - 15, 23);
            g1.DrawString("钢三线", f, Brushes.Black, Wall1_x + 15, Convert.ToInt32(Math.Round((height / 5) * 2.5)));
            f.Dispose();
            Font f1 = new Font("黑体", 15);//字体
            g1.DrawString("测厚线", f1, Brushes.Black, 1, height - 22);
            f1.Dispose();
            p1.Dispose();
            //一个像素所占的实际距离
            pix_distance_real = Math.Round(Convert.ToDouble((68)) / Convert.ToDouble((Wall1_x - Wall4_x)), 2);
            pix_distance_realy = Math.Round(15 * 1.0000 / xiaocheheight, 4);
            //panelShowThick开始时候对应的实际的坐标值
            panelShowThick_Start_Point_x = Wall1_x_real - pix_distance_real * Wall1_x;
        }

        private void label_jiaziW_Paint(object sender, PaintEventArgs e)
        {
            Graphics g1 = e.Graphics;//画布
            g1.Clear(Color.FromArgb(192, 192, 192));


            int width = label_jiaziE.Size.Width;
            int height = label_jiaziE.Size.Height;
            Pen p1 = new Pen(Color.FromArgb(205, 129, 98), 5);
            g1.DrawArc(p1, new Rectangle(0, 0, width, height), 0, 180);
            Font f = new Font("黑体", 11);//字体
            g1.DrawString(label_jiaziW_exsit_num, f, Brushes.Black, 0, 1);
            f.Dispose();
            p1.Dispose();
        }

        private void label_jiaziE_Paint(object sender, PaintEventArgs e)
        {
            Graphics g1 = e.Graphics;//画布
            g1.Clear(Color.FromArgb(192, 192, 192));
            int width = label_jiaziE.Size.Width;
            int height = label_jiaziE.Size.Height;
            Pen p1 = new Pen(Color.FromArgb(205, 129, 98), 5);
            g1.DrawArc(p1, new Rectangle(0, 0, width, height), 0, 180);
            Font f = new Font("黑体", 11);//字体
            g1.DrawString(label_jiaziE_exsit_num, f, Brushes.Black, 0, 1);
            f.Dispose();
            p1.Dispose();
        }
        private void GetPosiData()
        {
            MySqlConnection myconnection;
            myconnection = new MySqlConnection(SQLServerUser);
            try
            {
                
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();
                }
                if ((tabCtrlMeas.SelectedIndex == 0) && (myconnection.State == ConnectionState.Open))
                {
                    MySQLConnection1.Image = Properties.Resources.green2;
                }
                if ((tabCtrlMeas.SelectedIndex == 1) && (myconnection.State == ConnectionState.Open))
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                string SQL = "SELECT * FROM tbgz_view_ak_hctc2 ORDER BY ID";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                DataSet ds = new DataSet();
                using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                {
                    objDataAdpter.Fill(ds, "tbgz_view_ak_hctc2");
                }
                string strSQL = "update sysinteractinstr_copy1 set headheart = (headheart+1)%30000  where id = 1";
                using (MySqlCommand strthisCommand = new MySqlCommand(strSQL, myconnection))
                {
                    strthisCommand.ExecuteNonQuery();
                }   
                if ((tabCtrlMeas.SelectedIndex == 0) || (tabCtrlMeas.SelectedIndex == 2))
                {
                    skytrain104_x = Convert.ToInt32(Wall1_x_real - Convert.ToInt32(ds.Tables[0].Rows[0][4]) - tempoffset);//104天车西东X位置
                    skytrain105_x = Convert.ToInt32(Wall1_x_real - Convert.ToInt32(ds.Tables[0].Rows[1][4]) - tempoffset);//105天车西东X位置
                    skytrain106_x = Convert.ToInt32(Wall1_x_real - Convert.ToInt32(ds.Tables[0].Rows[2][4]) - tempoffset);//106天车西东X位置
                    heavyLadle104_exsit_num = (ds.Tables[0].Rows[0][11].ToString());//104天车重包号，不存在即为NULL
                    heavyLadle105_exsit_num = (ds.Tables[0].Rows[1][11].ToString());//105天车重包号，不存在即为NULL
                    heavyLadle106_exsit_num = (ds.Tables[0].Rows[2][11].ToString());//106天女重包号，不存在即为NULL
                    label_jiaziE1_exsit_num = ds.Tables[0].Rows[11][15].ToString();//测温架子E1号
                    label_jiaziE2_exsit_num = ds.Tables[0].Rows[12][15].ToString();//测温架子E2号
                    label_jiaziE3_exsit_num = ds.Tables[0].Rows[13][15].ToString();//测温架子E3号
                    label_jiaziE4_exsit_num = ds.Tables[0].Rows[14][15].ToString();//测温架子E4号
                    label_jiaziE1_heavyLadle_exsit_num = ds.Tables[0].Rows[11][11].ToString();//测温架子E1重包包号；不存在即为NULL
                    label_jiaziE2_heavyLadle_exsit_num = ds.Tables[0].Rows[12][11].ToString();//测温架子E2重包包号；不存在即为NULL
                    label_jiaziE3_heavyLadle_exsit_num = ds.Tables[0].Rows[13][11].ToString();//测温架子E3重包包号；不存在即为NULL
                    label_jiaziE4_heavyLadle_exsit_num = ds.Tables[0].Rows[14][11].ToString();//测温架子E4重包包号；不存在即为NULL
                }
                
                if ((tabCtrlMeas.SelectedIndex == 1) || (tabCtrlMeas.SelectedIndex == 2))
                {
                    skytrain111_x = Convert.ToInt32(Convert.ToInt32(ds.Tables[0].Rows[3][4]) - 7.1) - Convert.ToInt32(panelShowThick_Start_Point_x);//111天车东西X位置
                    skytrain112_x = Convert.ToInt32(Convert.ToInt32(ds.Tables[0].Rows[4][4]) - 7.1) - Convert.ToInt32(panelShowThick_Start_Point_x);//112天车东西X位置
                    skytrain127_x = Convert.ToInt32(Convert.ToInt32(ds.Tables[0].Rows[6][4]) - 5.1) - Convert.ToInt32(panelShowThick_Start_Point_x);//127天车东西X位置
                    emptyLaddle111_exsit_num = (ds.Tables[0].Rows[3][11].ToString());//111天车空包号：不存在即为NULL
                    emptyLaddle112_exsit_num = (ds.Tables[0].Rows[4][11].ToString());//112天车空包号：不存在即为NULL
                    emptyLaddle127_exsit_num = (ds.Tables[0].Rows[6][11].ToString());//127天车空包号：不存在即为NULL
                    label_jiaziE_exsit_num = ds.Tables[0].Rows[15][15].ToString();//测厚架子E号
                    label_jiaziW_exsit_num = ds.Tables[0].Rows[16][15].ToString();//测厚架子W号
                    label_jiaziE_emptyLaddle_exsit_num = ds.Tables[0].Rows[15][11].ToString();//测厚架子E号空包包号；不存在即为NULL
                    label_jiaziW_emptyLaddle_exsit_num = ds.Tables[0].Rows[16][11].ToString();//测厚架子W号空包包号；不存在即为NULL

                    SQL = "SELECT * FROM crnblkcurpos where id = 1";
                    objDataAdpter = new MySqlDataAdapter();
                    ds = new DataSet();
                    using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                    {
                        objDataAdpter.Fill(ds, "crnblkcurpos");
                    }
                    label_thicktrain_x = (Convert.ToInt32(ds.Tables[0].Rows[0][1]));//小车的位置是啥，具体检查
                    label_thicktrain_y = (Convert.ToInt32(ds.Tables[0].Rows[0][2]));//
                }
                SQL = "update crnblkcurpos set Heart = (Heart+1)%30000  where id = 1";
                using (MySqlCommand thisCommand = new MySqlCommand(SQL, myconnection))
                {
                    thisCommand.ExecuteNonQuery();
                }
                myconnection.Close();
            }
            catch (Exception ee)
            {
                if ((tabCtrlMeas.SelectedIndex == 0) && (myconnection.State == ConnectionState.Closed))
                {
                    MySQLConnection1.Image = Properties.Resources.redshine;
                }
                if ((tabCtrlMeas.SelectedIndex == 1) && (myconnection.State == ConnectionState.Closed))
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }


        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //0.变量初始化
            if ((tabCtrlMeas.SelectedIndex == 0) || (tabCtrlMeas.SelectedIndex == 2))
            {
                 skytrain104_x = 0;
                 skytrain105_x = 0;
                 skytrain106_x = 0;
                 heavyLadle104_exsit_num = "";
                 heavyLadle105_exsit_num = "";
                 heavyLadle106_exsit_num = "";
                 label_jiaziE1_exsit_num = "";
                 label_jiaziE2_exsit_num = "";
                 label_jiaziE3_exsit_num = "";
                 label_jiaziE4_exsit_num = "";
                 label_jiaziE1_heavyLadle_exsit_num = "";
            }

            if ((tabCtrlMeas.SelectedIndex == 1) || (tabCtrlMeas.SelectedIndex == 2))
            {
                skytrain111_x = 0;
                skytrain112_x = 0;
                skytrain127_x = 0;
                emptyLaddle111_exsit_num = "";
                emptyLaddle112_exsit_num = "";
                emptyLaddle127_exsit_num = "";
                label_jiaziE_exsit_num = "";
                label_jiaziW_exsit_num = "";
                label_thicktrain_x = 0;
                label_thicktrain_y = 0;
            }


            //1. 读数据库 表，获得绘图变量
            GetPosiData();

            //2.使用绘图变量绘图
            if ((tabCtrlMeas.SelectedIndex == 0) || (tabCtrlMeas.SelectedIndex == 2))
            {
                //温度天车画图
                //104天车
                label_skytrain104.Location = new Point(Convert.ToInt32(skytrain104_x * pix_distance_real2) - 10, skyreain_y_Posi_temp - 20);
                label1_skytrain104.Location = new Point(Convert.ToInt32(skytrain104_x * pix_distance_real2), skyreain_y_Posi_temp);
                if ((heavyLadle104_exsit_num == "")||(heavyLadle104_exsit_num == null))
                {
                    label_HeavyLaddle104.Visible = false;
                }
                else
                {
                    label_HeavyLaddle104.Location = new Point(Convert.ToInt32(skytrain104_x * pix_distance_real2) - 5, skyreain_y_Posi_temp + 21);
                    label_HeavyLaddle104.Text = heavyLadle104_exsit_num.Substring(heavyLadle104_exsit_num.Length - 2);
                    label_HeavyLaddle104.Visible = true;
                }
                //105天车
                label_skytrain105.Location = new Point(Convert.ToInt32(skytrain105_x * pix_distance_real2) - 10, skyreain_y_Posi_temp - 20);
                label1_skytrain105.Location = new Point(Convert.ToInt32(skytrain105_x * pix_distance_real2), skyreain_y_Posi_temp);
                if ((heavyLadle105_exsit_num == "")||(heavyLadle105_exsit_num == null))
                {
                    label_HeavyLaddle105.Visible = false;
                }
                else
                {
                    label_HeavyLaddle105.Location = new Point(Convert.ToInt32(skytrain105_x * pix_distance_real2) - 5, skyreain_y_Posi_temp + 21);
                    label_HeavyLaddle105.Text = heavyLadle105_exsit_num.Substring(heavyLadle105_exsit_num.Length - 2);
                    label_HeavyLaddle105.Visible = true;
                }
                //106天车
                label_skytrain106.Location = new Point(Convert.ToInt32(skytrain106_x * pix_distance_real2) - 10, skyreain_y_Posi_temp - 20);
                label1_skytrain106.Location = new Point(Convert.ToInt32(skytrain106_x * pix_distance_real2), skyreain_y_Posi_temp);
                if ((heavyLadle106_exsit_num == "")|| (heavyLadle106_exsit_num == null))
                {
                    label_HeavyLaddle106.Visible = false;
                }
                else
                {
                    label_HeavyLaddle106.Location = new Point(Convert.ToInt32(skytrain106_x * pix_distance_real2) - 5, skyreain_y_Posi_temp + 21);
                    label_HeavyLaddle106.Text = heavyLadle106_exsit_num.Substring(heavyLadle106_exsit_num.Length - 2);
                    label_HeavyLaddle106.Visible = true;
                }
                //车架
                if ((label_jiaziE1_exsit_num == "")|| (label_jiaziE1_exsit_num == null))
                {
                    label_jiaziE1.Visible = false;
                }
                else
                {
                    label_jiaziE1.Visible = true;
                    label_jiaziE1.Refresh();
                }
                if ((label_jiaziE2_exsit_num == "")|| (label_jiaziE2_exsit_num == null))
                {
                    label_jiaziE2.Visible = false;
                }
                else
                {
                    label_jiaziE2.Visible = true;
                    label_jiaziE2.Refresh();
                }
                if ((label_jiaziE3_exsit_num == "")|| (label_jiaziE3_exsit_num == null))
                {
                    label_jiaziE3.Visible = false;
                }
                else
                {
                    label_jiaziE3.Visible = true;
                    label_jiaziE3.Refresh();
                }
                if ((label_jiaziE4_exsit_num == "")|| (label_jiaziE4_exsit_num == null))
                {
                    label_jiaziE4.Visible = false;
                }
                else
                {
                    label_jiaziE4.Visible = true;
                    label_jiaziE4.Refresh();
                }
                //架子上的包
                if ((label_jiaziE1_heavyLadle_exsit_num == "")||(label_jiaziE1_heavyLadle_exsit_num == null))
                {
                    label_jiaziE1_heavyLadle.Visible = false;
                }
                else
                {
                    label_jiaziE1_heavyLadle.Text = label_jiaziE1_heavyLadle_exsit_num.Substring(label_jiaziE1_heavyLadle_exsit_num.Length - 2);
                    label_jiaziE1_heavyLadle.Visible = true;

                }
                if ((label_jiaziE2_heavyLadle_exsit_num == "")||(label_jiaziE2_heavyLadle_exsit_num == null))
                {
                    label_jiaziE2_heavyLadle.Visible = false;
                }
                else
                {
                    label_jiaziE2_heavyLadle.Text = label_jiaziE2_heavyLadle_exsit_num.Substring(label_jiaziE2_heavyLadle_exsit_num.Length - 2);
                    label_jiaziE2_heavyLadle.Visible = true;
                }
                if ((label_jiaziE3_heavyLadle_exsit_num == "")|| (label_jiaziE3_heavyLadle_exsit_num == null))
                {
                    label_jiaziE3_heavyLadle.Visible = false;
                }
                else
                {
                    label_jiaziE3_heavyLadle.Text = label_jiaziE3_heavyLadle_exsit_num.Substring(label_jiaziE3_heavyLadle_exsit_num.Length - 2);
                    label_jiaziE3_heavyLadle.Visible = true;

                }
                if ((label_jiaziE4_heavyLadle_exsit_num == "")|| (label_jiaziE4_heavyLadle_exsit_num == null))
                {
                    label_jiaziE4_heavyLadle.Visible = false;
                }
                else
                {
                    label_jiaziE4_heavyLadle.Text = label_jiaziE4_heavyLadle_exsit_num.Substring(label_jiaziE4_heavyLadle_exsit_num.Length - 2);
                    label_jiaziE4_heavyLadle.Visible = true;
                }
            }
            if (tabCtrlMeas.SelectedIndex == 1) 
            {
                label_skytrain112.Location = new Point(Convert.ToInt32(skytrain112_x / pix_distance_real) - 10, skyreain_y_Posi - 20);
                label1_skytrain112.Location = new Point(Convert.ToInt32(skytrain112_x / pix_distance_real), skyreain_y_Posi);
                if ((emptyLaddle112_exsit_num == "")||(emptyLaddle112_exsit_num == null))
                {
                    label_EmptyLaddle112.Visible = false;
                }
                else
                {
                    label_EmptyLaddle112.Location = new Point(Convert.ToInt32(skytrain112_x / pix_distance_real) - 12, skyreain_y_Posi + 21);
                    label_EmptyLaddle112.Text = emptyLaddle112_exsit_num.Substring(emptyLaddle112_exsit_num.Length - 2);
                    label_EmptyLaddle112.Visible = true;
                }
                //111
                label_skytrain111.Location = new Point(Convert.ToInt32(skytrain111_x / pix_distance_real) - 8, skyreain_y_Posi - 20);
                label1_skytrain111.Location = new Point(Convert.ToInt32(skytrain111_x / pix_distance_real), skyreain_y_Posi);
                if ((emptyLaddle111_exsit_num == "")||(emptyLaddle111_exsit_num == null))
                {
                    label_EmptyLaddle111.Visible = false;
                }
                else
                {
                    label_EmptyLaddle111.Location = new Point(Convert.ToInt32(skytrain111_x / pix_distance_real) - 12, skyreain_y_Posi + 21);
                    label_EmptyLaddle111.Text = emptyLaddle111_exsit_num.Substring(emptyLaddle111_exsit_num.Length - 2);
                    label_EmptyLaddle111.Visible = true;
                }

                //127
                label_skytrain127.Location = new Point(Convert.ToInt32(skytrain127_x / pix_distance_real) - 10, skyreain_y_Posi - 20);
                label1_skytrain127.Location = new Point(Convert.ToInt32(skytrain127_x / pix_distance_real), skyreain_y_Posi);
                if ((emptyLaddle127_exsit_num == "")||(emptyLaddle127_exsit_num == null))
                {
                    label_EmptyLaddle127.Visible = false;
                }
                else
                {
                    label_EmptyLaddle127.Location = new Point(Convert.ToInt32(skytrain127_x / pix_distance_real) - 10, skyreain_y_Posi + 21);
                    label_EmptyLaddle127.Text = emptyLaddle127_exsit_num.Substring(emptyLaddle127_exsit_num.Length - 2);
                    label_EmptyLaddle127.Visible = true;
                }
                //小车
                label_thicktrain_x = Convert.ToInt32(Wall1_x - Math.Round(Convert.ToDouble(label_thicktrain_x / 1000) / pix_distance_real, 4));
                double a;
                a = Math.Round(Convert.ToDouble(label_thicktrain_y / 1000) / pix_distance_realy);
                label_thicktrain_y = Convert.ToInt32(skyreain_y_Posi + Math.Round(Convert.ToDouble(label_thicktrain_y / 1000) / pix_distance_realy));
                label_thicktrain.Location = new Point(label_thicktrain_x - 10, skyreain_y_Posi - 20);
                label1_thicktrain.Location = new Point(label_thicktrain_x, skyreain_y_Posi);
                label1_xiaoche.Location = new Point(label_thicktrain_x - 10, label_thicktrain_y - 10);
                //车架
                if ((label_jiaziE_exsit_num == "")||(label_jiaziE_exsit_num == null))
                {
                    label_jiaziE.Visible = false;
                }
                else
                {
                    label_jiaziE.Visible = true;
                    label_jiaziE.Refresh();
                }
                if ((label_jiaziW_exsit_num == "")|| (label_jiaziW_exsit_num == null))
                {
                    label_jiaziW.Visible = false;
                }
                else
                {
                    label_jiaziW.Visible = true;
                    label_jiaziW.Refresh();
                }
                //架子上的包
                if ((label_jiaziE_emptyLaddle_exsit_num == "")|| (label_jiaziE_emptyLaddle_exsit_num == null))
                {
                    label_jiaziE_emptyLadle.Visible = false;
                }
                else
                {
                    label_jiaziE_emptyLadle.Text = label_jiaziE_emptyLaddle_exsit_num.Substring(label_jiaziE_emptyLaddle_exsit_num.Length - 2);
                    label_jiaziE_emptyLadle.Visible = true;

                }
                if ((label_jiaziW_emptyLaddle_exsit_num == "")||(label_jiaziW_emptyLaddle_exsit_num == null))
                {
                    label_jiaziW_emptyLadle.Visible = false;
                }
                else
                {
                    label_jiaziW_emptyLadle.Text = label_jiaziW_emptyLaddle_exsit_num.Substring(label_jiaziW_emptyLaddle_exsit_num.Length - 2);
                    label_jiaziW_emptyLadle.Visible = true;
                }
            }
            
        

        }
        UInt16[] data1 = new UInt16[640 * 480];
        UInt16[] data2 = new UInt16[640 * 480];
        UInt16[] data3 = new UInt16[640 * 480];
        UInt16[] data4 = new UInt16[640 * 480];
        UInt16[] data5 = new UInt16[640 * 480];


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

        private void panelShowTemp_Paint(object sender, PaintEventArgs e)
        {
            Graphics g1 = e.Graphics;//画布
            g1.Clear(Color.FromArgb(192, 192, 192));

            int width = panelShowTemp.Size.Width;
            int height = panelShowTemp.Size.Height;
            skyreain_y_Posi_temp = Convert.ToInt32(Math.Round((height / 5) * 1.3));
            int skytrainheight = Convert.ToInt32(Math.Round((height / 5) * 3.8)) - Convert.ToInt32(Math.Round((height / 5) * 1.3));
            label1_skytrain104.Height = skytrainheight;
            label1_skytrain105.Height = skytrainheight;
            label1_skytrain106.Height = skytrainheight;
            //一个像素所占的实际距离
            pix_distance_real2 = Math.Round(Convert.ToDouble(Wall1_x_real * 1.00 / width), 2);
            int railwayEnd = Convert.ToInt32(pix_distance_real2 * 60);
            //天车轨道
            g1.FillRectangle(new SolidBrush(Color.FromArgb(238, 233, 233)), new Rectangle(0, Convert.ToInt32(Math.Round((height / 5) * 1.3)), width, 7));
            g1.FillRectangle(new SolidBrush(Color.FromArgb(238, 233, 233)), new Rectangle(0, Convert.ToInt32(Math.Round((height / 5) * 3.6)), width, 7));
            //钢1.2线
            Pen p1 = new Pen(Color.Black, 1);
            g1.DrawLine(p1, 0, Convert.ToInt32(Math.Round((height / 5) * 2.6)), railwayEnd, Convert.ToInt32(Math.Round((height / 5) * 2.6)));
            g1.DrawLine(p1, 0, Convert.ToInt32(Math.Round((height / 5) * 3.2)), railwayEnd, Convert.ToInt32(Math.Round((height / 5) * 3.2)));
            //方向标识

            Font f = new Font("黑体", 8);//字体

            p1.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            p1.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            g1.DrawLine(p1, width - 20, 30, width - 20, 8);
            g1.DrawLine(p1, width - 30, 20, width - 10, 20);
            g1.DrawString("南", f, Brushes.Black, width - 35, 5);
            g1.DrawString("西", f, Brushes.Black, width - 15, 23);
            g1.DrawString("钢二线", f, Brushes.Black, 1, Convert.ToInt32(Math.Round((height / 5) * 2.1)));
            g1.DrawString("钢一线", f, Brushes.Black, 1, Convert.ToInt32(Math.Round((height / 5) * 3.3)));
            f.Dispose();
            Font f1 = new Font("黑体", 15);//字体
            g1.DrawString("测温线", f1, Brushes.Black, width - 70, height - 22);
            f1.Dispose();
            p1.Dispose();
        }

        private void label_jiaziE1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g1 = e.Graphics;//画布
            g1.Clear(Color.FromArgb(192, 192, 192));
            int width = label_jiaziE1.Size.Width;
            int height = label_jiaziE1.Size.Height;
            Pen p1 = new Pen(Color.FromArgb(205, 129, 98), 5);
            g1.DrawArc(p1, new Rectangle(0, 0, width, height), 0, 180);
            Font f = new Font("黑体", 11);//字体
            g1.DrawString(label_jiaziE1_exsit_num, f, Brushes.Black, 0, 2);
            f.Dispose();
            p1.Dispose();
        }

        private void label_jiaziE2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g1 = e.Graphics;//画布
            g1.Clear(Color.FromArgb(192, 192, 192));
            int width = label_jiaziE1.Size.Width;
            int height = label_jiaziE1.Size.Height;
            Pen p1 = new Pen(Color.FromArgb(205, 129, 98), 5);
            g1.DrawArc(p1, new Rectangle(0, 0, width, height), 0, 180);
            Font f = new Font("黑体", 11);//字体
            g1.DrawString(label_jiaziE1_exsit_num, f, Brushes.Black, 0, 2);
            f.Dispose();
            p1.Dispose();
        }

        private void label_jiaziE3_Paint(object sender, PaintEventArgs e)
        {
            Graphics g1 = e.Graphics;//画布
            g1.Clear(Color.FromArgb(192, 192, 192));
            int width = label_jiaziE3.Size.Width;
            int height = label_jiaziE3.Size.Height;
            Pen p1 = new Pen(Color.FromArgb(205, 129, 98), 5);
            g1.DrawArc(p1, new Rectangle(0, 0, width, height), 0, 180);
            Font f = new Font("黑体", 11);//字体
            g1.DrawString(label_jiaziE3_exsit_num, f, Brushes.Black, 0, 2);
            f.Dispose();
            p1.Dispose();
        }

        private void label_jiaziE4_Paint(object sender, PaintEventArgs e)
        {
            Graphics g1 = e.Graphics;//画布
            g1.Clear(Color.FromArgb(192, 192, 192));
            int width = label_jiaziE4.Size.Width;
            int height = label_jiaziE4.Size.Height;
            Pen p1 = new Pen(Color.FromArgb(205, 129, 98), 5);
            g1.DrawArc(p1, new Rectangle(0, 0, width, height), 0, 180);
            Font f = new Font("黑体", 11);//字体
            g1.DrawString(label_jiaziE4_exsit_num, f, Brushes.Black, 0, 2);
            f.Dispose();
            p1.Dispose();
        }

        bool blBotTemp = true;
        private void TempeReadBlob()
        {
            blBotTemp = true;
            bDrawTemp[0] = false; bDrawTemp[1] = false; bDrawTemp[2] = false; bDrawTemp[3] = false; bDrawTemp[4] = false;
            string timenow1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //label1.Text = timenow1;
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");

            try
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if ((tabCtrlMeas.SelectedIndex == 0) && (myconnection.State == ConnectionState.Open))
                {
                    MySQLConnection1.Image = Properties.Resources.green2;
                }
                if ((tabCtrlMeas.SelectedIndex == 1) && (myconnection.State == ConnectionState.Open))
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                //string SQL = "SELECT CircumTemp1,CircumTemp2,CircumTemp3,CircumTemp4,BottomTemp FROM tmptempe where id=" + TbID+"";
                string SQL = "SELECT CircumTemp1,CircumTemp2,CircumTemp3,CircumTemp4,BottomTemp FROM heavyldl_meased where id=" + TbID + "";
                //MySqlCommand cmd = new MySqlCommand(SQL, myconnection);
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                DataSet ds = new DataSet();
                using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                {
                    objDataAdpter.Fill(ds, "heavyldl_meased");
                }
                {
                    bDrawTemp[0] = true; bDrawTemp[1] = true; bDrawTemp[2] = true; bDrawTemp[3] = true; bDrawTemp[4] = true;
                    if (ds.Tables[0].Rows[0][0] == System.DBNull.Value)
                    {
                        bDrawTemp[0] = false;
                    }
                    if (ds.Tables[0].Rows[0][1] == System.DBNull.Value)
                    {
                        bDrawTemp[1] = false;
                    }
                    if (ds.Tables[0].Rows[0][2] == System.DBNull.Value)
                    {
                        bDrawTemp[2] = false;
                    }
                    if (ds.Tables[0].Rows[0][3] == System.DBNull.Value)
                    {
                        bDrawTemp[3] = false;
                    }
                    if (ds.Tables[0].Rows[0][4] == System.DBNull.Value)
                    {
                        bDrawTemp[4] = false;
                    }
                    long len;
                    int inlen = 640 * 480 * 2;
                    byte[] buffer = new byte[inlen];

                    if (bDrawTemp[0])
                    {
                        //存放获得的二进制数据，温度
                        buffer = (byte[])ds.Tables[0].Rows[0][0];

                        for (int i = 0; i < inlen / 2; i++)
                        {
                            data1[i] = (UInt16)((buffer[2 * i]) + (buffer[2 * i + 1]) * 256);
                            //获得字节数组
                        }
                    }


                    if (bDrawTemp[1])
                    {
                        buffer = (byte[])ds.Tables[0].Rows[0][1];
                        for (int i = 0; i < inlen / 2; i++)
                        {
                            data2[i] = (UInt16)((buffer[2 * i]) + (buffer[2 * i + 1]) * 256);

                        }

                    }

                    if (bDrawTemp[2])
                    {
                        buffer = (byte[])ds.Tables[0].Rows[0][2];
                        for (int i = 0; i < inlen / 2; i++)
                        {
                            data3[i] = (UInt16)((buffer[2 * i]) + (buffer[2 * i + 1]) * 256);

                        }
                    }

                    if (bDrawTemp[3])
                    {
                        buffer = (byte[])ds.Tables[0].Rows[0][3];
                        for (int i = 0; i < inlen / 2; i++)
                        {
                            data4[i] = (UInt16)((buffer[2 * i]) + (buffer[2 * i + 1]) * 256);

                        }

                    }

                    if (bDrawTemp[4])
                    {
                        buffer = (byte[])ds.Tables[0].Rows[0][4];
                        //FileStream fs5 = new FileStream("D:\\ak5.txt", FileMode.Append, FileAccess.Write);
                        //StreamWriter sw5 = new StreamWriter(fs5);
                        for (int i = 0; i < inlen / 2; i++)
                        {
                            data5[i] = (UInt16)((buffer[2 * i]) + (buffer[2 * i + 1]) * 256);
                            //string content5 = data5[i].ToString();
                            //开始写入
                            //sw5.WriteLine(content5);
                            //清空缓冲区、关闭流
                            //sw5.Flush();
                        }
                        //fs5.Close();
                    }


                }
            }
            catch (Exception EE)
            {
                if ((tabCtrlMeas.SelectedIndex == 0) && (myconnection.State == ConnectionState.Closed))
                {
                    MySQLConnection1.Image = Properties.Resources.redshine;
                }
                if ((tabCtrlMeas.SelectedIndex == 1) && (myconnection.State == ConnectionState.Closed))
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
            finally
            {
                myconnection.Close();
            }
        }

        double g_dwf, b_dwf;
        double g_dhf, b_dhf;
        int g_iCirPartRows = 640;
        int g_iCirPartCols = 480;
        int indLim = 24;
        int iDiv = 10;
        int iDiv1 = 15;


        private void timer1_Tick(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            try
            {
                //定义一个mysql数据库连接对象
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if ((tabCtrlMeas.SelectedIndex == 0) && (myconnection.State == ConnectionState.Open))
                {
                    MySQLConnection1.Image = Properties.Resources.green2;
                }
                if ((tabCtrlMeas.SelectedIndex == 1) && (myconnection.State == ConnectionState.Open))
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                if ((tabCtrlMeas.SelectedIndex == 0) || (tabCtrlMeas.SelectedIndex == 2))
                {
                    string SQL = "select SerNo 序号,CrnBlkNo 位置,LadleNo 包号,LadleAge 包龄  from heavyldl_tomeas";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    DataSet ds = new DataSet();
                    using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                    {
                        objDataAdpter.Fill(ds, "heavyldl_tomeas");
                    }
                    string ladleno104now = ds.Tables[0].Rows[0][2].ToString();
                    string ladleno105now = ds.Tables[0].Rows[1][2].ToString();
                    if ((ladleno104now != ladleno104last) || (ladleno105now != ladleno105last))
                    {
                        Heavyldl_ToMeas.DataSource = ds.Tables[0];
                        ladleno104last = ladleno104now;
                        ladleno105last = ladleno105now;
                    }
                    //数据表的测量时间降序排列取前10行，显示部分列到界面
                    string SQL2 = "select id, LadleNo,LadleServDuty, LadleAge, MeasTm , MaxTemp,MaxTempPos  from heavyldl_meased where `Delete`=0 order by id desc limit 0,50";
                    MySqlDataAdapter objDataAdpter2 = new MySqlDataAdapter();
                    DataSet ds2 = new DataSet();
                    using (objDataAdpter2.SelectCommand = new MySqlCommand(SQL2, myconnection))
                    {
                        objDataAdpter2.Fill(ds2, "heavyldl_meased");
                    }
                    int rowsnowHeavyldl_Meased = Convert.ToInt32(ds2.Tables[0].Rows[0][0]);
                    if ((rowsnowHeavyldl_Meased - rowslastHeavyldl_Meased) == 1)
                    {
                        Heavyldl_Meased.AutoGenerateColumns = false;
                        Heavyldl_Meased.DataSource = ds2.Tables[0];
                        //为温度图中高温区画框标记做准备，从已测重包表里提取图号、最高温度点坐标存放到程序变量                
                        TbID = rowsnowHeavyldl_Meased;
                        string SQLMaxTempImager = "select MaxTempImagerNo,MaxTempXbyPixel,MaxTempYbyPixel  from heavyldl_meased where id="+ TbID +";";
                        MySqlDataAdapter objDataAdpterMaxTempImager = new MySqlDataAdapter();
                        DataSet dsMaxTempImager = new DataSet();
                        using (objDataAdpterMaxTempImager.SelectCommand = new MySqlCommand(SQLMaxTempImager, myconnection))
                        {
                            objDataAdpterMaxTempImager.Fill(dsMaxTempImager, "heavyldl_meased");
                        }
                        iMaxTempImagerNo = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][0]);
                        iMaxTempXinPixel = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][1]);
                        iMaxTempYbyPixel = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][2]);
                        bDrawMaxTempScope = new bool[5] { false, false, false, false, false };
                        if (iMaxTempImagerNo != 0)
                        {
                            bDrawMaxTempScope[iMaxTempImagerNo - 1] = true;
                        }
                        
                        ShowtempPic();
                        rowslastHeavyldl_Meased = rowsnowHeavyldl_Meased;
                    }
                    string SQL6 = "SELECT controlheart FROM sysinteractinstr_copy1";
                    MySqlDataAdapter objDataAdpter6 = new MySqlDataAdapter();
                    DataSet ds6 = new DataSet();
                    using (objDataAdpter6.SelectCommand = new MySqlCommand(SQL6, myconnection))
                    {
                        objDataAdpter6.Fill(ds6, "sysinteractinstr_copy1");
                    }
                    controlheartnow = Convert.ToInt32(ds6.Tables[0].Rows[0][0]);//后台心跳
                    if (controlheartnow != controlheartlast)
                    {
                        houtaiHeart1.Image = Properties.Resources.green2;
                        controlheartlast = controlheartnow;
                    }
                    else if (controlheartnow == controlheartlast)
                    {
                        houtaiHeart1.Image = Properties.Resources.redshine;
                    }
                }
                if ((tabCtrlMeas.SelectedIndex == 1) || (tabCtrlMeas.SelectedIndex == 2))
                {
                    string SQL4 = "select id,LadleNo,LadleServDuty,LadleAge,MeasTm,MinThick,MinThickPos,StatusCode,ModeType  from emptyldl_meased where `Delete`=0 order by id desc limit 0,50;";
                    MySqlDataAdapter objDataAdpter4 = new MySqlDataAdapter();
                    DataSet ds4 = new DataSet();
                    using (objDataAdpter4.SelectCommand = new MySqlCommand(SQL4, myconnection))
                    {
                        objDataAdpter4.Fill(ds4, "emptyldl_meased");
                    }
                    int rowsnowdtaGrdViewEmtyMeased = Convert.ToInt32(ds4.Tables[0].Rows[0][0]);
                    if (((rowsnowdtaGrdViewEmtyMeased - rowslastdtaGrdViewEmtyMeased) >= 1)||(dtaGrdViewEmtyMeased.Rows.Count==0))
                    {
                        dtaGrdViewEmtyMeased.AutoGenerateColumns = false;
                        dtaGrdViewEmtyMeased.DataSource = ds4.Tables[0];
                        ElID = rowsnowdtaGrdViewEmtyMeased;
                        ThickMeasTm = Convert.ToDateTime(ds4.Tables[0].Rows[0][4]);
                        ThickStatusCode = Convert.ToInt32(ds4.Tables[0].Rows[0][7]);
                        ShowthickImage();
                        rowslastdtaGrdViewEmtyMeased = rowsnowdtaGrdViewEmtyMeased;
                    }
                }
                    
                if ((tabCtrlMeas.SelectedIndex == 1))
                {
                    if (myconnection.State == ConnectionState.Closed)
                    {
                        tiancheAvoid.Image = Properties.Resources.black;
                        tiancheBroken.Image = Properties.Resources.black;
                        tiancheRomotrMode.Image = Properties.Resources.black;
                        tiancheHurryStop.Image = Properties.Resources.black;
                        tiancheDoorOpen.Image = Properties.Resources.black;
                        headTotiancheHeartCount.Image = Properties.Resources.black;
                        tiancheHeart.Image = Properties.Resources.black;
                        thickHeart.Image = Properties.Resources.black;
                    }
                    string SQL5 = "SELECT controlheart,tianchestatus01,tianchestatus02,tianchestatus03,tianchestatus04,tianchestatus05,tianchestatus06,tianchestatus07,tianchestatus08,tianchestatus11,tianchestatus12,tiancheheart,thickheartstatus,thickTmpOut,thickHumid,thickwarning,gbgzstatus FROM sysinteractinstr_copy1";
                    MySqlDataAdapter objDataAdpter5 = new MySqlDataAdapter();
                    objDataAdpter5.SelectCommand = new MySqlCommand(SQL5, myconnection);
                    DataSet ds5 = new DataSet();
                    objDataAdpter5.Fill(ds5, "sysinteractinstr_copy1");
                    controlheartnow = Convert.ToInt32(ds5.Tables[0].Rows[0][0]);//后台心跳
                    Status12t_1 = Convert.ToInt32(ds5.Tables[0].Rows[0][1]);//紧急避让行车
                    Status12t_2 = Convert.ToInt32(ds5.Tables[0].Rows[0][2]);//行车故障
                    Status12t_3 = Convert.ToInt32(ds5.Tables[0].Rows[0][3]);//本地操作模式
                    Status12t_4 = Convert.ToInt32(ds5.Tables[0].Rows[0][4]);//紧急停车
                    Status12t_5 = Convert.ToInt32(ds5.Tables[0].Rows[0][5]);//行车上车门未关
                    Status12t_6 = Convert.ToInt32(ds5.Tables[0].Rows[0][6]);//心跳计数器故障
                    Status12t_7 = Convert.ToInt32(ds5.Tables[0].Rows[0][7]);//行车上车门未关
                    Status12t_8 = Convert.ToInt32(ds5.Tables[0].Rows[0][8]);//心跳计数器故障
                    Status12t_11 = Convert.ToInt32(ds5.Tables[0].Rows[0][9]);//心跳计数器故障
                    Status12t_12 = Convert.ToInt32(ds5.Tables[0].Rows[0][10]);//心跳计数器故障
                    tiancheheart = Convert.ToInt32(ds5.Tables[0].Rows[0][11]);
                    thickheartstatus = Convert.ToInt32(ds5.Tables[0].Rows[0][12]);
                    thickTmpOut = Convert.ToSingle(ds5.Tables[0].Rows[0][13]);
                    thickHumid = Convert.ToSingle(ds5.Tables[0].Rows[0][14]);
                    thickwarning = Convert.ToInt32(ds5.Tables[0].Rows[0][15]);
                    gbgzStatus= Convert.ToInt32(ds5.Tables[0].Rows[0][16]);
                    if (controlheartnow != controlheartlast)
                    {
                        houtaiHeart.Image = Properties.Resources.green2;
                        controlheartlast = controlheartnow;
                    }
                    else if (controlheartnow == controlheartlast) 
                    {
                        houtaiHeart.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_1 == 0)
                    {
                        tiancheAvoid.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_1 == 1)
                    {
                        tiancheAvoid.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_2 == 0)
                    {
                        tiancheBroken.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_2 == 1)
                    {
                        tiancheBroken.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_3 == 0)
                    {
                        tiancheRomotrMode.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_3 == 1)
                    {
                        tiancheRomotrMode.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_4 == 0)
                    {
                        tiancheHurryStop.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_4 == 1)
                    {
                        tiancheHurryStop.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_5 == 0)
                    {
                        tiancheDoorOpen.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_5 == 1)
                    {
                        tiancheDoorOpen.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_6 == 0)
                    {
                        headTotiancheHeartCount.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_6 == 1)
                    {
                        headTotiancheHeartCount.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_7 == 0)
                    {
                        WarningBxComunication.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_7 == 1)
                    {
                        WarningBxComunication.Image = Properties.Resources.redshine;
                    }
                    if (Status12t_8 == 0)
                    {
                        InverterEn.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_8 == 1)
                    {
                        InverterEn.Image = Properties.Resources.red;
                    }
                    if (Status12t_11 == 0)
                    {
                        RackPosBl.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_11 == 1)
                    {
                        RackPosBl.Image = Properties.Resources.red;
                    }
                    if (Status12t_12 == 0)
                    {
                        RemoteEn.Image = Properties.Resources.green2;
                    }
                    else if (Status12t_12 == 1)
                    {
                        RemoteEn.Image = Properties.Resources.red;
                    }
                    if (tiancheheart == 0)
                    {
                        tiancheHeart.Image = Properties.Resources.green2;
                    }
                    else if (tiancheheart == 1)
                    {
                        tiancheHeart.Image = Properties.Resources.redshine;
                    }
                    if (thickheartstatus == 0)
                    {
                        thickHeart.Image = Properties.Resources.green2;
                    }
                    else if (thickheartstatus == 1)
                    {
                        thickHeart.Image = Properties.Resources.redshine;
                    }
                    if (thickTmpOut <= 40)
                    {
                        lblthickTempOut.Text = "温度(℃):" + thickTmpOut.ToString();
                    }
                    else if (thickTmpOut > 40)
                    {
                        lblthickTempOut.Text = "温度(℃):" + thickTmpOut.ToString();
                    }
                    if (thickHumid <= 40)
                    {
                        lblHumidOut.Text = "湿度(%):" + thickHumid.ToString();
                    }
                    else if (thickHumid > 40)
                    {
                        lblHumidOut.Text = "湿度(%):" + thickHumid.ToString();
                    }
                    switch (thickwarning)
                    {
                        case 0:
                            {
                                ThickWorkStatus.Image = Properties.Resources.green2;
                                lblThickWorkStatus.Text = "正常";
                            }
                            break;
                        case 1:
                            {
                                ThickWorkStatus.Image = Properties.Resources.purple;
                                lblThickWorkStatus.Text = "温度预警";
                            }
                            break;
                        case 2:
                            {
                                ThickWorkStatus.Image = Properties.Resources.red;
                                lblThickWorkStatus.Text = "离线状态";
                            }
                            break;
                        case 3:
                            {
                                ThickWorkStatus.Image = Properties.Resources.yellow;
                                lblThickWorkStatus.Text = "空闲状态";
                            }
                            break;
                        case 4:
                            {
                                ThickWorkStatus.Image = Properties.Resources.green2;
                                lblThickWorkStatus.Text = "扫描状态";
                            }
                            break;
                        case 5:
                            {
                                ThickWorkStatus.Image = Properties.Resources.purple;
                                lblThickWorkStatus.Text = "数据传输";
                            }
                            break;
                    }
                    if (gbgzStatus == 0)
                    {
                        gbgzheart.Image = Properties.Resources.green2;
                    }
                    else if (gbgzStatus == 1)
                    {
                        gbgzheart.Image = Properties.Resources.red;
                    }
                }
                myconnection.Close();
            }
            catch (Exception ee)
            {
                if ((tabCtrlMeas.SelectedIndex == 0) && (myconnection.State == ConnectionState.Closed))
                {
                    MySQLConnection1.Image = Properties.Resources.redshine;
                }
                if ((tabCtrlMeas.SelectedIndex == 1) && (myconnection.State == ConnectionState.Closed))
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
        }
        
       
        int g_iOperateMode = 0; //2--手动，1--半自动，0--自动
        
        private void comboBoxSelectMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBoxSelectMode.SelectedItem.ToString())
            {
                case "自动":
                    g_iOperateMode = 0;
                    comboBoxSelectAimPos.Text = "选择目标位";
                    comboBoxSelectAimPos.Enabled = false;
                    txtBoxShowStatus.Text = "请填写包号，示例：T088即88号包！";
                    textBoxLadleNoEast.Text = "T0";
                    textBoxLadleNoEast.Enabled = true;
                    textBoxLadleNoWest.Text = "T0";
                    textBoxLadleNoWest.Enabled = true;
                    XYCarSemiAutoEastPos.Enabled = false;
                    XYCarSemiAutoWestPos.Enabled = false;
                    XYCarSemiAutoEastWestPos.Enabled = false;
                    XCarsToTarget.Enabled = false;
                    YCarToTarget.Enabled = false;
                    MeasThick.Enabled = false;
                    XCarToHome.Enabled = false;
                    YCarToHome.Enabled = false;
                    XYCarsToTarget.Enabled = false;
                    XYCarsTakePhoto.Enabled = false;
                    XYCarsToHome.Enabled = false;
                    butSurePara.Enabled = true;
                    break;
                case "半自动":
                    g_iOperateMode = 1;
                    txtBoxShowStatus.Text = "请选择测量包位！";
                    comboBoxSelectAimPos.Text = "选择目标位";
                    comboBoxSelectAimPos.Enabled = true;
                    textBoxLadleNoEast.Text = "";
                    textBoxLadleNoEast.Enabled = false;
                    textBoxLadleNoWest.Text = "";
                    textBoxLadleNoWest.Enabled = false;
                    XYCarSemiAutoEastPos.Enabled = false;
                    XYCarSemiAutoWestPos.Enabled = false;
                    XYCarSemiAutoEastWestPos.Enabled = false;
                    XCarsToTarget.Enabled = false;
                    YCarToTarget.Enabled = false;
                    MeasThick.Enabled = false;
                    XCarToHome.Enabled = false;
                    YCarToHome.Enabled = false;
                    XYCarsToTarget.Enabled = false;
                    XYCarsTakePhoto.Enabled = false;
                    XYCarsToHome.Enabled = false;
                    butSurePara.Enabled = false;
                    break;
                case "手动":
                    g_iOperateMode = 2;
                    txtBoxShowStatus.Text = "请选择测量包位！";
                    comboBoxSelectAimPos.Text = "选择目标位";
                    comboBoxSelectAimPos.Enabled = true;
                    textBoxLadleNoEast.Text = "";
                    textBoxLadleNoEast.Enabled = false;
                    textBoxLadleNoWest.Text = "";
                    textBoxLadleNoWest.Enabled = false;
                    XYCarSemiAutoEastPos.Enabled = false;
                    XYCarSemiAutoWestPos.Enabled = false;
                    XYCarSemiAutoEastWestPos.Enabled = false;
                    XCarsToTarget.Enabled = false;
                    YCarToTarget.Enabled = false;
                    MeasThick.Enabled = false;
                    XCarToHome.Enabled = false;
                    YCarToHome.Enabled = false;
                    XYCarsToTarget.Enabled = false;
                    XYCarsTakePhoto.Enabled = false;
                    XYCarsToHome.Enabled = false;
                    butSurePara.Enabled = false;
                    break;
                    

            }
        }

        int g_itarget = 0;
        byte g_operation = 0;
        private void comboBoxSelectAimPos_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            switch (comboBoxSelectAimPos.SelectedItem.ToString())
            {
                
                case "东包位":
                    g_itarget = 1;
                    textBoxLadleNoEast.Enabled=true;
                    textBoxLadleNoWest.Enabled = false;
                    textBoxLadleNoEast.Text = "T0";
                    textBoxLadleNoWest.Text = "";
                    txtBoxShowStatus.Text = "请输入包号！示例：输入T088 即是88号包！";
                    butSurePara.Enabled = true;
                    break;
                case "西包位":
                    g_itarget = 2;
                    textBoxLadleNoEast.Enabled = false;
                    textBoxLadleNoWest.Enabled = true;
                    textBoxLadleNoEast.Text = "";
                    textBoxLadleNoWest.Text = "T0";
                    txtBoxShowStatus.Text = "请输入包号！示例：输入T088 即是88号包！";
                    butSurePara.Enabled = true;
                    break;
            }
        }

        class Timeout
        {
            private int TimeoutInterval = 1;// 单位：秒
            public long lastTicks;//用于存储新建操作开始的时间
            public long elapsedTicks;//用于存储操作消耗的时间
            public Timeout(int timeout_in_seconds = 1)
            {
                TimeoutInterval = timeout_in_seconds;
                lastTicks = DateTime.Now.Ticks;
            }
            public bool IsTimeout()
            {
                elapsedTicks = DateTime.Now.Ticks - lastTicks;
                TimeSpan span = new TimeSpan(elapsedTicks);
                double diff = span.TotalSeconds;
                if (diff > TimeoutInterval)
                    return true;
                else
                    return false;
            }
        }
        int[] g_iTimeout = new int[] { 1000, 1000, 1000, 1000, 1000, 1000, 1000,1000, 1000, 1000,1000 };
        bool g_bTimeoutCheckFlag = false;
        bool g_bTimeout = false;
        int controlheartlast, controlheartnow,Status12t_1 = 0, Status12t_2 = 0, Status12t_3 = 0, Status12t_4 = 0, Status12t_5 = 0, Status12t_6 = 0, Status12t_7 = 0, Status12t_8 = 0, Status12t_9 = 0, Status12t_10 = 0, Status12t_11 = 0, Status12t_12 = 0, Status12t_13 = 0, Status12t_14 = 0, Status12t_15 = 0,gbgzStatus=0;

        private void ShowtempPic()
        {
            TempeReadBlob();
            picBxCircumTemp1.Refresh();
            picBxCircumTemp2.Refresh();
            picBxCircumTemp3.Refresh();
            picBxCircumTemp4.Refresh();
            picBxBottomTemp.Refresh();
        }
        private void picBxCircumTemp1_Paint(object sender, PaintEventArgs e)
        {
            
            //画图
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            if (bDrawTemp[0]) 
            {
                int m = g_iCirPartRows, n = g_iCirPartCols;

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
                UInt16 a;
                int b;
                for (double X = 0; X < m; X++)  //  640行
                {
                    for (double Y = 0; Y < n; Y++)  //480列
                    {
                        //x = (double)(rt.Location.X + j * dwf);
                        //y = (double)(rt.Location.Y + i * dhf);
                        x = dwf * Y;
                        y = dhf * X;
                        PTf = new PointF((float)x, (float)y);
                        tmprtf = new RectangleF(PTf, Sf);

                        a = data1[(int)Y * 640 + (int)X];
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


                //如果最高温度在此图像中，则执行画红框，后面几幅温度图paint类似，只有底部温度图x和y交换
                if (bDrawMaxTempScope[0])
                {
                    Point LTPntofRt;
                    Pen PenofRt;
                    Size SzofRt;
                    Rectangle Rt;
                    float Penwid = (float)4.1;
                    LTPntofRt = new Point((int)((iMaxTempYbyPixel) * dwf) - 10, (int)(iMaxTempXinPixel * dhf) - 10);
                    SzofRt = new Size(20, 20);//定义画红框的大小，宽20高20
                    Rt = new Rectangle(LTPntofRt, SzofRt);
                    using (PenofRt = new Pen(Color.Green, Penwid)) 
                    {
                        g.DrawRectangle(PenofRt, Rt);
                    }
                }
            }
            
        }

        private void picBxCircumTemp2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            //int i,j;
            g.Clear(Color.White);
            if (bDrawTemp[1]) 
            {
                int m = g_iCirPartRows, n = g_iCirPartCols;

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
                UInt16 a;
                int b;
                for (int X = 0; X < m; X++)  //  640行
                {
                    for (int Y = 0; Y < n; Y++)  //480列
                    {
                        x = dwf * Y;
                        y = dhf * X;
                        PTf = new PointF((float)x, (float)y);
                        tmprtf = new RectangleF(PTf, Sf);

                        //a = data1[X * 480 + Y];
                        a = data2[Y * 640 + X];
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

                //如果最高温度在此图像中即变量为真，则执行画红框
                if (bDrawMaxTempScope[1])
                {
                    Point LTPntofRt;
                    Pen PenofRt;
                    Size SzofRt;
                    Rectangle Rt;
                    float Penwid = (float)4.1;
                    //LTPntofRt = new Point((int)(iMaxTempXinPixel * dwf) - 10, (int)(iMaxTempYbyPixel * dhf) - 10);
                    LTPntofRt = new Point((int)((iMaxTempYbyPixel) * dwf) - 10, (int)(iMaxTempXinPixel * dhf) - 10);
                    SzofRt = new Size(20, 20);//定义画红框的大小，宽20高20
                    Rt = new Rectangle(LTPntofRt, SzofRt);
                    using (PenofRt = new Pen(Color.Green, Penwid))
                    {
                        g.DrawRectangle(PenofRt, Rt);
                    }
                }
            }
            
            //g.Dispose();
        }

        private void picBxCircumTemp3_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            //int i,j;
            g.Clear(Color.White);
            if (bDrawTemp[2]) 
            {
                int m = g_iCirPartRows, n = g_iCirPartCols;

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
                UInt16 a;
                int b;

                for (int X = 0; X < m; X++)  //  640行
                {
                    for (int Y = 0; Y < n; Y++)  //480列
                    {
                        x = dwf * Y;
                        y = dhf * X;
                        PTf = new PointF((float)x, (float)y);
                        tmprtf = new RectangleF(PTf, Sf);
                        a = data3[Y * 640 + X];
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


                //如果最高温度在此图像中即变量为真，则执行画红框
                if (bDrawMaxTempScope[2])
                {
                    Point LTPntofRt;
                    Pen PenofRt;
                    Size SzofRt;
                    Rectangle Rt;
                    float Penwid = (float)4.1;
                    //LTPntofRt = new Point((int)(iMaxTempXinPixel * dwf) - 10, (int)(iMaxTempYbyPixel * dhf) - 10);
                    LTPntofRt = new Point((int)((iMaxTempYbyPixel) * dwf) - 10, (int)(iMaxTempXinPixel * dhf) - 10); SzofRt = new Size(20, 20);//定义画红框的大小，宽20高20
                    Rt = new Rectangle(LTPntofRt, SzofRt);
                    using (PenofRt = new Pen(Color.Green, Penwid)) 
                    {
                        g.DrawRectangle(PenofRt, Rt);
                    } 
                }
            }
            
            //g.Dispose();
        }
        private void picBxCircumTemp4_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            if (bDrawTemp[3]) 
            {
                int m = g_iCirPartRows, n = g_iCirPartCols;

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
                UInt16 a;
                int b;

                for (int X = 0; X < m; X++)  //  640行
                {
                    for (int Y = 0; Y < n; Y++)  //480列
                    {
                        x = dwf * Y;
                        y = dhf * X;
                        PTf = new PointF((float)x, (float)y);
                        tmprtf = new RectangleF(PTf, Sf);

                        //a = data1[X * 480 + Y];
                        a = data4[Y * 640 + X];
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


                //如果最高温度在此图像中即变量为真，则执行画红框
                if (bDrawMaxTempScope[3])
                {
                    Point LTPntofRt;
                    Pen PenofRt;
                    Size SzofRt;
                    Rectangle Rt;
                    float Penwid = (float)4.1;
                    //LTPntofRt = new Point((int)(iMaxTempXinPixel * dwf) - 10, (int)(iMaxTempYbyPixel * dhf) - 10);
                    LTPntofRt = new Point((int)((iMaxTempYbyPixel) * dwf) - 10, (int)(iMaxTempXinPixel * dhf) - 10);
                    SzofRt = new Size(20, 20);//定义画红框的大小，宽20高20
                    Rt = new Rectangle(LTPntofRt, SzofRt);
                    using (PenofRt = new Pen(Color.Green, Penwid)) 
                    {
                        g.DrawRectangle(PenofRt, Rt);
                    }  
                }
            }
            
            //g.Dispose();
        }

        private void picBxBottomTemp_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            if (bDrawTemp[4]) 
            {
                //int i,j;
                //行数480，列数640
                int rows = 480;
                int cols = 640;

                Rectangle rt = e.ClipRectangle;
                double dwf = (double)(rt.Size.Width * 1.0f / cols);
                double dhf = (double)(rt.Size.Height * 1.0f / rows);

                b_dwf = dwf;
                b_dhf = dhf;
                SizeF Sf = new SizeF((float)dwf, (float)dhf);
                PointF PTf;

                RectangleF tmprtf;
                Brush bsh;
                double x, y;
                UInt16 a;
                int b;

                for (int row = 0; row < rows; row++)  //  480行
                {
                    for (int col = 0; col < cols; col++)  //640列
                    {
                        x = dwf * col;
                        // y = dhf * (rows - row);
                        y = dhf * row;
                        PTf = new PointF((float)x, (float)y);
                        tmprtf = new RectangleF(PTf, Sf);

                        // a = data5[640*row + col];
                        a = data5[640 * (rows - 1 - row) + col];
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

                //如果最高温度在此图像中即变量为真，则执行画红框
                if (bDrawMaxTempScope[4])
                {
                    Point LTPntofRt;
                    Pen PenofRt;
                    Size SzofRt;
                    Rectangle Rt;
                    float Penwid = (float)4.1;
                    //LTPntofRt = new Point((int)(iMaxTempYbyPixel * dwf) - 10,(int)(iMaxTempXinPixel * dhf) - 10);
                    LTPntofRt = new Point((int)(iMaxTempXinPixel * dwf) - 10, (int)((rows - iMaxTempYbyPixel) * dhf) - 10);
                    SzofRt = new Size(20, 20);//定义画红框的大小，宽20高20
                    Rt = new Rectangle(LTPntofRt, SzofRt);
                    using (PenofRt = new Pen(Color.Green, Penwid)) 
                    {
                        g.DrawRectangle(PenofRt, Rt);
                    }
                }
            }

            
           // g.Dispose();
        }

        private void picBxCircumTemp1_MouseClick(object sender, MouseEventArgs e)
        {
            //鼠标点击该控件上绘制的图像出现坐标值            

            Point p1 = MousePosition;
            Point p2 = picBxCircumTemp1.PointToClient(p1);

            int X = (int)(Math.Floor(p2.X / g_dwf));
            int Y = (int)(Math.Floor(p2.Y / g_dhf));
            String str = "温度：" + Convert.ToString(data1[X * 640 + Y] + "℃；  当前坐标：(" + X + "," + Y + ")");
            txtBxTempPosiInform.Text = str;
            //this.toolTip1.Show(str, this.picBxCircumTemp1);
        }

        private void picBxCircumTemp2_MouseClick(object sender, MouseEventArgs e)
        {
            //鼠标点击该控件上绘制的图像出现坐标值            
            Point p1 = MousePosition;
            Point p2 = picBxCircumTemp2.PointToClient(p1);

            int X = (int)(Math.Floor(p2.X / g_dwf));
            int Y = (int)(Math.Floor(p2.Y / g_dhf));
            String str = "温度：" + Convert.ToString(data2[X * 640 + Y] + "℃；  当前坐标：(" + X + "," + Y + ")");
            txtBxTempPosiInform.Text = str;
            //this.toolTip1.Show(str, this.picBxCircumTemp2);
        }

        private void picBxCircumTemp3_MouseClick(object sender, MouseEventArgs e)
        {
            //鼠标点击该控件上绘制的图像出现坐标值            
            Point p1 = MousePosition;
            Point p2 = picBxCircumTemp3.PointToClient(p1);

            int X = (int)(Math.Floor(p2.X / g_dwf));
            int Y = (int)(Math.Floor(p2.Y / g_dhf));
            String str = "温度：" + Convert.ToString(data3[X * 640 + Y] + "℃；  当前坐标：(" + X + "," + Y + ")");
            txtBxTempPosiInform.Text = str;
           // this.toolTip1.Show(str, this.picBxCircumTemp3); 
        }

        private void picBxCircumTemp4_MouseClick(object sender, MouseEventArgs e)
        {
            //鼠标点击该控件上绘制的图像出现坐标值            
            Point p1 = MousePosition;
            Point p2 = picBxCircumTemp4.PointToClient(p1);

            int X = (int)(Math.Floor(p2.X / g_dwf));
            int Y = (int)(Math.Floor(p2.Y / g_dhf));
            String str = "温度：" + Convert.ToString(data4[X * 640 + Y] + "℃；  当前坐标：(" + X + "," + Y + ")");
            txtBxTempPosiInform.Text = str;
           // this.toolTip1.Show(str, this.picBxCircumTemp4);
        }

        private void picBxBottomTemp_MouseClick(object sender, MouseEventArgs e)
        {
            //鼠标点击该控件上绘制的图像出现坐标值            
            Point p1 = MousePosition;
            Point p2 = picBxBottomTemp.PointToClient(p1);

            int X = (int)(Math.Floor(p2.X / b_dwf));
            int Y = (int)(Math.Floor(p2.Y / b_dhf));
            String str = "温度：" + Convert.ToString(data5[(g_iCirPartCols - Y-1) * 640 + X] + "℃；当前坐标：(" + X + "," + Y + ")");
            txtBxTempPosiInform.Text = str;
           // this.toolTip1.Show(str, this.picBxBottomTemp);
        }

        private void butStop_Click(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            try
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                string strSQL1 = "update sysinteractinstr_copy1 set operation='10' where id='1'";
                MySqlCommand thisCommand = new MySqlCommand(strSQL1, myconnection);
                thisCommand.ExecuteNonQuery();
                myconnection.Close();
                
                txtBoxShowStatus.Text = "停止";
                m_iOperation = 10;
                XYCarSemiAutoEastPos.Enabled = false;
                XYCarSemiAutoWestPos.Enabled = false;
                XYCarSemiAutoEastWestPos.Enabled = false;
                
                comboBoxSelectMode.Text = "选择操作模式";
                comboBoxSelectAimPos.Text = "选择目标位";
                comboBoxSelectAimPos.Enabled = false;
                textBoxLadleNoEast.Enabled = false;
                textBoxLadleNoWest.Enabled = false;
                XYCarsToTarget.Enabled = false;
                XYCarsTakePhoto.Enabled = false;
                XYCarsToHome.Enabled = false;
                XCarsToTarget.Enabled = false;
                YCarToTarget.Enabled = false;
                MeasThick.Enabled = false;
                YCarToHome.Enabled = false;
                XCarToHome.Enabled = false;
                txtBoxShowStatus.Text = "请先选择操作模式！";
                ThickRecordSave($"{DateTime.Now} 前端按下停止键！");
                //MessageBox.Show("启动大车！");
            }
            catch (Exception ee)
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
        }

        
        private void btnTempeAnal_Click(object sender, EventArgs e)
        {
            TempeAnal fmTempeAnal = new TempeAnal();
            fmTempeAnal.ShowDialog();
        }

        private void dtaGrdViewEmtyMeased_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ElID = Convert.ToInt32(dtaGrdViewEmtyMeased.CurrentRow.Cells[0].Value.ToString());
            ThickMeasTm = Convert.ToDateTime(dtaGrdViewEmtyMeased.CurrentRow.Cells[4].Value);
            ThickStatusCode= Convert.ToInt32(dtaGrdViewEmtyMeased.CurrentRow.Cells[7].Value.ToString());
            ShowthickImage();
        }

        private void buttonThickAnalysys_Click(object sender, EventArgs e)
        {
            ThickAnal fmThickAnal = new ThickAnal();
            fmThickAnal.ShowDialog();
        }

        private void Heavyldl_Meased_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            TbID = Convert.ToInt32(Heavyldl_Meased.CurrentRow.Cells["id"].Value.ToString());
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            try
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection1.Image = Properties.Resources.green2;
                }
                string SQLMaxTempImager = "select MaxTempImagerNo,MaxTempXbyPixel,MaxTempYbyPixel  from heavyldl_meased where id=" + TbID + "";
                MySqlDataAdapter objDataAdpterMaxTempImager = new MySqlDataAdapter();
                DataSet dsMaxTempImager = new DataSet();
                using (objDataAdpterMaxTempImager.SelectCommand = new MySqlCommand(SQLMaxTempImager, myconnection))
                {
                    objDataAdpterMaxTempImager.Fill(dsMaxTempImager, "heavyldl_meased");
                }
                iMaxTempImagerNo = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][0]);
                iMaxTempXinPixel = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][1]);
                iMaxTempYbyPixel = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][2]);
                bDrawMaxTempScope = new bool[5] { false, false, false, false, false };
                if (iMaxTempImagerNo != 0)
                {
                    bDrawMaxTempScope[iMaxTempImagerNo - 1] = true;
                }
                myconnection.Close();
            }
            catch (Exception ee)
            {
                if  (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection1.Image = Properties.Resources.redshine;
                }
            }

            ShowtempPic();
        }

        private void XYCarSemiAutoEastPos_Click(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
           
            try
            {
                byte iFeedBack = 0;
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                string SQL = "SELECT status FROM sysinteractinstr_copy1";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                DataSet ds = new DataSet();
                using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                {
                    objDataAdpter.Fill(ds, "sysinteractinstr_copy1");
                }
                myconnection.Close();
                byte[] ArryStatus = (byte[])(ds.Tables[0].Rows[0][0]);
                int num=ArryStatus.Length;
                iFeedBack = ArryStatus[num-1];
                //2. 如果status为0，则进行下面操作
                if (iFeedBack == StandardAnswer)
                {
                    if (ModifySysInsStatus())
                    {
                        g_itarget = 1;
                        g_operation = 71;
                        g_bEastStatus = false;
                        g_bEastWestStatus = false;
                        g_bWestStatus = false;
                        drawStatusFlag = 0;
                        butStop.Enabled = false;
                        comboBoxSelectMode.Enabled = false;
                        if (myconnection.State == ConnectionState.Closed)
                        {
                            myconnection.Open();//打开数据库     
                        }
                        string strSQL = "update sysinteractinstr_copy1 set operation=71,target=" + g_itarget + " where id=1;";
                        using (MySqlCommand thisCommand = new MySqlCommand(strSQL, myconnection))
                        {
                            thisCommand.ExecuteNonQuery();
                        }
                        string ladlenoEast = textBoxLadleNoEast.Text.ToString();
                        
                        string strSQLEast = "select id,MeasTm,StatusCode from emptyldl_meased where LadleNo='" + ladlenoEast + "' and `DELETE`=0 order by id desc LIMIT 0,1;";
                        MySqlDataAdapter objDataAdpterEast = new MySqlDataAdapter();
                        DataSet dsEast = new DataSet();
                        using (objDataAdpterEast.SelectCommand = new MySqlCommand(strSQLEast, myconnection))
                        {
                            objDataAdpterEast.Fill(dsEast, "emptyldl_meased");
                        }
                        myconnection.Close();
                        if (dsEast.Tables[0].Rows.Count > 0)
                        {
                            ElID = Convert.ToInt32(dsEast.Tables[0].Rows[0][0]);
                            ThickMeasTmEast = Convert.ToDateTime(dsEast.Tables[0].Rows[0][1]);
                            ThickStatusCodeEast = Convert.ToInt32(dsEast.Tables[0].Rows[0][2]);
                            g_bEastStatus = ((Convert.ToInt32((DateTime.Now - ThickMeasTmEast).TotalDays) <= 3) && (ThickStatusCodeEast != 0)) ? true : false;
                        }
                        
                        txtBoxShowStatus.Text = "天车准备去往东包位";
                        m_iOperation = 71;
                        XYCarSemiAutoEastPos.Enabled = false;

                        if (g_iOperateMode == 0)
                        {
                            bExitFlag = false;
                            if (!bExitFlag)
                            {
                                AnalysisStatus(); 
                            }

                        }
                    }
                    
                }
                else
                {
                    MessageBox.Show("当前正在执行其他操作！请等待......");
                }
                
            }
            catch (Exception ee)
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
        }

        private async void AnalysisStatus()
        {
            await AnalysisStart();
            butStop.Enabled = true;
            int[] Answer = await AnalysisSecond();
            int target = Answer[0];
            byte result = (byte)Answer[1];
            switch (m_iOperation)
            {
                case 10:
                    {
                        if (txtBoxShowStatus.IsHandleCreated)
                        {
                            txtBoxShowStatus.BeginInvoke(new Action(() =>
                            {
                                txtBoxShowStatus.Text = StatusExplain(target, result);
                            }));
                        }

                    }
                    break;
                case 71:
                    {
                        /*if (XYCarSemiAutoEastPos.IsHandleCreated)
                        {
                            Action action = () =>
                            {
                                XYCarSemiAutoEastPos.Enabled = true;
                            };
                            Invoke(action);
                        }*/
                        if (txtBoxShowStatus.IsHandleCreated)
                        {
                            txtBoxShowStatus.BeginInvoke(new Action(() =>
                            {
                                txtBoxShowStatus.Text = StatusExplain(target, result);
                            }));
                        }

                    }
                    break;
                case 72:
                    {
                        /*Action action = () =>
                        {
                            XYCarSemiAutoWestPos.Enabled = true;
                        };
                        Invoke(action);*/
                        if (txtBoxShowStatus.IsHandleCreated)
                        {
                            txtBoxShowStatus.BeginInvoke(new Action(() =>
                            {
                                txtBoxShowStatus.Text = StatusExplain(target, result);
                            }));
                        }
                    }
                    break;
                case 73:
                    {
                        /*Action action = () =>
                        {
                            XYCarSemiAutoEastWestPos.Enabled = true;
                        };
                        Invoke(action);*/
                        if (txtBoxShowStatus.IsHandleCreated)
                        {
                            txtBoxShowStatus.BeginInvoke(new Action(() =>
                            {
                                txtBoxShowStatus.Text = StatusExplain(target, result);
                            }));
                        }
                    }
                    break;
                case 7:
                    {
                        if (result == 27)
                        {
                            if (XYCarsTakePhoto.IsHandleCreated)
                            {
                                Action action = () =>
                                {
                                    XYCarsTakePhoto.Enabled = true;
                                };
                                Invoke(action);
                            }
                        }
                        if (result == 29)
                        {
                            if (XYCarsToHome.IsHandleCreated)
                            {
                                Action action = () =>
                                {
                                    XYCarsToHome.Enabled = true;
                                };
                                Invoke(action);
                            }
                        }
                        if (txtBoxShowStatus.IsHandleCreated)
                        {
                            txtBoxShowStatus.BeginInvoke(new Action(() =>
                            {
                                txtBoxShowStatus.Text = StatusExplain(target, result);
                            }));
                        }


                    }
                    break;
                case 8:
                    {
                        if (XYCarsTakePhoto.IsHandleCreated)
                        {
                            Action action = () =>
                            {
                                XYCarsTakePhoto.Enabled = true;
                            };
                            Invoke(action);
                        }
                        if (txtBoxShowStatus.IsHandleCreated)
                        {
                            txtBoxShowStatus.BeginInvoke(new Action(() =>
                            {
                                txtBoxShowStatus.Text = StatusExplain(target, result);
                            }));
                        }
                    }
                    break;
                case 9:
                    {

                        if (XYCarsToHome.IsHandleCreated)
                        {
                            Action action = () =>
                            {
                                XYCarsToHome.Enabled = true;
                            };
                            Invoke(action);
                        }
                        if (txtBoxShowStatus.IsHandleCreated)
                        {
                            txtBoxShowStatus.BeginInvoke(new Action(() =>
                            {
                                txtBoxShowStatus.Text = StatusExplain(target, result);
                            }));
                        }
                    }
                    break;
                case 1:
                    {
                        if (YCarToTarget.IsHandleCreated)
                        {
                            Action action = () =>
                            {
                                YCarToTarget.Enabled = true;
                            };
                            Invoke(action);
                        }
                        if (txtBoxShowStatus.IsHandleCreated)
                        {
                            txtBoxShowStatus.BeginInvoke(new Action(() =>
                            {
                                txtBoxShowStatus.Text = StatusExplain(target, result);
                            }));
                        }

                    }
                    break;
                case 2:
                    {
                        if (MeasThick.IsHandleCreated)
                        {
                            Action action = () =>
                            {
                                MeasThick.Enabled = true;
                            };
                            Invoke(action);
                        }
                        if (txtBoxShowStatus.IsHandleCreated)
                        {
                            txtBoxShowStatus.BeginInvoke(new Action(() =>
                            {
                                txtBoxShowStatus.Text = StatusExplain(target, result);
                            }));
                        }
                    }
                    break;
                case 3:
                    {
                        if (XCarToHome.IsHandleCreated)
                        {
                            Action action = () =>
                            {
                                XCarToHome.Enabled = true;
                            };
                            Invoke(action);
                        }
                        if (txtBoxShowStatus.IsHandleCreated)
                        {
                            txtBoxShowStatus.BeginInvoke(new Action(() =>
                            {
                                txtBoxShowStatus.Text = StatusExplain(target, result);
                            }));
                        }
                    }
                    break;
                case 5:
                    {
                        if (YCarToHome.IsHandleCreated)
                        {
                            Action action = () =>
                            {
                                YCarToHome.Enabled = true;
                            };
                            Invoke(action);
                        }
                        if (txtBoxShowStatus.IsHandleCreated)
                        {
                            txtBoxShowStatus.BeginInvoke(new Action(() =>
                            {
                                txtBoxShowStatus.Text = StatusExplain(target, result);
                            }));
                        }
                    }
                    break;
                case 6:
                    {
                        if (YCarToHome.IsHandleCreated)
                        {
                            Action action = () =>
                            {
                                YCarToHome.Enabled = true;
                            };
                            Invoke(action);
                        }
                        if (txtBoxShowStatus.IsHandleCreated)
                        {
                            txtBoxShowStatus.BeginInvoke(new Action(() =>
                            {
                                txtBoxShowStatus.Text = StatusExplain(target, result);
                            }));
                        }
                    }
                    break;
            }
            comboBoxSelectMode.Enabled = true;
            ThickRecordSave($"{DateTime.Now}———此过程结束—-—");
            g_bstatus = false;
            g_bdrawStatus = false;
            drawStatusFlag = 0;
        }

        private Task AnalysisStart()
        {
            //根据不同按钮设置不同超时
            bExitFlag = false;
            // Thread.Sleep(1000);
            return Task.Run(() =>
            {
                MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
                try
                {
                    myconnection.Open();//打开数据库 
                    if (myconnection.State == ConnectionState.Open)
                    {
                        MySQLConnection.Image = Properties.Resources.green2;
                    }
                    while (!bExitFlag)
                    {
                        //Thread.Sleep(1000);
                        int target = 0;
                        long countStatus = 0;
                        int inlen = 200;
                        byte[] statusArry = new byte[inlen];
                        string SQL = "SELECT target,status FROM sysinteractinstr_copy1";
                        MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                        DataSet ds = new DataSet();
                        using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                        {
                            objDataAdpter.Fill(ds, "sysinteractinstr_copy1");
                        }
                        target = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                        statusArry = (byte[])(ds.Tables[0].Rows[0][1]);
                        countStatus = statusArry.Length;
                        if (statusArry[0] == g_operation)
                        {
                            g_bstatus = true;
                            if (g_bEastStatus || g_bWestStatus || g_bEastWestStatus)
                            {
                                g_bdrawStatus = true;
                            }
                            bExitFlag = true;
                        }
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show("解析状态码过程1出问题！" + ee.ToString());
                }
                finally
                {
                    myconnection.Close();
                }
            });


        }
        private Task<int[]> AnalysisSecond()
        {
            //根据不同按钮设置不同超时
            StatusIndex = 0;
            bExitFlag = false;
            int[] resultReturn = { 0, 0 };
            //Thread.Sleep(1000);
            return Task.Run(() =>
            {
                MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
                try
                {
                    myconnection.Open();//打开数据库 
                    if (myconnection.State == ConnectionState.Open)
                    {
                        MySQLConnection.Image = Properties.Resources.green2;
                    }
                    while (!bExitFlag)
                    {
                        //Thread.Sleep(1000);
                        byte iFeedBackStatus = 0;
                        int target = 0;
                        long countStatus = 0;
                        int inlen = 200;
                        byte[] statusArry = new byte[inlen];
                        string SQL = "SELECT target,status FROM sysinteractinstr_copy1";
                        MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                        DataSet ds = new DataSet();
                        using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                        {
                            objDataAdpter.Fill(ds, "sysinteractinstr_copy1");
                        }
                        target = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                        statusArry = (byte[])(ds.Tables[0].Rows[0][1]);
                        countStatus = statusArry.Length;
                        if ((statusArry[0] == g_operation) && (statusArry[countStatus - 1] != StandardAnswer))
                        {
                            for (int i = StatusIndex; i < countStatus; i++)
                            {
                                iFeedBackStatus = statusArry[i];
                                ThickRecordSave(StatusExplain(target, iFeedBackStatus));
                                StatusIndex++;
                            }
                        }
                        if (g_bstatus && (statusArry[countStatus - 1] == StandardAnswer))
                        {
                            byte result = statusArry[countStatus - 2];
                            for (int i = StatusIndex; i < countStatus - 1; i++)
                            {
                                iFeedBackStatus = statusArry[i];
                                ThickRecordSave(StatusExplain(target, iFeedBackStatus));
                                StatusIndex++;
                            }
                            resultReturn[0] = target;
                            resultReturn[1] = result;
                            bExitFlag = true;

                        }
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show("解析状态码过程2出问题！" + ee.ToString());
                }
                finally
                {
                    myconnection.Close();
                }
                return resultReturn;
            });
        }

        private bool ModifySysInsStatus()
        {
            int num = 0;
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            //1. 查询交互表反馈字段status，并存储到iFeedBack变量。
            try
            {
                
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                string SQL = "update sysinteractinstr_copy1 set status=b'0' where id=1;";
                using (MySqlCommand thisCommand = new MySqlCommand(SQL, myconnection))
                {
                    num = thisCommand.ExecuteNonQuery();
                }
                myconnection.Close();

            }
            catch (Exception ee)
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
            if (num == 1)
            { return true; }
            else { return false; }

        }
        private void XYCarSemiAutoWestPos_Click(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            //1. 查询交互表反馈字段status，并存储到iFeedBack变量。
            try
            {
                byte iFeedBack = 0;
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                string SQL = "SELECT status FROM sysinteractinstr_copy1";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                DataSet ds = new DataSet();
                using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                {
                    objDataAdpter.Fill(ds, "sysinteractinstr_copy1");
                }
                myconnection.Close();
                byte[] ArryStatus = (byte[])(ds.Tables[0].Rows[0][0]);
                int num = ArryStatus.Length;
                iFeedBack = ArryStatus[num - 1];

                //2. 如果status为0，则进行下面操作
                if (iFeedBack == StandardAnswer)
                {
                    if (ModifySysInsStatus())
                    {
                        g_itarget = 2;
                        g_operation = 72;
                        g_bEastStatus = false;
                        g_bEastWestStatus = false;
                        g_bWestStatus = false;
                        drawStatusFlag = 0;
                        butStop.Enabled = false;
                        comboBoxSelectMode.Enabled = false;
                        if (myconnection.State == ConnectionState.Closed)
                        {
                            myconnection.Open();//打开数据库     
                        }
                        string strSQL = "update sysinteractinstr_copy1 set operation=72,target=" + g_itarget + " where id=1;";
                        using (MySqlCommand thisCommand = new MySqlCommand(strSQL, myconnection))
                        {
                            thisCommand.ExecuteNonQuery();
                        }
                        string ladlenoWest = textBoxLadleNoWest.Text.ToString();

                        string strSQLWest = "select id,MeasTm,StatusCode from emptyldl_meased where LadleNo='" + ladlenoWest + "' and `DELETE`=0 order by id desc LIMIT 0,1;";
                        MySqlDataAdapter objDataAdpterWest = new MySqlDataAdapter();
                        DataSet dsWest = new DataSet();
                        using (objDataAdpterWest.SelectCommand = new MySqlCommand(strSQLWest, myconnection))
                        {
                            objDataAdpterWest.Fill(dsWest, "emptyldl_meased");
                        }
                        myconnection.Close();
                        if (dsWest.Tables[0].Rows.Count > 0)
                        {
                            ElID = Convert.ToInt32(dsWest.Tables[0].Rows[0][0]);
                            ThickMeasTmWest = Convert.ToDateTime(dsWest.Tables[0].Rows[0][1]);
                            ThickStatusCodeWest = Convert.ToInt32(dsWest.Tables[0].Rows[0][2]);
                            g_bWestStatus = ((Convert.ToInt32((DateTime.Now - ThickMeasTmWest).TotalDays) <= 3) && (ThickStatusCodeWest != 0)) ? true : false;
                        }
                        txtBoxShowStatus.Text = "天车准备去往西包位";

                        m_iOperation = 72;
                        XYCarSemiAutoWestPos.Enabled = false;

                        if (g_iOperateMode == 0)
                        {
                            bExitFlag = false;
                            if (!bExitFlag)
                            {
                                AnalysisStatus();
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("当前正在执行其他操作！请等待......");
                }
                
            }
            catch (Exception ee)
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
        }

        private void XYCarSemiAutoEastWestPos_Click(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            //1. 查询交互表反馈字段status，并存储到iFeedBack变量。
            try
            {
                byte iFeedBack = 0;
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                string SQL = "SELECT status FROM sysinteractinstr_copy1";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                DataSet ds = new DataSet();
                using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                {
                    objDataAdpter.Fill(ds, "sysinteractinstr_copy1");
                }
                myconnection.Close();
                byte[] ArryStatus = (byte[])(ds.Tables[0].Rows[0][0]);
                int num = ArryStatus.Length;
                iFeedBack = ArryStatus[num - 1];

                //2. 如果status为0，则进行下面操作
                if (iFeedBack == StandardAnswer)
                {
                    if (ModifySysInsStatus())
                    {
                        g_itarget = 1;
                        g_operation = 73;
                        g_bEastStatus = false;
                        g_bEastWestStatus = false;
                        g_bWestStatus = false;
                        butStop.Enabled = false;
                        comboBoxSelectMode.Enabled = false;
                        if (myconnection.State == ConnectionState.Closed)
                        {
                            myconnection.Open();//打开数据库     
                        }
                        
                        string strSQL = "update sysinteractinstr_copy1 set operation=73,target=" + g_itarget + " where id=1;";
                        using (MySqlCommand thisCommand = new MySqlCommand(strSQL, myconnection))
                        {
                            thisCommand.ExecuteNonQuery();
                        }

                        string ladlenoEast = textBoxLadleNoEast.Text.ToString();
                        string strSQLEast = "select id,MeasTm,StatusCode from emptyldl_meased where LadleNo='" + ladlenoEast + "' and `DELETE`=0 order by id desc LIMIT 0,1;";
                        MySqlDataAdapter objDataAdpterEast = new MySqlDataAdapter();
                        DataSet dsEast = new DataSet();
                        using (objDataAdpterEast.SelectCommand = new MySqlCommand(strSQLEast, myconnection))
                        {
                            objDataAdpterEast.Fill(dsEast, "emptyldl_meased");
                        }
                        if (dsEast.Tables[0].Rows.Count > 0)
                        {
                            ElID = Convert.ToInt32(dsEast.Tables[0].Rows[0][0]);
                            ThickMeasTmEast = Convert.ToDateTime(dsEast.Tables[0].Rows[0][1]);
                            ThickStatusCodeEast = Convert.ToInt32(dsEast.Tables[0].Rows[0][2]);
                            g_bEastStatus = ((Convert.ToInt32((DateTime.Now - ThickMeasTmEast).TotalDays) <= 3) && (ThickStatusCodeEast != 0)) ? true : false;
                        }
                        string ladlenoWest = textBoxLadleNoWest.Text.ToString();
                        string strSQLWest = "select id,MeasTm,StatusCode from emptyldl_meased where LadleNo='" + ladlenoWest + "' and `DELETE`=0 order by id desc LIMIT 0,1;";
                        MySqlDataAdapter objDataAdpterWest = new MySqlDataAdapter();
                        DataSet dsWest = new DataSet();
                        using (objDataAdpterWest.SelectCommand = new MySqlCommand(strSQLWest, myconnection))
                        {
                            objDataAdpterWest.Fill(dsWest, "emptyldl_meased");
                        }
                        myconnection.Close();
                        if (dsWest.Tables[0].Rows.Count > 0)
                        {
                            ElID = Convert.ToInt32(dsWest.Tables[0].Rows[0][0]);
                            ThickMeasTmWest = Convert.ToDateTime(dsWest.Tables[0].Rows[0][1]);
                            ThickStatusCodeWest = Convert.ToInt32(dsWest.Tables[0].Rows[0][2]);
                            g_bWestStatus = ((Convert.ToInt32((DateTime.Now - ThickMeasTmWest).TotalDays) <= 3) && (ThickStatusCodeWest != 0)) ? true : false;

                        }

                        if (g_bEastStatus && g_bWestStatus)
                        {
                            g_bEastWestStatus = true;
                            g_bEastStatus = false;
                            g_bWestStatus = false;
                        }
                        txtBoxShowStatus.Text = "天车去往东西包位";

                        m_iOperation = 73;
                        XYCarSemiAutoEastWestPos.Enabled = false;

                        if (g_iOperateMode == 0)
                        {
                            bExitFlag = false;
                            if (!bExitFlag)
                            {
                                AnalysisStatus();
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("当前正在执行其他操作！请等待......");
                }
                
            }
            catch (Exception ee)
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
        }

        private void Heavyldl_Meased_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            MySqlConnection myconnection;
            myconnection = new MySqlConnection(SQLServerUser);
            int iAge;
            try
            {
                if (e.RowIndex != -1)
                {
                    string strrow = Heavyldl_Meased.Rows[e.RowIndex].Cells[0].Value.ToString();//获取焦点触发行的第一个值
                    string value = Heavyldl_Meased.CurrentCell.Value.ToString();//获取当前点击的活动单元格的值
                    if (myconnection.State == ConnectionState.Closed)
                    {
                        myconnection.Open();//打开数据库     
                    }
                    if (myconnection.State == ConnectionState.Open)
                    {
                        MySQLConnection1.Image = Properties.Resources.green2;
                    }

                    string SQL = "SELECT  Ladle_Age from ladle_age where Ladle_ID ='" + value + "'";
                    using (MySqlCommand thisCommand = new MySqlCommand(SQL, myconnection))
                    {
                        iAge = Convert.ToInt32(thisCommand.ExecuteScalar());
                    }

                    string strSQL = "update heavyldl_meased set LadleNo ='" + value + "',LadleAge =" + iAge + " where id = " + strrow + ";";
                    using (MySqlCommand thisCommand1 = new MySqlCommand(strSQL, myconnection))
                    {
                        thisCommand1.ExecuteNonQuery();
                    }
                    string SQL2 = "select id, LadleNo,LadleServDuty,LadleAge, MeasTm, MaxTemp,MaxTempPos  from heavyldl_meased where `Delete`=0 order by id desc limit 0,50";
                    MySqlDataAdapter objDataAdpter2 = new MySqlDataAdapter();
                    DataSet ds2 = new DataSet();
                    using (objDataAdpter2.SelectCommand = new MySqlCommand(SQL2, myconnection))
                    {
                        objDataAdpter2.Fill(ds2, "heavyldl_meased");
                    }
                    Heavyldl_Meased.AutoGenerateColumns = false;
                    Heavyldl_Meased.DataSource = ds2.Tables[0];
                    myconnection.Close();
                }
                
            }
            catch (Exception ee)
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection1.Image = Properties.Resources.redshine;
                }
                
            }
        }

        private void Heavyldl_Meased_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            //是否可以进行编辑的条件检查 
            if (dgv.Columns[e.ColumnIndex].Name != "ladleno")
            {
                // 取消编辑 
                e.Cancel = true;
            }
        }
        private void picBoxBottomPic_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ThickBotPic fmThickBotPic = new ThickBotPic(image_rote);

           // fmThickBotPic.ThickBotPic_ChangePicture += new ThickBotPic.ChangePictureHandler(Form1_Change_Picture);//使用委托，关闭子窗体时，图片回传
            fmThickBotPic.Show();
            
        }
        private void Form1_Change_Picture(Image pic)
        {
            this.picBoxBottomPic.Image = pic;

        }
        private int ReadCurrLadleServDuty(string no)
        {
            MySqlConnection connMysql = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");

            try
            {
                int  lalSerDy = -1;
                
                if (connMysql.State == ConnectionState.Closed)
                {
                    connMysql.Open();
                }

                string SQL = "select LadleServDuty from ladleservduty where LadleNo='" + no + "';";
                MySqlCommand thisCommand1 = new MySqlCommand(SQL, connMysql);
                lalSerDy = Convert.ToInt32(thisCommand1.ExecuteScalar());
              
                connMysql.Close();
                return lalSerDy;
            }
            catch (Exception ee)
            {
                MessageBox.Show("读取已测重包包龄失败" + ee.ToString());
                return -1;
            }
        }
        private bool IsEmptyLdlZero(string LadleNo)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");

            bool isNull = false;
            int ladleSeDuty = -1;
            try
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                //求包号最大的包役
                ladleSeDuty = ReadCurrLadleServDuty(LadleNo);
                string SQL = "SELECT CircumThick FROM emptyldl_meased WHERE LadleNo='" + LadleNo + "' AND LadleServDuty="+ladleSeDuty+ " AND LadleAge=0 AND `Delete`=0 order by id desc limit 0,1;";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                DataSet ds = new DataSet();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection);
                objDataAdpter.Fill(ds, "emptyldl_meased");
                if (ds.Tables[0].Rows.Count > 0)
                {    //行   
                    if (ds.Tables[0].Rows[0][0] == DBNull.Value)
                    {
                        isNull = true;
                        //break;                                              //如果有一个字段不为空，表示存在数据     
                    }
                }
                if(ds.Tables[0].Rows.Count == 0)
                {
                    isNull = true;
                }
                myconnection.Close();
            }
            catch (Exception ee)
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
            return isNull;
        }
        private void butSurePara_Click(object sender, EventArgs e)
        {
            
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");

            string ladlenoEast = textBoxLadleNoEast.Text.ToString();
            string ladlenoWest = textBoxLadleNoWest.Text.ToString();
            bool b_ladlenoEast = false, b_ladlenoWest = false;
            int iAge=-1;
            if (ladlenoEast != "") {
                b_ladlenoEast = String.Equals(ladlenoEast.Substring(0, 2), "T0") && (ladlenoEast.Length == 4);
            }
            if (ladlenoWest != "") {
                b_ladlenoWest = String.Equals(ladlenoWest.Substring(0, 2), "T0") && (ladlenoWest.Length == 4);
            }
            if (g_iOperateMode == 0) 
            {
                
                int ladleageEast = -1;
               
                if (ladlenoEast != "")
                {
                    if (b_ladlenoEast) 
                    {
                        if (myconnection.State == ConnectionState.Closed)
                        {
                            myconnection.Open();//打开数据库     
                        }
                        if (myconnection.State == ConnectionState.Open)
                        {
                            MySQLConnection.Image = Properties.Resources.green2;
                        }
                        string SQL = "SELECT  Ladle_Age from ladle_age where Ladle_ID ='" + ladlenoEast + "'";
                        using (MySqlCommand thisCommand = new MySqlCommand(SQL, myconnection))
                        {
                            if (thisCommand.ExecuteScalar() != DBNull.Value)
                            {
                                iAge = Convert.ToInt32(thisCommand.ExecuteScalar());
                            }
                            else
                            {
                                iAge = -2;
                            }
                        }
                        myconnection.Close();
                        if (b_ladlenoEast && (iAge != -2))
                        {
                            if (IsEmptyLdlZero(ladlenoEast))
                            {
                                if ((MessageBox.Show("此次测量：东包位：包号为" + ladlenoEast + "的空包，实际包龄为" + iAge + ",测厚包龄是否强制为0？", "提示：已测空包表中不存在" + ladlenoEast + "包0包龄记录。", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes) && (iAge != -2))
                                {
                                    ladleageEast = 0;
                                }
                                else
                                {
                                    b_ladlenoEast = false;
                                }
                            }
                        }
                        else if (MessageBox.Show("东包位铁包非周转包！", "提示", MessageBoxButtons.OK) == DialogResult.OK)
                        {
                            b_ladlenoEast = false;
                            textBoxLadleNoEast.Text = "";
                            if (!b_ladlenoWest)
                            {
                                textBoxLadleNoWest.Text = "";
                                return;
                            }
                            
                        }
                    }
                    else 
                    {
                            textBoxLadleNoEast.Text = "";
                        
                    }
                }
                int ladleageWest = -1;
                if (ladlenoWest != "") 
                {
                    if (b_ladlenoWest)
                    {
                        if (myconnection.State == ConnectionState.Closed)
                        {
                            myconnection.Open();//打开数据库     
                        }
                        if (myconnection.State == ConnectionState.Open)
                        {
                            MySQLConnection.Image = Properties.Resources.green2;
                        }
                        string SQL = "SELECT  Ladle_Age from ladle_age where Ladle_ID ='" + ladlenoWest + "'";
                        using (MySqlCommand thisCommand = new MySqlCommand(SQL, myconnection))
                        {
                            if (thisCommand.ExecuteScalar() != DBNull.Value)
                            {
                                iAge = Convert.ToInt32(thisCommand.ExecuteScalar());
                            }
                            else
                            {
                                iAge = -2;
                            }
                        }
                        myconnection.Close();
                        if (b_ladlenoWest && (iAge != -2))
                        {
                            if (IsEmptyLdlZero(ladlenoWest))
                            {
                                if ((MessageBox.Show("此次测量：西包位：包号为" + ladlenoWest + "的空包，实际包龄为" + iAge + ",测厚包龄是否强制为0？", "提示：已测空包表中不存在" + ladlenoWest + "包0包龄记录。", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes) && (iAge != -2))
                                {
                                    ladleageWest = 0;
                                }
                                else
                                {
                                    b_ladlenoWest = false;
                                }
                            }
                        }
                        else if (MessageBox.Show("西包位铁包非周转包！", "提示", MessageBoxButtons.OK) == DialogResult.OK)
                        {
                            b_ladlenoWest = false;
                            textBoxLadleNoWest.Text = "";
                            if (!b_ladlenoEast)
                            {
                                textBoxLadleNoEast.Text = "";
                                return;
                            }
                            
                            
                        }

                    }
                    else 
                    {
                        
                            textBoxLadleNoWest.Text = "";
                        
                    }
                    
                }
                try 
                {
                    if (g_iOperateMode == 0)
                    {
                        if (b_ladlenoEast && !b_ladlenoWest)
                        {
                            XYCarSemiAutoEastPos.Enabled = true;
                            XYCarSemiAutoWestPos.Enabled = false;
                            XYCarSemiAutoEastWestPos.Enabled = false;
                            XCarsToTarget.Enabled = false;
                            YCarToTarget.Enabled = false;
                            MeasThick.Enabled = false;
                            XCarToHome.Enabled = false;
                            YCarToHome.Enabled = false;
                            XYCarsToTarget.Enabled = false;
                            XYCarsTakePhoto.Enabled = false;
                            XYCarsToHome.Enabled = false;
                            textBoxLadleNoEast.Enabled = false;
                            textBoxLadleNoWest.Enabled = false;
                        }
                        if (!b_ladlenoEast && b_ladlenoWest) 
                        {
                            XYCarSemiAutoEastPos.Enabled = false;
                            XYCarSemiAutoWestPos.Enabled = true;
                            XYCarSemiAutoEastWestPos.Enabled = false;
                            XCarsToTarget.Enabled = false;
                            YCarToTarget.Enabled = false;
                            MeasThick.Enabled = false;
                            XCarToHome.Enabled = false;
                            YCarToHome.Enabled = false;
                            XYCarsToTarget.Enabled = false;
                            XYCarsTakePhoto.Enabled = false;
                            XYCarsToHome.Enabled = false;
                            textBoxLadleNoEast.Enabled = false;
                            textBoxLadleNoWest.Enabled = false;
                        }
                        if (b_ladlenoEast && b_ladlenoWest) 
                        {
                            XYCarSemiAutoEastPos.Enabled = false;
                            XYCarSemiAutoWestPos.Enabled = false;
                            XYCarSemiAutoEastWestPos.Enabled = true;
                            XCarsToTarget.Enabled = false;
                            YCarToTarget.Enabled = false;
                            MeasThick.Enabled = false;
                            XCarToHome.Enabled = false;
                            YCarToHome.Enabled = false;
                            XYCarsToTarget.Enabled = false;
                            XYCarsTakePhoto.Enabled = false;
                            XYCarsToHome.Enabled = false;
                            textBoxLadleNoEast.Enabled = true;
                            textBoxLadleNoWest.Enabled = true;
                        }
                        if (!b_ladlenoEast && !b_ladlenoWest)
                        {
                            XYCarSemiAutoEastPos.Enabled = false;
                            XYCarSemiAutoWestPos.Enabled = false;
                            XYCarSemiAutoEastWestPos.Enabled = false;
                            XCarsToTarget.Enabled = false;
                            YCarToTarget.Enabled = false;
                            MeasThick.Enabled = false;
                            XCarToHome.Enabled = false;
                            YCarToHome.Enabled = false;
                            XYCarsToTarget.Enabled = false;
                            XYCarsTakePhoto.Enabled = false;
                            XYCarsToHome.Enabled = false;
                            textBoxLadleNoEast.Enabled = false;
                            textBoxLadleNoWest.Enabled = false;
                        }
                        
                    }

                    if (myconnection.State == ConnectionState.Closed)
                    {
                        myconnection.Open();//打开数据库     
                    }
                    if (myconnection.State == ConnectionState.Open)
                    {
                        MySQLConnection.Image = Properties.Resources.green2;
                    }
                    ladlenoEast = textBoxLadleNoEast.Text.ToString();
                    ladlenoWest = textBoxLadleNoWest.Text.ToString();
                    string SQL1 = "update sysinteractinstr_copy1 set mode=" + g_iOperateMode + ",target=0,ladlenoEast='" + ladlenoEast + "',ladleageEast='" + ladleageEast + "',ladlenoWest='" + ladlenoWest + "',ladleageWest='" + ladleageWest+"' where id=1;";
                    using (MySqlCommand thisCommand = new MySqlCommand(SQL1, myconnection))
                    {
                        thisCommand.ExecuteNonQuery();
                    } 
                    myconnection.Close();

                }
                catch (Exception ee)
                {
                    if (myconnection.State == ConnectionState.Closed)
                    {
                        MySQLConnection.Image = Properties.Resources.redshine;
                    }
                }
            }
            if ((g_iOperateMode == 1) || (g_iOperateMode == 2)) 
            {
                if ((g_itarget != 1) && (g_itarget != 2))
                {
                    MessageBox.Show("请选择目标包位！");
                }
                else 
                {
                    if (!b_ladlenoEast && !b_ladlenoWest) 
                    {
                        if (g_iOperateMode == 1)
                        {
                            XYCarSemiAutoEastPos.Enabled = false;
                            XYCarSemiAutoWestPos.Enabled = false;
                            XYCarSemiAutoEastWestPos.Enabled = false;
                            XCarsToTarget.Enabled = false;
                            YCarToTarget.Enabled = false;
                            MeasThick.Enabled = false;
                            XCarToHome.Enabled = false;
                            YCarToHome.Enabled = false;
                            XYCarsToTarget.Enabled = false;
                            XYCarsTakePhoto.Enabled = false;
                            XYCarsToHome.Enabled = true;
                            textBoxLadleNoEast.Enabled = false;
                            textBoxLadleNoEast.Text = "";
                            textBoxLadleNoWest.Enabled = false;
                            textBoxLadleNoWest.Text = "";
                            comboBoxSelectAimPos.Enabled = false;
                            butSurePara.Enabled = false;
                        }
                        //手动
                        if (g_iOperateMode == 2)
                        {
                            XYCarSemiAutoEastPos.Enabled = false;
                            XYCarSemiAutoWestPos.Enabled = false;
                            XYCarSemiAutoEastWestPos.Enabled = false;
                            XCarsToTarget.Enabled = false;
                            YCarToTarget.Enabled = false;
                            MeasThick.Enabled = false;
                            XCarToHome.Enabled = true;
                            YCarToHome.Enabled = true;
                            XYCarsToTarget.Enabled = false;
                            XYCarsTakePhoto.Enabled = false;
                            XYCarsToHome.Enabled = false;
                            textBoxLadleNoEast.Enabled = false;
                            textBoxLadleNoWest.Enabled = false;
                            comboBoxSelectAimPos.Enabled = false;
                            butSurePara.Enabled = false;
                        }
                        try
                        {
                            if (myconnection.State == ConnectionState.Closed)
                            {
                                myconnection.Open();//打开数据库     
                            }
                            if (myconnection.State == ConnectionState.Open)
                            {
                                MySQLConnection.Image = Properties.Resources.green2;
                            }
                            ladlenoEast = textBoxLadleNoEast.Text.ToString();
                            ladlenoWest = textBoxLadleNoWest.Text.ToString();
                            if (g_itarget == 1)
                            {
                                string SQL1 = "update sysinteractinstr_copy1 set mode=" + g_iOperateMode + ",target=" + g_itarget + ",ladlenoEast=null,ladleageEast=-1,ladlenoWest=null,ladleageWest=-1 where id='1'";
                                using (MySqlCommand thisCommand = new MySqlCommand(SQL1, myconnection))
                                {
                                    thisCommand.ExecuteNonQuery();
                                }
                                myconnection.Close();
                            }
                            if (g_itarget == 2)
                            {
                                string SQL1 = "update sysinteractinstr_copy1 set mode=" + g_iOperateMode + ",target=" + g_itarget + ",ladlenoEast=null,ladleageEast=-1,ladlenoWest=null,ladleageWest=-1 where id='1'";
                                using (MySqlCommand thisCommand = new MySqlCommand(SQL1, myconnection))
                                {
                                    thisCommand.ExecuteNonQuery();
                                } 
                                myconnection.Close();
                            }
                        }
                        catch (Exception ee)
                        {
                            if (myconnection.State == ConnectionState.Closed)
                            {
                                MySQLConnection.Image = Properties.Resources.redshine;
                            }
                        }
                    }
                    if (b_ladlenoEast || b_ladlenoWest) 
                    {
                        try
                        {
                            if (myconnection.State == ConnectionState.Closed)
                            {
                                myconnection.Open();//打开数据库     
                            }
                            if (myconnection.State == ConnectionState.Open)
                            {
                                MySQLConnection.Image = Properties.Resources.green2;
                            }
                            ladlenoEast = textBoxLadleNoEast.Text.ToString();
                            ladlenoWest = textBoxLadleNoWest.Text.ToString();
                            if (g_itarget == 1)
                            {
                                int ladleageEast = -1;
                                if (b_ladlenoEast)
                                {
                                    
                                    string SQL = "SELECT  Ladle_Age from ladle_age where Ladle_ID ='" + ladlenoEast + "'";
                                    using (MySqlCommand thisCommand = new MySqlCommand(SQL, myconnection))
                                    {
                                        if (thisCommand.ExecuteScalar() != DBNull.Value)
                                        {
                                            iAge = Convert.ToInt32(thisCommand.ExecuteScalar());
                                        }
                                        else
                                        {
                                            iAge = -2;
                                        }
                                    }
                                    if (b_ladlenoEast && (iAge != -2))
                                    {
                                        if (IsEmptyLdlZero(ladlenoEast))
                                        {
                                            if ((MessageBox.Show("此次测量：东包位：包号为" + ladlenoEast + "的空包，实际包龄为" + iAge + ",测厚包龄是否强制为0？", "提示：已测空包表中不存在" + ladlenoEast + "包0包龄记录。", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes) && (iAge != -2))
                                            {
                                                ladleageEast = 0;
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                        string SQL1 = "update sysinteractinstr_copy1 set mode=" + g_iOperateMode + ",target=" + g_itarget + ",ladlenoEast='" + ladlenoEast + "',ladleageEast='" + ladleageEast + "',ladlenoWest=null,ladleageWest=-1 where id='1'";
                                        using (MySqlCommand thisCommand = new MySqlCommand(SQL1, myconnection))
                                        {
                                            thisCommand.ExecuteNonQuery();
                                        }
                                        myconnection.Close();
                                    }
                                    else if(MessageBox.Show("此包非周转包！", "提示", MessageBoxButtons.OK) == DialogResult.OK)
                                    {
                                        textBoxLadleNoEast.Text = "T0";
                                        return;
                                    }

                                }
                                else
                                {
                                    if (MessageBox.Show("请在东包位输入正确格式的包号！", "提示", MessageBoxButtons.OK) == DialogResult.OK)
                                    {
                                        textBoxLadleNoEast.Text = "T0";
                                    }
                                }
                                
                            }
                            if (g_itarget == 2)
                            {
                                int ladleageWest = -1;
                                if (b_ladlenoWest)
                                {
                                    string SQL = "SELECT  Ladle_Age from ladle_age where Ladle_ID ='" + ladlenoWest + "'";
                                    using (MySqlCommand thisCommand = new MySqlCommand(SQL, myconnection))
                                    {
                                        if (thisCommand.ExecuteScalar() != DBNull.Value)
                                        {
                                            iAge = Convert.ToInt32(thisCommand.ExecuteScalar());
                                        }
                                        else
                                        {
                                            iAge = -2;
                                        }
                                    }
                                    if (b_ladlenoWest && (iAge != -2))
                                    {
                                        if (IsEmptyLdlZero(ladlenoWest))
                                        {
                                            if (MessageBox.Show("西包位：包号为" + ladlenoWest + "的空包，实际包龄为" + iAge + ",测厚包龄是否强制为0？", "提示：已测空包表中不存在" + ladlenoWest + "包0包龄记录。", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                                            {
                                                ladleageWest = 0;
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                        string SQL1 = "update sysinteractinstr_copy1 set mode=" + g_iOperateMode + ",target=" + g_itarget + ",ladlenoEast=null,ladleageEast=-1,ladlenoWest='" + ladlenoWest + "',ladleageWest='" + ladleageWest + "' where id='1'";
                                        using (MySqlCommand thisCommand = new MySqlCommand(SQL1, myconnection))
                                        {
                                            thisCommand.ExecuteNonQuery();
                                        }
                                        myconnection.Close();
                                    }
                                    else if (MessageBox.Show("此包非周转包！", "提示", MessageBoxButtons.OK) == DialogResult.OK)
                                    {
                                        textBoxLadleNoWest.Text = "T0";
                                        return;
                                    }
                                }
                                else 
                                {
                                    if (MessageBox.Show("请在西包位输入正确格式的包号！", "提示", MessageBoxButtons.OK) == DialogResult.OK)
                                    {
                                        textBoxLadleNoWest.Text = "T0";
                                        return;
                                    }
                                    
                                }
                                
                            }
                            //半自动
                            if (g_iOperateMode == 1)
                            {
                                XYCarSemiAutoEastPos.Enabled = false;
                                XYCarSemiAutoWestPos.Enabled = false;
                                XYCarSemiAutoEastWestPos.Enabled = false;
                                XCarsToTarget.Enabled = false;
                                YCarToTarget.Enabled = false;
                                MeasThick.Enabled = false;
                                XCarToHome.Enabled = false;
                                YCarToHome.Enabled = false;
                                XYCarsToTarget.Enabled = true;
                                XYCarsTakePhoto.Enabled = false;
                                XYCarsToHome.Enabled = true;
                                textBoxLadleNoEast.Enabled = false;
                                textBoxLadleNoWest.Enabled = false;
                                comboBoxSelectAimPos.Enabled = false;
                                butSurePara.Enabled = false;
                            }
                            //手动
                            if (g_iOperateMode == 2)
                            {
                                XYCarSemiAutoEastPos.Enabled = false;
                                XYCarSemiAutoWestPos.Enabled = false;
                                XYCarSemiAutoEastWestPos.Enabled = false;
                                XCarsToTarget.Enabled = true;
                                YCarToTarget.Enabled = true;
                                MeasThick.Enabled = true;
                                XCarToHome.Enabled = true;
                                YCarToHome.Enabled = true;
                                XYCarsToTarget.Enabled = false;
                                XYCarsTakePhoto.Enabled = false;
                                XYCarsToHome.Enabled = false;
                                textBoxLadleNoEast.Enabled = false;
                                textBoxLadleNoWest.Enabled = false;
                                comboBoxSelectAimPos.Enabled = false;
                                butSurePara.Enabled = false;
                            }
                        }
                        catch (Exception ee)
                        {
                            if (myconnection.State == ConnectionState.Closed)
                            {
                                MySQLConnection.Image = Properties.Resources.redshine;
                            }
                        }
                    }
                }
            }
            

            
            
        }

        int StatusCode = 0, Heart = 0, tiancheheart = 0;
        int thickheartstatus = 0, thickwarning = 0;
        float   thickTmpOut = 0, thickHumid = 0;
        

        private string StatusExplain(int target, byte num)
        {
            string str = "";
            switch (num)
            {
                case 1:
                    {
                        if (target == 1)
                        {
                            str = $"{DateTime.Now} 大车准备开往东包位";
                        }
                        if (target == 2)
                        {
                            str = $"{DateTime.Now} 大车准备开往西包位";
                        }
                    }
                    break;
                case 2:
                    {
                        str = $"{DateTime.Now} 小车到位";
                    }
                    break;
                case 3:
                    {
                        str = $"{DateTime.Now} 测厚设备开始扫拍";
                    }
                    break;
                case 5:
                    {
                        str = $"{DateTime.Now} 大车准备回零";
                    }
                    break;
                case 6:
                    {
                        str = $"{DateTime.Now} 小车准备回零";
                    }
                    break;
                case 7:
                    {
                        if (target == 1)
                        {
                            str = $"{DateTime.Now} 天车正在开往东包位……";
                        }
                        if (target == 2)
                        {
                            str = $"{DateTime.Now} 天车正在开往西包位……";
                        }
                    }
                    break;
                case 8:
                    {
                        str = $"{DateTime.Now} 开始扫拍";
                    }
                    break;
                case 9:
                    {
                        str = $"{DateTime.Now} 天车准备回零……";
                    }
                    break;
                case 11:
                    {
                        str = $"{DateTime.Now} 相机开机超时，已取消测厚，还需测厚需重发指令。";
                    }
                    break;
                case 12:
                    {
                        str = $"{DateTime.Now} 测厚扫描超时，天车将自动开回零位，途中请勿点击停止。待回到零位后，下次发送测厚指令，开机成功后再进行测厚。";
                    }
                    break;
                case 13:
                    {
                        str = $"{DateTime.Now} 提醒，包架不存在，本次测厚取消，还需测厚需重发指令。";
                    }
                    break;
                case 14:
                    {
                        str = $"{DateTime.Now} 天车超时未到测量位，本次测厚将取消，天车将自动回零位。";
                    }
                    break;
                case 15:
                    {
                        str = $"{DateTime.Now} 相机开机完成！";
                    }
                    break;
                case 16:
                    {
                        str = $"{DateTime.Now} 相机正在开机中……";
                    }
                    break;
                case 17:
                    {
                        str = $"{DateTime.Now} 后台收到前端停止命令！";
                    }
                    break;
                case 18:
                    {
                        str = $"{DateTime.Now} 测厚设备高温报警，测厚取消，天车将会自动回零。";
                    }
                    break;
                case 21:
                    {
                        str = $"{DateTime.Now} 大车已到测量位。";
                    }
                    break;
                case 22:
                    {
                        str = $"{DateTime.Now} 小车已到测量位。";
                    }
                    break;
                case 28:
                    {
                        str = $"{DateTime.Now} 扫拍完成！";
                    }
                    break;
                case 25:
                    {
                        str = $"{DateTime.Now} 大车已回到零位。";
                    }
                    break;
                case 26:
                    {
                        str = $"{DateTime.Now} 小车已回到零位。";
                    }
                    break;
                case 27:
                    {
                        str = $"{DateTime.Now} 天车已经到位！";
                    }
                    break;
                case 29:
                    {
                        str = $"{DateTime.Now} 天车已经回到零位！";
                    }
                    break;
                case 71:
                    {
                        str = $"{DateTime.Now} 已进入一键东包位流程";
                    }
                    break;
                case 72:
                    {
                        str = $"{DateTime.Now} 已进入一键西包位流程";
                    }
                    break;
                case 73:
                    {
                        str = $"{DateTime.Now} 已进入一键东西包位流程";
                    }
                    break;
                case 77:
                    {
                        str = $"{DateTime.Now} 已进入天车到位流程";
                    }
                    break;
                case 78:
                    {
                        str = $"{DateTime.Now} 已进入天车扫拍流程";
                    }
                    break;
                case 79:
                    {
                        str = $"{DateTime.Now} 已进入天车回零流程";
                    }
                    break;
                case 81:
                    {
                        str = $"{DateTime.Now} 已进入大车到位流程";
                    }
                    break;
                case 82:
                    {
                        str = $"{DateTime.Now} 已进入小车到位流程";
                    }
                    break;
                case 83:
                    {
                        str = $"{DateTime.Now} 已进入测厚扫拍流程";
                    }
                    break;
                case 85:
                    {
                        str = $"{DateTime.Now} 已进入大车回零流程";
                    }
                    break;
                case 86:
                    {
                        str = $"{DateTime.Now} 已进入小车回零流程";
                    }
                    break;
            }
            return str;
        }

        private List<string> ThickRecord = new List<string>();
        private int ThickRecordCotlast = 0;
        public int StatusIndex=1;
        bool bExitFlag = false;
        bool g_bstatus = false,g_bdrawStatus= false,g_bEastStatus=false,g_bWestStatus=false, g_bEastWestStatus = false;

        private void Button6_Click(object sender, EventArgs e)
        {

        }

        private void DtGridViewBall_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            int i = 0;
            if (e.RowIndex != -1)
            {
                foreach (DataGridViewRow dgvRow in this.dtGridViewBall.Rows)
                {
                    if (i < this.dtGridViewBall.Rows.Count)
                    {
                        // object Position = this.dtGridViewBall.Rows[dgvRow.Index].Cells[1].Value;
                        if (dtGridViewBall.Columns[e.ColumnIndex].Name.Equals("statusCodeBall"))
                        {
                            if (Equals(e.Value.ToString(), "3"))
                            //这里是将列Column2的显示内容改成*号
                            {
                                e.Value = "1、2 ";
                            }
                            if (Equals(e.Value.ToString(), "4"))
                            {
                                e.Value = "3 ";
                            }
                            if (Equals(e.Value.ToString(), "5"))
                            {
                                e.Value = "1、3 ";
                            }
                            if (Equals(e.Value.ToString(), "6"))
                            {
                                e.Value = "2、3 ";
                            }
                            if (Equals(e.Value.ToString(), "7"))
                            {
                                e.Value = "1、2、3 ";
                            }
                            if (Equals(e.Value.ToString(), "8"))
                            {
                                e.Value = "4 ";
                            }
                            if (Equals(e.Value.ToString(), "9"))
                            {
                                e.Value = "1、4 ";
                            }
                            if (Equals(e.Value.ToString(), "10"))
                            {
                                e.Value = "2、4 ";
                            }
                            if (Equals(e.Value.ToString(), "11"))
                            {
                                e.Value = "1、2、4 ";
                            }
                            if (Equals(e.Value.ToString(), "12"))
                            {
                                e.Value = "3、4 ";
                            }
                            if (Equals(e.Value.ToString(), "13"))
                            {
                                e.Value = "1、3、4 ";
                            }
                            if (Equals(e.Value.ToString(), "14"))
                            {
                                e.Value = "2、3、4 ";
                            }
                            if (Equals(e.Value.ToString(), "15"))
                            {
                                e.Value = "1、2、3、4 ";
                            }
                        }
                    }
                    i++;
                }
            }
        }

      

        private void ThickRecordSave(string thickHistoryInstr)
        {
            ThickRecord.Add(thickHistoryInstr);
            
            
        }

        void MonitAutoOp()
        {
            //根据不同按钮设置不同超时
            StatusIndex = 0;
            Thread.Sleep(1000);
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            try
            {
                myconnection.Open();//打开数据库 
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                while (!bExitFlag)
                {
                    Thread.Sleep(1000);
                    byte iFeedBackStatus = 0;
                    int target = 0;
                    long countStatus = 0;
                    int inlen = 200;
                    byte[] statusArry = new byte[inlen];
                    string SQL = "SELECT target,status FROM sysinteractinstr_copy1";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    DataSet ds = new DataSet();
                    using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                    {
                        objDataAdpter.Fill(ds, "sysinteractinstr_copy1");
                    }
                    target = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                    statusArry = (byte[])(ds.Tables[0].Rows[0][1]);
                    countStatus = statusArry.Length;
                    if ((statusArry[0] == g_operation) && (statusArry[countStatus - 1] != StandardAnswer))
                    {
                        g_bstatus = true;
                        if (g_bEastStatus||g_bWestStatus||g_bEastWestStatus)
                        {
                            g_bdrawStatus = true;
                        }
                        for (int i = StatusIndex; i < countStatus; i++)
                        {
                            iFeedBackStatus = statusArry[i];
                            ThickRecordSave(StatusExplain(target, iFeedBackStatus));
                            StatusIndex++;
                            if (i == 0)
                            {
                                if (butStop.IsHandleCreated)
                                {
                                    Action action = () =>
                                    {
                                        butStop.Enabled = true;
                                    };
                                    Invoke(action);
                                }
                            }

                        }
                    }
                    if (g_bstatus&&(statusArry[countStatus - 1] == StandardAnswer))
                    {
                        byte result=statusArry[countStatus - 2];
                        for (int i = StatusIndex; i < countStatus - 1; i++)
                        {
                            iFeedBackStatus = statusArry[i];
                            ThickRecordSave(StatusExplain(target, iFeedBackStatus));
                            StatusIndex++;
                        }
                        switch (m_iOperation)
                        {
                            case 10:
                                {
                                    if (txtBoxShowStatus.IsHandleCreated)
                                    {
                                        txtBoxShowStatus.BeginInvoke(new Action(() =>
                                        {
                                            txtBoxShowStatus.Text = StatusExplain(target, result);
                                        }));
                                    }
                                    
                                }
                                break;
                            case 71:
                                {
                                    /*if (XYCarSemiAutoEastPos.IsHandleCreated)
                                    {
                                        Action action = () =>
                                        {
                                            XYCarSemiAutoEastPos.Enabled = true;
                                        };
                                        Invoke(action);
                                    }*/
                                    if (txtBoxShowStatus.IsHandleCreated)
                                    {
                                        txtBoxShowStatus.BeginInvoke(new Action(() =>
                                        {
                                            txtBoxShowStatus.Text = StatusExplain(target, result);
                                        }));
                                    }
                                    
                                }
                                break;
                            case 72:
                                {
                                    /*Action action = () =>
                                    {
                                        XYCarSemiAutoWestPos.Enabled = true;
                                    };
                                    Invoke(action);*/
                                    if (txtBoxShowStatus.IsHandleCreated)
                                    {
                                        txtBoxShowStatus.BeginInvoke(new Action(() =>
                                        {
                                            txtBoxShowStatus.Text = StatusExplain(target, result);
                                        }));
                                    }
                                }
                                break;
                            case 73:
                                {
                                    /*Action action = () =>
                                    {
                                        XYCarSemiAutoEastWestPos.Enabled = true;
                                    };
                                    Invoke(action);*/
                                    if (txtBoxShowStatus.IsHandleCreated)
                                    {
                                        txtBoxShowStatus.BeginInvoke(new Action(() =>
                                        {
                                            txtBoxShowStatus.Text = StatusExplain(target, result);
                                        }));
                                    }
                                }
                                break;
                            case 7:
                                {
                                    if (result == 27)
                                    {
                                        if (XYCarsTakePhoto.IsHandleCreated)
                                        {
                                            Action action = () =>
                                            {
                                                XYCarsTakePhoto.Enabled = true;
                                            };
                                            Invoke(action);
                                        }
                                    }
                                    if (result == 29)
                                    {
                                        if (XYCarsToHome.IsHandleCreated)
                                        {
                                            Action action = () =>
                                            {
                                                XYCarsToHome.Enabled = true;
                                            };
                                            Invoke(action);
                                        }
                                    }
                                    if (txtBoxShowStatus.IsHandleCreated)
                                    {
                                        txtBoxShowStatus.BeginInvoke(new Action(() =>
                                        {
                                            txtBoxShowStatus.Text = StatusExplain(target, result);
                                        }));
                                    }
                                    

                                }
                                break;
                            case 8:
                                {
                                    if (XYCarsTakePhoto.IsHandleCreated)
                                    {
                                        Action action = () =>
                                        {
                                            XYCarsTakePhoto.Enabled = true;
                                        };
                                        Invoke(action);
                                    }
                                    if (txtBoxShowStatus.IsHandleCreated)
                                    {
                                        txtBoxShowStatus.BeginInvoke(new Action(() =>
                                        {
                                            txtBoxShowStatus.Text = StatusExplain(target, result);
                                        }));
                                    }
                                }
                                break;
                            case 9:
                                {
                                  
                                    if (XYCarsToHome.IsHandleCreated)
                                    {
                                        Action action = () =>
                                        {
                                            XYCarsToHome.Enabled = true;
                                        };
                                        Invoke(action);
                                    }
                                    if (txtBoxShowStatus.IsHandleCreated)
                                    {
                                        txtBoxShowStatus.BeginInvoke(new Action(() =>
                                        {
                                            txtBoxShowStatus.Text = StatusExplain(target, result);
                                        }));
                                    }
                                }
                                break;
                            case 1:
                                {
                                    if (YCarToTarget.IsHandleCreated)
                                    {
                                        Action action = () =>
                                        {
                                            YCarToTarget.Enabled = true;
                                        };
                                        Invoke(action);
                                    }
                                    if (txtBoxShowStatus.IsHandleCreated)
                                    {
                                        txtBoxShowStatus.BeginInvoke(new Action(() =>
                                        {
                                            txtBoxShowStatus.Text = StatusExplain(target, result);
                                        }));
                                    }
                                    
                                }
                                break;
                            case 2:
                                {
                                    if (MeasThick.IsHandleCreated)
                                    {
                                        Action action = () =>
                                        {
                                            MeasThick.Enabled = true;
                                        };
                                        Invoke(action);
                                    }
                                    if (txtBoxShowStatus.IsHandleCreated)
                                    {
                                        txtBoxShowStatus.BeginInvoke(new Action(() =>
                                        {
                                            txtBoxShowStatus.Text = StatusExplain(target, result);
                                        }));
                                    }
                                }
                                break;
                            case 3:
                                {
                                    if (XCarToHome.IsHandleCreated) {
                                        Action action = () =>
                                        {
                                            XCarToHome.Enabled = true;
                                        };
                                        Invoke(action);
                                    }
                                    if (txtBoxShowStatus.IsHandleCreated)
                                    {
                                        txtBoxShowStatus.BeginInvoke(new Action(() =>
                                        {
                                            txtBoxShowStatus.Text = StatusExplain(target, result);
                                        }));
                                    }
                                }
                                break;
                            case 5:
                                {
                                    if (YCarToHome.IsHandleCreated)
                                    {
                                        Action action = () =>
                                        {
                                            YCarToHome.Enabled = true;
                                        };
                                        Invoke(action);
                                    }
                                    if (txtBoxShowStatus.IsHandleCreated)
                                    {
                                        txtBoxShowStatus.BeginInvoke(new Action(() =>
                                        {
                                            txtBoxShowStatus.Text = StatusExplain(target, result);
                                        }));
                                    }
                                }
                                break;
                            case 6:
                                {
                                    if (YCarToHome.IsHandleCreated)
                                    {
                                        Action action = () =>
                                        {
                                            YCarToHome.Enabled = true;
                                        };
                                        Invoke(action);
                                    }
                                    if (txtBoxShowStatus.IsHandleCreated)
                                    {
                                        txtBoxShowStatus.BeginInvoke(new Action(() =>
                                        {
                                            txtBoxShowStatus.Text = StatusExplain(target, result);
                                        }));
                                    }





                                }
                                break;
                        }
                        if (comboBoxSelectMode.IsHandleCreated)
                        {
                            Action action = () =>
                            {
                                comboBoxSelectMode.Enabled = true;
                            };
                            Invoke(action);
                        }
                        bExitFlag = true;
                        ThickRecordSave($"{DateTime.Now}———此过程结束—-—");
                        g_bstatus = false;
                        g_bdrawStatus = false;
                        drawStatusFlag = 0;
                    }
                }
                
            }
            catch (Exception ee)
            {
                MessageBox.Show("解析状态码过程出问题！" + ee.ToString());
            }
            finally
            {
                myconnection.Close();
            }

        }

        private void LaddleSelfCheckSys_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dr;
            dr = MessageBox.Show("确认退出吗", "确认对话框", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.Yes)
            {
                if (ThickRecord.Count != 0)
                {
                    string FileName = $"AppData\\ThickHistoryInstr\\{DateTime.Now.Year}-{DateTime.Now.Month}";
                    if (!Directory.Exists(FileName))
                    {
                        Directory.CreateDirectory(FileName, null);
                    }
                    string strFileName = $"AppData\\ThickHistoryInstr\\{DateTime.Now.Year}-{DateTime.Now.Month}\\{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Hour}-{DateTime.Now.Minute}.txt";
                    //把异常信息输出到文件

                    StreamWriter fs = new StreamWriter(strFileName, true);
                    fs.BaseStream.Seek(0, SeekOrigin.Begin);

                    for (int i = 0; i < ThickRecord.Count; i++)
                    {
                        fs.WriteLine(ThickRecord[i]);
                        fs.Flush();
                    }
                    fs.Close();
                    ThickRecord.Clear();
                    this.Dispose();
                    Application.Exit();
                }  
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void Timer3_Tick(object sender, EventArgs e)
        {
            if ((tabCtrlMeas.SelectedIndex == 1))
            {
                
                if ((ThickRecord.Count!=0) && (ThickRecord.Count != ThickRecordCotlast))
                {
                    for (int i = ThickRecordCotlast; i < ThickRecord.Count; i++)
                    {
                        txtBxThickHistoryInstru.AppendText(ThickRecord[i] + "\r\n");
                    }
                    ThickRecordCotlast = ThickRecord.Count;
                }

                if ((ThickRecord.Count() > MeaThickRecordCount) && (!g_bstatus))
                {
                    string FileName = $"AppData\\ThickHistoryInstr\\{DateTime.Now.Year}-{DateTime.Now.Month}";
                    if (!Directory.Exists(FileName))
                    {
                        Directory.CreateDirectory(FileName, null);
                    }
                    string strFileName = $"AppData\\ThickHistoryInstr\\{DateTime.Now.Year}-{DateTime.Now.Month}\\{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Hour}-{DateTime.Now.Minute}.txt";
                    //把异常信息输出到文件

                    StreamWriter fs = new StreamWriter(strFileName, true);
                    fs.BaseStream.Seek(0, SeekOrigin.Begin);

                    for (int i = 0; i < ThickRecord.Count; i++)
                    {
                        fs.WriteLine(ThickRecord[i]);
                        fs.Flush();
                    }
                    fs.Close();

                    ThickRecord.Clear();
                }
                if ((txtBxThickHistoryInstru.Lines.Count() > 50)&& (!g_bstatus))
                {
                    txtBxThickHistoryInstru.Clear();
                }
                if ((g_bdrawStatus)&&(drawStatusFlag==0))
                {
                    drawStatusFlag = 1;
                    picBoxThickCir.Refresh();
                    picBoxThickBottom.Refresh();
                }
               
            }
        }


        private void Heavyldl_Meased_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                foreach (DataGridViewRow dgvRow in this.Heavyldl_Meased.Rows)
                {
                    object obj = this.Heavyldl_Meased.Rows[dgvRow.Index].Cells[5].Value;
                    if (obj != null && (Convert.ToInt32(obj.ToString()) >= 300))
                    {
                        Heavyldl_Meased.Rows[dgvRow.Index].DefaultCellStyle.BackColor = Color.LightPink;
                    }
                }
            }
            
        }

        private void DtaGrdViewEmtyMeased_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                foreach (DataGridViewRow dgvRow in this.dtaGrdViewEmtyMeased.Rows)
                {
                    object obj_Erode = this.dtaGrdViewEmtyMeased.Rows[dgvRow.Index].Cells[5].Value;

                    object obj = this.dtaGrdViewEmtyMeased.Rows[dgvRow.Index].Cells[8].Value;
                    if (obj_Erode != null && (obj_Erode.ToString() != "无数据") )
                    {
                        if (Convert.ToInt32(obj_Erode) > 80)
                        {
                            dtaGrdViewEmtyMeased.Rows[dgvRow.Index].DefaultCellStyle.BackColor = Color.LightPink;
                        }
                    }
                    if (obj != null && (Convert.ToInt32(obj.ToString()) == 1))
                    {
                        if (obj_Erode.ToString() == "无数据")
                        {
                            dtaGrdViewEmtyMeased.Rows[dgvRow.Index].DefaultCellStyle.BackColor = Color.LightYellow;
                        }
                        if((obj_Erode.ToString() != "无数据")&& (Convert.ToInt32(obj_Erode) <= 80))
                        dtaGrdViewEmtyMeased.Rows[dgvRow.Index].DefaultCellStyle.BackColor = Color.LightYellow;
                    }
                }
            }
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            try
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                string inputLadleNo = this.textBox2.Text.ToString();
                string inputLadleServduty = this.textBox3.Text.ToString();
                string SQL;
                //打开数据库
                
                    bool bladleno = String.Equals(inputLadleNo.Substring(0, 2), "T0") && (inputLadleNo.Length == 4);
                    if (bladleno)
                    {
                        bool bladleServDuty = IsNumber(inputLadleServduty);
                        if (bladleServDuty)
                        {
                            SQL = "select id, LadleNo,LadleServDuty,LadleAge, MeasTm,MinThick,MinThickPos,StatusCode,ModeType  from emptyldl_meased where `Delete`=0 and LadleNo='" + inputLadleNo + "' and LadleServDuty=" + inputLadleServduty + "  order by id desc limit 0,50;";

                            MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                            DataSet ds = new DataSet();
                            using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                            {
                                objDataAdpter.Fill(ds, "emptyldl_meased");
                            }
                            myconnection.Close();
                            dtaGrdViewEmtyMeased.AutoGenerateColumns = false;
                            dtaGrdViewEmtyMeased.DataSource = ds.Tables[0];
                            int rowsCount = ds.Tables[0].Rows.Count;
                            if (rowsCount > 0)
                            {
                                ElID = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                                ThickMeasTm = Convert.ToDateTime(ds.Tables[0].Rows[0][4]);
                                ThickStatusCode = Convert.ToInt32(ds.Tables[0].Rows[0][7]);
                                ShowthickImage();
                            }
                            if (rowsCount == 0)
                            {
                                MessageBox.Show("无查询结果！");
                            }
                        }
                        else
                        {
                            SQL = "select id, LadleNo,LadleServDuty,LadleAge, MeasTm,MinThick,MinThickPos,StatusCode,ModeType  from emptyldl_meased where `Delete`=0 and LadleNo='" + inputLadleNo + "'  order by id desc limit 0,50;";
                            MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                            DataSet ds = new DataSet();
                            using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                            {
                                objDataAdpter.Fill(ds, "emptyldl_meased");
                            }
                            myconnection.Close();
                            dtaGrdViewEmtyMeased.AutoGenerateColumns = false;
                            dtaGrdViewEmtyMeased.DataSource = ds.Tables[0];
                            int rowsCount = ds.Tables[0].Rows.Count;
                            if (rowsCount > 0)
                            {
                                ElID = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                                ThickMeasTm = Convert.ToDateTime(ds.Tables[0].Rows[0][4]);
                                ThickStatusCode = Convert.ToInt32(ds.Tables[0].Rows[0][7]);
                                ShowthickImage();
                            }
                            if (rowsCount == 0)
                            {
                                MessageBox.Show("无查询结果！");
                                return;
                            }
                        }
                    }
                    else
                    {
                        textBox2.Text = "";
                        textBox3.Text = "";
                        MessageBox.Show("请输入正确的包号！");
                        return;
                    }
                
                
            }
            catch (Exception ee)
            {
                MessageBox.Show("查询数据失败！" + ee.ToString());
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            TempThickInfo fmTempThickInfo = new TempThickInfo(Heavyldl_Meased.CurrentRow.Cells[1].Value.ToString(),Convert.ToInt32(Heavyldl_Meased.CurrentRow.Cells[2].Value.ToString()), Convert.ToInt32(Heavyldl_Meased.CurrentRow.Cells[3].Value.ToString()));

            // fmThickBotPic.ThickBotPic_ChangePicture += new ThickBotPic.ChangePictureHandler(Form1_Change_Picture);//使用委托，关闭子窗体时，图片回传
            fmTempThickInfo.Show();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            try
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection1.Image = Properties.Resources.green2;
                }
                string inputLadleNo = this.textBox1.Text.ToString();
                string Time1 = dateTimePicker1.Value.ToString();
                string Time2 = dateTimePicker2.Value.ToString();
                string SQL;
                //打开数据库
                if (inputLadleNo != "")
                {
                    bool bladleno = String.Equals(inputLadleNo.Substring(0, 2), "T0") && (inputLadleNo.Length == 4);
                    if (bladleno)
                    {
                        SQL = "select id, LadleNo,LadleServDuty,LadleAge,LadleContractor, MeasTm, MaxTemp,MaxTempPos from heavyldl_meased where `Delete`=0 and LadleNo='" + inputLadleNo + "' and MeasTm >='" + Time1 + "' and MeasTm <='" + Time2 + "' order by id desc limit 0,50;";
                        MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                        DataSet ds = new DataSet();
                        using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                        {
                            objDataAdpter.Fill(ds, "heavyldl_meased");
                        }
                        Heavyldl_Meased.AutoGenerateColumns = false;
                        Heavyldl_Meased.DataSource = ds.Tables[0];
                        int rowsCount = ds.Tables[0].Rows.Count;
                        if (rowsCount > 0)
                        {
                            TbID = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                            string SQLMaxTempImager = "select MaxTempImagerNo,MaxTempXbyPixel,MaxTempYbyPixel  from heavyldl_meased where id=" + TbID + "";
                            MySqlDataAdapter objDataAdpterMaxTempImager = new MySqlDataAdapter();
                            DataSet dsMaxTempImager = new DataSet();
                            using (objDataAdpterMaxTempImager.SelectCommand = new MySqlCommand(SQLMaxTempImager, myconnection))
                            {
                                objDataAdpterMaxTempImager.Fill(dsMaxTempImager, "heavyldl_meased");
                            }
                            iMaxTempImagerNo = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][0]);
                            iMaxTempXinPixel = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][1]);
                            iMaxTempYbyPixel = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][2]);
                            bDrawMaxTempScope = new bool[5] { false, false, false, false, false };
                            if (iMaxTempImagerNo != 0)
                            {
                                bDrawMaxTempScope[iMaxTempImagerNo - 1] = true;
                            }
                            myconnection.Close();
                            ShowtempPic();
                        }
                        else if (rowsCount == 0)
                        {
                            MessageBox.Show("无查询结果！");
                        }
                    }
                    else
                    {
                        textBox1.Text = "";
                        MessageBox.Show("请输入正确的包号！");
                        return;
                    }
                }
                else
                {
                    SQL = "select id, LadleNo,LadleServDuty, LadleAge,LadleContractor, MeasTm, MaxTemp ,MaxTempPos from heavyldl_meased where `Delete`=0 and MeasTm >='" + Time1 + "' and MeasTm<='" + Time2 + "' order by id desc limit 0,50;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    DataSet ds = new DataSet();
                    using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                    {
                        objDataAdpter.Fill(ds, "heavyldl_meased");
                    }
                    Heavyldl_Meased.AutoGenerateColumns = false;
                    Heavyldl_Meased.DataSource = ds.Tables[0];
                    int rowsCount = ds.Tables[0].Rows.Count;
                    if (rowsCount > 0)
                    {
                        TbID = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                        string SQLMaxTempImager = "select MaxTempImagerNo,MaxTempXbyPixel,MaxTempYbyPixel  from heavyldl_meased where id=" + TbID + "";
                        MySqlDataAdapter objDataAdpterMaxTempImager = new MySqlDataAdapter();
                        DataSet dsMaxTempImager = new DataSet();
                        using (objDataAdpterMaxTempImager.SelectCommand = new MySqlCommand(SQLMaxTempImager, myconnection))
                        {
                            objDataAdpterMaxTempImager.Fill(dsMaxTempImager, "heavyldl_meased");
                        }
                        iMaxTempImagerNo = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][0]);
                        iMaxTempXinPixel = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][1]);
                        iMaxTempYbyPixel = Convert.ToInt32(dsMaxTempImager.Tables[0].Rows[0][2]);
                        bDrawMaxTempScope = new bool[5] { false, false, false, false, false };
                        if (iMaxTempImagerNo != 0)
                        {
                            bDrawMaxTempScope[iMaxTempImagerNo - 1] = true;
                        }
                        myconnection.Close();
                        ShowtempPic();
                    }
                    else if (rowsCount == 0)
                    {
                        MessageBox.Show("无查询结果！");
                        return;
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("查询数据失败！" + ee.ToString());
            }
        }

        private void XCarToTarget_Click(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            try
            {
                //1. 查询交互表反馈字段status，并存储到iFeedBack变量。
                byte iFeedBack = 0;
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                string SQL = "SELECT status FROM sysinteractinstr_copy1";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                DataSet ds = new DataSet();
                using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                {
                    objDataAdpter.Fill(ds, "sysinteractinstr_copy1");
                }
                byte[] ArryStatus = (byte[])(ds.Tables[0].Rows[0][0]);
                int num = ArryStatus.Length;
                iFeedBack = ArryStatus[num - 1];

                //2. 如果status为0，则进行下面操作
                if (iFeedBack == StandardAnswer)
                {
                    if (ModifySysInsStatus())
                    {
                        butStop.Enabled = false;
                        string SQL1 = "update sysinteractinstr_copy1 set operation=1 where id=1;";
                        using (MySqlCommand thisCommand = new MySqlCommand(SQL1, myconnection))
                        {
                            thisCommand.ExecuteNonQuery();
                        }   
                        //MessageBox.Show("启动大车！");
                        g_operation = 81;
                        m_iOperation = 1;
                        txtBoxShowStatus.Text = "启动大车";
                        XCarsToTarget.Enabled = false;
                        if ((g_iOperateMode == 1) || (g_iOperateMode == 2)) //半自动，手动
                        {
                            bExitFlag = false;
                            if (!bExitFlag)
                            {
                                AnalysisStatus();
                            }
                        }
                    }
                    
                }
                else
                {
                    MessageBox.Show("当前正在执行其他操作！请等待......");
                }
                myconnection.Close();
            }
            catch (Exception ee)
            {
                
                if (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
        }

        private void YCarToTarget_Click(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            //1. 查询交互表反馈字段status，并存储到iFeedBack变量。
            try
            {
                byte iFeedBack = 0;
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                string SQL = "SELECT status FROM sysinteractinstr_copy1";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                DataSet ds = new DataSet();
                using (objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection))
                {
                    objDataAdpter.Fill(ds, "sysinteractinstr_copy1");
                }
                byte[] ArryStatus = (byte[])(ds.Tables[0].Rows[0][0]);
                int num = ArryStatus.Length;
                iFeedBack = ArryStatus[num - 1];

                //2. 如果status为0，则进行下面操作
                if (iFeedBack == StandardAnswer)
                {
                    if (ModifySysInsStatus())
                    {
                        butStop.Enabled = false;
                        string SQL1 = "update sysinteractinstr_copy1 set operation=2 where id=1;";
                        using (MySqlCommand thisCommand = new MySqlCommand(SQL1, myconnection))
                        {
                            thisCommand.ExecuteNonQuery();
                        }
                        g_operation = 82;
                        m_iOperation = 2;
                        txtBoxShowStatus.Text = "启动小车";
                        YCarToTarget.Enabled = false;
                        if ((g_iOperateMode == 1) || (g_iOperateMode == 2)) //半自动，手动
                        {
                            bExitFlag = false;
                            if (!bExitFlag)
                            {
                                AnalysisStatus();
                            }
                        }
                    }
                    
                }
                else
                {
                    MessageBox.Show("当前正在执行其他操作！请等待......");
                }
                myconnection.Close();
            }
            catch (Exception ee)
            {
                
                if (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
                
            }
        }

        

        private void XCarToHome_Click(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            try
            {
                //1. 查询交互表反馈字段status，并存储到iFeedBack变量。
                byte iFeedBack = 0;

                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                string SQL = "SELECT status FROM sysinteractinstr_copy1";
                DataSet ds = new DataSet();
                using (MySqlDataAdapter objDataAdpter = new MySqlDataAdapter())
                {
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection);
                    objDataAdpter.Fill(ds, "sysinteractinstr_copy1");
                }
                byte[] ArryStatus = (byte[])(ds.Tables[0].Rows[0][0]);
                int num = ArryStatus.Length;
                iFeedBack = ArryStatus[num - 1];

                //2. 如果status为0，则进行下面操作
                if (iFeedBack == StandardAnswer)
                {
                    if (ModifySysInsStatus())
                    {
                        butStop.Enabled = false;
                        string strSQL1 = "update sysinteractinstr_copy1 set operation=5 where id=1;";
                        MySqlCommand thisCommand = new MySqlCommand(strSQL1, myconnection);
                        thisCommand.ExecuteNonQuery();
                        txtBoxShowStatus.Text = "大车准备回零";
                        g_operation = 85;
                        m_iOperation = 5;
                        XCarToHome.Enabled = false;
                        if ((g_iOperateMode == 1) || (g_iOperateMode == 2)) //半自动，手动
                        {
                            bExitFlag = false;
                            if (!bExitFlag)
                            {
                                AnalysisStatus();
                            }
                        }
                    }
                    
                }
                else
                {
                    MessageBox.Show("当前正在执行其他操作！请等待......");
                }
                myconnection.Close();
            }
            catch (Exception ee)
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
        }

        private void YCarToHome_Click(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            //1. 查询交互表反馈字段status，并存储到iFeedBack变量。
            try
            {
                byte iFeedBack = 0;
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }

                string SQL = "SELECT status FROM sysinteractinstr_copy1";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "sysinteractinstr_copy1");
                byte[] ArryStatus = (byte[])(ds.Tables[0].Rows[0][0]);
                int num = ArryStatus.Length;
                iFeedBack = ArryStatus[num - 1];

                //2. 如果status为0，则进行下面操作
                if (iFeedBack == StandardAnswer)
                {
                    if (ModifySysInsStatus())
                    {
                        butStop.Enabled = false;
                        string strSQL1 = "update sysinteractinstr_copy1 set operation=6 where id=1;";
                        MySqlCommand thisCommand = new MySqlCommand(strSQL1, myconnection);
                        thisCommand.ExecuteNonQuery();
                        txtBoxShowStatus.Text = "小车准备回零";
                        g_operation = 86;
                        m_iOperation = 6;
                        YCarToHome.Enabled = false;
                        if ((g_iOperateMode == 1) || (g_iOperateMode == 2)) //半自动，手动
                        {
                            bExitFlag = false;
                            if (!bExitFlag)
                            {
                                AnalysisStatus();
                            }
                        }
                    }
                    
                }
                else
                {
                    MessageBox.Show("当前正在执行其他操作！请等待......");
                }
                myconnection.Close();
            }
            catch (Exception ee)
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
        }

        private void XYCarsToTarget_Click(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            //1. 查询交互表反馈字段status，并存储到iFeedBack变量。
            try
            {
                byte iFeedBack = 0;
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                string SQL = "SELECT status FROM sysinteractinstr_copy1";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "sysinteractinstr_copy1");
                byte[] ArryStatus = (byte[])(ds.Tables[0].Rows[0][0]);
                int num = ArryStatus.Length;
                iFeedBack = ArryStatus[num - 1];

                //2. 如果status为0，则进行下面操作
                if (iFeedBack == StandardAnswer)
                {
                    if (ModifySysInsStatus())
                    {
                        butStop.Enabled = false;
                        comboBoxSelectMode.Enabled = false;
                        string strSQL1 = "update sysinteractinstr_copy1 set operation=7 where id=1;";
                        MySqlCommand thisCommand = new MySqlCommand(strSQL1, myconnection);
                        thisCommand.ExecuteNonQuery();
                        if (g_itarget == 1)
                        {
                            txtBoxShowStatus.Text = "天车去往东包位";
                        }
                        if (g_itarget == 2)
                        {
                            txtBoxShowStatus.Text = "天车去往西包位";
                        }
                        g_operation = 77;
                        m_iOperation = 7;
                        XYCarsToTarget.Enabled = false;
                        if ((g_iOperateMode == 1) || (g_iOperateMode == 2)) //半自动，手动
                        {
                            bExitFlag = false;
                            if (!bExitFlag)
                            {
                                AnalysisStatus();
                            }
                        }
                    }
                    
                }
                else
                {
                    MessageBox.Show("当前正在执行其他操作！请等待......");
                }
                myconnection.Close();
            }
            catch (Exception ee)
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
        }

        private void MeasThick_Click(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
           
            //1. 查询交互表反馈字段status，并存储到iFeedBack变量。
            try
            {
                byte iFeedBack = 0;
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                string SQL = "SELECT status FROM sysinteractinstr_copy1";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "sysinteractinstr_copy1");
                byte[] ArryStatus = (byte[])(ds.Tables[0].Rows[0][0]);
                int num = ArryStatus.Length;
                iFeedBack = ArryStatus[num - 1];

                //2. 如果status为0，则进行下面操作
                if (iFeedBack == StandardAnswer)
                {
                    if (ModifySysInsStatus())
                    {
                        butStop.Enabled = false;
                        string strSQL1 = "update sysinteractinstr_copy1 set operation=3 where id=1;";
                        MySqlCommand thisCommand = new MySqlCommand(strSQL1, myconnection);
                        thisCommand.ExecuteNonQuery();
                        txtBoxShowStatus.Text = "启动扫描";
                        g_operation = 83;
                        m_iOperation = 3;
                        MeasThick.Enabled = false;
                        if ((g_iOperateMode == 1) || (g_iOperateMode == 2)) //半自动，手动
                        {
                            bExitFlag = false;
                            if (!bExitFlag)
                            {
                                AnalysisStatus();
                            }
                        }
                    }
                    
                }
                else
                {
                    MessageBox.Show("当前正在执行其他操作！请等待......");
                }
                myconnection.Close();
            }
            catch (Exception ee)
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
        }

        private void XYCarsTakePhoto_Click(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            try
            {
                byte iFeedBack = 0;
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                string SQL = "SELECT status FROM sysinteractinstr_copy1";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "sysinteractinstr_copy1");
                byte[] ArryStatus = (byte[])(ds.Tables[0].Rows[0][0]);
                int num = ArryStatus.Length;
                iFeedBack = ArryStatus[num - 1];

                //2. 如果status为0，则进行下面操作
                if (iFeedBack == StandardAnswer)
                {
                    if (ModifySysInsStatus())
                    {
                        comboBoxSelectMode.Enabled = false;
                        butStop.Enabled = false;
                        string strSQL1 = "update sysinteractinstr_copy1 set operation=8 where id=1;";
                        MySqlCommand thisCommand = new MySqlCommand(strSQL1, myconnection);
                        thisCommand.ExecuteNonQuery();
                        txtBoxShowStatus.Text = "启动扫拍";
                        g_operation = 78;
                        m_iOperation = 8;
                        XYCarsTakePhoto.Enabled = false;
                        if ((g_iOperateMode == 1) || (g_iOperateMode == 2)) //半自动，手动
                        {
                            bExitFlag = false;
                            if (!bExitFlag)
                            {
                                AnalysisStatus();
                            }
                        }
                    }
                    
                }
                else
                {
                    MessageBox.Show("当前正在执行其他操作！请等待......");
                }
                myconnection.Close();
            }
            catch (Exception ee)
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
        }

        private void XYCarsToHome_Click(object sender, EventArgs e)
        {
            MySqlConnection myconnection = new MySqlConnection("user id=" + UserID + ";password=" + Password + ";server=" + Server + ";persistsecurityinfo=True;database=ironldlthermodetectmanactrl;SslMode=None;allowPublicKeyRetrieval=true");
            try
            {
                byte iFeedBack = 30;
                //1. 查询交互表反馈字段status，并存储到iFeedBack变量。
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();//打开数据库     
                }
                if (myconnection.State == ConnectionState.Open)
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                string SQL = "SELECT status FROM sysinteractinstr_copy1";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "sysinteractinstr_copy1");
                byte[] ArryStatus = (byte[])(ds.Tables[0].Rows[0][0]);
                int num = ArryStatus.Length;
                iFeedBack = ArryStatus[num - 1];

                //2. 如果status为0，则进行下面操作
                if (iFeedBack == StandardAnswer)
                {
                    if (ModifySysInsStatus())
                    {
                        butStop.Enabled = false;
                        comboBoxSelectMode.Enabled = false;
                        string strSQL1 = "update sysinteractinstr_copy1 set operation=9 where id=1;";
                        MySqlCommand thisCommand = new MySqlCommand(strSQL1, myconnection);
                        thisCommand.ExecuteNonQuery();
                        txtBoxShowStatus.Text = "天车正在回零";
                        g_operation = 79;
                        m_iOperation = 9;
                        XYCarsToHome.Enabled = false;
                        if ((g_iOperateMode == 1) || (g_iOperateMode == 2)) //半自动，手动
                        {
                            bExitFlag = false;
                            //启动手动操作按钮监控线程
                            AnalysisStatus();
                        }
                    }
                    
                }
                else
                {
                    MessageBox.Show("当前正在执行其他操作！请等待......");
                }
                myconnection.Close();
            }
            catch (Exception ee)
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
        }

        private static int ThickCirRows = 360;
        private static int ThickCirCols = 720;
        private static int ThickBotRows = 380;
        private static int ThickBotCols = 380;
        Int16[] dataThickCir = new Int16[ThickCirRows * ThickCirCols];
        Int16[] dataThickBot = new Int16[ThickBotRows * ThickBotCols];

        /*Color[] cr_thick = new Color[] { 
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
         Color.Black};*/

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
        private void ThickReadBlob()
        {
            MySqlConnection myconnection = new MySqlConnection(SQLServerUser);
            try
            {
                if (myconnection.State == ConnectionState.Closed)
                {
                    myconnection.Open();
                }
                if ((tabCtrlMeas.SelectedIndex == 0) && (myconnection.State == ConnectionState.Open))
                {
                    MySQLConnection1.Image = Properties.Resources.green2;
                }
                if ((tabCtrlMeas.SelectedIndex == 1) && (myconnection.State == ConnectionState.Open))
                {
                    MySQLConnection.Image = Properties.Resources.green2;
                }
                
                string SQL = "SELECT  CircumThick,BottomThick,ThickPhoto  FROM emptyldl_meased where id ='" + ElID + "';";
                //MySqlCommand cmd = new MySqlCommand(SQL, myconnection);
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, myconnection);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "emptyldl_meased");
                myconnection.Close();
                //MySqlDataReader rdr = cmd.ExecuteReader();
                bDrawThick[0] = false; bDrawThick[1] = false;
                //while (rdr.Read())
                {
                    bDrawThick[0] = true; bDrawThick[1] = true;
                    if (ds.Tables[0].Rows[0][0] == System.DBNull.Value)
                    {
                        bDrawThick[0] = false;
                    }
                    if (ds.Tables[0].Rows[0][1] == System.DBNull.Value)
                    {
                        bDrawThick[1] = false;
                    }

                    
                    if (bDrawThick[0]) 
                    {
                        //long len;
                        int inlen = ThickCirRows * ThickCirCols * 2;
                        byte[] buffer = new byte[inlen];
                        buffer = (byte[])ds.Tables[0].Rows[0][0];
                        for (int i = 0; i < inlen / 2; i++)
                        {
                            dataThickCir[i] = (Int16)((buffer[2 * i]) + (buffer[2 * i + 1]) * 256);

                        }
                    }

                    if (bDrawThick[1]) 
                    {
                        
                        int inlen1 = ThickBotRows * ThickBotCols * 2;
                        byte[] buffer1 = new byte[inlen1];
                        buffer1 = (byte[])ds.Tables[0].Rows[0][1];
                        //存放获得的二进制数据，温度
                        
                        for (int i = 0; i < inlen1 / 2; i++)
                        {
                            dataThickBot[i] = (Int16)((buffer1[2 * i]) + (buffer1[2 * i + 1]) * 256);
                        }
                    }
                    if (ds.Tables[0].Rows[0][2] == DBNull.Value)
                    {
                        Image image = LadleThermDetectSys.Properties.Resources.内衬照片11;
                        picBoxBottomPic.Image = image;
                        image_rote = image;
                    }
                    else
                    {
                        byte[] Thick = (byte[])ds.Tables[0].Rows[0][2];
                        MemoryStream stream = new MemoryStream(Thick, true);
                        Image image = Image.FromStream(stream, true);
                        stream.Close();
                        picBoxBottomPic.Image = image;
                        image_rote = image;
                    }
                    image_rote.RotateFlip(RotateFlipType.Rotate270FlipXY);

                }
                //rdr.Close();
                
            }
            catch (Exception EE)
            {
                if ((tabCtrlMeas.SelectedIndex == 0) && (myconnection.State == ConnectionState.Closed))
                {
                    MySQLConnection1.Image = Properties.Resources.redshine;
                }
                if ((tabCtrlMeas.SelectedIndex == 1) && (myconnection.State == ConnectionState.Closed))
                {
                    MySQLConnection.Image = Properties.Resources.redshine;
                }
            }
        }
        private void ShowthickImage()
        {
            ThickReadBlob();
            picBoxThickCir.Refresh();
            picBoxThickBottom.Refresh();
        }
        Image image_rote;
        
        private double f_dwf=0, f_dhf=0,s_dwf=0,s_dhf=0;
        private void picBoxThickCir_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            if (bDrawThick[0]&&(!g_bdrawStatus))
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
            if (!bDrawThick[0] && (!g_bdrawStatus))
            {
                if (ThickStatusCode != 0)
                {
                    bool b_dwf00 = false, b_dwf01 = false, b_dwf02 = false, b_dwf03 = false;
                    int width = picBoxThickCir.Size.Width;
                    int widthd = width * 3 / 5;
                    int widthd1 = width * 1 / 3;
                    int widthd2 = width * 2 / 7;
                    int height = picBoxThickCir.Size.Height;
                    int heightd = height * 2 / 3;
                    int heightd1 = height * 1 / 3;
                    Pen p1 = new Pen(Color.FromArgb(205, 129, 98), 5);
                    Pen p2 = new Pen(Color.Red, 5);
                    g.DrawArc(p1, new Rectangle(widthd, 35, widthd1 - 30, heightd), 165, 210);
                    g.DrawArc(p1, new Rectangle(widthd + 5, 70, widthd1 - 40, heightd), 0, 180);
                    using (Font f = new Font("黑体", 10))//字体
                    {
                        g.DrawString("大包嘴", f, Brushes.Black, widthd + (widthd1 - 30) / 2 - 10, 15);
                        g.DrawString("小包嘴", f, Brushes.Black, widthd + 5 + (widthd1 - 40) / 2 - 10, 70 + heightd + 5);
                    }
                    string ThickStatusAll = Convert.ToString(ThickStatusCode, 2);
                    for (int i = ThickStatusAll.Length - 1; i >= 0; i--)
                    {
                        if (i == ThickStatusAll.Length - 1)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf00 = true;
                            }
                        }
                        if (i == ThickStatusAll.Length - 2)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf01 = true;
                            }
                        }
                        if (i == ThickStatusAll.Length - 3)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf02 = true;
                            }
                        }
                        if (i == ThickStatusAll.Length - 4)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf03 = true;
                            }
                        }
                    }
                    if (b_dwf00)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd + widthd1 - 33, 150, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd + widthd1 - 33, 150, 20, 20), 0, 360);
                    }
                    if (b_dwf01)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd - 15, 150, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd - 15, 150, 20, 20), 0, 360);
                    }
                    if (b_dwf02)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd - 5, 220, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd - 5, 220, 20, 20), 0, 360);
                    }
                    if (b_dwf03)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd + widthd1 - 45, 220, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd + widthd1 - 45, 220, 20, 20), 0, 360);
                    }
                    using (Font f = new Font("黑体", 20))//字体
                    {
                        g.DrawString("此包测量时间为" + ThickMeasTm + "", f, Brushes.Black, 15, 10);
                        g.DrawString("测量失败原因为定位球故障", f, Brushes.Black, 15, 40);
                        g.DrawString("右图图示红色定位球即为需维护定位球", f, Brushes.Black, 15, 70);
                        g.DrawString("请联系脱硫站工作人员进行定位球维护", f, Brushes.Black, 15, 100);
                    }
                    p1.Dispose();
                    //g.Dispose();
                }
                if (ThickStatusCode == 0)
                {
                    bool b_dwf00 = false, b_dwf01 = false, b_dwf02 = false, b_dwf03 = false;
                    int width = picBoxThickCir.Size.Width;
                    int widthd = width * 3 / 5;
                    int widthd1 = width * 1 / 3;
                    int widthd2 = width * 2 / 7;
                    int height = picBoxThickCir.Size.Height;
                    int heightd = height * 2 / 3;
                    int heightd1 = height * 1 / 3;
                    Pen p1 = new Pen(Color.FromArgb(205, 129, 98), 5);
                    Pen p2 = new Pen(Color.Red, 5);
                    g.DrawArc(p1, new Rectangle(widthd, 35, widthd1 - 30, heightd), 165, 210);
                    g.DrawArc(p1, new Rectangle(widthd + 5, 70, widthd1 - 40, heightd), 0, 180);
                    using (Font f = new Font("黑体", 10))//字体
                    {
                        g.DrawString("大包嘴", f, Brushes.Black, widthd + (widthd1 - 30) / 2 - 10, 15);
                        g.DrawString("小包嘴", f, Brushes.Black, widthd + 5 + (widthd1 - 40) / 2 - 10, 70 + heightd + 5);
                    }
                    string ThickStatusAll = Convert.ToString(ThickStatusCode, 2);
                    for (int i = ThickStatusAll.Length - 1; i >= 0; i--)
                    {
                        if (i == ThickStatusAll.Length - 1)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf00 = true;
                            }
                        }
                        if (i == ThickStatusAll.Length - 2)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf01 = true;
                            }
                        }
                        if (i == ThickStatusAll.Length - 3)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf02 = true;
                            }
                        }
                        if (i == ThickStatusAll.Length - 4)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf03 = true;
                            }
                        }
                    }
                    if (b_dwf00)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd + widthd1 - 33, 150, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd + widthd1 - 33, 150, 20, 20), 0, 360);
                    }
                    if (b_dwf01)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd - 15, 150, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd - 15, 150, 20, 20), 0, 360);
                    }
                    if (b_dwf02)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd - 5, 220, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd - 5, 220, 20, 20), 0, 360);
                    }
                    if (b_dwf03)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd + widthd1 - 45, 220, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd + widthd1 - 45, 220, 20, 20), 0, 360);
                    }
                    using (Font f = new Font("黑体", 20))//字体
                    {
                        g.DrawString("此包测量时间为" + ThickMeasTm + "", f, Brushes.Black, 15, 10);
                        g.DrawString("测量失败原因为定位误差超限", f, Brushes.Black, 15, 40);
                        g.DrawString("定位球上有渣子，但无法确定具体位置", f, Brushes.Black, 15, 70);
                        g.DrawString("请联系脱硫站工作人员进行定位球维护", f, Brushes.Black, 15, 100);
                    }
                    p1.Dispose();
                    //g.Dispose();
                }
            }
            if (g_bdrawStatus)
            {
                if (g_bEastStatus)
                {
                    //g_bEastStatus = false;
                    bool b_dwf00 = false, b_dwf01 = false, b_dwf02 = false, b_dwf03 = false;
                    int width = picBoxThickCir.Size.Width;
                    int widthd = width * 3 / 5;
                    int widthd1 = width * 1 / 3;
                    int widthd2 = width * 2 / 7;
                    int height = picBoxThickCir.Size.Height;
                    int heightd = height * 2 / 3;
                    int heightd1 = height * 1 / 3;
                    Pen p1 = new Pen(Color.FromArgb(205, 129, 98), 5);
                    Pen p2 = new Pen(Color.Red, 5);
                    g.DrawArc(p1, new Rectangle(widthd, 35, widthd1 - 30, heightd), 165, 210);
                    g.DrawArc(p1, new Rectangle(widthd + 5, 70, widthd1 - 40, heightd), 0, 180);
                    using (Font f = new Font("黑体", 10))//字体
                    {
                        g.DrawString("大包嘴", f, Brushes.Black, widthd + (widthd1 - 30) / 2 - 10, 15);
                        g.DrawString("小包嘴", f, Brushes.Black, widthd + 5 + (widthd1 - 40) / 2 - 10, 70 + heightd + 5);
                    }
                    string ThickStatusAll = Convert.ToString(ThickStatusCodeEast, 2);
                    for (int i = ThickStatusAll.Length - 1; i >= 0; i--)
                    {
                        if (i == ThickStatusAll.Length - 1)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf00 = true;
                            }
                        }
                        if (i == ThickStatusAll.Length - 2)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf01 = true;
                            }
                        }
                        if (i == ThickStatusAll.Length - 3)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf02 = true;
                            }
                        }
                        if (i == ThickStatusAll.Length - 4)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf03 = true;
                            }
                        }
                    }
                    if (b_dwf00)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd + widthd1 - 33, 150, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd + widthd1 - 33, 150, 20, 20), 0, 360);
                    }
                    if (b_dwf01)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd - 15, 150, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd - 15, 150, 20, 20), 0, 360);
                    }
                    if (b_dwf02)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd - 5, 220, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd - 5, 220, 20, 20), 0, 360);
                    }
                    if (b_dwf03)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd + widthd1 - 45, 220, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd + widthd1 - 45, 220, 20, 20), 0, 360);
                    }
                    using (Font f = new Font("黑体", 20))//字体
                    {
                        g.DrawString("此次测量铁包" + textBoxLadleNoEast.Text + "上次在钢三线测量时间为" + ThickMeasTmEast + "", f, Brushes.Black, 15, 10);
                        g.DrawString("测量失败原因为定位球故障", f, Brushes.Black, 15, 40);
                        g.DrawString("右图图示红色定位球即为需维护定位球", f, Brushes.Black, 15, 70);
                        g.DrawString("请联系脱硫站工作人员进行定位球维护", f, Brushes.Black, 15, 100);
                    }
                    p1.Dispose();
                }
                if (g_bWestStatus)
                {
                    //g_bEastStatus = false;
                    bool b_dwf00 = false, b_dwf01 = false, b_dwf02 = false, b_dwf03 = false;
                    int width = picBoxThickCir.Size.Width;
                    int widthd = width * 3 / 5;
                    int widthd1 = width * 1 / 3;
                    int widthd2 = width * 2 / 7;
                    int height = picBoxThickCir.Size.Height;
                    int heightd = height * 2 / 3;
                    int heightd1 = height * 1 / 3;
                    Pen p1 = new Pen(Color.FromArgb(205, 129, 98), 5);
                    Pen p2 = new Pen(Color.Red, 5);
                    g.DrawArc(p1, new Rectangle(widthd, 35, widthd1 - 30, heightd), 165, 210);
                    g.DrawArc(p1, new Rectangle(widthd + 5, 70, widthd1 - 40, heightd), 0, 180);
                    using (Font f = new Font("黑体", 10))//字体
                    {
                        g.DrawString("大包嘴", f, Brushes.Black, widthd + (widthd1 - 30) / 2 - 10, 15);
                        g.DrawString("小包嘴", f, Brushes.Black, widthd + 5 + (widthd1 - 40) / 2 - 10, 70 + heightd + 5);
                    }
                    string ThickStatusAll = Convert.ToString(ThickStatusCodeWest, 2);
                    for (int i = ThickStatusAll.Length - 1; i >= 0; i--)
                    {
                        if (i == ThickStatusAll.Length - 1)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf00 = true;
                            }
                        }
                        if (i == ThickStatusAll.Length - 2)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf01 = true;
                            }
                        }
                        if (i == ThickStatusAll.Length - 3)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf02 = true;
                            }
                        }
                        if (i == ThickStatusAll.Length - 4)
                        {
                            if (String.Equals(ThickStatusAll.Substring(i, 1), "1"))
                            {
                                b_dwf03 = true;
                            }
                        }
                    }
                    if (b_dwf00)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd + widthd1 - 33, 150, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd + widthd1 - 33, 150, 20, 20), 0, 360);
                    }
                    if (b_dwf01)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd - 15, 150, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd - 15, 150, 20, 20), 0, 360);
                    }
                    if (b_dwf02)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd - 5, 220, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd - 5, 220, 20, 20), 0, 360);
                    }
                    if (b_dwf03)
                    {
                        g.FillPie(Brushes.Red, new Rectangle(widthd + widthd1 - 45, 220, 20, 20), 0, 360);
                    }
                    else
                    {
                        g.FillPie(Brushes.Green, new Rectangle(widthd + widthd1 - 45, 220, 20, 20), 0, 360);
                    }
                    using (Font f = new Font("黑体", 20))//字体
                    {
                        g.DrawString("此次测量铁包" + textBoxLadleNoWest.Text + "上次在钢三线测量时间为" + ThickMeasTmWest + "", f, Brushes.Black, 15, 10);
                        g.DrawString("测量失败原因为定位球故障", f, Brushes.Black, 15, 40);
                        g.DrawString("右图图示红色定位球即为需维护定位球", f, Brushes.Black, 15, 70);
                        g.DrawString("请联系脱硫站工作人员进行定位球维护", f, Brushes.Black, 15, 100);
                    }
                    p1.Dispose();
                }
                if (g_bEastWestStatus)
                {
                    string b_dwf00East = "", b_dwf01East = "", b_dwf02East = "", b_dwf03East = "";
                    string b_dwf00West = "", b_dwf01West = "", b_dwf02West = "", b_dwf03West = "";
                    string ThickStatusAllEast = Convert.ToString(ThickStatusCodeEast, 2);
                    for (int i = ThickStatusAllEast.Length - 1; i >= 0; i--)
                    {
                        if (i == ThickStatusAllEast.Length - 1)
                        {
                            if (String.Equals(ThickStatusAllEast.Substring(i, 1), "1"))
                            {
                                b_dwf00East = "东北方位";
                            }
                        }
                        if (i == ThickStatusAllEast.Length - 2)
                        {
                            if (String.Equals(ThickStatusAllEast.Substring(i, 1), "1"))
                            {
                                b_dwf01East = "西北方位";
                            }
                        }
                        if (i == ThickStatusAllEast.Length - 3)
                        {
                            if (String.Equals(ThickStatusAllEast.Substring(i, 1), "1"))
                            {
                                b_dwf02East = "西南方位";
                            }
                        }
                        if (i == ThickStatusAllEast.Length - 4)
                        {
                            if (String.Equals(ThickStatusAllEast.Substring(i, 1), "1"))
                            {
                                b_dwf03East = "东南方位";
                            }
                        }
                    }
                    string ThickStatusAllWest = Convert.ToString(ThickStatusCodeWest, 2);
                    for (int i = ThickStatusAllWest.Length - 1; i >= 0; i--)
                    {
                        if (i == ThickStatusAllWest.Length - 1)
                        {
                            if (String.Equals(ThickStatusAllWest.Substring(i, 1), "1"))
                            {
                                b_dwf00West = "东北方位";
                            }
                        }
                        if (i == ThickStatusAllWest.Length - 2)
                        {
                            if (String.Equals(ThickStatusAllWest.Substring(i, 1), "1"))
                            {
                                b_dwf01West = "西北方位";
                            }
                        }
                        if (i == ThickStatusAllWest.Length - 3)
                        {
                            if (String.Equals(ThickStatusAllWest.Substring(i, 1), "1"))
                            {
                                b_dwf02West = "西南方位";
                            }
                        }
                        if (i == ThickStatusAllWest.Length - 4)
                        {
                            if (String.Equals(ThickStatusAllWest.Substring(i, 1), "1"))
                            {
                                b_dwf03West = "东南方位";
                            }
                        }
                    }
                    using (Font f = new Font("黑体", 25))//字体
                    {
                        g.DrawString("此次东包位测量铁包" + textBoxLadleNoEast.Text + "上次在钢三线测量时间为" + ThickMeasTmEast + "", f, Brushes.Black, 3, 10);
                       /// g.DrawString("测量失败原因为定位球故障", f, Brushes.Black,10,35);
                        g.DrawString("东包位铁包" + textBoxLadleNoEast.Text + "维护"+b_dwf00East+"、"+b_dwf01East+"、"+b_dwf02East+"、"+b_dwf03East+"定位球", f, Brushes.Black, 3, 50);
                        g.DrawString("请联系脱硫站工作人员进行定位球维护", f, Brushes.Black, 3, 90);
                        g.DrawString("此次西包位测量铁包" + textBoxLadleNoWest.Text + "上次在钢三线测量时间为" + ThickMeasTmWest + "", f, Brushes.Black, 3,170);
                        //g.DrawString("测量失败原因为定位球故障", f, Brushes.Black, 10, 145);
                        g.DrawString("西包位铁包" + textBoxLadleNoWest.Text + "维护"+b_dwf00West+"、"+b_dwf01West+"、"+b_dwf02West+"、"+b_dwf03West+"定位球", f, Brushes.Black, 3, 210);
                        g.DrawString("请联系脱硫站工作人员进行定位球维护", f, Brushes.Black, 3, 250);
                    }
                }
            }
        }

        
        private void picBoxThickBottom_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            if (bDrawThick[1]&&(!g_bdrawStatus))
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
