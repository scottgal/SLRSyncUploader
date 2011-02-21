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
using System.Collections.Generic;
using System.IO;

namespace NetSync
{
    public class ExcludeStruct
    {
        public string Pattern;
        public UInt32 MatchFlags;
        public int SlashCnt;

        public ExcludeStruct(string pattern, UInt32 matchFlags, int slashCnt)
        {
            this.Pattern = pattern;
            this.MatchFlags = matchFlags;
            this.SlashCnt = slashCnt;
        }

        public ExcludeStruct() { }
    }

    public class Exclude
    {
        private Options options;

        public Exclude(Options opt)
        {
            options = opt;
        }

        public static void AddCvsExcludes() { }
        public void AddExcludeFile(ref List<ExcludeStruct> exclList, string fileName, int xFlags)
        {
            var wordSplit = (xFlags & Options.XFLG_WORD_SPLIT) != 0;
            TextReader f;
            if (fileName == null || fileName.CompareTo(String.Empty) == 0 || !File.Exists(fileName))
            {
                return;
            }
            if (fileName.CompareTo("-") == 0)
            {
                f = System.Console.In;
            }
            else
            {
                try
                {
                    f = new System.IO.StreamReader(fileName);
                }
                catch
                {
                    if ((xFlags & Options.XFLG_FATAL_ERRORS) != 0)
                    {
                        Log.Write("failed to open " + (((xFlags & Options.XFLG_DEF_INCLUDE) != 0) ? "include" : "exclude") + " file " + fileName);
                    }
                    return;
                }
            }
            while (true)
            {
                string line = f.ReadLine();
                if (line == null)
                {
                    break;
                }
                if (line.CompareTo(String.Empty) != 0 && (wordSplit || (line[0] != ';' && line[0] != '#')))
                {
                    AddExclude(ref exclList, line, xFlags);
                }
            }
            f.Close();

        }
        public void AddExclude(ref List<ExcludeStruct> exclList, string pattern, int xFlags)
        {
            UInt32 mFlags;
            if (pattern == null)
            {
                return;
            }
            string cp = pattern;
            int len = 0;
            while (true)
            {
                if (len >= cp.Length)
                {
                    break;
                }
                cp = GetExcludeToken(cp.Substring(len), out len, out mFlags, xFlags);
                if (len == 0)
                {
                    break;
                }
                if ((mFlags & Options.MATCHFLG_CLEAR_LIST) != 0)
                {
                    if (options.verbose > 2)
                    {
                        Log.WriteLine("[" + options.WhoAmI() + "] clearing exclude list");
                    }
                    exclList.Clear();
                    continue;
                }

                MakeExlude(ref exclList, cp, mFlags);
                if (options.verbose > 2)
                {
                    Log.WriteLine("[" + options.WhoAmI() + "] AddExclude(" + cp + ")");
                }
            }
        }

        public void MakeExlude(ref List<ExcludeStruct> exclList, string pat, UInt32 mFlags)
        {
            var exLen = 0;
            var patLen = pat.Length;
            var ret = new ExcludeStruct();
            if (options.excludePathPrefix != null)
            {
                mFlags |= Options.MATCHFLG_ABS_PATH;
            }
            if (options.excludePathPrefix != null && pat[0] == '/')
            {
                exLen = options.excludePathPrefix.Length;
            }
            else
            {
                exLen = 0;
            }
            ret.Pattern = String.Empty;
            if (exLen != 0)
            {
                ret.Pattern += options.excludePathPrefix;
            }
            ret.Pattern += pat.Replace('\\', '/');
            patLen += exLen;

            if (ret.Pattern.IndexOfAny(new char[] { '*', '[', '?' }) != -1)
            {
                mFlags |= Options.MATCHFLG_WILD;
                if (ret.Pattern.IndexOf("**") != -1)
                {
                    mFlags |= Options.MATCHFLG_WILD2;
                    if (ret.Pattern.IndexOf("**") == 0)
                    {
                        mFlags |= Options.MATCHFLG_WILD2_PREFIX;
                    }
                }
            }

            if (patLen > 1 && ret.Pattern[ret.Pattern.Length - 1] == '/')
            {
                ret.Pattern = ret.Pattern.Remove(ret.Pattern.Length - 1, 1);
                mFlags |= Options.MATCHFLG_DIRECTORY;
            }

            for (var i = 0; i < ret.Pattern.Length; i++)
            {
                if (ret.Pattern[i] == '/')
                {
                    ret.SlashCnt++;
                }
            }
            ret.MatchFlags = mFlags;
            exclList.Add(ret);
        }

