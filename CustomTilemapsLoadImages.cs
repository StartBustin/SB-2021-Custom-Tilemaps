using MelonLoader;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomTilemaps
{
    class CustomTilemapsLoadImages : MonoBehaviour
    {
        void Awake() 
        {
            MelonLogger.Msg("Yo, I'm awake. Start the Coroutine!");
            StartCoroutine(IHaveNoIdeaWhatToCallThis());
        }

        IEnumerator IHaveNoIdeaWhatToCallThis()
        {
            MelonLogger.Msg("Carnival Part 1");
            

            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + @"D:\Games\SAGE 2021\Starbuster Demo 2021 V1.07\union.png"))
            {
                yield return uwr.SendWebRequest();

                MelonLogger.Msg("Carnival Part 2.");
                if (uwr.isHttpError)
                {
                    MelonLogger.Msg(uwr.error);
                }
                else
                {
                    // Get downloaded asset bundle
                    CustomTilemaps.carnivalIsLoaded = true;
                    CustomTilemaps.carnivalTexture = DownloadHandlerTexture.GetContent(uwr);
                    CustomTilemaps.carnivalTexture.filterMode = FilterMode.Point;
                }
                Destroy(this);
            }
        }

        IEnumerator IHaveNoIdeaWhatToCallThisOld()
        {
            MelonLogger.Msg("Carnival Part 1");
            WWW www = new WWW("file://" + @"D:\Games\SAGE 2021\Starbuster Demo 2021 V1.06 - Modded\union.png");
            while (!www.isDone)
                yield return null;

            CustomTilemaps.carnivalIsLoaded = true;
            CustomTilemaps.carnivalTexture = www.texture;
            CustomTilemaps.carnivalTexture.filterMode = FilterMode.Point;
            MelonLogger.Msg("Carnival Part 2.");
            Destroy(this);
        }
    }
}
