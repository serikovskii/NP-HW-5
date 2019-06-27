using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace NP_HW_5
{
    public partial class MainWindow : Window
    {
        UdpClient client;
        Thread receiveThread;
        int portEP;
        bool userIsConnected;
        bool buttonStop;
        string ipAddres;
        private ObservableCollection<string> namesUser;

        public MainWindow()
        {
            InitializeComponent();
            namesUser = new ObservableCollection<string>();
            namesUser.Add("Online list:");
            buttonStop = false;
            ipAddres = "235.5.5.1";
        }

        private void ConnectedButton(object sender, RoutedEventArgs e)
        {
            if (buttonStop)
            {
                Disconnect();
                connect.Content = "Start";
                connect.Background = new SolidColorBrush(Colors.Indigo);
                buttonStop = false;
            }
            else
            {
                portEP = int.Parse(port.Text);
                client = new UdpClient(portEP, AddressFamily.InterNetwork);
                client.JoinMulticastGroup(IPAddress.Parse(ipAddres), 20);
                receiveThread = new Thread(ReceiveProc);
                receiveThread.Start();

                string messageFirst = $"{name.Text} - join to chat";
                userIsConnected = true;
                Sending(messageFirst);
                connect.Content = "Stop";
                buttonStop = true;
            }
        }

        private void Disconnect()
        {
            string messageDisconnect="";
            Dispatcher.Invoke(() => messageDisconnect = $"{name.Text} - disabled");
            Sending(messageDisconnect);

        }
        private void ReceiveProc()
        {
            try
            {
                while (userIsConnected)
                {
                    IPEndPoint remoteEP = null;
                    byte[] buffer = client.Receive(ref remoteEP);
                    string messageReceive = Encoding.ASCII.GetString(buffer);
                    if (messageReceive.Contains("disabled"))
                    {
                        Dispatcher.Invoke(() => namesUser.Remove(ListOnline(messageReceive)));

                        userIsConnected = false;
                    }
                    else if (messageReceive.Contains("join"))
                    {
                        namesUser.Add(ListOnline(messageReceive));
                    }
                    Dispatcher.Invoke(() => chat.AppendText($"{messageReceive}\n"));
                    Dispatcher.Invoke(() => clients.ItemsSource = namesUser);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                client.DropMulticastGroup(IPAddress.Parse(ipAddres));
                client.Close();

            }
        }

        private void SendMessage(object sender, RoutedEventArgs e)
        {
            try
            {
                string messageToChat = $"{name.Text}: {message.Text}";
                Sending(messageToChat);
                message.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Sending(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            client.Send(buffer, buffer.Length, ipAddres, portEP);
        }

        private string ListOnline(string message)
        {
            var index = 0;
            var name = "";
            for (int i = 0; i < message.Length; i++)
            {
                if (message[i] == ' ')
                {
                    index = i;
                    break;
                }
            }

            for (int j = 0; j < index; j++)
            {
                name += message[j];
            }

            return name;
        }
    }
}
