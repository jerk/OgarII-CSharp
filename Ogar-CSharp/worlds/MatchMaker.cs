﻿using Ogar_CSharp.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.Worlds
{
    public class MatchMaker
    {
        public ServerHandle handle;
        public List<Connection> queued = new List<Connection>();
        public MatchMaker(ServerHandle handle)
        {
            this.handle = handle;
        }
        public bool IsInQueue(Connection connection)
        {
            return queued.Contains(connection);
        }
        public void BroadcastQueueLength()
        {
            //To be implemented
        }
        public void ToggleQueued(Connection connection)
        {
            if (IsInQueue(connection))
                Dequeue(connection);
            else
                Enqueue(connection);
        }
        public void Dequeue(Connection connection)
        {
            if(handle.Settings.matchMakerNeedsQueueing)
            {
                //to be implemented
            }
            queued.Remove(connection);
            BroadcastQueueLength();
        }
        public void Enqueue(Connection connection)
        {
            if (handle.Settings.matchMakerNeedsQueueing)
            {
                //to be implemented
            }
            queued.Add(connection);
            BroadcastQueueLength();
        }
        public void Update()
        {
            var bulkSize = handle.Settings.matchMakerBulkSize;
            while (true)
            {
                if (queued.Count < bulkSize)
                    return;
                //var world =
            }
        }
        /*
        public World GetSuitableWorld()
        {
            World bestWorld = null;
            foreach(var world in handle.worlds)
            {
                if(!handle.gamemode.)
            }
        }*/
    }
}
