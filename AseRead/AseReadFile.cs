using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace QuickLook.Plugin.AsepriteViewer.AseRead
{
    public class AseReadFile
    {
        private readonly string[] _formats = { ".ase", ".aseprite" };

        public bool Read(string path)
        {
            if (Directory.Exists(path) || !_formats.Contains(Path.GetExtension(path)?.ToLower())) return false;

            var aseBufferRead = new AseBufferRead(File.ReadAllBytes(path), 0);
            var layerChunks = new List<Chunk>();
            var celChunks = new List<Chunk>();
            try
            {
                var header = ReadAseHeader(aseBufferRead);
                // 循环帧 但是这里只取第一帧
                var readAseFrameHeader = ReadAseFrameHeader(aseBufferRead);


                // 循环块
                for (var i = 0; i < readAseFrameHeader.Chunks; i++)
                {
                    var chunk = ReadAseFrameChunk(aseBufferRead);
                    if (chunk == null) continue;
                    if (chunk.Type == ChunkEnum.CelChunk)
                        celChunks.Add(chunk);
                    else
                        layerChunks.Add(chunk);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception{0}", e);
                return false;
            }

            Console.WriteLine($"当前路径：{path} ", path);
            return true;
        }

        public bool ReadAseIsGIF(string path, string[] formats)
        {
            if (Directory.Exists(path) || !formats.Contains(Path.GetExtension(path)?.ToLower())) return false;

            var aseBufferRead = new AseBufferRead(File.ReadAllBytes(path), 0);
            var header = ReadAseHeader(aseBufferRead);
            return header.Frames > 1;
        }

        public Chunk ReadAseFrameChunk(AseBufferRead aseBufferRead)
        {
            var chunk = new Chunk
            {
                Size = aseBufferRead.ReadDword(),
                Type = (ChunkEnum)aseBufferRead.ReadWord()
            };
            var constOffset = AseBufferRead.DWORD + AseBufferRead.WORD;
            // 查询指定块
            switch (chunk.Type)
            {
                case ChunkEnum.OldPaletteChunk:
                    aseBufferRead.SkipData((int)chunk.Size - constOffset);
                    break;
                case ChunkEnum.OldPaletteChunk2:
                    aseBufferRead.SkipData((int)chunk.Size - constOffset);
                    break;
                case ChunkEnum.LayerChunk:
                    aseBufferRead.SkipData((int)chunk.Size - constOffset);
                    return chunk;
                case ChunkEnum.CelChunk:
                    aseBufferRead.SkipData((int)chunk.Size - constOffset);
                    /*return ReadCelChunk(chunk, aseBufferRead);*/
                    return chunk;
                case ChunkEnum.CelExtraChunk:
                    aseBufferRead.SkipData((int)chunk.Size - constOffset);
                    break;
                case ChunkEnum.ColorProfileChunk:
                    aseBufferRead.SkipData((int)chunk.Size - constOffset);
                    break;
                case ChunkEnum.ExternalFilesChunk:
                    aseBufferRead.SkipData((int)chunk.Size - constOffset);
                    break;
                case ChunkEnum.MaskChunk:
                    aseBufferRead.SkipData((int)chunk.Size - constOffset);
                    break;
                case ChunkEnum.PathChunk:
                    aseBufferRead.SkipData((int)chunk.Size - constOffset);
                    break;
                case ChunkEnum.PaletteChunk:
                    aseBufferRead.SkipData((int)chunk.Size - constOffset);
                    break;
                case ChunkEnum.UserDataChunk:
                    aseBufferRead.SkipData((int)chunk.Size - constOffset);
                    break;
                case ChunkEnum.SliceChunk:
                    aseBufferRead.SkipData((int)chunk.Size - constOffset);
                    break;
                case ChunkEnum.TilesetChunk:
                    aseBufferRead.SkipData((int)chunk.Size - constOffset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private CelChunk ReadCelChunk(Chunk chunk, AseBufferRead aseBufferRead)
        {
            // 构造一个基本的cel实例
            var celChunk = new CelChunk()
            {
                Size = chunk.Size,
                Type = chunk.Type
            };

            celChunk.LayerIndex = aseBufferRead.ReadWord();
            celChunk.XPos = aseBufferRead.ReadShort();
            celChunk.YPos = aseBufferRead.ReadShort();
            celChunk.OpacityLevel = aseBufferRead.ReadByte();
            celChunk.CelType = (ChunkType)aseBufferRead.ReadWord();

            aseBufferRead.SkipData(7);

            switch (celChunk.CelType)
            {
                case ChunkType.RawImageData:
                    break;
                case ChunkType.LinkedCel:
                    break;
                case ChunkType.CompressedImage:
                    break;
                case ChunkType.CompressedTilemap:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            return celChunk;
        }

        public AseFrameHeader ReadAseFrameHeader(AseBufferRead aseBufferRead)
        {
            var header = new AseFrameHeader();

            aseBufferRead.SkipData(AseBufferRead.DWORD);
            if (aseBufferRead.ReadWord() != 0xF1FA)
                throw new InvalidOperationException("无效的操作！无法解析ase文件的帧头，文件可能损坏。");
            header.Chunks = aseBufferRead.ReadWord();

            aseBufferRead.SkipData(4);
            var newChunks = aseBufferRead.ReadDword();
            if (newChunks != 0)
                header.Chunks = newChunks;

            return header;
        }

        public AseHeader ReadAseHeader(AseBufferRead aseBufferRead)
        {
            var header = new AseHeader();

            header.Size = aseBufferRead.ReadDword();

            if (aseBufferRead.ReadWord() != 0xA5E0)
                throw new InvalidOperationException("无效的操作！无法解析ase文件，文件可能损坏。");

            // 获取帧数
            header.Frames = aseBufferRead.ReadWord();

            aseBufferRead.SkipData(AseBufferRead.WORD, 2);

            // 获取位深
            header.ColorDepth = aseBufferRead.ReadWord();
            aseBufferRead.Depth = (BitDepth)header.ColorDepth;

            header.LayerOpacity = aseBufferRead.ReadDword();
            aseBufferRead.SkipData(AseBufferRead.WORD);
            aseBufferRead.SkipData(AseBufferRead.DWORD, 2);
            var paletteEntryIndex = aseBufferRead.ReadByte();

            aseBufferRead.SkipData(3);

            header.ColorCount = aseBufferRead.ReadWord();

            aseBufferRead.SkipData(94);


            return header;
        }
    }
}