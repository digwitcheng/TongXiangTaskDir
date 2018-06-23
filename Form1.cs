using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TASK.MSG;
using ImageLib;
using ImageLib.Threads;
using VideoCoding.Services;
using AGV_TASK;
using System.Threading;
using System.Threading.Tasks;
using Const;
using Newtonsoft.Json;
using TASK.AGV;
using AGV_V1._0;
using AGV_TASK.MSG;
using TASK.XUMAP;
using client20710711;
using Cowboy.Sockets; 
using AGV_Scanner;

namespace AGV_TASK
{
    public partial class Form1 : Form
    {

        static ClientManager cm;
        private System.Threading.Timer sendRecvTimer;
        private int HandleDateTime = 2;
        public static Form1 form1;
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            form1 = this;
            string s = V_loc.ScanArea.ToString();
           // LoadFile();

            ////初始化socket
            //ImsNetManager.Instance.ReadConfguration();
            ////开始连接
            //ImsNetManager.Instance.Init();
            ////启动发送消息和记录日志线程
            //ImsNetManager.Instance.ThreadStart(); 
            txtBarcode.Select();
            ReadExpressList.readXml();
            ConnectToServer();
            MapLoad();
            

        }

        void AgvLoad()
        {
            AGVRead aaread = new AGVRead();
            aaread.AGV_Read();
            
        }

        void MapLoad()
        {
            MapRead mmread = new MapRead();
            mmread.MAP_classify();
          
        }
        void LoadFile()
        {
            AgvLoad();
          //  MapLoad();
            
        }

        void ConnectToServer()
        {
            try
            {
                cm = new ClientManager();
                cm.ShowMessage += ClientMessage;
                cm.DateMessage += HandleData;
                cm.ReLoad += LoadFile;
                cm.ConnectToServer("127.0.0.1", 5556);
                this.sendRecvTimer = new System.Threading.Timer(PostData, null, 0, HandleDateTime);
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接异常:" + ex.ToString());
            }
        }

        void PostData(object state)
        {
            if (QueueInstance.Instance.GetMyMessageCount() > 0)
            {
              ImsResponse sendMsg=  QueueInstance.Instance.GetMyMessageList();
              string str = sendMsg.data;
             // byte[] buffer = System.Text.Encoding.ASCII.GetBytes(str);
              cm.Send(MessageType.msg, str);
              string mstr = System.DateTime.Now.ToString() + ":-》》》》" + str + "\n";
              QueueInstance.Instance.AddMessageShowList(mstr);
            }
        }

        public static int msflag = 0;
        
