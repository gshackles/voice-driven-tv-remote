module Search

open System
open FSharp.Data
open FSharp.Data.HttpRequestHeaders

type SearchRequest = JsonProvider<""" {"filter": "ChannelId eq 125", "top": 100, "search": "seinfeld"} """>
type ShowSearchResults = JsonProvider<""" {"value": [{"@search.score": 2.5086286, "ShowId": "1046", "Title": "Seinfeld", 
                                                      "StartTime": "20170102200000", "EndTime": "20170102233000", "ChannelId": 125,
                                                      "Description": "Yada yada yada", "Category": "Sitcom" }]} """>
type ChannelSearchResults = JsonProvider<""" {"value": [{"@search.score": 1, "ChannelId": "150", "XmlTvId": "foo.stations.xmltv.tvmedia.ca",
                                                         "DisplayName": "CBSSN-HD", "FullName": "CBS Sports Network USA HD", "Number": 123}]} """>

let private search index (request: SearchRequest.Root) =
    let url = sprintf "%s/indexes('%s')/docs/search?api-version=2015-02-28" (Environment.GetEnvironmentVariable("SearchUrlBase")) index
    let apiKeyHeader = "api-key", (Environment.GetEnvironmentVariable("SearchApiKey"))
    let json = request.JsonValue.ToString()

    Http.RequestString(url, body = TextRequest json,
                       headers = [ ContentType HttpContentTypes.Json; apiKeyHeader ])

let private searchShows request = search "shows" request |> ShowSearchResults.Parse
let private searchChannels request = search "channels" request |> ChannelSearchResults.Parse

let findChannel channelId = 
    SearchRequest.Root((sprintf "ChannelId eq '%i'" channelId), 1, "") 
    |> searchChannels
    |> fun result -> 
        match Array.tryPick Some result.Value with
        | Some(channel) -> Some(channel.Number)
        | None -> None

let findShowOnNow (name: string) =
    let timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss")
    let filter = sprintf "StartTime lt '%s' and EndTime gt '%s'" timestamp timestamp
    
    SearchRequest.Root(filter, 1, name) 
    |> searchShows
    |> fun result -> Array.tryPick Some result.Value