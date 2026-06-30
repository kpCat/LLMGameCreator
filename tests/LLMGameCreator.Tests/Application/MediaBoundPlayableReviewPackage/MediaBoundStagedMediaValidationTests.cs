using System.Security.Cryptography;
using LLMGameCreator.Application.Design.MediaBoundPlayableReviewPackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.MediaBoundPlayableReviewPackage;

public sealed class MediaBoundStagedMediaValidationTests
{
    [Fact]
    public async Task StagedFilesCopyGoal054BytesAndValidatePngWavStructure()
    {
        using var temp = new TempMediaBoundPlayableReviewPackageProject();
        var service = MediaBoundPlayableReviewPackageTestFactory.CreateService();
        var result = service.Build(MediaBoundPlayableReviewPackageTestFactory.FindRepoRoot());
        var write = await service.WriteAsync(temp.Path, result);

        Assert.Equal(15, result.ReviewPackageManifest.StagedFileCount);
        Assert.Equal(9, result.ReviewPackageManifest.PngFileCount);
        Assert.Equal(3, result.ReviewPackageManifest.WavFileCount);
        Assert.Equal(3, result.ReviewPackageManifest.BundleJsonFileCount);
        Assert.All(result.ReviewPackageManifest.StagedFiles, file =>
        {
            Assert.True(file.SourceHashMatches);
            Assert.True(file.SafeRelativePath);
            Assert.Equal(file.SourceSha256, file.StagedSha256);
            Assert.StartsWith("review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/", file.StagedRelativePath, StringComparison.Ordinal);
            Assert.False(Path.IsPathRooted(file.StagedRelativePath));
            Assert.DoesNotContain("..", file.StagedRelativePath);
        });

        foreach (var file in result.ReviewPackageManifest.StagedFiles)
        {
            var path = Path.Combine(write.OutputDirectoryPath, file.StagedRelativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(path), "Missing staged media file: " + file.StagedRelativePath);
            var bytes = await File.ReadAllBytesAsync(path);
            Assert.Equal(file.SizeBytes, bytes.LongLength);
            Assert.Equal(file.StagedSha256, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());

            if (file.StagedRelativePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                var png = MediaBoundMediaValidators.ValidatePng(bytes);
                Assert.True(png.Passed);
                Assert.Equal(32, png.Width);
                Assert.Equal(32, png.Height);
            }

            if (file.StagedRelativePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                var wav = MediaBoundMediaValidators.ValidateWav(bytes);
                Assert.True(wav.Passed);
                Assert.Equal(16000, wav.SampleRate);
                Assert.Equal(1, wav.Channels);
                Assert.Equal(4000, wav.SampleCount);
            }
        }
    }
}
