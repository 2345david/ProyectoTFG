using UnityEngine;
using Metroidvania.Gameplay;

namespace EnemyScript
{
    /// <summary>
    /// Maneja los enemigos de una habitación. Cuando el jugador entra, puede crear de nuevo
    /// a los enemigos; y cuando sale, puede borrarlos. Para saber cuándo el jugador entra o
    /// sale, "escucha" los avisos OnEnter/OnExit de la habitación (RoomController).
    /// </summary>
    public class RoomEnemyManager : MonoBehaviour
    {
        [Header("Prefab de Enemigos")]
        [Tooltip("Un GameObject (o Prefab) que contenga todos los enemigos de la habitación.")]
        public GameObject enemiesContainerPrefab;

        [Header("Configuración")]
        public bool destroyEnemiesOnExit = true;
        public bool respawnOnEnter = true;

        [Header("Posicionamiento")]
        [Tooltip("Si está asignado, usa la posición de este objeto para el spawn. Si no, usa el offset.")]
        public Transform customSpawnPoint;
        [Tooltip("Coordenadas locales (offset) respecto al centro de la habitación.")]
        public Vector3 spawnOffset = Vector3.zero;

        // La habitación a la que pertenece este gestor (está en el mismo objeto).
        private RoomController _room;
        // El grupo de enemigos que tenemos creado ahora mismo en la habitación.
        private GameObject _currentEnemiesInstance;

        // Al despertar, guardamos la referencia a la habitación de este objeto.
        private void Awake()
        {
            _room = GetComponent<RoomController>();
        }

        // Al activarse, empezamos a "escuchar" los avisos de entrar y salir de la habitación.
        private void OnEnable()
        {
            if (_room != null)
            {
                _room.OnEnter += HandleRoomEnter;
                _room.OnExit += HandleRoomExit;
            }
        }

        // Al desactivarse, dejamos de escuchar esos avisos (para evitar errores).
        private void OnDisable()
        {
            if (_room != null)
            {
                _room.OnEnter -= HandleRoomEnter;
                _room.OnExit -= HandleRoomExit;
            }
        }

        // El jugador entra: si está configurado, (re)genera los enemigos.
        private void HandleRoomEnter()
        {
            if (respawnOnEnter)
            {
                RespawnEnemies();
            }
        }

        // El jugador sale: si está configurado, destruye los enemigos actuales.
        private void HandleRoomExit()
        {
            if (destroyEnemiesOnExit && _currentEnemiesInstance != null)
            {
                Destroy(_currentEnemiesInstance);
                _currentEnemiesInstance = null;
            }
        }

        /// <summary>Borra los enemigos anteriores (si los hay) y crea un grupo nuevo de enemigos.</summary>
        public void RespawnEnemies()
        {
            // Si ya teníamos enemigos creados, primero los borramos.
            if (_currentEnemiesInstance != null)
            {
                Destroy(_currentEnemiesInstance);
            }

            if (enemiesContainerPrefab != null)
            {
                // Creamos el grupo de enemigos colgándolo de la habitación, así sus posiciones
                // se miden tomando como referencia el centro de la habitación.
                _currentEnemiesInstance = Instantiate(enemiesContainerPrefab, transform);
                
                // Lo colocamos en el punto indicado, o usando el desplazamiento (offset) si no hay punto.
                if (customSpawnPoint != null)
                {
                    _currentEnemiesInstance.transform.position = customSpawnPoint.position;
                    _currentEnemiesInstance.transform.rotation = customSpawnPoint.rotation;
                }
                else
                {
                    _currentEnemiesInstance.transform.localPosition = spawnOffset;
                    _currentEnemiesInstance.transform.localRotation = Quaternion.identity;
                }
                
                _currentEnemiesInstance.name = "[Enemies] " + (_room != null ? _room.roomName : name);
            }
        }
    }
}