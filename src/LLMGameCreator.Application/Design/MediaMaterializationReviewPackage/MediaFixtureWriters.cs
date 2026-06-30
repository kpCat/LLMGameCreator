using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace LLMGameCreator.Application.Design.MediaMaterializationReviewPackage;

public static class MediaFixtureWriters
{
    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];

    public static MaterializedMediaFilePayload CreatePayload(MediaMaterializationQueueItem item)
    {
        var bytes = item.MaterializedMediaFormat switch
        {
            "png" => CreatePng(item),
            "wav_pcm_s16_mono" => CreateWav(item),
            "bundle_manifest_json" => CreateBundleManifest(item),
            _ => throw new InvalidOperationException("Unsupported media fixture format: " + item.MaterializedMediaFormat)
        };

        return new MaterializedMediaFilePayload
        {
            RelativePath = item.OutputRelativePath,
            Bytes = bytes
        };
    }

    public static bool HasValidPngSignature(byte[] bytes) =>
        bytes.Length > PngSignature.Length && PngSignature.SequenceEqual(bytes.Take(PngSignature.Length));

    public static bool HasValidWavHeader(byte[] bytes)
    {
        if (bytes.Length < 44)
        {
            return false;
        }

        return Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF"
            && Encoding.ASCII.GetString(bytes, 8, 4) == "WAVE"
            && Encoding.ASCII.GetString(bytes, 12, 4) == "fmt "
            && Encoding.ASCII.GetString(bytes, 36, 4) == "data"
            && BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(20, 2)) == 1
            && BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(22, 2)) == 1
            && BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(34, 2)) == 16;
    }

    public static bool ValidatePngChunkCrcs(byte[] bytes)
    {
        if (!HasValidPngSignature(bytes))
        {
            return false;
        }

        var offset = PngSignature.Length;
        while (offset + 12 <= bytes.Length)
        {
            var length = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset, 4)));
            offset += 4;
            if (length < 0 || offset + 4 + length + 4 > bytes.Length)
            {
                return false;
            }

            var typeAndData = bytes.AsSpan(offset, 4 + length);
            offset += 4 + length;
            var expected = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset, 4));
            offset += 4;
            if (Crc32(typeAndData) != expected)
            {
                return false;
            }

            if (Encoding.ASCII.GetString(typeAndData[..4]) == "IEND")
            {
                return offset == bytes.Length;
            }
        }

        return false;
    }

    private static byte[] CreatePng(MediaMaterializationQueueItem item)
    {
        const int width = 32;
        const int height = 32;
        var seed = MediaMaterializationReviewPackageHash.SeedBytes(
            item.MaterializationId,
            item.FamilyId,
            item.MediaSlotId,
            item.GeneratedTargetId);

        var raw = new byte[(1 + width * 3) * height];
        var offset = 0;
        for (var y = 0; y < height; y++)
        {
            raw[offset++] = 0;
            for (var x = 0; x < width; x++)
            {
                var index = (x + y * width) % seed.Length;
                raw[offset++] = (byte)(seed[index] ^ (x * 7));
                raw[offset++] = (byte)(seed[(index + 11) % seed.Length] ^ (y * 5));
                raw[offset++] = (byte)(seed[(index + 19) % seed.Length] ^ ((x + y) * 3));
            }
        }

        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            zlib.Write(raw, 0, raw.Length);
        }

        using var stream = new MemoryStream();
        stream.Write(PngSignature);
        var ihdr = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(ihdr.AsSpan(0, 4), width);
        BinaryPrimitives.WriteUInt32BigEndian(ihdr.AsSpan(4, 4), height);
        ihdr[8] = 8;
        ihdr[9] = 2;
        ihdr[10] = 0;
        ihdr[11] = 0;
        ihdr[12] = 0;
        WriteChunk(stream, "IHDR", ihdr);
        WriteChunk(stream, "IDAT", compressed.ToArray());
        WriteChunk(stream, "IEND", []);
        return stream.ToArray();
    }

    private static byte[] CreateWav(MediaMaterializationQueueItem item)
    {
        const int sampleRate = 16000;
        const short channels = 1;
        const short bitsPerSample = 16;
        const int sampleCount = sampleRate / 4;
        var seed = MediaMaterializationReviewPackageHash.SeedBytes(
            item.MaterializationId,
            item.FamilyId,
            item.MediaSlotId,
            item.GeneratedTargetId);
        var dataSize = sampleCount * channels * bitsPerSample / 8;

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataSize);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * bitsPerSample / 8);
        writer.Write((short)(channels * bitsPerSample / 8));
        writer.Write(bitsPerSample);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataSize);

        var step = 37 + seed[0];
        var value = (seed[1] << 8) | seed[2];
        for (var i = 0; i < sampleCount; i++)
        {
            value = (value + step + seed[i % seed.Length]) & 0xffff;
            var centered = (short)(value - 32768);
            var sample = (short)(centered / 5);
            writer.Write(sample);
        }

        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] CreateBundleManifest(MediaMaterializationQueueItem item)
    {
        var payload = new
        {
            schemaVersion = "goal054_bundle_manifest_fixture_v1",
            materializationId = item.MaterializationId,
            familyId = item.FamilyId,
            sourceRequestId = item.SourceRequestId,
            sourceBindingId = item.SourceBindingId,
            mediaKind = item.MediaKind,
            mediaSlotId = item.MediaSlotId,
            generatedTargetId = item.GeneratedTargetId,
            providerCalled = false,
            networkCalled = false,
            finalMedia = false
        };
        return Encoding.UTF8.GetBytes(MediaMaterializationReviewPackageHash.Serialize(payload) + "\n");
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(buffer, checked((uint)data.Length));
        stream.Write(buffer);
        var typeBytes = Encoding.ASCII.GetBytes(type);
        stream.Write(typeBytes);
        stream.Write(data);

        var crcInput = new byte[typeBytes.Length + data.Length];
        Buffer.BlockCopy(typeBytes, 0, crcInput, 0, typeBytes.Length);
        Buffer.BlockCopy(data, 0, crcInput, typeBytes.Length, data.Length);
        BinaryPrimitives.WriteUInt32BigEndian(buffer, Crc32(crcInput));
        stream.Write(buffer);
    }

    private static uint Crc32(ReadOnlySpan<byte> bytes)
    {
        uint crc = 0xffffffff;
        foreach (var b in bytes)
        {
            crc ^= b;
            for (var i = 0; i < 8; i++)
            {
                var mask = (uint)-(int)(crc & 1);
                crc = (crc >> 1) ^ (0xedb88320 & mask);
            }
        }

        return ~crc;
    }
}
