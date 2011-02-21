/**
 *  Copyright (C) 2006 Alex Pedenko
 * 
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
using System.IO;

namespace NetSync
{
    public class FileIO
    {
        private static byte _lastByte;
        private static int _lastSparse;

        public static int SparseEnd(Stream f)
        {
            if (_lastSparse != 0)
            {
                f.Seek(-1, SeekOrigin.Current);
                f.WriteByte(_lastByte);
                return 0;
            }
            _lastSparse = 0;
            return 0;
        }


        public static int WriteSparse(Stream f, byte[] buf, int len)
        {
            int l1, l2;
            int returnValue;

            for (l1 = 0; l1 < len && buf[l1] == 0; l1++)
            {
            }
            for (l2 = 0; l2 < len - l1 && buf[len - (l2 + 1)] == 0; l2++)
            {
            }

            _lastByte = buf[len - 1];

            if (l1 == len || l2 > 0)
            {
                _lastSparse = 1;
            }

            if (l1 > 0)
            {
                f.Seek(l1, SeekOrigin.Current);
            }


            if (l1 == len)
            {
                return len;
            }

            f.Write(buf, l1, len - (l1 + l2));
            returnValue = len - (l1 + l2);
            if (returnValue == -1 || returnValue == 0)
            {
                return returnValue;
            }

            if (returnValue != (len - (l1 + l2)))
            {
                return (l1 + returnValue);
            }

            if (l2 > 0)
            {
                f.Seek(l2, SeekOrigin.Current);
            }

            return len;
        }

        public static int WriteFile(Stream f, byte[] buf, int off, int len)
        {
            f.Write(buf, off, len);
            return len;
        }
    }

    public class MapFile
    {
        /// <summary>
        /// Default window size
        /// </summary>
        private readonly int defaultWindowSize;

        /// <summary>
        /// File Descriptor
        /// </summary>
        private readonly Stream fileDescriptor;

        /// <summary>
        /// File size (from stat)
        /// </summary>
        public int fileSize;

        /// <summary>
        /// Window pointer
        /// </summary>
        public byte[] p;

        /// <summary>
        /// Offset of cursor in file descriptor ala lseek
        /// </summary>
        private int pFileDescriptorOffset;

        /// <summary>
        /// Latest (rounded) window size
        /// </summary>
        private int pLength;

        /// <summary>
        /// Window start
        /// </summary>
        private int pOffset;

        /// <summary>
        /// Largest window size we allocated
        /// </summary>
        private int pSize;

        /// <summary>
        /// First errno from read errors (Seems to be false all the time)
        /// </summary>
        public bool status;

        /// <summary>
        /// Initialyze new instance
        /// </summary>
        /// <param name="fileDescriptor"></param>
        /// <param name="length"></param>
        /// <param name="mapSize"></param>
        /// <param name="blockSize"></param>
        public MapFile(Stream fileDescriptor, int length, int mapSize, int blockSize)
        {
            if (blockSize != 0 && (mapSize%blockSize) != 0)
            {
                mapSize += blockSize - (mapSize%blockSize);
            }
            this.fileDescriptor = fileDescriptor;
            fileSize = length;
            defaultWindowSize = mapSize;
        }

        /// <summary>
        /// Returns offset in p array
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int MapPtr(int offset, int length)
        {
            int readStart;
            int readSize, readOffset;

            if (length == 0)
            {
                return -1;
            }

            if (length > (fileSize - offset))
            {
                length = fileSize - offset;
            }

            if (offset >= pOffset && offset + length <= pOffset + pLength)
            {
                return offset - pOffset;
            }

            int windowStart = offset;
            int windowSize = defaultWindowSize;
            if (windowStart + windowSize > fileSize)
            {
                windowSize = fileSize - windowStart;
            }
            if (offset + length > windowStart + windowSize)
            {
                windowSize = (offset + length) - windowStart;
            }

            if (windowSize > pSize)
            {
                ExtendArray(ref p, windowSize);
                pSize = windowSize;
            }

            if (windowStart >= pOffset &&
                windowStart < pOffset + pLength &&
                windowStart + windowSize >= pOffset + pLength)
            {
                readStart = pOffset + pLength;
                readOffset = readStart - windowStart;
                readSize = windowSize - readOffset;
                MemoryMove(ref p, p, (pLength - readOffset), readOffset);
            }
            else
            {
                readStart = windowStart;
                readSize = windowSize;
                readOffset = 0;
            }
            if (readSize <= 0)
            {
                Log.WriteLine("Warning: unexpected read size of " + readSize + " in MapPtr");
            }
            else
            {
                if (pFileDescriptorOffset != readStart)
                {
                    if (fileDescriptor.Seek(readStart, SeekOrigin.Begin) != readStart)
                    {
                        MainClass.Exit("Seek failed in MapPtr", null);
                    }
                    pFileDescriptorOffset = readStart;
                }

                int numberOfReadBytes;
                if ((numberOfReadBytes = fileDescriptor.Read(p, readOffset, readSize)) != readSize)
                {
                    FillMemory(ref p, readOffset + numberOfReadBytes, 0, readSize - numberOfReadBytes);
                }
                pFileDescriptorOffset += numberOfReadBytes;
            }

            pOffset = windowStart;
            pLength = windowSize;
            return offset - pOffset;
        }

        /// <summary>
        /// Returns status
        /// </summary>
        /// <returns></returns>
        public bool UnMapFile()
        {
            return status;
        }

        /// <summary>
        /// Fills 'data' with given 'value'
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="value"></param>
        /// <param name="length"></param>
        private static void FillMemory(ref byte[] data, int offset, byte value, int length)
        {
            for (var i = 0; i < length; i++)
            {
                data[offset + i] = value;
            }
        }

        /// <summary>
        /// Moves 'length' bytes in array. So pass the same array as source and destination
        /// </summary>
        /// <param name="destination">Destination array</param>
        /// <param name="source">Source array</param>
        /// <param name="sourceOffset">Start taking bytes from this offset</param>
        /// <param name="length">Number of bytes to move</param>
        private static void MemoryMove(ref byte[] destination, byte[] source, int sourceOffset, int length)
            //it seems that ref is't needed
        {
            var sourceCopy = (byte[]) source.Clone();
            for (int i = 0; i < length; i++)
            {
                destination[i] = sourceCopy[sourceOffset + i];
            }
        }

        /// <summary>
        /// Extends array to new [bigger] 'size'
        /// </summary>
        /// <param name="array"></param>
        /// <param name="size"></param>
        public static void ExtendArray(ref byte[] array, int size)
        {
            if (array == null)
            {
                array = new byte[size];
            }
            else
            {
                var tempArray = new byte[array.Length];
                array.CopyTo(tempArray, 0);
                array = new byte[size];
                tempArray.CopyTo(array, 0);
            }
        }

        /// <summary>
        /// Extends array to new [bigger] 'size'
        /// </summary>
        /// <param name="array"></param>
        /// <param name="size"></param>
        public static void ExtendArray(ref string[] array, int size)
        {
            if (array == null)
            {
                array = new string[size];
            }
            else
            {
                var tempArray = new string[array.Length];
                array.CopyTo(tempArray, 0);
                array = new string[size];
                tempArray.CopyTo(array, 0);
            }
        }
    }
}