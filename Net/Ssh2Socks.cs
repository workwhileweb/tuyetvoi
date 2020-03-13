using System;
using System.Net;
using Renci.SshNet;

namespace TuyetVoi.Net
{
    public class Ssh2Socks : IDisposable
    {
        public ForwardedPortDynamic ForwardedPort;

        public SshClient Ssh;

        public Ssh2Socks(IPAddress ip, string user, string pass)
            : this(ip, 22, user, pass)
        {
        }

        public Ssh2Socks(IPAddress ip, uint port, string user, string pass)
            : this(ip, port, 0, user, pass)
        {
        }

        public Ssh2Socks(IPAddress ip, uint port, uint socksPort, string user, string pass)
            : this(ip, port, socksPort, user, pass, null)
        {
        }

        public Ssh2Socks(string ip, uint port, uint socksPort, string user, string pass, string keyFile)
            : this(IPAddress.Parse(ip), port, socksPort, user, pass, keyFile)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="socksPort"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="keyFile"></param>
        public Ssh2Socks(IPAddress ip, uint port, uint socksPort, string user, string pass, string keyFile)
        {
            Ip = ip;
            Port = port <= 0 ? 22 : port;
            User = user;
            Pass = pass;
            KeyFile = keyFile;

            Ssh = KeyFile == null
                ? new SshClient(Ip.ToString(), (int)Port, User, Pass)
                : new SshClient(Ip.ToString(), (int)Port, User, new PrivateKeyFile(KeyFile));
            Ssh.Connect();
            
            SocksPort = socksPort;
            if (SocksPort <= 0) SocksPort = (uint) Helper.GetFreeTcpPort();

            ForwardedPort = new ForwardedPortDynamic("127.0.0.1", SocksPort);
            Ssh.AddForwardedPort(ForwardedPort);
            ForwardedPort.Start();
        }

        public IPAddress Ip { get; }
        public uint Port { get; }
        public uint SocksPort { get; }
        public string User { get; }
        public string Pass { get; }
        public string KeyFile { get; }

        public void Dispose()
        {
            ForwardedPort.Stop();
            ForwardedPort.Dispose();
            Ssh.Disconnect();
            Ssh?.Dispose();
        }
    }
}