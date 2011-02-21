using System;

namespace NetSync
{
    public class Module
    {
        public string Name;
        public string Path = String.Empty;
        public string Comment = String.Empty;
        public bool ReadOnly = true;
        public bool WriteOnly = false;
        public string HostsAllow = String.Empty;
        public string HostsDeny = String.Empty;
        public string AuthUsers = String.Empty;
        public string SecretsFile = String.Empty;

        public Module(string name)
        {
            Name = name;
        }
    }
}