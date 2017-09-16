# AllPawnsMustDie
Chess Engine GUI for UCI using C# and WinForms

[Code Documentation (doxygen)](https://manixaist.github.io/AllPawnsMustDie/)

## Instructions
* Download [StockFish](https://stockfishchess.org/) to use as a UCI engine
* Clone repo, open solution in Visual Studio (Community 2017 used to develop)
* Build and run solution
* Select "File->Load Engine..." and navigate to stockfish exe. (e.g. X:\ChessEngines\stockfish-8-win\Windows\stockfish_8_x64.exe)
* Select "File->New Game..."
* Choose your color

At this point you can click on a piece to highlight the valid moves. If you select a valid move, the piece will update and the engine will make its move.  This continues until you quit or no moves can be made.

This is a work in progress.  Here is a recent capture showing basic play (white is human, play is StockFish8).

*Demo*

![](./images/APMD_demo.gif)
