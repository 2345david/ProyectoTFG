using UnityEngine;
using Metroidvania.Gameplay;

namespace EnemyScript
{
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

        private RoomController _room;
        private GameObject _currentEnemiesInstance;

        private void Awake()
        {
            _room = GetComponent<RoomController>();
        }

        private void OnEnable()
        {
            if (_room != null)
            {
                _room.OnEnter += HandleRoomEnter;
                _room.OnExit += HandleRoomExit;
            }
        }

        private void OnDisable()
        {
            if (_room != null)
            {
                _room.OnEnter -= HandleRoomEnter;
                _room.OnExit -= HandleRoomExit;
            }
        }

        private void HandleRoomEnter()
        {
            if (respawnOnEnter)
            {
                RespawnEnemies();
            }
        }

        private void HandleRoomExit()
        {
            if (destroyEnemiesOnExit && _currentEnemiesInstance != null)
            {
                Destroy(_currentEnemiesInstance);
                _currentEnemiesInstance = null;
            }
        }

        public void RespawnEnemies()
        {
            if (_currentEnemiesInstance != null)
            {
                Destroy(_currentEnemiesInstance);
            }

            if (enemiesContainerPrefab != null)
            {
                // Instanciamos el prefab como hijo de la habitación.
                // Al usar 'transform' como padre, las coordenadas del prefab se vuelven RELATIVAS a la habitación.
                _currentEnemiesInstance = Instantiate(enemiesContainerPrefab, transform);
                
                // Posicionamos el contenedor según las coordenadas especificadas o el punto de spawn.
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