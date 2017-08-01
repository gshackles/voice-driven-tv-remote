open FSharp.Data

let Run(timerTrigger: TimerInfo, log: TraceWriter, lineupsBlob: byref<string>) =
    let lineupUrl = sprintf "https://www.xmltvlistings.com/xmltv/get/%s/%s/2/0" 
                            (Environment.GetEnvironmentVariable("XmlTvApiKey"))
                            (Environment.GetEnvironmentVariable("XmlTvLineupId"))
    let xml = Http.RequestString(lineupUrl, responseEncodingOverride = "utf-8")
    
    lineupsBlob <- xml
    
    sprintf "Successfully downloaded XML, length: %i" xml.Length |> log.Info