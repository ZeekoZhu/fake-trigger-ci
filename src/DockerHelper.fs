namespace Fake.MyFakeTools

open System.IO
open Fake.Core

module DockerHelper =

    let login username (password: string) =
        let input = StreamRef.Empty

        let proc =
            CreateProcess.fromRawCommand
                "docker" [ "login"; "-u"; username; "--password-stdin" ]
            |> CreateProcess.withStandardInput (CreatePipe input)
            |> Utils.showOutput
            |> Proc.start
        use inputWriter = new StreamWriter(input.Value)
        inputWriter.WriteLine password
        inputWriter.Close()
        proc.Wait()
