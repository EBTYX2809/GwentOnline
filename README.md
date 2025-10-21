<div align="center">
  <h1> General information</h1>
</div>

It's standalone copy of Gwent from The Witcher 3: Wild Hunt, with posibility play with your friend in online. I very like this card minigame form original game, but playing against bots was boring, so I decided to recreate it and add online functional.

<div align="center">
  <h1> How to play</h1>
</div>

<div align="center">
  
  [Link for download](https://www.youtube.com/watch?v=dQw4w9WgXcQ)

  Unzip folder, and run exe file to start app.

  If server will be offline, you could try to play it locally by guide at server page: [link to server](https://github.com/EBTYX2809/ServerForGwent)
  
</div>

Link to server: https://github.com/EBTYX2809/ServerForGwent

![Menu screenshot](https://github.com/EBTYX2809/GwentOnline/blob/master/GwentMenu.jpg) 


Implied that you will be playing against a friend or acquaintance, because online such a project is obviously about zero, so if you really want to play, call someone and at the same time launch the game + - (after a minute of searching, the session closes). You can recognize each other by name.


![Game screenshot](https://github.com/EBTYX2809/GwentOnline/blob/master/GwentGame.jpg) 


There are only 2 decks to choose from, the North Kingdoms and Nilfgaard, because I think they are the most balanced and interesting. Scoia'tael's faction feature is quite weak, and the Monsters are very monotonous with the mechanics of summoning similar cards to the melee row. 

# Technical details

- Project developed on WPF as client framework.
- The server is implemented using WebSockets and Tcp;
- It put users in sessions of 2 people per game, exchanging moves by turn;
- The interface is completely replicated from the original game;
- Dynamically changing interface thanks to MVVM bindings;
- The game hasn't animations, but there are all the necessary visual markers for players;
- The general game context is implemented as a Singleton, which allowed me to use inheritance from INotifyPropertyChanged for it;
- Different cards are implemented using classic inheritance from an abstract class, which is used in various collections, such as decks or battle rows;
- A modified Prototype pattern is used to clone cards into a deck;
- Card functions are implemented as separate delegates;
- A complex system for counting card points in a row, calculating them according to modifiers with priorities;
- Very convenient classes for the player and combat row, with methods for all basic tasks, implemented using the Facade pattern for comfortable use in higher-level classes;
- Determining the winner of a round and the game is implemented using the Strategy pattern for better code readability;
- Active use of event models in different parts of the game for instant notifications to both players;
- Good synchronization of the game process with the second player through the competent use of asynchronous programming.
- Added localization of cards in 3 different languages.
- Added soundtrack from the original game in the background.