        void HandleData(object sender, MessageEventArgs e)
        {
            if (e.Type == MessageType.move)
            {
                //工作
                msflag = 0;
                #region
                string mstr = System.DateTime.Now.ToString() + ":" + "工作";
                QueueInstance.Instance.AddMessageShowList(mstr);
                // #region 与界面建立socket连接，归位，所有小车回到RestArea

                //工作消息，所有小车去往RestArea,将处理得到的数据放到队列里
                for (int i = 0; i < AGVConstDefine.AGV.Count(); i++)
                {
                    if ( AGVConstDefine.AGV[i].StartLoc =="")
                    {
                      AGVDestination.Confirm_EndPoint(AGVConstDefine.AGV[i], "", AGVConstDefine.AGV[i].BeginX, AGVConstDefine.AGV[i].BeginY, V_loc.WaitArea.ToString());
                    }
                    else if (AGVConstDefine.AGV[i].StartLoc == V_loc.WaitArea.ToString()) 
                    { 
                        AGVDestination.Confirm_EndPoint(AGVConstDefine.AGV[i], V_loc.WaitArea.ToString(), AGVConstDefine.AGV[i].BeginX, AGVConstDefine.AGV[i].BeginY, V_loc.ScanArea.ToString());
                       
                    }
                    else if (AGVConstDefine.AGV[i].StartLoc == V_loc.ScanArea.ToString())
                    {
                       AGVDestination.Confirm_EndPoint(AGVConstDefine.AGV[i], V_loc.ScanArea.ToString(), AGVConstDefine.AGV[i].BeginX, AGVConstDefine.AGV[i].BeginY, V_loc.DestArea.ToString()); 
                    }
                   AGV_V1._0. SendData sd = JsonHelper.GetSendObj(AGVConstDefine.AGV[i]);
                    string sendjs = Newtonsoft.Json.JsonConvert.SerializeObject(sd);
                    //放进队列里
                    if (sendjs.Trim() != "")
                    {
                        if (AGVConstDefine.AGV[i].StartLoc == V_loc.ScanArea.ToString())
                        {
                            Task.Factory.StartNew(() => { Thread.Sleep(AGVConstDefine.AfterScanPause_Time); });
                        }
                        ImsResponse pathmessage = new ImsResponse();
                        pathmessage.obj = "PATH";
                        pathmessage.data = sendjs;
                        //一个一个入队
                        QueueInstance.Instance.AddMyMessageList(pathmessage);
                    }

                }
                #endregion
            }
            if (e.Type == MessageType.reStart)
            {
                //去休息
                msflag = 1;
                #region
                string mstr = System.DateTime.Now.ToString() + ":" + "休息";
                QueueInstance.Instance.AddMessageShowList(mstr);
                // #region 与界面建立socket连接，归位，所有小车回到RestArea

                //归位消息，所有小车去往RestArea,将处理得到的数据放到队列里
                for (int i = 0; i < AGVConstDefine.AGV.Count(); i++)
                {
                    if (AGVConstDefine.AGV[i].State == Const.State.free)
                    {
                        AGVDestination.Confirm_EndPoint(AGVConstDefine.AGV[i], "", AGVConstDefine.AGV[i].BeginX, AGVConstDefine.AGV[i].BeginY, V_loc.RestArea.ToString());
                    }
                    else if (AGVConstDefine.AGV[i].State ==Const. State.carried)
                    {
                        AGVConstDefine.AGV[i].StartLoc = V_loc.ScanArea.ToString();
                        AGVConstDefine.AGV[i].EndLoc = V_loc.DestArea.ToString();
                    }
                    AGV_V1._0.SendData sd = JsonHelper.GetSendObj(AGVConstDefine.AGV[i]);
                    string sendjs = Newtonsoft.Json.JsonConvert.SerializeObject(sd);
                    //放进队列里
                    if (sendjs.Trim() != "")
                    {
                        if (AGVConstDefine.AGV[i].StartLoc == V_loc.ScanArea.ToString())
                        {
                            Task.Factory.StartNew(() => { Thread.Sleep(AGVConstDefine.AfterScanPause_Time); });
                        }
                        ImsResponse pathmessage = new ImsResponse();
                        pathmessage.obj = "PATH";
                        pathmessage.data = sendjs;
                        //一个一个入队
                        QueueInstance.Instance.AddMyMessageList(pathmessage);
                    }

                }
                #endregion
            }
            else if (e.Type == MessageType.arrived)
            {
                #region 到达
                string strContent = e.Message;//Encoding.ASCII.GetString(bRecTmp, 1, receiveNumber - 1);
                string mstr = System.DateTime.Now.ToString() + ":Arrive消息" + strContent + "\n";
                QueueInstance.Instance.AddMessageShowList(mstr);

                List<AGV_V1._0.SendData> jsonObj = new List<AGV_V1._0.SendData>();
                //从第0个位置开始查找json数据对象
                int start = strContent.IndexOf("{", 0);
                int endx = strContent.IndexOf("}", 0);
                while (start >= 0)
                {
                    string str = strContent.Substring(start, endx - start + 1);
                    //对收到的消息进行处理
                    QueueInstance.Instance.AddYourMessageList(str);

                    start = strContent.IndexOf("{", endx);
                    endx = strContent.IndexOf("}", endx + 1);
                }
                #endregion
            }
            
            else if (e.Type == MessageType.msg)
            {

            }
            else if (e.Type == MessageType.AgvFile)
            {
                //  Response.Write(e.Message);
                string agvpath = System.Configuration.ConfigurationManager.AppSettings["AGVPath"].ToString().Trim();
               

                string[] xmlstr = new string[1];
                xmlstr[0] = e.Message;
                //System.IO.File.WriteAllLines(agvpath, xmlstr, Encoding.UTF8);
                System.IO.File.WriteAllLines(agvpath, xmlstr);


                AgvLoad();
                
            }
            else if (e.Type == MessageType.mapFile)
            {
                //  Response.Write(e.Message);
                string mappath = System.Configuration.ConfigurationManager.AppSettings["MAPPath"].ToString().Trim();


                string[] xmlstr = new string[1];
                xmlstr[0] = e.Message;
                //System.IO.File.WriteAllLines(agvpath, xmlstr, Encoding.UTF8);
                System.IO.File.WriteAllLines(mappath, xmlstr);
                MapLoad();
            }
        }

