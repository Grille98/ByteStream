﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GGL.IO.Compression;
internal class DecompressorStackEntry : StreamStackEntry
{
    public readonly BinaryViewReader Reader;
    public readonly CompressionType Type;
    public readonly long Length;

    public DecompressorStackEntry(BinaryViewReader br, CompressionType type, long length) : base(br.StreamStack, new MemoryStream(), false)
    {
        Reader = br;
        Type = type;
        Length = length;

        using (var compressedSection = Reader.StreamStack.GetSubStream(Length))
        {
            using (var decompressStream = CompressionFactory.CreateDecompresser(Type, compressedSection, true))
            {
                decompressStream.CopyTo(Stream);
                Stream.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}