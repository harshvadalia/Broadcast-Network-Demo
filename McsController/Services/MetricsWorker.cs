using Prometheus;

namespace McsController.Services;

public class MetricsWorker : BackgroundService
{
    private readonly LinuxMetricsService _linuxService;

    // here i define the metrics which will be used by grafana
    private static readonly Gauge MediaFlow = Metrics.CreateGauge(
        "smpte_2110_mbps", 
        "Bandwidth Flow", 
        new GaugeConfiguration { LabelNames = new[] { "direction", "device" } });

    public MetricsWorker(LinuxMetricsService linuxService)
    {
        _linuxService = linuxService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // fetching tx transit from camera1
            double txMbps = await _linuxService.GetContainerTxMbps("clab-mcs-demo-camera1");

            // updating prom
            MediaFlow.WithLabels("TX-Source", "Camera1").Set(txMbps);

            await Task.Delay(1000, stoppingToken);
        }
    }
}