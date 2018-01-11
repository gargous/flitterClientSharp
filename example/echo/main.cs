using System;
using FlitterClient;
using System.Threading;
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
		public void OnRecvMessage(Message msg){
			Console.WriteLine("Client Receive Message: "+msg);
			m_receiveCount++;
			DisConnect();
		}
		public void OnRecvFrame(FrameMessage msg){
			Console.WriteLine("Client Receive Frame: "+msg);
			string err = m_dealer.SendFrame(msg);
			if (err != "") {
				Console.WriteLine("Client Send Frame Error "+err);
			}
		}
		public void OnRecvQueues(FrameQueuesMessage msg){
			Console.WriteLine("Client Receive Queues: "+msg);
		}
		public void OnError(string err){
			Console.WriteLine("Client Err: "+err);
		}
		public void OnConfig(string name,IClientHandlerGetter getter){
			Console.WriteLine("Client Config: "+name);
		}
		public void OnStart(IClientDealer dealer){
			m_dealer = dealer;
			Console.WriteLine("Client Connected "+dealer.GetEndpoint());
			m_connected = true;
			var t = new Thread(()=>{

				for (int i=0;i<3; i++) {
					Thread.Sleep(300);
					string err = m_dealer.SendMessage(new Message("Say",System.Text.Encoding.Default.GetBytes("Hi2"+i)));
					if (err != "") {
						Console.WriteLine("Client Send Message Error "+err);
					}
				}
				for (int i=0;i<3; i++) {
					Thread.Sleep(300);
					DateTime now = DateTime.Now;
					string err = m_dealer.SendFrame(new FrameMessage("Spell",System.Text.Encoding.Default.GetBytes("Hi1"+i),(ulong)now.Ticks));
					if (err != "") {
						Console.WriteLine("Client Send Frame Error "+err);
					}
				}
			});
			t.Start();
			DisConnect();
		}
		public void OnEnd(IClientDealer dealer){
			Console.WriteLine("Client DisConnected "+dealer.GetEndpoint());
		}	
	}
	public class ExamDemo{
        static void Main(string[] args){
        	var app = new Client();
        	app.Rejister("EchoTest",new ExamClientHandler());
			app.Start("127.0.0.1", 9090, 10);
        }
    }
}