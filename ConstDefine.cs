using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TASK.AGV;
using TASK.XUMAP;
using System.Windows.Forms;
using System.Xml;

namespace Const
{
   public  static class AGVConstDefine
   { 
        public const byte MSG = 1;
        public const byte MAP_FILE = 2;
        public const byte AGV_FILE = 3;
      //  public static  AGVInformation[] AGV=new AGVInformation [500];
        public static AGVInformation[] AGV;
        public static MAP[] Rest = new MAP[1000];
        public static MAP[] wait = new MAP[500];
        public static MAP[] Destination = new MAP[1000];
        public static MAP[] QQ = new MAP[1000];
        public static int  AGVSUM=0;
        public static int AfterScanPause_Time = 5000;
        public static  List<ListViewItem> listItem = new List<ListViewItem>();

       //demo
        public static string barcode;
        public struct DEST
        {
            public int destinationNum;
        }
       public static  DEST[] p = new DEST [500];

       //读条码信息
       public static int sumBarcodes = 0;
       public static XmlDocument expressFile;
       public static string pathExpressFile;
       public static XmlNodeList expressList;
       public static int sumExpress;
       public static int destagvnum=-1;
       
    }
   public enum V_loc { WaitArea, ScanArea, DestArea, RestArea };
  // public enum V_State { normal, needCharge, breakdown, cannotToDestination };
  public enum State { free, needCharge, breakdown, cannotToDestination, carried,unloading };
   public enum Direction { Right, Down, Left, Up };
    
}
