using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.IO;
namespace FlitterClient {
	public interface IClientHandlerGetter {
		IClientHandler GetClientHandler(string name);
	}
	public interface IClientHandler {
		void OnConfig(string name,IClientHandlerGetter getter);
		void OnRecvMessage(Message msg);
		void OnRecvFrame(FrameMessage msg);
		void OnRecvQueues(FrameQueuesMessage msg);
		void OnError(string err);
		void OnStart(IClientDealer dealer);
		void OnEnd(IClientDealer dealer);
	}
	public interface IClientDealer{
		string SendMessage(Message msg);
		string SendFrame(FrameMessage msg);
		string SendQueues(FrameQueuesMessage msg);
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
		string send(MessageType t,Writer r){
			try {
				if (m_sendStream == null) {
            		m_sendStream = new NetworkStream(m_socket);
        		}
        		t.Write(m_sendStream);
            	r.Write(m_sendStream);
            	m_sendStream.Flush();
        		return "";
			} catch (System.Exception e) {
				return e.ToString();
			}
		}
		public string SendMessage(Message msg){
			return send(MessageType.Normal,msg);
		}
		public string SendFrame(FrameMessage msg){
			return send(MessageType.Frame,msg);
		}
		public string SendQueues(FrameQueuesMessage msg){
			return send(MessageType.FrameQueues,msg);
		}
		public void Recv(int time,Action<MessageType,Message,FrameMessage,FrameQueuesMessage> cb){
			if (m_socket.Poll(time, SelectMode.SelectRead)) {
                if (m_recvStream == null) {
            		m_recvStream = new NetworkStream(m_socket);
        		}
        		MessageType t = new MessageType(0);
        		t.Read(m_recvStream);
        		switch ((byte)t) {
        			case MessageType.Normal:
        				Message msg = new Message();
						msg.Read(m_recvStream);
						cb(t,msg,null,null);
        				break;
        			case MessageType.Frame:
        				FrameMessage frame = new FrameMessage();
						frame.Read(m_recvStream);
						cb(t,null,frame,null);
        				break;
        			case MessageType.FrameQueues:
        				FrameQueuesMessage queues = new FrameQueuesMessage();
						queues.Read(m_recvStream);
						cb(t,null,null,queues);
        				break;
        			default:
        			  	break;
        		}
            } else {
                Thread.Sleep(time);
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
	public class Client:IClientHandlerGetter {
		ClientDealer m_dealer;
		Dictionary<string,IClientHandler> m_handlers;
		Thread m_recvThread;
		public Client(){
			m_handlers = new Dictionary<string,IClientHandler>();
		}
	  	public void Start(string ip,int port,int recvFrequant){
	  		m_dealer = new ClientDealer();
	  		m_dealer.Connect(ip,port);
	  		int clientHandlerLen = 0;
	  		List<string> clientsName = new List<string>();
	  		List<IClientHandler> clientsHandler = new List<IClientHandler>();
	  		foreach (var kv in m_handlers) {
	  			clientsName.Add(kv.Key);
	  			clientsHandler.Add(kv.Value);
	  			clientHandlerLen++;
	  		}
	  		m_recvThread = new Thread(()=>{
	  			for (int i = 0; i < clientHandlerLen; i++) {
	  				clientsHandler[i].OnConfig(clientsName[i],this);
					clientsHandler[i].OnStart(m_dealer);
				}
	  			for (; m_dealer.IsConnected(); ) {
	  				try {
	  					m_dealer.Recv(recvFrequant,(t,msg,frame,queues)=>{
	  						switch ((byte)t) {
								case MessageType.Normal:
									for (int i = 0; i < clientHandlerLen; i++) {
										clientsHandler[i].OnRecvMessage(msg);
									}
        							break;
        						case MessageType.Frame:
        							for (int i = 0; i < clientHandlerLen; i++) {
										clientsHandler[i].OnRecvFrame(frame);
									}
        							break;
        						case MessageType.FrameQueues:
        							for (int i = 0; i < clientHandlerLen; i++) {
										clientsHandler[i].OnRecvQueues(queues);
									}
        							break;
        						default:
        			 	 			break;
							}
							
	  					});
	  				} catch (System.Exception e) {
	  					for (int i = 0; i < clientHandlerLen; i++) {
							clientsHandler[i].OnError(e.ToString());
						}
						break;
	  				}
	  			}
	  			for (int i = 0; i < clientHandlerLen; i++) {
	  				clientsHandler[i].OnEnd(m_dealer);
				}
	  		});
			m_recvThread.Start();
	  	}
	  	public void Rejister(string name,IClientHandler handler){
			m_handlers.Add(name,handler);
	  	}
	  	public IClientHandler GetClientHandler(string name){
	  		return m_handlers[name];
	  	}
	}
}