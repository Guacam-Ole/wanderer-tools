using System.Collections.Immutable;
using System.Text;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Simplify;

namespace PocketBaseTrailReader.Services;

public class GpxSimplificationService : IGpxSimplificationService
{
    private readonly ILogger<GpxSimplificationService> _logger;

    public GpxSimplificationService(ILogger<GpxSimplificationService> logger)
    {
        _logger = logger;
    }

    public byte[] Simplify(byte[] gpxData, double minDistanceMeters)
    {
        var gpxText = Encoding.UTF8.GetString(gpxData);
        var gpxFile = GpxFile.Parse(gpxText, null);

        if (gpxFile.Tracks.Count == 0)
        {
            _logger.LogWarning("GPX file contains no tracks, returning original");
            return gpxData;
        }

        // Get latitude from first trackpoint for meter→degree conversion
        var firstPoint = gpxFile.Tracks
            .SelectMany(t => t.Segments)
            .SelectMany(s => s.Waypoints)
            .FirstOrDefault();

        if (firstPoint == null)
        {
            _logger.LogWarning("GPX file contains no trackpoints, returning original");
            return gpxData;
        }

        var latitudeRad = firstPoint.Latitude.Value * Math.PI / 180.0;
        var toleranceDegrees = minDistanceMeters / (111_320 * Math.Cos(latitudeRad));

        _logger.LogDebug("Using tolerance: {Meters}m ≈ {Degrees}° at latitude {Lat}",
            minDistanceMeters, toleranceDegrees, firstPoint.Latitude.Value);

        var geometryFactory = new GeometryFactory();
        var simplifiedTracks = new List<GpxTrack>();

        foreach (var track in gpxFile.Tracks)
        {
            var simplifiedSegments = new List<GpxTrackSegment>();

            foreach (var segment in track.Segments)
            {
                var waypoints = segment.Waypoints.ToList();
                if (waypoints.Count < 2)
                {
                    simplifiedSegments.Add(segment);
                    continue;
                }

                var coordinates = waypoints
                    .Select(wp => new CoordinateZ(
                        wp.Longitude.Value,
                        wp.Latitude.Value,
                        wp.ElevationInMeters ?? double.NaN))
                    .ToArray();

                var lineString = geometryFactory.CreateLineString(coordinates);
                var simplified = DouglasPeuckerSimplifier.Simplify(lineString, toleranceDegrees);

                var simplifiedWaypoints = new ImmutableGpxWaypointTable(
                    simplified.Coordinates.Select(c => new GpxWaypoint(
                        longitude: new GpxLongitude(c.X),
                        latitude: new GpxLatitude(c.Y),
                        elevationInMeters: double.IsNaN(c.Z) ? null : c.Z)));

                simplifiedSegments.Add(new GpxTrackSegment(simplifiedWaypoints, null));
            }

            simplifiedTracks.Add(new GpxTrack(
                track.Name,
                track.Comment,
                track.Description,
                track.Source,
                track.Links,
                track.Number,
                track.Classification,
                track.Extensions,
                simplifiedSegments.ToImmutableArray()));
        }

        var simplifiedGpxFile = new GpxFile
        {
            Metadata = gpxFile.Metadata
        };

        foreach (var track in simplifiedTracks)
            simplifiedGpxFile.Tracks.Add(track);

        foreach (var waypoint in gpxFile.Waypoints)
            simplifiedGpxFile.Waypoints.Add(waypoint);

        foreach (var route in gpxFile.Routes)
            simplifiedGpxFile.Routes.Add(route);

        return Encoding.UTF8.GetBytes(simplifiedGpxFile.BuildString(null));
    }
}
