#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Herramientas para rampas: el aspecto "escalón" suele venir del tipo de collider del tile (Grid = caja por celda).
/// </summary>
public static class RampTileAndSlopeTools
{
    private const string MenuRoot = "Tools/Proyecto Final/Rampas/";

    [MenuItem(MenuRoot + "Tiles seleccionados: collider basado en Sprite (recomendado para rampas)")]
    public static void SetSelectedTilesToSpriteCollider()
    {
        var changed = 0;
        foreach (var obj in Selection.objects)
        {
            if (obj == null || !EditorUtility.IsPersistent(obj))
            {
                continue;
            }

            if (TrySetColliderTypeEnumToSprite(obj))
            {
                changed++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            $"Rampas: collider tipo Sprite aplicado en {changed} asset(s). " +
            "Edita la forma física en el Sprite Editor (Custom Physics Shape) para cada sprite de rampa.",
            null);
    }

    [MenuItem(MenuRoot + "Ayuda: rampas y diagonales (consola)")]
    public static void LogRampHelp()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Rampas en Tilemap 2D ===");
        sb.AppendLine("1) Cada celda con Collider Type = GRID genera un cuadrado alineado a la rejilla → las diagonales parecen escalones.");
        sb.AppendLine("2) Cambia a SPRITE (menú de arriba o en el asset del Tile): el collider sigue el 'Physics Shape' del sprite.");
        sb.AppendLine("3) Abre el sprite del tile → Sprite Editor → Custom Physics Shape: dibuja un triángulo o trapecio que recorte la rampa (no uses el rectángulo completo si quieres pendiente lisa).");
        sb.AppendLine("4) Con TM_Ground + CompositeCollider2D, los polígonos de tiles vecinos se fusionan: si cada tile tiene corte diagonal, el borde exterior puede quedar una pendiente continua.");
        sb.AppendLine("5) Rampas largas/curvas: ya tienes el paquete 2D Sprite Shape (Spline + collider al borde). Úsalo como objeto aparte en la misma capa Ground o como hijo del Grid.");
        Debug.Log(sb.ToString());
    }

    private static bool TrySetColliderTypeEnumToSprite(Object obj)
    {
        var so = new SerializedObject(obj);
        var applied = false;

        foreach (var path in new[] { "m_ColliderType", "m_DefaultColliderType" })
        {
            var p = so.FindProperty(path);
            if (p != null && p.propertyType == SerializedPropertyType.Enum && TrySetEnumToSprite(p))
            {
                applied = true;
            }
        }

        if (!applied)
        {
            var it = so.GetIterator();
            var enter = true;
            while (it.NextVisible(enter))
            {
                enter = false;
                if (it.propertyType != SerializedPropertyType.Enum)
                {
                    continue;
                }

                if (it.name.IndexOf("collider", System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

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

        if (applied)
        {
            so.ApplyModifiedProperties();
        }

        return applied;
    }

    private static bool TrySetEnumToSprite(SerializedProperty enumProp)
    {
        for (var i = 0; i < enumProp.enumNames.Length; i++)
        {
            if (enumProp.enumNames[i] != "Sprite")
            {
                continue;
            }

            if (enumProp.enumValueIndex == i)
            {
                return false;
            }

            enumProp.enumValueIndex = i;
            return true;
        }

        return false;
    }
}
#endif
