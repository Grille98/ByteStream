﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection.Emit;

namespace GGL.IO.Compression;



public static class CompressionExtensions
{
    // Read
    record struct CompressedSectionInfo(CompressionType Type, CompressionLevel Level, LengthPrefix LengthPrefix);

    /// <summary>All Data after this will be read as compressed</summary>
    public static void CompressAll(this BinaryViewReader br, CompressionType type)
    {
        br.StreamStack.Push(new DecompressorStackEntry(br, type, br.Remaining));
    }

    /// <summary>Decompress data with CompressionStream, position will reset</summary>
    public static StreamStackEntry BeginCompressedSection(this BinaryViewReader br, CompressionType type, LengthPrefix lengthPrefix = LengthPrefix.Default)
    {
        long length = br.ReadLengthPrefix(lengthPrefix);
        return br.BeginCompressedSection(type, length);
    }

    public static StreamStackEntry BeginCompressedSection(this BinaryViewReader br, CompressionType type, long length)
    {
        br.StreamStack.Push(new DecompressorStackEntry(br, type, length));
        return br.StreamStack.Peak;
    }

    public static void EndCompressedSection(this BinaryViewReader br)
    {
        var entry = br.StreamStack.Pop();
        if (entry is not CompressorStackEntry)
            throw new InvalidOperationException();
        entry.Dispose();
    }

    // Write
    /// <summary>All Data after this will be writen as compressed</summary>
    public static void CompressAll(this BinaryViewWriter bw, CompressionType type, CompressionLevel level = CompressionLevel.Optimal)
    {
        bw.StreamStack.Push(new CompressorStackEntry(bw, type, level));
    }

    public static StreamStackEntry BeginCompressedSection(
        this BinaryViewWriter bw, CompressionType type,
        CompressionLevel level = CompressionLevel.Optimal, 
        LengthPrefix lengthPrefix = LengthPrefix.Default
    )
    {
        bw.StreamStack.Push(new CompressorStackEntry(bw, type, level, lengthPrefix));
        return bw.StreamStack.Peak;
    }

    public static void EndCompressedSection(this BinaryViewWriter bw)
    {
        var entry = bw.StreamStack.Pop();
        if (entry is not CompressorStackEntry)
            throw new InvalidOperationException();
        entry.Dispose();
    }

    /// <summary>All Data after this will be compressed</summary>
    public static void CompressAll(this BinaryView view, CompressionType type, CompressionLevel level = CompressionLevel.Optimal)
    {
        if (view.Mode == ViewMode.Read)
            view.Reader.CompressAll(type);
        else
            view.Writer.CompressAll(type, level);
    }

    // view
    public static void BeginCompressedSection(this BinaryView view, CompressionType type, CompressionLevel level = CompressionLevel.Optimal, LengthPrefix prefix = LengthPrefix.Default)
    {
        if (view.Mode == ViewMode.Read)
            view.Reader.BeginCompressedSection(type, prefix);
        else
            view.Writer.BeginCompressedSection(type, level, prefix);
    }

    public static void EndCompressedSection(this BinaryView view)
    {
        if (view.Mode == ViewMode.Read)
            view.Reader.EndCompressedSection();
        else
            view.Writer.EndCompressedSection();
    }
}