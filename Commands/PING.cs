namespace FasterResp.Commands
{
    public class PING : IRedisCommand
    {
        private RespFormatter _format;
        public PING(RespFormatter format){
            _format = format;
        }
        public byte[] Exec(RespObject input) {
            var content = "PONG";
            if(input.Type == RespType.Array)
            {
                RespObject[] value = input.Value as RespObject[];
                if(value != null)
                {
                    
                }
            }
        }
    }
}