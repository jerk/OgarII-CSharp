﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Ogar_CSharp.primitives
{
    public class Ticker
    {
        public HashSet<Action> callbacks = new HashSet<Action>();
        public bool running;
        public short step;
        public Thread tickingThread;
        public long virtualTime;
        public Ticker(short step) =>
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
                var delta = (virtualTime + step) - DateTime.Now.Ticks;
                if (delta < 0)
                    virtualTime -= delta;
                Thread.Sleep((int)delta);
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