namespace GameTheory.Games.FiveTribes
{
    public enum Phase : byte
    {
        Bid,
        MoveTurnMarker,
        PickUpMeeples,
        MoveMeeples,
        TileControlCheck,
        TribesAction,
        TileAction,
        CleanUp,
        End,
    }
}
