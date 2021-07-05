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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO.Ports;

namespace MultiSerialPortViewer {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    /// 
    public partial class MainWindow : Window {
        public ObservableCollection<COMDevice> comLists = new ObservableCollection<COMDevice>();


        public MainWindow() {
            InitializeComponent();
        }

        private void UpdatePortsList() {
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports) {
                portComboBox.Items.Add(port);
            }

            if (portComboBox.Items.Count > 0) {
                portComboBox.SelectedIndex = 0;
            }
        }

        private void Form_Load(object sender, RoutedEventArgs e) {
            UpdatePortsList();
        }

        private void Form_Closed(object sender, EventArgs e) {
            foreach(COMDevice com in comLists) {
                if (com?.IsOpen == true) com.Close();
            }
            Console.WriteLine("Close Window");
        }


        private void Clicked_ConnectBT(object sender, RoutedEventArgs e) {

            int newID = comLists.Count;
            COMDevice com = new COMDevice(newID, portComboBox.SelectedItem.ToString());

            try {
                com.Connect();
                if (com.IsOpen) {
                    Console.WriteLine($"COMポート {com.PortName} に接続しました！");
                    AddNewBTTab(com);
                } else {
                    throw new Exception($"COMポート {com.PortName} が開けません！");
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        void AddNewBTTab(COMDevice com) {
            this.comLists.Add(com);

            TabItem item = new TabItem();
            item.Header = com.PortName;
            item.Content = new BTTab(com, this);
            BTTabItems.Items.Add(item);
            BTTabItems.SelectedIndex = this.comLists.Count - 1;

            if (BTTabItems.Items.Count != 0) { 
                TabPanel.Visibility = Visibility.Visible;
            }

            Console.WriteLine(comLists.Count());
        }

        public void ClosePort(int id) {
            this.comLists[id].Close();
            this.comLists.Remove(this.comLists[id]);
            foreach(COMDevice com in this.comLists) {
                Console.WriteLine($"com {com.Id}");
                if (com.Id > id) com.Id--;
                Console.WriteLine($"{com.Id} | ");
            }

            Console.WriteLine($"tab {BTTabItems.Items.Count} ");
            BTTabItems.Items.Remove(BTTabItems.SelectedItem);
            Console.WriteLine($"{BTTabItems.Items.Count} | ");

            if (this.comLists.Count == 0) {
                TabPanel.Visibility = Visibility.Collapsed;
            }
        }


        // つながってるシリアルポートすべてにブロードキャスト
        private void BroadcastTextBoxToPort() {
            if (!String.IsNullOrEmpty(broadcastTextBox.Text)) {
                foreach(COMDevice com in this.comLists) {
                    com.Send(broadcastTextBox.Text + "\n");
                }
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

                            foreach (COMDevice com in this.comLists) {
                                com.Send(sendChar + "\n");
                            }

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