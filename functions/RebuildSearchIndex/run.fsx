open System
open FSharp.Data
open FSharp.Data.HttpRequestHeaders

type SearchRequest = JsonProvider<""" {"filter": "ChannelId eq 125", "top": 100, "search": "seinfeld"} """>
type SearchResults = JsonProvider<""" {"value": [{ "ShowId": "abc" }]} """>
type DeleteRequest = JsonProvider<""" {"value": [{ "@search.action": "delete", "ShowId": "abc" }]} """>

let postJson urlPath json =
    let apiKeyHeader = "api-key", (Environment.GetEnvironmentVariable("SearchApiKey"))
    let url = sprintf "%s/%s?api-version=2015-02-28" (Environment.GetEnvironmentVariable("SearchUrlBase")) urlPath
    
    Http.RequestString(url,
                       body = TextRequest json,
                       headers = [ ContentType HttpContentTypes.Json; apiKeyHeader ])

let search query filter count =
    SearchRequest.Root(filter, count, query).JsonValue.ToString()
    |> postJson "indexes('shows')/docs/search"
    |> SearchResults.Parse
    |> fun results -> results.Value

let rec clearIndex() =
    let results = search "" "" 1000

    if not <| Seq.isEmpty results then
        results 
        |> Array.map (fun result -> DeleteRequest.Value("delete", result.ShowId))
        |> fun ops -> DeleteRequest.Root(ops).JsonValue.ToString()
        |> postJson "indexes('shows')/docs/index"
        |> ignore

        clearIndex()

let rebuildIndex() =
    postJson "indexers/shows-indexer/run" ""
    |> ignore

let Run(inputMessage: string, log: TraceWriter) =
    sprintf "Starting rebuild request for: %s" inputMessage |> log.Info

    clearIndex()
    rebuildIndex()

    sprintf "Finished rebuild request for: %s" inputMessage |> log.Info