public enum PeekDirection { NorthWest, NorthEast, SouthWest, SouthEast }

public static class PeekDirectionExtensions
{
    public static PeekDirection Opposite(this PeekDirection dir)
    {
        switch (dir)
        {
            case PeekDirection.NorthWest: return PeekDirection.SouthEast;
            case PeekDirection.NorthEast: return PeekDirection.SouthWest;
            case PeekDirection.SouthWest: return PeekDirection.NorthEast;
            default: return PeekDirection.NorthWest;
        }
    }

    public static bool IsEast(this PeekDirection dir) => dir == PeekDirection.NorthEast || dir == PeekDirection.SouthEast;
}
