module Telemetry

open System
open Microsoft.ApplicationInsights
open Microsoft.ApplicationInsights.Extensibility
open Microsoft.ApplicationInsights.DataContracts

let private instrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY")
let private telemetryClient = TelemetryClient(InstrumentationKey = instrumentationKey)

let setOperationId operationId =
    telemetryClient.Context.Operation.Id <- operationId

let startOperation (name:string) = 
    telemetryClient.StartOperation<DependencyTelemetry>(name)