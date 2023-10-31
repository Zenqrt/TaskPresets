module Views

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.Animation
open Avalonia.Input
open Avalonia.Threading

let iconButtonWithLabelView (label: string) (icon: Types.IView) (func: Interactivity.RoutedEventArgs -> unit) =
    StackPanel.create [
        StackPanel.width 75
        StackPanel.height 85

        StackPanel.children [
            Button.create [
                Button.dock Dock.Top
                Button.content (icon)
                Button.width 75
                Button.height 75
                Button.cornerRadius 10
                Button.onClick func
            ]
            TextBlock.create [
                TextBlock.textAlignment Media.TextAlignment.Center
                TextBlock.text (if label.Length > 8 then label.Remove 8 + "..." else label)
            ]
        ]
    ]

let mainWindow: Window option =
    match Application.Current.ApplicationLifetime with
    | :? IClassicDesktopStyleApplicationLifetime as applicationLifeTime -> Some applicationLifeTime.MainWindow
    | _ -> None
