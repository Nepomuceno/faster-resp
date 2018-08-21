using System.Collections.Generic;
using System.Text;

namespace FasterResp.Commands
{

    public class RespFormatter
    {
        public byte[] GetResponse(string content)
        {
            if (content.Length < 100)
            {
                return Encoding.UTF8.GetBytes($"+{content}\r\n");
            }
            else
            {
                return Encoding.UTF8.GetBytes($"${content.Length}\r\n{content}\r\n");
            }
        }
    }
}