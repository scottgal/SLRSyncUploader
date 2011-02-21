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
using System;

namespace NetSync
{
    public class Token
    {
        public static int residue;
        private readonly Options options;

        public Token(Options opt)
        {
            options = opt;
        }

        public void SetCompression(string fname)
        {
            if (!options.doCompression)
            {
                return; 
            }
        }

        public void SendToken(IOStream f, int token, MapFile buf, int offset, int n, int toklen)
        {
            if (!options.doCompression)
            {
                SimpleSendToken(f, token, buf, offset, n);
            }
            else
            {
                SendDeflatedToken(f, token, buf, offset, n, toklen);
            }
        }

        public int ReceiveToken(IOStream ioStream, ref byte[] data, int offset)
        {
            int token;
            if (!options.doCompression)
            {
                token = SimpleReceiveToken(ioStream, ref data, offset);
            }
            else
            {
                token = ReceiveDeflatedToken(ioStream, data, offset);
            }
            return token;
        }

        public int SimpleReceiveToken(IOStream ioStream, ref byte[] data, int offset)
        {
            int n;
            if (residue == 0)
            {
                int i = ioStream.ReadInt();
                if (i <= 0)
                {
                    return i;
                }
                residue = i;
            }

            n = Math.Min(Match.CHUNK_SIZE, residue);
            residue -= n;
            data = ioStream.ReadBuffer(n);
            return n;
        }

        public int ReceiveDeflatedToken(IOStream f, byte[] data, int offset)
        {
            return 0;
        }

        public void SendDeflatedToken(IOStream f, int token, MapFile buf, int offset, int nb, int toklen)
        {
        }

        public void SeeToken(byte[] data, int offset, int tokLen)
        {
            if (options.doCompression)
            {
                SeeDeflateToken(data, offset, tokLen);
            }
        }

        public void SeeDeflateToken(byte[] data, int offset, int tokLen)
        {
        }

        public void SimpleSendToken(IOStream f, int token, MapFile buf, int offset, int n)
        {
            if (n > 0)
            {
                var l = 0;
                while (l < n)
                {
                    var n1 = Math.Min(Match.CHUNK_SIZE, n - l);
                    f.writeInt(n1);
                    var off = buf.MapPtr(offset + l, n1);
                    f.Write(buf.p, off, n1);
                    l += n1;
                }
            }
            if (token != -2)
            {
                f.writeInt(-(token + 1));
            }
        }
    }
}