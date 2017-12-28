using System;
using FlitterClient;
namespace ExamEcho{
	public class ExamClientHandler:IClientHandler{
		IClientDealer m_dealer;
		int m_receiveCount = 0;
		bool m_connected = false;
		void DisConnect(){
			if (m_connected && m_dealer!=null && m_receiveCount>=3) {
				m_dealer.Disconnect();
			}
		}
		public void OnReceive(Message msg){
			Console.WriteLine("Client Receive: "+msg);
			m_receiveCount++;
			DisConnect();
		}
		public void OnError(string err){
			Console.WriteLine("Client Err: "+err);
		}
		public void OnConfig(string name,IClientHandlerGetter getter){

		}
		public void OnStart(IClientDealer dealer){
			m_dealer = dealer;
			Console.WriteLine("Client Connected "+dealer.GetEndpoint());
			string err = dealer.Send(new Message("Spell",System.Text.Encoding.Default.GetBytes("Hi")));
			if (err != "") {
				Console.WriteLine("Client Send Error "+err);
			}
			m_connected = true;
			DisConnect();
		}
		public void OnEnd(IClientDealer dealer){
			Console.WriteLine("Client DisConnected "+dealer.GetEndpoint());
		}	
	}
	public class ExamDemo{
        static void Main(string[] args){
        	var app = new Client();
        	app.Rejister(new ExamClientHandler());
			app.Start("127.0.0.1", 9090, 10);
        }
    }
}