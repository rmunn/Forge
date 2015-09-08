module Fix

open Fake.Git
open Fake.FileHelper
open System.IO
open System

let RefreshTemplates () =
    printfn "Getting templates..."
    Repository.cloneSingleBranch "." "https://github.com/fsprojects/generator-fsharp.git" "templates" "templates"

let applicationNameToProjectName folder projectName =
    let applicationName = "ApplicationName"
    let files = Directory.GetFiles folder |> Seq.where (fun x -> x.Contains applicationName)
    files |> Seq.iter (fun x -> File.Move(x, x.Replace(applicationName, projectName)))

let sed (find:string) replace folder =
    folder 
    |> Directory.GetFiles
    |> Seq.iter (fun x -> 
                    let contents = File.ReadAllText(x).Replace(find, replace)
                    File.WriteAllText(x, contents))

let New projectName =
    let directory = System.Environment.CurrentDirectory
    let templatePath = Path.Combine(directory, "templates")
    let projectFolder = Path.Combine(directory, projectName)

    if not <| Directory.Exists templatePath
    then RefreshTemplates ()

    printfn "Choose a template:"
    let templates = Directory.GetDirectories(templatePath) 
                    |> Seq.map (fun x -> x.Replace(Path.GetDirectoryName(x) + "\\", ""))
                    |> Seq.where (fun x -> not <| x.StartsWith("."))
    
    templates |> Seq.iter (fun x -> printfn "%s" x)

    let templateChoice = Console.ReadLine()
    printfn "Fixing template %s" templateChoice
    let templateDir = Path.Combine(templatePath, templateChoice)
    
    Directory.Move(templateDir, projectFolder)


    //Fake.FileHelper.CopyDir projectFolder templateDir (fun _ -> true)

    printfn "Changing filenames from ApplicationName.* to %s.*" projectName
    applicationNameToProjectName projectFolder projectName

    printfn "Changing namespace to %s" projectName
    projectFolder |> sed "<%= namespace %>" projectName
    
    let guid = Guid.NewGuid().ToString()
    printfn "Changing guid to %s" guid
    projectFolder |> sed "<%= guid %>" guid
    printfn "Done!"
    ()

let Help () = 
    printfn "Fix (Mix for F#)"
    printfn "Available Commands:"
    printfn " new [projectName] - creates a new project with the given name"
    printfn " help - displays this help"
    printfn ""

[<EntryPoint>]
let main argv = 
    let list = [ "new" ; "suaveTest"]
    match list with
    | [] -> Help()
    | h::t -> match h with
              | "new" -> New (t |> Seq.head)
              | _ -> printfn "Unknown option"
                     Help ()

    let stayOpenForDebugging = Console.ReadKey()
    0