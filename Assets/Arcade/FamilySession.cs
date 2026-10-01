namespace DaddysArcade
{
    // Each session owns its subscriptions; discarded sessions cannot affect a new game.
    public sealed class FamilySession
    {
        readonly MatchSession session;
        public BlocksRules Parent => session.Players[0];
        public BlocksRules Child => session.Players[1];
        public int HelpedRows => session.HelpedRows;
        public FamilySession(int seed)
        {
            session = new MatchSession(seed, MatchMode.ParentChild);
        }
        public void SetPaused(bool paused) => session.SetPaused(paused);
    }
}
