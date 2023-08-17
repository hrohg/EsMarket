
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
        private SerialPort serialPort;
        private Action<string> _serialPort_DataReceived;

        private SerialPortReader(string portName)
        {
            if (string.IsNullOrEmpty(portName)) return;
            try
            {
                serialPort = new SerialPort()
                {
                    PortName = portName,
                    BaudRate = 9600,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    DataBits = 8,
                    Handshake = Handshake.None
                };
                if (serialPort.IsOpen) serialPort.Close();
                serialPort.DtrEnable = true;
                serialPort.Open();
                serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                //serialPort.Close();

                //var listenSerialPortsThread = new Thread(ListenSerialPorts);
                //listenSerialPortsThread.Start();
            }
            catch (Exception ex) { MessageManager.OnMessage(ex.ToString()); listenSerialPort = false; serialPort.Close(); }
        }

        public SerialPortReader(string serialPort, Action<string> serialPort_DataReceived) : this(serialPort)
        {
            _serialPort_DataReceived = serialPort_DataReceived;
        }

        void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting().Trim();
            //MessageManager.OnMessage(string.Format("Data Received:{0} length:{1} ", indata, indata.Length));
            _serialPort_DataReceived(indata);
        }

        private void ListenSerialPorts()
        {
            listenSerialPort = true;
            serialPort.DtrEnable = true;
            try
            {
                serialPort.Open();
                while (listenSerialPort)
                {

                    MessageManager.OnMessage("serial port listening open.");
                    string message = serialPort.ReadLine();
                    MessageManager.OnMessage(new Models.MessageModel(message, MessageTypeEnum.Information));
                }
                serialPort.Close();
            }
            catch (TimeoutException) { }
            catch (Exception ex) { MessageManager.OnMessage(new Models.MessageModel(ex.Message, MessageTypeEnum.Warning)); listenSerialPort = false; }


        }
        public void Dispose()
        {
            listenSerialPort = false;
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            if (serialPort != null && serialPort.IsOpen) serialPort.Close();
        }
    }
}
