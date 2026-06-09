using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Metroidvania.Gameplay;

namespace Metroidvania.Core
{
    /// <summary>
    /// Es el "jefe" del mundo del juego. Se encarga de cambiar de zona (bioma), de hacer
    /// que la pantalla se oscurezca y se aclare al cambiar (fundido), de poner el fondo
    /// correcto y de colocar al jugador en el sitio donde debe aparecer.
    /// Solo existe uno y no se borra al cambiar de escena.
    /// </summary>
    public class WorldManager : MonoBehaviour
    {
        // El único WorldManager que existe; se puede usar desde cualquier parte del juego.
        public static WorldManager Instance { get; private set; }

        [Header("Settings")]
        public string firstBiomeScene;        // La primera zona que se carga al empezar a jugar.
        public CanvasGroup transitionFade;    // Pantalla negra que usamos para oscurecer y aclarar.
        public float fadeDuration = 0.5f;     // Cuántos segundos tarda en oscurecerse o aclararse.

        [Header("State")]
        public string currentBiome;           // Nombre de la zona que está cargada ahora mismo.
        public bool isTransitioning;          // Es true mientras estamos cambiando de zona.

        [Header("Background Settings")]
        public SpriteRenderer globalBackground;         // El dibujo del fondo que se ve detrás del juego.
        public List<BiomeBackground> biomeBackgrounds;  // Qué fondo le toca a cada zona.

        /// <summary>
        /// Pequeña ficha que une el nombre de una zona con el dibujo de fondo y su tamaño.
        /// </summary>
        [System.Serializable]
        public struct BiomeBackground
        {
            public string sceneName;        // Nombre de la zona.
            public Sprite backgroundSprite; // El dibujo de fondo para esa zona.
            public Vector3 scale;           // El tamaño que tendrá el fondo (si no es cero).
        }

