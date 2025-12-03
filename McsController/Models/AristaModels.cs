using System.Text.Json;

namespace McsController.Models;

public class JsonRpcRequest
{
    public string jsonrpc { get; set; } = "2.0";
    public string method { get; set; } = "runCmds";
    public object @params { get; set; }
    public string id { get; set; } = Guid.NewGuid().ToString();
}

public class AristaParams
{
    public string format { get; set; } = "json";
    public int version { get; set; } = 1;
    public string[] cmds { get; set; }
}

public class JsonRpcResponse
{
    public JsonElement[] result { get; set; }
    public JsonElement error { get; set; }
}