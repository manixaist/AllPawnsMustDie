# AllPawnsMustDie
Chess Engine GUI for UCI using C# and WinForms

This is a work in progress pre "v1", but is playable.  While the end-goal is to support UCI in general, right now stockfish is the only engine officially supported.  You will have mixed results with other engines, they may work (up to a point) or may not work at all.

## Demo

Here is a recent capture showing basic play (white is human, play is StockFish8).

![](./images/APMD_demo.gif)

## Instructions
* Download [StockFish](https://stockfishchess.org/) to use as a UCI engine
* Clone repo, open solution in Visual Studio (Community 2017 used to develop)
* Build and run solution
* Select "File->Load Engine..." and navigate to stockfish exe. (e.g. X:\ChessEngines\stockfish-8-win\Windows\stockfish_8_x64.exe)
* Select "File->New Game..."
* Choose your color, and optionally change the engine think time

## GamePlay
The interface is simple.  When it's your turn, click on one of your pieces for a set of legal moves to choose from (highlighed in yellow).  If you then click on one of those legal squares, the move is applied and the engine makes the next move, repeat until mate or you quit.

## Documentation

[Preliminary Code Documentation (doxygen)](https://manixaist.github.io/AllPawnsMustDie/)