        static string GetExcludeToken(string p, out int len, out uint mFlags, int xFlags)
        {
            len = 0;
            string s = p;
            mFlags = 0;
            if (p.CompareTo(String.Empty) == 0)
            {
                return String.Empty;
            }

            if ((xFlags & Options.XFLG_WORD_SPLIT) != 0)
            {
                p = s = p.Trim(' ');
            }
            if ((xFlags & Options.XFLG_WORDS_ONLY) == 0 && (s[0] == '-' || s[0] == '+') && s[1] == ' ')
            {
                if (s[0] == '+')
                {
                    mFlags |= Options.MATCHFLG_INCLUDE;
                }
                s = s.Substring(2);
            }
            else if ((xFlags & Options.XFLG_DEF_INCLUDE) != 0)
            {
                mFlags |= Options.MATCHFLG_INCLUDE;
            }
            if ((xFlags & Options.XFLG_DIRECTORY) != 0)
            {
                mFlags |= Options.MATCHFLG_DIRECTORY;
            }
            if ((xFlags & Options.XFLG_WORD_SPLIT) != 0)
            {
                var i = 0;
                while (i < s.Length && s[i] == ' ')
                {
                    i++;
                }
                len = s.Length - i;
            }
            else
            {
                len = s.Length;
            }
            if (p[0] == '!' && len == 1)
            {
                mFlags |= Options.MATCHFLG_CLEAR_LIST;
            }
            return s;
        }

        /*
        * Return -1 if file "name" is defined to be excluded by the specified
        * exclude list, 1 if it is included, and 0 if it was not matched.
        */
        public int CheckExclude(List<ExcludeStruct> listp, string name, int nameIsDir)
        {
            foreach (var ex in listp)
            {
                if (CheckOneExclude(name, ex, nameIsDir))
                {
                    ReportExcludeResult(name, ex, nameIsDir);
                    return (ex.MatchFlags & Options.MATCHFLG_INCLUDE) != 0 ? 1 : -1;
                }
            }
            return 0;
        }

