module Tasks

open System.Diagnostics
open System.Drawing

type TaskType =
    | Browser of {| Urls: string list |}
    | CommandPrompt of {| Commands: string list |}
    | SystemExecutable of {| Path: string |}

type Task = {
    Name: string
    TaskType: TaskType
}


let private startUpTask (task: Task) =
    match task.TaskType with
    | Browser browser -> browser.Urls |> List.iter (fun url -> Process.Start url |> ignore)

    | CommandPrompt commandPrompt ->
        commandPrompt.Commands
        |> String.concat " && "
        |> fun command -> Process.Start("CMD.exe", command) |> ignore

    | SystemExecutable systemExecutable -> systemExecutable.Path |> Process.Start |> ignore

type NotificationExceptionType =
    | Allow
    | Silence

type Application = {
    Name: string
    InstallationPath: string
    IconBitmap: Bitmap option
}

type TaskPreset = {
    Name: string
    CloseOtherApplications: bool
    DisableOpeningOtherApplications: bool
    HideNotifications: bool
    NotificationExceptionType: NotificationExceptionType
    NotificationExceptionApplications: Application list
    Tasks: Task list
}

let dummyTaskPreset = {
    Name = "Dummy Preset"
    CloseOtherApplications = false
    DisableOpeningOtherApplications = false
    HideNotifications = true
    NotificationExceptionType = Silence
    NotificationExceptionApplications = []
    Tasks = []
}

let startUpPreset (taskPreset: TaskPreset) =
    taskPreset.Tasks |> List.iter startUpTask

let taskPresets: TaskPreset list = [ dummyTaskPreset ]