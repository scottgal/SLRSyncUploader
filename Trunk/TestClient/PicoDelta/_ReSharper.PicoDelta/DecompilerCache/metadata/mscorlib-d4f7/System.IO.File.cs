// Type: System.IO.File
// Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll

using System;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Text;

namespace System.IO
{
    [ComVisible(true)]
    public static class File
    {
        [SecuritySafeCritical]
        public static StreamReader OpenText(string path);

        [SecuritySafeCritical]
        public static StreamWriter CreateText(string path);

        [SecuritySafeCritical]
        public static StreamWriter AppendText(string path);

        public static void Copy(string sourceFileName, string destFileName);
        public static void Copy(string sourceFileName, string destFileName, bool overwrite);

        [SecuritySafeCritical]
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static FileStream Create(string path);

        [SecuritySafeCritical]
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static FileStream Create(string path, int bufferSize);

        [SecuritySafeCritical]
        public static FileStream Create(string path, int bufferSize, FileOptions options);

        public static FileStream Create(string path, int bufferSize, FileOptions options, FileSecurity fileSecurity);

        [SecuritySafeCritical]
        public static void Delete(string path);

        [SecuritySafeCritical]
        public static void Decrypt(string path);

        [SecuritySafeCritical]
        public static void Encrypt(string path);

        [SecuritySafeCritical]
        public static bool Exists(string path);

        [SecuritySafeCritical]
        public static FileStream Open(string path, FileMode mode);

        [SecuritySafeCritical]
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static FileStream Open(string path, FileMode mode, FileAccess access);

        [SecuritySafeCritical]
        public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share);

        public static void SetCreationTime(string path, DateTime creationTime);

        [SecuritySafeCritical]
        public static void SetCreationTimeUtc(string path, DateTime creationTimeUtc);

        [SecuritySafeCritical]
        public static DateTime GetCreationTime(string path);

        [SecuritySafeCritical]
        public static DateTime GetCreationTimeUtc(string path);

        public static void SetLastAccessTime(string path, DateTime lastAccessTime);

        [SecuritySafeCritical]
        public static void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc);

        [SecuritySafeCritical]
        public static DateTime GetLastAccessTime(string path);

        [SecuritySafeCritical]
        public static DateTime GetLastAccessTimeUtc(string path);

        public static void SetLastWriteTime(string path, DateTime lastWriteTime);

        [SecuritySafeCritical]
        public static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc);

        [SecuritySafeCritical]
        public static DateTime GetLastWriteTime(string path);

        [SecuritySafeCritical]
        public static DateTime GetLastWriteTimeUtc(string path);

        [SecuritySafeCritical]
        public static FileAttributes GetAttributes(string path);

        [SecuritySafeCritical]
        public static void SetAttributes(string path, FileAttributes fileAttributes);

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static FileSecurity GetAccessControl(string path);

        public static FileSecurity GetAccessControl(string path, AccessControlSections includeSections);

        [SecuritySafeCritical]
        public static void SetAccessControl(string path, FileSecurity fileSecurity);

        [SecuritySafeCritical]
        public static FileStream OpenRead(string path);

        [SecuritySafeCritical]
        public static FileStream OpenWrite(string path);

        [SecuritySafeCritical]
        public static string ReadAllText(string path);

        [SecuritySafeCritical]
        public static string ReadAllText(string path, Encoding encoding);

        [SecuritySafeCritical]
        public static void WriteAllText(string path, string contents);

        [SecuritySafeCritical]
        public static void WriteAllText(string path, string contents, Encoding encoding);

        [SecuritySafeCritical]
        public static byte[] ReadAllBytes(string path);

        [SecuritySafeCritical]
        public static void WriteAllBytes(string path, byte[] bytes);

        [SecuritySafeCritical]
        public static string[] ReadAllLines(string path);

        [SecuritySafeCritical]
        public static string[] ReadAllLines(string path, Encoding encoding);

        [SecuritySafeCritical]
        public static IEnumerable<string> ReadLines(string path);

        [SecuritySafeCritical]
        public static IEnumerable<string> ReadLines(string path, Encoding encoding);

        [SecuritySafeCritical]
        public static void WriteAllLines(string path, string[] contents);

        [SecuritySafeCritical]
        public static void WriteAllLines(string path, string[] contents, Encoding encoding);

        [SecuritySafeCritical]
        public static void WriteAllLines(string path, IEnumerable<string> contents);

        [SecuritySafeCritical]
        public static void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding);

        [SecuritySafeCritical]
        public static void AppendAllText(string path, string contents);

        [SecuritySafeCritical]
        public static void AppendAllText(string path, string contents, Encoding encoding);

        [SecuritySafeCritical]
        public static void AppendAllLines(string path, IEnumerable<string> contents);

        [SecuritySafeCritical]
        public static void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding);

        [SecuritySafeCritical]
        public static void Move(string sourceFileName, string destFileName);

        public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName);

        [SecuritySafeCritical]
        public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName,
                                   bool ignoreMetadataErrors);
    }
}
