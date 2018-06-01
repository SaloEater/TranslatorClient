using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace TranslatorClient
{
    class Client
    {
        IPAddress iPAddress;
        int port;
        TcpClient tcpClient;
        NetworkStream netStream;
        FileUtils fileUtils;
        public Dictionary<string, int> versions;

        public Client(IPAddress ip, int port)
        {
            iPAddress = ip;
            this.port = port;
            fileUtils = new FileUtils();
        }

        public Dictionary<string, int> GetFileIdsFromServer()
        {
            ConnectToServer();
            if (netStream == null) throw new NullReferenceException();
            SendData(BitConverter.GetBytes(0), netStream, 0);
            byte[] bytesFiles = ReceiveData(tcpClient, netStream);
            DisconnectFromServer();
            string filesStr = System.Text.Encoding.UTF8.GetString(bytesFiles);
            Console.WriteLine("Received: {0}", filesStr);
            Dictionary<string, int> files = JsonConvert.DeserializeObject<Dictionary<string, int>>(filesStr);
            return files;
        }

        public void ConnectToServer()
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(new IPEndPoint(iPAddress, port));
                netStream = tcpClient.GetStream();
                Console.WriteLine("Connected to server!");
            } catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Ошибка при подключении");
            }
        }

        public void DisconnectFromServer()
        {
            netStream.Close();
            tcpClient.Close();
            Console.WriteLine("Disconnected from server!");
        }

        /*internal long GetServersideFileSize(string filename)
        {
            ConnectToServer();
            byte[] bytesfilename = System.Text.Encoding.UTF8.GetBytes(filename);
            SendData(bytesfilename, netStream, 4);
            byte[] bytesSize = ReceiveData(tcpClient, netStream);
            long fileSize = BitConverter.ToInt64(bytesSize, 0);
            return fileSize;
        }*/

        internal string GetFile(string filename)
        {
            ConnectToServer();
            if (netStream == null) throw new NullReferenceException();
            byte[] bytesfilename = System.Text.Encoding.UTF8.GetBytes(filename);
            SendData(bytesfilename, netStream, 1);
            byte[] bytesFile = ReceiveData(tcpClient, netStream);
            string file = System.Text.Encoding.UTF8.GetString(bytesFile);
            return file;
        }

        internal void SendFile(string filename, TranslationsList translations)
        {
            if (translations == null)
            {
                Console.WriteLine("{0} send null", filename);
                return;
            }
            ConnectToServer();
            if (netStream == null) throw new NullReferenceException();
            byte[] bytesFile = System.Text.Encoding.UTF8.GetBytes(filename);
            SendData(bytesFile, netStream, 3);
            //TranslationsList translations = fileUtils.ReadFormattedFile(filename);
            string translationsStr = fileUtils.FormattedToString(translations);
            byte[] bytesTrans = System.Text.Encoding.UTF8.GetBytes(translationsStr);
            SendDataOnly(bytesTrans, netStream);
        }

        internal int AskVersion(string filename)
        {
            ConnectToServer();
            if (netStream == null) throw new NullReferenceException();
            byte[] bytesFile = System.Text.Encoding.UTF8.GetBytes(filename);
            SendData(bytesFile, netStream, 5);
            byte[] bytesResponse = ReceiveData(tcpClient, netStream);
            int version = BitConverter.ToInt32(bytesResponse, 0);
            return version;
            
        }

        private void SendData(byte[] data, NetworkStream stream, int responseType)
        {
            SendDataOnly(BitConverter.GetBytes(responseType), stream);
            SendDataOnly(data, stream);            
        }

        private void SendDataOnly(byte[] data, NetworkStream stream)
        {
            int bufferSize = 1024;

            byte[] dataLength = BitConverter.GetBytes(data.Length);

            stream.Write(dataLength, 0, 4);

            int bytesSent = 0;
            int bytesLeft = data.Length;

            while (bytesLeft > 0)
            {
                int curDataSize = Math.Min(bufferSize, bytesLeft);

                stream.Write(data, bytesSent, curDataSize);

                bytesSent += curDataSize;
                bytesLeft -= curDataSize;
            }
        }

        private byte[] ReceiveData(TcpClient client, NetworkStream stream)
        {
            byte[] fileSizeBytes = new byte[4];
            int bytes = stream.Read(fileSizeBytes, 0, 4);
            int dataLength = BitConverter.ToInt32(fileSizeBytes, 0);

            int bytesLeft = dataLength;
            byte[] data = new byte[dataLength];

            int bufferSize = 1024;
            int bytesRead = 0;

            while (bytesLeft > 0)
            {
                int curDataSize = Math.Min(bufferSize, bytesLeft);
                if (client.Available < curDataSize)
                {
                    curDataSize = client.Available;
                }

                bytes = stream.Read(data, bytesRead, curDataSize);

                bytesRead += curDataSize;
                bytesLeft -= curDataSize;
            }

            return data;
        }

        internal void ChangeIP(IPAddress ip)
        {
            iPAddress = ip;
        }

        internal bool TryConnect()
        {
            ConnectToServer();
            if (netStream == null) return false;
            DisconnectFromServer();
            return true;
        }

        internal void SaveIP()
        {
            fileUtils.SaveIP(iPAddress.ToString());
        }
    }
}