        protected void ClientMessage(object sender, MessageEventArgs e)
        {
            System.Console.WriteLine(e.Message);
            AddToLog(e.Message);
        }

        private void AddToLog(string str)
        {

            if (listViewLog.InvokeRequired)
            {
                // 当一个控件的InvokeRequired属性值为真时，说明有一个创建它以外的线程想访问它
                Action actionDelegate = () => { ShowLog(str); };

                //    IAsyncResult asyncResult =actionDelegate.BeginInvoke()

                // 或者 
                // Action<string> actionDelegate = delegate(string txt) { this.label2.Text = txt; };
                this.listViewLog.Invoke(actionDelegate, null);
            }
            else
            {
                ShowLog(str);
            }
        }

        void ShowLog(string str)
        {
            if (listViewLog.Items.Count > 100)
            {
                listViewLog.Clear();
            }
            listViewLog.View = View.Details;
            listViewLog.GridLines = false;
            listViewLog.Columns.Add("信息", 250, HorizontalAlignment.Left);
            listViewLog.Items.Add(new ListViewItem(str));
        }

        public void Form1_Load(object sender, EventArgs e)
        {
            // _del = new showdelegate(UpdateBox);
            //string str= ImsRecvTask.getstring();
            // _del(str);
           // HandleData(sender, new MessageEventArgs(MessageType.move,""));
        }

        public static int agvarrive=0;

