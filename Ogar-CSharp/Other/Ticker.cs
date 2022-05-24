using Ogar_CSharp.Cells;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private long virtualTime;
        private Thread tickingThread;
        private Stopwatch stopWatch = Stopwatch.StartNew();
        private long[] averageTicks = new long[24];
        public bool ticksFilled = false;
        private long tickPos;
        public Ticker(int step) =>
            this.step = step  * 10_000;
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
            virtualTime = stopWatch.ElapsedMilliseconds;
            tickingThread = new Thread(TickThread) { IsBackground = true, Name = "tick thread" };
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
                if (delta > 0)
                    Thread.Sleep((int)GetDelta(delta));
            }
        }
        private long GetDelta(long newDelta)
        {
            var averageDelta = (long)averageTicks.Average();
            tickPos++;
            if ((tickPos % averageTicks.Length) == 1)
            {
                ticksFilled = true;
                tickPos = 0;
            }
            averageTicks[tickPos] = newDelta;
            return averageDelta;
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
