using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace MultiSerialPortViewer {
    public class COMDevice : SerialPort {
        public int Id;

        public COMDevice(
            int id, 
            string portName,
            int baudRate=115200, 
            Parity parity= Parity.None, 
            int databits=8, 
            StopBits stopBits=StopBits.One, 
            Handshake handshake=Handshake.None) 
        {
            this.Id = id;
            this.PortName = portName;
            this.BaudRate = baudRate;
            this.Parity = parity;
            this.DataBits = databits;
            this.StopBits = stopBits;
            this.Handshake = handshake;
            Console.WriteLine($"COMPort Constructor! {Id} {PortName}");
        }

        public void Connect() {
            this.Open();
        }

        public void Send(String text) {
            if (this?.IsOpen == true) {
                this.Write(text);
            }
        }
    }
}
