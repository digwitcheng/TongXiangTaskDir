﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TASK.AGV;
using Const;
using AGV_TASK;
using TASK.XUMAP;
using System.Threading;
using System.Xml;
using System.Windows.Forms;

namespace TASK.AGV
{
    public class AGVDestination
    {

        //  MapRead MapRead = new MapRead();
        public static int seed = 400;
        static Random rd = new Random(seed);


        private void f1()
        {
            WaitForScan mmm = new WaitForScan();
            MessageBox.Show("请扫码");
            Form1.form1.txtBarcode.Select();
            mmm.stopWay(10000);
        }
        public static AGVInformation Confirm_EndPoint(AGVInformation agv, string startloc, int x, int y, string endloc)
        {
            if (endloc == V_loc.RestArea.ToString())
            {
                if (agv.State == State.cannotToDestination)
                {
                    CanToRest(agv, x, y);
                }
                //休息1.0
                ToRest(agv, x, y);
            }
            else if (endloc == V_loc.WaitArea.ToString())
            {

                if (startloc != V_loc.DestArea.ToString())
                {
                    //工作1.0
                    RandToWait(agv, x, y);

                }
                else if (startloc == V_loc.DestArea.ToString())
                {
                    DestToWait(agv, x, y);
                }
                else if (startloc == V_loc.RestArea.ToString())
                {
                    RandToWait(agv, x, y);
                }
            }
            else if (startloc == V_loc.WaitArea.ToString() && endloc == V_loc.ScanArea.ToString())
            {
                //工作2.0
                WaitToScan(agv, x, y);
            }
            else if (startloc == V_loc.ScanArea.ToString() && endloc == V_loc.DestArea.ToString())
            { 
               ScanToDest(agv, x, y);   
            }
            else if (startloc == V_loc.DestArea.ToString() && endloc == V_loc.DestArea.ToString())
            {
                agv.EndX = agv.BeginX;
                agv.EndY = agv.BeginY;
                if (agv.State == State.unloading)
                {
                    agv.State = State.unloading;
                }
                else if (agv.State == State.free)
                {
                    agv.State = State.free;
                }


            }
            else if (startloc == V_loc.DestArea.ToString() && endloc == V_loc.RestArea.ToString())
            {
                DestToRest(agv, x, y);
            }


            return agv;
        }
        //按照小车编号，按照顺序

        public static int randSeed = 200;
        public static void CanToRest(AGVInformation agv, int x, int y)
        {
            int agvnum = agv.Number;

            int canRest;
            Random r = new Random(randSeed);
            if (randSeed < 500)
            {
                randSeed++;
            }
            else
            {
                randSeed = 200;
            }
            canRest = r.Next(0, MapRead.krest);
            //agv.EndX = MapRead.RR[canRest].x;
            //agv.EndY = MapRead.RR[canRest].y;
            agv.EndX = 0;
            agv.EndY = 0;
            agv.Dire = Direction.Right;
            //agv.StartLoc = agv.StartLoc;
            agv.EndLoc = V_loc.RestArea.ToString();
            agv.State = State.free;
        }
        public static void ToRest(AGVInformation agv, int x, int y)
        {
            int agvnum = agv.Number;
            agv.EndX = MapRead.RR[agvnum % (MapRead.krest)].x;
            agv.EndY = MapRead.RR[agvnum % (MapRead.krest)].y;
            agv.Dire = Direction.Right;
            //agv.StartLoc = agv.StartLoc;
            agv.EndLoc = V_loc.RestArea.ToString();
            agv.State = State.free;
        }

