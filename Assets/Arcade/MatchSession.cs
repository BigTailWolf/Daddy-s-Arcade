using System;

namespace DaddysArcade
{
    public enum MatchMode { Kid, Parent, ParentChild, PK, WholeFamily }

    // Owns boards, attack/help routing and pause state for all five modes.
    public sealed class MatchSession
    {
        public MatchMode Mode { get; }
        public BlocksRules[] Players { get; }
        public int HelpedRows { get; private set; }
        public int[] SentRows { get; }
        readonly Random attackRandom;
        public bool Finished {
            get { foreach (var player in Players) if (player.Over) return true; return false; }
        }
        public MatchSession(int seed, MatchMode mode)
        {
            Mode = mode;
            int count = mode == MatchMode.WholeFamily ? 3 : mode == MatchMode.ParentChild || mode == MatchMode.PK ? 2 : 1;
            Players = new BlocksRules[count]; SentRows = new int[count];
            attackRandom = new Random(unchecked(seed ^ 0x5A27));
            for (int i=0;i<count;i++) {
                bool kid = mode == MatchMode.Kid || (mode == MatchMode.ParentChild && i == 1) || (mode == MatchMode.WholeFamily && i == 2);
                // PK parents share the same piece sequence; garbage uses separate randomness.
                int playerSeed = (mode == MatchMode.PK || mode == MatchMode.WholeFamily) && i<2 ? seed : unchecked(seed+i);
                Players[i] = new BlocksRules(playerSeed, kid ? PlayMode.Kid : PlayMode.Daddy);
                int player = i;
                Players[i].LinesCleared += lines => OnClear(player, lines);
            }
        }
        void OnClear(int player, int lines)
        {
            if (Finished || lines < 3 || lines > 4) return;
            if (Mode == MatchMode.ParentChild && player == 0)
                HelpedRows += Players[1].ReceiveParentHelp(lines);
            if ((Mode == MatchMode.PK || Mode == MatchMode.WholeFamily) && player < 2) {
                SentRows[player] += Players[1-player].ReceiveAttack(lines, attackRandom.Next(BlocksRules.Width));
                if (Mode == MatchMode.WholeFamily) HelpedRows += Players[2].ReceiveParentHelp(lines);
            }
        }
        public void SetPaused(bool paused) { foreach (var player in Players) player.Paused = paused; }
        public string PlayerTitle(int player)
        {
            if (Mode == MatchMode.Kid) return "KID";
            if (Mode == MatchMode.Parent) return "PARENT";
            if (Mode == MatchMode.ParentChild) return player == 0 ? "PARENT" : "KID";
            if (Mode == MatchMode.WholeFamily && player == 2) return "KID";
            return "PARENT " + (player+1);
        }
        public string Result {
            get {
                if (!Finished) return "";
                if (Mode == MatchMode.PK || Mode == MatchMode.WholeFamily) {
                    if (Mode == MatchMode.WholeFamily && Players[2].Over) return "Kid topped out - round over";
                    if (Players[0].Over && Players[1].Over) return "Draw";
                    return Players[0].Over ? "Parent 2 wins!" : "Parent 1 wins!";
                }
                return Mode == MatchMode.ParentChild ? "Family round over" : "Game Over";
            }
        }
    }
}
