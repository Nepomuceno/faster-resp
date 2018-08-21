namespace FasterResp
{
    public class RespObject
    {
        public RespType Type { get; set; }
        public object Value { get; set; }
    }

    public enum RespType
    {
        Err,
        Str,
        Int,
        Bulk,
        Array
    }
}