        //选择较近的三个工件台中的最少排队的一个工件台
        public static  int  ThreeLeast(AGVInformation agv, int x, int y)
        {
            int agvnum = agv.Number;
            int choosework = 0;

            int side = 0;//0,只有右边;1,只有左边;2,两边
            if (MapRead.leftWorkstationNum != 0 && MapRead.rightWorkstationNum != 0) side = 2;
            else if (MapRead.rightWorkstationNum == 0) side = 1; 

            if (y < (MapRead.widthNum / side)  && side!=0)
            {
                //锁定较近的工件台
                #region
                int workStart = NearestWorkStation(MapRead.LeftWork, x);
                #endregion
                //再选当前排队中最少的一个工件台
                //int choosework = 0;
                choosework = LeastEntrance(MapRead.LeftWork,workStart);
            }
            else
            {
                //选定三个相邻的工件台
                int workStart = NearestWorkStation(MapRead.RightWork, x);
                //选择最少被锁定排队的工件台
                //int choosework = 0;
                choosework = LeastEntrance(MapRead.RightWork,workStart);
            }
            return choosework;
        }
        static int NearestWorkStation(MAP[] MAPArray,int x)
        {
            int workStart = 0;

            //for (int i = 0; i < MapRead.rightWorkstationNum; i++)
            ////for (int j = 0; j < MapRead.entranceNum; j++)
            //{
            //    // if (System.Math.Abs(x - MapRead.RightWorkstation[i, j].x) <= 3)
            //    if (System.Math.Abs(x - MapRead.RightWork[i].x) <= 6)
            //    {
            //        if (i == 0)
            //        {
            //            workStart = i;
            //            break;
            //        }
            //        else if (i > 0 && i + 1 < MapRead.rightWorkstationNum)
            //        {
            //            workStart = i - 1;
            //            break;
            //        }
            //        else
            //        {
            //            workStart = i - 2;
            //            break;
            //        }
            //    }
            //}

            int min = int.MaxValue;
            for (int i = 0; i < MAPArray.Length; i++)
            //for (int j = 0; j < MapRead.entranceNum; j++)
            {
                // if (System.Math.Abs(x - MapRead.RightWorkstation[i, j].x) <= 3)
                if (Math.Abs(x - MAPArray[i].x) < min)
                {
                    min = Math.Abs(x - MAPArray[i].x);
                    workStart = i;
                }
            }
            /* hxc 2017.12
            if (workStart > 0 && workStart < MAPArray.Length - 1)
            {
                workStart = workStart - 1;
            }
            if (workStart == MAPArray.Length - 2)
            {
                workStart = workStart - 1;
            }
            */

            //xzy 2018.3.11 
            if (workStart > 0 && workStart < MAPArray.Length - 1)
            {
                workStart = workStart - 1;
            }
            else if (workStart == MAPArray.Length - 1)
            {
                //xzy 2018.3.11 考虑工件台个数
                if (MAPArray.Length >= 3) workStart = workStart - 2;
                else if (MAPArray.Length < 3 ) workStart = 0;
            }

            return workStart;
        }
       static int LeastEntrance(MAP[] MAPArray,int workStart)
        {
            //int choosework = 0;

            //if (MAPArray[workStart].agvNumOfQueuing < MAPArray[workStart + 1].agvNumOfQueuing)
            //{
            //    choosework = workStart;
            //}
            //else
            //{
            //    if (MAPArray[workStart + 1].agvNumOfQueuing < MAPArray[workStart + 2].agvNumOfQueuing)
            //    {
            //        choosework = workStart + 1;
            //    }
            //    else
            //    {
            //        choosework = workStart + 2;
            //    }
            //}
            //return choosework;


            int min = int.MaxValue;
            int minIndex = 0;
            int WorkStationNumSort = 0;
           //xzy 2018.3.11 考虑工件台个数
            if (MAPArray.Length >= 3) WorkStationNumSort = 3;
            else WorkStationNumSort = MAPArray.Length;

            for (int i = workStart; i < workStart + WorkStationNumSort; i++)
            {
                if (min > MAPArray[i].agvNumOfQueuing)
                {
                    min = MAPArray[i].agvNumOfQueuing;
                    minIndex = i;
                }
            }
            return minIndex;
        }


      
        //按照小车编号，按照顺序
        public static void RandToWait(AGVInformation agv, int x, int y)
        {
            int agvnum = agv.Number;
            int worki = 0;//ThreeLeast(agv, x, y), workj = 0;
            int workj = 0;
            //int worki = ThreeLeast(agv, x, y), workj = 0;
            //agv.WorkStaionPassBy = worki;
           #region 多个
           // // agv.WorkNum = worki;
           // //xzy 2017 排队区有左右两边
           //// if (y < (MapRead.widthNum / 2))

           // //xzy 2018.3 考虑排队区是否在左右两边
           // int side = 0 ;//0,只有右边;1,只有左边;2,两边
           // if (MapRead.leftWorkstationNum != 0 && MapRead.rightWorkstationNum != 0) side = 2;
           // else if (MapRead.rightWorkstationNum == 0) side = 1; 

           // if (y < (MapRead.widthNum / side) &&  side !=0 )
           // {
           //     while (MapRead.LeftWorkstation[worki, workj].occupy == true)
           //     {
           //         if (workj == (MapRead.entranceNum - 1))
           //         {
           //             workj = 0;
           //             for (int i = 0; i < MapRead.entranceNum; i++)
           //                 MapRead.LeftWorkstation[worki, i].occupy = false;
           //         }
           //         else
           //         { workj++; }
           //     }
           //     agv.EndX = MapRead.LeftWorkstation[worki,workj].x;
           //     agv.EndY = MapRead.LeftWorkstation[worki, workj].y;
           //     MapRead.LeftWork[worki].agvNumOfQueuing++;
           //     MapRead.LeftWorkstation[worki, workj].occupy = true;
           //     agv.LWorkNum = worki;
           //     agv.RWorkNum = -1;
           // }
           // else 
           // {
           //     while (MapRead.RightWorkstation[worki, workj].occupy == true)
           //     {
           //         if (workj == (MapRead.entranceNum - 1))
           //         {
           //             workj = 0;
           //             for (int i = 0; i < MapRead.entranceNum; i++)
           //                 MapRead.RightWorkstation[worki, i].occupy = false;
           //         }
           //         else
           //         { workj++; }
           //     }
           //   // int worki = agvnum % MapRead.rightWorkstationNum;
           //     agv.EndX = MapRead.RightWorkstation[worki, workj].x;
           //     agv.EndY = MapRead.RightWorkstation[worki, workj].y;
           //     MapRead.RightWork[worki].agvNumOfQueuing++;
           //     MapRead.RightWorkstation[worki, workj].occupy = true;
           //     agv.RWorkNum = worki;
           //     agv.LWorkNum = -1;
           //  //   MapRead.RightWorkstation[worki, workj].occupy = true;
           // }
           // //while (MapRead.RightWorkstation[worki, workj].occupy == true)
           // //{ 
           // //    workj++;
           // //    if (workj == (MapRead.entranceNum - 1))
           // //    {

           // //        workj = 0;
           // //    }
           // //}
           // //if (workj == (MapRead.entranceNum-1))
           // //{

           // //    workj = 0;
           // //}
           // //else
           // //{
           // //    workj++;
           // //}

           // //agv.StartLoc = "RandArea";
           // agv.EndLoc = V_loc.WaitArea.ToString();
           // agv.State = State.free;

            #endregion 
            agv.EndX = MapRead.LeftWorkstation[worki, workj].x;
            agv.EndY = MapRead.LeftWorkstation[worki, workj].y;
            //MapRead.LeftWork[worki].agvNumOfQueuing++;
            agv.LWorkNum = worki;
            //agv.LWorkNum = -1;
            MapRead.LeftWorkstation[worki, workj].occupy = true;
            //agv.StartLoc = V_loc.DestArea.ToString();
            agv.EndLoc = V_loc.WaitArea.ToString();
            agv.State = State.free;

        }

