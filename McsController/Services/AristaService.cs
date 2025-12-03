using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using McsController.Models;

namespace McsController.Services;

public class AristaService
{
    private readonly HttpClient _httpClient;

    public AristaService()
    {
        // 1. Bypass SSL Validation (Because cEOS uses self-signed certs)
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        _httpClient = new HttpClient(handler);
        
        // 2. Set Basic Auth (admin : admin)
        var authString = Convert.ToBase64String(Encoding.ASCII.GetBytes("admin:admin"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);
    }

    public async Task<string> ExecuteCommandAsync(string switchUrl, string command)
    {
        // 3. Prepare the Arista eAPI Payload
        var requestObject = new JsonRpcRequest
        {
            @params = new AristaParams { cmds = new[] { command } }
        };

        var jsonPayload = JsonSerializer.Serialize(requestObject);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            // 4. Send the POST Request
            // Ensure switchUrl is full path, e.g. "https://localhost:8001/command-api"
            var response = await _httpClient.PostAsync(switchUrl, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            
            // 5. Basic Parsing to check for errors
            var rpcResponse = JsonSerializer.Deserialize<JsonRpcResponse>(responseBody);

            if (rpcResponse?.error.ValueKind != JsonValueKind.Undefined)
            {
                return $"ARISTA ERROR: {rpcResponse?.error.ToString()}";
            }

            // Return the raw JSON result for the first command
            return rpcResponse?.result[0].ToString() ?? "No result";
        }
        catch (Exception ex)
        {
            return $"CONNECTION ERROR: {ex.Message}";
        }
    }

    // public async Task<double> GetInterfaceRateAsync(string switchUrl, string interfaceName)
    // {
    //     // CORRECT COMMAND: Get rates for ALL interfaces
    //     // (Arista doesn't let you filter this specific command by interface in the CLI)
    //     var response = await ExecuteCommandAsync(switchUrl, "show interfaces counters rates");

    //     // 1. Safety Check
    //     if (string.IsNullOrEmpty(response) || !response.Trim().StartsWith("{"))
    //     {
    //         Console.WriteLine($"[Arista API Error] {response}");
    //         return 0;
    //     }

    //     try 
    //     {
    //         using var doc = JsonDocument.Parse(response);
    //         var root = doc.RootElement;
            
    //         // 2. Parse the correct path
    //         // JSON Path: interfaces -> [Ethernet2] -> outBps
    //         if (root.TryGetProperty("interfaces", out var interfaces) && 
    //             interfaces.TryGetProperty(interfaceName, out var iface))
    //         {
    //             // 3. Extract the value
    //             // "outBps" is the field for bits-per-second provided by this specific command
    //             if (iface.TryGetProperty("outBps", out var bpsElement))
    //             {
    //                 double bps = bpsElement.GetDouble();
    //                 return bps / 1_000_000.0; // Convert to Mbps
    //             }
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"[JSON Parse Error] {ex.Message}");
    //     }

    //     return 0;
    //     }
}