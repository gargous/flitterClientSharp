using System.IO;
using System;
using System.Collections.Generic;
namespace FlitterClient {
	public struct Length {
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
			byte[] buf = new byte[10];
			int n = 0;
			if (tLen == 0) {
				buf[0] = 0;
				w.Write(buf,0,1);
				return;
			}
			for( ;tLen > 0; n++) {
				ulong temp = tLen >> 7;
				ulong now = tLen & 127;
				if(temp <= 0) {
					//No More
					buf[n] = (byte)now;
				} else {
					//Append More
					buf[n] = (byte)(now | 128);
				}
				tLen = temp;
			}
			w.Write(buf,0,n);
		}
		public void Read(Stream r){
			ulong tLen = 0;
			int offset = 0;
			byte[] buf = new byte[1];
			for (int i = 0; i < 10; i++) {
				r.Read(buf,0,1);
				ulong now = (ulong)(buf[0]);
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
	public class Message {
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
	public class FrameMessage:Message {
	  	public Length TimeStamp;
	  	public FrameMessage(){
	 	}
	  	public FrameMessage(string head,byte[] body,ulong timeStamp){
	 		Head = head;
	 		Body = body;
	 		TimeStamp = timeStamp;
	 	}
	 	new public void Write(Stream w){
	 		base.Write(w);
	 		TimeStamp.Write(w);
		}
		new public void Read(Stream r){
			base.Read(r);
			TimeStamp.Read(r);
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
	public class FrameQueuesMessage {
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
}