# Overwatch Match Tracker
Official website at http://owtracker.info/

![overwatch tracker website](https://i.imgur.com/yaC0p57.png)
## What does it do?
My match tracker will seamlessly run as you play competitive Overwatch games and record your stats, such as..
* Current skill rating
* Eliminations
* Objective Eliminations
* Damage done
* Healing done
* Deaths
* Played heroes
* Final score

... and more

## What does it **not** do?
* Store or send any private information
* Store or send any images of your screen (aside from optional playerlist at the start of the game)
* Inject into the game

## How does it work?
The Tracker works by looking at the game screen and applying computer vision then passing it through trained neural networks to analyze and extract data from small parts of the screen.

All processing is done locally through this application, the server only displays the data that the tracker sends nicely for you.

## Examples
![games](http://owtracker.info/images/games.jpg)
![detailed-game](http://owtracker.info/images/detailed-game.jpg)

Live demo on my profile: http://owtracker.info/Avoid
