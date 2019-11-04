using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Ogar_CSharp
{
    public class Ticker
    {
        public HashSet<Action> callbacks = new HashSet<Action>();
        public bool running;
        public int step;
        public Thread tickingThread;
        public long virtualTime;
        public Ticker(int step) =>
            this.step = step;
        public void Add(Action callback)
        {
            callbacks.Add(callback);
        }
        public void Remove(Action callback)
        {
            callbacks.Remove(callback);
        }
        public void Start()
        {
            if (running)
                throw new Exception("The ticker has already been started");
            running = true;
            virtualTime = DateTime.Now.Ticks;
            tickingThread = new Thread(TickThread) { IsBackground = true };
            tickingThread.Start();
        }
        private void TickThread()
        {
            while (running)
            {
                foreach(var callback in callbacks)
                {
                    callback();
                }
                virtualTime += step;
                long delta = ((virtualTime + step) - DateTime.Now.Ticks);
                if (delta < 0)
                    virtualTime -= delta;
                Thread.Sleep((int)100);
            }
        }
        public void Stop()
        {
            if (!running)
                throw new Exception("The ticker hasn't started");
            running = false;
            if (tickingThread != null)
                tickingThread.Join();
        }
    }
}
