using UnityEngine;

public static class GameSettings
{
    public static bool IsTwoPlayerMode = false;
    public static int SelectedCharacter = 0;

    // ✅ CORREGIDO: Propiedad para compatibilidad con código existente
    public static bool IsSinglePlayer
    {
        get { return !IsTwoPlayerMode; }
        set { IsTwoPlayerMode = !value; }
    }

    public static void SetTwoPlayerMode(bool twoPlayer)
    {
        IsTwoPlayerMode = twoPlayer;
        Debug.Log($"🎮 GameSettings: Modo {(twoPlayer ? "2 Jugadores" : "1 Jugador")}");
    }

    public static void SetSinglePlayerMode(bool singlePlayer)
    {
        IsTwoPlayerMode = !singlePlayer;
        Debug.Log($"🎮 GameSettings: Modo {(singlePlayer ? "1 Jugador" : "2 Jugadores")}");
    }

    public static void SetSelectedCharacter(int characterIndex)
    {
        SelectedCharacter = characterIndex;
        Debug.Log($"🎮 GameSettings: Personaje seleccionado {characterIndex}");
    }
}