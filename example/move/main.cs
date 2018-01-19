using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using SimpleJson;
using System.Collections.Generic;
namespace ExamMove {
    public struct MessageID  {
        public ulong ID;
        public string ToJson(){
            var jobj = new JsonObject();
            jobj.Add("ID",ID);
            return jobj.ToString(); 
        }
        static public MessageID FromJson(string jobjStr){
            var jobj = SimpleJson.SimpleJson.DeserializeObject<JsonObject>(jobjStr);
            var mid = new MessageID();
            mid.ID = ulong.Parse(jobj["ID"].ToString());
            return mid; 
        }
    }
	public struct Vector {
		public float X;
		public float Y;
        public Vector(float x,float y){
            X = x;
            Y = y;
        }
		public static Vector operator +(Vector v1,Vector v2){
			return new Vector{
				X=v1.X+v2.X,
				Y=v1.Y+v2.Y
			};
		}
		public static Vector operator *(Vector v,float sf){
			return new Vector{
				X=v.X*sf,
				Y=v.Y*sf
			};
		}
        public bool Equals(Vector v){
            if (X!=v.X) {
                return false;
            }
            if (Y!=v.Y) {
                return false;
            }
            return true;
        }
        public string ToJson(){
            var jobj = new JsonObject();
            jobj.Add("X",X);
            jobj.Add("Y",Y);
            return jobj.ToString();
        }
        static public Vector FromJson(string jobjStr){
            var jobj = SimpleJson.SimpleJson.DeserializeObject<JsonObject>(jobjStr);
            var vec = new Vector();
            vec.X = float.Parse(jobj["X"].ToString());
            vec.Y = float.Parse(jobj["Y"].ToString());
            return vec; 
        }
	}
	public class Actor {
	  	public ulong id;
	  	public Vector pos;
	  	public Vector vel;
        public Vector size;
	  	Vector m_shandowPos;
	  	Brush m_bursh;
        ulong m_timeStamp;
	  	public Actor(ulong id,ulong timeStamp){
            size = new Vector(30,30);
	  		m_bursh = new SolidBrush(Color.FromArgb(255,255,50,70));
            this.id = id;
            m_timeStamp = timeStamp;
	  	}
	  	public void Update(int deltaTime){
            m_timeStamp += (ulong)deltaTime;
	  		pos += vel*(0.001f*deltaTime);
        	float lerp = 0.1f;
        	m_shandowPos = m_shandowPos * (1-lerp) + pos*lerp;
	  	}
        public void Move(ulong timeStamp,Vector vel){
            long newTime = (long)timeStamp;
            long oldTime = (long)m_timeStamp;
            long diff = newTime - oldTime;
            Console.WriteLine(newTime+"-"+oldTime+"="+diff);

            pos += this.vel*(0.001f*diff);
            
            this.vel = vel;
            m_timeStamp = timeStamp;
        }
	  	public void Draw(Graphics g){
			g.FillEllipse(m_bursh, m_shandowPos.X, m_shandowPos.Y, size.X, size.Y);
	  	}
	}
    public class ExamClientHandler:FlitterClient.IClientHandler{
        FlitterClient.IClientDealer m_dealer;
        bool m_connected = false;
        ulong m_id;
        event System.Action<ulong,ulong,ulong> m_startHandler = (a,b,c)=>{};
        event System.Action<bool,ulong,ulong> m_createHandler = (a,b,c)=>{};
        event System.Action<bool,ulong,ulong> m_deleteHandler = (a,b,c)=>{};
        event System.Action<bool,ulong,Vector,ulong> m_moveHandler = (a,b,v,c)=>{};
        Queue<FlitterClient.Message> m_waitQueue;
        ExamDemo m_demo;
        ulong m_timeStamp = 0;
        ulong m_deltaTime = 10;
        public ExamClientHandler(ExamDemo demo){
            m_demo = demo;
        }
        public ulong GetTimeStamp(){
            return m_timeStamp;
        }
        public void SetTimeStamp(ulong timeStamp){
            m_timeStamp = timeStamp;
        }
        public void OnRecvMessage(FlitterClient.Message msg){
            Console.WriteLine("FlitterClient.Client Receive FlitterClient.Message: "+msg);
        }
        public void OnRecvFrame(FlitterClient.FrameMessage msg){
            Console.WriteLine("FlitterClient.Client Receive Frame: "+msg);
            switch (msg.Head) {
                case "Start":
                m_timeStamp = msg.TimeStamp;
                var mid = MessageID.FromJson(Encoding.Default.GetString(msg.Body));
                m_startHandler(mid.ID,m_timeStamp,m_deltaTime);
                m_id = mid.ID;
                break;
            }
        }
        public void SetOnStartFrame(System.Action<ulong,ulong,ulong> cb){
            m_startHandler += cb;
        }
        public void OnRecvQueues(FlitterClient.FrameQueuesMessage msg){
            Console.WriteLine("FlitterClient.Client Receive Queues: "+msg);
            for (int i=0; i<msg.Queues.Count;i++) {
                var kv = msg.Queues[i];
                var id = kv.Key;
                var frames = kv.Value;
                for (int j=0; j<frames.Length;j++) {
                    var frame = frames[j];
                    OnRecvFrame(false,id,frame);
                }
            }
        }
        public void OnError(string err){
            Console.WriteLine("FlitterClient.Client Err: "+err);
        }
        public void OnConfig(string name,FlitterClient.IClientHandlerGetter getter){

        }
        public void OnStart(FlitterClient.IClientDealer dealer){
            m_dealer = dealer;
            Console.WriteLine("FlitterClient.Client Connected "+dealer.GetEndpoint());
            m_connected = true;
            m_waitQueue = new Queue<FlitterClient.Message>();
        }
        void OnRecvFrame(bool local,ulong id,FlitterClient.FrameMessage msg){
            switch (msg.Head) {
                case "CreateBall":
                m_createHandler(local,id,msg.TimeStamp);
                break;
                case "DeleteBall":
                m_deleteHandler(local,id,msg.TimeStamp);
                break;
                case "MoveBall":
                var vec = Vector.FromJson(Encoding.Default.GetString(msg.Body));
                m_moveHandler(local,id,vec,msg.TimeStamp);
                break;
            }
        }
        public void CreateBall(){
            var msg = new FlitterClient.Message("CreateBall",new byte[]{(byte)'0'});
            Send(msg);
        }
        public void SetOnCreateBall(System.Action<bool,ulong,ulong> cb){
            m_createHandler += cb;
        }
        public void MoveBall(Vector vel){
            var msg = new FlitterClient.Message("MoveBall",Encoding.Default.GetBytes(vel.ToJson()));
            Send(msg);
        }
        public void SetOnMoveBall(System.Action<bool,ulong,Vector,ulong> cb){
            m_moveHandler += cb;
        }
        public void DeleteBall(){
            var msg = new FlitterClient.Message("DeleteBall",new byte[]{(byte)'0'});
            Send(msg);
        }
        public void SetOnDeleteBall(System.Action<bool,ulong,ulong> cb){
            m_deleteHandler += cb;
        }
        public void DispatchFrame(){
            if(m_waitQueue.Count>0){
                var msg = m_waitQueue.Dequeue();
                var frame = new FlitterClient.FrameMessage(msg,m_timeStamp);
                m_dealer.SendFrame(frame);
                OnRecvFrame(true,m_id,frame);
            }
            m_timeStamp+=m_deltaTime;
        }
        void Send(FlitterClient.Message msg){
            m_waitQueue.Enqueue(msg);
        }
        public void OnEnd(FlitterClient.IClientDealer dealer){
            Console.WriteLine("FlitterClient.Client DisConnected "+dealer.GetEndpoint());
        }
    }
	public class ExamDemo:Form{
		float m_speed;
		Timer m_timer;
        FlitterClient.Client m_client;
        Dictionary<ulong,Actor> m_actors;
        Actor m_localActor;
        ExamClientHandler m_handler;
		public static void Main(){
            Application.Run(new ExamDemo());
        }
        public ExamDemo() {
        	Text = "Exam Move Demo";
        	BackColor = Color.Black;
        	Width = 800;
        	Height = 480;
        	KeyPreview = true;
        	m_speed = 200;
            m_actors = new Dictionary<ulong,Actor>();
            m_client = new FlitterClient.Client();
            m_handler = new ExamClientHandler(this);
            m_client.Rejister("Move",m_handler);
            m_handler.SetOnStartFrame((id,timeStamp,delatTime)=>{
                m_localActor = new Actor(id,timeStamp);
                m_timer = new Timer();
                m_timer.Interval = (int)delatTime;
                m_timer.Tick += new EventHandler(OnTimeTick);
                m_timer.Start();
            });
            m_handler.SetOnCreateBall((local,id,timeStamp)=>{
                Random ra = new Random((int)((id+timeStamp)%10000000));
                if (id==m_localActor.id && local) {
                    Console.WriteLine("I Enter "+id+":"+timeStamp);
                    m_localActor.pos.X = ra.Next((int)m_localActor.size.X,(int)(Width-m_localActor.pos.X));
                    m_localActor.pos.Y = ra.Next((int)m_localActor.size.Y,(int)(Height-m_localActor.pos.Y));
                    m_actors[m_localActor.id] = m_localActor;
                } else{
                    if (id!=m_localActor.id) {
                        Console.WriteLine("Some Enter "+id+":"+timeStamp);
                        var actor = new Actor(id,timeStamp);
                        actor.pos.X = ra.Next((int)actor.size.X,(int)(Width-actor.pos.X));
                        actor.pos.Y = ra.Next((int)actor.size.Y,(int)(Height-actor.pos.Y));
                        m_actors[id] = actor;
                    }
                }
            });
            m_handler.SetOnMoveBall((local,id,vel,timeStamp)=>{
                if (id==m_localActor.id && local) {
                    m_actors[m_localActor.id].Move(timeStamp,vel);
                }else{
                    if (id!=m_localActor.id) {
                        m_actors[id].Move(timeStamp,vel);
                    }
                }
            });
            m_client.Start("127.0.0.1", 9090, 10);
        }
        void OnTimeTick(object obj,EventArgs e){
            m_handler.DispatchFrame();
            foreach (var v in m_actors.Values) {
                v.Update(m_timer.Interval);
            }
        	Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e){
        	Graphics g = e.Graphics;
        	g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            foreach (var v in m_actors.Values) {
                v.Draw(g);
            }
        }
        protected override void OnKeyDown(KeyEventArgs e){
            var vel = m_localActor.vel;
        	switch (e.KeyCode) {
        		case Keys.Left:
        			vel.X = -m_speed;
        			break;
        		case Keys.Right:
        			vel.X = m_speed;
        			break;
        		case Keys.Up:
        			vel.Y = -m_speed;
        			break;
        		case Keys.Down:
        			vel.Y = m_speed;
        			break;
        	}
            if (!m_localActor.vel.Equals(vel)) {
                m_handler.MoveBall(vel);
            }
        }
        protected override void OnKeyUp(KeyEventArgs e){
            var vel = m_localActor.vel;
        	switch (e.KeyCode) {
                case Keys.Space:
                    m_handler.CreateBall();
                    return;
        		case Keys.Left:
        			vel.X = 0;
        			break;
        		case Keys.Right:
        			vel.X = 0;
        			break;
        		case Keys.Up:
        			vel.Y = 0;
        			break;
        		case Keys.Down:
        			vel.Y = 0;
        			break;
        	}
            if (!m_localActor.vel.Equals(vel)) {
                m_handler.MoveBall(vel);
            }
        }
	}
}