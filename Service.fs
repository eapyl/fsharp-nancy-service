namespace FsharpNancyService

open System
open System.Collections.Concurrent
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open Daemon

module Service =
    let start (logger:ILogger) (items:Item[]) =
        let version = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion
        logger.LogInformation("Staring service {version}", version)
        let itemCount = Array.length items
        logger.LogInformation("Item count is {Length}", itemCount)
        let proceedItem item =
            async {
                logger.LogTrace("ExecuteForItem {ip}", item.id)
            }
        let jobs = items |> Array.map (fun item ->
            {
                action = proceedItem item;
                cron = item.cron
            })
        let daemon = run jobs
        logger.LogInformation("Started service")
        daemon
