using TMPro;
using UnityEngine;

public class ButtonCell : MonoBehaviour
{
    public int row;
    public int column;

    public TicTacToeManager ticTacToeManager;

    private void OnMouseDown()
    {
        this.ticTacToeManager.HandlePlayerMove(this.row, this.column);
    }

    public void SetSymbol(string symbol)
    {
        GetComponentInChildren<TextMeshProUGUI>().text = symbol;
    }
}
