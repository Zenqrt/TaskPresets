namespace TaskPresets.DSL

[<AutoOpen>]
module ExperimentalAcrylicBorder =
    open Avalonia.Controls
    open Avalonia.FuncUI.Types
    open Avalonia.FuncUI.Builder
    open Avalonia.FuncUI.DSL
    open Avalonia.Media

    let create (attrs: IAttr<ExperimentalAcrylicBorder> list) : IView<ExperimentalAcrylicBorder> =
        ViewBuilder.Create<ExperimentalAcrylicBorder>(attrs)

    type ExperimentalAcrylicBorder with

        static member material<'t when 't :> ExperimentalAcrylicBorder>
            (value: ExperimentalAcrylicMaterial)
            : IAttr<'t> =
            AttrBuilder<'t>
                .CreateProperty<ExperimentalAcrylicMaterial>(
                    ExperimentalAcrylicBorder.MaterialProperty,
                    value,
                    ValueNone
                )
