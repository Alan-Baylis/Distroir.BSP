/*
Distroir.BSP
Copyright (C) 2017 Distroir

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distroir.BSP
{
    public class BspFile
    {
        #region Fields

        /// <summary>
        /// BSP file identifier
        /// </summary>
        public int Identifier { get; private set; }
        /// <summary>
        /// BSP file version
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// lump directory array
        /// </summary>
        public BspLump[] Lumps { get; private set; }
        /// <summary>
        /// Map's revision number
        /// </summary>
        public int MapRevision { get; private set; }

        #endregion

        #region Static methods

        public BspFile FromFile(string filename)
        {
            return FromFile(filename, FileMode.Open);
        }

        public BspFile FromFile(string filename, FileMode mode)
        {
            using (FileStream fs = new FileStream(filename, mode))
            {
                return FromStream(fs);
            }
        }

        public BspFile FromStream(Stream stream)
        {
            using (BinaryReader r = new BinaryReader(stream))
            {
                return FromBinaryReader(r);
            }
        }

        public BspFile FromBinaryReader(BinaryReader reader)
        {
            // Read identifier
            Identifier = reader.ReadInt32();

            // Validate identifier
            // Little-endian "VBSP"   0x50534256
            if (Identifier != 0x50534256)
            {
                throw new InvalidBspFileException();
            }

            // Read version
            Version = reader.ReadInt32();

            // Read game lumps
            Lumps = new BspLump[64];

            for (int i = 0; i < 64; i++)
            {
                Lumps[i] = ReadLump(reader);
            }

            // Read map revision number
            MapRevision = reader.ReadInt32();

            // Return value
            return this;
        }

        public BspFile FromByteArray(byte[] array)
        {
            using (MemoryStream ms = new MemoryStream(array))
            {
                return FromStream(ms);
            }
        }

        #endregion

        #region Reading lumps

        BspLump ReadLump(BinaryReader r)
        {
            //Create new lump
            BspLump l = new BspLump();

            //Read  lump data
            l.FileOffset = r.ReadInt32();
            l.FileLength = r.ReadInt32();
            l.Version = r.ReadInt32();
            l.fourCC = r.ReadInt32();

            //Return value
            return l;
        }

        public BspLump ReadLump(BinaryReader reader, BspLumpOffsets lumpId)
        {
            return ReadLump(reader, (int)lumpId);
        }

        public BspLump ReadLump(BinaryReader reader, int lumpId)
        {
            //Calculate and set offset
            reader.BaseStream.Position = BspOffsets.CalculateLumpOffset(lumpId);
            //Read lump
            return ReadLump(reader);
        }

        public static byte[] GetLumpData(BinaryReader reader, BspLump lump)
        {
            //Set offset
            reader.BaseStream.Position = lump.FileOffset;
            //Return value
            return reader.ReadBytes(lump.FileLength);
        }

        #endregion
    }
}
