using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.IO;
namespace FlitterClient {
	public interface IClientHandler {
		void OnReceive(Message msg);
		void OnError(string err);
		void OnStart(IClientDealer dealer);
		void OnEnd(IClientDealer dealer);
	}
	public interface IClientDealer{
		string Send(Message msg);
		void Disconnect();
		IPEndPoint GetEndpoint();
	}
	public class ClientDealer:IClientDealer {
		IPEndPoint m_endpoint;
		Socket m_socket;
		Stream m_recvStream;
		Stream m_sendStream;
		public ClientDealer(){
			m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}
		public string Send(Message msg){
			try {
				if (m_sendStream == null) {
            		m_sendStream = new NetworkStream(m_socket);
            		msg.Write(m_sendStream);
            		m_sendStream.Flush();
        		}
			} catch (System.Exception e) {
				return e.ToString();
			}
			return "";
		}
		public Message Recv(int time){
			if (m_socket.Poll(time, SelectMode.SelectRead)) {
                if (m_recvStream == null) {
            		m_recvStream = new NetworkStream(m_socket);
        		}
				Message msg = new Message();
				msg.Read(m_recvStream);
				return msg;
            } else {
                Thread.Sleep(time);
                return null;
            }
		}
		public bool IsConnected(){
			return m_socket.Connected;
		}
		public void Connect(string ip,int port){
        	m_endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
        	m_socket.Connect(m_endpoint);
		}
		public void Disconnect(){
			m_socket.Shutdown(SocketShutdown.Both);
			m_socket.Disconnect(false);
		}
		public IPEndPoint GetEndpoint(){
			return m_endpoint;
		}
	}
	public class Client {
		ClientDealer m_dealer;
		List<IClientHandler> m_handlers;
		Thread m_recvThread;
		public Client(){
			m_handlers = new List<IClientHandler>();
		}
	  	public void Start(string ip,int port,int recvFrequant){
	  		m_dealer = new ClientDealer();
	  		m_dealer.Connect(ip,port);
	  		int clientHandlerLen = m_handlers.Count;
	  		m_recvThread = new Thread(()=>{
	  			for (; m_dealer.IsConnected(); ) {
	  				try {
	  					var msg = m_dealer.Recv(recvFrequant);
	  					if (msg == null) {
	  						continue;
	  					}
	  					for (int i = 0; i < clientHandlerLen; i++) {
							m_handlers[i].OnReceive(msg);
						}
	  				} catch (System.Exception e) {
	  					for (int i = 0; i < clientHandlerLen; i++) {
							m_handlers[i].OnError(e.ToString());
						}
						break;
	  				}
	  			}
	  			for (int i = 0; i < clientHandlerLen; i++) {
	  				m_handlers[i].OnEnd(m_dealer);
				}
	  		});
			m_recvThread.Start();
			for (int i = 0; i < clientHandlerLen; i++) {
				m_handlers[i].OnStart(m_dealer);
			}
	  	}
	  	public void Rejister(IClientHandler handler){
			m_handlers.Add(handler);
	  	}
	}
}