using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace hcClient
{
    public enum ConnectionStatus { Disconnected, Connecting, OK }

    class TcpClient //: IDisposable
    {
        #region " Constructor "

        public TcpClient()
        {
            _reconnectTimer = new System.Timers.Timer(Defaults.ReconnectTimerInterval);
            _reconnectTimer.Elapsed += new System.Timers.ElapsedEventHandler(ReconnectTimerElapsed);

            _watchdogTimer = new System.Timers.Timer(Defaults.WatchdogTimerInterval);
            _watchdogTimer.Elapsed += new System.Timers.ElapsedEventHandler(WatchdogElapsed);

            _remoteAddress = Defaults.LocalServer;
            _remotePort = Defaults.TcpPort;
            _localId = 0;
        }

        #endregion

        #region " DataCache "

        private hcCache<int> _cache = new hcCache<int>(Defaults.DataCacheSize, -1);

        private bool locked = false;
        public bool Locked
        {
            get { return locked; }
            set { locked = value; }
        }

        public int GetData(int id)
        {
            return _cache.Read(id);
        }

        public void SetDataOverride(int _id, int _value)
        {
            sendData(_id, _value);
        }

        public void SetData(int _id, int _value)
        {
            if (!locked)
                sendData(_id, _value);
        }

        #endregion

        #region " Events "

        public event EventHandler<hcData> DataReceived;
        protected void OnDataReceived(int id, int data)
        {
            _cache.Write(id, data);
            if (DataReceived != null)
                DataReceived(this, new hcData(id, _cache.Read(id)));
        }

        public event EventHandler<hcCommand> CommandReceived;
        protected void OnCommandReceived(string command)
        {
            if (CommandReceived != null)
                CommandReceived(this, new hcCommand(command));
        }

        public event EventHandler ConnectionStatusChanged;
        protected void OnConnectionStatusChanged()
        {
            if (ConnectionStatusChanged != null)
                ConnectionStatusChanged(this, EventArgs.Empty);
        }

        #endregion

        #region " Network "

        #region " Network Parameters "

        private Socket _socket;

        private byte[] asyncBuffer;
        private byte[] inputBuffer;
        private int inputBufferSize;

        private string _remoteAddress;
        public string RemoteAddress
        {
            get { return _remoteAddress; }
            set
            {
                if (value != null && _connectionStatus == ConnectionStatus.Disconnected)
                    _remoteAddress = value;
            }
        }

        private int _remotePort;
        public int RemotePort
        {
            get { return _remotePort; }
            set
            {
                if (value != 0 && _connectionStatus == ConnectionStatus.Disconnected)
                    _remotePort = value;
            }
        }

        private byte _localId;
        public byte ID
        {
            get { return _localId; }
            set
            {
                if (_connectionStatus == ConnectionStatus.Disconnected)
                    _localId = value;
            }
        }

        #endregion

        #region " Connection Status "

        private ConnectionStatus _connectionStatus = ConnectionStatus.Disconnected;
        public ConnectionStatus ConnectionStatus
        {
            get { return _connectionStatus; }
            private set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    OnConnectionStatusChanged();
                }
            }
        }

        #endregion

        #region " Keep Alive "

        private System.Timers.Timer _reconnectTimer;

        private void ReconnectTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _reconnectTimer.Stop();
            Connect();
        }

        private System.Timers.Timer _watchdogTimer;

        private void WatchdogReset()
        {
            _watchdogTimer.Stop();
            _watchdogTimer.Start();
        }

        private void WatchdogElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _watchdogTimer.Stop();
            Disconnect();
        }

        #endregion

        public void Connect()
        {
            if (_connectionStatus != ConnectionStatus.Disconnected) return;

            try
            {
                ConnectionStatus = ConnectionStatus.Connecting;
                var IPs = Dns.GetHostAddresses(_remoteAddress);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.BeginConnect(IPs, _remotePort, new AsyncCallback(ConnectCallback), _socket);
            }
            catch (SocketException)
            {
                Disconnect();
            }
            catch (Exception exc)
            {
                Disconnect();
                System.Windows.Forms.MessageBox.Show("TcpOpen.Exception: " + exc);
            }
        }

        private void Disconnect()
        {
            ConnectionStatus = ConnectionStatus.Disconnected;
            _reconnectTimer.Stop();
            _watchdogTimer.Stop();
            try
            {
                _socket.Close();
            }
            finally
            {
                /*if (!this.disposed)*/ _reconnectTimer.Start();
                _cache.Reset();
                for (var i = 0; i < _cache.Count; i++)
                    OnDataReceived(i, _cache.Read(i));
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket s = (Socket)ar.AsyncState;
                s.EndConnect(ar);

                if (asyncBuffer == null) asyncBuffer = new byte[Defaults.IoBufferSize];
                if (inputBuffer == null) inputBuffer = new byte[Defaults.IoBufferSize];
                inputBufferSize = 0;

                _watchdogTimer.Start();
                BeginAutorization();
            }
            catch (SocketException)
            {
                Disconnect();
            }
            catch (Exception exc)
            {
                Disconnect();
                System.Windows.Forms.MessageBox.Show("TcpConnect.Exception: " + exc);
            }
        }

        private void BeginAutorization()
        {
            _socket.Send(Encoding.ASCII.GetBytes(String.Format("%{0:X2}1\r\n", _localId)));
            ConnectionStatus = ConnectionStatus.OK;
            _socket.BeginReceive(
              asyncBuffer, 0, asyncBuffer.Length,
              SocketFlags.None,
              new AsyncCallback(ReceiveCallback), _socket);
        }

        private void tcpSend(string str)
        {
            if (_connectionStatus == ConnectionStatus.OK)
            {
                try
                {
                    _socket.Send(Encoding.ASCII.GetBytes(str));
                }
                catch (SocketException)
                {
                    Disconnect();
                }
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                Socket s = (Socket)result.AsyncState;
                int bytesRead = s.EndReceive(result);
                if (0 != bytesRead)
                {
                    WatchdogReset();
                    byte b;
                    for (int i = 0; i < bytesRead; i++)
                    {
                        b = asyncBuffer[i];
                        switch (b)
                        {
                        case 0x0A: break;
                        case 0x0D:
                            ParseReceived();
                            inputBufferSize = 0;
                            break;
                        default:
                            inputBuffer[inputBufferSize++] = b;
                            if (inputBufferSize > inputBuffer.Length)
                                inputBufferSize = 0;
                            break;
                        }
                    }

                    if (_connectionStatus != ConnectionStatus.Disconnected)
                        _socket.BeginReceive(
                          asyncBuffer, 0, asyncBuffer.Length,
                          SocketFlags.None,
                          new AsyncCallback(ReceiveCallback), _socket);
                }
                else
                {
                    Disconnect();
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        private void ParseReceived()
        {
            if (inputBufferSize == 0)
                return;

            switch ((char)inputBuffer[0])
            {
            case '#':
                if (inputBufferSize == (1 + 4 + 4))
                {
                    var id = HexParser.GetUInt16(inputBuffer, 1);
                    var val = HexParser.GetInt16(inputBuffer, 5);
                    OnDataReceived(id, val);
                }
                break;
            case 'K':
                if (inputBufferSize > (1 + 2))
                {
                    if (_localId != 0)
                    {
                        var id = HexParser.GetByte(inputBuffer, 1);
                        if (id == _localId || id == 0)
                        {
                            var s = Encoding.ASCII.GetString(inputBuffer, 3, inputBufferSize - 3);
                            OnCommandReceived(s);
                        }
                    }
                }
                break;
            }
        }

        private void sendData(int id, int data)
        {
            tcpSend(String.Format("#{0:X4}{1:X4}\r\n", (UInt16)id, (Int16)data));
        }

        private void sendTimestamp()
        {
            DateTime now = DateTime.Now;

            tcpSend(String.Format("$T{0:X4}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}\r\n",
              (UInt16)now.Year, (byte)now.Month, (byte)now.Day,
              (byte)now.Hour, (byte)now.Minute, (byte)now.Second));
        }

        #endregion

        //#region " IDisposable "

        //private bool disposed = false;

        //public void Dispose()
        //{
        //    if (!this.disposed)
        //    {
        //        _socket.Close();
        //        _reconnectTimer.Dispose();
        //        _watchdogTimer.Dispose();
        //        this.disposed = true;
        //    }
        //}

        //#endregion
    }

}
