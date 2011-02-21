using System;

namespace NetSync
{
    public class WinRsync
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var mc = new MainClass();
            mc.Run(args);
        }
    }
}