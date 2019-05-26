#r "../packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "../packages/FSharp.Data.SqlClient/lib/net40/FSharp.Data.SqlClient.dll"
#r "System.Xml.Linq.dll"
#r "System.Configuration"

open System.IO  
open System.Text.RegularExpressions  
open FSharp.Data  
open FSharp.Data.SqlClient

type Channels = XmlProvider<"./channels.xml">  
let doc = Channels.Parse (File.ReadAllText "./channels.xml")

type TvChannel = { XmlTvId: string; FullName: string; DisplayName: string; Number: int }

doc.Channels  
|> Array.map (fun channel ->
    match channel.DisplayNames with
    | [| fullName; displayName; number |] -> Some({ XmlTvId = channel.Id
                                                    FullName = fullName.String.Value
                                                    DisplayName = Regex.Replace(displayName.String.Value, "\\-HD$", "", RegexOptions.None)
                                                    Number = number.Number.Value })
    | _ -> None
)
|> Array.choose id
|> Array.iter (fun channel ->
    use cmd = new SqlCommandProvider<"INSERT INTO Channel VALUES (@xmlTvId, @displayName, @fullName, @number)", "name=TVListings">()
    cmd.Execute(xmlTvId = channel.XmlTvId, displayName = channel.DisplayName, fullName = channel.FullName, number = channel.Number) |> ignore

    printfn "Added channel: %s" channel.DisplayName
)