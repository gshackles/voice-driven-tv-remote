#load "commands.fsx"
#load "search.fsx"
open System.Net.Http
open AlexaSkillsKit.Slu
open AlexaSkillsKit.Speechlet
open AlexaSkillsKit.UI

let buildResponse output shouldEndSession =
    SpeechletResponse(ShouldEndSession = shouldEndSession,
                      OutputSpeech = PlainTextOutputSpeech(Text = output))

let handleDirectCommand (intent: Intent) =
    match (Commands.getCommand intent.Slots.["command"].Value) with
    | Some(command) ->
        Commands.executeCommand command.Slug
        buildResponse "OK" true
    | None -> buildResponse "Sorry, that command is not available right now" true

let handleWatchShow (intent: Intent) =
    Search.findShowOnNow intent.Slots.["name"].Value |> function
    | Some(show) -> Search.findChannel show.ChannelId |> function
                    | Some(channel) -> 
                        Commands.watchTV()
                        Commands.changeChannel channel
                        
                        buildResponse "OK" true
                    | None -> buildResponse "Sorry, I could not find the channel for that show" true
    | None -> buildResponse "Sorry, I could not find that show" true

let handleIntent (intent: Intent) =
    match intent.Name with
    | "DirectCommand" -> handleDirectCommand intent
    | "WatchShow" -> handleWatchShow intent
    | _ -> buildResponse "Sorry, I'm not sure how to do that" true

type RemoteSpeechlet(log: TraceWriter) =
    inherit Speechlet()

    override this.OnLaunch(request: LaunchRequest, session: Session): SpeechletResponse =
        sprintf "OnLaunch: request %s, session %s" request.RequestId session.SessionId |> log.Info
        buildResponse "" false

    override this.OnIntent(request: IntentRequest, session: Session): SpeechletResponse =
        sprintf "OnIntent: request %s, session %s" request.RequestId session.SessionId |> log.Info
        handleIntent request.Intent

    override this.OnSessionStarted(request: SessionStartedRequest, session: Session) =
        sprintf "OnSessionStarted: request %s, session %s" request.RequestId session.SessionId |> log.Info

    override this.OnSessionEnded(request: SessionEndedRequest, session: Session) =
        sprintf "OnSessionEnded: request %s, session %s" request.RequestId session.SessionId |> log.Info

let Run(req: HttpRequestMessage, log: TraceWriter) =
    sprintf "%O" req
    let speechlet = RemoteSpeechlet log
    speechlet.GetResponse req