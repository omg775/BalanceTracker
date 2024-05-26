namespace Asssignment2

open WebSharper
open WebSharper.UI
open WebSharper.UI.Notation
open WebSharper.JavaScript
open WebSharper.UI.Templating
open WebSharper.UI.Client
open WebSharper.ChartJs
open WebSharper.UI.Html

[<JavaScript>]
module Templates =   
    type MainTemplate = Templating.Template<"Main.html", ClientLoad.FromDocument, ServerLoad.PerRequest>

[<JavaScript>]
module Client =
    let DoSomething (input: string) =
        System.String(Array.rev(input.ToCharArray()))

    type Entry = { Type: string; Category: string; Amount: float; eventDate: string }
    type Summary = { Category: string; Amount: float }
    type Result ={ Category: string; SumAmount: float }

    let data = ListModel.Create(fun entry -> entry) 
                [   {Type = "Expense"; Category = "Entertainment"; Amount = 200.0; eventDate = "2024-04-19"}
                    {Type = "Expense"; Category = "Groceries"; Amount = 500.0; eventDate = "2024-05-19"}
                    {Type = "Income"; Category = "Pension"; Amount = 1000.0; eventDate = "2024-03-22"}
                    {Type = "Expense"; Category = "Clothing"; Amount = 60.0; eventDate = "2024-04-20"}
                    {Type = "Income"; Category = "Rental"; Amount = 500.0; eventDate = "2024-02-21"}   ]

    let entryType = Var.Create "Income"
    let category = Var.Create ""
    let amount = Var.Create 0.0
    let eventDate = Var.Create ""

    let totalBalance =
        data.View
        |> View.MapSeqCached (fun entry ->
            match entry.Type with
            | "Income" -> entry.Amount
            | _ -> -entry.Amount
        )
        |> View.Map Seq.sum 
        |> View.Map (sprintf "%.2f")

    let addEntry () =
            data.Add { Type = entryType.Value; Category = category.Value; Amount = amount.Value; eventDate = eventDate.Value}
            category := ""
            amount := 0.0

    let groupedData =
        data.View
        |> View.Map (Seq.groupBy (fun entry -> entry.Category))
        |> View.Map (Seq.map (fun (category, entries) -> 
                        { Category = category
                          Amount = entries |> Seq.sumBy (fun entry -> entry.Amount) }
        ))
        |> View.Map Seq.toArray

    let GetAllData() =
        data |> Seq.toArray

    let newData = GetAllData()

    let groupedData1 = 
        newData
        |> Array.filter(fun a -> a.Type = "Expense")
        |> Array.groupBy (fun x -> x.Category)
        |> Array.map (fun (category, items) ->
            let sumAmount = items |> Array.sumBy (fun item -> item.Amount)
            { Category = category; SumAmount = sumAmount })

    let data1 = 
        groupedData1
        |> Array.map (fun arr -> arr.SumAmount)

    let label1 =
        groupedData1
        |> Array.map (fun arr -> arr.Category)
     
    let bgColor = [|
        "rgba(255, 99, 132, 0.2)";
        "rgba(54, 162, 235, 0.2)";
        "rgba(255, 206, 86, 0.2)";
        "rgba(75, 192, 192, 0.2)";
        "rgba(153, 102, 255, 0.2)";
        "rgba(255, 159, 64, 0.2)"
    |]
    
    let bdColor = [|
            "rgba(255, 99, 132, 1)";
            "rgba(54, 162, 235, 1)";
            "rgba(255, 206, 86, 1)";
            "rgba(75, 192, 192, 1)";
            "rgba(153, 102, 255, 1)";
            "rgba(255, 159, 64, 1)"
        |] 

    let pieDataSet = PieChartDataSet()
    pieDataSet.Label <- "Number of votes"
    pieDataSet.Data <- data1
    pieDataSet.BackgroundColor <- Union2Of2 bgColor
    pieDataSet.BorderColor <- Union2Of2 bdColor  
    pieDataSet.BorderWidth <- 3

    let pieData = ChartData()
    pieData.Datasets <- [|pieDataSet|]
    pieData.Labels <- label1

    let pieOptions = Options()
    pieOptions.Plugins <- Plugin(
        Title = TitleConfig(
            Display = true,
            Text = Union1Of2 "Pie Chart of Expenses",
            Font = Font(
                Size = 30,
                Family = "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif",
                Style = FontStyle.Italic
            )
        ),
        Subtitle = TitleConfig(
            Display = true,
            Text = Union1Of2 "Total expenses",
            Font = Font(
                Size = 18,
                Family = "'Lucida Console', 'Courier New', 'monospace'"
            )
        )
    )

    let pieChart = ChartCreate(pieData, pieOptions)
    let pie () = Chart("pie", pieChart)


    let Main () =
        let wrapCanvas x =
            div [] [x]
        Templates.MainTemplate.expense()
            .TypeChange(fun e ->
                let value = e.Vars.entryType.Value
                entryType := value
            )
            .CatInput(fun e -> 
                let catValue = e.Vars.category.Value
                category := catValue
            )
            .AmountInput(fun e ->
                let amt = e.Vars.amount.Value
                amount := amt
            )
            .AddEntry(fun e ->
                if category.Value <> "" && amount.Value > 0.0 then
                    addEntry ()
                    e.Vars.category.Value <- ""
                    e.Vars.amount.Value <- 0.0
            )
            .DateInput(fun e ->
                let dt = e.Vars.eventDate.Value
                eventDate := dt
            )
            .TotalBalance(totalBalance)
            .Entries(
                ListModel.View data |> Doc.BindSeqCached (fun entry ->
                    Templates.MainTemplate.Item()
                        .Type(entry.Type.ToString())
                        .Category(entry.Category.ToString())
                        .Amount(entry.Amount.ToString())
                        .EventDate(entry.eventDate.ToString())
                        .Doc()
                )
            )
            .Graph(
                let x = canvas [attr.``id`` "pie";on.afterRender (fun _ -> pie () |> ignore)] [] |> wrapCanvas
                wrapCanvas x
            )
            .Doc()
