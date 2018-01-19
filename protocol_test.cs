using NUnit.Framework;
using System;
using System.IO;
namespace FlitterClient {
	[TestFixture]
	public class LengthTest{
		Length len;
		ulong lon;
		[TestFixtureSetUp]
		public void Init(){
			Console.WriteLine("Start Test Length!");
			len = new Length(1234567);
			lon = 1234567;
		}
		[Test]
		public void TestConvertToLength(){
			Length alen = (Length)lon;
			Length bLen = new Length(1234567);
			Assert.AreEqual(alen, bLen);
			Assert.Inconclusive("Test ConvertToLength");
		}
		[Test]
		public void TestConvertToUlong(){
			ulong alen = (ulong)len;
			ulong bLen = 1234567;
			Assert.AreEqual(alen, bLen);
			Assert.Inconclusive("Test ConvertToUlong");
		}
		[Test]
		public void TestReadWrite(){
			MemoryStream ms = new MemoryStream();
			Length _len = new Length(0);
			Length aLen = new Length(156);
			Length _aLen = new Length(0);
			try {
				len.Write(ms);
				aLen.Write(ms);
				ms.Position = 0;
				_len.Read(ms);
				_aLen.Read(ms);
			} catch (System.Exception e) {
				Console.WriteLine("Exception");
				Console.WriteLine(e);
			}
			Assert.AreEqual(len, _len);
			Assert.AreEqual(aLen, _aLen);
			Assert.Inconclusive("Test ReadWrite");
		}
		[TestFixtureTearDown]
		public void Dispose(){
			Console.WriteLine("End Test Length!");
　　　　　　	}
	}
	[TestFixture]
	public class MessageTest{
		Message msg;
		[TestFixtureSetUp]
		public void Init(){
			Console.WriteLine("Start Test Message!");
			msg = new Message("Say",System.Text.Encoding.Default.GetBytes("Hello"));
		}
		[Test]
		public void TestReadWrite(){
			MemoryStream ms = new MemoryStream();
			Message _msg = new Message();
			Message aMsg = new Message("Spell",System.Text.Encoding.Default.GetBytes("Fuck"));
			Message _aMsg = new Message();
			try {
				msg.Write(ms);
				aMsg.Write(ms);
				ms.Position = 0;
				_msg.Read(ms);
				_aMsg.Read(ms);
			} catch (System.Exception e) {
				Console.WriteLine("Exception");
				Console.WriteLine(e);
			}
			Assert.AreEqual(msg.ToString(), _msg.ToString());
			Assert.AreEqual(aMsg.ToString(), _aMsg.ToString());
			Assert.Inconclusive("Test ReadWrite");
		}
		[TestFixtureTearDown]
		public void Dispose(){
			Console.WriteLine("End Test Message！");
　　　　　　	}
	}
	[TestFixture]
	public class FrameMessageTest{
		FrameMessage msg;
		[TestFixtureSetUp]
		public void Init(){
			DateTime now = DateTime.Now;
			Console.WriteLine("Start Test FrameMessage!");
			msg = new FrameMessage("Say",System.Text.Encoding.Default.GetBytes("Hello"),(ulong)now.Ticks);
		}
		[Test]
		public void TestReadWrite(){
			MemoryStream ms = new MemoryStream();
			FrameMessage _msg = new FrameMessage();
			DateTime now = DateTime.Now;
			FrameMessage aMsg = new FrameMessage("Spell",System.Text.Encoding.Default.GetBytes("Fuck"),(ulong)now.Ticks);
			FrameMessage _aMsg = new FrameMessage();
			try {
				msg.Write(ms);
				aMsg.Write(ms);
				ms.Position = 0;
				_msg.Read(ms);
				_aMsg.Read(ms);
			} catch (System.Exception e) {
				Console.WriteLine("Exception");
				Console.WriteLine(e);
			}
			Assert.AreEqual(msg.ToString(), _msg.ToString());
			Assert.AreEqual(aMsg.ToString(), _aMsg.ToString());
			Assert.Inconclusive("Test ReadWrite");
		}
		[TestFixtureTearDown]
		public void Dispose(){
			Console.WriteLine("End Test FrameMessage");
　　　　　　	}
	}
	[TestFixture]
	public class FrameQueuesMessageTest{
		FrameQueuesMessage queues;
		[TestFixtureSetUp]
		public void Init(){
			DateTime now = DateTime.Now;
			Console.WriteLine("Start Test FrameQueuesMessageTest!");
			queues = new FrameQueuesMessage("HelloQueue");
			queues.AppendQueue(0,new FrameMessage[]{
				new FrameMessage("Start",System.Text.Encoding.Default.GetBytes("Hello 1"),(ulong)now.Ticks),
				new FrameMessage("Hello",System.Text.Encoding.Default.GetBytes("Hello 2"),(ulong)now.Ticks + 10),
				new FrameMessage("Say",System.Text.Encoding.Default.GetBytes("Hello 3"),(ulong)now.Ticks + 20),
				new FrameMessage("Say",System.Text.Encoding.Default.GetBytes("Hello 4"),(ulong)now.Ticks),
				new FrameMessage("End",System.Text.Encoding.Default.GetBytes("Hello 5"),(ulong)now.Ticks)
			});
			queues.AppendQueue(1,new FrameMessage[]{
				new FrameMessage("Start",System.Text.Encoding.Default.GetBytes("Hello 11"),(ulong)now.Ticks),
				new FrameMessage("Hello",System.Text.Encoding.Default.GetBytes("Hello 21"),(ulong)now.Ticks + 10),
				new FrameMessage("Say",System.Text.Encoding.Default.GetBytes("Hello 31"),(ulong)now.Ticks + 20),
				new FrameMessage("Say",System.Text.Encoding.Default.GetBytes("Hello 41"),(ulong)now.Ticks),
				new FrameMessage("End",System.Text.Encoding.Default.GetBytes("Hello 51"),(ulong)now.Ticks)
			});
		}
		[Test]
		public void TestReadWrite(){
			MemoryStream ms = new MemoryStream();
			var oQueues = new FrameQueuesMessage();
			try {
				queues.Write(ms);
				ms.Position = 0;
				oQueues.Read(ms);
			} catch (System.Exception e) {
				Console.WriteLine("Exception");
				Console.WriteLine(e);
			}
			Assert.AreEqual(oQueues.ToString(), queues.ToString());
			Assert.Inconclusive("Test ReadWrite");
		}
		[TestFixtureTearDown]
		public void Dispose(){
			Console.WriteLine("End Test FrameQueuesMessageTest");
　　　　　　	}
	}
	[TestFixture]
	public class MessageTypeTest{
		MessageType t;
		byte t_b;
		[TestFixtureSetUp]
		public void Init(){
			Console.WriteLine("Start Test MessageType!");
			t = new MessageType(3);
			t_b = 2;
		}
		[Test]
		public void TestConvertToMessageType(){
			MessageType aT = (MessageType)t_b;
			MessageType bT = new MessageType(t_b);
			Assert.AreEqual(aT, bT);
			Assert.Inconclusive("Test ConvertToMessageType");
		}
		[Test]
		public void TestConvertToByte(){
			byte aT = (byte)t;
			byte bT = 3;
			Assert.AreEqual(aT, bT);
			Assert.Inconclusive("Test ConvertToByte");
		}
		[Test]
		public void TestReadWrite(){
			MemoryStream ms = new MemoryStream();
			MessageType t1 = t;
			MessageType t2 = t_b;
			MessageType t1_r = new MessageType(0);
			MessageType t2_r = new MessageType(0);
			try {
				t1.Write(ms);
				t2.Write(ms);
				ms.Position = 0;
				t1_r.Read(ms);
				t2_r.Read(ms);
			} catch (System.Exception e) {
				Console.WriteLine("Exception");
				Console.WriteLine(e);
			}
			Assert.AreEqual(t1, t1_r);
			Assert.AreEqual(t2, t2_r);
			Assert.AreEqual(MessageType.Frame, t2_r);
			Assert.Inconclusive("Test ReadWrite");
		}
		[TestFixtureTearDown]
		public void Dispose(){
			Console.WriteLine("End Test MessageType!");
　　　　　　	}
	}
}