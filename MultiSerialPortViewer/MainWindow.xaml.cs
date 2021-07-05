using System;
using System.Collections.Generic;
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

using System.IO.Ports;


namespace MultiSerialPortViewer {

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {

        COMDevice serialPort1;
        COMDevice serialPort2;


        public MainWindow() {
            InitializeComponent();

            var window = new EnterWindow();
            window.Show();
            this.Close();
        }

        private void UpdatePortsList() {
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports) {
                portComboBox1.Items.Add(port);
                portComboBox2.Items.Add(port);
            }

            if (portComboBox1.Items.Count > 0) {
                portComboBox1.SelectedIndex = 0;
            }
            if (portComboBox2.Items.Count > 0) {
                portComboBox2.SelectedIndex = 0;
            }
        }

        private void Form_Load(object sender, RoutedEventArgs e) {
            UpdatePortsList();
        }

        private void Form_closed(object sender, EventArgs e) {
            if (serialPort2?.IsOpen == true) serialPort2.Close();
            Console.WriteLine("Close Window");
        }


        // デバイスに接続
        private void Clicked_OK1(object sender, RoutedEventArgs e) {
            if (serialPort1?.IsOpen == true) {
                Console.WriteLine("Disconnect: {0}", serialPort1.PortName);
                serialPort1.Close();
                connectButton1.Content = "接続";
                portComboBox1.IsEnabled = true;
            } else {
                serialPort1 = new COMDevice(1, portComboBox1.SelectedItem.ToString());
                serialPort1.BaudRate = 115200;
                serialPort1.Parity = Parity.None;
                serialPort1.DataBits = 8;
                serialPort1.StopBits = StopBits.One;
                serialPort1.Handshake = Handshake.None;
                //serialPort1.Encoding = Encoding.Unicode;
                serialPort1.DataReceived += async (object sSender, SerialDataReceivedEventArgs eS) => {
                    if (serialPort1.IsOpen == false) {
                        return;
                    }

                    string str = serialPort1.ReadLine(); // たまに改行を読み込み落として最新まで表示できてないときあり
                    //string str = serialPort1.ReadExisting(); // 今来てる文字をすべて取り出し
                    Dispatcher.Invoke((Action<string>)((string text) => {
                        //Console.WriteLine("{0}: {1}", serialPort1.Com, text);
                        text = text + " | " + BitConverter.ToString(Encoding.ASCII.GetBytes(text)) + "\n";
                        textBox1.AppendText(text);
                        textBox1.ScrollToEnd();
                    }), new object[] { str });
                };

                try {
                    serialPort1.Open();
                    connectButton1.Content = "切断";
                    portComboBox1.IsEnabled = false;
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Clicked_OK2(object sender, RoutedEventArgs e) {
            if (serialPort2?.IsOpen == true) {
                Console.WriteLine("Disconnect: {0}", serialPort2.PortName);
                serialPort2.Close();
                connectButton2.Content = "接続";
                portComboBox2.IsEnabled = true;
            } else {
                serialPort2 = new COMDevice(2, portComboBox2.SelectedItem.ToString());
                serialPort2.BaudRate = 115200;
                serialPort2.Parity = Parity.None;
                serialPort2.DataBits = 8;
                serialPort2.StopBits = StopBits.One;
                serialPort2.Handshake = Handshake.None;
                //serialPort2.Encoding = Encoding.Unicode;
                serialPort2.DataReceived += async (object sSender, SerialDataReceivedEventArgs eS) => {
                    if (serialPort2.IsOpen == false) {
                        return;
                    }

                    string str = serialPort2.ReadLine(); // 
                    //string str = serialPort2.ReadExisting(); // 今来てる文字をすべて取り出し
                    Dispatcher.Invoke((Action<string>)((string text) => {
                        //Console.WriteLine("{0}: {1}", serialPort2.Com, text);
                        text = text + " | " + BitConverter.ToString(Encoding.ASCII.GetBytes(text)) + "\n";
                        textBox2.AppendText(text);
                        textBox2.ScrollToEnd();
                    }), new object[] { str });
                };

                try {
                    serialPort2.Open();
                    connectButton2.Content = "切断";
                    portComboBox2.IsEnabled = false;
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
            }

        }


        /// テキストをデバイスに送る
        // シリアルポート1に送る
        private void SendTextBoxToPort1() {
            if (!String.IsNullOrEmpty(sendTextBox1.Text)) {
                serialPort1.Send(sendTextBox1.Text + "\n");
                sendTextBox1.Clear();
            }
        }
        private void Clicked_Send1(object sender, RoutedEventArgs e) {
            SendTextBoxToPort1();
        }
        private void Pressed_EnterToSend1(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                SendTextBoxToPort1();
            }
        }

        // シリアルポート2に送る
        private void SendTextBoxToPort2() {
            if (!String.IsNullOrEmpty(sendTextBox2.Text)) {
                serialPort2.Send(sendTextBox2.Text + "\n");
                sendTextBox2.Clear();
            }
        }
        private void Clicked_Send2(object sender, RoutedEventArgs e) {
            SendTextBoxToPort2();
        }
        private void Pressed_EnterToSend2(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                SendTextBoxToPort2();
            }
        }

        // つながってるシリアルポートすべてにブロードキャスト
        private void BroadcastTextBoxToPort() {
            if (!String.IsNullOrEmpty(broadcastTextBox.Text)) {
                serialPort1.Send(broadcastTextBox.Text + "\n");
                serialPort2.Send(broadcastTextBox.Text + "\n");
                broadcastTextBox.Clear();
            }
        }
        private void Pressed_EnterToBroadcast(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                BroadcastTextBoxToPort();
            }
        }
        private void Clicked_Broadcast(object sender, RoutedEventArgs e) {
            BroadcastTextBoxToPort();
        }

        // 1秒ごとに1文字ずつブロードキャスト
        private async void Clicked_Broadcast_CharPerSecond(object sender, RoutedEventArgs e) {
            if (!String.IsNullOrEmpty(broadcastTextBox.Text)) {
                broadcastTextBox.IsEnabled = false;
                broadcastButton.IsEnabled = false;
                broadcastPerSecondButton.IsEnabled = false;

                await Task.Run(async () => {
                    Dispatcher.Invoke((Action)(async () => {
                        while (!String.IsNullOrEmpty(broadcastTextBox.Text)) {
                            char sendChar = broadcastTextBox.Text[0];
                            broadcastTextBox.Text = broadcastTextBox.Text.Remove(0, 1);
                            serialPort1?.Send(sendChar + "\n");
                            serialPort2?.Send(sendChar + "\n");
                            int sec = Int32.Parse(secondForm.Text);
                            await Task.Delay(sec);
                        }
                    }));
                });

                broadcastTextBox.IsEnabled = true;
                broadcastButton.IsEnabled = true;
                broadcastPerSecondButton.IsEnabled = true;
            }
        }
    }
}
