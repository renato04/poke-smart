module Data

open Microsoft.ML.Data

[<CLIMutable>]
type ClusterPrediction = {
    [<ColumnName("PredictedLabel")>] 
    PredictedClusterId : uint32

    [<ColumnName("Score")>] 
    Distances : single array
}    