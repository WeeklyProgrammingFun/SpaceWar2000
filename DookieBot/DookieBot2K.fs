open System
open System.Diagnostics

// Chris Lomont SpaceWar 2K bot

//********************* Types ***************************************
type Owner   = NoOne | Player1 | Player2
type Planet  = {x : float; y:float; owner : Owner; id : int; population : int; growthRate : int}
type Fleet   = {owner : Owner; id : int; population : int; src : int; dst : int}
type State   = {rand : Random; frame:int; maxFrame:int; enemyName:string;}
//********************* Utility *************************************

let moveStrings = [|"ROCK";"PAPER";"SCISSORS"|]
let beats = [|1;2;0|] // index i is who beats move i

// score.[i*3+j] is +1 if i beats j, -1 if j beats i, else 0
let score = 
    [|
    +0; -1;  1;  // rock     : ...
    +1;  0; -1;  // paper    : ...
    -1;  1;  0   // scissors : ...
    |]


// compute score over all history
let computeScore selfHistory otherHistory = 
    List.fold (fun acc (s,o) -> acc+(score.[s*3+o])) 0 (List.zip selfHistory otherHistory)

//********************* Predictors **********************************
// predictors take played and opponent history and return a predicted move
// some take parameters, some take nothing

// return random move 0,1,2 using local state
let randPredict = 
    let rand = Random()
    fun _ -> rand.Next(3)

let frequencyPredict length playedHistory opponentHistory  = 
    // todo - up to given move length
    let counts = Seq.map (fun v -> Seq.length (Seq.filter(fun m -> m=v) opponentHistory)) [0..2]
    // return index of max value
    fst (counts
        |> Seq.mapi(fun i v -> i,v) // index and value
        |> Seq.maxBy snd
        )

let historyPredict paired length playedHistory opponentHistory  = 
    let matchLength (items:list<int>) start1 start2 maxLength = 
        let len = List.length items
        let rec counter curLen = 
            let i1,i2 = start1+curLen,start2+curLen
            if curLen = maxLength || i1 >= len || i2 >= len || items.[i1] <> items.[i2] then
                curLen
            else
                counter (curLen+1)
        counter 0
    let step,history = 
        match paired with 
        | false -> 1 , opponentHistory
        | true  -> 2 ,(List.zip opponentHistory playedHistory)|> List.collect (fun (a,b)-> [a;b])
    let len = List.length history// todo - match also on played history??
    if len < step*2 then
        randPredict()
    else
        // get length at each start position step by step matching to start
        let runs = Seq.mapi(fun i s -> i, matchLength history 0 s length) (seq{step..length})

        // return index of max value
        let maxIndex = step + (fst (runs |> Seq.maxBy snd))
        match maxIndex with
        | 0 -> randPredict()
        | _ -> history.[maxIndex-step]
        


//********************* Meta strategy *******************************
// convert a predictor into 6 varying strategies covering all cases
// of second guessing, and of using similar predictors against this program
let makeStrategies predictor = 
    seq {
    // direct, double, triple guessing
    yield fun hist1 hist2 -> beats.[predictor hist1 hist2]
    yield fun hist1 hist2 -> beats.[beats.[predictor hist1 hist2]]
    yield fun hist1 hist2 -> beats.[beats.[beats.[predictor hist1 hist2]]]
    // reversed
    yield fun hist2 hist1 -> beats.[predictor hist1 hist2]
    yield fun hist2 hist1 -> beats.[beats.[predictor hist1 hist2]]
    yield fun hist2 hist1 -> beats.[beats.[beats.[predictor hist1 hist2]]]
    }

//********************* Move generation *****************************

// return updated strategies, histories, and a best move
let generateMove (strategies,playedHistory:list<int>,opponentHistory) = 
    // todo - do scores over 10 20 50 length, pick then, helps change tactics faster
    let scores        = List.map(fun s -> computeScore (snd s) opponentHistory) strategies
    let moves         = List.map(fun s -> (fst s) playedHistory opponentHistory) strategies
    let newStrategies = List.map(fun (s,m) -> ((fst s),m::snd s)) (List.zip strategies moves)
    let bestMove      = snd ( (Seq.zip scores moves) |> Seq.maxBy fst )
    ((newStrategies, bestMove::playedHistory, opponentHistory),bestMove)


//********************* console interface ***************************

// make a new state: start name max seed
let startGame state (words : list<string>) = 
    {
        enemyName = words.[1];
//        maxFrame = words.[3] |> int;
        maxFrame = 200;
        rand = new Random(words.[2]|>int);
        frame = 0;
    }

// return (new state, move list to execute)
let doStep state words = 
    (state,[])

// format moves for player 1 as a string: MOVE L src dst pop ... E
let formatMoves (moveList :list<Fleet>) = 
    (List.fold(fun acc f -> acc + String.Format("L {0} {1} {2}",f.src,f.dst,f.population)) "MOVE " moveList) + " E"

// read move till end E seen
let rec readMove prefix = 
    let cur = prefix + Console.ReadLine()
    if cur.EndsWith(" E") then
        cur.ToLower()
    else
        readMove cur

let splitChars = [|' ';'\n';'\r'|]
// main game loop
let rec gameRunner state = 
    let text = readMove ""
    let words = text.Split(splitChars,System.StringSplitOptions.RemoveEmptyEntries) |> Array.toList
    let next = 
        match words with 
        | "start"   :: _ -> Some(startGame state words)
        | "state"   :: _ -> 
            let (s,moves) = doStep state words
            let formattedMoves = formatMoves moves
            Console.WriteLine(formattedMoves)        
            Some(s)
        | "result"  :: _ -> Some(state) // same as last time
        | "quit"    :: _ -> None
        | _              -> Some(state) // if not understood, do again 
    Console.Error.WriteLine("Saw {0}",text);
    match next with 
    | None           -> ()                  // done, exit program
    | Some(newState) -> gameRunner newState // recurse

[<EntryPoint>]
let main argv = 
    gameRunner {rand = new Random(1234); frame=0; maxFrame=200; enemyName="bob"} |> ignore
    1 // return value

