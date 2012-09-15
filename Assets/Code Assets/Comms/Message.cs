using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com {
    /// <summary>
    /// Stores buffer information for data sent from the Scala Agent server.
    /// </summary>
    public class Message {

        public const int BufferSize = 256;
        public byte[] Buffer { get; set; }
        public StringBuilder Builder { get; set; }

        public Message() {
            this.Buffer = new byte[Message.BufferSize];
            this.Builder = new StringBuilder();
        }
    }
}