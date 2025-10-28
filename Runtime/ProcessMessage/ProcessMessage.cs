using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImxCoreSockets
{
    public class ProcessMessage
    {
        public enum MessageType { ProcessVariable, ProcessMethod }

        public MessageType Type;
        public string ID; // The VariableID or FunctionID from our attributes
        public byte[] Payload; // The serialized data 
        
        public byte[] Serialize()
        {            
            using (var stream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(stream))
            {
                writer.Write((int)Type);
                writer.Write(ID);
                writer.Write(Payload.Length);
                writer.Write(Payload);
                return stream.ToArray();
            }
        }
       
        public static ProcessMessage Deserialize(byte[] data)
        {
            using (var stream = new System.IO.MemoryStream(data))
            using (var reader = new System.IO.BinaryReader(stream))
            {
                var message = new ProcessMessage
                {
                    Type = (MessageType)reader.ReadInt32(),
                    ID = reader.ReadString(),
                    Payload = reader.ReadBytes(reader.ReadInt32())
                };
                return message;
            }
        }
    }
}
