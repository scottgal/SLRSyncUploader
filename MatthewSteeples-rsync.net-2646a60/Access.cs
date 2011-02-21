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
using System.Net;
using System.Net.Sockets;

namespace NetSync
{
    /// <summary>
    /// Contains methods to check access rigth using ip or hostname
    /// </summary>
    public class Access
    {
        private const int NS_INT16SZ = 2; //changed to a const
        private const int NS_INADDRSZ = 4; //changed to a const
        private const int NS_IN6ADDRSZ = 16; //changet to a const

        /// <summary>
        /// Just creates new instance of Access class
        /// </summary>
        public Access()
        {
        }

        /// <summary>
        /// Checks whether 'addr' or 'host' is allowed
        /// </summary>
        /// <param name="addr">ip4 or ip6 address</param>
        /// <param name="host">hostname</param>
        /// <param name="allowList">pattern of allowed hosts</param>
        /// <param name="denyList">pattern of denied hosts</param>
        /// <returns></returns>
        public bool AllowAccess(string addr, string host, string allowList, string denyList)
        {
            if (allowList == null || allowList.CompareTo(String.Empty) == 0)
            {
                allowList = null;
            }
            if (denyList == null || denyList.CompareTo(String.Empty) == 0)
            {
                denyList = null;
            }
            /* if theres no deny list and no allow list then allow access */
            if (denyList == null && allowList == null)
            {
                return true;
            }
            /* if there is an allow list but no deny list then allow only hosts
               on the allow list */
            if (denyList == null)
            {
                return (AccessMatch(allowList, addr, host));
            }

            /* if theres a deny list but no allow list then allow
               all hosts not on the deny list */
            if (allowList == null)
            {
                return (!AccessMatch(denyList, addr, host));
            }

            /* if there are both type of list then allow all hosts on the
                   allow list */
            if (AccessMatch(allowList, addr, host))
            {
                return true;
            }

            /* if there are both type of list and it's not on the allow then
               allow it if its not on the deny */
            if (AccessMatch(denyList, addr, host))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether 'addr' or 'host' matches 'list' of patterns
        /// </summary>
        /// <param name="list">pattern string</param>
        /// <param name="addr">ip4 or ip6 address</param>
        /// <param name="host">hostname</param>
        /// <returns></returns>
        protected bool AccessMatch(string list, string addr, string host)
        {
            char[] separators = { ' ', ',', '\t' };
            string[] list2 = list.ToLower().Split(separators);
            if (host != null)
            {
                host = host.ToLower();
            }

            for (int i = 0; i < list2.Length; i++)
            {
                if (list2[i].CompareTo(String.Empty) == 0)
                {
                    continue;
                }
                if (MatchHostname(host, list2[i]) || MatchAddress(addr, list2[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether 'host' matches pattern 'tok'
        /// </summary>
        /// <param name="host">hostname</param>
        /// <param name="tok">pattern</param>
        /// <returns>True if matched</returns>
        protected bool MatchHostname(string host, string tok)
        {
            if (host == null)
            {
                return false;
            }
            return WildMatch.CheckWildMatch(tok, host);
        }

        /// <summary>
        /// Checks whether 'addr' matches pattern 'tok'
        /// </summary>
        /// <param name="addr">ip4 or ip6 address</param>
        /// <param name="tok">pattern</param>
        /// <returns>True if matched</returns>
        protected bool MatchAddress(string addr, string tok)
        {
            IPAddress ipaddr = null, iptok = null;
            string p = String.Empty;
            int len = 0, addrlen = 0;
            byte[] mask = new byte[16];
            int bits = 0; //changet to int?

            if (addr == null || addr.CompareTo(String.Empty) == 0)
            {
                return false;
            }
            int pos = tok.IndexOf('/');
            if (pos > 0)
            {
                p = tok.Substring(0, pos);
                len = p.Length;
            }
            else
            {
                len = tok.Length;
                p = tok;
            }

            try
            {
                ipaddr = IPAddress.Parse(addr);
                iptok = IPAddress.Parse(p);
            }
            catch (Exception)
            {
                return false;
            }
            if (ipaddr.AddressFamily != iptok.AddressFamily)
            {
                return false;
            }
            if (iptok.AddressFamily == AddressFamily.InterNetwork)
            {
                addrlen = 4;
            }
            else
            {
                addrlen = 16;
            }

            bits = -1;
            if (pos > 0)
            {
                if ((mask = InetPton(iptok.AddressFamily, tok.Substring(pos + 1, tok.Length - pos - 1))) == null)
                {
                    mask = new byte[16];
                    bits = Convert.ToInt32(tok.Substring(pos + 1)); //changed conversion from 64 to 32
                    if (bits == 0)
                    {
                        return true;
                    }
                    if (bits < 0 || bits > (addrlen << 3))
                    {
                        Log.Write("malformed mask in " + tok);
                        return false;
                    }
                }
            }
            else
            {
                bits = 128;
            }
            
            if (bits >= 0)
            {
                MakeMask(mask, (int)bits, addrlen);
            }

            return MatchBinary(ipaddr.ToString(), iptok.ToString(), mask, addrlen);
        }

        /// <summary>
        /// Makes mask from /xx syntax
        /// </summary>
        /// <param name="mask">here we put result</param>
        /// <param name="plen">digits after / in address pattern</param>
        /// <param name="addrlen"></param>
        protected void MakeMask(byte[] mask, int plen, int addrlen)
        {
            int w, b, i;

            w = plen >> 3;
            b = plen & 0x7;

            if (w > 0)
            {
                for (i = 0; i < w; i++)
                {
                    mask[i] = 0xff;
                }
            }

            if (w < addrlen)
            {
                mask[w] = (byte)(0xff & (0xff << (8 - b)));
            }

            if (w + 1 < addrlen)
            {
                for (i = 0; i < addrlen - w - 1; i++)
                {
                    mask[w + 1 + i] = 0;
                }
            }

            return;
        }

        /// <summary>
        /// Checks whether b1 and b2 fit mask
        /// </summary>
        /// <param name="b1">ipv4 or ipv6 address</param>
        /// <param name="b2">ipv4 or ipv6 address</param>
        /// <param name="mask"></param>
        /// <param name="addrlen"></param>
        /// <returns>True if matched</returns>
        protected bool MatchBinary(string b1, string b2, byte[] mask, int addrlen)
        {
            int i;

            for (i = 0; i < addrlen; i++)
            {
                if (((b1[i] ^ b2[i]) & mask[i]) > 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Convert string representation of mask to byte array
        /// </summary>
        /// <param name="af"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        protected byte[] InetPton(AddressFamily af, string src)
        {
            if (af == AddressFamily.InterNetwork)
            {
                return InetPton4(src);
            }
            else
            {
                return InetPton6(src);
            }
        }

        /// <summary>
        /// Convert string representation of mask to byte array for ipv4 if possible
        /// </summary>
        /// <param name="src"></param>
        /// <returns>null if parse failed, otherwise byte[] with mask</returns>
        protected byte[] InetPton4(string src)
        {
            string digits = "0123456789";
            char ch;
            byte[] res = new byte[16];
            int pos = 0, octets = 0;
            bool saw_digit;

            saw_digit = false;
            octets = 0;
            int i = 0;
            while (i < src.Length)
            {
                ch = src[i++];
                int pch;

                if ((pch = digits.IndexOf(ch)) >= 0)
                {
                    byte dig = (byte)(res[pos] * 10 + pch);
                    //if (dig > 255) //useless because byte can't be > 255
                    //{
                    //    return null;
                    //}
                    res[pos] = dig;
                    if (!saw_digit)
                    {
                        if (++octets > 4)
                        {
                            return null;
                        }
                        saw_digit = true;
                    }
                }
                else if (ch == '.' && saw_digit)
                {
                    if (octets == 4)
                    {
                        return null;
                    }
                    pos++;
                    saw_digit = false;
                }
                else
                {
                    return null;
                }
            }
            if (octets < 4)
            {
                return null;
            }
            return res;
        }

        /// <summary>
        /// Convert string representation of mask to byte array for ipv6
        /// </summary>
        /// <param name="src"></param>
        /// <returns>null if parse failed, otherwise byte[] with mask</returns>
        protected byte[] InetPton6(string src)
        {
            string xdigits = "0123456789abcdef";
            byte[] res = new byte[NS_IN6ADDRSZ];
            string curtok;
            bool saw_xdigit;
            int pos = 0, colonp = -1, respos = 0;
            char ch;
            int val;
            src = src.ToLower();

            /* Leading :: requires some special handling. */
            if (src[pos] == ':')
            {
                if (src[++pos] != ':')
                {
                    return null;
                }
            }
            curtok = src;
            saw_xdigit = false;
            val = 0;
            while (pos < src.Length - 1)
            {
                ch = src[++pos];
                int pch;

                if ((pch = xdigits.IndexOf(ch)) >= 0)
                {
                    val <<= 4;
                    val |= pch;
                    if (val > 0xffff)
                    {
                        return null;
                    }
                    saw_xdigit = true;
                    continue;
                }
                if (ch == ':')
                {
                    curtok = src;
                    if (!saw_xdigit)
                    {
                        if (colonp > 0)
                        {
                            return null;
                        }
                        colonp = respos;
                        continue;
                    }
                    if (respos + NS_INT16SZ > NS_IN6ADDRSZ)
                    {
                        return null;
                    }
                    res[respos++] = (byte)((val >> 8) & 0xff);
                    res[respos++] = (byte)(val & 0xff);
                    saw_xdigit = false;
                    val = 0;
                    continue;
                }
                if (ch == '.' && ((respos + NS_INADDRSZ) <= NS_IN6ADDRSZ))
                {
                    byte[] tp = InetPton4(curtok);
                    if (tp != null)
                    {
                        respos += NS_INADDRSZ;
                        saw_xdigit = false;
                        break;	/* '\0' was seen by inet_pton4(). */
                    }
                }
                return null;
            }
            if (saw_xdigit)
            {
                if (respos + NS_INT16SZ > NS_IN6ADDRSZ)
                {
                    return null;
                }
                res[respos++] = (byte)((val >> 8) & 0xff);
                res[respos++] = (byte)(val & 0xff);
            }
            if (colonp > 0)
            {
                /*
                * Since some memmove()'s erroneously fail to handle
                * overlapping regions, we'll do the shift by hand.
                */
                int n = respos - colonp;
                int i;

                for (i = 1; i <= n; i++)
                {
                    res[res.Length - i] = res[colonp + n - i];
                    res[colonp + n - i] = 0;
                }
                respos = NS_IN6ADDRSZ;
            }
            //			if (respos != NS_IN6ADDRSZ)
            //				return null;
            return res;
        }
    }
}
