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
			Console.WriteLine("Length 测试开始！");
			len = new Length(1234567);
			lon = 1234567;
			Console.WriteLine("len对象被初始化！");
			Console.WriteLine("lon对象被初始化！");
		}
		[Test]
		public void TestConvertToLength(){
			Length alen = (Length)lon;
			Length bLen = new Length(1234567);
			Assert.AreEqual(alen, bLen);
			Assert.Inconclusive("验证此测试方法的正确性");
		}
		[Test]
		public void TestConvertToUlong(){
			ulong alen = (ulong)len;
			ulong bLen = 1234567;
			Assert.AreEqual(alen, bLen);
			Assert.Inconclusive("验证此测试方法的正确性");
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
			Assert.Inconclusive("验证此测试方法的正确性");
		}
		[TestFixtureTearDown]
		public void Dispose(){
			Console.WriteLine("Length 测试结束！");
　　　　　　	}
	}
	[TestFixture]
	public class MessageTest{
		Message msg;
		[TestFixtureSetUp]
		public void Init(){
			Console.WriteLine("Message 测试开始！");
			msg = new Message("Say",System.Text.Encoding.Default.GetBytes("Hello"));
			Console.WriteLine("Message 对象被初始化！");
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
			Assert.Inconclusive("验证此测试方法的正确性");
		}
		[TestFixtureTearDown]
		public void Dispose(){
			Console.WriteLine("Message 测试结束！");
　　　　　　	}
	}
}