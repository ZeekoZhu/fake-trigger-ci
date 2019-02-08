module Fake.MyFakeTools
#load ".fake/build.fsx/intellisense.fsx"
#r "paket:
storage none
source https://api.nuget.org/v3/index.json
nuget CommandLineParser.FSharp
nuget Fake.Core.Target
nuget Fake.Tools.Git
nuget FSharp.Core 4.5.0
//"
open Fake.Core
open Fake.Tools
open CommandLine


// ----------------------
// Utils
// ----------------------

let showOutput =
    CreateProcess.redirectOutput
    >> (CreateProcess.withOutputEventsNotNull Trace.log Trace.traceError)
    >> CreateProcess.ensureExitCode

let runCmd file args =
    CreateProcess.fromRawCommand file args
    |> showOutput
    |> Proc.run
    |> ignore

let runCmdAndReturn file args =
    let result =
        CreateProcess.fromRawCommand file args
        |> showOutput
        |> Proc.run
    result.Result.Output

let dockerCmd (subCmd: string) (args: string list) = runCmd "docker" (subCmd::args)

let runGitCmd command =
    let (success, stdout, stderr) = Git.CommandHelper.runGitCommand "./" command
    if success then stdout |> List.head
    else failwith stderr

let getLatestTag () =
    let revListResult =
        runGitCmd "rev-list --tags --max-count=1"
    let tagName =
        runGitCmd (sprintf "describe --tags %s" revListResult)
    tagName

let handleCli<'t> (args: seq<string>) (fn: 't -> unit) =
    let parseResult =
        Parser.Default.ParseArguments<'t> args
    match parseResult with
    | :? Parsed<'t> as parsed -> fn parsed.Value
    | :? NotParsed<'t> as notParsed ->
        failwithf "Invalid: %A, Errors: %A" args notParsed.Errors
    | _ -> failwith "Invalid parser result"

// ----------------------
// Command Line Interface
// ----------------------

module TriggerCI =
    type TriggerCIOptions =
        { [<Option('v', "version", Required = true)>] Version: string
          [<Option('l', "latest", Default = false)>] IsProd: bool
        }

    let ensureWorkspaceClean () =
        let isEmpty = Git.Information.isCleanWorkingCopy "./"
        if not isEmpty then failwith "Workspace is not clean"
        isEmpty

    let validateVersion (options: TriggerCIOptions) =
        let newTag =
            if options.IsProd then options.Version
            else options.Version + "-" + Git.Information.getCurrentHash ()
            |> SemVer.parse
        let latestTag = getLatestTag () |> SemVer.parse
        Trace.tracefn "Latest version: %s" latestTag.AsString
        if newTag < latestTag then failwithf "Invalid version: %A < %A" newTag latestTag
        Trace.tracefn "New version: %s" newTag.AsString
        newTag

    let tagCurrent (tag) =
        Git.Branches.tag "./" tag

    let pushCommits () =
        let runCmd = Git.CommandHelper.runSimpleGitCommand "./"
        runCmd "push --all" |> ignore
        runCmd "push --tags" |> ignore

    let triggerCi (options: TriggerCIOptions) =
        Trace.logfn "%A" options
        ensureWorkspaceClean () |> ignore
        let version = validateVersion options
        tagCurrent version.AsString
        pushCommits ()
        ()

    let triggerCiCli (args: seq<string>) =
        handleCli args triggerCi

Target.create "TriggerCI" (fun p -> TriggerCI.triggerCiCli p.Context.Arguments)

