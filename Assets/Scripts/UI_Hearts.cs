using UnityEngine;
using UnityEngine.UI;

public class UIHearts : MonoBehaviour
{
    [SerializeField] private Image[] hearts;   
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    public void UpdateHearts(int currentLives)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < currentLives ? fullHeart : emptyHeart;
        }
    }
}

