#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.MyFakeTools


Target.useTriggerCI ()

Target.create "Empty" ignore

Target.runOrDefaultWithArguments "Empty"
