using System;
using System.Windows.Forms;
using System.Drawing;
using System.SimpleJson;
using System.Collections.Generic;
using FlitterClient;
namespace ExamMove {
    public struct MessageFrames  {
        Dictionary<uint>[]FrameMessage Frames
    }
    public struct FrameMessage {
        ulong TimeStamp;
        string Head;
        byte[] Body;
    }
    public struct MessageID  {
        uint ID;      
        ulong TimeStamp;
    }
    public struct MessageMoveBall {
        ulong TimeStamp;
        float X;   
        float Y;
    }
	public struct Vector {
		public float x;
		public float y;
		public static Vector operator +(Vector v1,Vector v2){
			return new Vector{
				x=v1.x+v2.x,
				y=v1.y+v2.y
			};
		}
		public static Vector operator *(Vector v,float sf){
			return new Vector{
				x=v.x*sf,
				y=v.y*sf
			};
		}
	}
	public class Actor {
	  	public uint id;
	  	public Vector pos;
	  	public Vector vel;
	  	Vector shandowPos;
        public ulong TimeStamp;
	  	Brush bursh;
	  	public Actor(){
	  		bursh = new SolidBrush(Color.FromArgb(255,255,50,70));
	  	}
	  	public void Update(){
	  		pos += vel;
        	float lerp = 0.1f;
        	shandowPos = shandowPos * (1-lerp) + pos*lerp;
	  	}
	  	public void Draw(Graphics g){
			g.FillEllipse(bursh, shandowPos.x, shandowPos.y, 30, 30);
	  	}
	}
    public struct ObjectMessage {
        int Head;
        uint ID;
        object Obj;
    }
    public class ExamClientHandler:IClientHandler{
        IClientDealer m_dealer;
        bool m_connected = false;
        uint m_id;
        Queue<ObjectMessage> m_msgQueue;
        ExamDemo m_demo;
        public ExamClientHandler(ExamDemo demo){
            m_demo = demo;
        }
        public void OnReceive(Message msg){
            Console.WriteLine("Client Receive: "+msg);
            var obj = SimpleJson.Deserialize<JsonObject>(msg.Body)
            switch (msg.Head) {
                case "Start":
                    m_id = uint.Parse(obj["ID"].ToString());
                    var timeStamp = ulong.Parse(obj["TimeStamp"].ToString());
                    m_demo.SetTimeStamp(timeStamp);
                    var createBody = SimpleJson.Serialize(new MessageMoveBall{
                        TimeStamp=timeStamp,
                        X = 50,
                        Y = 50
                    });
                    var createMsg = new Message("CreateBall",createBody);
                    Send(createMsg);
                    break;
                case "Frames":
                    var frames = (Dictionary<uint,object>)(obj["Frames"]);
                    foreach (kv in frames) {
                        uint id = kv.Key;
                        string valueStr = kv.Value.ToString();
                        var fmsg = SimpleJson.Deserialize<FrameMessage>(valueStr);
                        switch (fmsg.Head) {
                            case "CreateBall":
                                var createobj = SimpleJson.Deserialize<MessageMoveBall>(msg.Body);
                                m_msgQueue.Enqueue(new ObjectMessage{ID=id,Head=0,Obj=createobj});
                                break;
                            case "DeleteBall":
                                var deleteobj = SimpleJson.Deserialize<MessageID>(msg.Body);
                                m_msgQueue.Enqueue(new ObjectMessage{ID=id,Head=1,Obj=deleteobj});
                                break;
                            case "MoveBall":
                                var moveobj = SimpleJson.Deserialize<MessageMoveBall>(msg.Body);
                                m_msgQueue.Enqueue(new ObjectMessage{ID=id,Head=2,Obj=moveobj});
                                break;
                        }
                    }
                    break;
            }
        }
        public void OnError(string err){
            Console.WriteLine("Client Err: "+err);
        }
        public void OnConfig(string name,IClientHandlerGetter getter){

        }
        public Queue<ObjectMessage> GetQueue(){
            return m_msgQueue;
        }
        public void OnStart(IClientDealer dealer){
            m_dealer = dealer;
            Console.WriteLine("Client Connected "+dealer.GetEndpoint());
            m_connected = true;
            m_msgQueue = new Queue<ObjectMessage>();
        }
        public void Send(Message msg){
            string err = m_dealer.Send(msg);
            if (err!="") {
                Console.WriteLine("Client Send Error "+err);
            }
        }
        public void OnEnd(IClientDealer dealer){
            Console.WriteLine("Client DisConnected "+dealer.GetEndpoint());
        }   
    }
	public class ExamDemo:Form{
		Actor m_actor;
		float m_speed;
		Timer m_timer;
        Client m_client;
        Dictionary<uint,Actor> m_actors;
        ExamClientHandler m_handler;
        Queue<ObjectMessage> m_msgQueue;
		public static void Main(){
            m_actors = new Dictionary<uint,Actor>();
            Application.Run(new ExamDemo());
            m_client = new Client();
            m_handler = new ExamClientHandler(this);
            m_msgQueue = m_handler.GetQueue();
            m_client.Rejister(m_handler);
            m_client.Start("127.0.0.1", 9090, 10);
        }
        public void SetTimeStamp(ulong timeStamp){
            m_actor.TimeStamp = timeStamp;
        }
        public ExamDemo() {
        	Text = "Exam Move Demo";
        	BackColor = Color.Black;
        	Width = 1080;
        	Height = 680;
        	KeyPreview = true;
        	m_actor = new Actor();
        	m_speed = 8;
        	m_timer = new Timer();
        	m_timer.Interval = 10;
            m_timer.Tick += new EventHandler(OnTimeTick);
            m_timer.Start();
        }
        void OnTimeTick(object obj,EventArgs e){
            for (int i=0; i<10; i++) {
                if(m_msgQueue.Count<=0){
                    break
                }
                ObjectMessage msg = m_msgQueue.Dequeue();
                switch (msg.Head) {
                    case 0:
                        var createobj = (MessageMoveBall)msg.Obj;
                        break;
                    case 1:
                        var deleteobj = (MessageID)msg.Obj;
                        break;
                    case 2:
                        var moveobj = (MessageMoveBall)msg.Obj;
                        break;
                }
            }
        	m_actor.Update();
            m_actor.TimeStamp += 10; 
        	Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e){
        	Graphics g = e.Graphics;
        	g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			m_actor.Draw(g);
        }
        protected override void OnKeyDown(KeyEventArgs e){
        	switch (e.KeyCode) {
        		case Keys.Left:
        			m_actor.vel.x = -m_speed;
        			break;
        		case Keys.Right:
        			m_actor.vel.x = m_speed;
        			break;
        		case Keys.Up:
        			m_actor.vel.y = -m_speed;
        			break;
        		case Keys.Down:
        			m_actor.vel.y = m_speed;
        			break;
        	}
            var moveBody = SimpleJson.Serialize(new MessageMoveBall{
                TimeStamp=timeStamp,
                X = m_actor.vel.X,
                Y = m_actor.vel.Y
            });
            var moveMsg = new Message("MoveBall",moveBody);
            m_handler.Send(moveMsg);
        }
        protected override void OnKeyUp(KeyEventArgs e){
        	switch (e.KeyCode) {
        		case Keys.Left:
        			m_actor.vel.x = 0;
        			break;
        		case Keys.Right:
        			m_actor.vel.x = 0;
        			break;
        		case Keys.Up:
        			m_actor.vel.y = 0;
        			break;
        		case Keys.Down:
        			m_actor.vel.y = 0;
        			break;
        	}
            var moveBody = SimpleJson.Serialize(new MessageMoveBall{
                TimeStamp=timeStamp,
                X = m_actor.vel.X,
                Y = m_actor.vel.Y
            });
            var moveMsg = new Message("MoveBall",moveBody);
            m_handler.Send(moveMsg);
        }
	}
}