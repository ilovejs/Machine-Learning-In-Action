﻿#load "SupportVectorMachine.fs"
#r @"C:\Users\Mathias\Documents\GitHub\Machine-Learning-In-Action\MachineLearningInAction\packages\MSDN.FSharpChart.dll.0.60\lib\MSDN.FSharpChart.dll"
#r "System.Windows.Forms.DataVisualization"

open MachineLearning.SupportVectorMachine
open System
open System.Drawing
open System.Windows.Forms.DataVisualization
open MSDN.FSharp.Charting
 

let weights rows =
    rows 
    |> Seq.filter (fun r -> r.Alpha > 0.0)
    |> Seq.map (fun r ->
        let mult = r.Alpha * r.Label
        r.Data |> List.map (fun e -> mult * e))
    |> Seq.reduce (fun acc row -> 
        List.map2 (fun a r -> a + r) acc row )
        
// demo
let rng = new Random()

// tight dataset: there is no margin between 2 groups
let tightData = 
    [ for i in 1 .. 500 -> [ rng.NextDouble() * 100.0; rng.NextDouble() * 100.0 ] ]
let tightLabels = 
    tightData |> List.map (fun el -> 
        if (el |> List.sum >= 100.0) then 1.0 else -1.0)

// loose dataset: there is empty "gap" between 2 groups
let looseData = 
    tightData 
    |> List.filter (fun e -> 
        let tot = List.sum e
        tot > 110.0 || tot < 90.0)
let looseLabels = 
    looseData |> List.map (fun el -> 
        if (el |> List.sum >= 100.0) then 1.0 else -1.0)

// create an X,Y scatterplot, with different formatting for each label 
let scatterplot (dataSet: (float * float) seq) (labels: 'a seq) =
    let byLabel = Seq.zip labels dataSet |> Seq.toArray
    let uniqueLabels = Seq.distinct labels
    FSharpChart.Combine 
        [ // separate points by class and scatterplot them
          for label in uniqueLabels ->
               let data = 
                    Array.filter (fun e -> label = fst e) byLabel
                    |> Array.map snd
               FSharpChart.Point(data) :> ChartTypes.GenericChart
               |> FSharpChart.WithSeries.Marker(Size=10)
        ]
    |> FSharpChart.Create    

let test (data: float list list) (labels: float list) parameters =
    let estimator = simpleSvm data labels parameters
    let w = weights (fst estimator)
    let b = snd estimator

    let classify row = b + dot w row

    let performance = 
        data 
        |> List.map (fun row -> classify row)
        |> List.zip labels
        |> List.map (fun (a, b) -> if a * b > 0.0 then 1.0 else 0.0)
        |> List.average
    performance

let plot (data: float list list) (labels: float list) parameters =
    let estimator = simpleSvm data labels parameters
    let labels = 
        estimator 
        |> (fst) 
        |> Seq.map (fun row -> 
            if row.Alpha > 0.0 then 0
            else
                if row.Label < 0.0 then 1
                else 2)
    let data = 
        estimator 
        |> (fst) 
        |> Seq.map (fun row -> (row.Data.[0], row.Data.[1]))
    scatterplot data labels

let parameters = { C = 5.0; Tolerance = 0.01; Depth = 500 }
test tightData tightLabels parameters
test looseData looseLabels parameters
plot tightData tightLabels parameters
plot looseData looseLabels parameters