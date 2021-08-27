using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Threading;
using Oracle.DataAccess.Client;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace IronLadleThermoDetectManageCtrlSystem
{
    public partial class Form1 : Form
    {
        AutoResizeForm asc = new AutoResizeForm();
        public Form1()
        {
            PaddleXPredict.loadmodel(@"热像包号inference_model");
            InitializeComponent();
        }

        //定义数据库连接字符串
        //public static string MySQLUser = "user id=root;password=zcy1200;server=localhost;persistsecurityinfo=True;database=ironldlthermodetectmanactrl";
        public static string MySQLUser = "user id=root;password=ARIMlab2020.07.22;server=localhost;persistsecurityinfo=True;database=ironldlthermodetectmanactrl;port=3306;SslMode=None;allowPublicKeyRetrieval=true ";

        public static string SQLServerUser = "Data Source = 192.168.18.110;Initial Catalog = JTLadleDB; User ID = TBRJ; Password=TBRJ;Integrated Security=false; ";
        // public static string SQLServerUser = "Data Source=LEBRON;Initial Catalog=ironldlthermodetectmanactrl;Integrated Security=True; ";

        public static string OracleUser = " user id = ladletrack; password=ladletrack;Pooling=false;Persist Security Info=true;data source = (DESCRIPTION = (ADDRESS = (PROTOCOL = tcp)(HOST = 10.99.198.61)(PORT = 1521))(CONNECT_DATA = (SERVICE_NAME = ltcsorcl)))";
      
        public static int tiancheHeartLast=-1, headHeartLast=-1, thickHeartLast=-1;
        public static int tiancheHeartNow, headHeartNow, thickHeartNow;

        //检测前台回执标志位          
        public static int g_mode = 0;
        public static int g_operation = 0;
        public static int All_operation;
        public static int g_target = 0;
        public static int g_warning = 0;
        public static string HandleEmptyladlenoEast, HandleEmptyladlenoWest;
        public static int HandleEmptyladleAgeEast, HandleEmptyladleAgeWest;
        public static int HandleEmptyladleServDutyEast, HandleEmptyladleServDutyWest;
        public static string HandleEmptyladleContractorEast, HandleEmptyladleContractorWest;
        public static int HandleTarget=0;
        public static int HandleTargetlast=0;
        public static int HandleEmptyladleAgeEastReal, HandleEmptyladleAgeWestReal;
        public static int ThickAgeThreshold = 10;
        public static string HandThickModeEast, HandThickModeWest;
        public static string InitMeasEmptyInstr0 = "UPDATE measemptyldlinstr SET LadleNo='T088',LadleServDuty=1,LadleAge=1,ModeType=b'0',MeasureStart=0,MeasureStartTime=null,ScaningStart=0,ScaningStartTime=null,ScaningEnd=0,ScaningEndTime=null,TianChePosition=0,msg=null;";
        public static string InitMeasEmptyInstr = "UPDATE measemptyldlinstr SET LadleNo='T088',LadleServDuty=1,LadleAge=1,ModeType=b'0',MeasureStart=0,MeasureStartTime=null,ScaningStart=0,ScaningStartTime=null,ScaningEnd=0,ScaningEndTime=null,msg=null;";
        static bool finishFlag = false;
        //判断超时的标志位
        static bool bMoveBack = false;
        static bool bMoveBackStart = false;
        static bool bMoveBackScan = false;
        static bool bPosMeasDone = false;//是否得到正确的包架位置
        static bool bTimesOut = false;//天车是否到位
        static bool bMoveCrnBlk = false;//是否收到移车成功指令
        static bool bCameraPreReady = false;
        static bool bLinerMeasDone = false;
        public static float TmpOut = 0, HumidThick = 0;
        //测厚超时错误码
        static int TimeoutError = 10;

        //开始天车监控标志位
        bool g_bMonitCrnblk = false;
        //定义测温开始标志位
        bool g_start;

        //开始测厚标志位
        bool bStartThick = false;

        //开始自动测厚标志位
        bool bStartAutoThick = false;
        bool bStartSemiAutoThick = false;
        bool bStartHandleThick = false;

        //开始重包检测标志位
        static bool bMeasHeavy104 = false;                         //开始重包检测标志位
        static bool bMeasHeavy105 = false;                         //开始重包检测标志位

        //空包放下标志位
        static public bool bMeasEmptyDown31 = false;
        static public bool bMeasEmptyDown32 = false;


        public string[] HCurrentNo = new string[7];                //重包当前数组
        public string[] HLastNo = new string[7];                    //重包上次数组
        public string[] ECurrentNo = new string[5];                 //空包当前数组
        public string[] ELastNo = new string[5];                    //空包上次数组

        public static string[] sHeavyLadleNo = new string[2] { "", "" };                              //保存重包号数组
        public static string[] sHeavyLadleNolast = new string[2] { "T999", "T999" };                              //保存重包号数组
        public static string HeavyLadleNo = null;                                                        //保存重包号
        public static string EmptyLadleNo = null;                                                       //保存空包号
        public string[] sEmptyLadleCurrentNo = new string[2] { "", "" };                         //保存空包号数组      //临时调试赋值
        public string[] sEmptyLadleLastNo = new string[2] { "", "" };                         //保存空包号数组      //临时调试赋值
        public static string CrnBlkNo = "";                                                            //吊包天车号
        public static int[] iLadleAge = new int[2] { -1, -1 };
        public static int[] iLadleServDuty = new int[2] { -1, -1 };
        public static string[] iLadleContractor = new string[2] { "未知砌筑商", "未知砌筑商" };

        public static int LastHeavyAge;            //上次重包包龄
        public static int LastEmptyAge;            //上次空包包龄
        public static int HeavyCurr_Age;            //重包包龄
        public static int[] EmptyCurr_Age=new int[2] {-1,-1};            //空包包龄
        public static int[] EmptyCurr_ServDuty = new int[2] { -1, -1 };
        public static string[] EmptyCurr_Contractor = new string[2] { "未知砌筑商", "未知砌筑商" };

        public int HeavyLadleServDuty;       //重包包役
        public int HeavylastLadleAge;         //上次重包包龄
        public int EmptyLadleServDuty;       //空包包役
        public int EmptylastLadleAge;
        public int EmptylastLadleAgeReal;


        /*****************************************************************************************************************************************************************/

        static public int RackPos;                                              //包架位的X坐标
        static public int RealRackPosX=500;                                         //移动天车的目的X坐标
        public const int RealRackPosY = 12670;                                     //移动天车的目的Y坐标   固定值
        public const int SubRackPos = 23138;                                    //两个空包间的距离      固定值
        public const int EastRackPos = 35703;
        public const int WestRackPos = 60680;

        const int EmptyCrnBlk0X = 500;                                       //测厚天车零位X           固定值
        const int EmptyCrnBlk0Y = 5055;                                     //测厚天车零位Y            固定值

        /*****************************************************************************************************************************************************************/

        public int MoveCrbTime1 = 240;                                              //天车移动到定位超时定时
        public int MoveCrbTime3 = 240;                                               //天车移动到零位超时计时

        //天车移动表等待高位天车异常码
        public static int StatusCode;

        private static int ThickHistoryInstruRecordCount = 49;            //每次存入测厚历史记录每个txt的记录条数50条
        /****************************************************************************************************************************************************************/
        private static int DeltaX =2000;                                   //判断天车距离零位的东西方向的允许距离误差，单位为mm
        private static int DeltaY =9000;                                   //判断天车距离零位的南北方向的允许距离误差，单位为mm
        private static byte StandardAnswer = 33;

        private static int tiancherecordFlag = 0, thickrecordFlag = 0,gbgzThickrecordFlag=1,gbgzTemprecordFlag=1, thickRedTemp = 0;


        private static int ThickCirRows = 360;
        private static int ThickCirCols = 720;
        private static int ThickBotRows = 380;
        private static int ThickBotCols = 380;
        Int16[] dataThickCir = new Int16[ThickCirRows * ThickCirCols];
        Int16[] dataThickBot = new Int16[ThickBotRows * ThickBotCols];
        

        //初始化移天车指令表  
        public void MovecrnInit(string ThickHistoryInstru)
        {
            MySqlConnection connMySql;
            try
            {
                //定义一个mysql数据库连接对象
                connMySql = new MySqlConnection(MySQLUser);
                connMySql.Open();     //打开数据库
                string strSQL1 = "update movecrnblkinstr set SendInstrTime=null,MoveType=0,TargetX=0,TargetY=0,ReceInstrTime=null,ReceInstrFlag=0,MoveCrnBlkTime=null,MoveCrnBlkFlag=0;";
                MySqlCommand thisCommand = new MySqlCommand(strSQL1, connMySql);
                thisCommand.ExecuteNonQuery();
                connMySql.Close();
                ThickHistoryInstruSave(ThickHistoryInstru);
            }
            catch (Exception ee)
            {
                MessageBox.Show("清空移天车表操作失败！" + ee.ToString());
            }

        }

        //清空包架位表  
        public void MeasrackInit(string ThickHistoryInstru)
        {
            MySqlConnection connMySql;
            try
            {
                //定义一个mysql数据库连接对象
                connMySql = new MySqlConnection(MySQLUser);
                connMySql.Open();     //打开数据库
                string strSQL = "update measrackposinstr SET SendInstrTime=null,ReceInstrTime=null,ReceInstrFlag=0,FinMeasTime=null,FinMeasFlag=0,RackPos=0,StatusCode=0;";
                MySqlCommand thisCommand = new MySqlCommand(strSQL, connMySql);
                thisCommand.ExecuteNonQuery();
                connMySql.Close();
                ThickHistoryInstruSave(ThickHistoryInstru);
            }
            catch (Exception ee)
            {
                MessageBox.Show("清空包架位表操作失败！" + ee.ToString());
            }
        }

        //测包架位指令函数发送时间~
        public void MeasRackPos()
        {
            if (!finishFlag)
            {
                MySqlConnection connMySql;
                try
                {

                    //定义一个mysql数据库连接对象
                    connMySql = new MySqlConnection(MySQLUser);
                    if (connMySql.State == ConnectionState.Closed)
                    {
                        connMySql.Open();     //打开数据库
                    }
                    string CurrentTimeRackPos = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string strSQL3 = "update  measrackposinstr set SendInstrTime='" + CurrentTimeRackPos + "', ReceInstrTime=null,ReceInstrFlag=0,FinMeasTime=null,FinMeasFlag=0,StatusCode=0;";
                    MySqlCommand thisCommand3 = new MySqlCommand(strSQL3, connMySql);
                    if (!finishFlag)
                    {
                        thisCommand3.ExecuteNonQuery();
                    }
                    connMySql.Close();
                    ThickHistoryInstruSave("update  measrackposinstr set SendInstrTime='" + CurrentTimeRackPos + "', ReceInstrTime=null,ReceInstrFlag=0,FinMeasTime=null,FinMeasFlag=0,StatusCode=0;");
                }
                catch (Exception ee)
                {
                    ThickHistoryInstruSave("测量包架位置失败！");
                }
            }
            

        }

        //读包架位位置数据
        public void ReadRackPos()
        {
            if (!finishFlag)
            {
                MySqlConnection connMySql;
                try
                {
                    //定义一个mysql数据库连接对象
                    connMySql = new MySqlConnection(MySQLUser);
                    connMySql.Open();     //打开数据库               
                    string strSQL = "select RackPos from measrackposinstr where serno=1";
                    MySqlCommand thisCommand = new MySqlCommand(strSQL, connMySql);
                    bPosMeasDone = false;
                    if ((thisCommand.ExecuteScalar() != System.DBNull.Value)&& (thisCommand.ExecuteScalar() != null))
                    {
                        
                        RackPos = Convert.ToInt32(thisCommand.ExecuteScalar());
                        if ((RackPos > 30000) && (RackPos < 60000))
                        {
                            bPosMeasDone = true;
                            string strSQL1 = "update measrackposinstr SET SendInstrTime=null,ReceInstrTime=null,ReceInstrFlag=0,FinMeasTime=null,FinMeasFlag=0,RackPos=0,StatusCode=0;";
                            MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                            MySqlCommand thisCommand1 = new MySqlCommand(strSQL1, connMySql);
                            thisCommand1.ExecuteNonQuery();
                            ThickHistoryInstruSave("已测得包架位置值为" + RackPos + ",且初始化测量包架位表！");
                        }
                    } 
                    connMySql.Close();
                }
                catch (Exception ee)
                {
                    MessageBox.Show("读包架位位置数据操作失败！" + ee.ToString());
                }
            }
            if (!bPosMeasDone)
            {
                ThickHistoryInstruSave("未获得包架位值！");
                g_ListStatus.Add(13);
                ModifyInteract1(HandleTarget, g_ListStatus);
            }
        }

        //移天车指令函数（发启动指令）
        public void Move12tCrnBlkSend(int Movetype,int TargetX,int TargetY)
        {
            if (!finishFlag)
            {
                MySqlConnection connMySql;
                try
                {
                    //定义一个mysql数据库连接对象
                    connMySql = new MySqlConnection(MySQLUser);
                    //connMySql = new MySqlConnection("user id=root;password=wusan;server=localhost;persistsecurityinfo=True;database=work_test");
                    if (connMySql.State == ConnectionState.Closed)
                    {
                        connMySql.Open();     //打开数据库       
                    }
                    string CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string strSQL = "update movecrnblkinstr set SendInstrTime='" + CurrentTime + "', MoveType=" + Movetype + ",TargetX=" + TargetX + ",TargetY=" + TargetY + ",ReceInstrTime=null,ReceInstrFlag=0,MoveCrnBlkTime=null,MoveCrnBlkFlag=0";
                    MySqlCommand thisCommand = new MySqlCommand(strSQL, connMySql);
                    if (!finishFlag) 
                    {
                        thisCommand.ExecuteNonQuery();
                        if (dtGrdViewHianCheInstru.IsHandleCreated)
                        {
                            Action action = () =>
                            {
                                ShowTableTianChe("movecrnblkinstr");
                            };
                            BeginInvoke(action);
                        }
                        ThickHistoryInstruSave("移天车指令表：update movecrnblkinstr set SendInstrTime='" + CurrentTime + "', MoveType=" + Movetype + ",TargetX=" + TargetX + ",TargetY=" + TargetY + ",ReceInstrTime=null,ReceInstrFlag=0,MoveCrnBlkTime=null,MoveCrnBlkFlag=0");
                    }
                    connMySql.Close();
                }
                catch (Exception ee)
                {
                    MessageBox.Show("移天车指令操作失败！" + ee.ToString());
                }
            }
        }
        public  void Move12tCrnBlkOnTheWay(int Movetype, int TargetX, int TargetY)
        {
            if (!finishFlag)
            {
                MySqlConnection connMySql;
                try
                {
                    //定义一个mysql数据库连接对象
                    connMySql = new MySqlConnection(MySQLUser);
                    //connMySql = new MySqlConnection("user id=root;password=wusan;server=localhost;persistsecurityinfo=True;database=work_test");
                    if (connMySql.State == ConnectionState.Closed)
                    {
                        connMySql.Open();     //打开数据库       
                    }
                    string CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string strSQL = "update movecrnblkinstr set SendInstrTime='" + CurrentTime + "', MoveType=" + Movetype + ",TargetX=" + TargetX + ",TargetY=" + TargetY + ",ReceInstrTime=null,ReceInstrFlag=0;";
                    MySqlCommand thisCommand = new MySqlCommand(strSQL, connMySql);
                    if (!finishFlag)
                    {
                        thisCommand.ExecuteNonQuery();
                        if (dtGrdViewHianCheInstru.IsHandleCreated)
                        {
                            Action action = () =>
                            {
                                ShowTableTianChe("movecrnblkinstr");
                            };
                            BeginInvoke(action);
                        }
                        ThickHistoryInstruSave("检测移车指令表：update movecrnblkinstr set SendInstrTime='" + CurrentTime + "', MoveType=" + Movetype + ",TargetX=" + TargetX + ",TargetY=" + TargetY + ",ReceInstrTime=null,ReceInstrFlag=0;");
                    }
                    connMySql.Close();
                }
                catch (Exception ee)
                {
                    MessageBox.Show("天车中途检查状态码时候失败！" + ee.ToString());
                }
            }
        }
        public void Move12tCrnBlkStop(int Movetype, int TargetX, int TargetY)
        {
            MySqlConnection connMySql;
            try
            {
                //定义一个mysql数据库连接对象
                connMySql = new MySqlConnection(MySQLUser);
                //connMySql = new MySqlConnection("user id=root;password=wusan;server=localhost;persistsecurityinfo=True;database=work_test");
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();     //打开数据库       
                }
                string CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string strSQL = "update movecrnblkinstr set SendInstrTime='" + CurrentTime + "', MoveType=" + Movetype + ",TargetX=" + TargetX + ",TargetY=" + TargetY + ",ReceInstrTime=null,ReceInstrFlag=0,MoveCrnBlkTime=null,MoveCrnBlkFlag=0;";
                MySqlCommand thisCommand = new MySqlCommand(strSQL, connMySql);
                thisCommand.ExecuteNonQuery();
                connMySql.Close();
                if (dtGrdViewHianCheInstru.IsHandleCreated)
                {
                    Action action = () =>
                    {
                        ShowTableTianChe("movecrnblkinstr");
                    };
                    BeginInvoke(action);
                }
                ThickHistoryInstruSave("移车指令表：update movecrnblkinstr set SendInstrTime='" + CurrentTime + "', MoveType=" + Movetype + ",TargetX=" + TargetX + ",TargetY=" + TargetY + ",ReceInstrTime=null,ReceInstrFlag=0,MoveCrnBlkTime=null,MoveCrnBlkFlag=0;");
                
            }
            catch (Exception ee)
            {
                MessageBox.Show("移天车指令操作失败！" + ee.ToString());
            }
        }
        //初始化包内衬表
        public  void EmptyLdlLinInit(string instrution,string thickHistoryInstruSave)
        {
            MySqlConnection connMySql;
            try
            {
                //定义一个mysql数据库连接对象
                connMySql = new MySqlConnection(MySQLUser);
                //connMySql = new MySqlConnection("user id=root;password=wusan;server=localhost;persistsecurityinfo=True;database=work_test");
                connMySql.Open();     //打开数据库
                string strSQL1 = instrution;
                MySqlCommand thisCommand = new MySqlCommand(strSQL1, connMySql);
                thisCommand.ExecuteNonQuery();
                connMySql.Close();
                if (dtGrdViewThick.IsHandleCreated)
                {
                    Action action1 = () =>
                    {
                        ShowTableThick("measemptyldlinstr");
                    };
                    BeginInvoke(action1);
                }
                ThickHistoryInstruSave(thickHistoryInstruSave);
            }
            catch (Exception ee)
            {
                ThickHistoryInstruSave("测厚指令表：初始化测厚指令表失败！" + ee.ToString());
            }
        }


        //显示数据表
        private void ShowTable(string TableName)
        {
            try
            {
                MySqlConnection connMySql;
                connMySql = new MySqlConnection(MySQLUser);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();//打开数据库
                }
                string SQL = "select SerNo 序号,SendInstrTime 发指时间,LadleNo 包号,CrnBlkNo 位置,LadleServDuty 包役,LadleAge 包龄,ReceInstrTime 收指时间,ReceInstrFlag 收指标志,FinSendTempTime 完成时间,FinSendTempFlag  完成标志,StatusCode 状态码 from " + TableName + ";";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, TableName);
                dataGridView1.DataSource = ds.Tables[0];
                connMySql.Close();     //关闭数据库    
            }
            catch (Exception ee)
            {
                MessageBox.Show("显示数据表失败！" + ee.ToString());
            }
        }

        //显示数据表
        private void ShowTableTianChe(string TableName)
        {
            try
            {
                MySqlConnection connMySql = new MySqlConnection(MySQLUser);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();//打开数据库
                }
                string SQL = "select SerNo 序号,SendInstrTime 发指时间,MoveType 移动方式,TargetX 目标X,TargetY 目标Y,ReceInstrTime 收指时间,ReceInstrFlag 收指标志,MoveCrnBlkTime 到位时间,MoveCrnBlkFlag 到位标志,RealX 实际X,RealY 实际Y from " + TableName + ";";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, TableName);
                dtGrdViewHianCheInstru.DataSource = ds.Tables[0];
                connMySql.Close();     //关闭数据库    
            }
            catch (Exception ee)
            {
                MessageBox.Show("显示数据表失败！" + ee.ToString());
            }
        }

        //显示数据表
        private void ShowTableThick(string TableName)
        {
            try
            {
                MySqlConnection connMySql = new MySqlConnection(MySQLUser);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();//打开数据库
                }
                string SQL = "select ID 序号,LadleNo 包号,LadleServDuty 包役,LadleAge 包龄,LadleContractor 砌筑商,MeasureStart 开机请求标志,MeasureStartTime 开机请求时间,InstrumentReady 设备准备完成,InstrumentReadyTime 设备准备完成时间,ScaningStart 开始扫描,ScaningStart 开始扫描时间,ScaningEnd 扫描结束,ScaningEnd 扫描结束时间,TianChePosition 天车位置 from " + TableName + ";";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, TableName);
                dtGrdViewThick.DataSource = ds.Tables[0];
                connMySql.Close();     //关闭数据库    
            }
            catch (Exception ee)
            {
                MessageBox.Show("显示数据表失败！" + ee.ToString());
            }
        }


        //复制表中空包包号至数组 
        private void CopyEmptyLadleNo()
        {

            MySqlConnection connMySql;
            connMySql = new MySqlConnection(MySQLUser);
            try
            {
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }
                string SQL = "SELECT * FROM tbgz_view_ak_hctc2 ORDER BY ID";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "tbgz_view_ak_hctc2");
                ECurrentNo[0] = ds.Tables[0].Rows[3][11].ToString();//111天车的包
                ECurrentNo[1] = ds.Tables[0].Rows[4][11].ToString();//112天车的包
                ECurrentNo[2] = ds.Tables[0].Rows[6][11].ToString();//127天车的包
                ECurrentNo[3] = ds.Tables[0].Rows[15][11].ToString();//钢三线1#车东包位（先测）
                ECurrentNo[4] = ds.Tables[0].Rows[16][11].ToString();//钢三线2#车西包位（后测）

                string SQL2 = "select mode,operation,ladlenoEast,ladleageEast,ladlenoWest,ladleageWest,target,headheart from  sysinteractinstr_copy1 where id =1;";
                MySqlDataAdapter objDataAdpter2 = new MySqlDataAdapter();
                objDataAdpter2.SelectCommand = new MySqlCommand(SQL2, connMySql);
                DataSet ds2 = new DataSet();
                objDataAdpter2.Fill(ds2, "sysinteractinstr_copy1");
                if (ds2.Tables[0].Rows.Count > 0)
                {
                    int mode_finish_temp = Convert.ToInt32(ds2.Tables[0].Rows[0][0]);
                    int operation_temp = Convert.ToInt32(ds2.Tables[0].Rows[0][1]);
                    HandleEmptyladlenoEast = ds2.Tables[0].Rows[0][2].ToString();
                    int HandleEmptyladleAgeEast_temp = Convert.ToInt32(ds2.Tables[0].Rows[0][3]);
                    HandleEmptyladlenoWest = ds2.Tables[0].Rows[0][4].ToString();
                    int HandleEmptyladleAgeWest_temp = Convert.ToInt32(ds2.Tables[0].Rows[0][5]);
                    HandleTarget = Convert.ToInt32(ds2.Tables[0].Rows[0][6]);
                    int headheart_temp = Convert.ToInt32(ds2.Tables[0].Rows[0][7]);
                    if (mode_finish_temp != 3)
                    {
                        if (HandleEmptyladleAgeEast_temp == 0)
                        {
                            HandleEmptyladleAgeEast = 0;
                            HandleEmptyladleAgeEastReal = ReadCurr_Age2(HandleEmptyladlenoEast);
                            if (HandleEmptyladleAgeEastReal >= ThickAgeThreshold)
                            {
                                HandThickModeEast = "b'1'";
                            }
                            else
                            {
                                HandThickModeEast = "b'0'";
                            }
                            //
                            UpdateCurr_ModeType(HandleEmptyladlenoEast, HandThickModeEast);
                        }
                        if (HandleEmptyladleAgeEast_temp == -1)
                        {
                            HandleEmptyladleAgeEast = ReadCurr_Age2(HandleEmptyladlenoEast);
                            HandThickModeEast = "b" + "'" + ReadCurr_ModeType(HandleEmptyladlenoEast).ToString() + "'";
                        }
                        HandleEmptyladleServDutyEast = ReadCurrLadleServDuty(HandleEmptyladlenoEast);
                        HandleEmptyladleContractorEast = ReadCurrLadleContractor(HandleEmptyladlenoEast);
                        if (HandleEmptyladleAgeWest_temp == 0)
                        {
                            HandleEmptyladleAgeWest = 0;
                            HandleEmptyladleAgeWestReal = ReadCurr_Age2(HandleEmptyladlenoWest);
                            if (HandleEmptyladleAgeWestReal >= ThickAgeThreshold)
                            {
                                HandThickModeWest = "b'1'";
                            }
                            else
                            {
                                HandThickModeWest = "b'0'";
                            }
                            UpdateCurr_ModeType(HandleEmptyladlenoWest, HandThickModeWest);
                        }
                        if (HandleEmptyladleAgeWest_temp == -1)
                        {
                            HandleEmptyladleAgeWest = ReadCurr_Age2(HandleEmptyladlenoWest);
                            HandThickModeWest = "b" + "'" + ReadCurr_ModeType(HandleEmptyladlenoWest).ToString() + "'";
                        }
                        HandleEmptyladleServDutyWest = ReadCurrLadleServDuty(HandleEmptyladlenoWest);
                        HandleEmptyladleContractorWest = ReadCurrLadleContractor(HandleEmptyladlenoWest);
                    }
                    if (operation_temp == 10)
                    {
                        finishFlag = true;
                        //将系统交互数采指令表中operation置为0
                        //SysInstrInit("将数采交互表初始化");
                    }
                    else
                    {
                        finishFlag = false;
                    }
                    g_operation = operation_temp;

                    if (mode_finish_temp == 0)
                    {
                        bStartAutoThick = true;
                        bStartSemiAutoThick = false;
                        bStartHandleThick = false;
                    }
                    else if (mode_finish_temp == 1)
                    {
                        bStartAutoThick = false;
                        bStartSemiAutoThick = true;
                        bStartHandleThick = false;
                    }
                    else if (mode_finish_temp == 2)
                    {
                        bStartAutoThick = false;
                        bStartSemiAutoThick = false;
                        bStartHandleThick = true;
                    }
                    else if (mode_finish_temp == 3)
                    {
                        bStartAutoThick = false;
                        bStartSemiAutoThick = false;
                        bStartHandleThick = false;
                    }
                    g_mode = mode_finish_temp;
                }

                
                int Status12t_1 = 0, Status12t_2 = 0, Status12t_3 = 0, Status12t_4 = 0, Status12t_5 = 0, Status12t_6 = 0, Status12t_7 = 0, Status12t_8 = 0, Status12t_9 = 0, Status12t_10 = 0, Status12t_11 = 0, Status12t_12 = 0, Status12t_13 = 0, Status12t_14 = 0, Status12t_15 = 0;
                int StatusCode = 0, tiancheheart = 0;
                try
                {
                    //定义一个mysql数据库连接对象
                    string SQL3 = "select StatusCode,Heart_PLC from crnblkcurpos where id=1;";
                    MySqlDataAdapter objDataAdpter3 = new MySqlDataAdapter();
                    objDataAdpter3.SelectCommand = new MySqlCommand(SQL3, connMySql);
                    DataSet ds3 = new DataSet();
                    objDataAdpter3.Fill(ds3, "crnblkcurpos");
                    if (ds3.Tables[0].Rows.Count > 0)
                    {
                        if (ds3.Tables[0].Rows[0][0] != DBNull.Value)
                        {
                            StatusCode = Convert.ToInt32(ds3.Tables[0].Rows[0][0]);
                        }
                        if (ds3.Tables[0].Rows[0][1] != DBNull.Value)
                        {
                            tiancheHeartNow = Convert.ToInt32(ds3.Tables[0].Rows[0][1]);
                            if (tiancheHeartNow == tiancheHeartLast)
                            {
                                tiancheheart = 1;
                                if (tiancherecordFlag == 0)
                                {
                                    ThickHistoryInstruSave("天车PLC通讯断开！");
                                }
                                tiancherecordFlag = 1;
                            }
                            if (tiancheHeartNow != tiancheHeartLast)
                            {
                                if (tiancherecordFlag == 1)
                                {
                                    ThickHistoryInstruSave("天车PLC通讯恢复！");
                                }
                                tiancherecordFlag = 0;
                            }
                            tiancheHeartLast = tiancheHeartNow;
                        }
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show("在天车非运行状态下读天车移动表的心跳和状态失败！" + ee.ToString());
                }
                string Status12tAll = Convert.ToString(StatusCode, 2);
                for (int i = Status12tAll.Length - 1; i >= 0; i--)
                {
                    //0位
                    if (i == Status12tAll.Length - 1)
                    {
                        if (String.Equals(Status12tAll.Substring(i,1),"1"))
                        {
                            //status
                            Status12t_1 = 1;
                        }
                    }
                    //1位
                    if (i == Status12tAll.Length - 2)
                    {
                        if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                        {
                            Status12t_2 = 1;
                        }
                    }
                    //2位
                    if (i == Status12tAll.Length - 3)
                    {
                        if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                        {
                            Status12t_3 = 1;
                        }
                    }
                    if (i == Status12tAll.Length - 4)
                    {
                        if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                        {
                            Status12t_4 = 1;
                        }
                    }
                    if (i == Status12tAll.Length - 5)
                    {
                        if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                        {
                            Status12t_5 = 1;
                        }
                    }
                    if (i == Status12tAll.Length - 6)
                    {
                        if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                        {
                            Status12t_6 = 1;
                        }
                    }
                    if (i == Status12tAll.Length - 7)
                    {
                        if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                        {
                            Status12t_7 = 1;
                        }
                    }
                    if (i == Status12tAll.Length - 8)
                    {
                        if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                        {
                            Status12t_8 = 1;
                        }
                    }
                    if (i == Status12tAll.Length - 9)
                    {
                        if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                        {
                            Status12t_9 = 1;
                        }
                    }
                    if (i == Status12tAll.Length - 10)
                    {
                        if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                        {
                            Status12t_10 = 1;
                        }
                    }
                    if (i == Status12tAll.Length - 11)
                    {
                        if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                        {
                            Status12t_11 = 1;
                        }
                    }
                    if (i == Status12tAll.Length - 12)
                    {
                        if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                        {
                            Status12t_12 = 1;
                        }
                    }
                    if (i == Status12tAll.Length - 13)
                    {
                        if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                        {
                            Status12t_13 = 1;
                        }
                    }
                    if (i == Status12tAll.Length - 14)
                    {
                        if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                        {
                            Status12t_14 = 1;
                        }
                    }
                    if (i == Status12tAll.Length - 15)
                    {
                        if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                        {
                            Status12t_15 = 1;
                        }
                    }
                }
               
                int thickheartstatus = 0, thickwaring = 0, thickTmpOut = 0, thickHumid = 0;
                //bMoveBack = false;
                string SQL1 = "select Heartbeat,TemperatureIn,TemperatureOut,TemperatureInHum,Warning from  measheartbeat where ID =1;";
                MySqlDataAdapter objDataAdpter1 = new MySqlDataAdapter();
                objDataAdpter1.SelectCommand = new MySqlCommand(SQL1, connMySql);
                DataSet ds1 = new DataSet();
                objDataAdpter1.Fill(ds1, "measheartbeat");
                if (ds1.Tables[0].Rows.Count > 0)
                {
                    thickHeartNow = Convert.ToInt32(ds1.Tables[0].Rows[0][0]);
                    TmpOut = Convert.ToSingle(ds1.Tables[0].Rows[0][2]);
                    HumidThick = Convert.ToSingle(ds1.Tables[0].Rows[0][3]);
                    int Warning_temp = Convert.ToInt32(ds1.Tables[0].Rows[0][4]);
                    if (thickHeartNow == thickHeartLast)
                    {
                        thickheartstatus = 1;
                        if (thickrecordFlag == 0)
                        {
                            ThickHistoryInstruSave("测厚设备通讯断开！");
                        }
                        thickrecordFlag = 1;

                    }
                    if (thickHeartNow != thickHeartLast)
                    {
                        if (thickrecordFlag == 1)
                        {
                            ThickHistoryInstruSave("测厚设备通讯恢复！");
                        }
                        thickrecordFlag = 0;
                    }
                    thickHeartLast = thickHeartNow;
                    if (TmpOut >= 100)
                    {
                        thickTmpOut = 1;
                    }
                    if (HumidThick > 50)
                    {
                        thickHumid = 1;
                    }
                    g_warning = Warning_temp;
                    switch (Warning_temp)
                    {
                        case 0:
                            {
                                bMoveBack = false;
                                if (thickRedTemp == 1)
                                {
                                    ThickHistoryInstruSave("测厚设备解除高温报警！");
                                    thickRedTemp = 0;
                                }
                            }
                            break;
                        case 1:
                            {
                                thickwaring = 1;
                                bMoveBack = true;
                                if (thickRedTemp == 0)
                                {
                                    ThickHistoryInstruSave("测厚设备高温报警！读取空包包号，判断状态线程！");
                                    thickRedTemp = 1;
                                }
                                
                            }
                            break;
                        case 2:
                            {
                                thickwaring = 2;
                                bMoveBack = false;
                                if (thickRedTemp == 1)
                                {
                                    ThickHistoryInstruSave("测厚设备解除高温报警！");
                                    thickRedTemp = 0;
                                }
                            }
                            break;
                        case 3:
                            {
                                thickwaring = 3;
                                bMoveBack = false;
                                if (thickRedTemp == 1)
                                {
                                    ThickHistoryInstruSave("测厚设备解除高温报警！");
                                    thickRedTemp = 0;
                                }
                            }
                            break;
                        case 4:
                            {
                                thickwaring = 4;
                                bMoveBack = false;
                                if (thickRedTemp == 1)
                                {
                                    ThickHistoryInstruSave("测厚设备解除高温报警！");
                                    thickRedTemp = 0;
                                }
                            }
                            break;
                        case 5:
                            {
                                thickwaring = 5;
                                bMoveBack = false;
                                if (thickRedTemp == 1)
                                {
                                    ThickHistoryInstruSave("测厚设备解除高温报警！");
                                    thickRedTemp = 0;
                                }
                            }
                            break;
                    }
                }
                string SQL5 = "update sysinteractinstr_copy1 SET tianchestatus01=" + Status12t_1 + ",tianchestatus02=" + Status12t_2 + ",tianchestatus03=" + Status12t_3 + ",tianchestatus04=" + Status12t_4 + ",tianchestatus05=" + Status12t_5 + ",tianchestatus06=" + Status12t_6 + ",tianchestatus07=" + Status12t_7 + ",tianchestatus08=" + Status12t_8 + ",tianchestatus11=" + Status12t_11 + ",tianchestatus12=" + Status12t_12 + ",tiancheheart=" + tiancheheart + ",thickheartstatus=" + thickheartstatus + ",thickTmpOut=" + TmpOut + ",thickHumid=" + HumidThick + ",thickwarning=" + thickwaring + " WHERE id=1;";
                MySqlCommand thisCommand1 = new MySqlCommand(SQL5, connMySql);
                thisCommand1.ExecuteNonQuery();
                connMySql.Close();
            }
            catch (Exception ee)
            {
                MessageBox.Show("读取空包包号，操作模式，测厚状态出错！" + ee.ToString());
            }
            //读完一次数采交互指令，是不是应该初始化，害怕自动、半自动、手动线程一直循环，将
        }

        

        //判断空包是否落下
        private void EmptyLadleDown()
        {
            MySqlConnection connMySql;

            for (int i = 0; i < 3; i++)             //0 1 2
            {
                if (ECurrentNo[i] != "")     //判断是否有包号消失
                {
                    if (ECurrentNo[i] != ELastNo[i])
                    {
                        ELastNo[i] = ECurrentNo[i];
                    }
                }
            }
            if(ECurrentNo[3] != "")
            {
                if ((ECurrentNo[3] == ELastNo[0]) || (ECurrentNo[3] == ELastNo[1]) || (ECurrentNo[3] == ELastNo[2]))
                {
                    bMeasEmptyDown31 = true;
                    g_target = 1;
                    sEmptyLadleCurrentNo[0] = ECurrentNo[3];            //存包号
                    EmptyCurr_Age[0] = ReadCurr_Age2(sEmptyLadleCurrentNo[0]);
                    EmptyCurr_ServDuty[0] = ReadCurrLadleServDuty(sEmptyLadleCurrentNo[0]);
                    EmptyCurr_Contractor[0] = ReadCurrLadleContractor(sEmptyLadleCurrentNo[0]);
                }
                
            }
            if (ECurrentNo[4] != "")            //判断消失的包号是否出现
            {
                if ((ECurrentNo[4] == ELastNo[0]) || (ECurrentNo[4] == ELastNo[1]) || (ECurrentNo[4] == ELastNo[2]))
                {
                    bMeasEmptyDown32 = true;
                    g_target = 2;
                    sEmptyLadleCurrentNo[1] = ECurrentNo[4];            //存包号
                    EmptyCurr_Age[1] = ReadCurr_Age2(sEmptyLadleCurrentNo[1]);
                    EmptyCurr_ServDuty[1] = ReadCurrLadleServDuty(sEmptyLadleCurrentNo[1]);
                    EmptyCurr_Contractor[1] = ReadCurrLadleContractor(sEmptyLadleCurrentNo[1]);
                } 
            }
            
            
            if ((bMeasEmptyDown31 == true) || (bMeasEmptyDown32 == true))
            {
                try
                {
                    connMySql = new MySqlConnection(MySQLUser);
                    if (connMySql.State == ConnectionState.Closed)
                    {
                        connMySql.Open();
                    }
                    if (sEmptyLadleCurrentNo[0] != sEmptyLadleLastNo[0])
                    {
                        string strSQL1 = "update emptyldl_tomeas set LadleNo='" + sEmptyLadleCurrentNo[0] + "',LadleAge =" + EmptyCurr_Age[0] + " where SerNo = 1;";
                        MySqlCommand thisCommand = new MySqlCommand(strSQL1, connMySql);
                        thisCommand.ExecuteNonQuery();
                        ThickHistoryInstruSave("待测空包表：update emptyldl_tomeas set LadleNo='" + sEmptyLadleCurrentNo[0] + "',LadleAge =" + EmptyCurr_Age[0] + " where SerNo = 1;");
                        sEmptyLadleLastNo[0] = sEmptyLadleCurrentNo[0];
                    }
                    if (sEmptyLadleCurrentNo[1] != sEmptyLadleLastNo[1])
                    {
                        string strSQL1 = "update emptyldl_tomeas set LadleNo='" + sEmptyLadleCurrentNo[1] + "',LadleAge =" + EmptyCurr_Age[1] + " where SerNo = 2;";
                        MySqlCommand thisCommand = new MySqlCommand(strSQL1, connMySql);
                        thisCommand.ExecuteNonQuery();
                        ThickHistoryInstruSave("待测空包表：update emptyldl_tomeas set LadleNo='" + sEmptyLadleCurrentNo[1] + "',LadleAge =" + EmptyCurr_Age[1] + " where SerNo = 2;");
                        sEmptyLadleLastNo[1] = sEmptyLadleCurrentNo[1];
                    }
                    connMySql.Close();
                }
                catch (Exception ee)
                {
                    MessageBox.Show("更新待测空包表失败！" + ee.ToString());
                }
            }
            if (ECurrentNo[3] == "")
            {
                bMeasEmptyDown31 = false;
            }

            if (ECurrentNo[4] == "")
            {
                bMeasEmptyDown32 = false;
            }
        }

       



        //读取已测空包包龄
        private int ReadCurr_Age2(string no)
        {
            OracleConnection connOracle;
            try
            {
                connOracle = new OracleConnection(OracleUser);
                if (connOracle.State == ConnectionState.Closed)
                {
                    connOracle.Open();
                }

                string SQL = "SELECT  \"LADLE_AGE\"  FROM LADLE_AGE  where \"LADLE_ID\"  ='" + no + "'";
                OracleCommand thisCommand = new OracleCommand(SQL, connOracle);
                int iAge = Convert.ToInt32(thisCommand.ExecuteScalar());
                connOracle.Close();
                return iAge;
            }
            catch (Exception ee)
            {
                //MessageBox.Show("读取已测空包包龄失败" + ee.ToString());
                return -1;
            }
        }
            
        private int ReadCurrLadleServDuty(string no)
        {
            MySqlConnection connMysql;
            try
            {
                int lalSerDy=0;
                connMysql = new MySqlConnection(MySQLUser);
                if (connMysql.State == ConnectionState.Closed)
                {
                    connMysql.Open();
                }

                string SQL = "select LadleServDuty from ladleservduty where LadleNo='"+ no + "';";
                MySqlCommand thisCommand = new MySqlCommand(SQL, connMysql);
                if ((thisCommand.ExecuteScalar() == null)||(thisCommand.ExecuteScalar() == DBNull.Value))
                {
                    lalSerDy = -1;
                }
                else
                {
                    lalSerDy = Convert.ToInt32(thisCommand.ExecuteScalar());
                }
                connMysql.Close();
                return lalSerDy;
            }
            catch (Exception ee)
            {
                //MessageBox.Show("读取已测重包包龄失败" + ee.ToString());
                return -1;
            }
        }

        private string ReadCurrLadleContractor(string no)
        {
            OracleConnection connOracle;
            string iContractor = "未知砌筑商";
            try
            {
                connOracle = new OracleConnection(OracleUser);
                if (connOracle.State == ConnectionState.Closed)
                {
                    connOracle.Open();
                }
                string SQL = "SELECT  \"COMPANY\"  FROM T_LADLE_SERVICE_INFO  where \"LADLE_NO\"  ='" + no + "'";
                OracleCommand thisCommand = new OracleCommand(SQL, connOracle);
                if (thisCommand.ExecuteScalar() != DBNull.Value)
                {
                    iContractor = (string)thisCommand.ExecuteScalar();
                }
                connOracle.Close();
                return iContractor;
            }
            catch (Exception ee)
            {
                return iContractor;
            }
        }
        //读取测空包
        private int ReadCurr_ModeType(string no)
        {
            MySqlConnection connMySql;
            connMySql = new MySqlConnection(MySQLUser);
            try
            {
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }
                string SQL = "SELECT  ThickMode  FROM ladleservduty  where LadleNo  ='" + no + "';";
                MySqlCommand thisCommand = new MySqlCommand(SQL, connMySql);
                int ThickMode = Convert.ToInt32(thisCommand.ExecuteScalar());
                connMySql.Close();
                return ThickMode;
            }
            catch (Exception ee)
            {
                ThickHistoryInstruSave("ladleservduty:读取测厚模式失败！");
                return -1;
            }
        }

        private void UpdateCurr_ModeType(string no,string ThickMode)
        {
            MySqlConnection connMySql;
            connMySql = new MySqlConnection(MySQLUser);
            try
            {
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }
                string SQL = "update  ladleservduty  set ThickMode="+ThickMode+" where LadleNo  ='" + no + "';";
                MySqlCommand thisCommand = new MySqlCommand(SQL, connMySql);
                thisCommand.ExecuteNonQuery();
                connMySql.Close();
            }
            catch (Exception ee)
            {
                ThickHistoryInstruSave("ladleservduty:更新测厚模式失败！");
            }
        }
        public string heavyladletomeas;
        public int heavyladleagetomeas;

        //更新待测重包表


        public string emptyladletomeas;
        public int emptyladleagetomeas;


        private void EmptyLadleInit(string ThickHistoryInstru)
        {
            MySqlConnection connMySql;
            connMySql = new MySqlConnection(MySQLUser);
            try
            {
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }
                string SQL = "SELECT * FROM tbgz_view_ak_hctc2 ORDER BY ID";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "tbgz_view_ak_hctc2");
                connMySql.Close();
                ELastNo[0] = ds.Tables[0].Rows[3][11].ToString();//111天车的包
                ELastNo[1] = ds.Tables[0].Rows[4][11].ToString();//112天车的包
                ELastNo[2] = ds.Tables[0].Rows[6][11].ToString();//127天车的包
                ELastNo[3] = ds.Tables[0].Rows[15][11].ToString();//钢三线1#车东包位（先测）
                ELastNo[4] = ds.Tables[0].Rows[16][11].ToString();//钢三线2#车西包位（后测）
                ThickHistoryInstruSave(ThickHistoryInstru);
            }
            catch (Exception ee)
            {
                MessageBox.Show("得到旧包号值出错！" + ee.ToString());
            }

        }
        

        //按键开始测厚
        private void StartLine1_Click(object sender, EventArgs e)
        {
            bStartThick = true;
            StartLine1.Enabled = false;
            EndLine1.Enabled = true;
            timertxtBxShow.Enabled = true;
            //清空系统数采交互表
            SysInstrInit("将数采交互表初始化");
            //清空空包内衬指令表
            EmptyLdlLinInit("UPDATE measemptyldlinstr SET MeasureStart=0,MeasureStartTime=null,/*InstrumentReady=0,InstrumentReadyTime=null,*/ScaningStart=0,ScaningStartTime=null,ScaningEnd=0,ScaningEndTime=null,TianChePosition=0,msg=null;","将测量空包指令表初始化");
            //清空包架位表
            MeasrackInit("已清空包架位表");
            //初始化移天车表
            MovecrnInit("已经初始化移位天车指令表");
            if (dtGrdViewHianCheInstru.IsHandleCreated)
            {
                Action action = () =>
                {
                    ShowTableTianChe("movecrnblkinstr");
                };
                BeginInvoke(action);
            }
            //清空厚度数据临时表
            //cleantmpthick();

            EmptyLadleInit("已经获得初始的空包包号");
            //开始线程进行数据表复制读取，判断空包位落下加上置东西包标志位,以及测温模式还有前端停止按钮
            //此线程需要慢一点（？？放一个定时器？？？）
            Thread meas_ctrl_thd_emldlP = new Thread(meas_ctremldlP);
            meas_ctrl_thd_emldlP.Start();
            //线程：循环检测手动，半自动，自动并与天车测厚交互
            Thread meas_ctrl_thd_All = new Thread(meas_ctrALL);
            meas_ctrl_thd_All.Start();
            
        }

        //开始线程进行数据表复制读取，判断空包位落下加上置东西包标志位
        private void meas_ctremldlP()
        {

            while (bStartThick)
            {

                //复制SQL server钢包跟踪表至测厚相关MySQL钢包跟踪表

                //Copy_tbgz_view_ak_hctc();
                copy_tbgz_view_ak_hctc();
                //ThickHistoryInstruSave("成功复制铁包运营视图");
                CopyEmptyLadleNo();
                //从模式进行信息筛选，读空包相关包号，以及判断前端是否点击停止按钮
                //EmptyLadleDown();
                Thread.Sleep(5000);

            }
        }
        List<byte> g_ListStatus = new List<byte>();
        //测厚总线程
        private bool IsZeroPosiyion(int X,int Y) 
        {
            MySqlConnection connMySql;
            connMySql = new MySqlConnection(MySQLUser);
            try
            {
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }
                string SQL = "select CurrentX,CurrentY From crnblkcurpos where id=1;";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "crnblkcurpos");
                connMySql.Close();
                int currentX = Convert.ToInt32(ds.Tables[0].Rows[0][0]);//X值
                int currentY = Convert.ToInt32(ds.Tables[0].Rows[0][1]);//Y值
                if ((Math.Abs(EmptyCrnBlk0X - currentX) < X) && (Math.Abs(EmptyCrnBlk0Y - currentY) < Y)) 
                {
                    return true;
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("得到旧包号值出错！" + ee.ToString());
            }
            return false;

        }
        void meas_ctrALL()
        {
            try
            {
                while (bStartThick)
                {
                    //初始化移天车表
                    //MovecrnInit();
                    //自动线程：无需与前端交互
                    while (bStartAutoThick && (!finishFlag))
                    {
                        All_operation = 0;
                        switch (g_operation)
                        {
                            case 0:
                                {
                                    /*if (finishFlag && !bMoveBack)
                                    {
                                        //检查与零位位置
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        SysInstrInit("前端按下停止键！");
                                    }
                                    if (!finishFlag && bMoveBack) 
                                    {
                                        g_ListStatus.Clear();
                                        bMoveBack = false;
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        g_ListStatus.Add(18);
                                        ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                        ThickHistoryInstruSave("测厚设备高温报警！");
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //清空数采交互表
                                        SysInstrInit("将数采交互表初始化");
                                    }*/
                                }
                                break;
                            case 71:
                                {
                                    g_operation = 0;
                                    g_ListStatus.Clear();
                                    g_ListStatus.Add(71);
                                    ModifyInteract1_Mode(3,0, HandleTarget, g_ListStatus);
                                    //读包架位置
                                    MeasRackPos();
                                    Thread.Sleep(2000);
                                    ReadRackPos();
                                    //东包位
                                    RealRackPosX = EastRackPos - (RackPos / 10);
                                    if (bPosMeasDone)
                                    {
                                        bPosMeasDone = false;
                                        if (!finishFlag)
                                        {
                                            MeasEmptyLdlLin("MeasureStart", "MeasureStartTime", HandleEmptyladlenoEast, HandleEmptyladleServDutyEast, HandleEmptyladleAgeEast, HandleEmptyladleContractorEast, HandThickModeEast,1);
                                        }
                                        Thread.Sleep(10000);
                                        //检测测厚指令表中ID=2的status中是否置1
                                        CheckMeasThickStatus(130, "InstrumentReady", "InstrumentReadyTime", 1);
                                    }
                                    //移动天车指令表
                                    if ((!finishFlag) && bCameraPreReady)
                                    {
                                        bCameraPreReady = false;
                                        All_operation = 7;
                                        g_ListStatus.Add(7);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(1, RealRackPosX, RealRackPosY);
                                        //检查到位信息
                                        Thread.Sleep(5000);
                                        Check12tArrive();
                                        Move12tCrnBlkStop(0, RealRackPosX, RealRackPosY);
                                    }
                                    //将系统交互数采指令表中operation置为0
                                    //ModifyInteract1(0,g_target,0);
                                    //拍照测厚部分
                                    //ModifyInteract1(0, HandleTarget, 0);
                                    if (bMoveCrnBlk && (!finishFlag) && (bPosMeasDone))
                                    {
                                        bMoveCrnBlk = false;
                                        g_ListStatus.Add(8);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        All_operation = 8;
                                        MeasureThick();

                                        //ModifyInteract1(0, HandleTarget, 0);
                                        //天车回零
                                        //将系统交互数采指令表中operation置为0
                                        //ModifyInteract1(0, g_target, 9);


                                        //移动天车指令表
                                        if (bLinerMeasDone && (!finishFlag))
                                        {
                                            bLinerMeasDone = false;
                                            All_operation = 9;
                                            g_ListStatus.Add(9);
                                            ModifyInteract1(HandleTarget, g_ListStatus);
                                            Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                            //检查到位信息
                                            Thread.Sleep(2000);
                                            Check12tBack();
                                            if (bMoveCrnBlk)
                                            {
                                                bMoveCrnBlk = false;
                                                MeasEmptyLdlLinTianChePosi();
                                            }
                                            Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        }
                                        //将系统交互数采指令表中operation置为0
                                        //ModifyInteract1(0, g_target, 0);
                                        //ModifyInteract1(0, HandleTarget, 0);
                                    }
                                    if ((!bPosMeasDone) && (!bMoveCrnBlk) && (!finishFlag))
                                    {
                                        //检查与零位位置
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空,且位置为0");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空，且位置为1");
                                        }
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                    }
                                    if (bTimesOut && (!bMoveCrnBlk) && (!finishFlag))
                                    {

                                        bTimesOut = false;
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空,且位置为0");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空，且位置为1");
                                        }
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                    }
                                    
                                    //测温温度高报警
                                    if (!finishFlag && (bMoveBackStart||bMoveBackScan))
                                    {
                                        bMoveBackStart = false;
                                        bMoveBackScan = false;
                                        g_ListStatus.Add(18);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        ThickHistoryInstruSave("测厚设备高温报警！");
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                    }
                                    if (finishFlag && !bMoveBack)
                                    {
                                        //检查与零位位置
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        g_ListStatus.Add(17);
                                        ModifyInteract1(HandleTarget, g_ListStatus);

                                    }
                                    //SysInstrInit("将数采交互表初始化");
                                    g_ListStatus.Add(StandardAnswer);
                                    ModifyInteract1_Operate(0,HandleTarget, g_ListStatus);
                                    g_ListStatus.Clear();
                                    
                                }
                                break;
                            case 72:
                                {
                                    g_operation = 0;
                                    //将系统交互数采指令表中operation置为0
                                    //ModifyInteract1(0, g_target, 7);
                                    g_ListStatus.Clear();
                                    g_ListStatus.Add(72);
                                    ModifyInteract1_Mode(3, 0, HandleTarget, g_ListStatus);
                                    //读包架位置
                                    MeasRackPos();
                                    Thread.Sleep(2000);
                                    ReadRackPos();
                                    //东包位
                                    RealRackPosX = WestRackPos - RackPos / 10;
                                    if (bPosMeasDone)
                                    {
                                        bPosMeasDone = false;
                                        if (!finishFlag)
                                        {
                                            MeasEmptyLdlLin("MeasureStart", "MeasureStartTime", HandleEmptyladlenoWest, HandleEmptyladleServDutyWest, HandleEmptyladleAgeWest, HandleEmptyladleContractorWest, HandThickModeWest, 1);
                                        }
                                        Thread.Sleep(10000);
                                        //检测测厚指令表中ID=2的status中是否置1
                                        CheckMeasThickStatus(130, "InstrumentReady", "InstrumentReadyTime", 1);
                                    }
                                    //移动天车指令表
                                    if ((!finishFlag) && bCameraPreReady)
                                    {
                                        bCameraPreReady = false;
                                        All_operation = 7;
                                        g_ListStatus.Add(7);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(1, RealRackPosX, RealRackPosY);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tArrive();
                                        Move12tCrnBlkStop(0, RealRackPosX, RealRackPosY);
                                    }
                                    //将系统交互数采指令表中operation置为0
                                    //ModifyInteract1(0, g_target, 0);
                                    //ModifyInteract1(0, HandleTarget, 0);

                                    //拍照测厚
                                    if (bMoveCrnBlk && (!finishFlag) && (bPosMeasDone))
                                    {
                                        bMoveCrnBlk = false;
                                        All_operation = 8;
                                        g_ListStatus.Add(8);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        MeasureThick();
                                        //ModifyInteract1(0, HandleTarget, 0);
                                        //ModifyInteract1(0, g_target, 9);


                                        //移动天车指令表
                                        if ((!finishFlag) && bLinerMeasDone)
                                        {
                                            bLinerMeasDone = false;
                                            All_operation = 9;
                                            g_ListStatus.Add(9);
                                            ModifyInteract1(HandleTarget, g_ListStatus);
                                            Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                            //检查到位信息
                                            Thread.Sleep(2000);
                                            Check12tBack();
                                            if (bMoveCrnBlk)
                                            {
                                                bMoveCrnBlk = false;
                                                MeasEmptyLdlLinTianChePosi();
                                            }
                                            Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        }
                                        //将系统交互数采指令表中operation置为0
                                        //ModifyInteract1(0, g_target, 0);
                                        //ModifyInteract1(0, HandleTarget, 0);
                                    }
                                    if ((!bPosMeasDone) && (!bMoveCrnBlk) && (!finishFlag))
                                    {
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空,且位置为0");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空，且位置为1");
                                        }
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                    }
                                    if (bTimesOut && (!bMoveCrnBlk) && (!finishFlag))
                                    {
                                        bTimesOut = false;
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空,且位置为0");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空，且位置为1");
                                        }
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                    }
                                    //ModifyInteract1(0, HandleTarget, 0);
                                    
                                    if (!finishFlag && (bMoveBackStart || bMoveBackScan))
                                    {
                                        bMoveBackStart = false;
                                        bMoveBackScan = false;
                                        g_ListStatus.Add(18);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        ThickHistoryInstruSave("测厚设备高温报警！");
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空,且位置为0");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空，且位置为1");
                                        }
                                    }
                                    if (finishFlag && !bMoveBack)
                                    {
                                        //检查与零位位置
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        g_ListStatus.Add(17);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                    }
                                    //SysInstrInit("将数采交互表初始化");
                                    g_ListStatus.Add(StandardAnswer);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    g_ListStatus.Clear();
                                }
                                break;
                            case 73:
                                {
                                    g_operation = 0;
                                    g_ListStatus.Clear();
                                    g_ListStatus.Add(73);
                                    ModifyInteract1_Mode(3, 0, HandleTarget, g_ListStatus);
                                    //读包架位置
                                    MeasRackPos();
                                    Thread.Sleep(2000);
                                    ReadRackPos();
                                    //东包位
                                    RealRackPosX = EastRackPos - (RackPos / 10);

                                    if (bPosMeasDone)
                                    {
                                        bPosMeasDone = false;
                                        if (!finishFlag)
                                        {
                                            MeasEmptyLdlLin("MeasureStart", "MeasureStartTime", HandleEmptyladlenoEast, HandleEmptyladleServDutyEast, HandleEmptyladleAgeEast, HandleEmptyladleContractorEast, HandThickModeEast, 1);
                                        }
                                        Thread.Sleep(10000);
                                        //检测测厚指令表中ID=2的status中是否置1
                                        CheckMeasThickStatus(130, "InstrumentReady", "InstrumentReadyTime", 1);
                                    }
                                    //移动天车指令表
                                    if ((!finishFlag) && bCameraPreReady)
                                    {
                                        bCameraPreReady = false;
                                        All_operation = 7;
                                        g_ListStatus.Add(7);
                                        ModifyInteract1(1, g_ListStatus);
                                        Move12tCrnBlkSend(1, RealRackPosX, RealRackPosY);
                                        //检查到位信息
                                        Thread.Sleep(5000);
                                        Check12tArrive();
                                        Move12tCrnBlkStop(0, RealRackPosX, RealRackPosY);
                                    }
                                    //将系统交互数采指令表中operation置为0
                                    //ModifyInteract1(0,g_target,0);
                                    //ModifyInteract1(0, 1, 0);
                                    //拍照测厚部分

                                    if (bMoveCrnBlk && (!finishFlag) && (bPosMeasDone))
                                    {
                                        bMoveCrnBlk = false;
                                        All_operation = 8;
                                        g_ListStatus.Add(8);
                                        ModifyInteract1(1, g_ListStatus);
                                        MeasureThick();
                                        //ModifyInteract1(0, 1, 8);
                                        if ((!finishFlag) && bLinerMeasDone)
                                        {
                                            bLinerMeasDone = false;
                                            //ModifyInteract1(0, g_target, 7);

                                            MeasRackPos();
                                            Thread.Sleep(2000);
                                            ReadRackPos();
                                            RealRackPosX = WestRackPos - (RackPos / 10);
                                            //检测测厚指令表中ID=2的status中是否置1

                                            if ((!finishFlag) && bPosMeasDone)
                                            {
                                                bPosMeasDone = false;
                                                All_operation = 7;
                                                g_ListStatus.Add(7);
                                                ModifyInteract1(2, g_ListStatus);
                                                //移动天车指令表
                                                Move12tCrnBlkSend(1, RealRackPosX, RealRackPosY);
                                                //检查到位信息
                                                Thread.Sleep(2000);
                                                Check12tArrive();
                                                Move12tCrnBlkStop(0, RealRackPosX, RealRackPosY);
                                            }
                                            //将系统交互数采指令表中operation置为0
                                            //ModifyInteract1(0, g_target, 0);
                                            //ModifyInteract1(0, 2, 0);
                                            //拍照测厚部分(新写一个线程)
                                            if (bMoveCrnBlk && (!finishFlag) && bPosMeasDone)
                                            {
                                                bMoveCrnBlk = false;
                                                MeasEmptyLdlLin("MeasureStart", "MeasureStartTime", HandleEmptyladlenoWest, HandleEmptyladleServDutyWest, HandleEmptyladleAgeWest, HandleEmptyladleContractorWest, HandThickModeWest, 1);
                                                Thread.Sleep(1000);
                                                CheckMeasThickStatus(115, "InstrumentReady", "InstrumentReadyTime", 1);
                                                if ((bCameraPreReady) && (!finishFlag))
                                                {
                                                    bCameraPreReady = false;
                                                    All_operation = 8;
                                                    g_ListStatus.Add(8);
                                                    ModifyInteract1(2, g_ListStatus);
                                                    MeasureThick();
                                                }
                                                //天车回零
                                                //将系统交互数采指令表中operation置为0
                                                //ModifyInteract1(0, g_target, 9);


                                                if (bLinerMeasDone && (!finishFlag))
                                                {
                                                    bLinerMeasDone = false;
                                                    All_operation = 9;
                                                    g_ListStatus.Add(9);
                                                    ModifyInteract1(2, g_ListStatus);
                                                    //移动天车指令表
                                                    Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                                    //检查到位信息
                                                    Thread.Sleep(2000);
                                                    Check12tBack();
                                                    if (bMoveCrnBlk)
                                                    {
                                                        bMoveCrnBlk = false;
                                                        MeasEmptyLdlLinTianChePosi();
                                                    }
                                                    Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                                }
                                                //ModifyInteract1(0, 2, 0);

                                            }
                                        }
                                    }
                                    if ((!bPosMeasDone) && (!bMoveCrnBlk) && (!finishFlag))
                                    {
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空,且位置为0");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空，且位置为1");
                                        }
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(1, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);

                                    }
                                    if (bTimesOut && (!bMoveCrnBlk) && (!finishFlag))
                                    {
                                        bTimesOut = false;
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空,且位置为0");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空，且位置为1");
                                        }
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(1, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);

                                    }
                                    //ModifyInteract1(0, 1, 0);
                                    
                                    if (!finishFlag && (bMoveBackStart || bMoveBackScan))
                                    {
                                        bMoveBackStart = false;
                                        bMoveBackScan = false;
                                        g_ListStatus.Add(18);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        ThickHistoryInstruSave("测厚设备高温报警！");
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);

                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空,且位置为0");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空，且位置为1");
                                        }
                                    }
                                    if (finishFlag && !bMoveBack)
                                    {
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        g_ListStatus.Add(17);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                    }
                                    //SysInstrInit("将数采交互表初始化");
                                    g_ListStatus.Add(StandardAnswer);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    g_ListStatus.Clear(); 
                                }
                                break;
                        }
                        Thread.Sleep(2000);
                        //先测近包位（即东包位）
                        /*if (bMeasEmptyDown31 && (!finishFlag))
                        {
                            bMeasEmptyDown31=false;
                            ModifyInteract1(0, 1, 7);
                            All_operation = 7;
                            if (!finishFlag)
                            {
                                MeasEmptyLdlLin("MeasureStart", "MeasureStartTime", sEmptyLadleCurrentNo[0], EmptyCurr_ServDuty[0] , EmptyCurr_Age[0],EmptyCurr_Contractor[0], 1);
                            }
                            Thread.Sleep(10000);
                            //读包架位置
                            MeasRackPos();
                            Thread.Sleep(2000);
                            ReadRackPos();
                            //东包位
                            RealRackPosX = EastRackPos - (RackPos / 10);
                            //检测测厚指令表中ID=2的status中是否置1
                            CheckMeasThickStatus(200, "InstrumentReady", "InstrumentReadyTime", 1);
                            //移动天车指令表
                            if (bPosMeasDone && (!finishFlag) && bCameraPreReady)
                            {
                                bPosMeasDone = false;
                                bCameraPreReady = false;
                                Move12tCrnBlkSend(1, RealRackPosX, RealRackPosY);
                                //检查到位信息
                                Thread.Sleep(5000);
                                Check12tArrive();
                                Move12tCrnBlkStop(0, RealRackPosX, RealRackPosY);
                            }
                            //将系统交互数采指令表中operation置为0
                            //ModifyInteract1(0,g_target,0);
                            ModifyInteract1(0, 1, 0);
                            //拍照测厚部分

                            if (bMoveCrnBlk && (!finishFlag)&&(bPosMeasDone))
                            {
                                bMoveCrnBlk = false;
                                bPosMeasDone = false;
                                ModifyInteract1(0, 1, 8);
                                MeasureThick();
                                ModifyInteract1(0, 1, 8);
                                if (bMeasEmptyDown32 && (!finishFlag) && bLinerMeasDone)
                                {
                                    bMeasEmptyDown32=false;
                                    bLinerMeasDone = false;
                                    //ModifyInteract1(0, g_target, 7);
                                    ModifyInteract1(0, 2, 7);
                                    All_operation = 7;
                                    MeasEmptyLdlLin("MeasureStart", "MeasureStartTime",sEmptyLadleCurrentNo[1],EmptyCurr_ServDuty[1] , EmptyCurr_Age[1],EmptyCurr_Contractor[1], 1);
                                    MeasRackPos();
                                    Thread.Sleep(2000);
                                    ReadRackPos();
                                    RealRackPosX = WestRackPos - (RackPos / 10);
                                    CheckMeasThickStatus(200, "InstrumentReady", "InstrumentReadyTime", 1);
                                    if ((!finishFlag) && bPosMeasDone && bCameraPreReady)
                                    {
                                        bPosMeasDone = false;
                                        bCameraPreReady = false;
                                        //移动天车指令表
                                        Move12tCrnBlkSend(1, RealRackPosX, RealRackPosY);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tArrive();
                                        Move12tCrnBlkStop(0, RealRackPosX, RealRackPosY);

                                    }
                                    //将系统交互数采指令表中operation置为0
                                    //ModifyInteract1(0, g_target, 0);
                                    ModifyInteract1(0, 2, 0);
                                    //拍照测厚部分(新写一个线程)
                                    if (bMoveCrnBlk && (!finishFlag)&&bPosMeasDone)
                                    {
                                        bMoveCrnBlk = false;
                                        bPosMeasDone = false;
                                        //检测测厚指令表中ID=2的status中是否置1

                                        ModifyInteract1(0, 2, 8);
                                        MeasureThick();
                                        ModifyInteract1(0, 2, 0);
                                        //天车回零
                                        //将系统交互数采指令表中operation置为0
                                        //ModifyInteract1(0, g_target, 9);
                                        ModifyInteract1(0, 2, 9);
                                        All_operation = 9;
                                        if (bLinerMeasDone && (!finishFlag))
                                        {
                                            bLinerMeasDone = false;
                                            //移动天车指令表
                                            Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                            //检查到位信息
                                            Thread.Sleep(2000);
                                            Check12tBack();
                                            Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        }
                                        ModifyInteract1(0, 2, 0);

                                    }
                                    //天车回零
                                    //将系统交互数采指令表中operation置为0
                                    //ModifyInteract1(0, g_target, 9);

                                    //ModifyInteract1(0, g_target, 0);
                                    else if ((!bPosMeasDone) && (!bMoveCrnBlk) && (!finishFlag))
                                    {
                                        ModifyInteract1(0, 2, 9);
                                        All_operation = 9;
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);

                                    }
                                    ModifyInteract1(0, 2, 0);
                                }
                            }
                            else if ((!bPosMeasDone) && (!bMoveCrnBlk) && (!finishFlag))
                            {
                                ModifyInteract1(0, 1, 9);
                                All_operation = 9;
                                Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                //检查到位信息
                                Thread.Sleep(2000);
                                Check12tBack();
                                if (bMoveCrnBlk)
                                {
                                    bMoveCrnBlk = false;
                                    MeasEmptyLdlLinTianChePosi();
                                }
                                Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);

                            }
                            ModifyInteract1(0, 1, 0);
                            if (finishFlag)
                    {
                        EmptyLdlLinInit("UPDATE measemptyldlinstr SET LadleNo='T088',LadleServDuty=1,LadleAge=1,MeasureStart=0,MeasureStartTime=null,ScaningStart=0,ScaningStartTime=null,ScaningEnd=0,ScaningEndTime=null,TianChePosition=0,msg=null;", "将测量空包指令表清空");
                    }
                    if (finishFlag && bMoveBack)
                                    {
                                        bMoveBack = false;
                                        finishFlag = false;
                                        ModifyInteract1(0, HandleTarget, 9);
                                        All_operation = 9;
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                    }
                        }
                        //再测西包位
                        if (bMeasEmptyDown32&&(!bMeasEmptyDown31)&&(!finishFlag)) 
                        {
                            bMeasEmptyDown32=false;
                            ModifyInteract1(0, 2, 7);
                            All_operation = 7;
                            if (!finishFlag)
                            {
                                MeasEmptyLdlLin("MeasureStart", "MeasureStartTime", sEmptyLadleCurrentNo[1],EmptyCurr_ServDuty[1] , EmptyCurr_Age[1],EmptyCurr_Contractor[1], 1);
                            }
                            Thread.Sleep(10000);
                            //读包架位置
                            MeasRackPos();
                            Thread.Sleep(2000);
                            ReadRackPos();
                            //东包位
                            RealRackPosX = WestRackPos - RackPos / 10;
                            //检测测厚指令表中ID=2的status中是否置1
                            CheckMeasThickStatus(200, "InstrumentReady", "InstrumentReadyTime", 1);
                            //移动天车指令表
                            if (bPosMeasDone && (!finishFlag) && bCameraPreReady)
                            {
                                bPosMeasDone = false;
                                bCameraPreReady = false;
                                Move12tCrnBlkSend(1, RealRackPosX, RealRackPosY);
                                //检查到位信息
                                Thread.Sleep(2000);
                                Check12tArrive();
                                Move12tCrnBlkStop(0, RealRackPosX, RealRackPosY);
                            }
                            //将系统交互数采指令表中operation置为0
                            //ModifyInteract1(0, g_target, 0);
                            ModifyInteract1(0, 2, 0);

                            //拍照测厚
                            if (bMoveCrnBlk && (!finishFlag)&&(bPosMeasDone))
                            {
                                bMoveCrnBlk = false;
                                bPosMeasDone = true;
                                ModifyInteract1(0, 2, 8);
                                MeasureThick();
                                ModifyInteract1(0, 2, 0);
                                //ModifyInteract1(0, g_target, 9);
                                ModifyInteract1(0, 2, 9);
                                All_operation = 9;
                                //移动天车指令表
                                if ((!finishFlag) && bLinerMeasDone)
                                {
                                    bLinerMeasDone = false;
                                    Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                    //检查到位信息
                                    Thread.Sleep(2000);
                                    Check12tBack();
                                    if (bMoveCrnBlk)
                                    {
                                        bMoveCrnBlk = false;
                                        MeasEmptyLdlLinTianChePosi();
                                    }
                                    Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                }
                                //将系统交互数采指令表中operation置为0
                                //ModifyInteract1(0, g_target, 0);
                                ModifyInteract1(0, 2, 0);
                            }
                            else if ((!bPosMeasDone) && (!bMoveCrnBlk) && (!finishFlag))
                            {
                                ModifyInteract1(0, 2, 9);
                                All_operation = 9;
                                Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                //检查到位信息
                                Thread.Sleep(2000);
                                Check12tBack();
                                if (bMoveCrnBlk)
                                {
                                    bMoveCrnBlk = false;
                                    MeasEmptyLdlLinTianChePosi();
                                }
                                Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                            }
                            ModifyInteract1(0, 2, 0);
                            if (finishFlag)
                            {
                                EmptyLdlLinInit("UPDATE measemptyldlinstr SET LadleNo='T088',LadleServDuty=1,LadleAge=1,MeasureStart=0,MeasureStartTime=null,ScaningStart=0,ScaningStartTime=null,ScaningEnd=0,ScaningEndTime=null,TianChePosition=0,msg=null;", "将测量空包指令表清空");
                            }
                            if (finishFlag && bMoveBack)
                                    {
                                        bMoveBack = false;
                                        finishFlag = false;
                                        ModifyInteract1(0, HandleTarget, 9);
                                        All_operation = 9;
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                    }
                        }*/

                    }
                    //半自动线程
                    while (bStartSemiAutoThick && (!finishFlag))
                    {
                        All_operation = g_operation;
                        //用switch写这部分
                        switch (g_operation)
                        {
                            case 0:
                                {
                                    
                                    //什么都不做
                                    /*if (!finishFlag && bMoveBack)
                                    {
                                        bMoveBack = false;
                                        g_ListStatus.Clear();
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        g_ListStatus.Add(18);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        ThickHistoryInstruSave("测厚设备高温报警！");
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        SysInstrInit("将数采交互表初始化");
                                    }
                                    if (finishFlag && !bMoveBack)
                                    {
                                        //检查与零位位置
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        SysInstrInit("前端按下停止键！");
                                    }*/
                                }
                                break;
                            //operation=7, 天车到位
                            case 7:
                                {
                                    g_operation = 0;
                                    g_ListStatus.Clear();
                                    g_ListStatus.Add(77);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    //将系统交互数采指令表中operation置为0
                                    //ModifyInteract1(0, HandleTarget, 7);
                                    //读包架位置
                                    MeasRackPos();
                                    Thread.Sleep(2000);
                                    ReadRackPos();
                                    //东包位
                                    if ((HandleTarget == 1) && bPosMeasDone)
                                    {
                                        bPosMeasDone = false;
                                        //发出测厚相机开机指令
                                        //MeasEmptyLdlLin("MeasureStart", "MeasureStartTime", HandleEmptyladleno, 1, HandleEmptyladleAge, "未知砌筑商",1);
                                        MeasEmptyLdlLin("MeasureStart", "MeasureStartTime", HandleEmptyladlenoEast, HandleEmptyladleServDutyEast, HandleEmptyladleAgeEast, HandleEmptyladleContractorEast, HandThickModeEast, 1);
                                        RealRackPosX = EastRackPos - (RackPos / 10);
                                        CheckMeasThickStatus(140, "InstrumentReady", "InstrumentReadyTime", 1);
                                        if ((!finishFlag) && bCameraPreReady)
                                        {
                                            bCameraPreReady = false;
                                            All_operation = 7;
                                            g_ListStatus.Add(7);
                                            ModifyInteract1(HandleTarget, g_ListStatus);
                                            Move12tCrnBlkSend(1, RealRackPosX, RealRackPosY);
                                            //检查到位信息
                                            Thread.Sleep(5000);
                                            Check12tArrive();

                                            Move12tCrnBlkStop(0, RealRackPosX, RealRackPosY);
                                        }
                                    }
                                    //西包位
                                    if ((HandleTarget == 2) && bPosMeasDone)
                                    {
                                        //发出测厚相机开机指令
                                        bPosMeasDone = false;
                                        //MeasEmptyLdlLin("MeasureStart", "MeasureStartTime", HandleEmptyladleno, 1, HandleEmptyladleAge, "未知砌筑商",1);
                                        MeasEmptyLdlLin("MeasureStart", "MeasureStartTime", HandleEmptyladlenoWest, HandleEmptyladleServDutyWest, HandleEmptyladleAgeWest, HandleEmptyladleContractorWest, HandThickModeWest, 1);
                                        RealRackPosX = WestRackPos - (RackPos / 10);
                                        CheckMeasThickStatus(200, "InstrumentReady", "InstrumentReadyTime", 1);
                                        if ((!finishFlag) && bCameraPreReady)
                                        {
                                            bCameraPreReady = false;
                                            All_operation = 7;
                                            g_ListStatus.Add(7);
                                            ModifyInteract1(HandleTarget, g_ListStatus);
                                            Move12tCrnBlkSend(1, RealRackPosX, RealRackPosY);
                                            //检查到位信息
                                            Thread.Sleep(5000);
                                            Check12tArrive();
                                            Move12tCrnBlkStop(0, RealRackPosX, RealRackPosY);
                                        }
                                    }
                                    //将系统交互数采指令表中operation置为0

                                    if ((!bPosMeasDone) && (!bMoveCrnBlk) && (!finishFlag))
                                    {
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //ModifyInteract1(0, HandleTarget, 0);
                                    }
                                    if (bTimesOut && (!bMoveCrnBlk) && (!finishFlag))
                                    {
                                        bTimesOut = false;
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //ModifyInteract1(0, HandleTarget, 0);
                                    }
                                    if (bPosMeasDone)
                                    {
                                        bPosMeasDone = false;
                                    }
                                    //ModifyInteract1(0, HandleTarget, 0);
                                    
                                    if (!finishFlag && (bMoveBackStart || bMoveBackScan))
                                    {
                                        bMoveBackStart = false;
                                        bMoveBackScan = false;
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        g_ListStatus.Add(18);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        ThickHistoryInstruSave("测厚设备高温报警！");
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                    }
                                    if (finishFlag && !bMoveBack)
                                    {
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        g_ListStatus.Add(17);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                    }
                                    g_ListStatus.Add(StandardAnswer);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    g_ListStatus.Clear();
                                }
                                break;
                            //operation=8，扫描拍照
                            case 8:
                                {
                                    g_operation = 0;
                                    g_ListStatus.Clear();
                                    g_ListStatus.Add(78);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    if (bMoveCrnBlk && (!finishFlag))
                                    {
                                        bMoveCrnBlk = false;
                                        All_operation = 8;
                                        g_ListStatus.Add(8);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        MeasureThick();
                                    }
                                    //ModifyInteract1(0, HandleTarget, 0);
                                    if (bLinerMeasDone)
                                    {
                                        bLinerMeasDone = false;
                                    }
                                    
                                    if (!finishFlag && (bMoveBackStart || bMoveBackScan))
                                    {
                                        bMoveBackStart = false;
                                        bMoveBackScan = false;
                                        g_ListStatus.Add(18);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        ThickHistoryInstruSave("测厚设备高温报警！");
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                    }
                                    if (finishFlag && !bMoveBack)
                                    {
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        g_ListStatus.Add(17);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                    }
                                    g_ListStatus.Add(StandardAnswer);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    g_ListStatus.Clear();
                                }
                                break;
                            //operation=9，天车回零
                            case 9:
                                {
                                    g_operation = 0;
                                    g_ListStatus.Clear();
                                    g_ListStatus.Add(79);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    //将系统交互数采指令表中operation置为0
                                    //ModifyInteract1(0, HandleTarget, 9);
                                    //移动天车指令表
                                    if (!finishFlag)
                                    {
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1( HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(5000);
                                        Check12tBack();
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                    }
                                    if (finishFlag && !bMoveBack)
                                    {
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        g_ListStatus.Add(17);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                    }
                                    //将系统交互数采指令表中operation置为0
                                    //ModifyInteract1(0, HandleTarget, 0);
                                    g_ListStatus.Add(StandardAnswer);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    g_ListStatus.Clear();
                                }
                                break;

                        }
                        Thread.Sleep(2000);
                    }
                    //手动线程
                    while (bStartHandleThick && (!finishFlag))
                    {
                        All_operation = g_operation;
                        //用switch来写这部分
                        switch (g_operation)
                        {
                            case 0:
                                {
                                    
                                    //什么都不做
                                    if (finishFlag && bMoveBack)
                                    {
                                        bMoveBack = false;
                                        finishFlag = false;
                                        g_ListStatus.Clear();
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        g_ListStatus.Add(18);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        ThickHistoryInstruSave("测厚设备高温报警！");
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);


                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        SysInstrInit("将数采交互表初始化");
                                    }
                                    if (finishFlag && !bMoveBack)
                                    {
                                        //检查与零位位置
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        SysInstrInit("前端按下停止键！");
                                    }
                                }
                                break;
                            //operation=1,大车到位
                            case 1:
                                {
                                    g_operation = 0;
                                    g_ListStatus.Clear();
                                    g_ListStatus.Add(81);
                                    ModifyInteract1_Operate( 0, HandleTarget, g_ListStatus);
                                    //读包架位置
                                    MeasRackPos();
                                    Thread.Sleep(2000);
                                    ReadRackPos();
                                    //东包位
                                    if ((HandleTarget == 1) && bPosMeasDone)
                                    {
                                        bPosMeasDone = false;
                                        //发出测厚相机开机指令
                                        MeasEmptyLdlLin("MeasureStart", "MeasureStartTime", HandleEmptyladlenoEast, HandleEmptyladleServDutyEast, HandleEmptyladleAgeEast, HandleEmptyladleContractorEast, HandThickModeEast, 1);
                                        RealRackPosX = EastRackPos - RackPos / 10;
                                        CheckMeasThickStatus(140, "InstrumentReady", "InstrumentReadyTime", 1);
                                        if ((!finishFlag) && bCameraPreReady)
                                        {
                                            bCameraPreReady = false;
                                            All_operation = 1;
                                            g_ListStatus.Add(1);
                                            //将系统交互数采指令表中operation置为0
                                            ModifyInteract1(HandleTarget, g_ListStatus);
                                            Move12tCrnBlkSend(3, RealRackPosX, EmptyCrnBlk0Y);
                                            //检查到位信息
                                            Thread.Sleep(2000);
                                            Check12tArrive();
                                            Move12tCrnBlkStop(0, RealRackPosX, EmptyCrnBlk0Y);
                                        }
                                    }
                                    //西包位
                                    if ((HandleTarget == 2) && bPosMeasDone)
                                    {
                                        bPosMeasDone = false;
                                        //发出测厚相机开机指令
                                        MeasEmptyLdlLin("MeasureStart", "MeasureStartTime", HandleEmptyladlenoWest, HandleEmptyladleServDutyWest, HandleEmptyladleAgeWest, HandleEmptyladleContractorWest, HandThickModeWest, 1);
                                        RealRackPosX = WestRackPos - RackPos / 10;
                                        CheckMeasThickStatus(140, "InstrumentReady", "InstrumentReadyTime", 1);
                                        if ((!finishFlag) && bCameraPreReady)
                                        {
                                            bCameraPreReady = false;
                                            All_operation = 1;
                                            g_ListStatus.Add(1);
                                            //将系统交互数采指令表中operation置为0
                                            ModifyInteract1(HandleTarget, g_ListStatus);
                                            Move12tCrnBlkSend(3, RealRackPosX, EmptyCrnBlk0Y);
                                            //检查到位信息
                                            Thread.Sleep(2000);
                                            Check12tArrive();
                                            Move12tCrnBlkStop(0, RealRackPosX, EmptyCrnBlk0Y);
                                        }
                                    }

                                    if ((!bPosMeasDone) && (!bMoveCrnBlk) && (!finishFlag))
                                    {
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //ModifyInteract1(0, HandleTarget, 0);
                                    }
                                    if (bTimesOut && (!bMoveCrnBlk) && (!finishFlag))
                                    {
                                        bTimesOut = false;
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //ModifyInteract1(0, HandleTarget, 0);
                                    }
                                    if (bPosMeasDone)
                                    {
                                        bPosMeasDone = false;
                                    }
                                    if (bMoveCrnBlk)
                                    {
                                        bMoveCrnBlk = false;
                                    }
                                    //将系统交互数采指令表中operation置为0
                                    //ModifyInteract1(0, HandleTarget, 0);
                                    
                                    if (!finishFlag && (bMoveBackStart || bMoveBackScan))
                                    {
                                        bMoveBackStart = false;
                                        bMoveBackScan = false;
                                        g_ListStatus.Add(18);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        ThickHistoryInstruSave("测厚设备高温报警！");
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                    }
                                    if (finishFlag && !bMoveBack)
                                    {
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        g_ListStatus.Add(17);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                    }
                                    g_ListStatus.Add(StandardAnswer);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    g_ListStatus.Clear();
                                }
                                break;
                            //operation=2,小车到位
                            case 2:
                                {
                                    g_operation = 0;
                                    g_ListStatus.Clear();
                                    g_ListStatus.Add(82);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    g_ListStatus.Add(2);
                                    //将系统交互数采指令表中operation置为0
                                    ModifyInteract1(HandleTarget, g_ListStatus);

                                    //移动天车指令表
                                    Move12tCrnBlkSend(4, RealRackPosX, RealRackPosY);
                                    //检查到位信息
                                    Thread.Sleep(2000);
                                    Check12tArrive();
                                    Move12tCrnBlkStop(0, RealRackPosX, RealRackPosY);
                                    //将系统交互数采指令表中operation置为0
                                    g_ListStatus.Add(StandardAnswer);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    g_ListStatus.Clear();
                                }
                                break;
                            //operation=3，测厚扫描
                            case 3:
                                {
                                    g_operation = 0;
                                    g_ListStatus.Clear();
                                    g_ListStatus.Add(83);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    if (bMoveCrnBlk && (!finishFlag))
                                    {
                                        bMoveCrnBlk = false;
                                        g_ListStatus.Add(3);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        MeasureThick();
                                    }
                                    //ModifyInteract1(0, HandleTarget, 0);
                                    if (finishFlag && !bMoveBack)
                                    {
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                        g_ListStatus.Add(17);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                    }
                                    if (!finishFlag && (bMoveBackStart || bMoveBackScan))
                                    {
                                        bMoveBackStart = false;
                                        bMoveBackScan = false;
                                        g_ListStatus.Add(18);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        ThickHistoryInstruSave("测厚设备高温报警！");
                                        All_operation = 9;
                                        g_ListStatus.Add(9);
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                        Move12tCrnBlkSend(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        //检查到位信息
                                        Thread.Sleep(2000);
                                        Check12tBack();
                                        if (bMoveCrnBlk)
                                        {
                                            bMoveCrnBlk = false;
                                            MeasEmptyLdlLinTianChePosi();
                                        }
                                        Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                        if (IsZeroPosiyion(DeltaX, DeltaY))
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr0, "将测量空包指令表清空");
                                        }
                                        else
                                        {
                                            EmptyLdlLinInit(InitMeasEmptyInstr, "将测量空包指令表清空");
                                        }
                                    }
                                    g_ListStatus.Add(StandardAnswer);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    g_ListStatus.Clear();
                                }
                                break;
                            //operation=4，测厚拍照
                            case 4:
                                {

                                }
                                break;
                            //operation=5,大车回零
                            case 5:
                                {
                                    g_operation = 0;
                                    g_ListStatus.Clear();
                                    g_ListStatus.Add(85);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    g_ListStatus.Add(5);
                                    //将系统交互数采指令表中operation置为0
                                    ModifyInteract1(HandleTarget, g_ListStatus);
                                    //移动天车指令表
                                    Move12tCrnBlkSend(5, EmptyCrnBlk0X, RealRackPosY);
                                    //检查到位信息
                                    Thread.Sleep(2000);
                                    Check12tBack();
                                    Move12tCrnBlkStop(0, EmptyCrnBlk0X, RealRackPosY);
                                    if (bMoveCrnBlk)
                                    {
                                        bMoveCrnBlk = false;
                                        ModifyInteract1(HandleTarget, g_ListStatus);
                                    }
                                    //将系统交互数采指令表中operation置为0
                                    //ModifyInteract1(0, HandleTarget, 0);
                                    g_ListStatus.Add(StandardAnswer);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    g_ListStatus.Clear();
                                }
                                break;
                            //operation=6，小车回零
                            case 6:
                                {
                                    g_operation = 0;
                                    g_ListStatus.Clear();
                                    g_ListStatus.Add(86);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    g_ListStatus.Add(6);
                                    //将系统交互数采指令表中operation置为0
                                    ModifyInteract1(HandleTarget, g_ListStatus);
                                    //移动天车指令表
                                    Move12tCrnBlkSend(6, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                    //检查到位信息
                                    Thread.Sleep(2000);
                                    Check12tBack();
                                    Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                    if (bMoveCrnBlk)
                                    {
                                        bMoveCrnBlk = false;
                                        MeasEmptyLdlLinTianChePosi();
                                    }
                                    //将系统交互数采指令表中operation置为0
                                    //ModifyInteract1(0, HandleTarget, 0);
                                    g_ListStatus.Add(StandardAnswer);
                                    ModifyInteract1_Operate(0, HandleTarget, g_ListStatus);
                                    g_ListStatus.Clear();
                                }
                                break;
                        }
                        Thread.Sleep(2000);

                    }
                }
            }
            catch (Exception EE)
            {
                MessageBox.Show(EE.ToString());
            }
        }
        public static int StatusCodelast=99999;
        public static int MoveCrnBlkFlag_templast=99999;
        public static bool statusflag;
        //检车天车到位
        private void Check12tArrive()
        {
            MySqlConnection connMySql;
            try
            {
                //定义一个mysql数据库连接对象
                connMySql = new MySqlConnection(MySQLUser);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();     //打开数据库
                }
                Timeout waittime = new Timeout(1000);
                StatusCode = 9999;
                StatusCodelast = 9999;
                MoveCrnBlkFlag_templast = 9999;
                while (!finishFlag)
                {
                    Thread.Sleep(1000);
                    string CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string SQL = "select ReceInstrTime,ReceInstrFlag,MoveCrnBlkTime,MoveCrnBlkFlag,RealX from movecrnblkinstr where SerNo=1;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "movecrnblkinstr");
                    string ReceInstrTime_temp = ds.Tables[0].Rows[0][0].ToString();
                    int ReceInstrFlag_temp = Convert.ToInt32(ds.Tables[0].Rows[0][1]);
                    string MoveCrnBlkTime_temp = ds.Tables[0].Rows[0][2].ToString();
                    int MoveCrnBlkFlag_temp = Convert.ToInt32(ds.Tables[0].Rows[0][3]);
                    float CrnRealX = Convert.ToSingle(ds.Tables[0].Rows[0][4]);
                    string SQL1 = "select StatusCode from crnblkcurpos where id=1;";
                    MySqlCommand thisCommand = new MySqlCommand(SQL1, connMySql);
                    if ((thisCommand.ExecuteScalar()) != System.DBNull.Value)
                    {
                        StatusCode = Convert.ToInt32(thisCommand.ExecuteScalar());
                    }


                    string Status12tAll = Convert.ToString(StatusCode, 2);

                    if (StatusCode != StatusCodelast)
                    {
                        ThickHistoryInstruSave("检测移车指令表：状态码为'" + Status12tAll + "'");
                        statusflag = true;
                        StatusCodelast = StatusCode;
                    }
                    else
                    {
                        statusflag = false;
                    }
                    if (MoveCrnBlkFlag_temp != MoveCrnBlkFlag_templast)
                    {
                        ThickHistoryInstruSave("检测移车指令表：到位标志为'" + MoveCrnBlkFlag_temp + "'");
                        MoveCrnBlkFlag_templast = MoveCrnBlkFlag_temp;
                    }

                    if ((MoveCrnBlkFlag_temp == 0) && ((StatusCode != 128) || (StatusCode != 2176)|| (StatusCode !=0)||(StatusCode != 2048)) &&(!finishFlag)&&(statusflag))
                    {
                        for (int i = Status12tAll.Length - 1; i >= 0; i--)
                        {
                            //0位
                            if (i == Status12tAll.Length - 1)
                            {
                                if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                                {
                                    
                                    Move12tCrnBlkOnTheWay(0, RealRackPosX, RealRackPosY);
                                }
                            }
                            if (i == Status12tAll.Length - 2)
                            {
                                if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                                {
                                    Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                }
                            }
                            if (i == Status12tAll.Length - 3)
                            {
                                if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                                {
                                    Move12tCrnBlkOnTheWay(0, RealRackPosX, RealRackPosY);
                                }
                            }
                            if (i == Status12tAll.Length - 4)
                            {
                                if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                                {
                                    Move12tCrnBlkOnTheWay(0, RealRackPosX, RealRackPosY);
                                }
                            }
                            if (i == Status12tAll.Length - 5)
                            {
                                if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                                {
                                    Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                }
                            }
                        }
                    }
                    if ((MoveCrnBlkFlag_temp == 0)&&(!finishFlag) && (statusflag))
                    {
                        if ((All_operation == 7)&& (!finishFlag))
                        {
                            MeasRackPos();
                            Thread.Sleep(5000);
                            ReadRackPos();
                            if (bPosMeasDone & (!finishFlag))
                            {
                                bPosMeasDone = true;
                                Move12tCrnBlkOnTheWay(1, RealRackPosX, RealRackPosY);
                            }
                            if (!bPosMeasDone)
                            {
                                break;
                            }
                        }
                        if ((All_operation == 9)&& (!finishFlag))
                        {
                            Move12tCrnBlkOnTheWay(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                        }
                        if ((All_operation == 1)&& (!finishFlag))
                        {
                            MeasRackPos();
                            Thread.Sleep(5000);
                            ReadRackPos();
                            if (bPosMeasDone & (!finishFlag))
                            {
                                bPosMeasDone = true;
                                Move12tCrnBlkOnTheWay(3, RealRackPosX, RealRackPosY);
                            }
                            if (!bPosMeasDone)
                            {
                                break;
                            }
                        }
                        if ((All_operation == 2)&& (!finishFlag))
                        {
                            MeasRackPos();
                            Thread.Sleep(5000);
                            ReadRackPos();
                            if (bPosMeasDone&(!finishFlag))
                            {
                                bPosMeasDone = true;
                                Move12tCrnBlkOnTheWay(4, RealRackPosX, RealRackPosY);
                            }
                            if (!bPosMeasDone)
                            {
                                break;
                            }
                        }
                        if ((All_operation == 5)&& (!finishFlag))
                        {
                            Move12tCrnBlkOnTheWay(5, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                        }
                        if ((All_operation == 6)&& (!finishFlag))
                        {
                            Move12tCrnBlkOnTheWay(6, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                        }
                    }
                    if ((MoveCrnBlkFlag_temp == 1)&&((StatusCode == 0)||(StatusCode ==2048)) &&(!finishFlag))
                    {
                        MeasRackPos();
                        Thread.Sleep(1000);
                        ReadRackPos();
                        if (bPosMeasDone & (!finishFlag))
                        {
                            bPosMeasDone = true;
                        }
                        g_ListStatus.Add(27);
                        ModifyInteract1(HandleTarget,g_ListStatus);
                        if (dtGrdViewHianCheInstru.IsHandleCreated)
                        {
                            Action action = () =>
                            {
                                ShowTableTianChe("movecrnblkinstr");
                            };
                            BeginInvoke(action);
                        }
                        ThickHistoryInstruSave("移车指令表：天车已到位！移车指令目标地址为"+ RealRackPosX + ",天车实际位置为"+CrnRealX+"");
                        bMoveCrnBlk = true;
                        break;
                    }
                    if (finishFlag) 
                    {
                        ThickHistoryInstruSave("检测移车指令表被前端停止！");
                        break;
                    }
                    if (waittime.IsTimeout())
                    {
                        ThickHistoryInstruSave("移车指令表：超时！天车未到位！");
                        //将系统交互数采指令表中operation置为0
                        g_ListStatus.Add(14);
                        ModifyInteract1(HandleTarget ,g_ListStatus);
                        bTimesOut = true;
                        break;
                    }
                }
                connMySql.Close();

            }
            catch (Exception ee)
            {
                ThickHistoryInstruSave("监测检查天车到位出问题！" + ee.ToString());
            }
        }
        private void Check12tBack()
        {
            MySqlConnection connMySql;
            try
            {
                //定义一个mysql数据库连接对象
                connMySql = new MySqlConnection(MySQLUser);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();     //打开数据库
                }
                Timeout waittime = new Timeout(10000);
                StatusCode = 9999;
                StatusCodelast = 9999;
                MoveCrnBlkFlag_templast = 9999;
                while (!finishFlag)
                {
                    string CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string SQL = "select ReceInstrTime,ReceInstrFlag,MoveCrnBlkTime,MoveCrnBlkFlag from movecrnblkinstr where SerNo=1;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "movecrnblkinstr");
                    string ReceInstrTime_temp = ds.Tables[0].Rows[0][0].ToString();
                    int ReceInstrFlag_temp = Convert.ToInt32(ds.Tables[0].Rows[0][1]);
                    string MoveCrnBlkTime_temp = ds.Tables[0].Rows[0][2].ToString();
                    int MoveCrnBlkFlag_temp = Convert.ToInt32(ds.Tables[0].Rows[0][3]);


                    string SQL1 = "select StatusCode from crnblkcurpos where id=1;";
                    MySqlCommand thisCommand = new MySqlCommand(SQL1, connMySql);
                    if ((thisCommand.ExecuteScalar()) != System.DBNull.Value)
                    {
                        StatusCode = Convert.ToInt32(thisCommand.ExecuteScalar());
                    }
                    string Status12tAll = Convert.ToString(StatusCode, 2);

                    if (StatusCode != StatusCodelast)
                    {
                        ThickHistoryInstruSave("检测移车指令表：状态码为'" + Status12tAll + "'");
                        statusflag = true;
                        StatusCodelast = StatusCode;
                    }
                    else
                    {
                        statusflag = false;
                    }
                    if (MoveCrnBlkFlag_temp != MoveCrnBlkFlag_templast)
                    {
                        ThickHistoryInstruSave("检测移车指令表：到位标志为'" + MoveCrnBlkFlag_temp + "'");
                        MoveCrnBlkFlag_templast = MoveCrnBlkFlag_temp;
                    }
                    if ((MoveCrnBlkFlag_temp == 0) && ((StatusCode != 0)||(StatusCode != 2048)||((StatusCode != 1024))) && (!finishFlag)&&(statusflag))
                    {
                        for (int i = Status12tAll.Length - 1; i >= 0; i--)
                        {
                            //0位
                            if (i == Status12tAll.Length - 1)
                            {
                                if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                                {
                                   // Move12tCrnBlkOnTheWay(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                }
                            }
                            if (i == Status12tAll.Length - 2)
                            {
                                if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                                {
                                    Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                }
                            }
                            if (i == Status12tAll.Length - 3)
                            {
                                if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                                {
                                    Move12tCrnBlkOnTheWay(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                }
                            }
                            if (i == Status12tAll.Length - 4)
                            {
                                if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                                {
                                    Move12tCrnBlkOnTheWay(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                }
                            }
                            if (i == Status12tAll.Length - 5)
                            {
                                if (String.Equals(Status12tAll.Substring(i, 1), "1"))
                                {
                                    Move12tCrnBlkStop(0, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                                }
                            }
                            
                        }
                    }
                    if ((MoveCrnBlkFlag_temp == 0) && ((StatusCode == 0)||(StatusCode == 1024)|| (StatusCode == 2048)) && (!finishFlag) && (statusflag))
                    {
                        if ((All_operation == 7) && (!finishFlag))
                        {
                            Move12tCrnBlkOnTheWay(1, RealRackPosX, RealRackPosY);
                        }
                        if ((All_operation == 9) && (!finishFlag))
                        {
                            Move12tCrnBlkOnTheWay(2, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                        }
                        if ((All_operation == 1) && (!finishFlag))
                        {
                            Move12tCrnBlkOnTheWay(3, RealRackPosX, RealRackPosY);
                        }
                        if ((All_operation == 2) && (!finishFlag))
                        {
                            Move12tCrnBlkOnTheWay(4, RealRackPosX, RealRackPosY);
                        }
                        if ((All_operation == 5) && (!finishFlag))
                        {
                            Move12tCrnBlkOnTheWay(5, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                        }
                        if ((All_operation == 6) && (!finishFlag))
                        {
                            Move12tCrnBlkOnTheWay(6, EmptyCrnBlk0X, EmptyCrnBlk0Y);
                        }
                    }
                    if (IsZeroPosiyion(300, 2000))
                    {
                        MoveCrnBlkFlag_temp = 1;
                    }
                    if ((MoveCrnBlkFlag_temp == 1)&&(!finishFlag))
                    {
                        g_ListStatus.Add(29);
                        ModifyInteract1(HandleTarget, g_ListStatus);
                        bMoveCrnBlk = true;
                        if (dtGrdViewHianCheInstru.IsHandleCreated)
                        {
                            Action action = () =>
                            {
                                ShowTableTianChe("movecrnblkinstr");
                            };
                            BeginInvoke(action);
                        }
                        ThickHistoryInstruSave("移车指令表：天车已回到零位！");
                        break;
                    }
                    if (finishFlag)
                    {
                        ThickHistoryInstruSave("检测移车指令表是否回到零位时被前端停止！");
                        break;
                    }
                    if (waittime.IsTimeout())
                    {
                        ThickHistoryInstruSave("移车指令表：超时！天车未回到零位！");
                        //将系统交互数采指令表中operation置为0
                        break;
                    }
                }
                connMySql.Close();

            }
            catch (Exception ee)
            {
                MessageBox.Show("天车回零失败！" + ee.ToString());
            }
        }
        //修改交互表operation和status
        private  void ModifyInteract1(int target, List<byte> list)
        {
            MySqlConnection connMySql;

            try
            {
                //定义一个mysql数据库连接对象
                connMySql = new MySqlConnection(MySQLUser);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }   //打开数据库
                    //如何将一个列表statuslist存入数据库
                byte[] statusArry = list.ToArray();
                string SQL = "UPDATE sysinteractinstr_copy1 SET target=@target ,status=@status WHERE id=1;";
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = connMySql;
                cmd.CommandText = SQL;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue(@"target", target);
                cmd.Parameters.AddWithValue(@"status", statusArry);
                cmd.ExecuteNonQuery();
                connMySql.Close();
                ThickHistoryInstruSave("前后台交互表：UPDATE sysinteractinstr_copy1 SET target="+ target + " ,status="+ statusArry .Last()+ " WHERE id=1;");
            }
            catch (Exception ee)
            {
                MessageBox.Show("修改交互表失败！" + ee.ToString());
            }
        }
        private void ModifyInteract1_Operate(int operation,int target, List<byte> list)
        {
            MySqlConnection connMySql;

            try
            {
                //定义一个mysql数据库连接对象
                connMySql = new MySqlConnection(MySQLUser);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }   //打开数据库
                    //如何将一个列表statuslist存入数据库
                byte[] statusArry = list.ToArray();
                string SQL = "UPDATE sysinteractinstr_copy1 SET operation=@operation ,target=@target ,status=@status WHERE id=1;";
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = connMySql;
                cmd.CommandText = SQL;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue(@"operation", operation);
                cmd.Parameters.AddWithValue(@"target", target);
                cmd.Parameters.AddWithValue(@"status", statusArry);
                cmd.ExecuteNonQuery();
                connMySql.Close();
                ThickHistoryInstruSave("前后台交互表：UPDATE sysinteractinstr_copy1 SET operation=" + operation + ",target=" + target + " ,status=" + statusArry.Last() + " WHERE id=1;");
            }
            catch (Exception ee)
            {
                MessageBox.Show("修改交互表失败！" + ee.ToString());
            }
        }

        private void ModifyInteract1_Mode(int mode,int operation, int target, List<byte> list)
        {
            MySqlConnection connMySql;

            try
            {
                //定义一个mysql数据库连接对象
                connMySql = new MySqlConnection(MySQLUser);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }   //打开数据库
                    //如何将一个列表statuslist存入数据库
                byte[] statusArry = list.ToArray();
                string SQL = "UPDATE sysinteractinstr_copy1 SET mode=@mode,operation=@operation ,target=@target ,status=@status WHERE id=1;";
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = connMySql;
                cmd.CommandText = SQL;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue(@"mode", mode);
                cmd.Parameters.AddWithValue(@"operation", operation);
                cmd.Parameters.AddWithValue(@"target", target);
                cmd.Parameters.AddWithValue(@"status", statusArry);
                cmd.ExecuteNonQuery();
                connMySql.Close();
                ThickHistoryInstruSave("前后台交互表：UPDATE sysinteractinstr_copy1 SET mode=" + mode + ",operation=" + operation + ",target=" + target + " ,status=" + statusArry.Last() + " WHERE id=1;");
            }
            catch (Exception ee)
            {
                MessageBox.Show("修改交互表失败！" + ee.ToString());
            }
        }

        //清空交互表
        public void SysInstrInit(string thickHistoryInstruSave)
        {
            MySqlConnection connMySql;
            try
            {
                //定义一个mysql数据库连接对象
                connMySql = new MySqlConnection(MySQLUser);
                connMySql.Open();//打开数据库
                string strSQL = "UPDATE sysinteractinstr_copy1 SET mode=3,operation=0,target=0,status=x'21' WHERE id=1;";
                MySqlCommand thisCommand = new MySqlCommand(strSQL, connMySql);
                thisCommand.ExecuteNonQuery();
                connMySql.Close();
                ThickHistoryInstruSave(thickHistoryInstruSave);
            }
            catch (Exception ee)
            {
                MessageBox.Show("数采交互表初始化失败！" + ee.ToString());
            }
        }

        //自动测厚
        private void MeasureThick()
        {
            //发送测厚指令，将ID=3的status置1
            MeasEmptyLdlLin2("ScaningStart","ScaningStartTime");
            Thread.Sleep(10000);
            //检测测厚指令表中ID=4的status置1
            CheckMeasThickStatus(90,"ScaningEnd" , "ScaningEndTime",2);
            if (!bMoveBack)
            {
                EmptyLdlLinInit("UPDATE measemptyldlinstr SET LadleNo='T088',LadleServDuty=1,LadleAge=1,MeasureStart=0,MeasureStartTime=null,ScaningStart=0,ScaningStartTime=null,ScaningEnd=0,ScaningEndTime=null,msg=null;", "将测量空包指令表清空");
            }
            if (!bLinerMeasDone && bTimesOut)
            {
                EmptyLdlLinInit("UPDATE measemptyldlinstr SET LadleNo='T088',LadleServDuty=1,LadleAge=1,MeasureStart=0,MeasureStartTime=null,InstrumentReady=0,InstrumentReadyTime=null,ScaningStart=0,ScaningStartTime=null,ScaningEnd=0,ScaningEndTime=null,msg=null;", "将测量空包指令表清空,且测厚超时");

            }
        }
        //检测测厚相机指令
        private void CheckMeasThickStatus(int seconds,string ITEM,string ITEMTIME,int n)
        {
            MySqlConnection connMySql;
            try
            {
                connMySql = new MySqlConnection(MySQLUser);//定义一个mysql数据库连接对象
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();     //打开数据库
                }
                Timeout waittime = new Timeout(seconds);
                while (!finishFlag)
                {
                    string SQL = "select "+ITEM+","+ ITEMTIME + " from measemptyldlinstr WHERE id=1;";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(SQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "measemptyldlinstr");
                    int status_temp = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                    string time_temp = ds.Tables[0].Rows[0][1].ToString();
                    if (status_temp == 1)
                    {
                        switch (n) 
                        {
                            case 1:
                                {
                                    bCameraPreReady = true;
                                    g_ListStatus.Add(15);
                                    ModifyInteract1( HandleTarget, g_ListStatus);
                                    ThickHistoryInstruSave("检测测量空包指令表：相机已开机！");
                                    if (dtGrdViewThick.IsHandleCreated)
                                    {
                                        Action action1 = () =>
                                        {
                                            ShowTableThick("measemptyldlinstr");
                                        };
                                        BeginInvoke(action1);
                                    }
                                }
                                break;
                            case 2:
                                {
                                    bLinerMeasDone = true;
                                    g_ListStatus.Add(28);
                                    ModifyInteract1(HandleTarget, g_ListStatus);
                                    ThickHistoryInstruSave("检测测量空包指令表：线扫已完成！");
                                    if (dtGrdViewThick.IsHandleCreated)
                                    {
                                        Action action1 = () =>
                                        {
                                            ShowTableThick("measemptyldlinstr");
                                        };
                                        BeginInvoke(action1);
                                    }
                                }
                                break;
                        }
                        
                        break;
                    }
                    if (finishFlag&&!bMoveBack)
                    {
                        ThickHistoryInstruSave("测量空包指令表：前台退出！");
                        break;
                    }
                    if (bMoveBack)
                    {
                        if (n == 1)
                        {
                            bMoveBackStart = false;
                            bMoveBackScan = false;
                            ThickHistoryInstruSave("开机时，出现高温报警！");
                            Thread.Sleep(10000);
                            if (bMoveBack)
                            {
                                bMoveBackStart = true;
                                ThickHistoryInstruSave("开机时10s后，依旧高温报警！");
                                break;
                            }
                        }
                        if (n == 2)
                        {
                            bMoveBackScan = false;
                            bMoveBackStart = false;
                            ThickHistoryInstruSave("扫描空包时，出现高温报警！");
                            Thread.Sleep(20000);
                            if (bMoveBack)
                            {
                                bMoveBackScan = true;
                                ThickHistoryInstruSave("扫描空包20s后，依旧高温报警！");
                                break;
                            }
                        }
                        
                    }
                    if (waittime.IsTimeout())
                    {
                        switch (n) 
                        {
                            case 1: 
                                {
                                    bCameraPreReady = false;
                                    bTimesOut = true;
                                    g_ListStatus.Add(11);
                                    //将系统交互数采指令表中operation置为0
                                    ModifyInteract1( HandleTarget, g_ListStatus);
                                    ThickHistoryInstruSave("检测测量空包指令表：相机未开机！！！测厚已超时！");
                                    //清空测厚指令表
                                    EmptyLdlLinInit("UPDATE measemptyldlinstr SET MeasureStart=0,MeasureStartTime=null,ScaningStart=0,ScaningStartTime=null,ScaningEnd=0,ScaningEndTime=null,msg=null;", "将测量空包指令表清空");
                                }
                                break;
                            case 2: 
                                {
                                    bLinerMeasDone = false;
                                    bTimesOut = true;
                                    g_ListStatus.Add(12);
                                    //将系统交互数采指令表中operation置为0
                                    ModifyInteract1( HandleTarget, g_ListStatus);
                                    ThickHistoryInstruSave("检测测量空包指令表：线扫未完成！！！测厚已超时！");
                                    
                                }
                                break;
                        }
                        break;
                    }
                }
                connMySql.Close();

            }
            catch (Exception ee)
            {
                ThickHistoryInstruSave("检测测量空包指令表：数据库连接出问题" + ee.ToString());
            }
        }


        //写空包衬指令函数
        public void MeasEmptyLdlLin(string ITEM,string ITEMTIME,string ladleno,int ladleServduty,int ladleAge,string ladleContractor, string ThickMode,int TianChePosition)
        {
           
            MySqlConnection connMySql;
            try
            {
                if (!finishFlag)
                {
                    //定义一个mysql数据库连接对象
                    connMySql = new MySqlConnection(MySQLUser);
                    connMySql.Open();     //打开数据库
                    string CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    if (ladleno == "") 
                    {
                        ladleno = "T088";
                    }
                    string strSQL = "update measemptyldlinstr set LadleNo='" + ladleno + "',LadleServduty=" + ladleServduty + ",LadleAge=" + ladleAge + ",LadleContractor='" + ladleContractor + "',ModeType=" + ThickMode + "," + ITEM + "=1," + ITEMTIME + "='" + CurrentTime + "',ScaningStart=0,ScaningStartTime=null,ScaningEnd=0,ScaningEndTime=null,TianChePosition=" + TianChePosition+" where id =1;";
                    MySqlCommand thisCommand = new MySqlCommand(strSQL, connMySql);
                    if (!finishFlag)
                    {
                        thisCommand.ExecuteNonQuery();
                        g_ListStatus.Add(16);
                        ModifyInteract1(HandleTarget, g_ListStatus);
                        if (dtGrdViewThick.IsHandleCreated)
                        {
                            Action action1 = () =>
                            {
                                ShowTableThick("measemptyldlinstr");
                            };
                            BeginInvoke(action1);
                        }
                        ThickHistoryInstruSave("测量空表指令表：update measemptyldlinstr set LadleNo='" + ladleno + "',LadleServduty=" + ladleServduty + ",LadleAge=" + ladleAge + ",LadleContractor='" + ladleContractor + "',ModeType=" + ThickMode + "," + ITEM + "=1," + ITEMTIME + "='" + CurrentTime + "',ScaningStart=0,ScaningStartTime=null,ScaningEnd=0,ScaningEndTime=null,TianChePosition=" + TianChePosition + " where id =1;");
                    }
                    connMySql.Close();
                }
                
            }
            catch (Exception ee)
            {
                MessageBox.Show("写空包衬指令操作失败！" + ee.ToString());
            }
        }
        public  void MeasEmptyLdlLin2(string ITEM, string ITEMTIME)
        {
            MySqlConnection connMySql;
            connMySql = new MySqlConnection(MySQLUser);
            try
            {
                //定义一个mysql数据库连接对象
                if (!finishFlag)
                {
                    connMySql.Open();     //打开数据库
                    string CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string strSQL = "update measemptyldlinstr set " + ITEM + "=1 , " + ITEMTIME + "='" + CurrentTime + "' where id =1;";
                    MySqlCommand thisCommand = new MySqlCommand(strSQL, connMySql);
                    if (!finishFlag)
                    {
                        thisCommand.ExecuteNonQuery();
                        if (dtGrdViewThick.IsHandleCreated)
                        {
                            Action action1 = () =>
                            {
                                ShowTableThick("measemptyldlinstr");
                            };
                            BeginInvoke(action1);
                        }
                        ThickHistoryInstruSave("测量空包指令表：测厚启动扫描！update measemptyldlinstr set " + ITEM + "=1 , " + ITEMTIME + "='" + CurrentTime + "' where id =1;");
                        if (finishFlag) 
                        {
                            ThickHistoryInstruSave("前端退出！但已发扫描指令，测厚正在扫描！数据需要作废！！");
                        }
                    }
                    else if (finishFlag) 
                    {
                        ThickHistoryInstruSave("前端退出测厚扫描！");
                    }
                    connMySql.Close();
                }
                else if (finishFlag) 
                {
                    
                }
            }
            catch (Exception ee)
            {
                ThickHistoryInstruSave("数据库连接失败！测厚发送扫描指令失败！！");
            }
        }
        public void MeasEmptyLdlLinTianChePosi()
        {
            MySqlConnection connMySql;
            connMySql = new MySqlConnection(MySQLUser);
            try
            {
                //定义一个mysql数据库连接对象
                connMySql.Open();     //打开数据库
                string strSQL = "update measemptyldlinstr set TianChePosition=0 where id =1;";
                MySqlCommand thisCommand = new MySqlCommand(strSQL, connMySql);
                thisCommand.ExecuteNonQuery();
                ThickHistoryInstruSave("测量空包指令表：update measemptyldlinstr set TianChePosition=0 where id =1;");
                connMySql.Close();
            }
            catch (Exception ee)
            {
                MessageBox.Show("写空包衬指令操作失败！" + ee.ToString());
            }
        }
        //按键结束测厚
        private void EndLine1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确信要关闭测厚线程？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                ThickHistoryInstruSave("后台已按键关闭测厚线！");
                bStartThick = false;
                StartLine1.Enabled = true;
                EndLine1.Enabled = false;
                timertxtBxShow.Enabled = false;
            }
        }

        //按键开始监测天车
        private void BtnStartMonitCrnblk_Click(object sender, EventArgs e)
        {
            //CopyEmptyLadleNo();
            g_bMonitCrnblk = true;
            Action action = () =>
            {
                ShowTable("measheavyldlinstr");
            };
            Invoke(action);
            Thread thdMonitCrnblk = new Thread(MonitCrnblk);
            thdMonitCrnblk.Start();
 
            btnStartMonitCrnblk.Enabled = false;
            btnStopMonitCrnblk.Enabled = true;
        }
        //按键结束监测天车
        private void btnStopMonitCrnblk_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确信要关闭测温线程？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                TempHistoryInstruSave("后台已按键关闭测温线！");
                g_bMonitCrnblk = false;
                btnStartMonitCrnblk.Enabled = true;
                btnStopMonitCrnblk.Enabled = false;
            }
        }


        //监测流程
        private void MonitCrnblk()
        {
            while (g_bMonitCrnblk)
            {
                //1.赋值钢包跟踪视图
                copy_tbgz_view_ak_hctc_temp();
                //复制包龄表
                copyLadleAge();
                //2获得待测重包信息并适时发出测温指令
                GetHeavyladleToMeasInfo();
                Thread.Sleep(3000);
            }
        }

        DateTime dtCurTime104 = DateTime.Now, dtLastTime104 = DateTime.Now.AddDays(-1), dtCurTime105 = DateTime.Now, dtLastTime105 = DateTime.Now.AddDays(-1);
        private void GetHeavyladleToMeasInfo()
        {
            MySqlConnection connMySql;
            MySqlCommand thisCommand;
            string strSQL;
            try
            {
                connMySql = new MySqlConnection(MySQLUser);
                //connMySql = new MySqlConnection("user id=root;password=Lock470ml;server=localhost;persistsecurityinfo=True;database=ironldlthermodetectmanactrl");
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }

                strSQL = "SELECT X,XDeviation,Z,ladleno FROM tbgz_view_ak_hctc_temp where (id>=1 and id <=2)";        //in 1,2,3              
                thisCommand = new MySqlCommand(strSQL, connMySql);
                MySqlDataReader CopyDESCTable = thisCommand.ExecuteReader();
                int i = 0;
                double[] X = new double[2];
                double[] XDeviation = new double[2];
                double[] Z = new double[2];
                string[] ladleno = new string[2];
                while (CopyDESCTable.Read())
                {
                    X[i] = Convert.ToDouble(CopyDESCTable[0]);
                    XDeviation[i] = Convert.ToDouble(CopyDESCTable[1]);
                    Z[i] = Convert.ToDouble(CopyDESCTable[2]);
                    ladleno[i] = CopyDESCTable[3].ToString();


                    if (ladleno[i] !="")
                    {
                        //读Oracle表中实时包龄
                        sHeavyLadleNo[i] = ladleno[i];
                        iLadleAge[i] = ReadCurr_Age1(sHeavyLadleNo[i]);
                        iLadleServDuty[i] = ReadCurrLadleServDuty(sHeavyLadleNo[i]);
                        
                    }
                    else
                    {
                        sHeavyLadleNo[i] = "T000";
                        iLadleAge[i] = -1;
                        iLadleServDuty[i] = 1;
                    }
                    i++;
                }
                CopyDESCTable.Close();

                connMySql.Close();


                if (((sHeavyLadleNo[0] != sHeavyLadleNolast[0])|| sHeavyLadleNo[0]=="T000") &&(X[0] + XDeviation[0] < 513 + XDeviation[0]) && (X[0] + XDeviation[0] > 486 + XDeviation[0]) && (Z[0] > 350))
                {
                    try
                    {
                        //定义一个mysql数据库连接对象
                        connMySql = new MySqlConnection(MySQLUser);
                        connMySql.Open();     //打开数据库
                        string strSQL1 = "update heavyldl_tomeas set CrnBlkNo='HC104',LadleNo='" + sHeavyLadleNo[0] + "',LadleAge =" + iLadleAge[0] + " where SerNo = 1;";

                        MySqlCommand thisCommand1 = new MySqlCommand(strSQL1, connMySql);
                        thisCommand1.ExecuteNonQuery();
                        sHeavyLadleNolast[0] = sHeavyLadleNo[0];
                        connMySql.Close();
                        TempHistoryInstruSave("更新待测重包表：update heavyldl_tomeas set CrnBlkNo='HC104',LadleNo='" + sHeavyLadleNo[0] + "',LadleAge =" + iLadleAge[0] + " where SerNo = 1;");
                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show("更新待测重包表失败！" + ee.ToString());
                    }
                }

                if (((sHeavyLadleNo[1] != sHeavyLadleNolast[1]) || sHeavyLadleNo[1] == "T000") && (X[1] + XDeviation[1] < 490 + XDeviation[1]) && (X[1] + XDeviation[1] > 470 + XDeviation[1]) && (Z[1] > 350))
                {
                    try
                    {
                        //定义一个mysql数据库连接对象
                        connMySql = new MySqlConnection(MySQLUser);
                        connMySql.Open();     //打开数据库
                        string strSQL1 = "update heavyldl_tomeas set  CrnBlkNo='HC105',LadleNo='" + sHeavyLadleNo[1] + "',LadleAge =" + iLadleAge[1] + " where SerNo = 2;";

                        MySqlCommand thisCommand1 = new MySqlCommand(strSQL1, connMySql);
                        thisCommand1.ExecuteNonQuery();
                        sHeavyLadleNolast[1] = sHeavyLadleNo[1];
                        connMySql.Close();
                        TempHistoryInstruSave("更新待测重包表：update heavyldl_tomeas set CrnBlkNo='HC105',LadleNo='" + sHeavyLadleNo[1] + "',LadleAge =" + iLadleAge[1] + " where SerNo = 2;");
                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show("更新待测重包表失败！" + ee.ToString());
                    }
                }

                if ((X[0] + XDeviation[0] < 484 + XDeviation[0]) && (X[0] + XDeviation[0] > 474 + XDeviation[0]) && (Z[0] > 350))
                {

                    //0.变量及初始化                  
                    string CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    dtCurTime104 = DateTime.Now;

                    if (Convert.ToInt32((dtCurTime104 - dtLastTime104).TotalSeconds) > 25)
                    {
                        bMeasHeavy104 = true;
                    }
                    else 
                    {
                        bMeasHeavy104 = false;
                    }
                    dtLastTime104 = dtCurTime104;

                    //1. 写温度指令表
                    if (bMeasHeavy104)
                    {
                        NewTempMeasure(1, CurrentTime, sHeavyLadleNo[0], "HC104", iLadleServDuty[0], iLadleAge[0]);
                        

                        //显示测温表
                        Action action = () =>
                        {
                            ShowTable("measheavyldlinstr");
                        };
                        Invoke(action);
                    }
                }



                if ((X[1] + XDeviation[1] < 484 + XDeviation[1]) && (X[1] + XDeviation[1] > 474 + XDeviation[1]) && (Z[1] > 350))
                {
                    //0.变量及初始化                  
                    string CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    dtCurTime105 = DateTime.Now;

                    if (Convert.ToInt32((dtCurTime105 - dtLastTime105).TotalSeconds) > 25)
                        bMeasHeavy105 = true;
                    else
                        bMeasHeavy105 = false;

                    dtLastTime105 = dtCurTime105;

                    //1. 写温度指令表
                    if (bMeasHeavy105)
                    {
                        NewTempMeasure(2, CurrentTime, sHeavyLadleNo[1], "HC105", iLadleServDuty[1], iLadleAge[1]);

                        //显示测温表
                        Action action = () =>
                        {
                            ShowTable("measheavyldlinstr");
                        };
                        Invoke(action);
                    }
                }

                


            }
            catch (Exception ee)
            {
                MessageBox.Show("获取重包信息失败！" + ee.ToString());
            }
        }

        public  void NewTempMeasure(int serno ,string CurrentTime, string curHeavyLdlNo, string curCrnBlkNo, int LadleServDuty, int LadleAge)
        {
            MySqlConnection connMySql;
            try
            {
                //定义一个mysql数据库连接对象
                connMySql = new MySqlConnection(MySQLUser);
                connMySql.Open();     //打开数据库
                int[] rowsNumber = new int[3];
                string[] ladleno = new string[3];
                
                
                    string strSQL = "select Serno,LadleNo from measheavyldlinstr  where ReceInstrFlag=b'0';";
                    MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                    objDataAdpter.SelectCommand = new MySqlCommand(strSQL, connMySql);
                    DataSet ds = new DataSet();
                    objDataAdpter.Fill(ds, "measheavyldlinstr");
                    if (ds.Tables[0].Rows.Count != 0)
                    {
                        int rowsCount=ds.Tables[0].Rows.Count;
                        for (int i = 0; i < rowsCount; i++)
                        {
                            rowsNumber[i] = Convert.ToInt32(ds.Tables[0].Rows[i][0]);
                            ladleno[i] = ds.Tables[0].Rows[i][1].ToString();
                            if (rowsNumber[i] != serno)
                            {
                                string Instruction2 = "Update measheavyldlinstr set SendInstrTime='1988-08-08 08:08:08',LadleNo ='T000',LadleServDuty=1,LadleAge=1,ReceInstrTime='1988-08-08 08:08:08',ReceInstrFlag=1,FinSendTempTime='1988-08-08 08:08:08',FinSendTempFlag=b'0',StatusCode=b'0' where SerNo=" + rowsNumber[i] + ";";
                                MySqlCommand thisCommand1 = new MySqlCommand(Instruction2, connMySql);
                                thisCommand1.ExecuteNonQuery();
                                TempHistoryInstruSave("测温失败：第" + rowsNumber[i] + "行，重包号为" + ladleno[i] + "测温失败！；");
                            }
                        }
                    } 
                
                //发送测温指令
                string strSQL1 = "update measheavyldlinstr set sendinstrtime='" + CurrentTime + "',ladleno='" + curHeavyLdlNo + "',CrnBlkNo = '" + curCrnBlkNo +
                    "',LadleServDuty='"+LadleServDuty+"',LadleAge='" + LadleAge + "',ReceInstrFlag=b'0'  where serno='" + serno+"'";

                MySqlCommand thisCommand = new MySqlCommand(strSQL1, connMySql);
                thisCommand.ExecuteNonQuery();
                connMySql.Close();
                TempHistoryInstruSave("测温指令表：update measheavyldlinstr set sendinstrtime='" + CurrentTime + "',ladleno='" + curHeavyLdlNo + "',CrnBlkNo = '" + curCrnBlkNo +
                    "',LadleServDuty='" + LadleServDuty + "',LadleAge='" + LadleAge + "',ReceInstrFlag=b'0'  where serno='" + serno + "'");
            }
            catch (Exception ee)
            {
                MessageBox.Show("发送测温指令操作失败！" + ee.ToString());
            }

        }
        //按键启动测温线程
        private void StartMeasTempe_Click(object sender, EventArgs e)
        {
            g_start = true;
            StartMeasTempeProcess.Enabled = false;
            EndLine.Enabled = true;

            Thread thdMeasTempeProcess = new Thread(MeasTempeProcess);
            thdMeasTempeProcess.Start();
        }



        //按键结束测温线程
        private void StopMeasTempe_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确信要关闭存储线程？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                ThickHistoryInstruSave("后台已按键关闭存储线程！");
                g_start = false;
                StartMeasTempeProcess.Enabled = true;
                EndLine.Enabled = false;
                label2.Text = "";
                label3.Text = "";
            }
            
        }

        private List<string> TempHistoryInstru = new List<string>();
        private int TempHistoryInstruCotlast = 0;
        private void TempHistoryInstruSave(string tempHistoryInstru)
        {
            if ((TempHistoryInstru!=null) &&(TempHistoryInstru.Count() > 99))
            {
                string FileName = $"AppData\\TempHistoryInstr\\{DateTime.Now.Year}-{DateTime.Now.Month}";
                if (!Directory.Exists(FileName))
                {
                    Directory.CreateDirectory(FileName, null);
                }
                string strFileName = $"AppData\\TempHistoryInstr\\{DateTime.Now.Year}-{DateTime.Now.Month}\\{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Hour}-{DateTime.Now.Minute}.txt";
                //把异常信息输出到文件

                StreamWriter fs = new StreamWriter(strFileName, true);
                fs.BaseStream.Seek(0, SeekOrigin.Begin);

                for (int i = 0; i < TempHistoryInstru.Count; i++)
                {
                    fs.WriteLine(TempHistoryInstru[i]);
                    fs.Flush();
                }
                fs.Close();
                TempHistoryInstru.Clear();
            }
            TempHistoryInstru.Add("当前时间：" + DateTime.Now.ToString() + "  历史指令：" + tempHistoryInstru);
        }
        private List<string> ThickHistoryInstru = new List<string>();
        private int ThickHistoryInstruCotlast = 0;
        private void ThickHistoryInstruSave(string thickHistoryInstr)
        {
            if (ThickHistoryInstru.Count() > ThickHistoryInstruRecordCount)
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

                for (int i = 0; i < ThickHistoryInstru.Count; i++)
                {
                    fs.WriteLine(ThickHistoryInstru[i]);
                    fs.Flush();
                }
                fs.Close();
                
                ThickHistoryInstru.Clear();
            }
            ThickHistoryInstru.Add("当前时间：" + DateTime.Now.ToString() + "  历史指令：" + thickHistoryInstr);
        }

        private void TimertxtBxShow_Tick(object sender, EventArgs e)
        {
            if ((TempHistoryInstru != null)&&(TempHistoryInstru.Count != TempHistoryInstruCotlast))
            {
                for (int i = TempHistoryInstruCotlast; i < TempHistoryInstru.Count; i++)
                {
                    txtBxTempHistory.AppendText(TempHistoryInstru[i] + "\r\n");
                }
                
                TempHistoryInstruCotlast = TempHistoryInstru.Count;
            }
            if (txtBxTempHistory.Lines.Count() > 120)
            {
                txtBxTempHistory.Clear();
            }
            if ((ThickHistoryInstru != null) && (ThickHistoryInstru.Count != ThickHistoryInstruCotlast))
            {
                for (int i = ThickHistoryInstruCotlast; i< ThickHistoryInstru.Count; i++)
                {
                    txtBxThickHistoryInstru.AppendText(ThickHistoryInstru[i] + "\r\n");
                }
                ThickHistoryInstruCotlast = ThickHistoryInstru.Count;
            }

            if (txtBxThickHistoryInstru.Lines.Count() > ThickHistoryInstruRecordCount*2+1)
            {
                
                txtBxThickHistoryInstru.Clear();
            }
            MySqlConnection connMySql;
            try
            {
                connMySql = new MySqlConnection(MySQLUser);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();     //打开数据库       
                }
                string SQL5 = "update sysinteractinstr_copy1 SET controlheart=(controlheart+1)%30000 WHERE id=1;";
                MySqlCommand thisCommand1 = new MySqlCommand(SQL5, connMySql);
                thisCommand1.ExecuteNonQuery();
                //当finishflag=false时候，立马读取天车
                connMySql.Close();
            }
            catch (Exception ee)
            {
                ThickHistoryInstruSave("后台通信有问题！" );
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            asc.controlAutoSize(this);
        }

        public void copyLadleAge()
        {
            OracleConnection connOracle;
            MySqlConnection connMySql;
            try
            {
                connOracle = new OracleConnection(OracleUser);
                if (connOracle.State == ConnectionState.Closed)
                {
                    connOracle.Open();
                }
                string SQL = "SELECT  *  FROM LADLE_AGE ";
                OracleDataAdapter objDataAdpter = new OracleDataAdapter();
                objDataAdpter.SelectCommand = new OracleCommand(SQL, connOracle);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "LADLE_AGE");
                int count = ds.Tables[0].Rows.Count;
                connOracle.Close();
                connMySql = new MySqlConnection(MySQLUser);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();     //打开数据库       
                }

                MySqlTransaction tran = connMySql.BeginTransaction(IsolationLevel.ReadCommitted);
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //所有行设为修改状态
                    dr.SetModified();
                }
                //为Adapter定位目标表
                MySqlCommand cmd = new MySqlCommand(string.Format("select * from ladleage ", ds.Tables[0].TableName), connMySql, tran);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                MySqlCommandBuilder mysqlCmdBuilder = new MySqlCommandBuilder(da);
                da.AcceptChangesDuringUpdate = false;
                MySqlCommand updatecmd = new MySqlCommand(string.Format(" update ladleage SET  Ladle_Age = @Age WHERE (Ladle_ID = @ID) ", ds.Tables[0].TableName));
                //MySqlCommand updatecmd = new MySqlCommand(string.Format(" update ladleage_copy1 SET  LadleAge = @Age,Ladle_ID = @ID ", ds.Tables[0].TableName));

                //不修改源DataTable
                updatecmd.UpdatedRowSource = UpdateRowSource.None;
                da.UpdateCommand = updatecmd;
                da.UpdateCommand.Parameters.Add("@Age", MySqlDbType.Int32, 10, ds.Tables[0].Columns[1].ColumnName);
                da.UpdateCommand.Parameters.Add("@ID", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[0].ColumnName);
                da.UpdateBatchSize = 100;
                da.Update(ds.Tables[0]);
                ds.AcceptChanges();
                tran.Commit();
                connMySql.Close();
            }
            catch (Exception ee)
            {
                MessageBox.Show("复制包龄表失败" + ee.ToString());
            }
        }
        public void copy_tbgz_view_ak_hctc_temp()
        {
            SqlConnection connSql;
            connSql = new SqlConnection(SQLServerUser);
            MySqlConnection connMySql;
            try
            {
           
                //connSql = new SqlConnection(SQLServerUser);//
                if (connSql.State == ConnectionState.Closed)
                {
                    connSql.Open();
                }
                string SQL = "SELECT  *  FROM TBGZ_View_AK_HCTC ";
                SqlDataAdapter objDataAdpter = new SqlDataAdapter();
                objDataAdpter.SelectCommand = new SqlCommand(SQL, connSql);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "TBGZ_View_AK_HCTC");
                int count = ds.Tables[0].Rows.Count;
                connSql.Close();
                connMySql = new MySqlConnection(MySQLUser);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();     //打开数据库       
                }

                MySqlTransaction tran = connMySql.BeginTransaction(IsolationLevel.ReadCommitted);
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //所有行设为修改状态
                    dr.SetModified();
                }
                //为Adapter定位目标表
                MySqlCommand cmd = new MySqlCommand(string.Format("select * from tbgz_view_ak_hctc_temp ", ds.Tables[0].TableName), connMySql, tran);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                MySqlCommandBuilder mysqlCmdBuilder = new MySqlCommandBuilder(da);
                da.AcceptChangesDuringUpdate = false;
                MySqlCommand updatecmd = new MySqlCommand(string.Format(" update tbgz_view_ak_hctc_temp SET Name=@Name,Distance=@Distance,X=@X,Y=@Y,Z=@Z,XSource=@XSource,YSource=@YSource,BelongCross=@BelongCross,GetTime=@GetTime,LadleNo=@LadleNo,XDeviation=@XDeviation,YDeviation=@YDeviation,State=@State,RackCode=@RackCode WHERE (Code = @Code) ", ds.Tables[0].TableName));
                //MySqlCommand updatecmd = new MySqlCommand(string.Format(" update ladleage_copy1 SET  LadleAge = @Age,Ladle_ID = @ID ", ds.Tables[0].TableName));

                //不修改源DataTable
                updatecmd.UpdatedRowSource = UpdateRowSource.None;
                da.UpdateCommand = updatecmd;
                da.UpdateCommand.Parameters.Add("@Code", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[0].ColumnName);
                da.UpdateCommand.Parameters.Add("@Name", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[1].ColumnName);
                da.UpdateCommand.Parameters.Add("@Distance", MySqlDbType.Float, 30, ds.Tables[0].Columns[2].ColumnName);
                da.UpdateCommand.Parameters.Add("@X", MySqlDbType.Float, 10, ds.Tables[0].Columns[3].ColumnName);
                da.UpdateCommand.Parameters.Add("@Y", MySqlDbType.Float, 10, ds.Tables[0].Columns[4].ColumnName);
                da.UpdateCommand.Parameters.Add("@Z", MySqlDbType.Float, 10, ds.Tables[0].Columns[5].ColumnName);
                da.UpdateCommand.Parameters.Add("@XSource", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[6].ColumnName);
                da.UpdateCommand.Parameters.Add("@YSource", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[7].ColumnName);
                da.UpdateCommand.Parameters.Add("@BelongCross", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[8].ColumnName);
                da.UpdateCommand.Parameters.Add("@GetTime", MySqlDbType.DateTime, 30, ds.Tables[0].Columns[9].ColumnName);
                da.UpdateCommand.Parameters.Add("@LadleNo", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[10].ColumnName);
                da.UpdateCommand.Parameters.Add("@XDeviation", MySqlDbType.Float, 10, ds.Tables[0].Columns[11].ColumnName);
                da.UpdateCommand.Parameters.Add("@YDeviation", MySqlDbType.Float, 10, ds.Tables[0].Columns[12].ColumnName);
                da.UpdateCommand.Parameters.Add("@State", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[13].ColumnName);
                da.UpdateCommand.Parameters.Add("@RackCode", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[14].ColumnName);
                da.UpdateBatchSize = 10000;
                da.Update(ds.Tables[0]);
                ds.AcceptChanges();
                tran.Commit();
                connMySql.Close();
                if (gbgzTemprecordFlag == 0)
                {
                    TempHistoryInstruSave("测温：复制钢包视图恢复！");
                }
                gbgzTemprecordFlag = 1;

            }
            catch (Exception ee)
            {
                if (connSql.State == ConnectionState.Open)
                {
                    connSql.Close();
                }
                if (gbgzTemprecordFlag == 1)
                {
                    TempHistoryInstruSave("测温：复制钢包视图失败！");
                }
                gbgzTemprecordFlag = 0;
                //MessageBox.Show("复制钢包视图失败" + ee.ToString());
            }
            finally
            {
                connSql.Close();

            }
        }
        
        public void copy_tbgz_view_ak_hctc()
        {
            SqlConnection connSql;
            MySqlConnection connMySql;
            bool b_111 = false, b_112 = false, b_127 = false;
            string str_gbgz="b'1'";
            try
            {
                connSql = new SqlConnection(SQLServerUser);//
                if (connSql.State == ConnectionState.Closed)
                {
                    connSql.Open();
                }
                string SQL = "SELECT  *  FROM TBGZ_View_AK_HCTC ";
                SqlDataAdapter objDataAdpter = new SqlDataAdapter();
                objDataAdpter.SelectCommand = new SqlCommand(SQL, connSql);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "TBGZ_View_AK_HCTC");
                connSql.Close();
                int count = ds.Tables[0].Rows.Count;
               // DateTime 111_time = Convert.ToDateTime(ds.Tables[0].Rows[3][9]);
               // DateTime 112_time = Convert.ToDateTime(ds.Tables[0].Rows[4][9]);
               // DateTime 127_time = Convert.ToDateTime(ds.Tables[0].Rows[6][9]);
                b_111=(Convert.ToInt32((DateTime.Now - Convert.ToDateTime(ds.Tables[0].Rows[3][9])).TotalSeconds) <= 10) ? true : false;
                b_112 = (Convert.ToInt32((DateTime.Now - Convert.ToDateTime(ds.Tables[0].Rows[4][9])).TotalSeconds) <= 10) ? true : false;
                b_127 =(Convert.ToInt32((DateTime.Now - Convert.ToDateTime(ds.Tables[0].Rows[6][9])).TotalSeconds) <= 10) ? true : false;
                if (b_111 && b_127)
                {
                    str_gbgz="b'0'";
                }
                else
                {
                    str_gbgz="b'1'";
                }
                connMySql = new MySqlConnection(MySQLUser);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();     //打开数据库       
                }
                MySqlTransaction tran = connMySql.BeginTransaction(IsolationLevel.ReadCommitted);
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //所有行设为修改状态
                    dr.SetModified();
                }
                //为Adapter定位目标表
                MySqlCommand cmd = new MySqlCommand(string.Format("select * from tbgz_view_ak_hctc2", ds.Tables[0].TableName), connMySql, tran);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                MySqlCommandBuilder mysqlCmdBuilder = new MySqlCommandBuilder(da);
                da.AcceptChangesDuringUpdate = false;
                MySqlCommand updatecmd = new MySqlCommand(string.Format(" update tbgz_view_ak_hctc2 SET Name=@Name,Distance=@Distance,X=@X,Y=@Y,Z=@Z,XSource=@XSource,YSource=@YSource,BelongCross=@BelongCross,GetTime=@GetTime,LadleNo=@LadleNo,XDeviation=@XDeviation,YDeviation=@YDeviation,State=@State,RackCode=@RackCode WHERE (Code = @Code) ", ds.Tables[0].TableName));
                //MySqlCommand updatecmd = new MySqlCommand(string.Format(" update ladleage_copy1 SET  LadleAge = @Age,Ladle_ID = @ID ", ds.Tables[0].TableName));

                //不修改源DataTable
                updatecmd.UpdatedRowSource = UpdateRowSource.None;
                da.UpdateCommand = updatecmd;
                da.UpdateCommand.Parameters.Add("@Code", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[0].ColumnName);
                da.UpdateCommand.Parameters.Add("@Name", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[1].ColumnName);
                da.UpdateCommand.Parameters.Add("@Distance", MySqlDbType.Float, 30, ds.Tables[0].Columns[2].ColumnName);
                da.UpdateCommand.Parameters.Add("@X", MySqlDbType.Float, 10, ds.Tables[0].Columns[3].ColumnName);
                da.UpdateCommand.Parameters.Add("@Y", MySqlDbType.Float, 10, ds.Tables[0].Columns[4].ColumnName);
                da.UpdateCommand.Parameters.Add("@Z", MySqlDbType.Float, 10, ds.Tables[0].Columns[5].ColumnName);
                da.UpdateCommand.Parameters.Add("@XSource", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[6].ColumnName);
                da.UpdateCommand.Parameters.Add("@YSource", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[7].ColumnName);
                da.UpdateCommand.Parameters.Add("@BelongCross", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[8].ColumnName);
                da.UpdateCommand.Parameters.Add("@GetTime", MySqlDbType.DateTime, 30, ds.Tables[0].Columns[9].ColumnName);
                da.UpdateCommand.Parameters.Add("@LadleNo", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[10].ColumnName);
                da.UpdateCommand.Parameters.Add("@XDeviation", MySqlDbType.Float, 10, ds.Tables[0].Columns[11].ColumnName);
                da.UpdateCommand.Parameters.Add("@YDeviation", MySqlDbType.Float, 10, ds.Tables[0].Columns[12].ColumnName);
                da.UpdateCommand.Parameters.Add("@State", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[13].ColumnName);
                da.UpdateCommand.Parameters.Add("@RackCode", MySqlDbType.VarChar, 30, ds.Tables[0].Columns[14].ColumnName);
                da.UpdateBatchSize = 10000;
                da.Update(ds.Tables[0]);
                ds.AcceptChanges();
                tran.Commit();
                string SQL1 = "UPDATE `sysinteractinstr_copy1` SET gbgzstatus="+str_gbgz+" WHERE id=1;";
                MySqlCommand thisCommand1 = new MySqlCommand(SQL1, connMySql);
                thisCommand1.ExecuteNonQuery();
                connMySql.Close();
                if(gbgzThickrecordFlag == 0)
                {
                    ThickHistoryInstruSave("测温：复制钢包视图恢复！");
                }
                gbgzThickrecordFlag = 1;
            }
            catch (Exception ee)
            {
                connMySql = new MySqlConnection(MySQLUser);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();     //打开数据库       
                }
                string SQL1 = "UPDATE `sysinteractinstr_copy1` SET gbgzstatus=" + str_gbgz + " WHERE id=1;";
                MySqlCommand thisCommand1 = new MySqlCommand(SQL1, connMySql);
                thisCommand1.ExecuteNonQuery();
                connMySql.Close();
                if (gbgzThickrecordFlag == 1)
                {
                    ThickHistoryInstruSave("测厚：复制钢包视图失败！");
                }
                gbgzThickrecordFlag = 0;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dr;
            dr = MessageBox.Show("确认退出吗", "确认对话框", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.Yes)
            {
                ThickHistoryInstruSave("关闭总软件！！！！");
                if (TempHistoryInstru != null)
                {
                    string FileName = $"AppData\\TempHistoryInstr\\{DateTime.Now.Year}-{DateTime.Now.Month}";
                    if (!Directory.Exists(FileName))
                    {
                        Directory.CreateDirectory(FileName, null);
                    }
                    string strFileName = $"AppData\\TempHistoryInstr\\{DateTime.Now.Year}-{DateTime.Now.Month}\\{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Hour}-{DateTime.Now.Minute}.txt";
                    //把异常信息输出到文件

                    StreamWriter fs = new StreamWriter(strFileName, true);
                    fs.BaseStream.Seek(0, SeekOrigin.Begin);

                    for (int i = 0; i < TempHistoryInstru.Count; i++)
                    {
                        fs.WriteLine(TempHistoryInstru[i]);
                        fs.Flush();
                    }
                    fs.Close();
                    TempHistoryInstru.Clear();
                }


                if (ThickHistoryInstru != null)
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

                    for (int i = 0; i < ThickHistoryInstru.Count; i++)
                    {
                        fs.WriteLine(ThickHistoryInstru[i]);
                        fs.Flush();
                    }
                    fs.Close();
                    ThickHistoryInstru.Clear();
                }

                Environment.Exit(0);
            }
            else
            {
                e.Cancel = true;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            asc.controllInitializeSize(this);
            MySqlConnection connMySql;
            try
            {
                connMySql = new MySqlConnection(MySQLUser);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }
                string SQL1 = "select Heart_PLC FROM crnblkcurpos where id=1; ";
                MySqlCommand thisCommand1 = new MySqlCommand(SQL1, connMySql);
                if ((thisCommand1.ExecuteScalar()) != System.DBNull.Value)
                {
                    tiancheHeartLast = Convert.ToInt32(thisCommand1.ExecuteScalar());
                }
                string SQL2 = "select headheart from  sysinteractinstr_copy1 where id =1;";
                MySqlCommand thisCommand2 = new MySqlCommand(SQL2, connMySql);
                if ((thisCommand2.ExecuteScalar()) != System.DBNull.Value)
                {
                    headHeartLast = Convert.ToInt32(thisCommand2.ExecuteScalar());
                }
                string SQL3 = "select Heartbeat from measheartbeat where ID=1;";
                MySqlCommand thisCommand3 = new MySqlCommand(SQL3, connMySql);
                if ((thisCommand3.ExecuteScalar()) != System.DBNull.Value)
                {
                    thickHeartLast = Convert.ToInt32(thisCommand3.ExecuteScalar());
                }
                //是否初始化各种表
                connMySql.Close();
            }
            catch (Exception ee)
            {
                MessageBox.Show("后台初始化读取天车心跳、测厚心跳、前端心跳出错"+ee.ToString());
            }
        }
        private struct Measheavyld
        {
            public int SerNo;
            public string SendInstrTime;
            public string LadleNo;
            public string CrnBlkNo;
            public int LadleServDuty;
            public int LadleAge;
            public string LadleContrator;
            public string FinSendTempTime;
        }

        private struct Measemptyld
        {
            public string LadleNo;
            public int LadleServDuty;
            public int LadleAge;
            public string DataExist;
            public short StatusCode;
            public DateTime MeasTime;
        }
        Int16[] data1 = new Int16[640 * 480];
        Int16[] data2 = new Int16[640 * 480];
        Int16[] data3 = new Int16[640 * 480];
        Int16[] data4 = new Int16[640 * 480];
        bool[] bDrawTemp = new bool[4] { false, false, false, false };
        bool[] bDrawThick = new bool[2] { false, false };
        private DateTime time;

        //测温流程
        private void MeasTempeProcess()
        {
            MySqlConnection connMySql;
            while (g_start)
            {
                connMySql = new MySqlConnection(MySQLUser);
                if (connMySql.State == ConnectionState.Closed)
                {
                    connMySql.Open();
                }
                string Instr = "select Serno,SendInstrTime,LadleNo from measheavyldlinstr where  ReceInstrFlag=b'0';";
                MySqlDataAdapter objDtAdpt = new MySqlDataAdapter();
                objDtAdpt.SelectCommand = new MySqlCommand(Instr, connMySql);
                DataSet datatable = new DataSet();
                objDtAdpt.Fill(datatable, "measheavyldlinstr");
                if (datatable.Tables[0].Rows.Count>0)
                {
                    if (Convert.ToInt32((DateTime.Now - Convert.ToDateTime(datatable.Tables[0].Rows[0][1])).TotalSeconds) > 30) 
                    {
                        int Serno_temp = Convert.ToInt32(datatable.Tables[0].Rows[0][0]);
                        string LadleNo_temp = datatable.Tables[0].Rows[0][2].ToString();
                        string Instr2 = "Update measheavyldlinstr set SendInstrTime='1988-08-08 08:08:08',LadleNo ='T000',LadleServDuty=1,LadleAge=1,ReceInstrTime='1988-08-08 08:08:08',ReceInstrFlag=b'1',FinSendTempTime='1988-08-08 08:08:08',FinSendTempFlag=b'0',StatusCode=b'0' where SerNo=" + Serno_temp + "";
                        MySqlCommand thisCod = new MySqlCommand(Instr2, connMySql);
                        thisCod.ExecuteNonQuery();
                        TempHistoryInstruSave("测温指令表：重包包号为'" + LadleNo_temp + "'测温失败，且将此行初始化！");
                        if (dataGridView1.IsHandleCreated)
                        {
                            Action action = () =>
                            {
                                ShowTable("measheavyldlinstr");
                            };
                            BeginInvoke(action);
                        }
                    }
                }
                Measheavyld a = new Measheavyld();
                string Instruction1 = "select SerNo,LadleNo,CrnBlkNo,LadleServDuty,LadleAge from measheavyldlinstr where  FinSendTempFlag=b'1'";
                MySqlDataAdapter objDataAdpter = new MySqlDataAdapter();
                objDataAdpter.SelectCommand = new MySqlCommand(Instruction1, connMySql);
                DataSet ds = new DataSet();
                objDataAdpter.Fill(ds, "measheavyldlinstr");
                if (ds.Tables[0].Rows.Count != 0)
                {
                    a.SerNo = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                    a.LadleNo = ds.Tables[0].Rows[0][1].ToString();
                    a.CrnBlkNo = ds.Tables[0].Rows[0][2].ToString();
                    if (ds.Tables[0].Rows[0][3] == System.DBNull.Value)
                    {
                        a.LadleServDuty = 0;
                    }
                    else
                    {
                        a.LadleServDuty = Convert.ToInt32(ds.Tables[0].Rows[0][3]);
                    }
                    if (ds.Tables[0].Rows[0][4] == System.DBNull.Value)
                    {
                        a.LadleAge = 0;
                    }
                    else
                    {
                        a.LadleAge = Convert.ToInt32(ds.Tables[0].Rows[0][4]);
                    }
                    
                    if (connMySql.State == ConnectionState.Closed)
                    {
                        connMySql.Open();
                    }
                    //string InstrLdlServDuty = "select LadleServDuty from; ";
                    string Instruction2 = "Update measheavyldlinstr set SendInstrTime='1988-08-08 08:08:08',LadleNo ='T000',LadleServDuty=1,LadleAge=1,ReceInstrTime='1988-08-08 08:08:08',ReceInstrFlag=b'1',FinSendTempTime='1988-08-08 08:08:08',FinSendTempFlag=b'0',StatusCode=b'0' where SerNo=" + a.SerNo + "";
                    MySqlCommand thisCommand = new MySqlCommand(Instruction2, connMySql);
                    thisCommand.ExecuteNonQuery();
                    TempHistoryInstruSave("测温指令表：已收到包号为'" + a.LadleNo + "'的收指指令，且将此行初始化！");
                    if (dataGridView1.IsHandleCreated)
                    {
                        Action action = () =>
                        {
                            ShowTable("measheavyldlinstr");
                        };
                        BeginInvoke(action);
                    }
                    List<int> lalno = new List<int>();
                    //检查对比包号
                    string SQL_predict = "select CircumTemp1,CircumTemp2,CircumTemp3,CircumTemp4 from tmptempe where id = " + a.SerNo + ";";
                    MySqlCommand cmd_predict = new MySqlCommand(SQL_predict, connMySql);
                    MySqlDataAdapter objDataAdpter_predict = new MySqlDataAdapter();
                    objDataAdpter_predict.SelectCommand = new MySqlCommand(SQL_predict, connMySql);
                    DataSet ds_predict = new DataSet();
                    objDataAdpter_predict.Fill(ds_predict, "tmptempe");
                    MySqlDataReader rdr = cmd_predict.ExecuteReader();
                    while (rdr.Read())
                    {
                        bDrawTemp[0] = true; bDrawTemp[1] = true; bDrawTemp[2] = true; bDrawTemp[3] = true;
                        if (ds_predict.Tables[0].Rows[0][0] == System.DBNull.Value)
                        {
                            bDrawTemp[0] = false;
                        }
                        if (ds_predict.Tables[0].Rows[0][1] == System.DBNull.Value)
                        {
                            bDrawTemp[1] = false;
                        }
                        if (ds_predict.Tables[0].Rows[0][2] == System.DBNull.Value)
                        {
                            bDrawTemp[2] = false;
                        }
                        if (ds_predict.Tables[0].Rows[0][3] == System.DBNull.Value)
                        {
                            bDrawTemp[3] = false;
                        }
                        
                        long len;
                        int inlen = 640 * 480 * 2;
                        byte[] buffer = new byte[inlen];
                        if (bDrawTemp[0])
                        {
                            //存放获得的二进制数据，温度
                            len = rdr.GetBytes(0, 0, buffer, 0, inlen);

                            for (int i = 0; i < len / 2; i++)
                            {
                                data1[i] = (Int16)((buffer[2 * i]) + (buffer[2 * i + 1]) * 256);
                                //获得字节数组
                                
                            }
                        }

                        if (bDrawTemp[1])
                        {
                            len = rdr.GetBytes(1, 0, buffer, 0, inlen);
                            for (int i = 0; i < len / 2; i++)
                            {
                                data2[i] = (Int16)((buffer[2 * i]) + (buffer[2 * i + 1]) * 256);
                                
                            }

                        }

                        if (bDrawTemp[2])
                        {
                            len = rdr.GetBytes(2, 0, buffer, 0, inlen);
                            for (int i = 0; i < len / 2; i++)
                            {
                                data3[i] = (Int16)((buffer[2 * i]) + (buffer[2 * i + 1]) * 256);
                                
                            }
                        }
                        if (bDrawTemp[3])
                        {
                            len = rdr.GetBytes(3, 0, buffer, 0, inlen);

                            for (int i = 0; i < len / 2; i++)
                            {
                                data4[i] = (Int16)((buffer[2 * i]) + (buffer[2 * i + 1]) * 256);
                               
                            }

                        }
                    }
                    rdr.Close();
                    lalno.Clear();
                    if (bDrawTemp[0])
                    {
                        Bitmap image1 = BlobToBytes.GrayImage(data1, 480, 640);
                        lalno.Add(PaddleXPredict.Inference(image1));
                    }
                    if (bDrawTemp[1])
                    {
                        Bitmap image2 = BlobToBytes.GrayImage(data2, 480, 640);
                        lalno.Add(PaddleXPredict.Inference(image2));
                    }
                    if (bDrawTemp[2])
                    {
                        Bitmap image3 = BlobToBytes.GrayImage(data3, 480, 640);
                        lalno.Add(PaddleXPredict.Inference(image3));
                    }
                    if (bDrawTemp[3])
                    {
                        Bitmap image4 = BlobToBytes.GrayImage(data4, 480, 640);
                        lalno.Add(PaddleXPredict.Inference(image4));
                    }
                    if (lalno.Count > 0)
                    {
                        string lalno_predict = LadleNo(lalno);
                        if (!String.Equals(lalno_predict, "T000"))
                        {
                            if (String.Equals(lalno_predict, a.LadleNo))
                            {
                                a.LadleAge = ReadCurr_Age2(a.LadleNo);
                                a.LadleServDuty = ReadCurrLadleServDuty(a.LadleNo);
                                a.LadleContrator = ReadCurrLadleContractor(a.LadleNo);
                                //存数据
                                string strSQL1 = "insert into heavyldl_meased(MaxTemp, MaxTempPos,MaxTempImagerNo,MaxTempXbyPixel,MaxTempYbyPixel,CircumExpandTemp, BottomExpandTemp, CircumTemp1, CircumTemp2, CircumTemp3, CircumTemp4, BottomTemp, StatusCode,MeasTm)" +
                                 "select MaxTemp,MaxTempPos,MaxTempImagerNo,MaxTempXbyPixel,MaxTempYbyPixel,CircumExpandTemp,BottomExpandTemp,CircumTemp1,CircumTemp2,CircumTemp3,CircumTemp4,BottomTemp,StatusCode,UpdateTime from tmptempe where id = " + a.SerNo + "; "; /*+
                                "update heavyldl_meased_copy1 set LadleNo=" + a.LadleNo + ",LadleServDuty=" + a.LadleServDuty + ",LadleAge=" + a.LadleAge + ",CrnBlkNo=" +a.CrnBlkNo + ",MeasTm=" + a.FinSendTempTime + " ORDER BY id DESC LIMIT 1;";*/
                                MySqlCommand thisCommand1 = new MySqlCommand(strSQL1, connMySql);
                                thisCommand1.ExecuteNonQuery();
                                string strSQL2 = "update heavyldl_meased set LadleNo='" + a.LadleNo + "',LadleServDuty=" + a.LadleServDuty + ",LadleAge=" + a.LadleAge + ",CrnBlkNo='" + a.CrnBlkNo + "',LadleContractor='" + a.LadleContrator + "' ORDER BY id DESC LIMIT 1;";
                                MySqlCommand thisCommand2 = new MySqlCommand(strSQL2, connMySql);
                                thisCommand2.ExecuteNonQuery();
                                TempHistoryInstruSave("已测重包表：模型识别与指标中相同，将包号为'" + a.LadleNo + "'的数据存储到已测重包表！");
                            }
                            else
                            {
                                a.LadleNo = LadleNo(lalno);
                                //得到包役号
                                a.LadleAge= ReadCurr_Age2(a.LadleNo);
                                a.LadleServDuty = ReadCurrLadleServDuty(a.LadleNo);
                                a.LadleContrator = ReadCurrLadleContractor(a.LadleNo);
                                //存数据
                                string strSQL1 = "insert into heavyldl_meased(MaxTemp, MaxTempPos,MaxTempImagerNo,MaxTempXbyPixel,MaxTempYbyPixel,CircumExpandTemp, BottomExpandTemp, CircumTemp1, CircumTemp2, CircumTemp3, CircumTemp4, BottomTemp, StatusCode,MeasTm)" +
                                 "select MaxTemp,MaxTempPos,MaxTempImagerNo,MaxTempXbyPixel,MaxTempYbyPixel,CircumExpandTemp,BottomExpandTemp,CircumTemp1,CircumTemp2,CircumTemp3,CircumTemp4,BottomTemp,StatusCode,UpdateTime from tmptempe where id = " + a.SerNo + "; "; /*+
                                "update heavyldl_meased_copy1 set LadleNo=" + a.LadleNo + ",LadleServDuty=" + a.LadleServDuty + ",LadleAge=" + a.LadleAge + ",CrnBlkNo=" +a.CrnBlkNo + ",MeasTm=" + a.FinSendTempTime + " ORDER BY id DESC LIMIT 1;";*/
                                MySqlCommand thisCommand1 = new MySqlCommand(strSQL1, connMySql);
                                thisCommand1.ExecuteNonQuery();
                                string strSQL2 = "update heavyldl_meased set LadleNo='" + a.LadleNo + "',LadleServDuty=" + a.LadleServDuty + ",LadleAge=" + a.LadleAge + ",CrnBlkNo='" + a.CrnBlkNo + "',LadleContractor='" + a.LadleContrator + "' ORDER BY id DESC LIMIT 1;";
                                MySqlCommand thisCommand2 = new MySqlCommand(strSQL2, connMySql);
                                thisCommand2.ExecuteNonQuery();
                                TempHistoryInstruSave("已测重包表：模型识别与指令表中不同，将包号为'" + a.LadleNo + "'的数据存储到已测重包表！");
                            }

                        }
                        else
                        {
                            TempHistoryInstruSave("已测重包表：将包号为'" + a.LadleNo + "'的数据舍弃！");
                        }
                    }
                    else
                    {
                        a.LadleAge=ReadCurr_Age2(a.LadleNo);
                        a.LadleServDuty = ReadCurrLadleServDuty(a.LadleNo);

                        a.LadleContrator = ReadCurrLadleContractor(a.LadleNo);
                        //存数据
                        string strSQL1 = "insert into heavyldl_meased(MaxTemp, MaxTempPos,MaxTempImagerNo,MaxTempXbyPixel,MaxTempYbyPixel,CircumExpandTemp, BottomExpandTemp, CircumTemp1, CircumTemp2, CircumTemp3, CircumTemp4, BottomTemp, StatusCode,MeasTm)" +
                         "select MaxTemp,MaxTempPos,MaxTempImagerNo,MaxTempXbyPixel,MaxTempYbyPixel,CircumExpandTemp,BottomExpandTemp,CircumTemp1,CircumTemp2,CircumTemp3,CircumTemp4,BottomTemp,StatusCode,UpdateTime from tmptempe where id = " + a.SerNo + "; "; /*+
                        "update heavyldl_meased_copy1 set LadleNo=" + a.LadleNo + ",LadleServDuty=" + a.LadleServDuty + ",LadleAge=" + a.LadleAge + ",CrnBlkNo=" +a.CrnBlkNo + ",MeasTm=" + a.FinSendTempTime + " ORDER BY id DESC LIMIT 1;";*/
                        MySqlCommand thisCommand1 = new MySqlCommand(strSQL1, connMySql);
                        thisCommand1.ExecuteNonQuery();
                        string strSQL2 = "update heavyldl_meased set LadleNo='" + a.LadleNo + "',LadleServDuty=" + a.LadleServDuty + ",LadleAge=" + a.LadleAge + ",CrnBlkNo='" + a.CrnBlkNo + "',LadleContractor='" + a.LadleContrator + "' ORDER BY id DESC LIMIT 1;";
                        MySqlCommand thisCommand2 = new MySqlCommand(strSQL2, connMySql);
                        thisCommand2.ExecuteNonQuery();
                        TempHistoryInstruSave("已测重包表:将包号为'" + a.LadleNo + "'的数据存储到已测重包表！");
                    }
                }
                //查测厚Datatransform置为1
                Measemptyld thickdata = new Measemptyld();
                string SQL = "select LadleNo,CircumThick,BottomThick,StatusCode,scaningEndTime from tmpthick order by id limit 1;";
                MySqlDataAdapter objDataAdpter1 = new MySqlDataAdapter();
                objDataAdpter1.SelectCommand = new MySqlCommand(SQL, connMySql);
                DataSet ds1 = new DataSet();
                objDataAdpter1.Fill(ds1, "tmpthick");
                string MaxErodeThick = "",MaxErodeThickPos="";
                int MaxErodeThickCir = -10000, MaxErodeThickBot = 0;
                Int16 ThickErodeCir,ThickErodeBot;
                if (ds1.Tables[0].Rows.Count > 0)
                {
                    thickdata.LadleNo = ds1.Tables[0].Rows[0][0].ToString();
                    thickdata.StatusCode = Convert.ToInt16(ds1.Tables[0].Rows[0][3]);
                    thickdata.MeasTime = Convert.ToDateTime(ds1.Tables[0].Rows[0][4]);
                    bDrawThick[0] = true; bDrawThick[1] = true;
                    if (ds1.Tables[0].Rows[0][1] == System.DBNull.Value)
                    {
                        bDrawThick[0] = false;
                    }
                    if (ds1.Tables[0].Rows[0][2] == System.DBNull.Value)
                    {
                        bDrawThick[1] = false;
                    }
                    if (bDrawThick[0])
                    {
                        //long len;
                        int inlen = ThickCirRows * ThickCirCols * 2;
                        byte[] buffer = new byte[inlen];
                        buffer = (byte[])ds1.Tables[0].Rows[0][1];
                        for (int i = 0; i < inlen / 2; i++)
                        {
                            dataThickCir[i] = (Int16)((buffer[2 * i]) + (buffer[2 * i + 1]) * 256);

                        }
                        for (int X = 0; X < ThickCirRows; X++)  //  270行
                        {
                            for (int Y = 0; Y < ThickCirCols; Y++)  //360列
                            {
                                ThickErodeCir = dataThickCir[X * ThickCirCols + Y];
                                if ((X >= 100) && (ThickErodeCir != 404))
                                {
                                    if (MaxErodeThickCir < ThickErodeCir)
                                    {
                                        MaxErodeThickCir = ThickErodeCir;
                                    }
                                }
                            }
                        }
                    }

                    if (bDrawThick[1])
                    {

                        int inlen1 = ThickBotRows * ThickBotCols * 2;
                        byte[] buffer1 = new byte[inlen1];
                        buffer1 = (byte[])ds1.Tables[0].Rows[0][2];
                        //存放获得的二进制数据，温度

                        for (int i = 0; i < inlen1 / 2; i++)
                        {
                            dataThickBot[i] = (Int16)((buffer1[2 * i]) + (buffer1[2 * i + 1]) * 256);
                        }
                        for (int X = 0; X < ThickBotRows; X++)  //  270行
                        {
                            for (int Y = 0; Y < ThickBotCols; Y++)  //360列
                            {
                                ThickErodeBot = dataThickBot[X * ThickBotCols + Y];
                                if ((MaxErodeThickBot < ThickErodeBot) && (ThickErodeBot != 404))
                                {
                                    MaxErodeThickBot = ThickErodeBot;
                                }
                            }
                        }
                    }
                    if (bDrawThick[0] && bDrawThick[1])
                    {
                        if (MaxErodeThickBot >= MaxErodeThickCir)
                        {
                            MaxErodeThick = MaxErodeThickBot.ToString();
                            MaxErodeThickPos = "底部";
                        }
                        else
                        {
                            MaxErodeThick = MaxErodeThickCir.ToString();
                            MaxErodeThickPos = "周壁";
                        }
                        thickdata.DataExist = "b'0'";
                    }
                    else
                    {
                        MaxErodeThick ="无数据";
                        MaxErodeThickPos = "无数据";
                        thickdata.DataExist = "b'1'";
                    }
                    if (connMySql.State == ConnectionState.Closed)
                    {
                        connMySql.Open();
                    }
                    string SQL6 = "Update tmpthick SET MinThick='" + MaxErodeThick + "',MinThickPos='" + MaxErodeThickPos + "' order by id limit 1;";
                    MySqlCommand thisCommand6 = new MySqlCommand(SQL6, connMySql);
                    thisCommand6.ExecuteNonQuery();
                    string SQL2 = "INSERT into emptyldl_meased(LadleNo,LadleAge,LadleServDuty,LadleContractor,ModeType,Meastm,MinThick,MinThickPos, CircumThick, BottomThick, ThickPhoto,StatusCode) (SELECT ladleno,ladleAge,ladleServDuty,ladleContractor,ModeType,scaningEndTime,MinThick, MinThickPos,CircumThick, BottomThick, CameraPhoto,StatusCode FROM tmpthick order by id limit 0,1);";
                    MySqlCommand thisCommand3 = new MySqlCommand(SQL2, connMySql);
                    thisCommand3.ExecuteNonQuery();
                    string SQL_RealAge = "Select Ladle_Age from ladleage where Ladle_ID='"+ thickdata.LadleNo + "';";
                    MySqlCommand thisCommand_RealAge = new MySqlCommand(SQL_RealAge, connMySql);
                    object age_obj = thisCommand_RealAge.ExecuteScalar();
                    if ((age_obj == null) || (age_obj == DBNull.Value))
                    {
                        EmptylastLadleAgeReal = 0;
                    }
                    else
                    {
                        EmptylastLadleAgeReal = Convert.ToInt32(thisCommand_RealAge.ExecuteScalar());
                    }
                    string SQL3 = "Update emptyldl_meased SET LadleAgeReal="+ EmptylastLadleAgeReal + " order by id DESC limit 1;";
                    MySqlCommand thisCommand4 = new MySqlCommand(SQL3, connMySql);
                    thisCommand4.ExecuteNonQuery();
                    string SQL8 = "update `ladleaimball` set StatusCode=" + thickdata.StatusCode + ",MeasTime='" + thickdata.MeasTime + "',DataExist=" + thickdata.DataExist + " WHERE LadleNo_Ball='" + thickdata.LadleNo + "';";
                    MySqlCommand thisCommand8 = new MySqlCommand(SQL8, connMySql);
                    thisCommand8.ExecuteNonQuery();
                    string SQL4 = "delete from tmpthick order by id limit 1;";
                    MySqlCommand thisCommand5 = new MySqlCommand(SQL4, connMySql);
                    thisCommand5.ExecuteNonQuery();
                    ThickHistoryInstruSave("已测空包表：将空包号为'"+ thickdata.LadleNo + "'的测厚数据已存到已测空包表！");
                    
                }
                connMySql.Close();
                Thread.Sleep(20000);
            }        
        }
        /// <summary>
        /// 找出arry中数量最多的元素
        /// </summary>
        /// <param name="arry"></param>
        /// <returns></returns>
        private static string  LadleNo(List<int> arry)
        {
            var res = from n in arry
                      group n by n into g
                      orderby g.Count() descending
                      select g;
            // 分组中第一个组就是重复最多的
            var gr = res.First();
            if (gr.First() != 0)
            {
                if (gr.First() > 9)
                {
                    return "T0" + gr.First().ToString();
                }
                else 
                {
                    return "T00"+ gr.First().ToString();
                }
            }
            else
            {
                for (int i = 0; i < arry.Count; i++) 
                {
                    if (arry[i] != 0)
                    {
                        if (arry[i] > 9)
                        {
                            return "T0" + gr.First().ToString();
                        }
                        else
                        {
                            return "T00" + gr.First().ToString();
                        }
                    }

                }
                
                
            }
            return "T000";
        }
        //读取已测重包包龄
        private int ReadCurr_Age1(string No)
        {
            OracleConnection connOracle;
            try
            {
                connOracle = new OracleConnection(OracleUser);
                if (connOracle.State == ConnectionState.Closed)
                {
                    connOracle.Open();
                }

                string SQL = "SELECT  \"LADLE_AGE\"  FROM LADLE_AGE  where \"LADLE_ID\"  ='" + No + "'";
                OracleCommand thisCommand = new OracleCommand(SQL, connOracle);
                int iAge = Convert.ToInt32(thisCommand.ExecuteScalar());
                connOracle.Close();
                return iAge;
            }
            catch (Exception ee)
            {
                MessageBox.Show("读取已测重包包龄失败" + ee.ToString());
                return -1;
            }
        }
    }
}



            

        







    


    



