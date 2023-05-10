# fsm97trainer
Patch and trainer for EA's FIFA Soccer Manager (1997). 

## Requirements

Requires .Net 4 (included in Windows 10).  

.Net 4.8 Recommended (https://dotnet.microsoft.com/zh-cn/download/dotnet-framework/thank-you/net48-web-installer)

For Windows XP users .Net 4.0 can be downloaded from https://www.microsoft.com/en-us/download/details.aspx?id=17851 

## How to install

Unzip the files to anywhere. 

## Patching the executable

Run the patcher, enter the path to the menus.exe in game folder, or click the browse button to select the file. Choose the traning effects you want. Then click Patch.

To remove the patch, enter the path to menus.exe, then click Unpatch. 

## Features

### Patcher

Patch modifies the game's executable. Currently only supports the Chinese and English version of the game. The English 97/98 season patch is supported.

#### Changing bench size

By default English leagues allows only 3 players on the bench. This creates a hassle to constantly add/remove benches players for continental competitions where the bench size is 5. The patch set the bench size to 5 at all times.

#### Changing training effects

This would let you choose to have previously untrainable attibutes trainable. For example, due to a cap on leadership at 74, respawn players can only be trained to 98 at the CD position.
The 2X effect would double the training speed, letting you to max a player in 5-8 years instead of 10-16 years. This would open the door to versatile training. In addition, some training type now can have extra effects, making them less useless. 

### Trainer

#### Scouting tools

Exporting player data exports player data in csv format, you can then use your favorite csv tool (for example, Microsoft Excel) to sort and filter data. Attributes unable to be seen in the in-game transfer market, such as individual attributes in the speed/tackling/goalkeeping groups, best position and best position rating (in case computer moved Ronaldo to CD), would be seen in the csv file. The csv file also has the player's age, position, rating and nationality in case you want to purchase a player in game.

Importing player would allow you to import player data back after modifying in your favorite csv tool. This makes batch modifying possible and also can help you to reassign your player's numbers (automatic numbering sucks). 

Copying data would take a snapshot of the current player data and pasting would restore the player data. Note this may cause you to lose your training progress. This can help if you want to transfer the training progress to a respawn when one of your favorite old player retires. 

#### Quality of life improvements

No Absense would make player available in the next game after an injury or red card. You still have to move the player back to rotation though. Auto rotation would help on that. It can rotate by energy (sometimes this makes an unused goalkeeper to play out of position) or by statistics. 

Max strength and Max energy makes game less interrupted by injury.

Auro contract renewal disables the contract renewal reminders and renew the contract for you. 

Auto Train would adjust the player's training schedule automatically. It would train for another position if the current one is maxed out, unless you disable that. Converting to GK would be another option to minimize traing injury but players don't really like that and may threaten to retire and the club may threat to fire you (but never will). 

Due to the way training works a player could have best position changed (e.g. LRB and RWB could become LW and RW if you do sprint training which is also essential for wing backs) be trained and complain about it. 

Auto position would change every player to their best position. Well, sort of. Sometimes you want a fixed formation instead of letting players choose their position freely. You can set your formation in game (you don't really need to care if you put a CD in a forward position), then click the Save Current Formation button and check the Auto Position using Saved Formation checkbox. The next time you do auto positon or auto rotation, the saved formation would be used. You don't want to limit the player's free will if a player threatened to retire, however, and you should do Auto Position without formation when you enter the offseason. 

#### Improving the computer

The computer does not train. This makes late game matchs a cake walk. If you want some challenges you can improve computer player attributes. The Boost Youth Player feature would increase the attributes of all respawn players (age <20 and contract weeks between 96 and 144) by 25. The Improve All Players feature would increase the attributes of all players by 1. You can use it to compensate the attribute cap on respawn players and lack of training for computer players. 

#### Updating Player Names Online

When an old player retires a new player would be respawn with the same last name. This makes the player data gradually obselete. 

The Updating Respawn Names Online feature (should only be run at the starting of a new season) would query Wikipedia for players born 17 years ago (if not found, then players born since 1960). Due to the limitation of data format, female players would also be available. 

Together with the reset date feature (should only be run in the offseason) you can choose to go back to a few decades before the game officially started and have fitting player names from that era. 

#### Game bugs

The game has an year 2079 bug where all players would suddenly become age 90+ due to birth date being stored as a WORD and thus capped at 65535 days after December, 30, 1899. This would begin a 16 year dark period where every single player in the game would have a high chance to retire next season, starting from a club's main team until only 16 players left in the club. Computers would also lose the ability to purchase players off the transfer market during this period. After the 16 years, the game would be full of youth players and each year a smaller and smaller amount of players retire as respwns making their retirement possible. Around 2105, when all old players from the 16 years retired but no new player aged enough to retire, the game would freeze at the begining of the next season due to lack of respawns. 

To fix this bug, Date reset would reset the date to the start but keeping player ages intact. This should only be done in the offseason, though, as changing date in the middle of a season would disrupt competition schedule.

The game has a buffer overrun bug at 40 players (39 players in the official 97/98 season patch). When your team becomes large, sometimes you would see a player with number 40 or 39 joins your team as the result of poorly planned player transfer or respawn. At this time, your game state becomes corrupted, random events will happen weekly until you are fired and game progress deleted shortly after.

The only way to savalge the game save is to force exit the game process before the current game state is written to file, which the game does when you quit using the game's interface. You can instead close the game using the Task Manager tool in Windows, but the trainer provides a conviently wa to force exit the game.




