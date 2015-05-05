module FileParser

open System
open System.IO
open System.Text.RegularExpressions

type private Name = string
type private Weight = int
type private Variant = Name * Weight
type private Test = Name * Variant list

let parseFile file =

    let rec loop accumulatedTests currentTest remainingLines : Test list =

        let parseVariant (variant:string) =
            let [| name; weight; |] = 
                variant.Split([| ":" |], StringSplitOptions.RemoveEmptyEntries) 
                |> Array.map(fun s -> s.Trim())

            (name,int(weight))

        match remainingLines with
        | [] -> 
            match currentTest with
            | None -> accumulatedTests
            | Some current -> current::accumulatedTests
        | head::tail ->
            match head with
            | test when Regex.IsMatch(test, "^\s") = false ->
                let newTest = Some (test.Trim(),[])
                let accumulatedTests = 
                    match currentTest with
                    | None -> accumulatedTests
                    | Some current -> (current::accumulatedTests)
                loop accumulatedTests newTest tail
            | variant ->
                match currentTest with
                | None -> failwith ("Expecting a test, but given: " + variant)
                | Some (testName,variants) -> loop accumulatedTests (Some (testName,(variant |> parseVariant)::variants)) tail

    loop [] None (File.ReadAllLines(file) |> List.ofArray)