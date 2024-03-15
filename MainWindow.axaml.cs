using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Logging;
using System;
using System.IO.Ports;
using System.Threading.Tasks;
namespace SimpleCOM
{

    public partial class MainWindow : Window
    {
        static SerialPort? serialPort;
        public MainWindow()
        {
            Logger.TryGet(LogEventLevel.Fatal, LogArea.Control)?.Log(this, "Avalonia Infrastructure");
            System.Diagnostics.Debug.WriteLine("System Diagnostics Debug");
            InitializeComponent();
            InitSerialPort();
            
        }

        private void InitSerialPort()
        {
            string[] SerialNames = SerialPort.GetPortNames();
            SerialList.Items.Clear();
            foreach (string SerialName in SerialNames)
                SerialList.Items.Add(SerialName);
                
            if (SerialList.Items.Count > 0 || SerialList.SelectedIndex != 0)
            {
                SerialList.SelectedIndex = 0;
            }

        }

        public void StartPort(object? sender, RoutedEventArgs e)
        {


            serialPort = new SerialPort();
            serialPort.BaudRate = GetSelectedValue<int>(BaudRateList,115200);
            serialPort.DataBits = GetSelectedValue<int>(DataBitsList, 8);
            serialPort.PortName = GetSelectedValue<String>(SerialList, " ");
            serialPort.Parity = Parity.None;


            try
            {
                serialPort.Open(); // 打开串口

                Task.Run(() =>
                {
                    while (true)
                    {
                        if (serialPort.BytesToRead > 0)
                        {
                            var receivedData = serialPort.ReadExisting();

                            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(new Action(() =>
                            {
                                ReciveTextShow.Text += receivedData; // 读取串口缓冲区中的数据并追加到 receivedData 中
                            }));
                        }
                        System.Threading.Thread.Sleep(100);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("读取串口数据时发生错误：" + ex.Message);
            }



        }

        private static T GetSelectedValue<T>(ComboBox comboBox, T defaultValue)
        {
            if (comboBox.Items[comboBox.SelectedIndex] != null)
            {
                try
                {
                    return (T)comboBox.Items[comboBox.SelectedIndex];
                }
                catch (InvalidCastException)
                {
                    // Ignore the exception and return the default value
                }
            }
            return defaultValue;
        }


        public void StopPort(object? sender, RoutedEventArgs e)
        {

        }

        public void ClosePort(object? sender, RoutedEventArgs e)
        {

        }
    }
}