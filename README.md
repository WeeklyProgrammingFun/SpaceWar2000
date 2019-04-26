# SPACE WAR 2000

Specification for the game and bots are in SPEC.md

This part build with Visual Studio 2019 beta, C#. 

Place your exe in the Staging directory, run WPFRunner. If on close bots are not killed, you can find and kill them with SysInternals ProcessExplorer.

For some example code, the BotBase directory has a C# base bot from which others are derived by overriding the GenMoves function.

To see how the game is simulated, look in the WPFRunner/SpaceWar2K directory, especially files GameRunner.cs and StateUpdater.cs.

Currently the crosstable stuff in the runner is not working. There a likely bugs in the runner; report them or fix them.

Good luck