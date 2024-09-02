using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
namespace SimpleCOM
{

    public partial class MainWindow : UserControl
    {
        private static SerialPort? _serialPort;
        private Timer _timer = null!;
        public MainWindow()
        {
            Logger.TryGet(LogEventLevel.Fatal, LogArea.Control)?.Log(this, "Avalonia Infrastructure");
            System.Diagnostics.Debug.WriteLine("System Diagnostics Debug");
            InitializeComponent();
            SerialPortUpdater();

        }

        

        private void SerialPortUpdater()
        {
            _timer = new Timer(1000); // 设置定时器的间隔为1000毫秒（1秒）
            _timer.Elapsed += TimerElapsed!;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(InitSerialPort);
        }


        private void InitSerialPort()
        {
            // 保存当前选中的项
            var selectedSerialName = SerialList.SelectedItem as string;

            string[] serialNames = SerialPort.GetPortNames();
            SerialList.Items.Clear();
            foreach (string serialName in serialNames)
                SerialList.Items.Add(serialName);

            // 尝试将选中的项设置回去
            if (serialNames.Contains(selectedSerialName))
            {
                SerialList.SelectedItem = selectedSerialName;
            }
            else if (SerialList.Items.Count > 0)
            {
                SerialList.SelectedIndex = 0;
            }
        }


        public void StartPort(object? sender, RoutedEventArgs e)
        {

            ButtonStop.IsEnabled = true;
            ButtonClose.IsEnabled = true;
            ButtonStart.IsEnabled = false;

            _serialPort = new SerialPort();

            _serialPort.PortName = SerialList.Items[SerialList.SelectedIndex] as string;

            {
                var contentProperty = BaudRateList.Items[BaudRateList.SelectedIndex]?.GetType().GetProperty("Content");
                if (contentProperty != null)
                {
                    var contentValue = (string)contentProperty.GetValue(BaudRateList.Items[BaudRateList.SelectedIndex])!;
                    var baudRateValue = int.Parse(contentValue);

                    _serialPort.BaudRate = baudRateValue;
                }
            }

            {
                var contentProperty = DataBitsList.Items[DataBitsList.SelectedIndex]?.GetType().GetProperty("Content");
                if (contentProperty != null)
                {
                    var contentValue = (string)contentProperty.GetValue(DataBitsList.Items[DataBitsList.SelectedIndex])!;
                    var baudRateValue = int.Parse(contentValue);

                    _serialPort.DataBits = baudRateValue;
                }
            }

            switch (ValiList.SelectedIndex)
            {
                case 0:
                    _serialPort.Parity = Parity.None;
                    break;
                case 1:
                    _serialPort.Parity = Parity.Odd;
                    break;
                case 2:
                    _serialPort.Parity = Parity.Even;
                    break;
                case 3:
                    _serialPort.Parity = Parity.Space;
                    break;
                case 4:
                    _serialPort.Parity = Parity.Even;
                    break;
            }

            _serialPort.StopBits = StopBitsList.SelectedIndex switch
            {
                0 => StopBits.One,
                1 => StopBits.OnePointFive,
                2 => StopBits.Two,
                _ => _serialPort.StopBits
            };

            try
            {
                _serialPort.Open(); // 打开串口

                Task.Run(() =>
                {
                    while (true)
                    {
                        if (_serialPort.BytesToRead > 0)
                        {
                            string receivedData;

                          
                            Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                if (ShowASCII.IsChecked == true)
                                {
                                    receivedData = _serialPort.ReadExisting(); // 读取ASCII数据
                                }

                                else
                                {
                                    // 如果不需要ASCII，则读取原始数据
                                    byte[] buffer = new byte[_serialPort.BytesToRead];
                                    _serialPort.Read(buffer, 0, buffer.Length);
                                    receivedData = BitConverter.ToString(buffer).Replace("-", " ");
                                }

                                if (DisplayTime.IsChecked == true)
                                {
                                    string currentTime = DateTime.Now.ToString("HH:mm:ss"); // 获取当前时间的字符串表示，格式为时:分:秒

                                    // 将接收到的数据与当前时间连接起来
                                    receivedData = receivedData + " - 时间: " + currentTime;
                                }               
                                if (AutoChangeLine.IsChecked == true)
                                {
                                    receivedData = receivedData + "\n\n";
                                }

                                ReciveTextShow.Text += receivedData;
                                ReciveTextScrollView.ScrollToEnd();
                            });
                        }
                        System.Threading.Thread.Sleep(1000);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("读取串口数据时发生错误：" + ex.Message);
            }



        }


        public void StopPort(object? sender, RoutedEventArgs e)
        {
            _serialPort?.DiscardInBuffer(); // 清空接收缓冲区
            _serialPort?.DiscardOutBuffer(); // 清空发送缓冲区
            _serialPort?.Close(); // 关闭串口

            ButtonStop.IsEnabled = false;
            ButtonStart.IsEnabled = true;
        }

        public void ClosePort(object? sender, RoutedEventArgs e)
        {
            _serialPort?.Close();
            ButtonStop.IsEnabled = false;
            ButtonClose.IsEnabled = false;
            ButtonStart.IsEnabled = true;
        }

        public void TriggerPane(object? sender, RoutedEventArgs e)
        {
            viewSplit.IsPaneOpen = !viewSplit.IsPaneOpen;
        }
    }
}