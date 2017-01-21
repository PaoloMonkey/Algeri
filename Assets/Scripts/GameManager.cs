using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public enum Actor
    {
        Meursault,
        Amante,
        Amico,
        Vittima,
        Prete,
        Avvocato,
        Infermiera,
        Guardiano,
        Prigioniero,
        Giudice,
        Uccello
    }

    public enum Location
    {
        Prigione,
        Spiaggia,
        Tribunale,
        CameraArdente
    }

    public enum Scene
    {
        Prete,
        Amante,
        Avvocato,
        Amico,
        Vittima,
        Veglia,
        Processo
    }

    [System.Serializable]
    public class SceneInfo : object
    {
        public string name;
        public Scene scene;
        public Location location;
        public CinematicInfo cinematic;
    }

    public SceneInfo[] sceneInfoArray;

    public static void PlayCinematic()
    {

    }
}
