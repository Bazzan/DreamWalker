using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SLoadScene : MonoBehaviour
{

    public float timeUntilLoad;

    public string sceneName;

    public bool unlockMouseOnLoad;
    public bool stopMusicOnLoad;

    public void EnableTimer()
    {
        StartCoroutine(LoadFirstScene());
    }

    public IEnumerator LoadFirstScene()
    {
        yield return new WaitForSeconds(timeUntilLoad);
        SceneManager.LoadScene(sceneName);
    }

    public void LoadScene()
    {
        if (unlockMouseOnLoad)
            Cursor.lockState = CursorLockMode.None;

        if (stopMusicOnLoad)
        {
            var emitters = FindObjectsOfType<StudioEventEmitter>();
            foreach (StudioEventEmitter emitter in emitters)
            {
                emitter.EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }


        SceneManager.LoadScene(sceneName);
    }
}
