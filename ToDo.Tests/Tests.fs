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
let ``Changing new ToDo title in ui updates new Todo title in model`` () =
    let updatedModel = update (ToDoTitleChanged ("", "NewTitle")) initialModel |> fst
    updatedModel.NewToDo.Title |> should equal "NewTitle"

[<Fact>]
let ``ClearCompletedToDos does nothing if all todos are not completed`` () =
    let model = { initialModel with ToDos = [{ Title = "1"; IsCompleted = false }; 
                                             { Title = "2"; IsCompleted = false };]}
    let updatedModel = update ClearCompletedToDos model |> fst
    updatedModel.ToDos |> should equal model.ToDos

[<Fact>]
let ``ClearCompletedToDos Removes completed Todos from model`` () =
    let model = { initialModel with ToDos = [{ Title = "1"; IsCompleted = false };
                                             { Title = "2"; IsCompleted = true };
                                             { Title = "3"; IsCompleted = true };]}
    let updatedModel = update ClearCompletedToDos model |> fst
    updatedModel.ToDos |> List.exactlyOne

[<Fact>]
let ``Sort ToDos moves completed ToDos to the end of the ToDo list`` () =
    let firstCompletedItem = { Title = "1"; IsCompleted = true }
    let secondNotCompletedItem = { Title = "2"; IsCompleted = false }
    let thirdCompletedItem = { Title = "3"; IsCompleted = true }
    let model = { initialModel with ToDos = [ firstCompletedItem; secondNotCompletedItem; thirdCompletedItem ]}
    let updatedModel = update SortToDos model |> fst
    updatedModel.ToDos |> List.head |> should equal secondNotCompletedItem
    updatedModel.ToDos.[1] |> should equal firstCompletedItem
    updatedModel.ToDos |> List.last |> should equal thirdCompletedItem
