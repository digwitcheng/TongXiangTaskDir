using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AGV_TASK
{
    class WaitForScan
    {
        public WaitForScan()
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;//，跨线程调用控件必加上去         
        }
        private int stopTime = 0;//暂停的时间
        ThreadStart myStart;
        Thread TheStop;
        public  readonly  object MyLockWord = new object();
   // 
        public   void stopWay(int stopTime)
        {
           
            this.stopTime = stopTime;
            myStart = new ThreadStart(this .ToStop );
            TheStop = new Thread(myStart );
            TheStop.Start();
        }
        private   void ToStop()
        {
            lock (MyLockWord)
            {
          
                Thread.Sleep(this .stopTime );
                Thread.CurrentThread.Abort();
 
            }
           
        }
    }
}
