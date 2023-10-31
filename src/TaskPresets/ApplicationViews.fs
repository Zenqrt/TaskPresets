module ApplicationViews

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open System
open System.IO
open System.Drawing
open Avalonia.Animation
open Avalonia.Input
open Avalonia.Threading
open System.Threading.Tasks
open WindowsSystem
open Tasks

[<RequireQualifiedAccess>]
module Async =
    let map f a =
        async {
            let! a = a
            return f a
        }

let private mainWindow: Window option =
    match Application.Current.ApplicationLifetime with
    | :? IClassicDesktopStyleApplicationLifetime as applicationLifeTime -> Some applicationLifeTime.MainWindow
    | _ -> None

let private applicationSearchView (searchQuery: IWritable<string>) =
    TextBox.create [
        TextBox.horizontalAlignment HorizontalAlignment.Left
        TextBox.watermark "Search"
        TextBox.width 350
        TextBox.onTextChanged searchQuery.Set
    ]

let private handleFileSelect (topLevel: TopLevel) : Async<string option> =
    Dispatcher.UIThread.InvokeAsync<string option>(
        Func<Task<string option>>(fun () ->
            task {
                let options =
                    Platform.Storage.FilePickerOpenOptions(
                        Title = "Select a .exe file",
                        AllowMultiple = false,
                        FileTypeFilter = [
                            Platform.Storage.FilePickerFileType("Executable file", Patterns = [| "*.exe" |])
                        ]
                    )

                let! folders = topLevel.StorageProvider.OpenFilePickerAsync options
                return folders |> Seq.tryHead |> Option.map (fun folder -> folder.Path.LocalPath)
            })
    )
    |> Async.AwaitTask

let private chooseExecutableButtonView (window: Window) (selectedFile: IWritable<string option>) =
    Button.create [
        Button.content "Choose .exe"
        Button.width 100
        Button.onClick (fun _ ->
            let topLevel = TopLevel.GetTopLevel window

            handleFileSelect topLevel |> Async.map selectedFile.Set |> Async.Start

        )
    ]

let private operationBoxApplicationSelectionView
    (window: Window)
    (searchQuery: IWritable<string>)
    (selectedFile: IWritable<string option>)
    =
    StackPanel.create [
        StackPanel.dock Dock.Top
        StackPanel.margin (15, 10)
        StackPanel.orientation Orientation.Horizontal
        StackPanel.spacing 10

        StackPanel.children [
            applicationSearchView searchQuery
            chooseExecutableButtonView window selectedFile
        ]
    ]


let private applicationListView (searchQuery: IWritable<string>) =
    let searchQuery =
        if searchQuery.Current <> null then
            searchQuery.Current
        else
            ""

    let applications = [
        for application in installedApplications do
            if application.Name.ToLower().Contains(searchQuery.ToLower()) then
                application
    ]

    ListBox.create [

        ListBox.dataItems applications

        ListBox.itemTemplate (
            DataTemplateView<Application>.create (fun application ->
                DockPanel.create [
                    DockPanel.children [
                        application.IconBitmap
                        |> Option.map (fun iconBitmap -> [
                            let tempIconPath = Path.GetTempPath() + Guid.NewGuid.ToString() + ".png"

                            iconBitmap.Save(tempIconPath, Imaging.ImageFormat.Png)

                            Image.source (new Media.Imaging.Bitmap(tempIconPath))
                            Image.width 15
                            Image.height 15

                            File.Delete tempIconPath
                        ])
                        |> Option.defaultValue []
                        |> Image.create


                        TextBlock.create [
                            TextBlock.margin (10, 0, 0, 0)
                            TextBlock.text application.Name
                        ]
                    ]
                ])
        )
    ]

let private applicationSelectionView (window: Window) =
    Component(fun context ->
        let searchQuery = context.useState ""
        let selectedFile = context.useState None

        DockPanel.create [
            DockPanel.children [
                operationBoxApplicationSelectionView window searchQuery selectedFile
                applicationListView searchQuery
            ]
        ])

let applicationSelectionWindow () =
    let window = Window()
    window.Title <- "Choose an application"
    window.CanResize <- false
    window.Width <- 800
    window.Height <- 500
    window.Content <- applicationSelectionView window

    match mainWindow with
    | Some mainWindow -> window.ShowDialog mainWindow
    | None -> null
