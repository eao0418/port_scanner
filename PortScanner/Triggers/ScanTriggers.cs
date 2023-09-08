namespace PortScanner.Triggers
{
    using Azure.Data.Tables;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Extensions.Logging;
    using PortScanner.Model;
    using PortScanner.Provider;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs.ServiceBus;
    using PortScanner.Model.Request;
    using System.Collections.Concurrent;

    public class ScanTriggers
    {
        private const string ScanIpTableName = "ScanIps";
        private const string RequestScanQueue = "requestscan";
        private const string RequestUdpScanQueue = "requestudpscan";
        private const string EventHubName = "scanresults";
        private readonly ILogger<ScanTriggers> log;
        private readonly ITableClientFactory tableFactory;
        private readonly Scanner scanner;

        public ScanTriggers(ILogger<ScanTriggers> logger, ITableClientFactory tableClientFactory, Scanner scan)
        {
            log = logger ?? throw new ArgumentNullException(nameof(logger));
            tableFactory = tableClientFactory ?? throw new ArgumentNullException(nameof(tableClientFactory));
            scanner = scan ?? throw new ArgumentNullException(nameof(scanner));
        }

        [FunctionName(nameof(TriggerScans))]
        public async Task TriggerScans(
            [TimerTrigger("0 0 */12 * * *")] TimerInfo myTimer,
            [DurableClient] IDurableClient orchestrationClient,
            ExecutionContext context)
        {
            var invocation = context.InvocationId.ToString();
            log.LogInformation("C# Timer trigger function executed at: {time}. Invocation: {invocation}, Past Due: {stat}", DateTime.Now, invocation, myTimer.IsPastDue);
            _ = await orchestrationClient.StartNewAsync(nameof(OrchestrateSiteScans), invocation);
            return;
        }

        [FunctionName(nameof(OrchestrateSiteScans))]
        public async Task OrchestrateSiteScans(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var tasks = new List<Task>();

            var sites = Enum.GetNames<Site>();
            int siteCount = sites.Length;
            for (int i = 1; i < siteCount; i++)
            {
                var site = Enum.Parse<Site>(sites[i]);
                var request = new SiteScanRequest
                {
                    ScanId = context.InstanceId,
                    Site = site,
                };

                tasks.Add(context.CallActivityAsync(nameof(StartScanForSiteAsync), request));
            }

            await Task.WhenAll(tasks);
        }

        [FunctionName(nameof(StartScanForSiteAsync))]
        public async Task StartScanForSiteAsync(
            [ActivityTrigger] SiteScanRequest @request,
            [ServiceBus(RequestScanQueue, EntityType = ServiceBusEntityType.Queue, Connection = "ServiceBusConnection")] IAsyncCollector<IpScanRequest> output,
            [ServiceBus(RequestUdpScanQueue, EntityType = ServiceBusEntityType.Queue, Connection = "ServiceBusConnection")] IAsyncCollector<IpScanRequest> udpqueue)
        {
            TableClient tableClient = await tableFactory.GetTableClient(ScanIpTableName);

            var queryResults = tableClient.QueryAsync<TableEntity>(x => x.PartitionKey == request.Site.ToString());

            // exit early if there is no work to do.
            if (queryResults is null)
            {
                log.LogInformation("No IP addresses found for site {site}", request.Site);
                return;
            }

            await foreach (var result in queryResults)
            {
                var scanRequest = new IpScanRequest
                {
                    IPAddress = IPAddress.Parse(result.RowKey),
                    ScanId = request.ScanId,
                    Site = request.Site,
                };
                await output.AddAsync(scanRequest);
                await udpqueue.AddAsync(scanRequest);
            }

            return;
        }

        [FunctionName(nameof(TcpScanIpAsync))]
        public async Task TcpScanIpAsync(
            [ServiceBusTrigger(RequestScanQueue, Connection = "ServiceBusConnection")] IpScanRequest request,
            [EventHub(EventHubName, Connection = "EventHubConnection")] IAsyncCollector<PortScanResult> eventHubOutput)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            log.LogInformation("Started {trigger} with scanId: {invocation} for IP {ipAddress}", nameof(TcpScanIpAsync), request.ScanId, request.IPAddress);
            ConcurrentBag<PortScanResult> results = null;

            try
            {
                results = await scanner.ScanIpAddressTcp(request);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error scanning IP {ipAddress}. Message is : {ex}", request.IPAddress, ex.Message);
                return;
            }

            foreach (var item in results)
            {
                await eventHubOutput.AddAsync(item);
            }
            return;
        }

        [FunctionName(nameof(UdpScanIpAsync))]
        public async Task UdpScanIpAsync(
        [ServiceBusTrigger(RequestUdpScanQueue, Connection = "ServiceBusConnection")] IpScanRequest request,
        [EventHub(EventHubName, Connection = "EventHubConnection")] IAsyncCollector<PortScanResult> eventHubOutput)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            log.LogInformation("Started {trigger} with scanId: {invocation} for IP {ipAddress}", nameof(UdpScanIpAsync), request.ScanId, request.IPAddress);
            ConcurrentBag<PortScanResult> results = null;

            try
            {
                results = await scanner.ScanIpAddressUdp(request);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error scanning IP {ipAddress}. Message is : {ex}", request.IPAddress, ex.StackTrace);
                return;
            }

            foreach (var item in results)
            {
                await eventHubOutput.AddAsync(item);
            }
            return;
        }
    }
}
