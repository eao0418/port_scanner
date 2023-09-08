namespace PortScanner.Triggers
{
    using Azure.Data.Tables;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using NetTools;
    using Newtonsoft.Json;
    using PortScanner.Model;
    using PortScanner.Model.TableEntity;
    using PortScanner.Provider;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class HttpTriggers
    {
        private readonly ILogger<HttpTriggers> log;
        private readonly ITableClientFactory tableFactory;
        private const string ScanIpTableName = "ScanIps";

        public HttpTriggers(ILogger<HttpTriggers> logger, ITableClientFactory tableClientFactory)
        {
            log = logger ?? throw new ArgumentNullException(nameof(logger));
            tableFactory = tableClientFactory ?? throw new ArgumentNullException(nameof(tableClientFactory));
        }

        /// <summary>
        /// Registers an IP address or range of IP addresses to be scanned.
        /// </summary>
        /// <param name="req">The <see cref="HttpRequest"/>.</param>
        /// <param name="context">The <see cref="ExecutionContext"/>.</param>
        /// <returns>A <see cref="Task{IActionResult}"/>.</returns>
        [FunctionName(nameof(RegisterIpAddress))]
        public async Task<IActionResult> RegisterIpAddress(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "scan/registerip")] HttpRequest req,
            ExecutionContext context)
        {
            var invocation = context.InvocationId.ToString();
            log.LogInformation("C# HTTP trigger function executed at: {time}. Invocation: {invocation}", DateTime.Now, invocation);

            if (req.Body is null)
            {
                log.LogError("Request body is null for invocation {invocation}", invocation);
                return new BadRequestObjectResult("A requestbody is required.");
            }

            string requestBody = string.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            RegisterIpAddressRequest data;

            try
            {
                data = JsonConvert.DeserializeObject<RegisterIpAddressRequest>(requestBody);
            }
            catch (Exception ex)
            {
                log.LogError("Unable to deserialize request body for invocation {invocation}. Body: {body}. Error message: {msg}", invocation, requestBody, ex.Message, ex);
                return new BadRequestObjectResult($"Unable to deserialize request body: {ex.Message}");
            }

            IPAddressRange ipRange;

            try
            {
                ipRange = IPAddressRange.Parse($"{data.StartIpAddress}/{data.CidrRange}");
            }
            catch (Exception rex)
            {
                log.LogError("Unable to parse IP range for invocation {invocation}. Error message: {msg}", invocation, rex.Message, rex);
                return new BadRequestObjectResult($"Unable to parse IP range with the startIp {data.StartIpAddress} and range {data.CidrRange}: {rex.Message}");
            }

            TableClient tableClient = await tableFactory.GetTableClient(ScanIpTableName);

            foreach (var ip in ipRange)
            {
                var scanIp = ScanIpEntity.Create(data.Site, ip);
                try
                {
                    await tableClient.AddEntityAsync(scanIp.ToTableEntity());
                }
                catch (Exception e)
                {
                    log.LogError("Unable to add entity to table storage", e);
                    return new InternalServerErrorResult();
                }

            }

            return new CreatedResult(string.Empty, ipRange);
        }

        /// <summary>
        /// Gets the entry for the requested IP address.
        /// </summary>
        /// <param name="ip">The <see cref="IPAddress"/> to get.</param>
        /// <param name="context">The <see cref="ExecutionContext"/>.</param>
        /// <returns>A <see cref="Task{IActionResult}"/>.</returns>
        [FunctionName(nameof(GetScanAddress))]
        public async Task<IActionResult> GetScanAddress(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "scan/config/{ip}")] HttpRequest req,
            string ip,
            ExecutionContext context)
        {
            var invocation = context.InvocationId.ToString();
            log.LogInformation(
                "{trigger} HTTP trigger function executed at: {time}. Invocation: {invocation}",
                nameof(GetScanAddress),
                DateTime.Now,
                invocation);

            if (ip is null || !IPAddress.TryParse(ip, out _))
            {
                log.LogError("IP address is null for invocation {invocation}", invocation);
                return new BadRequestObjectResult("IP address is required in a valid format.");
            }

            TableClient tableClient = await tableFactory.GetTableClient(ScanIpTableName);
            var output = new List<ScanIpEntity>(10);

            var queryResults = tableClient.QueryAsync<TableEntity>(x => x.RowKey == ip);

            if (queryResults is not null)
            {
                await foreach (var result in queryResults)
                {
                    output.Add(ScanIpEntity.FromTableEntity(result));
                }
            }

            if (queryResults is null || output.Count == 0)
            {
                return new NotFoundObjectResult(new KeyValuePair<string, string>("ipAddress", ip));
            }
            log.LogInformation("Found {count} results for IP address {ip}", output.Count, ip);

            return new OkObjectResult(output);
        }

        /// <summary>
        /// Gets the list of sites that can be used for scanning.
        /// </summary>
        /// <param name="context">The <see cref="ExecutionContext"/>.</param>
        /// <returns>A <see cref="IActionResult"/>.</returns>
        [FunctionName(nameof(GetScanSites))]
        public IActionResult GetScanSites(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "scan/sites")] HttpRequest req,
            ExecutionContext context)
        {
            var invocation = context.InvocationId.ToString();
            log.LogInformation(
                "{trigger} HTTP trigger function executed at: {time}. Invocation: {invocation}",
                nameof(GetScanSites),
                DateTime.Now,
                invocation);

            var output = Enum.GetNames<Site>();

            return new OkObjectResult(output);
        }
    }
}
