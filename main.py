import sys


rows = 5
column = 5
playerpos = [0, 0]
exitpos = [4, 4]
movesLeft = 10
startingValue = 5
currentValue = startingValue
currentEquation = str(startingValue)
gameWon = False
operations = ('+', '-', "*", "/")
board = [[0 for _ in range(rows)] for _ in range(column)]
def printGameState():
    for i in range(rows):
        if(i == rows-1):
            print(board[i], "<--- exit here")
        else:
            print(board[i])
    print("win by reaching the exit with 25")
    print("WASD to move")
    print("Moves Left: ", movesLeft)
    print("Current Equation: ", currentEquation)
    return

def initializeGame():
    board[0][0] = "O"# type: ignore
    board[0][2] = "*" # type: ignore
    board[0][4] = 5
    target = 25
    printGameState()
    while(not gameWon):
        move()
        if(movesLeft <= 0):
            print("You lost")
            return
    print("YOU WON!")
    return

def EvaluateEquation():
    global currentValue
    equation = currentEquation.split()
    currentOperand = ""
    for component in equation:
        if(component in operations):
            currentOperand = component
        else:
            match(currentOperand):
                case "":
                    continue
                case "*":
                    currentValue *= int(component)
                case "+":
                    currentValue += int(component)
                case "/":
                    currentValue /= int(component)
                case "-":
                    currentValue -= int(component)
    return 
                    

def proccessMove(locValue):
    global currentEquation
    global movesLeft
    global playerpos
    global gameWon
    if str(locValue) == str(exitpos):
        EvaluateEquation()
        print(currentValue)
        if currentValue == 25:
            gameWon = True
            board[locValue[0]][locValue[1]] = "O"
            board[playerpos[0]][playerpos[1]] = 0
            playerpos = [locValue[0], locValue[1]]
            printGameState()
            printGameState()
            return
    if locValue[0] < 0 or locValue[0] > column-1 or locValue[1] < 0 or locValue[1] > rows-1:
        return
    if(currentEquation.endswith(operations)):
        if(board[locValue[0]][locValue[1]] in operations):
            print("You can't move there, finish this operation first")
            return
    
    if str(board[locValue[0]][locValue[1]]) != '0':
        currentEquation += " " + str(board[locValue[0]][locValue[1]])
    movesLeft -= 1
    board[locValue[0]][locValue[1]] = "O"
    board[playerpos[0]][playerpos[1]] = 0
    playerpos = [locValue[0], locValue[1]]
    printGameState()
        
def move():
    direction = input()
    match direction:
        case 'w':
            proccessMove([playerpos[0]-1, playerpos[1]])
        case 'a':
            proccessMove([playerpos[0], playerpos[1]-1])
        case 's':
            proccessMove([playerpos[0]+1, playerpos[1]])
        case 'd':
            proccessMove([playerpos[0], playerpos[1]+1])
    return

initializeGame()

