// Learn more about F# at http://fsharp.org
module PokeSmart

open System
open Microsoft.ML
open Microsoft.ML.Data
open Pokemon
open Data
open FSharp.Data
open XPlot.Plotly
open System.IO
open System.Diagnostics

[<Literal>]
let Url = "Dataset/pokemon.csv"
type PokemonCSV = CsvProvider<Url>

let showResult pokemon =
    printfn "Number; Name; Total; Type1; Type2; Cluster"

    pokemon
    |> Seq.iter(fun (pkmn, result) -> printfn "%i;%s;%i;%s;%s;%i" 
                                                ((int)pkmn.Number) 
                                                pkmn.Name 
                                                ((int)pkmn.Total)
                                                pkmn.Type1
                                                pkmn.Type2
                                                result.PredictedClusterId)

let generateChartData (options:Microsoft.ML.Trainers.KMeansTrainer.Options) pokemon =
    [
        for cluster in 1..options.NumberOfClusters do
            let pkmn = pokemon 
                        |> Seq.filter( fun (pkmn, result) -> result.PredictedClusterId = (uint32) cluster)

            yield Scatter3d(
                x = (pkmn |> Seq.map( fun (pkmn, result) -> pkmn.ConvertedType1)),
                y = (pkmn |> Seq.map( fun (pkmn, result) -> pkmn.ConvertedType2)),
                z = (pkmn |> Seq.map( fun (pkmn, result) -> pkmn.Total)),
                text = (pkmn |> Seq.map( fun (pkmn, result) -> pkmn.Name)),
                mode = "markers",
                marker =
                    Marker(
                        size = 12.,
                        opacity = 0.8
                    )
        )
    ]  

let generateChart chartOptions (chartData: Scatter3d list) =
    chartData
    |> Chart.Plot
    |> Chart.WithOptions chartOptions
    |> Chart.WithHeight 600
    |> Chart.WithWidth 800
    |> Chart.WithLabels ["Pokémon fracos"; "Pokémon Fortes";"Pokémon Medianos"]

let generateFileProcess (chart: PlotlyChart) =
    let html = chart.GetHtml()
    File.Delete("metrics.html")
    File.AppendAllLines ("metrics.html",[html])

    Process.Start (@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", 
                   "file:\\" + Directory.GetCurrentDirectory() + "\\metrics.html")

[<EntryPoint>]
let main argv =
    let allPokemon = PokemonCSV.Load Url
    let listOfPokemon = allPokemon.Rows
                        |> Seq.map (fun pokemon -> {
                            Number = (single) pokemon.``#``
                            Name = pokemon.Name 
                            Type1 = pokemon.``Type 1``
                            ConvertedType1 = (typeToSingle pokemon.``Type 1``)
                            Type2 = pokemon.``Type 2``
                            ConvertedType2 = (typeToSingle pokemon.``Type 2``)
                            HP = (single) pokemon.HP
                            Attack = (single) pokemon.Attack
                            SpAttack = (single) pokemon.``Sp. Atk``
                            Defense = (single) pokemon.Defense
                            SpDefense = (single) pokemon.``Sp. Def``
                            Speed = (single) pokemon.Speed
                            Total = (single) pokemon.Total
                        })

    let mlContext = MLContext();
    let data = listOfPokemon
               |> mlContext.Data.LoadFromEnumerable

    let pipeline = EstimatorChain().Append(
                               mlContext.Transforms.Concatenate( "Features", 
                                   "ConvertedType1","ConvertedType2",
                                   "HP","Attack","SpAttack", 
                                   "Defense", "SpDefense", "Speed","Total" ))

    let options = Trainers.KMeansTrainer.Options()
    options.NumberOfClusters <- 3
    options.FeatureColumnName <- "Features"
        
    let trainer = mlContext.Clustering.Trainers.KMeans options                                                     

    let pipelineTraining = pipeline.Append trainer
    let model = pipelineTraining.Fit data

    let predictiveModel = mlContext.Model.CreatePredictionEngine<Pokemon, ClusterPrediction>(model)

    let chartOptions = Options ( title = "Poke Cluster")

    listOfPokemon
    |> Seq.map(fun pokemon -> pokemon,(predictiveModel.Predict pokemon))
    |> generateChartData options
    |> generateChart chartOptions
    |> generateFileProcess
    |> ignore

    0