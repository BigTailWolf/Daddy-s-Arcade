using System;
using UnityEditor;
using UnityEngine;
using DaddysArcade;

public static class CoreChecks
{
    [MenuItem("Daddy's Arcade/Run Core Checks")]
    public static void Run()
    {
        var game = new BlocksRules(42);
        while (game.Move(-1,0)) { }
        int x = game.X;
        Require(!game.Move(-1,0) && game.X == x, "Wall collision");
        game.Paused = true;
        int y = game.Y;
        for(int i=0;i<100;i++) game.Tick(.1);
        Require(game.Y == y && !game.Move(1,0), "Pause freezes rules");
        game.Paused = false;
        for(int col=0;col<BlocksRules.Width;col++) {
            game.Board[col,19] = 1; game.Board[col,18] = 2;
        }
        game.Board[3,17] = 7;
        Require(game.ClearLines() == 2 && game.Board[3,19] == 7, "Multiple line collapse");
        var a = new BlocksRules(17); var b = new BlocksRules(17);
        for(int i=0;i<1000;i++) {
            Require(a.Kind == b.Kind && a.X == b.X && a.Y == b.Y && a.Over == b.Over, "Seed reproducibility");
            a.StepDown(); b.StepDown();
        }
        Require(a.Over, "Stack reaches game over");
        Require(!new BlocksRules(17).Over, "Fresh session");
        Debug.Log("Daddy's Arcade core checks passed.");
    }
    static void Require(bool passed, string label) { if (!passed) throw new Exception(label); }
}
