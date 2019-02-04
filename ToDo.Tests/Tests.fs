module Tests

open Xunit
open FsUnit
open ToDo.App

[<Fact>]
let ``Adding todo without title does not change ToDos list count`` () =
    let updatedModel = update AddToDo { initialModel with NewToDo = { Title = ""; IsCompleted = false }} |> fst
    updatedModel.ToDos.Length |> should equal 0

[<Fact>]
let ``Adding ToDo with title adds it to todos list`` () =
    let model = { initialModel with NewToDo = { Title = "Test"; IsCompleted = false }}
    let updatedModel = update AddToDo model |> fst
    updatedModel.ToDos.Length |> should equal 1

[<Fact>]
let ``After new ToDo is Added to list next new ToDo has empty title`` () =
    let model = { initialModel with NewToDo = { Title = "Test"; IsCompleted = false }}
    let updatedModel = update AddToDo model |> fst
    updatedModel.NewToDo.Title |> should equal ""

[<Fact>]
let ``Changing new ToDo title updates new Todo model`` () =
    let updatedModel = update (ToDoTitleChanged ("", "NewTitle")) initialModel |> fst
    updatedModel.NewToDo.Title |> should equal "NewTitle"