        private void f1()
        {
            WaitForScan mmm = new WaitForScan();
            MessageBox.Show("请扫码");
            txtBarcode.Select();
            mmm.stopWay(10000);
        }
        public void timer1_Tick(object sender, EventArgs e)
        {
            if (QueueInstance.Instance.IsMessageShowHasData())
            {
                //增加方法
                //_del = new showdelegate(UpdateBox);
               _del = UpdateBox;
               // string mmstr = ImsRecvTask.getstring();
                string mmstr = QueueInstance.Instance.GetMessageShowList();
                //应用
                _del(mmstr);
            } 
            //出队
            //对每一个arrive之后的字符串进行处理
            for (int i = 0; i < 100; i++)
            {

                if (QueueInstance.Instance.IsYourMessageHasData())
                {

                    string str = QueueInstance.Instance.GetYourMessageList();
                    //将得到的数据转换成json对象
                    AGV_V1._0.SendData dataObj;
                    dataObj = (AGV_V1._0.SendData)JsonConvert.DeserializeObject(str, typeof(AGV_V1._0.SendData));
                    if (dataObj.State == Const.State.cannotToDestination)
                    {
                        dataObj.EndLoc = V_loc.RestArea.ToString();
                    }
                    else
                    {

                        if (msflag == 0)
                        { //工作
                            #region
                            dataObj = (AGV_V1._0.SendData)JsonConvert.DeserializeObject(str, typeof(AGV_V1._0.SendData));
                            if (dataObj.Arrive == true)
                            {
                                if (dataObj.StartLoc == V_loc.RestArea.ToString()|| dataObj.StartLoc=="null")
                                {
                                    dataObj.EndLoc = V_loc.WaitArea.ToString();

                                }
                                else if (dataObj.StartLoc == V_loc.WaitArea.ToString())
                                {
                                    dataObj.EndLoc = V_loc.ScanArea.ToString();
                                    dataObj.State = Const.State.free;
                                }
                                else if (dataObj.StartLoc == V_loc.ScanArea.ToString())
                                { 
                                   
                                    //if (dataObj.StartLoc == V_loc.ScanArea.ToString() && AGVConstDefine.destagvnum == -1) 
                                    dataObj.EndLoc = V_loc.DestArea.ToString();
                                    dataObj.State = Const.State.carried; 
                                    
                                   // Thread.Sleep(AGVConstDefine.AfterScanPause_Time);
                                }
                                else if (dataObj.EndLoc == V_loc.DestArea.ToString() && dataObj.State == Const.State.carried)
                                {
                                    dataObj.EndLoc = V_loc.DestArea.ToString();
                                    dataObj.State = Const.State.carried;
                                }
                                else if (dataObj.EndLoc == V_loc.DestArea.ToString() && dataObj.State == Const.State.unloading)
                                {
                                    dataObj.EndLoc = V_loc.DestArea.ToString();
                                    dataObj.State = Const.State.unloading;
                                }
                                else if (dataObj.EndLoc == V_loc.DestArea.ToString() && dataObj.State == Const.State.free)
                                {
                                    dataObj.EndLoc = V_loc.WaitArea.ToString();
                                    dataObj.State = Const.State.free;
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            //休息
                            dataObj = (AGV_V1._0.SendData)JsonConvert.DeserializeObject(str, typeof(AGV_V1._0.SendData));
                            if (dataObj.Arrive == true)
                            {
                                if (dataObj.StartLoc == V_loc.RestArea.ToString())
                                {
                                    //dataObj.EndLoc = "";
                                    //break;
                                }
                                else if (dataObj.EndLoc == V_loc.DestArea.ToString() && dataObj.State == Const.State.carried)
                                {
                                    dataObj.EndLoc = V_loc.DestArea.ToString();
                                    dataObj.State = Const.State.carried;
                                }
                                else if (dataObj.EndLoc == V_loc.DestArea.ToString() && dataObj.State == Const.State.unloading)
                                {
                                    dataObj.EndLoc = V_loc.DestArea.ToString();
                                    dataObj.State = Const.State.unloading;
                                }
                                else if (dataObj.EndLoc == V_loc.DestArea.ToString() && dataObj.State == Const.State.free)
                                {
                                    dataObj.EndLoc = V_loc.RestArea.ToString();
                                    dataObj.State = Const.State.free;
                                }
                            }

                        }
                    }
                    if (dataObj.StartLoc == V_loc.DestArea.ToString() && dataObj.EndLoc == V_loc.DestArea.ToString())
                    {
                    }
                    #region 扫描
                    else if (dataObj.StartLoc == V_loc.ScanArea.ToString() && AGVConstDefine.destagvnum == -1)
                    {
                        Task.Factory.StartNew(() =>
                        {

                            while (AGVConstDefine.destagvnum == -1)
                            {
                                txtBarcode.Select();
                                txtBarcode_TextChanged(sender, e);
                            }

                            AGVConstDefine.AGV[dataObj.Num].BeginX = dataObj.BeginX;
                            AGVConstDefine.AGV[dataObj.Num].BeginY = dataObj.BeginY;
                            AGVConstDefine.AGV[dataObj.Num].StartLoc = dataObj.StartLoc;
                            AGVConstDefine.AGV[dataObj.Num].EndLoc = dataObj.EndLoc;
                            AGVConstDefine.AGV[dataObj.Num].State = dataObj.State; 
                            AGVDestination.Confirm_EndPoint(AGVConstDefine.AGV[dataObj.Num], AGVConstDefine.AGV[dataObj.Num].StartLoc,
                           AGVConstDefine.AGV[dataObj.Num].BeginX, AGVConstDefine.AGV[dataObj.Num].BeginY, AGVConstDefine.AGV[dataObj.Num].EndLoc);
                            //} 
                            AGV_V1._0.SendData sd = JsonHelper.GetSendObj(AGVConstDefine.AGV[dataObj.Num]);
                            string sendjs = Newtonsoft.Json.JsonConvert.SerializeObject(sd); 
                            ImsResponse pathmessage = new ImsResponse();
                            pathmessage.obj = "PATH";
                            pathmessage.data = sendjs;
                            QueueInstance.Instance.AddMyMessageList(pathmessage);
                        }
                        );
                    }
                    #endregion

                    else
                    {
                        AGVConstDefine.AGV[dataObj.Num].BeginX = dataObj.BeginX;
                        AGVConstDefine.AGV[dataObj.Num].BeginY = dataObj.BeginY;
                        AGVConstDefine.AGV[dataObj.Num].StartLoc = dataObj.StartLoc;
                        AGVConstDefine.AGV[dataObj.Num].EndLoc = dataObj.EndLoc;
                        AGVConstDefine.AGV[dataObj.Num].State = dataObj.State; 
                        AGVDestination.Confirm_EndPoint(AGVConstDefine.AGV[dataObj.Num], AGVConstDefine.AGV[dataObj.Num].StartLoc,
                       AGVConstDefine.AGV[dataObj.Num].BeginX, AGVConstDefine.AGV[dataObj.Num].BeginY, AGVConstDefine.AGV[dataObj.Num].EndLoc);
                        //} 
                        AGV_V1._0.SendData sd = JsonHelper.GetSendObj(AGVConstDefine.AGV[dataObj.Num]);
                        string sendjs = Newtonsoft.Json.JsonConvert.SerializeObject(sd);
                        //出队
                        //if (AGVConstDefine.AGV[i].StartLoc == V_loc.ScanArea.ToString())
                        //{
                        //    //Task.Factory.StartNew(() => { Thread.Sleep(AGVConstDefine.AfterScanPause_Time); });
                        //}
                        ImsResponse pathmessage = new ImsResponse();
                        pathmessage.obj = "PATH";
                        pathmessage.data = sendjs;
                        QueueInstance.Instance.AddMyMessageList(pathmessage); 
                    }


                }
            }
        }
        //1.公共 关键字 返回类型 委托类型名 参数列表
        public delegate void showdelegate(string str);
        //2.声明委托变量
        public showdelegate _del;
        //3.初始化委托变量
        //第一种：_del=new showdelegate(UpdateBox);
        //第二种：_del=UpdateBox;
        //4.委托调用
        //_del(str)


        public void AddTask(string barcode)
        {
            if (listBox2.InvokeRequired)
            {
                // 当一个控件的InvokeRequired属性值为真时，说明有一个创建它以外的线程想访问它
                Action actionDelegate = () => { showBarcode(barcode); };
                // 或者
                // Action<string> actionDelegate = delegate(string txt) { this.label2.Text = txt; };
                listBox1.Invoke(actionDelegate, null);
            }
            else
            {
                showBarcode(barcode);
            }
        }
        public void showBarcode(string barcode)
        {
            if (listBox2.Items.Count == 20)
            {
                listBox2.Items.Clear();

            }
            listBox2.Items.Add(listBox2.Items.Count.ToString()+":"+barcode); 
        }
        public void UpdateBox(string str)
        {
            if (listBox1.InvokeRequired)
            {
                // 当一个控件的InvokeRequired属性值为真时，说明有一个创建它以外的线程想访问它
                Action actionDelegate = () => { showBox(str); };
                // 或者
                // Action<string> actionDelegate = delegate(string txt) { this.label2.Text = txt; };
                listBox1.Invoke(actionDelegate, null);
            }
            else
            {
                showBox(str);
            }
        }
        public static int listboxnum = 0;
        public void showBox(string json)
        { 
            if (listBox1.Items.Count== 1000)
            {
                listBox1.Items.Clear();

            }
              listBox1.Items.Add(json);
              

        } 

        public static string BARCODE;
        public void txtBarcode_TextChanged(object sender, EventArgs e)
        {
            //texbox是多行的情况，判断是否有换行符
           int pos = txtBarcode.Text.IndexOf("\r\n");
            string boxContent = txtBarcode.Text;
           if (pos > 0)
           {
               AGVConstDefine.barcode = txtBarcode.Text.Substring(0, pos);
               string currentBarcode = AGVConstDefine.barcode;
               //显示在listbox中
               _del = AddTask;
               _del(currentBarcode);
              //检查应该去哪个隔口 
               //扫描仪到隔口的任务调度
               txtBarcode.Text = currentBarcode; 
               
               //判断去哪个隔口
               for (int i = 0; i < AGVConstDefine.sumExpress; i++)
               {
                   string barcodeInnerText = AGVConstDefine.expressList[i].Attributes["num"].InnerText.ToString();
                   if (string.Compare(barcodeInnerText, AGVConstDefine.barcode) == 0)
                   {
                       AGVConstDefine.destagvnum = Convert.ToInt32(AGVConstDefine.expressList[i].Attributes["destnum"].InnerText.ToString());
                       break;
                   }
               }
           } 
           
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            HandleData(sender, new MessageEventArgs(MessageType.move, ""));
        }
    }
}
   

