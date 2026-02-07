using PocketBaseTrailReader.Models;

namespace PocketBaseTrailReader.Services;

public interface ITrailService
{
    Task<byte[]> DownloadGpxAsync(Trail trail);
    Task UploadGpxAsync(Trail trail, byte[] gpxData, string fileName);
    Task ReduceGpx();
}
