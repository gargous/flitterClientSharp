using System;
using System.Windows.Forms;
using System.Drawing;
using System.Json;
namespace ExamMove {
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
	public class ExamDemo:Form{
		Actor m_actor;
		float m_speed;
		Timer m_timer; 
		public static void Main(){
            Application.Run(new ExamDemo());
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
        	m_actor.Update();
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
        }
	}
}