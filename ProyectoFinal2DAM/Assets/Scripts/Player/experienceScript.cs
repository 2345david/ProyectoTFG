using UnityEngine;
using UnityEngine.UI; // Sirve para usar la barra de experiencia (Image) y el texto del nivel (Text).

/// <summary>
/// Este script lleva la experiencia y el nivel del jugador.
/// Va sumando experiencia y, cuando llega a la cantidad necesaria, sube de nivel,
/// mejora al jugador (más vida y más daño) y actualiza lo que se ve en la pantalla.
/// </summary>
public class ExperienceScript : MonoBehaviour
{
    // La barra de experiencia que se ve en la pantalla.
    public Image expImage;
    // El texto que muestra en qué nivel está el jugador.
    public Text crrentLevelText;
    // Experiencia que el jugador lleva acumulada en el nivel actual.
    public  float currentExperience;
    // Experiencia que hace falta para subir al siguiente nivel.
    public float expTNL;
    // Número del nivel en el que está el jugador ahora.
    public int currentLevel;

    // "instance" es un atajo para que otros scripts puedan dar experiencia desde cualquier sitio.
    public static ExperienceScript instance;

    // Esto se ejecuta al crearse el objeto. Guardamos el atajo "instance" para que otros lo encuentren.
    private void Awake()
    {
        // Solo guardamos el atajo si no había uno antes, para que no haya dos copias.
        if(instance == null)
        {
            instance = this;
        }
    }

    // Esto se ejecuta una vez al empezar. Pone valores iniciales y muestra el nivel y la barra.
    void Start()
    {
        // Si no se cargó una partida guardada, empezamos en el nivel 1 y con 100 de experiencia necesaria.
        if (currentLevel <= 0) currentLevel = 1;
        if (expTNL <= 0) expTNL = 100f; // Cantidad inicial necesaria para subir de nivel.

        // Escribimos el nivel en la pantalla.
        crrentLevelText.text = currentLevel.ToString();
        // Llenamos la barra según la experiencia que tengamos (si expTNL es 0, la dejamos vacía para no dividir entre 0).
        expImage.fillAmount = expTNL > 0 ? currentExperience / expTNL : 0;
    }


    // Suma experiencia al jugador. Si llega a lo necesario, lo hace subir de nivel (incluso varios de golpe).
    public void expModifer(float experience)
    {
        // Añadimos la experiencia que acabamos de ganar.
        currentExperience += experience;

        // Mientras tengamos experiencia de sobra para subir, seguimos subiendo de nivel.
        while (currentExperience >= expTNL)
        {
            // Le quitamos lo que costó este nivel y guardamos lo que sobra para el siguiente.
            currentExperience -= expTNL;
            
            // El próximo nivel será más difícil: pedimos un 30% más de experiencia.
            expTNL = Mathf.Floor(expTNL * 1.3f);
            
            // Mejoramos al jugador poco a poco con cada nivel.
            PlayerHealth.instance.maxHealth += 10f; // Le damos 10 de vida máxima más.
            PlayerAttack.instance.damage += 1f;     // Le damos 1 de daño más.
            
            // Le curamos toda la vida como premio por subir de nivel.
            PlayerHealth.instance.health = PlayerHealth.instance.maxHealth;

            // Subimos el número del nivel.
            currentLevel++;
            
            // Sonido de "subiste de nivel" (si está disponible).
            if (AudioManager.instance != null && AudioManager.instance.LvlUp != null)
                AudioManager.instance.PlayAudio(AudioManager.instance.LvlUp);

            Debug.Log($"Level Up! Now Level {currentLevel}. Next level needs {expTNL} XP.");
        }

        // Cuando terminamos, actualizamos lo que se ve en la pantalla.
        UpdateUI();
    }

    // Actualiza lo que se ve en la pantalla: el número del nivel y lo llena que está la barra de experiencia.
    public void UpdateUI()
    {
        // Si el texto del nivel está puesto, escribimos el nivel actual.
        if (crrentLevelText != null)
        {
            crrentLevelText.text = currentLevel.ToString();
        }

        // Si la barra está puesta, la llenamos según la experiencia (sin dividir entre 0).
        if (expImage != null)
        {
            expImage.fillAmount = expTNL > 0 ? currentExperience / expTNL : 0;
        }
    }


}
