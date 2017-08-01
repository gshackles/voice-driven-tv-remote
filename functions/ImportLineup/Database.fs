module Database

open FSharp.Data
open FSharp.Data.SqlClient

[<Literal>]
let configFile = "D:\\home\\site\\wwwroot\\ImportLineup\\app.config"

let getChannelLookup() =
    use cmd = new SqlCommandProvider<"SELECT ChannelId, XmlTvId FROM Channel", "name=TVListings", ConfigFile=configFile>()
    cmd.Execute()
    |> Seq.map(fun row -> (row.XmlTvId, row.ChannelId))
    |> dict

let clearShows() =
    use cmd = new SqlCommandProvider<"TRUNCATE TABLE Show", "name=TVListings", ConfigFile=configFile>()
    cmd.Execute() |> ignore

let addShow title startTime endTime channelId description category =
    use cmd = new SqlCommandProvider<"INSERT INTO Show VALUES (@title, @startTime, @endTime, @channelId, @description, @category)", "name=TVListings", ConfigFile=configFile>()
    cmd.Execute(title = title, startTime = startTime, endTime = endTime,
                channelId = channelId, description = description, category = category) |> ignore