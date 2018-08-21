namespace FasterResp.Commands
{
    public interface IRedisCommand
    {
        byte[] Exec(RespObject input);
    }
}