namespace PortScanner
{
    using Microsoft.Extensions.Logging;
    using PortScanner.Model;
    using PortScanner.Model.Request;
    using PortScanner.Util;
    using System;
    using System.Collections.Concurrent;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public class Scanner
    {
        private readonly ILogger<Scanner> log;
        private readonly int maxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.25));

        public Scanner(ILogger<Scanner> logger)
        {
            log = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ConcurrentBag<PortScanResult>> ScanIpAddressTcp(IpScanRequest scanRequest)
        {
            log.LogDebug($"TCP: Scanning IP Address: {scanRequest.IPAddress}");

            if (scanRequest is null)
            {
                throw new ArgumentNullException(nameof(scanRequest));
            }

            ConcurrentBag<PortScanResult> results = new ConcurrentBag<PortScanResult>();

            // Assuming 60 % of the CPU is used for scanning where each processor has 2 threads.
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 3
            };

            var scanDate = DateTime.UtcNow;

            using (TcpClient tcpClient = new TcpClient())
            {
                foreach (var port in ScanUtil.GetSamplePorts())
                {
                    try
                    {
                        await tcpClient.ConnectAsync(scanRequest.IPAddress, port);
                        if (tcpClient.Connected)
                        {
                            results.Add(new PortScanResult
                            {
                                IPAddress = scanRequest.IPAddress,
                                IsOpen = true,
                                Port = port,
                                ScanId = scanRequest.ScanId,
                                ScanDate = scanDate,
                                Protocol = "TCP",
                                Site = scanRequest.Site,
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        log.LogError(e, "TcpScan: Could not connect on port {port} for ip address {ipAddress}: Message: {msg}", port, scanRequest.IPAddress, e.Message);
                        results.Add(new PortScanResult
                        {
                            IPAddress = scanRequest.IPAddress,
                            IsOpen = false,
                            Port = port,
                            ScanId = scanRequest.ScanId,
                            ScanDate = scanDate,
                            Protocol = "TCP",
                            Site = scanRequest.Site,
                        });
                    }
                }
            }

            log.LogDebug($"Finished TCP scanning IP Address: {scanRequest.IPAddress}");
            return results;
        }

        public async Task<ConcurrentBag<PortScanResult>> ScanIpAddressUdp(IpScanRequest scanRequest)
        {
            log.LogDebug($"UDP Scanning IP Address: {scanRequest.IPAddress}");

            if (scanRequest is null)
            {
                throw new ArgumentNullException(nameof(scanRequest));
            }

            ConcurrentBag<PortScanResult> results = new ConcurrentBag<PortScanResult>();

            // Assuming 60 % of the CPU is used for scanning where each processor has 2 threads.
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };

            var scanDate = DateTime.UtcNow;

            using (UdpClient udpClient = new UdpClient())
            {
                foreach (var port in ScanUtil.GetSamplePorts())
                {
                    try
                    {
                        await udpClient.SendAsync(new byte[1], 1, scanRequest.IPAddress.ToString(), port);

                        results.Add(new PortScanResult
                        {
                            IPAddress = scanRequest.IPAddress,
                            IsOpen = true,
                            Port = port,
                            ScanId = scanRequest.ScanId,
                            ScanDate = scanDate,
                            Protocol = "UDP",
                            Site = scanRequest.Site,
                        });
                    }
                    catch (Exception e)
                    {
                        log.LogError(e, "UdpScan: Could not connect on port {port} for ip address {ipAddress}: Message: {msg}", port, scanRequest.IPAddress, e.Message);
                        results.Add(new PortScanResult
                        {
                            IPAddress = scanRequest.IPAddress,
                            IsOpen = false,
                            Port = port,
                            ScanId = scanRequest.ScanId,
                            ScanDate = scanDate,
                            Protocol = "UDP",
                            Site = scanRequest.Site
                        });
                    }
                }
            }

            log.LogDebug($"Finished UDP scanning IP Address: {scanRequest.IPAddress}");
            return results;
        }
    }
}

