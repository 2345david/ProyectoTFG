using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Metroidvania.Gameplay;

namespace Metroidvania.Core
{
public class WorldManager : MonoBehaviour
    {
        public static WorldManager Instance { get; private set; }

        [Header("Settings")]
        public string firstBiomeScene;
        public CanvasGroup transitionFade;
        public float fadeDuration = 0.5f;

        [Header("State")]
        public string currentBiome;
        public bool isTransitioning;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator Start()
        {
            // Give other systems (like CheckpointManager) a frame to initiate their own load logic
            yield return null;

            if (!string.IsNullOrEmpty(currentBiome) || isTransitioning) yield break;

            if (!string.IsNullOrEmpty(firstBiomeScene))
            {
                yield return LoadBiome(firstBiomeScene, "");
            }
        }

        public void TravelToBiome(string biomeName, string spawnPointTag)
        {
            if (isTransitioning) return;
            StartCoroutine(LoadBiome(biomeName, spawnPointTag));
        }

        private IEnumerator LoadBiome(string biomeName, string spawnPointTag)
        {
            if (isTransitioning) yield break;
            isTransitioning = true;

            // Freeze player during transition to prevent falling while scenes unload/load
            if (PlayerController.instance != null)
            {
                PlayerController.instance.FreezePlayer();
            }

            // Fade Out
            if (transitionFade != null)
            {
                yield return StartCoroutine(Fade(1));
            }

            // Unload current if exists
            if (!string.IsNullOrEmpty(currentBiome))
            {
                yield return SceneManager.UnloadSceneAsync(currentBiome);
            }

            // Load new aditively
            AsyncOperation op = SceneManager.LoadSceneAsync(biomeName, LoadSceneMode.Additive);
            while (!op.isDone) yield return null;

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(biomeName));
            currentBiome = biomeName;

            // Position Player
            PositionPlayer(spawnPointTag);

            // Unfreeze player after loading is complete and positioned
            if (PlayerController.instance != null)
            {
                PlayerController.instance.UnfreezePlayer();
            }

            // Fade In
            if (transitionFade != null)
            {
                yield return StartCoroutine(Fade(0));
            }

            isTransitioning = false;
        }

        private void PositionPlayer(string spawnPointTag)
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
            foreach (var sp in spawnPoints)
            {
                var spComp = sp.GetComponent<SpawnPoint>();
                if (spComp != null && spComp.spawnTag == spawnPointTag)
                {
                    PlayerController.instance.transform.position = sp.transform.position;
                    // Reset camera immediately
                    return;
                }
            }
        }

        private IEnumerator Fade(float targetAlpha)
        {
            float startAlpha = transitionFade.alpha;
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                transitionFade.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
                yield return null;
            }
            transitionFade.alpha = targetAlpha;
        }
    }
}
