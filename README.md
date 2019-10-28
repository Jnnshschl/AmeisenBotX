# AmeisenBotX
Hopefully the last rewrite of my WoW bot. I've learned much during the other 2 attempts so i decided to rewrite it from scratch.

Currently supported versions:

* 3.3.5a 12340

## Structure

**AmeisenBot** : Contains the GUI

**AmeisenBot.Core** : Core logic of the bot

**AmeisenBot.Memory** : Memory editing related stuff

**AmeisenBot.Pathfinding** : Pathfinding clients

**AmeisenBot.Test** : Unit-Tests

## Configuration

```
// use the auto start/restart feature
AutostartWow = false;
PathToWowExe = string.Empty;

// use the autologin feature
AutoLogin = false;

// autologin details
Username = string.Empty;
Password = string.Empty;
CharacterSlot = 0;

// example: WarriorArms, PriestHoly, ...
CombatClassName = "ClassSpec";

// following stuff
FollowGroupLeader = false;
FollowGroupMembers = false;
FollowSpecificCharacter = false;
SpecificCharacterToFollow = string.Empty;
MaxFollowDistance = 100;
MinFollowDistance = 6;

// see my repo AmeisenNavigation
// https://github.com/Jnnshschl/AmeisenNavigation
NavmeshServerIp = "127.0.0.1";
NameshServerPort = 47110;

// time after which all objects are updated
ObjectUpdateMs = 100;

// statemachine update tick
StateMachineTickMs = 50;

// increase performance by caching names and reactions
// highly reccomended to set this to true
PermanentNameCache = true;
PermanentReactionCache = true;

// restore the positions of the windows
SaveBotWindowPosition = false;
SaveWowWindowPosition = false;

// misc stuff
ReleaseSpirit = false;
UseClickToMove = true;

// if set to true the bot will run out of AoE Spells
AutoDodgeAoeSpells = false;
```
