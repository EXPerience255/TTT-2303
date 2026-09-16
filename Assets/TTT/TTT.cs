using System.Collections.Generic;
using UnityEngine;

public enum PlayerOption
{
    NONE, //0
    X, // 1
    O // 2
}

public class TTT : MonoBehaviour
{
    public int Rows;
    public int Columns;
    [SerializeField] BoardView board;

    PlayerOption currentPlayer = PlayerOption.X;
    Cell[,] cells;
    int turn;

    // Start is called before the first frame update
    void Start()
    {
        cells = new Cell[Columns, Rows];

        board.InitializeBoard(Columns, Rows);

        for(int i = 0; i < Rows; i++)
        {
            for(int j = 0; j < Columns; j++)
            {
                cells[j, i] = new Cell();
                cells[j, i].current = PlayerOption.NONE;
            }
        }
    }

    public void ChooseSpace(int column, int row)
    {
        // can't choose space if game is over
        if (GetWinner() != PlayerOption.NONE)
            return;

        // can't choose a space that's already taken
        if (cells[column, row].current != PlayerOption.NONE)
            return;

        // set the cell to the player's mark
        cells[column, row].current = currentPlayer;

        // update the visual to display X or O
        board.UpdateCellVisual(column, row, currentPlayer);

        // if there's no winner, keep playing, otherwise end the game
        if(GetWinner() == PlayerOption.NONE)
            EndTurn();
        else
        {
            Debug.Log("GAME OVER!");
        }
    }

    public void EndTurn()
    {
        // increment player, if it goes over player 2, loop back to player 1
        currentPlayer += 1;
        turn++;
        if ((int)currentPlayer > 2)
            currentPlayer = PlayerOption.X;
    }

