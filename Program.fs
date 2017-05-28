// Learn more about F# at http://fsharp.org

open System
open System.IO
open Microsoft.AspNetCore.Hosting
open FsharpNancyService

[<EntryPoint>]
let main argv =
    let host = WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls("http://*:5000")
                .Build()

    host.Run()
    0 // return an integer exit code
