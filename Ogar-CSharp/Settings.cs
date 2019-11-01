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
        public int listeningPort = 443;
        public int listenerMaxConnections;
        public int listenerMaxClientDormancy = 1000 * 60;
        public List<string> listenerAcceptedOrigins = new List<string>();
        public List<string> listenerForbiddenIPs = new List<string>();
        public short listenerMaxConnectionsPerIP = -1;
        public short serverFrequency = 25;
        public byte playerMaxNameLength;
        public string serverName = "An unnamed server";
        public string serverGamemode = "FFA";
        public bool chatEnabled = true;
        public List<string> chatFilteredPhrases = new List<string>();
        public int chatCoolDown = 1000;
        public int worldMapX;
        public int worldMapY;
        public int worldMapW = 7071;
        public int worldMapH = 7071;
        public short worldFinderMaxLevel = 16;
        public short worldFinderMaxItems = 16;
        public short worldSafeSpawnTries = 16;
        public double worldSafeSpawnFromEjectedChange = 0.8f;
        public int worldPlayerDisposeDelay = 25 * 60;
        public int worldPlayerBotsPerWorld;
        public List<string> worldPlayerBotNames = new List<string>();
        public List<string> worldPlayerBotSkins = new List<string>();
        public int worldMinionsPerPlayer;
        public int worldMaxPlayers;
        public int worldMinCount;
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
        public int pelletCount;
        public int virusMinCount = 30;
        public int virusMaxCount = 90;
        public int virusSize = 100;
        public int virusFeedTimes = 7;
        public bool virusPushing;
        public int virusSplitBoost = 780;
        public int virusPushBoost = 120;
        public bool virusMontonePops;
        public int ejectedSize = 38;
        public int ejectingLoss = 43;
        public double ejectDispersion = 0.3f;
        public int ejectedCellBost = 780;
        public int motherCellSize = 149;
        public int motherCellCount;
        public double motherCellPassiveSpawnChance = 0.05f;
        public double motherCellActiveSpawnSpeed = 1f;
        public double motherCellActivePelletBoost = 90;
        public int motherCellMaxPellets = 96;
        public int motherCellMaxSize = 65535;
        public double playerRoamSpeed;
        public double playerRoamViewScale = 0.4f;
        public double playerViewScaleMult = 1;
        public double playerMinViewScale = 0.01f;
        public short MaxNameLength = 16;
        public bool playerAllowSkinInName = true;
        public int playerMinSize = 32;
        public int playerSpawnSize = 32;
        public int playerMaxSize = 1500;
        public int playerMinSplitSize = 60;
        public int playerMinEjectSize = 60;
        public short playerSplitCap = 255;
        public double playerEjectDelay = 2;
        public int playerMaxCells = 16;
        public double playerMoveMult = 1;
        public double playerSplitBoost = 780;
        public double playerNoCollideDelay = 14;
        public double playerNoMergeDelay = 15;
        public MergeVersion playerMergeVersion = MergeVersion.Old;
        public double playerMergeTime = 30;
        public double playerMergeTimeIncrease = 0.02f;
        public double playerDecayMult = 0.001f;
    }
}
