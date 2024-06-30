
using ES.Common.Enumerations;
using ES.Common.Managers;
using System;
using System.IO.Ports;
using System.Threading;

namespace ES.Common.Helpers
{
    public class SerialPortReader : IDisposable
    {
        private bool listenSerialPort;
        private string _portName;
        protected SerialPort SerialPort;
        private Action<string> _dataReceivedCallback;

        private SerialPortReader(string portName)
        {
            _portName = portName;
        }

        public SerialPortReader(string serialPort, Action<string> dataReceivedCallback) : this(serialPort)
        {
            _dataReceivedCallback = dataReceivedCallback;
        }
        public void Start()
        {
            if (listenSerialPort) return;
            new Thread(() => { ReadSerialPort(); }).Start();
        }
        public void Dispose()
        {
            listenSerialPort = false;
            if (SerialPort != null && SerialPort.IsOpen) SerialPort.Close();
            if (SerialPort != null)
            {
                SerialPort.Dispose();
            }
            SerialPort = null;
        }
        private void ReadSerialPort()
        {
            if (string.IsNullOrEmpty(_portName)) return;
            try
            {
                listenSerialPort = true;
                SerialPort = new SerialPort()
                {
                    PortName = _portName,
                    BaudRate = 9600,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    DataBits = 8,
                    Handshake = Handshake.None
                };
                if (SerialPort.IsOpen) SerialPort.Close();
                SerialPort.DtrEnable = true;
                SerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                SerialPort.Open();

                //var listenSerialPortsThread = new Thread(ListenSerialPorts);
                //listenSerialPortsThread.Start();
            }
            catch (Exception ex) { MessageManager.OnMessage(ex.ToString()); listenSerialPort = false; SerialPort.Close(); }
        }
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = sender as SerialPort;
            if (sender != null)
            {
                try
                {
                    string indata = sp.ReadExisting().Trim();
                    _dataReceivedCallback(indata);
                }
                catch(Exception ex)
                {
                    MessageManager.OnMessage(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Not in use right now.
        /// </summary>
        private void ListenSerialPorts()
        {
            listenSerialPort = true;
            SerialPort.DtrEnable = true;
            try
            {
                SerialPort.Open();
                while (listenSerialPort)
                {
                    MessageManager.OnMessage("serial port listening open.");
                    string message = SerialPort.ReadLine();
                    MessageManager.OnMessage(new Models.MessageModel(message, MessageTypeEnum.Information));
                }
                SerialPort.Close();
            }
            catch (TimeoutException) { }
            catch (Exception ex) { MessageManager.OnMessage(new Models.MessageModel(ex.Message, MessageTypeEnum.Warning)); listenSerialPort = false; }


        }
    }
}
