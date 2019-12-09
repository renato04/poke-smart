module Pokemon


type Pokemon = {
    Number : single
    Name : string
    Type1: string
    Type2 : string
    ConvertedType1:single
    ConvertedType2:single
    Total: single
    HP : single
    Attack : single
    Defense : single
    SpAttack : single
    SpDefense : single
    Speed : single
}  

let typeToSingle pkmnType = 
    match pkmnType with
    |"Bug" -> 0
    |"Dark"-> 1
    |"Dragon"-> 2
    |"Electric"-> 3
    |"Fairy"-> 4
    |"Fighting"-> 5
    |"Fire"-> 6
    |"Flying"-> 7
    |"Ghost"-> 8
    |"Grass"-> 9
    |"Ground"-> 10
    |"Ice"-> 11
    |"Normal"-> 12
    |"Poison"-> 13
    |"Psychic"-> 14
    |"Rock"-> 15
    |"Stell"-> 16
    |"Water"-> 17
    | _ -> -1

    |> single