        static bool CheckOneExclude(string name, ExcludeStruct ex, int nameIsDir)
        {
            var matchStart = 0;
            var pattern = ex.Pattern;

            if (name.CompareTo(String.Empty) == 0)
            {
                return false;
            }
            if (pattern.CompareTo(String.Empty) == 0)
            {
                return false;
            }

            if (0 != (ex.MatchFlags & Options.MATCHFLG_DIRECTORY) && nameIsDir == 0)
            {
                return false;
            }

            if (pattern[0] == '/')
            {
                matchStart = 1;
                if (name[0] == '/')
                {
                pattern = pattern.TrimStart('/');
                    name = name.TrimStart('/');
                }
            }

            if ((ex.MatchFlags & Options.MATCHFLG_WILD) != 0)
            {
                /* A non-anchored match with an infix slash and no "**"
                 * needs to match the last slash_cnt+1 name elements. */
                if (matchStart != 0 && ex.SlashCnt != 0 && 0 == (ex.MatchFlags & Options.MATCHFLG_WILD2))
                {
                    name = name.Substring(name.IndexOf('/') + 1);
                }
                if (WildMatch.CheckWildMatch(pattern, name))
                {
                    return true;
                }
                if ((ex.MatchFlags & Options.MATCHFLG_WILD2_PREFIX) != 0)
                {
                    /* If the **-prefixed pattern has a '/' as the next
                    * character, then try to match the rest of the
                    * pattern at the root. */
                    if (pattern[2] == '/' && WildMatch.CheckWildMatch(pattern.Substring(3), name))
                    {
                        return true;
                    }
                }
                else if (0 == matchStart && (ex.MatchFlags & Options.MATCHFLG_WILD2) != 0)
                {
                    /* A non-anchored match with an infix or trailing "**"
                    * (but not a prefixed "**") needs to try matching
                    * after every slash. */
                    int posSlash;
                    while ((posSlash = name.IndexOf('/')) != -1)
                    {
                        name = name.Substring(posSlash + 1);
                        if (WildMatch.CheckWildMatch(pattern, name))
                        {
                            return true;
                        }
                    }
                }
            }
            else if (matchStart != 0)
            {
                if (name.CompareTo(pattern) == 0)
                {
                    return true;
                }
            }
            else
            {
                var l1 = name.Length;
                var l2 = pattern.Length;
                if (l2 <= l1 &&
                    name.Substring(l1 - l2).CompareTo(pattern) == 0 &&
                    (l1 == l2 || name[l1 - (l2 + 1)] == '/'))
                {
                    return true;
                }
            }

            return false;
        }

        public void ReportExcludeResult(string name, ExcludeStruct ent, int nameIsDir)
        {
            /* If a trailing slash is present to match only directories,
            * then it is stripped out by make_exclude.  So as a special
            * case we add it back in here. */

            if (options.verbose >= 2)
            {
                Log.Write(options.WhoAmI() + " " + ((ent.MatchFlags & Options.MATCHFLG_INCLUDE) != 0 ? "in" : "ex") +
                    "cluding " + (nameIsDir != 0 ? "directory" : "file") + " " +
                    name + " because of " + ent.Pattern + " pattern " +
                    ((ent.MatchFlags & Options.MATCHFLG_DIRECTORY) != 0 ? "/" : String.Empty) + "\n");
            }
        }

        public void SendExcludeList(IOStream f)
        {
            if (options.listOnly && !options.recurse)
            {
                AddExclude(ref options.excludeList, "/*/*", 0);
            }


            string pattern;
            int patternLength;
            
            foreach (var ent in options.excludeList)
            {
                if (ent.Pattern.Length == 0 || ent.Pattern.Length > Options.MAXPATHLEN)
                {
                    continue;
                }
                patternLength = ent.Pattern.Length;
                pattern = ent.Pattern;
                if ((ent.MatchFlags & Options.MATCHFLG_DIRECTORY) != 0)
                {
                    pattern += "/\0";
                }

                if ((ent.MatchFlags & Options.MATCHFLG_INCLUDE) != 0)
                {
                    f.writeInt(patternLength + 2);
                    f.IOPrintf("+ ");
                }
                else if ((pattern[0] == '-' || pattern[0] == '+') && pattern[1] == ' ')
                {
                    f.writeInt(patternLength + 2);
                    f.IOPrintf("- ");
                }
                else
                {
                    f.writeInt(patternLength);
                }
                f.IOPrintf(pattern);

            }
            f.writeInt(0);
        }

        /// <summary>
        /// Receives exclude list from stream
        /// </summary>
        /// <param name="ioStream"></param>
        public void ReceiveExcludeList(IOStream ioStream)
        {
            var line = String.Empty;
            int length;
            while ((length = ioStream.ReadInt()) != 0)
            {
                if (length >= Options.MAXPATHLEN + 3)
                {
                    Log.Write("Overflow: recv_exclude_list");
                    continue;
                }

                line = ioStream.ReadStringFromBuffer(length);
                AddExclude(ref options.excludeList, line, 0);
            }
        }
    }
}
