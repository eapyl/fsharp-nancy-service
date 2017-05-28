namespace FsharpNancyService

open System
open System.Threading
open System.Text.RegularExpressions

module Schedule =
    type System.String with 
        static member split c (value: string) =
            value.Split c

    [<Literal>]
    let DividePattern = @"(\*/\d+)"
    [<Literal>]
    let RangePattern = @"(\d+\-\d+(/\d+)?)"
    [<Literal>]
    let WildPattern = @"(\*)"
    [<Literal>]
    let OneValuePattern = @"(\d)"
    [<Literal>]
    let ListPattern = @"((\d+,)+\d+)"

    type ISchedueSet = 
        { 
            Minutes: int list;
            Hours: int list;
            DayOfMonth: int list;
            Months: int list;
            DayOfWeek: int list
        }
    let generate expression =
        let dividedArray (m:string) start max =
            let divisor = m |>  String.split [|'/'|] |> Array.skip 1 |> Array.head |> Int32.Parse
            [start .. max] |> List.filter (fun x -> x % divisor = 0)
        let rangeArray (m:string) =
            let split = m |> String.split [|'-'; '/'|] |> Array.map Int32.Parse
            match Array.length split with
                | 2 -> [split.[0] .. split.[1]]
                | 3 -> [split.[0] .. split.[1]] |> List.filter (fun x -> x % split.[2] = 0)
                | _ -> []
        let wildArray (m:string) start max =
            [start .. max]
        let oneValue (m:string) =
            [m |> Int32.Parse]
        let listArray (m:string) =
            m |> String.split [|','|] |> Array.map Int32.Parse |> Array.toList

        let getStartAndMax i =
            match i with
            | 0 -> (0, 59)
            | 1 -> (0, 23)
            | 2 -> (1, 31)
            | 3 -> (1, 12)
            | 4 -> (0, 6)
            | _ -> raise (TooMuchArgumentsException i)
        
        let (|MatchRegex|_|) pattern input =
            let m = Regex.Match(input, pattern)
            if m.Success then Some (m.ToString()) else None

        let parts =
            expression 
            |> String.split [|' '|]
            |> Array.mapi (fun i x ->
                let (start, max) = getStartAndMax i 
                match x with
                    | MatchRegex DividePattern x -> dividedArray x start max
                    | MatchRegex RangePattern x -> rangeArray x
                    | MatchRegex WildPattern x -> wildArray x start max
                    | MatchRegex ListPattern x -> listArray x
                    | MatchRegex OneValuePattern x -> oneValue x
                    | _ -> []
            )
        { 
            Minutes = parts.[0];
            Hours = parts.[1];
            DayOfMonth = parts.[2];
            Months = parts.[3];
            DayOfWeek = parts.[4]
        }
    let isTime (dateTime : DateTime) schedueSet  =
        List.exists ((=) dateTime.Minute) schedueSet.Minutes && 
        List.exists ((=) dateTime.Hour) schedueSet.Hours &&
        List.exists ((=) dateTime.Day) schedueSet.DayOfMonth &&
        List.exists ((=) dateTime.Month) schedueSet.Months &&
        List.exists ((=) (int dateTime.DayOfWeek)) schedueSet.DayOfWeek

module Daemon =
    [<Literal>]
    let INTERVAL = 30000

    let internalRun interval (now: unit->DateTime) (jobs: seq<Job>) =
        let createDisposable f =
            {
                new IDisposable with
                    member x.Dispose() = f()
            }

        let timerElapsed obj =
            let checkJob = () |> now |> Schedule.isTime
            jobs 
            |> Seq.map (fun x ->
                let schedule = Schedule.generate x.cron
                (schedule, x.action)
            ) 
            |> Seq.filter (fun (x, y) -> checkJob x)
            |> Seq.map (fun (x, y) -> y) 
            |> Async.Parallel 
            |> Async.RunSynchronously
            |> ignore

        let localTimer = new Timer(timerElapsed, null, Timeout.Infinite, interval);
        localTimer.Change(0, interval) |> ignore
        createDisposable (fun() -> localTimer.Dispose())
    let now = fun () -> DateTime.UtcNow
    let run jobs = internalRun INTERVAL now jobs