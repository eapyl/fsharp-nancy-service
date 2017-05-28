namespace FsharpNancyService

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Nancy.Owin
open Serilog

type Startup() = 
    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment, loggerfactory: ILoggerFactory, appLifetime: IApplicationLifetime) =
        LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .WriteTo.LiterateConsole()
            .WriteTo.RollingFile("logs/log-{Date}.txt")
            .CreateLogger() |> loggerfactory.AddSerilog |> ignore
        Log.CloseAndFlush |> appLifetime.ApplicationStopped.Register |> ignore
        app.UseOwin(fun x ->
            x.UseNancy(fun options ->
                options.Bootstrapper <- new Bootstrapper(app.ApplicationServices)
            ) |> ignore
        ) |> ignore
    member this.Startup(env: IHostingEnvironment) =
        ()