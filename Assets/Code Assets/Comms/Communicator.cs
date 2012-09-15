using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Com {
    public class Communicator {

        private IPEndPoint endpoint;
        private TcpClient client;
        private NetworkStream networkStream;
        private StreamWriter writerStream;
        private byte delimiter;

        private AsyncCallback OnConnect;
        private AsyncCallback OnSend;
        private AsyncCallback OnReceive;

        private ReceiveHandler Handle;

        public bool Connected { get { return this.client.Connected; } }

        /// <summary>
        /// Handles communications between the Scala Agents and the Unity NPCs.
        /// </summary>
        public Communicator(ReceiveHandler handler, IPEndPoint remote) {
            //Create the endpoint.
            //IPAddress[] address = Dns.GetHostAddresses("localhost");
            this.endpoint = remote;

            //Create the socket.
            this.client = new TcpClient();
            this.client.NoDelay = true;

            this.delimiter = UTF8Encoding.UTF8.GetBytes(";")[0];

            //Specify handler and callbacks.
            this.Handle = handler;
            this.OnConnect = this.ConnectCallback;
            this.OnSend = this.SendCallback;
            this.OnReceive = this.ReceiveCallback;
        }

        /// <summary>
        /// Connects the Communicator to the Scala Agents, blocking.
        /// </summary>
        public void Connect() {
            client.Connect(this.endpoint);

            this.networkStream = this.client.GetStream();
            this.writerStream = new StreamWriter(this.networkStream);
            this.writerStream.AutoFlush = true;

            //Debug.Log("Communication connection established.");
            Console.WriteLine("Communication connection established with remote server.");
            Console.WriteLine("Remote end point: " + this.client.Client.RemoteEndPoint);
            Console.WriteLine("Local end point:  " + this.client.Client.LocalEndPoint);
        }

        /// <summary>
        /// Completes an asynchronous connect.  Not used.
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar) {
            client.EndConnect(ar);

            this.networkStream = this.client.GetStream();
            this.writerStream = new StreamWriter(this.networkStream);

            //Debug.Log("Communication connection established.");
            Console.WriteLine("Communication connection established.");
        }

        /// <summary>
        /// Sends data to the Scala Agent server, blocking.
        /// </summary>
        /// <param name="data">The string to send over the socket.</param>
        public void Send(string data) {
            //Console.WriteLine("Sending data...");
            if (this.Connected) this.writerStream.WriteLine(data);
        }
        /// <summary>
        /// Completes the asynchronous send.  Not used.
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar) {
            this.networkStream.EndWrite(ar);

            //Debug.Log("Sent "+ (int)ar.AsyncState + " bytes to the Scala Agent server.");
            //Console.WriteLine("Sent " + 0 + " bytes to the Scala Agent server.");
        }

        /// <summary>
        /// Asynchronously receives data from the Scala Agent server.  This must
        /// be called after Communicator.Connect().
        /// </summary>
        public void Receive() {
            //Debug.Log("Beginning receive thread.");
            //Console.WriteLine("Beginning receive thread.");

            Message message = new Message();
            this.networkStream.BeginRead(message.Buffer, 0, Message.BufferSize, this.OnReceive, message);
        }
        /// <summary>
        /// Completes the asynchronous receive.
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar) {
            //Read data from server.
            int bytesRead;
            try {
                bytesRead = this.networkStream.EndRead(ar);
                //Debug.Log("Read " + bytesRead + " bytes from server.");
                //Console.WriteLine("Read " + bytesRead + " bytes from server.");
            } catch (ObjectDisposedException) {
                //No big deal, the other thread closed the socket.
                return;
            }

            //Get the message being processed.
            Message message = (Message)ar.AsyncState;

            //TODO: Figure out why there's nonsense here and remove it, rather than filter here.
            List<byte> filter = new List<byte>();
            for (int i = 0; i < bytesRead; i++) {
                if (message.Buffer[i] != 0) {
                    if (message.Buffer[i] == this.delimiter) {
                        //Store data received thus far.
                        if (filter.Count > 0) message.Builder.Append(UTF8Encoding.UTF8.GetString(filter.ToArray()));
                        
                        string received = message.Builder.ToString();
                        this.Handle(received);

                        //Dump filter, and builder.
                        filter.Clear();
                        message.Builder.Remove(0, message.Builder.Length);
                    } else {
                        filter.Add(message.Buffer[i]);
                    }
                }
            }

            //Store data received thus far.
            if (filter.Count > 0) message.Builder.Append(UTF8Encoding.UTF8.GetString(filter.ToArray()));

            //Check for more data.
            //Debug.Log("Continuing to receive.");
            //Console.WriteLine("Continuing to receive.");
            this.networkStream.BeginRead(message.Buffer, 0, Message.BufferSize, this.OnReceive, message);
        }
        /// <summary>
        /// A blcoking receive.
        /// </summary>
        /// <returns>The received message.</returns>
        public string RecieveBlocking() {
            Message message = new Message();
            int bytesRead;
            //Get the bytes!
            try {
                 bytesRead = this.networkStream.Read(message.Buffer, 0, Message.BufferSize);
                //Debug.Log("Read " + bytesRead + " bytes from server.");
                //Console.WriteLine("Read " + bytesRead + " bytes from server.");
            } catch (ObjectDisposedException) {
                //No big deal, the other thread closed the socket.
                return null;
            }

            //TODO: Figure out why there's nonsense here and remove it, rather than filter here.
            List<byte> filter = new List<byte>();
            //string received;
            for (int i = 0; i < bytesRead; i++) {
                if (message.Buffer[i] != 0) {
                    //if (message.Buffer[i] == this.delimiter) {
                    //    //Store data received thus far.
                    //    if (filter.Count > 0) message.Builder.Append(UTF8Encoding.UTF8.GetString(filter.ToArray()));

                    //    received = message.Builder.ToString();
                    //    this.Handle(received);

                    //    //Dump filter, and builder.
                    //    filter.Clear();
                    //    message.Builder.Remove(0, message.Builder.Length);
                    //} else {
                    //    filter.Add(message.Buffer[i]);
                    //}
                    filter.Add(message.Buffer[i]);
                }
            }

            return UTF8Encoding.UTF8.GetString(filter.ToArray());
        }

        /// <summary>
        /// Closes the connection to the server.
        /// </summary>
        public void Close() {
            this.writerStream.Close();
            this.networkStream.Close();
            this.client.Close();
        }
    }

    public delegate void ReceiveHandler(string received);
}