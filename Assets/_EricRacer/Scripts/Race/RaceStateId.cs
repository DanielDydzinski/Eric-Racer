namespace EricRacer.Race
{
    public enum RaceStateId : byte
    {
        /// <summary>No race scene running (menu, loading).</summary>
        None,
        /// <summary>Karts on the grid, waiting for everyone to finish loading.</summary>
        Grid,
        Countdown,
        Racing,
        Results
    }
}
