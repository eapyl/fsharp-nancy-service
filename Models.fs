namespace FsharpNancyService

type Item = 
    {
        id: string;
        cron: string
    }

type Job = { action: Async<unit>; cron: string }

exception TooMuchArgumentsException of int