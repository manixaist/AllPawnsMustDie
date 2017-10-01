# All Pawns Must Die

[![forthebadge](http://forthebadge.com/images/badges/winter-is-coming.svg)](http://forthebadge.com)
[![forthebadge](http://forthebadge.com/images/badges/built-with-love.svg)](http://forthebadge.com)

[![MIT Licence](https://badges.frapsoft.com/os/mit/mit.png?v=103)](https://opensource.org/licenses/mit-license.php)
[![Open Source Love](https://badges.frapsoft.com/os/v1/open-source.svg?v=103)](https://github.com/ellerbrock/open-source-badges/)

**All Pawns Must Die** is a simple GUI for chess engines implementing the UCI protocol.  It is written in C# and uses Windows Forms.

### Released Versions
[v1.0](https://github.com/manixaist/AllPawnsMustDie/releases/tag/v1.0)

### Gameplay (v1.0)
The interface is simple by design.  When it's your turn, click on one of your pieces for a set of legal moves to choose from (highlighted in yellow).  If you then click on one of those legal squares, the move is applied and the engine makes the next move, repeat until mate, draw or you quit.  The last move is always highlighted in blue with an arrow to show from->to.

![](./images/APMD_gameplay_v1.gif)

### Requirements
It's C# and Windows Forms, which means you will need Windows and the .NET Framework (some version is likely installed if you have Windows.) AllPawnsMustDie targets version 4.5.2, which is fairly old.  Bottom line, if you're running Windows 8+ you're probably fine.

### Instructions
* Grab a release binary above, or clone and build the repo ([Visual Studio](https://www.visualstudio.com/) 2017 Community Edition works and is free)
* Download a chess engine that implements the UCI protocol, such as [StockFish](https://stockfishchess.org/)
* Launch the application
* Select "File->Load Engine..." and navigate to engine.exe (e.g. X:\ChessEngines\stockfish-8-win\Windows\stockfish_8_x64.exe)
* Select "File->New Game..."
* Choose your color, and optionally change the engine think time
* [Optional] You can start from any valid position by supplying a FEN via "File->New Position..."
* [Optional] You can watch the engine play itself via "File->Self Play"
* [Optional] You can attempt to reduce the play strength of the engine, though this is a limited feature right now
* [Optional] You can get the current FEN any time from "Edit->Show FEN"

### Blog Entries
* [01: All Pawns Must Die](http://manixaist.com/coding/csharp/game/chess/uci/2017/09/29/APMD-01.html)
* [02: Componentization](http://manixaist.com/coding/csharp/game/chess/uci/2017/09/29/APMD-02.html)
* [03: UCI Chess Engine Wrapper](http://manixaist.com/coding/csharp/game/chess/uci/2017/09/29/APMD-03.html)
* [04: ChessGame, The Data...Sorta](http://manixaist.com/coding/csharp/game/chess/uci/2017/09/30/APMD-04.html)
* [05: ChessBoard & ChessPiece](http://manixaist.com/coding/csharp/game/chess/uci/2017/09/30/APMD-05.html)
* [06: FEN & The FenParser](http://manixaist.com/coding/csharp/game/chess/uci/2017/10/01/APMD-06.html)

### Documentation
[Code Documentation (generated with doxygen)](https://manixaist.github.io/AllPawnsMustDie/)
