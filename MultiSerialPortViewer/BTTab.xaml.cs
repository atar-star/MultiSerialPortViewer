using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultiSerialPortViewer {
    /// <summary>
    /// BTTab.xaml の相互作用ロジック
    /// </summary>
    /// 
    public partial class BTTab : UserControl {
        private COMDevice Com;
        private EnterWindow parent;

        public BTTab() {
            InitializeComponent();
        }

        public BTTab(COMDevice com, EnterWindow parent) {
            InitializeComponent();
            this.Com = com;
            this.parent = parent;

            this.Com.DataReceived += async (object sSender, SerialDataReceivedEventArgs eS) => {
                if (this.Com.IsOpen == false) {
                    return;
                }

                string str = this.Com.ReadLine(); // 
                                                     //string str = serialPort2.ReadExisting(); // 今来てる文字をすべて取り出し
                Dispatcher.Invoke((Action<string>)((string text) => {
                    //Console.WriteLine("{0}: {1}", serialPort2.Com, text);
                    text = text + " | " + BitConverter.ToString(Encoding.ASCII.GetBytes(text)) + "\n";
                    textBox.AppendText(text);
                    textBox.ScrollToEnd();
                }), new object[] { str });
            };
        }

        private void Button_Close(object sender, RoutedEventArgs e) {
            Console.WriteLine($"Close {Com.Id} {Com.PortName}");
            this.parent.ClosePort(Com.Id);
        }

        private void SendTextBoxToPort() {
            if (!String.IsNullOrEmpty(sendTextBox.Text)) {
                this.Com.Send(sendTextBox.Text + "\n");
                sendTextBox.Clear();
            }
        }
        private void Clicked_Send(object sender, RoutedEventArgs e) {
            SendTextBoxToPort();
        }
        private void Pressed_EnterToSend(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                SendTextBoxToPort();
            }
        }

    }
}
