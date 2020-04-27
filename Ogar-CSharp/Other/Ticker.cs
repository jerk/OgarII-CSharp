using Ogar_CSharp.Cells;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Ogar_CSharp
{
    /// <summary>
    /// Used for updating game logic
    /// </summary>
    public class Ticker
    {

        private event Action CallBacks;
        private bool running;
        public int step;
        private int virtualTime;
        private Thread tickingThread;
        private Stopwatch stopWatch = Stopwatch.StartNew();
        public Ticker(int step) =>
            this.step = step;
        /// <summary>
        /// Add action for a tick
        /// </summary>
        /// <param name="callback">Action to add</param>
        public void Add(Action callback)
        {
            CallBacks += callback;
        }
        /// <summary>
        /// Remove action for a tick
        /// </summary>
        /// <param name="callback">Action to remove</param>
        public void Remove(Action callback)
        {
            CallBacks -= callback;
        }
        /// <summary>
        /// Starts the tick thread.
        /// </summary>
        public void Start()
        {
            if (running)
                throw new Exception("The ticker has already been started");
            running = true;
            virtualTime = (int)stopWatch.ElapsedMilliseconds;
            tickingThread = new Thread(TickThread) { IsBackground = true };
            tickingThread.Start();
        }
        private void TickThread()
        {
            while (running)
            {
                CallBacks();
                virtualTime += step;
                var delta = (virtualTime + step) - (int)stopWatch.ElapsedMilliseconds;
                if (delta < 0)
                    virtualTime -= delta;
                if(delta > 0)
                    Thread.Sleep(delta);
            }
        }
        /// <summary>
        /// Stops the ticking thread.
        /// </summary>
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
