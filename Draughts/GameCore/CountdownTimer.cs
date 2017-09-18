using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace GameCore
{
    public class CountdownTimer
    {
        private Timer timer;
        private int interval;
        private int timeInSeconds;
        private bool timeout;
        

        public CountdownTimer()
        {
            interval = 1000;
            timer = new Timer(interval);
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            timeout = false;
            timeInSeconds = 0;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            timeInSeconds--;

            if (timeInSeconds == 0)
            {
                timer.Enabled = false;
                timeout = true;
            }
        }

        public void Start(int timeInSeconds)
        {
            if (timeInSeconds <= 0)
                return;

            this.timeInSeconds = timeInSeconds;
            timeout = false;

            timer.Enabled = true;
        }

        public void Stop()
        {
            timer.Enabled = false;
        }

        public bool isTimeout()
        {
            return timeout;
        }

        public int getRemainTime()
        {
            return timeInSeconds;
        }

        public String getRemainTimeString()
        {
            return String.Format("{0:00}", timeInSeconds / 60) + ":" + String.Format("{0:00}", timeInSeconds % 60);
        }

        public void addElapsedEventHandler(ElapsedEventHandler handler)
        {
            timer.Elapsed += handler;
        }
    }
}
