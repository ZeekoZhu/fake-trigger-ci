#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.MyFakeTools

let MyFeeds = "https://www.myget.org/F/zeekoget/api/v2/package"

let build () =
    Shell.cd "./src"
    Shell.rm_rf "./bin"
    Utils.runCmd "dotnet" ["build"; "-c"; "Release"]
    Utils.runCmd "paket" ["pack"; "bin/Release"]

let pushPackage () =
    let nupkg =
        !! "bin/Release/*.nupkg"
        |> Seq.head
    Utils.runCmd "paket" ["push"; "--url"; MyFeeds; nupkg]

Target.create "Build" (fun _ -> build ())

Target.create "Push" (fun _ -> pushPackage ())

"Build" ==> "Push"

Target.useTriggerCI ()

Target.create "Empty" ignore

Target.runOrDefaultWithArguments "Empty"
