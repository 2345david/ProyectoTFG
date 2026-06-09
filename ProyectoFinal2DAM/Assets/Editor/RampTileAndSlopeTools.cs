// Todo el archivo es código de Editor: solo se compila dentro del editor de Unity.
#if UNITY_EDITOR
using System.Text;          // StringBuilder (texto de ayuda multilínea para la consola).
using UnityEditor;          // API del editor (MenuItem, Selection, SerializedObject...).
using UnityEngine;          // Debug.Log y tipos base.

/// <summary>
/// Herramientas de Editor para arreglar rampas/diagonales en tiles. El aspecto "escalón"
/// suele venir del tipo de collider del tile (Grid = una caja cuadrada por celda).
/// Permite cambiar el collider de los tiles seleccionados a tipo Sprite (que sigue la
/// forma física del sprite) y muestra ayuda en consola. Menú: <c>Tools/Proyecto Final/Rampas/…</c>.
/// </summary>
public static class RampTileAndSlopeTools
{
    // Prefijo común de las entradas de menú de esta herramienta.
    private const string MenuRoot = "Tools/Proyecto Final/Rampas/";

    /// <summary>
    /// Recorre los assets seleccionados en el Project y cambia su tipo de collider a Sprite,
    /// para que la forma física siga el contorno del sprite en vez de una caja por celda.
    /// </summary>
    [MenuItem(MenuRoot + "Tiles seleccionados: collider basado en Sprite (recomendado para rampas)")]
    public static void SetSelectedTilesToSpriteCollider()
    {
        // Contador de assets realmente modificados (para el mensaje final).
        var changed = 0;
        foreach (var obj in Selection.objects)
        {
            // Solo interesan assets persistentes en disco (ignora objetos de escena/nulos).
            if (obj == null || !EditorUtility.IsPersistent(obj))
            {
                continue;
            }

            // Intenta poner su collider en modo Sprite; cuenta si hubo cambio.
            if (TrySetColliderTypeEnumToSprite(obj))
            {
                changed++;
            }
        }

        // Persiste los cambios en disco e informa al usuario del resultado.
        AssetDatabase.SaveAssets();
        Debug.Log(
            $"Rampas: collider tipo Sprite aplicado en {changed} asset(s). " +
            "Edita la forma física en el Sprite Editor (Custom Physics Shape) para cada sprite de rampa.",
            null);
    }

    /// <summary>
    /// Imprime en la consola una guía paso a paso sobre por qué las diagonales se ven como
    /// escalones y cómo conseguir pendientes lisas en tilemaps 2D.
    /// </summary>
    [MenuItem(MenuRoot + "Ayuda: rampas y diagonales (consola)")]
    public static void LogRampHelp()
    {
        // Se compone el texto línea a línea y se vuelca de una vez a la consola.
        var sb = new StringBuilder();
        sb.AppendLine("=== Rampas en Tilemap 2D ===");
        sb.AppendLine("1) Cada celda con Collider Type = GRID genera un cuadrado alineado a la rejilla → las diagonales parecen escalones.");
        sb.AppendLine("2) Cambia a SPRITE (menú de arriba o en el asset del Tile): el collider sigue el 'Physics Shape' del sprite.");
        sb.AppendLine("3) Abre el sprite del tile → Sprite Editor → Custom Physics Shape: dibuja un triángulo o trapecio que recorte la rampa (no uses el rectángulo completo si quieres pendiente lisa).");
        sb.AppendLine("4) Con TM_Ground + CompositeCollider2D, los polígonos de tiles vecinos se fusionan: si cada tile tiene corte diagonal, el borde exterior puede quedar una pendiente continua.");
        sb.AppendLine("5) Rampas largas/curvas: ya tienes el paquete 2D Sprite Shape (Spline + collider al borde). Úsalo como objeto aparte en la misma capa Ground o como hijo del Grid.");
        Debug.Log(sb.ToString());
    }

    /// <summary>
    /// Intenta poner en modo "Sprite" la opción de forma de choque de un azulejo. Primero
    /// prueba con los nombres habituales de esa opción y, si no los encuentra, revisa todas
    /// las opciones del azulejo buscando una de tipo lista cuyo nombre tenga "collider" y "type".
    /// </summary>
    /// <returns>Verdadero si cambió algo; falso si no.</returns>
    private static bool TrySetColliderTypeEnumToSprite(Object obj)
    {
        var so = new SerializedObject(obj);
        var applied = false;

        // 1) Camino rápido: probamos con los nombres más típicos de esa opción.
        foreach (var path in new[] { "m_ColliderType", "m_DefaultColliderType" })
        {
            var p = so.FindProperty(path);
            if (p != null && p.propertyType == SerializedPropertyType.Enum && TrySetEnumToSprite(p))
            {
                applied = true;
            }
        }

        // 2) Plan B: si no la encontramos por nombre, revisamos una a una todas las opciones.
        if (!applied)
        {
            var it = so.GetIterator();
            var enter = true;
            while (it.NextVisible(enter))
            {
                enter = false;
                // Solo nos interesan las opciones que son una lista de valores a elegir.
                if (it.propertyType != SerializedPropertyType.Enum)
                {
                    continue;
                }

                // Su nombre tiene que contener "collider"...
                if (it.name.IndexOf("collider", System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                // ...y también "type", para asegurarnos de que es la opción que buscamos.
                if (it.name.IndexOf("type", System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (TrySetEnumToSprite(it))
                {
                    applied = true;
                }
            }
        }

        // Solo guardamos en disco si de verdad se cambió algo.
        if (applied)
        {
            so.ApplyModifiedProperties();
        }

        return applied;
    }

    /// <summary>
    /// Recibe una opción de tipo lista, busca dentro la opción llamada "Sprite" y la elige.
    /// </summary>
    /// <returns>Verdadero si la cambió; falso si ya estaba en "Sprite" o no existe esa opción.</returns>
    private static bool TrySetEnumToSprite(SerializedProperty enumProp)
    {
        for (var i = 0; i < enumProp.enumNames.Length; i++)
        {
            // Buscamos la opción que se llama exactamente "Sprite".
            if (enumProp.enumNames[i] != "Sprite")
            {
                continue;
            }

            // Si ya estaba en "Sprite", no hay nada que cambiar.
            if (enumProp.enumValueIndex == i)
            {
                return false;
            }

            // Elegimos "Sprite" y avisamos de que hubo cambio.
            enumProp.enumValueIndex = i;
            return true;
        }

        return false;
    }
}
#endif
