namespace PocketBaseTrailReader.Services;

public interface IGpxSimplificationService
{
    byte[] Simplify(byte[] gpxData, double minDistanceMeters);
}
