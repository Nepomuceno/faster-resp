using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FasterResp
{
    public class RespHandler : ConnectionHandler
    {
        private ILogger _logger;
        public RespHandler(ILogger<RespHandler> logger)
        {
            _logger = logger;
            _logger.LogInformation("Listener started");

        }
        public async override Task OnConnectedAsync(ConnectionContext connection)
        {
            while (true)
            {
                var result = await connection.Transport.Input.ReadAsync();
                var buffer = result.Buffer;
                byte[] corrent = Encoding.ASCII.GetBytes("+OK\r\n");
                try
                {
                    if (!buffer.IsEmpty)
                    {
                        using (StreamReader sr = new StreamReader(new MemoryStream(buffer.ToArray())))
                        {
                            while (!sr.EndOfStream)
                            {
                                RespObject content = GetContent(sr);
                                corrent = GetResponse(content);
                                _logger.LogDebug(JsonConvert.SerializeObject(content, Formatting.Indented));
                            }

                        }
                        await connection.Transport.Output.WriteAsync(corrent);
                    }
                    else if (result.IsCompleted)
                    {
                        break;
                    }
                }
                finally
                {
                    connection.Transport.Input.AdvanceTo(result.Buffer.End);
                }
            }
        }

        private byte[] GetResponse(RespObject content)
        {
            if(content.Type == )
        }

        private RespObject GetContent(StreamReader sr)
        {
            var type = sr.ReadLine();
            if (type.Length < 1) return null;
            switch (type[0])
            {
                case '+':
                    return new RespObject()
                    {
                        Type = RespType.Str,
                        Value = type.Substring(1)
                    };
                case '*':
                    var length = int.Parse(type.Substring(1));
                    RespObject[] content = new RespObject[length];
                    for (int i = 0; i < length; i++)
                    {
                        content[i] = GetContent(sr);
                    }
                    return new RespObject()
                    {
                        Type = RespType.Array,
                        Value = content
                    };
                case ':':
                    var number = int.Parse(type.Substring(1));
                    return new RespObject()
                    {
                        Type = RespType.Int,
                        Value = number
                    };
                case '-':
                    var error = type.Substring(1);
                    return new RespObject()
                    {
                        Type = RespType.Err,
                        Value = error
                    };
                case '$':
                    string bulk;
                    if (type[1] == '-')
                        bulk = null;
                    else
                        bulk = sr.ReadLine();
                    return new RespObject()
                    {
                        Type = RespType.Bulk,
                        Value = bulk
                    };
                default:
                    throw new Exception("Invalid value");
            }
        }

    }
}