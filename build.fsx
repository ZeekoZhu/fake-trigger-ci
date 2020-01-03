open Fake.MyFakeTools

#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

let MyFeeds = "https://www.myget.org/F/zeekoget/api/v2/package"

let build () =
    Shell.cd "./src"
    Trace.traceHeader "Removing previous build"
    Shell.rm_rf "./bin"
    Trace.traceHeader "Building project"
    Utils.runCmd "dotnet" ["build"; "-c"; "Release"]
    Trace.traceHeader "Packing nuget package"
    Utils.runCmd "paket" ["pack"; "bin/Release"]

let pushPackage () =
    let nupkg =
        !! "bin/Release/*.nupkg"
        |> Seq.head
    Trace.traceHeader "Pushing Package"
    Utils.runCmd "paket" ["push"; "--url"; MyFeeds; nupkg]

Target.create "Build" (fun _ -> build ())

Target.create "Push" (fun _ -> pushPackage ())

"Build" ==> "Push"

Target.useTriggerCI ()

Target.create "Empty" ignore

Target.runOrDefaultWithArguments "Empty"
