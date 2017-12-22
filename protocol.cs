using System.IO;
using System;
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
		public void Write(Stream w){
			ulong tLen = m_value;
			byte[] buf = new byte[8];
			int n = 0;
			 
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
			for (int i = 0; i < 8; i++) {
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
	public struct Message {
		public string Head;  
	 	public byte[] Body;
	 	public Message(string head,byte[] body){
	 		Head = head;
	 		Body = body;
	 	}
	 	public void Write(Stream w){
			w.WriteByte((byte)('\n'));
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
			byte validaterBuf = (byte)r.ReadByte();
			if (validaterBuf != '\n') {
				throw new System.Exception("Invalid Message");
			}
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
}