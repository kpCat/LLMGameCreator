using System.Buffers.Binary;
using System.Text;

namespace LLMGameCreator.Application.Design.MediaBoundPlayableReviewPackage;

public static class MediaBoundMediaValidators
{
    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];

    public static PngValidationResult ValidatePng(byte[] bytes)
    {
        if (bytes.Length <= PngSignature.Length || !PngSignature.SequenceEqual(bytes.Take(PngSignature.Length)))
        {
            return new PngValidationResult { SignatureValid = false, ChunkCrcsValid = false };
        }

        var offset = PngSignature.Length;
        var width = 0;
        var height = 0;
        while (offset + 12 <= bytes.Length)
        {
            var length = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset, 4)));
            offset += 4;
            if (length < 0 || offset + 4 + length + 4 > bytes.Length)
            {
                return new PngValidationResult { SignatureValid = true, ChunkCrcsValid = false, Width = width, Height = height };
            }

            var typeAndData = bytes.AsSpan(offset, 4 + length);
            var type = Encoding.ASCII.GetString(typeAndData[..4]);
            if (type == "IHDR" && length == 13)
            {
                width = checked((int)BinaryPrimitives.ReadUInt32BigEndian(typeAndData.Slice(4, 4)));
                height = checked((int)BinaryPrimitives.ReadUInt32BigEndian(typeAndData.Slice(8, 4)));
            }

            offset += 4 + length;
            var expected = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset, 4));
            offset += 4;
            if (Crc32(typeAndData) != expected)
            {
                return new PngValidationResult { SignatureValid = true, ChunkCrcsValid = false, Width = width, Height = height };
            }

            if (type == "IEND")
            {
                return new PngValidationResult
                {
                    SignatureValid = true,
                    ChunkCrcsValid = offset == bytes.Length,
                    Width = width,
                    Height = height
                };
            }
        }

        return new PngValidationResult { SignatureValid = true, ChunkCrcsValid = false, Width = width, Height = height };
    }

    public static WavValidationResult ValidateWav(byte[] bytes)
    {
        if (bytes.Length < 44
            || Encoding.ASCII.GetString(bytes, 0, 4) != "RIFF"
            || Encoding.ASCII.GetString(bytes, 8, 4) != "WAVE")
        {
            return new WavValidationResult { HeaderValid = false };
        }

        var offset = 12;
        ushort audioFormat = 0;
        ushort channels = 0;
        var sampleRate = 0;
        ushort bitsPerSample = 0;
        ushort blockAlign = 0;
        var dataSize = 0;

        while (offset + 8 <= bytes.Length)
        {
            var chunkId = Encoding.ASCII.GetString(bytes, offset, 4);
            var chunkSize = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(offset + 4, 4));
            offset += 8;
            if (chunkSize < 0 || offset + chunkSize > bytes.Length)
            {
                return new WavValidationResult { HeaderValid = false };
            }

            if (chunkId == "fmt " && chunkSize >= 16)
            {
                audioFormat = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset, 2));
                channels = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset + 2, 2));
                sampleRate = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(offset + 4, 4));
                blockAlign = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset + 12, 2));
                bitsPerSample = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset + 14, 2));
            }
            else if (chunkId == "data")
            {
                dataSize = chunkSize;
            }

            offset += chunkSize;
            if ((chunkSize & 1) == 1 && offset < bytes.Length)
            {
                offset++;
            }
        }

        var valid = audioFormat == 1
            && channels > 0
            && sampleRate > 0
            && bitsPerSample == 16
            && blockAlign > 0
            && dataSize > 0;

        return new WavValidationResult
        {
            HeaderValid = valid,
            SampleRate = sampleRate,
            Channels = channels,
            BitsPerSample = bitsPerSample,
            SampleCount = valid ? dataSize / blockAlign : 0
        };
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
