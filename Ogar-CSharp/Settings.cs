using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp
{
    public class Settings
    {
        public enum MergeVersion
        {
            Old,
            New
        }
        public int listeningPort = 444;
        public int listenerMaxConnections = 100;
        public int listenerMaxClientDormancy = 1000 * 60;
        public List<string> listenerAcceptedOrigins = new List<string>();
        public List<string> listenerForbiddenIPs = new List<string>();
        public short listenerMaxConnectionsPerIP = -1;
        public int serverFrequency = 25;
        public byte playerMaxNameLength = 16;
        public string serverName = "An unnamed server";
        public string serverGamemode = "FFA";
        public bool chatEnabled = true;
        public List<string> chatFilteredPhrases = new List<string>();
        public int chatCoolDown = 1000;
        public float worldMapX;
        public float worldMapY;
        public float worldMapW = 7071;
        public float worldMapH = 7071;
        public short worldFinderMaxLevel = 16;
        public short worldFinderMaxItems = 16;
        public short worldSafeSpawnTries = 16;
        public float worldSafeSpawnFromEjectedChange = 0.8f;
        public int worldPlayerDisposeDelay = 25 * 60;
        public int worldPlayerBotsPerWorld;
        public List<string> worldPlayerBotNames = new List<string>();
        public List<string> worldPlayerBotSkins = new List<string>();
        public int worldMinionsPerPlayer;
        public int worldMaxPlayers;
        public int worldMinCount = 1;
        public int worldMaxCount = 2;
        public bool matchMakerNeedsQueueing;
        public int matchMakerBulkSize = 1;
        public string minionName = "Minion";
        public int minionSpawnSize = 32;
        public bool minionEnableERTPControls;
        public bool minionEnableQBasedControl = true;
        public int pelletMinSize = 10;
        public int pelletMaxSize = 20;
        public int pelletGrowTicks = 25 * 60;
        public int pelletCount = 10000;
        public int virusMinCount = 30;
        public int virusMaxCount = 90;
        public int virusSize = 100;
        public int virusFeedTimes = 7;
        public bool virusPushing;
        public int virusSplitBoost = 780;
        public int virusPushBoost = 120;
        public bool virusMonotonePops;
        public int ejectedSize = 38;
        public int ejectingLoss = 43;
        public float ejectDispersion = 0.3f;
        public int ejectedCellBoost = 780;
        public int motherCellSize = 149;
        public int motherCellCount = 100;
        public float motherCellPassiveSpawnChance = 0.05f;
        public float motherCellActiveSpawnSpeed = 1f;
        public float motherCellPelletBoost = 90;
        public int motherCellMaxPellets = 96;
        public int motherCellMaxSize = 65535;
        public float playerRoamSpeed;
        public float playerRoamViewScale = 0.4f;
        public float playerViewScaleMult = 1;
        public float playerMinViewScale = 0.01f;
        public short MaxNameLength = 16;
        public bool playerAllowSkinInName = true;
        public int playerMinSize = 32;
        public int playerSpawnSize = 122;
        public int playerMaxSize = 1500;
        public int playerMinSplitSize = 60;
        public int playerMinEjectSize = 60;
        public short playerSplitCap = 255;
        public float playerEjectDelay = 2;
        public int playerMaxCells = 16;
        public float playerMoveMult = 1;
        public float playerSplitBoost = 780;
        public float playerNoCollideDelay = 14;
        public float playerNoMergeDelay = 15;
        public MergeVersion playerMergeVersion = MergeVersion.Old;
        public float playerMergeTime = 30;
        public float playerMergeTimeIncrease = 0.02f;
        public float playerDecayMult = 0.001f;
    }
}