        // Se ejecuta al nacer: deja a este como el único WorldManager y hace que no se borre nunca.
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // Hace que este objeto siga existiendo aunque cambiemos de escena.
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // Si ya había uno, este sobra y lo borramos.
                Destroy(gameObject);
            }
        }

        // Se ejecuta al empezar: espera un poquito y luego carga la primera zona del juego.
        private IEnumerator Start()
        {
            // Esperamos un fotograma para dar tiempo a que otros scripts (como el de guardado) arranquen.
            yield return null;

            // Si ya hay una zona cargada o estamos cambiando de zona, no hacemos nada.
            if (!string.IsNullOrEmpty(currentBiome) || isTransitioning) yield break;

            // Si hay una primera zona configurada, la cargamos.
            if (!string.IsNullOrEmpty(firstBiomeScene))
            {
                yield return LoadBiome(firstBiomeScene, "");
            }
        }

        // Pide viajar a otra zona y deja al jugador en el punto de aparición indicado.
        public void TravelToBiome(string biomeName, string spawnPointTag)
        {
            // Si ya estamos en mitad de un cambio de zona, ignoramos la petición.
            if (isTransitioning) return;
            StartCoroutine(LoadBiome(biomeName, spawnPointTag));
        }

        /// <summary>
        /// Hace todo el cambio de zona paso a paso: bloquea al jugador, oscurece la pantalla,
        /// quita la zona vieja, carga la nueva, coloca al jugador y vuelve a aclarar la pantalla.
        /// </summary>
        private IEnumerator LoadBiome(string biomeName, string spawnPointTag)
        {
            // Si ya estamos cambiando de zona, no empezamos otro cambio encima.
            if (isTransitioning) yield break;
            isTransitioning = true;

            // Bloqueamos al jugador para que no se mueva ni se caiga mientras cargamos las zonas.
            if (PlayerController.instance != null)
            {
                PlayerController.instance.FreezePlayer();
            }

            // Oscurecemos la pantalla hasta ponerla negra.
            if (transitionFade != null)
            {
                yield return StartCoroutine(Fade(1));
            }

            // Quitamos (descargamos) la zona que estaba puesta, si había alguna.
            if (!string.IsNullOrEmpty(currentBiome))
            {
                yield return SceneManager.UnloadSceneAsync(currentBiome);
            }

            // Cargamos la zona nueva sin quitar las demás escenas que ya estaban.
            AsyncOperation op = SceneManager.LoadSceneAsync(biomeName, LoadSceneMode.Additive);
            // Si la zona no existe o no está en la lista del juego, "op" será null.
            // Sin esta comprobación, el juego daría error al intentar usar "op".
            if (op == null)
            {
                Debug.LogError($"WorldManager: no se pudo cargar la escena '{biomeName}'. " +
                    "Comprueba que esté añadida en File > Build Profiles (Scene List) y que el nombre coincida.");
                // Deshacemos el bloqueo para no dejar el juego atascado en el cambio de zona.
                isTransitioning = false;
                if (PlayerController.instance != null) PlayerController.instance.UnfreezePlayer();
                if (transitionFade != null) yield return StartCoroutine(Fade(0));
                yield break;
            }
            // Esperamos a que la zona termine de cargarse del todo.
            while (!op.isDone) yield return null;

            // Marcamos la zona nueva como la escena principal y apuntamos su nombre.
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(biomeName));
            currentBiome = biomeName;

            // Ponemos el fondo que le corresponde a esta zona.
            UpdateBackground(biomeName);

            // Colocamos al jugador en el punto de aparición que toca.
            PositionPlayer(spawnPointTag);

            // Ya está todo listo: desbloqueamos al jugador para que pueda moverse.
            if (PlayerController.instance != null)
            {
                PlayerController.instance.UnfreezePlayer();
            }

            // Aclaramos la pantalla otra vez (quitamos el negro).
            if (transitionFade != null)
            {
                yield return StartCoroutine(Fade(0));
            }

            // Terminó el cambio de zona.
            isTransitioning = false;
        }

        // Cambia el dibujo y el tamaño del fondo según la zona que se haya cargado.
        private void UpdateBackground(string biomeName)
        {
            // Si no hay fondo asignado, no hay nada que cambiar.
            if (globalBackground == null) return;

            // Recorremos la lista de fondos hasta encontrar el de esta zona y lo aplicamos.
            foreach (var mapping in biomeBackgrounds)
            {
                if (mapping.sceneName == biomeName)
                {
                    globalBackground.sprite = mapping.backgroundSprite;
                    // Solo cambiamos el tamaño si se ha indicado uno (distinto de cero).
                    if (mapping.scale != Vector3.zero)
                    {
                        globalBackground.transform.localScale = mapping.scale;
                    }
                    return;
                }
            }
        }

        // Busca en la zona el punto de aparición correcto y coloca allí al jugador.
        private void PositionPlayer(string spawnPointTag)
        {
            // Buscamos todos los objetos marcados como "SpawnPoint" (puntos de aparición).
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
            foreach (var sp in spawnPoints)
            {
                var spComp = sp.GetComponent<SpawnPoint>();
                // Colocamos al jugador en el primer punto cuya etiqueta coincida con la pedida.
                if (spComp != null && spComp.spawnTag == spawnPointTag)
                {
                    PlayerController.instance.transform.position = sp.transform.position;
                    return;
                }
            }
        }

        // Oscurece o aclara la pantalla poco a poco hasta llegar al nivel indicado (targetAlpha).
        private IEnumerator Fade(float targetAlpha)
        {
            // Guardamos lo oscura que está la pantalla ahora y ponemos un cronómetro a cero.
            float startAlpha = transitionFade.alpha;
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                // Vamos cambiando la oscuridad poco a poco según pasa el tiempo.
                transitionFade.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
                yield return null;
            }
            // Al final dejamos el valor exacto que queríamos.
            transitionFade.alpha = targetAlpha;
        }
    }
}
