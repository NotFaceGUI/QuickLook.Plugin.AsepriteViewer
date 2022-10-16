using System;
using System.Collections.Generic;

namespace QuickLook.Plugin.AsepriteViewer.AseRead
{
    /// <summary>
    /// 用于存储文件
    /// </summary>
    public class AsepriteFile
    {

    }

    public enum ChunkEnum
    {
        OldPaletteChunk = 0x0004,
        OldPaletteChunk2 = 0x00011,
        LayerChunk = 0x2004,
        CelChunk = 0x2005,
        CelExtraChunk = 0x2006,
        ColorProfileChunk = 0x2007,
        ExternalFilesChunk = 0x2008,
        MaskChunk = 0x2016, // DEPRECATED
        PathChunk = 0x2017,
        PaletteChunk = 0x2019,
        UserDataChunk = 0x2020,
        SliceChunk = 0x2022,
        TilesetChunk = 0x2023,
    }


    public enum ChunkType
    {
        RawImageData = 0, // (un - used, compressed image is preferred)
        LinkedCel = 1,
        CompressedImage = 2,
        CompressedTilemap = 3
    }

    public struct AseHeader
    {
        /// <summary>
        /// 文件大小
        /// </summary>
        public uint Size { get; set; }
        /// <summary>
        /// 帧数
        /// </summary>
        public ushort Frames { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public uint LayerOpacity { get; set; }
        /// <summary>
        /// 调色板的颜色数
        /// </summary>
        public ushort ColorCount { get; set; }
        /// <summary>
        /// 颜色位深
        /// </summary>
        public ushort ColorDepth { get; set; }
    }

    public struct AseFrameHeader
    {
        /// <summary>
        /// 文件的块数
        /// </summary>
        public uint Chunks { get; set; }
    }

    public class LayerChunk : Chunk
    {

    }

    public class CelChunk : Chunk
    {
        public ushort LayerIndex { get; set; }
        public short XPos { get; set; }
        public short YPos { get; set; }
        public byte OpacityLevel { get; set; }
        public ChunkType CelType { get; set; }
    }

    public class Chunk
    {
        public uint Size;
        public ChunkEnum Type;
        public byte[] DataBytes;
    }
}