using System.IO;
using System;
using System.Collections.Generic;
namespace FlitterClient {
	public interface Writer {
		void Write(Stream w);
	}
	public interface Reader {
		void Read(Stream r);
	}
	public struct Length:Writer,Reader {
		ulong m_value;
		public Length(ulong value){
			m_value = value;
		}
		public static implicit operator Length(ulong value){
   			return new Length{
   				m_value=value
   			};
		}
		public static implicit operator ulong(Length len){
   			return len.m_value;
		}
		public static implicit operator Length(int value){
   			return new Length{
   				m_value=(ulong)value
   			};
		}
		public static implicit operator int(Length len){
   			return (int)len.m_value;
		}
		public void Write(Stream w){
			ulong tLen = m_value;
			if (tLen == 0) {
				w.WriteByte(0);
				return;
			}
			for( ;tLen > 0; ) {
				ulong temp = tLen >> 7;
				byte now = (byte)(tLen & 127);
				if(temp <= 0) {
					//No More
					w.WriteByte((byte)now);
				} else {
					//Append More
					w.WriteByte((byte)(now | 128));
				}
				tLen = temp;
			}
		}
		public void Read(Stream r){
			ulong tLen = 0;
			int offset = 0;
			for (int i = 0; i < 10; i++) {
				ulong now = (ulong)(r.ReadByte());
				ulong temp = now & 128;
				if (temp <= 0) {
					//No More
					tLen += now << offset;
					break;
				} else {
					//Append More
					tLen += (now & 127) << offset;
				}
				offset += 7;
			}
			m_value = tLen;
		}
		public override bool Equals(object obj){
			if (obj == null)
                return false;
            if (!(obj is Length))
                return false;
            var len = (Length)obj;
            return this.m_value == len.m_value;
        }
        public override int GetHashCode(){
        	return this.m_value.GetHashCode();
    	}
        public bool Equals(Length len){
            return this.m_value == len.m_value;
        }
        public bool Equals(ulong len){
        	return this.m_value == len;
        }
		public override string ToString(){
			return m_value+"";
		}
	}
	public class Message:Writer,Reader {
		public string Head;  
	 	public byte[] Body;
	 	public Message(){
	 	}
	 	public Message(string head,byte[] body){
	 		Head = head;
	 		Body = body;
	 	}
	 	public void Write(Stream w){
	 		if (Body==null || Body.Length<=0) {
	 			throw new System.Exception("Invalid Message");
	 		}
			int _headLen = Head.Length;
			Length headLen = (Length)(ulong)_headLen;
			headLen.Write(w);
			byte[] headBytes = System.Text.Encoding.Default.GetBytes(Head);
			w.Write(headBytes,0,_headLen);
			int _bodyLen = Body.Length;
			Length bodyLen = (Length)(ulong)_bodyLen;
			bodyLen.Write(w);
			w.Write(Body,0,_bodyLen);
		}
		public void Read(Stream r){
			Length headLen = new Length(0);
			headLen.Read(r);
			int _headLen = (int)(ulong)headLen;
			byte[] headBuf = new byte[_headLen];
			r.Read(headBuf,0,_headLen);
			Head = System.Text.Encoding.Default.GetString(headBuf);
			Length bodyLen = new Length(0);
			bodyLen.Read(r);
			int _bodyLen = (int)(ulong)bodyLen;
			byte[] bodyBuf = new byte[_bodyLen];
			r.Read(bodyBuf,0,_bodyLen);
			Body = bodyBuf;
		}
        public bool Equals(Message msg){
        	string mBody = System.Text.Encoding.Default.GetString(Body);
        	string oBody = System.Text.Encoding.Default.GetString(msg.Body);
        	return string.Equals(Head,msg.Head) && string.Equals(mBody,oBody);
        }
		public override string ToString(){
			return string.Concat("Head:", Head," Body:", System.Text.Encoding.Default.GetString(Body));
		}
	}
	public class FrameMessage:Message,Writer,Reader {
	  	public ulong TimeStamp;
	  	public FrameMessage(){
	 	}
	  	public FrameMessage(string head,byte[] body,ulong timeStamp){
	 		Head = head;
	 		Body = body;
	 		TimeStamp = timeStamp;
	 	}
	 	new public void Write(Stream w){
	 		base.Write(w);
	 		Length _timeStamp = (Length)TimeStamp;
	 		_timeStamp.Write(w);
		}
		new public void Read(Stream r){
			base.Read(r);
			Length _timeStamp = new Length(0);
			_timeStamp.Read(r);
			TimeStamp = (ulong)_timeStamp;
		}
        public bool Equals(FrameMessage msg){
        	if (TimeStamp != msg.TimeStamp) {
        		return false;
        	}
        	return base.Equals(msg);
        }
		public override string ToString(){
			return string.Concat(base.ToString(),"TimeStamp:", TimeStamp.ToString());
		}
	}
	public class FrameQueuesMessage:Writer,Reader {
	  	public string Head;
	  	public Dictionary<ulong,FrameMessage[]> Queues;
		int count;
		public FrameQueuesMessage(){
	 	}
		public FrameQueuesMessage(string head){
	 		Head = head;
	 	}
	 	public void AddQueue(ulong id, FrameMessage[] queue) {
			if (Queues == null) {
				Queues = new Dictionary<ulong,FrameMessage[]>();
			}
			count++;
			Queues.Add(id,queue);
		}
		public void RemoveQueue(ulong id) {
			if (Queues == null) {
				return;
			}
			count--;
			Queues.Remove(id);
		}
	 	public void Write(Stream w){
	 		if (Queues == null || count <= 0) {
		 		throw new System.Exception("Invalid Message");
			}
			int _headLen = Head.Length;
			Length headLen = (Length)(ulong)_headLen;
			headLen.Write(w);
			byte[] headBytes = System.Text.Encoding.Default.GetBytes(Head);
			w.Write(headBytes,0,_headLen);
			Length _count_l = (Length)count;
			_count_l.Write(w);
			foreach (var kv in Queues) {
				Length id_l = (Length)kv.Key;
				FrameMessage[] queue = kv.Value;
				id_l.Write(w);
				Length queueCount_l = (Length)queue.Length;
				queueCount_l.Write(w);
				int queueCount = (int)queueCount_l;
				for(int i = 0; i < queueCount; i++){
					queue[i].Write(w);
				}
			}
		}
		public void Read(Stream r){
			Length headLen = new Length(0);
			headLen.Read(r);
			int _headLen = (int)(ulong)headLen;
			byte[] headBuf = new byte[_headLen];
			r.Read(headBuf,0,_headLen);
			Head = System.Text.Encoding.Default.GetString(headBuf);

			Length count_l = (Length)count;
			count_l.Read(r);
			count = (int)count_l;
			Queues = new Dictionary<ulong,FrameMessage[]>();
			for (int i=0; i<count; i++) {
				Length id_l = new Length(0);
				id_l.Read(r);
				ulong id = (ulong)id_l;
				Length queueCount_l = new Length(0);
				queueCount_l.Read(r);
				int queueCount = (int)queueCount_l;
				Queues[id] = new FrameMessage[queueCount];
				for (int j=0; j<queueCount; j++) {
					var msg = new FrameMessage();
					msg.Read(r);
					Queues[id][j] = msg;
				}
			}
		}
        public bool Equals(FrameQueuesMessage queues){
        	if (count != queues.count) {
        		return false;
        	}
        	if (Head != queues.Head) {
        		return false;
        	}
        	foreach (var kv in Queues) {
        		var id = kv.Key;
        		FrameMessage[] queue = kv.Value;
        		FrameMessage[] oQueue;
				if (queues.Queues.TryGetValue(id,out oQueue)) {
					var queueCount = queue.Length;
					var oQueueCount = oQueue.Length;
					if (queueCount == oQueueCount){
						for (int i=0; i<queueCount; i++) {
							if(!queue[i].Equals(oQueue[i])){
								return false;
							}
						}
					}else{
						return false;
					}
				}else{
					return false;
				}
			}
        	return true;
        }
		public override string ToString(){
			string head = string.Concat("Head:", Head,"Count:",count,"Queue:[");
			foreach (var kv in Queues) {
        		var id = kv.Key;
        		head = string.Concat(head,id,":[");
        		FrameMessage[] queue = kv.Value;
				for (int i=0; i<queue.Length; i++) {
					head = string.Concat(head," ",queue[i].ToString());
				}
				head = string.Concat(head,"]");
			}
			head = string.Concat(head,"]");
			return head;
		}
	}
	
	public struct MessageType:Writer,Reader {	
	  	byte m_value;
	  	public const byte Normal = 1;
		public const byte Frame = 2;
		public const byte FrameQueues = 3;
	  	public MessageType (byte value){
	  		m_value = value;
	  	} 
	  	public static implicit operator MessageType(byte value){
   			return new MessageType{
   				m_value=value
   			};
		}
		public static implicit operator byte(MessageType t){
   			return t.m_value;
		}
		public static implicit operator MessageType(int value){
   			return new MessageType{
   				m_value=(byte)value
   			};
		}
		public static implicit operator int(MessageType t){
   			return (int)t.m_value;
		}
	  	public void Write(Stream w){
			w.WriteByte(m_value);
		}
		public void Read(Stream r){
			m_value = (byte)r.ReadByte();
		}
        public bool Equals(MessageType t){
        	return m_value == t.m_value;
        }
        public bool Equals(byte t){
        	return m_value == t;
        }
        public bool Equals(int t){
        	return m_value == (byte)t;
        }
		public override string ToString(){
			return m_value.ToString();
		}
	}
}