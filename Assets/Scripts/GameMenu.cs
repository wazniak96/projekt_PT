using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public void OpenInstruction()
    {
        Application.OpenURL("http://inicjatywab.pl/wp-content/uploads/2019/01/Eksploduj%C4%85ce-kotki-Instrukcja.pdf");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
