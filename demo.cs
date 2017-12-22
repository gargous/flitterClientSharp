using System;
using System.Net;
using System.Net.Sockets;
namespace HelloWorldApplication{
	public class DemoS{
        static void Main(string[] args){
        	var ip = IPAddress.Parse("127.0.0.1");
        	var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        	var connPort = 9090;
        	clientSocket.Connect(new IPEndPoint(ip, connPort)); //配置服务器IP与端口  
        	Console.WriteLine("连接服务器成功");
            Console.WriteLine("Hello World!");
        }
    }
}