using System;
using UnityEngine;
using Sirenix.Utilities;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum TileType
{
    None,
    Stack,
    Empty,
}

[Serializable]
public sealed class CellData
{
    [SerializeField] public TileType tileType;
    [SerializeField] public bool isMirror;
    [SerializeField] public bool isMultiProducer;
}

namespace PixelSort.Feature.GridGeneration
{
    [HideMonoScript]
    [CreateAssetMenu(fileName = "Level Position Data", menuName = "PixelSort/Level Position Data", order = 0)]
    public sealed class LevelPositionData : SerializedScriptableObject
    {
        [TitleGroup("LEVEL DATA", boldTitle: true)]
        [HorizontalGroup("LEVEL DATA/Split")]
        [VerticalGroup("LEVEL DATA/Split/Left"), LabelWidth(60)]
        [SerializeField] public int Row = 5;
        [InlineButton(nameof(MakeGrid))] 
        [VerticalGroup("LEVEL DATA/Split/Right"), LabelWidth(60)] 
        [SerializeField] public int Column = 5;

        [Space]
        [TitleGroup("GRID", boldTitle: true)]
        [TableMatrix
        (SquareCells = true, 
            HideRowIndices = false, 
            HideColumnIndices = true, 
            RespectIndentLevel = true,
            ResizableColumns = false, 
            DrawElementMethod = nameof(DrawCells))]
        [SerializeField] public CellData[,] Grid = new CellData[5,5];
        
        private void MakeGrid()
        {
#if UNITY_EDITOR
            if (EditorUtility.DisplayDialog("LEVEL DATA",
                    "Are you sure you want generate a new grid?\n\nWARNING\nGenerating a new grid will reset all current cells.",
                    "Yes", "No"))
            {
                Grid = new CellData[Column, Row];
                for (int x = 0; x < Column; x++)
                {
                    for (int y = 0; y < Row; y++)
                    {
                        Grid[x, y] = new CellData();
                    }
                }
            }
#endif
        }

        public static Color GetColor(TileType tileType) => tileType switch
        {
            TileType.None => new Color(1f, 0.3f, 0.1f, 1f),
            TileType.Stack => new Color(0.2f, 0.2f, 0.2f, 1f),
            TileType.Empty => new Color(0.4f, 0.7f, 0.5f, 1f),
            _ => Color.clear
        };

        private static CellData DrawCells(Rect rect, CellData value)
        {
            if (Application.isEditor is false)
            {
                return value;
            }

#if UNITY_EDITOR
            if (value == null)
            {
                value = new CellData();
            }
            EditorGUI.DrawRect(rect.Padding(1f), GetColor(value.tileType));
            GUILayout.BeginArea(rect);
            var style = new GUIStyle(GUI.skin.button);
            style.fontStyle = FontStyle.Bold;
            if (GUILayout.Button(value.tileType.ToString(), style))
            {
                value.tileType++;
                if ((int)value.tileType >= Enum.GetNames(typeof(TileType)).Length)
                {
                    value.tileType = 0;
                }
                GUI.changed = true;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 30;
            EditorGUILayout.EndHorizontal();
            if (value.tileType.Equals(TileType.Stack))
            {
                EditorGUIUtility.labelWidth = 50f;
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Is Mirror", GUILayout.Width(100));
                GUILayout.Space(2);
                value.isMirror = EditorGUILayout.Toggle(value.isMirror);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Is MultiProducer", GUILayout.Width(100));
                GUILayout.Space(2);
                value.isMultiProducer = EditorGUILayout.Toggle(value.isMultiProducer);
                EditorGUILayout.EndHorizontal();
            }
            else if (value.tileType.Equals(TileType.Empty))
            {
                EditorGUIUtility.labelWidth = 50f;
            }

            GUILayout.EndArea();
#endif

            return value;
        }
    }
}


