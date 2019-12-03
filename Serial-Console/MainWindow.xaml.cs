using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using System.IO.Ports;
using System;

namespace Serial_Console
{
    public partial class MainWindow : MetroWindow
    {
        /*************************************************************************************************
        *** Variables
        *************************************************************************************************/
        private SerialPort serialPort;
        private bool _consoleInputEnabled;
        private readonly int[] baudRates = { 9600, 115200 };
        private readonly Brush MSG_COLOR_LIGHT = Brushes.White;
        private readonly Brush MSG_COLOR_DARK = Brushes.Gray;

        /*************************************************************************************************
        *** Constructor
        *************************************************************************************************/
        public MainWindow()
        {
            InitializeComponent();

            ComboBox_Port.ItemsSource = SerialPort.GetPortNames();
            ComboBox_Port.SelectedIndex = 0;
            ComboBox_BaudRate.ItemsSource = baudRates;
            ComboBox_BaudRate.SelectedIndex = 0;

            ConsoleInputEnabled = false;
        }

        /*************************************************************************************************
        *** Properties
        *************************************************************************************************/
        private bool SerialPortOpen { get { return serialPort != null && serialPort.IsOpen; } }

        private bool ConsoleInputEnabled
        {
            get { return _consoleInputEnabled; }

            set
            {
                _consoleInputEnabled = value;
                TextBox_Prefix.IsEnabled = value;
                TextBox_Message.IsEnabled = value;
                TextBox_Suffix.IsEnabled = value;
            }
        }

        /*************************************************************************************************
        *** Methods
        *************************************************************************************************/
        private void LogMessageToConsole(string msg, Brush brush)
        {
            TextBlock textBlock = new TextBlock() { Text = msg, Foreground = brush, TextWrapping = TextWrapping.Wrap };
            ListBox_Console.Items.Add(textBlock);

            if ((bool)CheckBox_Autoscroll.IsChecked)
                ListBox_Console.ScrollIntoView(textBlock);
        }

        /*************************************************************************************************
        *** Events
        *************************************************************************************************/
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                LogMessageToConsole("> " + ((SerialPort)sender).ReadExisting(), MSG_COLOR_LIGHT);
            });
        }

        private void Button_Send_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBox_Message.Text))
            {
                if (SerialPortOpen)
                {
                    string msg = TextBox_Prefix.Text + TextBox_Message.Text + TextBox_Suffix.Text;
                    LogMessageToConsole("< " + msg, MSG_COLOR_DARK);
                    TextBox_Message.Text = "";
                    serialPort.Write(msg);
                }
                else
                {
                    MessageBox.Show("Serial port not open.", "Serial Console - Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TextBox_Prefix_TextChanged(object sender, TextChangedEventArgs e)
        {
            Label_Prefix.Visibility = string.IsNullOrEmpty(TextBox_Prefix.Text)
                                      ? Visibility.Visible : Visibility.Hidden;
        }

        private void TextBox_Message_TextChanged(object sender, TextChangedEventArgs e)
        {
            Label_Message.Visibility = string.IsNullOrEmpty(TextBox_Message.Text)
                                          ? Visibility.Visible : Visibility.Hidden;
        }

        private void TextBox_Suffix_TextChanged(object sender, TextChangedEventArgs e)
        {
            Label_Suffix.Visibility = string.IsNullOrEmpty(TextBox_Suffix.Text)
                                          ? Visibility.Visible : Visibility.Hidden;
        }

        private void TextBox_Message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
                Button_Send_Click(this, null);
        }

        private void Button_Console_Clear_Click(object sender, RoutedEventArgs e)
        {
            ListBox_Console.Items.Clear();
        }

        private void ListBox_Console_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
                ListBox_Console.Items.Clear();
        }

        private void ListBox_Console_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox_Console.SelectedItem = null;
        }

        private void ComboBox_Port_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox_Port.ItemsSource = SerialPort.GetPortNames();
        }

        private void Button_Connect_Click(object sender, RoutedEventArgs e)
        {
            if (!SerialPortOpen)
            {
                try
                {
                    string portName = ComboBox_Port.SelectedItem.ToString();
                    int baudRate = (int)ComboBox_BaudRate.SelectedItem;
                    serialPort = new SerialPort(portName, baudRate);
                    serialPort.Open();
                    serialPort.DataReceived -= SerialPort_DataReceived;
                    serialPort.DataReceived += SerialPort_DataReceived;

                    LogMessageToConsole("- Serial port " + portName + " created with " + baudRate + " daud rate", MSG_COLOR_DARK);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("> Error creating serial port.\n\n" + exception.ToString()
                                    , "Serial Console - Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                serialPort.Close();
            }

            Button_Connect.Content = SerialPortOpen ? "Connected" : "Connect";
            ConsoleInputEnabled = SerialPortOpen;
        }

        private void CheckBox_TopMost_Click(object sender, RoutedEventArgs e)
        {
            Topmost = (bool)CheckBox_TopMost.IsChecked;
        }
    }
}
