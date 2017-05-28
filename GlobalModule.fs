namespace FsharpNancyService

open Nancy
open Microsoft.Extensions.Logging

type GlobalModule(loggerFactory: ILoggerFactory) as self = 
    inherit NancyModule()
    let logger = loggerFactory.CreateLogger<GlobalModule>()
    do
      self.Get("/", fun _ ->
        "Hi")