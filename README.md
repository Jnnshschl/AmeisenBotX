# AmeisenBotX
Hopefully the last rewrite of my WoW bot. I've learned much during the other 2 attempts so i decided to rewrite it from scratch. The aim of this bot is to create a human like group player that assists you on your adventure. From managing it's equipment to gathering resources the bot should do anything on its own.

Currently supported versions:

* 3.3.5a 12340

## Achievements

### Dungeons/Raids

None yet...

## Supported Stuff

### Combat Classes

**Class** | | | |
------------ | ------------- | ------------- | -------------
Druid | ❌ Balance | ❌ Feral | ❌ Restoration 
Hunter | ❌ Marksmanship | ✔️ Beast Mastery | ❌ Survival
Mage | ✔️ Fire | ❌ Frost | ❌ Arcane
Paladin | ✔️ Holy | ❌ Retribution | ⚠️ Protection
Priest | ✔️ Holy | ❌ Discipline | ❌ Shadow
Rogue | ❌ Combat | ❌ Assasination | ❌ Sublety
Shaman | ❌ Elemental | ❌Enhancement | ❌ Restoration
Warlock | ❌ Affliction | ❌ Demonology | ❌ Destruction
Warrior | ⚠️ Arms | ✔️ Fury | ❌ Protection
Death Knight | ⚠️ Blood | ❌ Frost | ❌ Unholy

❌ Not supported
⚠️ Work in Progress
✔️ Supported

## Screenshots

... will follow.

## Setup Guide

### 1. The Bot

#### Prerequisites

Stuff you need:
* **Visual Studio 2019* (if you want to compile the bot)
  * .NET SDK 4.8
* .NET Runtime 4.8
* Wow 3.3.5a 12340 Client
* AmeisenNavigation Server (https://github.com/Jnnshschl/AmeisenNavigation)
  * 3.3.5a MMAPS (Movement Maps from TrinityCore)
* Some sort of Texteditor to edit JSON configs

#### Compilation

Clone the Project:
```shell
git clone https://github.com/Jnnshschl/AmeisenBotX.git
```

Open the Project in Visual Studio, right click on the Solution and restore its NuGet packages.

Press F5, if everything goes right the bot should start a few seconds later.

⚠️ **You may need to re/start Visual Studio with administrator rights.**

### 2. Navigation Server

#### Prerequisites

The AmeisenNavigation Server is needed to provide pathfinding for the bot, without it the bot won't move (in it's current state, maybe changed later). You may download the binary directly from here https://github.com/Jnnshschl/AmeisenNavigation/releases or compile it yourself like this:

#### Compilation

*Skip this step if you downloaded the binary from releases.*

Stuff you need:
* Visual Studio 2019
  * .NET SDK 4.8
  * Visual C++ SDK
  * C++/CLR SDK
* 3.3.5a MMAPS (Movement Maps from TrinityCore)

Clone the Project:
```shell
git clone https://github.com/Jnnshschl/AmeisenBotX.git
```

Select the ```AmeisenNavigation.Server``` project as the Startup-Project.

⚠️ **Make sure to set the build configuration to ```Release``` to increase the performance of the Server.**

Press F5, if everything goes right the bot should start a few seconds later.

If the compilation fails, try to manually build the project in this order:

1. recastnavigation
2. AmeisenNavigation
3. AmeisenNavigation.Wrapper
4. AmeisenNavigation.Server

#### Movement Maps (MMAPS)

Generate them yourself using the generator from the TrinityCore project: https://github.com/TrinityCore/TrinityCore

Or use the ones supplied in this repack, go check it out it's great: http://www.ac-web.org/forums/showthread.php?211443-Official-AC-Web-Ultimate-Repack-(3-3-5a)(Eluna-Engine)

Extract the mmaps to a folder where the server is able to read them. The folder should contain many ```*.mmap``` and ```*.mmtile``` files named with only numbers.

```
C:/mmaps/
└ 000.mmap
└ 001.mmap
└ ...
└ 0002239.mmtile
└ 0002240.mmtile
└ ...
```

#### Configuration

The first thing you may see is an error message saying that you should configure the mmaps folder. This can be done in the created ```config.json```. Open it with a Texteditor of your choice and change the ```mmapsFolder``` entry to the folder where you extracted the mmaps to.

```json
{
    "mmapsFolder":"C:\\mmaps\\",
    "ipAddress":"0.0.0.0",
    "port":47110,
    "preloadMaps":[],
    "logToFile":false,
    "removeOldLog":true,
    "logFilePath":"C:\\AmeisenNavigation\\log.txt",
    "logLevel":0
}
```

Now you're able to start the server and put it in the background. The bot is going to connect to it via TCP.

### 3. Running the Bot

Make sure the NavigationServer is reachable by the bot and you're ready to start it. If its the first start, the only option to choose is ```New Profile```, select that and fill in the required information. After you created your profile the bot will handle everything on its own. Next time you start the bot, your profile will be an option in the combobox to select.

⚠️ **At the moment you need to configure autostarting and autologin, otherwise the bot won't do anything.**

#### Configuration File

```json
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
// highly recommended to set this to true
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

## Project Structure

**AmeisenBot** : Contains the GUI

**AmeisenBot.Core** : Core logic of the bot

**AmeisenBot.Memory** : Memory editing related stuff

**AmeisenBot.Pathfinding** : Pathfinding clients

**AmeisenBot.Test** : Unit-Tests