// This file controls how the game is VIEWED. Meaning, we aren't changing how the game is played, only how its seen by the user - as in size of the shapes,
// and placement of the UI. Think of it like a filter.

module View

open GameCore
open Model
open Microsoft.Xna.Framework

let rw, rh = 400, 600
let resolution = Windowed (rw, rh)

let assetsToLoad = [
    Texture { key = "blank"; path = "Content/white" }
    Texture { key = "block"; path = "Content/block" }
    Font { key = "default"; path = "Content/coders_crux" }
    Sound { key = "blocked"; path = "Content/Sounds/Blocked.wav" }
    Sound { key = "drop"; path = "Content/Sounds/Drop.wav" }
    Sound { key = "gameOver"; path = "Content/Sounds/GameOver.wav" }
    Sound { key = "levelUp"; path = "Content/Sounds/LevelUp.wav" }
    Sound { key = "line"; path = "Content/Sounds/Line.wav" }
    Sound { key = "move"; path = "Content/Sounds/Move.wav" }
    Sound { key = "rotate"; path = "Content/Sounds/Rotate.wav" }
]

// Block size
let bw, bh = 25, 25

// Game space
let gx, gy, gw, gh = 10, 10, 250, 500

// Next block space
let nx, ny, nw, nh = 270, 10, 120, 70

// Game over space
let gameOverSpace = 20, 200, 230, 100

let textScale = 0.5
let textHeight = 20

// Score text
let sx, sy = nx + (nw / 2), ny + nh + 20

// Level text
let lx, ly = sx, sy + 60

// Instruction text
let ix, iy = rw / 2, gy + gh + 30

// Game over text
let gox, goy = 140, 230

let eventSoundMap =
    function
    | Moved -> "move"
    | Rotated -> "rotate"
    | Dropped -> "drop"
    | Line -> "line"
    | LevelUp -> "levelUp"
    | Blocked -> "blocked"
    | GameOver -> "gameOver"

let colorFor colour = 
    match colour with
    | Red -> Color.Red | Magenta -> Color.Magenta | Yellow -> Color.Yellow 
    | Cyan -> Color.Cyan | Blue -> Color.Blue | Silver -> Color.Silver | Green -> Color.Green

let posFor (x,y) (ox, oy) = 
    x * bw + ox, y * bh + oy, bw, bh

let getView _ (model: World) = 
    let gameSpace = [
        ColouredImage (Color.Black, { assetKey = "blank"; destRect = gx-1, gy-1, gw+2, gh+2; sourceRect = None })
        ColouredImage (Color.Gray, { assetKey = "blank"; destRect = gx, gy, gw, gh; sourceRect = None })
    ]
    
    let nextBlockSpace = [
        ColouredImage (Color.Black, { assetKey = "blank"; destRect = nx-1, ny-1, nw+2, nh+2; sourceRect = None })
        ColouredImage (Color.Gray, { assetKey = "blank"; destRect = nx, ny, nw, nh; sourceRect = None })
    ]

    let lines = 
        match model.linesToRemove with 
        | Some lines -> lines |> List.map (fun (_,_,y) -> y) 
        | _ -> []
    let staticBlocks = 
        model.staticBlocks
        |> List.map (fun (c,x,y) ->
            let color = if List.contains y lines then Color.White else colorFor c
            ColouredImage (color, { assetKey = "block"; destRect = posFor (x,y) (gx, gy); sourceRect = None }))

    let currentShape = 
        match model.shape with
        | Some (colour, blocks) ->
            plot model.pos blocks
                |> List.map (fun (x,y) ->
                    ColouredImage (colorFor colour, { assetKey = "block"; destRect = posFor (x,y) (gx, gy); sourceRect = None }))
        | _ -> []

    let nextColour = colorFor <| fst model.nextShape
    let nsw, nsh = snd model.nextShape |> List.head |> List.length, snd model.nextShape |> List.length
    let nsow, nsoh = (nw - (nsw * bw)) / 2, (nh - (nsh * bh)) / 2
    let nextShape = 
        plot (0, 0) <| snd model.nextShape
            |> List.map (fun (x,y) ->
                ColouredImage (nextColour, { assetKey = "block"; destRect = posFor (x,y) (nx + nsow, ny + nsoh); sourceRect = None }))

    let baseText = { assetKey = "default"; text = ""; position = (0, 0); origin = Centre; scale = textScale }
    let text = [
        Text { baseText with text = "Score"; position = (sx, sy) }
        Text { baseText with text = string model.score; position = (sx, sy + textHeight) }
        
        Text { baseText with text = "Level"; position = (lx, ly) }
        Text { baseText with text = string model.level; position = (lx, ly + textHeight) }

        Text { baseText with text = "Instructions"; position = (ix, iy) }
        Text { baseText with scale = 0.4; text = "left to move left, right to move right"; position = (ix, iy + textHeight) }
        Text { baseText with scale = 0.4; text = "up to rotate, down to drop"; position = (ix, iy + textHeight + textHeight) }
    ]

    let gameOver = 
        if model.isGameOver then [
            ColouredImage (Color.Black, { assetKey = "blank"; destRect = gameOverSpace; sourceRect = None })
            ColouredText (Color.White, { baseText with scale = 0.7; text = "Game Over!"; position = (gox, goy) })
            ColouredText (Color.White, { baseText with text = "Press R to restart"; position = (gox, goy + textHeight + textHeight) })
        ] else []

    let sounds = model.events |> List.map (eventSoundMap >> SoundEffect)

    gameSpace @ nextBlockSpace @ staticBlocks @ currentShape @ nextShape @ text @ gameOver @ sounds