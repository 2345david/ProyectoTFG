using System.Collections;        // Nos deja usar corrutinas (tareas que esperan un rato antes de seguir)
using UnityEngine;
using UnityEngine.UI;             // Nos deja usar imágenes, como la barra de vida del jefe

/// <summary>
/// Maneja la pantalla del jefe: el panel y su barra de vida.
/// Otros scripts del jefe usan "BossUI.instance" para mostrar u ocultar esta pantalla.
/// </summary>
public class BossUI : MonoBehaviour
{

    public GameObject bossPanel;     // El panel que se ve cuando aparece el jefe
    public Image healthBar;          // La imagen que se va vaciando para mostrar la vida del jefe
    public Text bossNameText;        // El texto que muestra el nombre del jefe (lee enemyNam del Enemy)

    // "instance" deja que otros scripts hablen con esta pantalla desde cualquier sitio (solo hay una)
    public static BossUI instance;

    // Al despertar: se guarda como la pantalla oficial del jefe si todavía no había ninguna.
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        // Si no se asignó el texto del nombre en el Inspector, lo buscamos dentro del panel del jefe.
        if (bossNameText == null && bossPanel != null)
        {
            foreach (Text t in bossPanel.GetComponentsInChildren<Text>(true))
            {
                if (t.gameObject.name == "BossName")
                {
                    bossNameText = t;
                    break;
                }
            }
        }
    }

    // Cambia el nombre que se muestra en la pantalla del jefe. Si llega vacío, deja el que ya hubiera.
    public void SetBossName(string bossName)
    {
        if (bossNameText != null && !string.IsNullOrEmpty(bossName))
        {
            bossNameText.text = bossName;
        }
    }

    // Al destruirse: si esta era la pantalla oficial, la borra para que no quede una referencia rota.
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // Al empezar la escena: esconde el panel del jefe (todavía no ha aparecido).
    void Start()
    {
        if (bossPanel != null)
        {
            bossPanel.SetActive(false);
        }
    }

    // Muestra el panel y la barra de vida del jefe. Se llama cuando aparece el jefe.
    public void BossActivator()
    {
        if (bossPanel != null)
        {
            bossPanel.SetActive(true);
        }
    }

    // Esconde el panel del jefe y, si está activo, lanza la escena de "jefe derrotado". Se llama cuando el jefe muere.
    public void BossDeactivator()
    {
        if (bossPanel != null)
        {
            bossPanel.SetActive(false);
        }

        // Solo lanza la tarea de derrota si este objeto está encendido y visible
        if (isActiveAndEnabled && gameObject.activeInHierarchy)
        {
            StartCoroutine(BossDefeated());
        }
    }

    // Deja la pantalla del jefe como nueva: para las tareas, esconde el panel y devuelve el control al jugador. Se usa al reaparecer.
    public void ResetBossUI()
    {
        StopAllCoroutines();
        if (bossPanel != null)
        {
            bossPanel.SetActive(false);
        }
        if (PlayerController.instance != null)
        {
            PlayerController.instance.enabled = true;
        }
    }

    // Tarea de derrota: quita el control al jugador durante 5 segundos (para un momento dramático) y luego se lo devuelve.
    IEnumerator BossDefeated()
    {
        // Si no hay jugador, no hay nada que hacer
        if (PlayerController.instance == null)
        {
            yield break;
        }

        // Bloquea el control del jugador mientras dura la escena de derrota
        PlayerController.instance.enabled = false;
        yield return new WaitForSeconds(5f);
        // Devuelve el control al jugador si todavía existe
        if (PlayerController.instance != null)
        {
            PlayerController.instance.enabled = true;
        }
    }
}
