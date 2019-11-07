# OgarII-CSharp
OgarII server ported completely to c#
Planning to implement everything from luka's OgarII

Currently Missing :

Teams mode,

Last Man Standing,

Mother Cells,

Commands

Bugs :
Minions don't work

Notes :
Not much of LegacyProtocol has been tested and ModernProtocol has not been tested at all.

Perfomance Differences :

(the values are the average tick time)

OgarII JS Version (Node.JS v10.16.3)

10,000 pellets & 1000 player bots :

1 min : 134.17667619393 ms,	

3 min : 156.19445411131 ms

100,000 pellets & 1000 player bots :

1 min : 1396.1630052051 ms,	

3 min : 1854.4840031809 ms

OgarII C# Version (.Net Core 3.0 (Debugging))

10,000 pellets & 1000 player bots :

1 min : 70.845972900763 ms,  

3 min : 82.097626611226 ms


100,000 pellets & 1000 player bots :

1 min : 679.52536666667 ms,

3 min : 1065.2625875969 ms

up to 2x as much of a perfomance gain.
