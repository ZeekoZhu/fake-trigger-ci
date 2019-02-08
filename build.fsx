#load ".fake/build.fsx/intellisense.fsx"
#load ""
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.Tools
open CommandLine



Target.create "Empty" ignore

Target.runOrDefault "Empty"
