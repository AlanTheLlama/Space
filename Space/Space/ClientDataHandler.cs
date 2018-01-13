using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Space {

    public class StateObject {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    class ClientDataHandler {

        Socket client;
        IPHostEntry ipHostInfo;
        IPAddress ipAddress;
        IPEndPoint remoteEP;

        public static bool connected = false;

        public ClientDataHandler() {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //ipHostInfo = Dns.GetHostEntry("frankensquad.zapto.org"); //Joel's DDNS
            //ipAddress = ipHostInfo.AddressList[0];
            //remoteEP = new IPEndPoint(ipAddress, 31579);
            remoteEP = new IPEndPoint(IPAddress.Parse("207.216.252.138"), 31579);
        }

        public bool isConnected() { return connected; }

        public void connectToServer() {
            try {
                ASCIIEncoding encoder = new ASCIIEncoding();

                System.Diagnostics.Debug.WriteLine("Beginning to poll for a connection...");
                client.BeginConnect(remoteEP, new AsyncCallback(AcceptCallback), client);
            } catch (Exception e) {
                client.Close();
                System.Diagnostics.Debug.WriteLine("Connection Failed! \n" + e);
                connected = false;
            }
        }

        public void AcceptCallback(IAsyncResult ar) {
            Socket handler = (Socket)ar.AsyncState;
            connected = true;
            handler.EndConnect(ar);

            System.Diagnostics.Debug.WriteLine("Connected!");

            StateObject state = new StateObject();
            state.workSocket = handler;
            try {
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveStuff), state);
            } catch (SocketException se) {
                System.Diagnostics.Debug.WriteLine("Server connected but did not ping back!\n ERROR: " + se);
            }
        }

        public void Send(String data) {
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            //if(client.Connected)
                client.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar) {
            try {
                Socket client = (Socket)ar.AsyncState;

                int bytesSent = client.EndSend(ar);
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        public void Receive() {
            try {
                StateObject state = new StateObject();
                state.workSocket = client;

                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveStuff), state);
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        public void ReceiveStuff(IAsyncResult ar) {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            ASCIIEncoding encoder = new ASCIIEncoding();

            int bytesRead = handler.EndReceive(ar);

            String content = String.Empty;

            if (bytesRead > 0) {
                state.sb.Append(encoder.GetString(state.buffer, 0, bytesRead));

                content = state.sb.ToString();

                if (content.IndexOf("<EOF>") > -1) {
                    System.Diagnostics.Debug.WriteLine("CLIENT: Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);
                } else {
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveStuff), state);
                }

                state.sb.Clear();

                String rec = content;
                System.Diagnostics.Debug.WriteLine("CLIENT-RECIEVED: " + rec);
            }
        }

        private static void ConnectCallback(IAsyncResult ar) {
            try {
                Socket client = (Socket)ar.AsyncState;

                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
