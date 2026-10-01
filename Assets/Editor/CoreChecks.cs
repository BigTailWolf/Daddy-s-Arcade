using System;
using UnityEditor;
using UnityEngine;
using DaddysArcade;
using PlayMode = DaddysArcade.PlayMode;

public static class CoreChecks
{
    public static void RunBatch()
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.playModeStateChanged += state => {
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            try { Run(); EditorApplication.Exit(0); }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        };
        EditorApplication.EnterPlaymode();
    }
    [MenuItem("Daddy's Arcade/Run Core Checks")]
    public static void Run()
    {
        int[] rewards = {0,100,300,600,1000};
        for (int lines=0; lines<=4; lines++) Require(BlocksRules.LineReward(lines) == rewards[lines], "Line reward " + lines);
        var kid = new BlocksRules(42, PlayMode.Kid);
        var dad = new BlocksRules(42, PlayMode.Daddy);
        for(int i=0;i<4;i++) { kid.Tick(.2); dad.Tick(.2); }
        Require(kid.Y == 0 && dad.Y == 1, "Mode gravity difference");
        while(kid.Move(0,1)) { }
        kid.StepDown();
        Require(kid.Board[4,19] == 0, "Kid drop does not bypass grace");
        for(int i=0;i<4;i++) kid.Tick(.2);
        Require(kid.Y == 0, "Kid locks after grace and spawns");
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
        CheckFamilyAndControls();
        CheckMatchModes();
        CheckNextPiece();
        CheckMusic();
        if (Application.isBatchMode) ControllerChecks.Run();
        Debug.Log("Daddy's Arcade core checks passed.");
    }
    static void PrepareClear(BlocksRules board, int lines)
    {
        Require(board.Kind == 0, "Test setup has an I piece");
        board.Rotate();
        for (int y=20-lines;y<20;y++)
            for (int x=0;x<10;x++) if (x!=5) board.Board[x,y] = 1;
    }
    static void CheckMatchModes()
    {
        int seed=0;
        while (new BlocksRules(seed).Kind != 0) seed++;
        int[] counts = { 1,1,2,2,3 };
        foreach (MatchMode mode in Enum.GetValues(typeof(MatchMode))) {
            var match = new MatchSession(seed,mode);
            Require(match.Players.Length == counts[(int)mode], "Five mode player counts");
            for (int i=0;i<match.Players.Length;i++) {
                bool kid = mode==MatchMode.Kid || (mode==MatchMode.ParentChild && i==1) || (mode==MatchMode.WholeFamily && i==2);
                Require(match.Players[i].Mode == (kid ? PlayMode.Kid : PlayMode.Daddy), "Correct role gravity");
            }
            match.SetPaused(true);
            foreach (var player in match.Players) {
                player.HardDrop(); player.Tick(.25);
                Require(player.StackHeight==0 && player.Paused, "All players pause");
                Require(player.ReceiveAttack(4,0)==0, "Paused attacks ignored");
            }
            match.SetPaused(false);
            foreach (var player in match.Players) Require(!player.Paused, "All players resume");
        }
        foreach (int lines in new[] { 1,2,3,4 }) {
            foreach (MatchMode mode in new[] { MatchMode.PK, MatchMode.WholeFamily }) {
                for (int attacker=0;attacker<2;attacker++) {
                    var match = new MatchSession(seed,mode);
                    int expected = Math.Max(0,lines-2);
                    if (mode==MatchMode.WholeFamily) match.Players[2].Board[0,11]=7;
                    PrepareClear(match.Players[attacker],lines);
                    int opponent=1-attacker;
                    match.Players[attacker].HardDrop();
                    Require(match.Players[opponent].PendingRows==expected && match.Players[opponent].StackHeight==0,
                        "Attack both directions, no height threshold, deferred application");
                    Require(match.SentRows[attacker]==expected && match.Players[attacker].PendingRows==0, "Attack never targets self");
                    if (mode==MatchMode.WholeFamily) {
                        Require(match.Players[2].PendingRows==0 && match.Players[2].StackHeight==9-expected && match.HelpedRows==expected,
                            "Each parent simultaneously attacks opponent and helps protected kid");
                    }
                    match.Players[opponent].HardDrop();
                    Require(match.Players[opponent].PendingRows==0, "Incoming rows applied on lock");
                    for (int y=20-expected;y<20;y++) {
                        int filled=0;
                        for (int x=0;x<10;x++) if(match.Players[opponent].Board[x,y]==BlocksRules.GarbageColor) filled++;
                        Require(filled==9, "Garbage rows have exactly one gap");
                    }
                }
            }
        }
        var noCancel = new MatchSession(seed,MatchMode.PK);
        PrepareClear(noCancel.Players[0],4); PrepareClear(noCancel.Players[1],4);
        noCancel.Players[0].HardDrop(); noCancel.Players[1].HardDrop();
        Require(noCancel.Players[1].StackHeight==2 && noCancel.Players[0].PendingRows==2,
            "Own clear sends attack without cancelling incoming rows");
        var topOut = new MatchSession(seed,MatchMode.PK);
        topOut.Players[1].Board[0,0]=1;
        topOut.Players[1].ReceiveAttack(3,4); topOut.Players[1].HardDrop();
        Require(topOut.Finished && topOut.Players[1].Over && topOut.Result=="Parent 1 wins!", "Garbage overflow ends round with correct winner");
        var kidClearSeed=0;
        while(new BlocksRules(kidClearSeed+2).Kind!=0) kidClearSeed++;
        var protectedKid = new MatchSession(kidClearSeed,MatchMode.WholeFamily);
        PrepareClear(protectedKid.Players[2],4); protectedKid.Players[2].HardDrop();
        Require(protectedKid.Players[0].PendingRows==0 && protectedKid.Players[1].PendingRows==0,
            "Kid does not attack parents");
        var threshold = new MatchSession(seed,MatchMode.WholeFamily);
        threshold.Players[2].Board[0,12]=1;
        PrepareClear(threshold.Players[0],4); threshold.Players[0].HardDrop();
        Require(threshold.Players[2].StackHeight==8 && threshold.HelpedRows==0 && threshold.Players[1].PendingRows==2,
            "Whole-family aid uses strict eight-row threshold independently of PK");
        Debug.Log("Daddy's Arcade five-mode checks passed.");
    }
    static void CheckMusic()
    {
        foreach (bool energetic in new[] { false, true }) {
            float[] audio = ArcadeMusic.Compose(energetic);
            Require(audio.Length > ArcadeMusic.SampleRate*30, "Music has a full loop");
            double energy = 0;
            foreach (float sample in audio) {
                Require(!float.IsNaN(sample) && !float.IsInfinity(sample) && Math.Abs(sample)<.99f,
                    "Music samples are finite and do not clip");
                energy += sample*sample;
            }
            Require(Math.Sqrt(energy/audio.Length) > .02, "Music is audible, not silent");
            Require(Math.Abs(audio[0]-audio[audio.Length-1])<.01, "Loop seam has no amplitude jump");
        }
        Debug.Log("Daddy's Arcade music checks passed.");
    }
    static void CheckNextPiece()
    {
        for (int seed=0;seed<30;seed++) {
            var board = new BlocksRules(seed);
            var expectedSequence = new System.Random(seed);
            Require(board.Kind == expectedSequence.Next(7), "Preview preserves seeded piece sequence");
            for (int turn=0;turn<12;turn++) {
                int next = board.NextKind;
                Require(next == expectedSequence.Next(7), "Next piece follows seeded sequence");
                int[] cells = new int[8];
                for (int i=0;i<4;i++) board.NextCell(i,out cells[i*2],out cells[i*2+1]);
                board.Move(-1,0); board.Rotate();
                board.Paused = true; board.HardDrop(); board.Tick(.25);
                Require(board.NextKind == next, "Movement, rotation and pause preserve preview");
                board.Paused = false; board.HardDrop();
                Require(board.Kind == next && board.Rotation == 0, "Hard drop spawns the previewed piece");
                for (int i=0;i<4;i++) {
                    board.Cell(i,0,out int x,out int y);
                    Require(x == cells[i*2] && y == cells[i*2+1], "Preview geometry matches spawn");
                }
                Array.Clear(board.Board,0,board.Board.Length);
            }
        }
        var family = new FamilySession(42);
        int childNext = family.Child.NextKind, childKind = family.Child.Kind;
        family.Parent.HardDrop();
        Require(family.Child.NextKind == childNext && family.Child.Kind == childKind, "Family previews are independent");
    }
    static void CheckFamilyAndControls()
    {
        var rotated = new BlocksRules(42);
        rotated.Rotate(1); rotated.Rotate(-1);
        Require(rotated.Rotation == 0, "Opposite rotations undo each other");
        rotated.Paused = true;
        rotated.HardDrop(); rotated.Rotate(-1);
        Require(rotated.StackHeight == 0 && rotated.Rotation == 0, "Paused hard drop and rotation ignored");
        rotated.Paused = false;
        rotated.HardDrop();
        Require(rotated.StackHeight > 0 && rotated.Y == 0, "Hard drop locks and spawns immediately");
        var gentle = new BlocksRules(42, PlayMode.Kid);
        gentle.HardDrop();
        Require(gentle.StackHeight > 0, "Explicit hard drop bypasses kid grace");
        foreach (int height in new[] { 0, 8, 9, 15 }) {
            foreach (int lines in new[] { 1, 2, 3, 4 }) {
                var child = new BlocksRules(42, PlayMode.Kid);
                for (int row = BlocksRules.Height-height; row < BlocksRules.Height; row++) child.Board[0,row] = 1;
                int expected = height > 8 && lines >= 3 ? lines-2 : 0;
                int score = child.Score;
                Require(child.ReceiveParentHelp(lines) == expected, "Help threshold / clear count");
                Require(child.StackHeight == height-expected, "Help collapses bottom rows");
                Require(child.Score == score, "Help does not award child line points");
                for (int i=0;i<4;i++) {
                    child.Cell(i,child.Rotation,out int cx,out int cy);
                    Require(child.Board[child.X+cx,child.Y+cy] == 0, "Help leaves active piece valid");
                }
            }
        }
        for (int lines=3;lines<=4;lines++) {
            int seed = 0;
            while (new BlocksRules(seed).Kind != 0) seed++;
            var session = new FamilySession(seed);
            session.Child.Board[0,11] = 7; session.Child.Board[1,19] = 6;
            session.Parent.Rotate(); // Vertical I occupies column 5.
            for (int row=20-lines;row<20;row++)
                for(int col=0;col<10;col++) if(col != 5) session.Parent.Board[col,row]=1;
            session.Parent.HardDrop();
            Require(session.Parent.Score == BlocksRules.LineReward(lines), "Parent clear integration");
            Require(session.HelpedRows == lines-2 && session.Child.Board[0,11+lines-2] == 7,
                "Parent lock triggers child assistance");
            Require(session.Child.Board[1,19] == 0, "Assistance removes the actual bottom cells");
            session.SetPaused(true);
            int parentY=session.Parent.Y, childY=session.Child.Y;
            session.Parent.Tick(.25); session.Child.Tick(.25);
            Require(session.Parent.Y == parentY && session.Child.Y == childY && session.Child.ReceiveParentHelp(4)==0,
                "Shared pause freezes both boards and help");
            session.SetPaused(false);
            Require(!session.Parent.Paused && !session.Child.Paused, "Shared resume");
        }
        int chosen = -1;
        var menu = new ArcadePauseMenu(() => chosen=0, () => chosen=1, () => chosen=2,
            new ArcadePauseMenu.Option("Game option", () => chosen=3));
        menu.Confirm(); Require(chosen==0, "Resume default");
        menu.Move(1); menu.Confirm(); Require(chosen==1, "Game main menu route");
        menu.Move(1); menu.Confirm(); Require(chosen==2, "Arcade exit route");
        menu.Move(1); menu.Confirm(); Require(chosen==3, "Game-specific pause option");
        menu.Move(1); Require(menu.Selected==0, "Menu wraps");
    }
    static void Require(bool passed, string label) { if (!passed) throw new Exception(label); }
}
