using System.Security.Cryptography;
using LLMGameCreator.Application.Design.MediaMaterializationReviewPackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.MediaMaterializationReviewPackage;

public sealed class MediaFixtureWritersTests
{
    [Fact]
    public async Task PhysicalMediaFilesAreWrittenWithValidPngAndWavHeaders()
    {
        using var temp = new TempMediaMaterializationProject();
        var service = MediaMaterializationReviewPackageTestFactory.CreateService();
        var result = service.Build(MediaMaterializationReviewPackageTestFactory.FindRepoRoot());

        var write = await service.WriteAsync(temp.Path, result);

        Assert.True(result.MaterializedMediaInventory.Passed);
        Assert.True(result.MaterializedMediaInventory.PngFileCount >= 9);
        Assert.Equal(3, result.MaterializedMediaInventory.WavFileCount);
        Assert.Equal(result.MaterializedMediaInventory.FileCount, result.MaterializationQueue.QueueItemCount);

        foreach (var file in result.MaterializedMediaInventory.Files)
        {
            var path = Path.Combine(write.OutputDirectoryPath, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(path), "Missing materialized media file: " + file.RelativePath);
            var bytes = await File.ReadAllBytesAsync(path);
            Assert.Equal(file.ByteLength, bytes.LongLength);
            Assert.Equal(file.Sha256, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());

            if (file.MaterializedMediaFormat == "png")
            {
                Assert.True(MediaFixtureWriters.HasValidPngSignature(bytes), "Invalid PNG signature: " + file.RelativePath);
                Assert.True(MediaFixtureWriters.ValidatePngChunkCrcs(bytes), "Invalid PNG CRCs: " + file.RelativePath);
            }

            if (file.MaterializedMediaFormat == "wav_pcm_s16_mono")
            {
                Assert.True(MediaFixtureWriters.HasValidWavHeader(bytes), "Invalid WAV header: " + file.RelativePath);
            }
        }
    }
}
