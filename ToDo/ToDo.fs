namespace ToDo

open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms
open System

module App = 
    type ToDo = { Title: string; IsCompleted: bool }
    type Model = { ToDos: ToDo list; NewToDo: ToDo }
    type Msg = 
        | AddToDo 
        | SortToDos
        | ClearCompletedToDos
        | ToDoCompletedChanged of int * bool
        | ToDoTitleChanged of string * string

    let emptyToDo = { Title = ""; IsCompleted = false }
    let initialModel = { ToDos = List.Empty; NewToDo = emptyToDo }
    let init () = initialModel, Cmd.none
    let update msg model =
        match msg with
        | AddToDo -> (if model.NewToDo.Title.Length > 1 
                        then { ToDos = model.NewToDo :: model.ToDos; NewToDo = emptyToDo } 
                        else model), Cmd.none
        | ToDoTitleChanged(_, newValue) -> 
            { model with NewToDo = {Title = newValue; IsCompleted = false}}, Cmd.none
        | ToDoCompletedChanged(selectedIndex, isCompleted) -> 
            { model with ToDos = model.ToDos 
                         |> List.mapi 
                            (fun index todo -> if index = selectedIndex 
                                               then {todo with IsCompleted = isCompleted} 
                                               else todo)}, SortToDos 
                                               |> Cmd.ofMsg
        | ClearCompletedToDos -> 
            { model with ToDos = model.ToDos |> List.filter (fun todo -> todo.IsCompleted = false)}, Cmd.none
        | SortToDos -> { model with ToDos = model.ToDos |> List.sortBy(fun item -> item.IsCompleted)}, Cmd.none

    let view (model: Model) dispatch =
        View.ContentPage(
          content = View.StackLayout(padding = 10.0, verticalOptions = LayoutOptions.Start,
            children = [ 
                View.Entry(placeholder = "ToDo title"
                    ,horizontalOptions = LayoutOptions.Center
                    ,widthRequest = 200.0
                    ,text = model.NewToDo.Title
                    ,textChanged = debounce 250 (fun args -> dispatch (ToDoTitleChanged(args.OldTextValue, args.NewTextValue))))
                View.StackLayout(orientation = StackOrientation.Horizontal
                    ,children = [View.Button(text = "Add"
                                ,command = (fun () -> dispatch AddToDo)
                                ,isEnabled = (model.NewToDo.Title.Length > 0)
                                ,horizontalOptions = LayoutOptions.Start);
                                View.Button(text = "Clear completed"
                                ,command = (fun() -> dispatch ClearCompletedToDos)
                                ,horizontalOptions = LayoutOptions.End)
                    ]
                )
                View.Label(text = sprintf "%d" model.ToDos.Length
                    ,horizontalOptions = LayoutOptions.Center
                    ,widthRequest = 200.0
                    ,horizontalTextAlignment=TextAlignment.Center);
                View.ListView(
                    selectionMode = ListViewSelectionMode.None,
                    items = (model.ToDos |> List.mapi (fun index todo -> View.StackLayout(
                        orientation = StackOrientation.Horizontal,
                        children = [
                            View.Label(text=todo.Title, horizontalOptions = LayoutOptions.StartAndExpand)
                            View.Switch(isToggled = todo.IsCompleted, toggled = (fun args -> dispatch (ToDoCompletedChanged(index, args.Value))))
                      ])))
                    )
            ]))
    // Note, this declaration is needed if you enable LiveUpdate
    let program = Program.mkProgram init update view

type App () as app = 
    inherit Application ()
    let runner = 
        App.program
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> Program.runWithDynamicView app

    let modelId = "model"
    override __.OnSleep() = 
        let json = Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)
        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)
        app.Properties.[modelId] <- json

    override __.OnResume() = 
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try 
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) -> 
                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)
                let model = Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)
                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel (model, Cmd.none)
            | _ -> ()
        with ex -> 
            App.program.onError("Error while restoring model found in app.Properties", ex)

    override this.OnStart() = 
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()