    public PlayerOption GetWinner()
    {
        // sum each row/column based on what's in each cell X = 1, O = -1, blank = 0
        // we have a winner if the sum = 3 (X) or -3 (O)
        int sum = 0;

        // check rows
        for (int i = 0; i < Rows; i++)
        {
            sum = 0;
            for (int j = 0; j < Columns; j++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;

        }

        // check columns
        for (int j = 0; j < Columns; j++)
        {
            sum = 0;
            for (int i = 0; i < Rows; i++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;

        }

        // check diagonals
        // top left to bottom right
        sum = 0;
        for(int i = 0; i < Rows; i++)
        {
            int value = 0;
            if (cells[i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        // top right to bottom left
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            int value = 0;

            if (cells[Columns - 1 - i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[Columns - 1 - i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        return PlayerOption.NONE;
    }

    // bot stuff
    public void MakeOptimalMove()
    {
        // ignore everything if the game has ended
        if (GetWinner() != PlayerOption.NONE) return;

        // the bot will only function on a standard 3x3 grid
        if (Rows != 3 || Columns != 3) return;

        Debug.Log("--- TURN " + turn + " ---");

        // the first two turns are very unqiue
        if (turn < 2) { Debug.Log("Doing special turn " + turn); DoFirstTurn(); return; }

        // try to win
        int turnType = GetTurnType(false);
        if (turnType != -1) { Debug.Log("Win turn type " + turnType); DoWinOrBlock(turnType); return; }
        Debug.Log("Unable to win. Continuing...");

        // try to block
        turnType = GetTurnType(true);
        if (turnType != -1) { Debug.Log("Block turn type " + turnType); DoWinOrBlock(turnType); return; }
        Debug.Log("No block needed. Continuing...");

        // take center if it is still open
        if (cells[1, 1].current == PlayerOption.NONE) { Debug.Log("Took center"); ChooseSpace(1, 1); return; }

        // extend corner
        if (ExtendCorner()) { Debug.Log("Extended from a corner"); return; }
        Debug.Log("Could not extend from corner. Continuing...");

        // if all else fails, pick randomly
        PickRandomFromRemaining();
        Debug.Log("Picked a random cell");
    }

    private void DoFirstTurn()
    {
        int randValue = Random.Range(0, 4);
        if (turn == 0)
        {
            switch (randValue)
            {
                case 0:
                    ChooseSpace(0, 0);
                    break;
                case 1:
                    ChooseSpace(2, 0);
                    break;
                case 2:
                    ChooseSpace(0, 2);
                    break;
                case 3:
                    ChooseSpace(2, 2);
                    break;
            }
        }
        else
        {
            if (cells[1, 1].current == PlayerOption.NONE) ChooseSpace(1, 1);
            else
            {
                switch (randValue)
                {
                    case 0:
                        ChooseSpace(0, 0);
                        break;
                    case 1:
                        ChooseSpace(2, 0);
                        break;
                    case 2:
                        ChooseSpace(0, 2);
                        break;
                    case 3:
                        ChooseSpace(2, 2);
                        break;
                }
            }
        }
    }

    private int GetTurnType(bool isBlock)
    {
        int sum;

        // check rows
        for (int i = 0; i < Rows; i++)
        {
            sum = 0;
            for (int j = 0; j < Columns; j++)
            {
                var value = 0;
                if (cells[j, i].current == currentPlayer)
                    value = 1;
                else if (cells[j, i].current != currentPlayer && cells[j, i].current != PlayerOption.NONE)
                    value = -1;

                sum += value;
            }
            if ((sum == 2 && !isBlock) || (sum == -2 && isBlock)) return i;
        }

        // check columns
        for (int j = 0; j < Columns; j++)
        {
            sum = 0;
            for (int i = 0; i < Rows; i++)
            {
                var value = 0;
                if (cells[j, i].current == currentPlayer)
                    value = 1;
                else if (cells[j, i].current != currentPlayer && cells[j, i].current != PlayerOption.NONE)
                    value = -1;

                sum += value;
            }

            if ((sum == 2 && !isBlock) || (sum == -2 && isBlock)) return j + 3;
        }

        // check diagonals
        // top left to bottom right
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            int value = 0;
            if (cells[i, i].current == currentPlayer)
                value = 1;
            else if (cells[i, i].current != currentPlayer && cells[i, i].current != PlayerOption.NONE)
                value = -1;

            sum += value;
        }

        if ((sum == 2 && !isBlock) || (sum == -2 && isBlock)) return 6;

        // top right to bottom left
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            int value = 0;

            if (cells[Columns - 1 - i, i].current == currentPlayer)
                value = 1;
            else if (cells[Columns - 1 - i, i].current != currentPlayer && cells[Columns - 1 - i, i].current != PlayerOption.NONE)
                value = -1;

            sum += value;
        }

        if ((sum == 2 && !isBlock) || (sum == -2 && isBlock)) return 7;

        return -1;
    }

    private void DoWinOrBlock(int turnType)
    {
        if (turnType < 3)
        {
            for (int i = 0; i < Columns; i++)
            {
                if (cells[i, turnType].current == PlayerOption.NONE) ChooseSpace(i, turnType);
            }
        }
        else if (turnType < 6)
        {
            for (int i = 0; i < Rows; i++)
            {
                if (cells[turnType - 3, i].current == PlayerOption.NONE) ChooseSpace(turnType - 3, i);
            }
        }
        else if (turnType == 6)
        {
            for (int i = 0; i < Rows; i++)
            {
                if (cells[i, i].current == PlayerOption.NONE) ChooseSpace(i, i);
            }
        }
        else
        {
            for (int i = 0; i < Rows; i++)
            {
                if (cells[Columns - 1 - i, i].current == PlayerOption.NONE) ChooseSpace(Columns - 1 - i, i);
            }
        }
    }

    private bool ExtendCorner()
    {
        int randValue = Random.Range(0, 2);

        if (cells[0, 0].current == currentPlayer)
        {
            if (cells[1, 0].current == PlayerOption.NONE) { ChooseSpace(1, 0); return true; }
            else if (cells[0, 1].current == PlayerOption.NONE) { ChooseSpace(0, 1); return true; }
        }
        else if (cells[2, 0].current == currentPlayer)
        {
            if (cells[2, 1].current == PlayerOption.NONE) { ChooseSpace(2, 1); return true; }
            else if (cells[1, 0].current == PlayerOption.NONE) { ChooseSpace(1, 0); return true; }
        }
        else if (cells[2, 2].current == currentPlayer)
        {
            if (cells[1, 2].current == PlayerOption.NONE) { ChooseSpace(1, 2); return true; }
            else if (cells[2, 1].current == PlayerOption.NONE) { ChooseSpace(2, 1); return true; }
        }
        else if (cells[0, 2].current == currentPlayer)
        {
            if (cells[0, 1].current == PlayerOption.NONE) { ChooseSpace(0, 1); return true; }
            else if (cells[1, 2].current == PlayerOption.NONE) { ChooseSpace(1, 2); return true; }
        }

        return false;
    }

    private void PickRandomFromRemaining()
    {
        List<int> slotsOpen = new List<int>();

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if (cells[j, i].current == PlayerOption.NONE) slotsOpen.Add(i * 3 + j);
            }
        }

        int randValue = Random.Range(0, slotsOpen.Count);
        int slotValue = slotsOpen[randValue];
        ChooseSpace(slotValue % 3, (int)Mathf.Floor(slotValue / 3));
    }
}