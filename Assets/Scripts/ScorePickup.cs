using UnityEngine;
public class ScorePickup : Pickup
{
    public int amount = 150;

    public override void Activate()
    {
        base.Activate(); // Calls the Activate() in the base class (destroys the object)

        if (game != null)
        {
            game.AddScore(amount); // Pass 'amount' here
        }
        else
        {
            Debug.Log("GameManager (game) is null when trying to add score!");
        }
    }
}