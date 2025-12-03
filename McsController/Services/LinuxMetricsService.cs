using System.Diagnostics;

namespace McsController.Services;

public class LinuxMetricsService
{
    public async Task<double> GetContainerTxMbps(string containerName)
    {
        try
        {
            // 1. Measure Bytes at Time A
            long bytes1 = await ReadTxBytes(containerName);
            
            // 2. Wait 1 Second
            await Task.Delay(1000); 
            
            // 3. Measure Bytes at Time B
            long bytes2 = await ReadTxBytes(containerName);

            // 4. Calculate Speed (Mbps)
            if (bytes1 < 0 || bytes2 < 0) return 0;
            long diff = bytes2 - bytes1;
            
            // (Bytes * 8 bits) / 1,000,000 = Mbps
            return (diff * 8.0) / 1_000_000.0;
        }
        catch 
        {
            return 0; 
        }
    }

    private async Task<long> ReadTxBytes(string containerName)
    {
        var startInfo = new ProcessStartInfo
        {
            // CHANGE 1: Use the OrbStack CLI tool
            FileName = "orb", 
            
            // CHANGE 2: Use the '-m' flag to target the 'netlab' machine
            // Syntax: orb -m <machine> <command>
            Arguments = $"-m netlab docker exec {containerName} cat /sys/class/net/eth1/statistics/tx_bytes",
            
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try 
        {
            using var process = Process.Start(startInfo);
            if (process == null) return 0;
            
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (long.TryParse(output.Trim(), out long bytes))
            {
                return bytes;
            }
        }
        catch
        {
            // Fail silently (e.g., if 'orb' isn't in the PATH)
        }
        return -1;
    }
}