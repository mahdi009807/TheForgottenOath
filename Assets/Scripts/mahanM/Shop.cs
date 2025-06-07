using UnityEngine;

public class Shop : MonoBehaviour
{
    private int coinsCollected = 0;
    private int diamondsCollected = 0;

    public void AddCoins(int coin)
    {
        coinsCollected += coin;
    }

    public void AddDiamonds(int diamond)
    {
        diamondsCollected += diamond;
    }
}