        public static void DestToWait(AGVInformation agv, int x, int y)
        {
              int agvnum = agv.Number; 
              int worki = 0;//ThreeLeast(agv, x, y), workj = 0;
              int workj = 0;

             #region  多个
            //  agv.WorkStaionPassBy = worki;
            
            ////agv.WorkNum=worki;
            ////xzy 2017 if (y < (MapRead.widthNum / 2))

            //  //xzy 2018.3 考虑排队区是否在左右两边
            //  int side = 0;//0,只有右边;1,只有左边;2,两边
            //  //左右两边
            //  if (MapRead.leftWorkstationNum != 0 && MapRead.rightWorkstationNum != 0) side = 2;
            //  //左边
            //  else if (MapRead.rightWorkstationNum == 0) side = 1;

            //  if (y < (MapRead.widthNum / side) && side != 0)
            //{
            //    while (MapRead.LeftWorkstation[worki, workj].occupy == true)
            //    {
            //        if (workj == (MapRead.entranceNum - 1))
            //        {
            //            workj = 0;
            //            for (int i = 0; i < MapRead.entranceNum; i++)
            //                MapRead.LeftWorkstation[worki, i].occupy = false;
            //        }
            //        else
            //        { workj++; }
            //    }
            //   // int worki = agvnum % MapRead.leftWorkstationNum;
            //    agv.EndX = MapRead.LeftWorkstation[worki, workj].x;
            //    agv.EndY = MapRead.LeftWorkstation[worki, workj].y;
            //    MapRead.LeftWork[worki].agvNumOfQueuing++;
            //    agv.LWorkNum = worki;
            //    agv.RWorkNum = -1;
            //    MapRead.LeftWorkstation[worki, workj].occupy = true;
            //}
            //else
            //{
            //    while (MapRead.RightWorkstation[worki, workj].occupy == true)
            //    {
            //        if (workj == (MapRead.entranceNum - 1))
            //        {
            //            workj = 0;
            //            for (int i = 0; i < MapRead.entranceNum; i++)
            //                MapRead.RightWorkstation[worki, i].occupy = false;
            //        }
            //        else
            //        { workj++; }
            //    }

            //   //int worki = agvnum % MapRead.rightWorkstationNum;
            //    agv.EndX = MapRead.RightWorkstation[worki, workj].x;
            //    agv.EndY = MapRead.RightWorkstation[worki, workj].y;
            //    MapRead.RightWork[worki].agvNumOfQueuing++;
            //    agv.RWorkNum = worki;
            //    agv.LWorkNum = -1;
            //    MapRead.RightWorkstation[worki, workj].occupy = true;
            //}
              #endregion

              agv.EndX = MapRead.LeftWorkstation[worki, workj].x;
              agv.EndY = MapRead.LeftWorkstation[worki, workj].y;
             // MapRead.LeftWork[worki].agvNumOfQueuing++;
              agv.LWorkNum = worki;
              //agv.LWorkNum = -1;
            //  MapRead.LeftWorkstation[worki, workj].occupy = true;
              agv.StartLoc = V_loc.DestArea.ToString();
              agv.EndLoc = V_loc.WaitArea.ToString();
              agv.State = State.free;
        }

