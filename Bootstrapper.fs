namespace FsharpNancyService

open System
open Microsoft.Extensions.Logging
open Nancy
open Nancy.Bootstrapper
open Nancy.Configuration
open Nancy.Diagnostics
open Nancy.TinyIoc

type Bootstrapper(serviceProvider: IServiceProvider) = 
    inherit DefaultNancyBootstrapper()
    let mutable daemon : IDisposable = null
    override this.ConfigureApplicationContainer(container: TinyIoCContainer) =
        base.ConfigureApplicationContainer (container)
        serviceProvider.GetService (typeof<ILoggerFactory>)
            :?> ILoggerFactory
            |> container.Register<ILoggerFactory>
            |> ignore
    override this.ApplicationStartup(container: TinyIoCContainer, pipelines: IPipelines) =
        let loggerFactory = container.Resolve<ILoggerFactory>()
        let logger = loggerFactory.CreateLogger()
        let items = [|
            {
                id = "1"
                cron = "* * * * *"
            };
            {
                id = "2"
                cron = "* * * * *"
            }|]
        daemon <- Service.start logger items
