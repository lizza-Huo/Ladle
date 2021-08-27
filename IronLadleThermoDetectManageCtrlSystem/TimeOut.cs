using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronLadleThermoDetectManageCtrlSystem
{
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
}