        public static void DestToRest(AGVInformation agv, int x, int y)
        {
            int agvnum = agv.Number % MapRead.krest;
            agv.EndX = MapRead.RR[agvnum].x;
            agv.EndY = MapRead.RR[agvnum].y;
            agv.Dire = Direction.Right;
            agv.State = State.free;
            agv.StartLoc = V_loc.DestArea.ToString();
            agv.EndLoc = V_loc.RestArea.ToString();
        }

        public static void WaitToScan(AGVInformation agv, int x, int y)
        {
            int agvnum = agv.Number;
            //往前走
            string pathMap = System.Configuration.ConfigurationManager.AppSettings["MAPPath"].ToString();
            XmlDocument xmlfile = new XmlDocument();
            xmlfile.Load(pathMap);
            string agvxy = "config/Grid/td" + x.ToString() + "-" + y.ToString();
            XmlElement td = (XmlElement)xmlfile.SelectSingleNode(agvxy);
            string tdattr = td.Attributes["direction"].InnerText;

            string agvxy1 = "config/Grid/td" + x.ToString() + "-" + (y + 1).ToString();
            XmlElement td1 = (XmlElement)xmlfile.SelectSingleNode(agvxy1);
            string tdattr1 = td.Attributes["direction"].InnerText;
           //xzy 2017if (agv.BeginY < 50)

            //xzy 2018.3.11
            int side = 0;//0,只有右边;1,只有左边;2,两边
            if (MapRead.leftWorkstationNum != 0 && MapRead.rightWorkstationNum != 0) side = 2;
            else if (MapRead.rightWorkstationNum == 0) side = 1;
            if (agv.BeginY < (MapRead.widthNum / side) && side != 0)
            {
                for (int i = 0; i < MapRead.klscan; i++)
                {
                    if (System.Math.Abs(MapRead.LeftScanner[i].x - agv.BeginX) == 0
                      //  || System.Math.Abs(MapRead.LeftScanner[i].x - agv.BeginX) == 1
                        )
                    {
                        agv.EndX = MapRead.LeftScanner[i].x;
                        agv.EndY = MapRead.LeftScanner[i].y;
                        agv.Dire = Direction.Left;
                        //xzy 2018.3.11 该工件台排队数减少1
                        MapRead.LeftWork[agv.WorkStaionPassBy].agvNumOfQueuing--;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < MapRead.krscan; i++)
                {
                    if (System.Math.Abs(MapRead.RightScanner[i].x - agv.BeginX) == 0
                       // || System.Math.Abs(MapRead.RightScanner[i].x - agv.BeginX) == 1
                        )
                    {
                        agv.EndX = MapRead.RightScanner[i].x;
                        agv.EndY = MapRead.RightScanner[i].y;
                        agv.Dire = Direction.Right;
                        MapRead.RightWork[agv.WorkStaionPassBy].agvNumOfQueuing--;
                        break;
                    }
                }
            }

            agv.StartLoc = V_loc.WaitArea.ToString();
            agv.EndLoc = V_loc.ScanArea.ToString();
            agv.State = State.free;
        }
      
        public static void ScanToDest(AGVInformation agv, int x, int y)
        {
        
            int agvnum = agv.Number; 
           
            int tx, ty;
            int ss = MapRead.destinationNum; 
            tx = MapRead.Destination[AGVConstDefine.destagvnum].x;
            ty = MapRead.Destination[AGVConstDefine.destagvnum].y;
            //小车在DestArea下面
            //if (agv.BeginX > tx)
            //{
            //    agv.Dire = Direction.Up;
            //    agv.EndX = tx;
            //    agv.EndY = ty - 1;
            //    agv.DestX = tx;
            //    agv.DestY = ty;
            //}
            //else
            //{
            //    agv.Dire = Direction.Down;
            //    agv.EndX = tx;
            //    agv.EndY = ty + 1;
            //    agv.DestX = tx;
            //    agv.DestY = ty;
            //}

            agv.Dire = Direction.Down;
            agv.EndX = tx-1;
            agv.EndY = ty;
            agv.DestX = tx;
            agv.DestY = ty;

            agv.StartLoc = V_loc.ScanArea.ToString();
            agv.EndLoc = V_loc.DestArea.ToString();
            agv.State = State.carried; 
            //if (agv.LWorkNum != -1 && MapRead.LeftWork[agv.LWorkNum].agvNumOfQueuing > 0)
            //{
            //    int la = MapRead.LeftWork[agv.LWorkNum].agvNumOfQueuing;
            //    MapRead.LeftWork[agv.LWorkNum].agvNumOfQueuing--;
            //    agv.LWorkNum = -1;
            //} 
            //else if (agv.RWorkNum != -1 && MapRead.RightWork[agv.RWorkNum].agvNumOfQueuing > 0)
            //{
            //    int ra = MapRead.RightWork[agv.RWorkNum].agvNumOfQueuing;
            //    MapRead.RightWork[agv.RWorkNum].agvNumOfQueuing--;
            //    agv.RWorkNum = -1;
            //} 
            AGVConstDefine.destagvnum = -1;
        }

        public AGVDestination()
        {
        }
    }
}
 
