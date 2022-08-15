using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace KUM.Shared
{
    /// <summary>
    /// Use of worker threads helps by using multiple threads to help alot of data processing
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WorkerThreads
    {
        private class WorkerComparer : IComparer<Worker>
        {
            public static readonly WorkerComparer Comparer = new();
            int IComparer<Worker>.Compare(Worker x, Worker y)
            {
                return x.JobQueueCount.CompareTo(y.JobQueueCount);
            }
        }
        public bool IsReady { get; private set; }
        public ulong jobNumber;
        public readonly int WorkerCount;
        private readonly Worker[] workers;
        public class ReferenceInt 
        { 
            private int _ref;
            public int MAX;
            public int Value 
            {
                get
                {
                     _ref++;
                    return _ref;
                }
            }
        }

        private class Job
        {
            public Job(object obj, ulong jobNumber, Action<object> task, EventWaitHandle handle, ReferenceInt r)
            {
                this.jobNumber = jobNumber;
                this.task = task;
                this.handle = handle;
                this.obj = obj;
                referenceInt = r;
            }
            public readonly object obj;
            public readonly ulong jobNumber;
            public readonly Action<object> task;
            public readonly EventWaitHandle handle;
            public readonly ReferenceInt referenceInt;
        }
        private class Worker
        {
            internal Worker(WorkerThreads instance)
            {
                this.instance = instance;
                thread = new Thread(JobProcessor);
            }
            public int JobQueueCount => blockingCollection.Count;
            private readonly WorkerThreads instance;
            private readonly BlockingCollection<Job> blockingCollection = new();
            private readonly Thread thread;
            public void Start()
            {
                thread.Start();
              
            }
            public void EnqueueJob(Job job)
            {
                blockingCollection.Add(job);
            }
            private void JobProcessor()
            {
                while (instance.IsReady)
                {
                    if (blockingCollection.TryTake(out Job job, 100))
                    {

                        job.task(job.obj);
                        if (job.referenceInt != null && job.handle != null)
                        {
                            lock (job.referenceInt)
                            {
                                var value = job.referenceInt.Value;
                                if (value == job.referenceInt.MAX)
                                {
                                    job.handle.Set();
                                }
                            }
                        }
                    }
                }
            }
            public void Join()
            {
                thread.Join();
            }
        }
        /// <summary>
        /// Will use <see cref="Environment.ProcessorCount"/> for worker count
        /// </summary>
        public WorkerThreads() : this(Environment.ProcessorCount)
        {
            //throw new NotImplementedException();
        }
        /// <summary>
        /// Initializes a <see cref="WorkerThreads{T}"/>
        /// </summary>
        /// <param name="count">the amount of workers to create</param>
        public WorkerThreads(int count)
        {
            WorkerCount = count;
            workers = new Worker[count];
            for (int i = 0; i < count; i++)
            {
                workers[i] = new Worker(this);
            }
        }
        public void Start()
        {
            if (!IsReady)
            {
                IsReady = true;
                for (int i = 0; i < WorkerCount; i++)
                {
                    workers[i].Start();
                }
            }
        }
        public void Stop()
        {
            if (IsReady)
            {
                IsReady = false;
                for (int i = 0; i < WorkerCount; i++)
                {
                    workers[i].Join();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="onCompleted"></param>
        /// <param name="waitForLast"></param>
        public void EnqueueJob(object obj, Action<object> action, EventWaitHandle waitHandle, ReferenceInt _ref)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
             var worker = GetWorker();
            worker.EnqueueJob(new Job(obj, jobNumber++, action, waitHandle, _ref));
        }
        private Worker GetWorker()
        {
            Worker w = workers[0];
            for (int i = 1; i < WorkerCount; i++)
            {
                Worker s = workers[i];
                if (s.JobQueueCount < w.JobQueueCount)
                    w = s;
            }
            return w;
        }
    }
}
