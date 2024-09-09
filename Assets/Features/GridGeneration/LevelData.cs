using System;
using System.Collections.Generic;
using PixelSort.LevelData;
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
}

[System.Serializable]
public sealed class CellData
{
    [SerializeField] public TileType tileType;
    [SerializeField] public ChipStackData ChipStackData;
    // [Title("Stack Data List")] [ListDrawerSettings(Expanded = true)] 
    // [SerializeField] public List<ChipStack> stackDataList = new List<ChipStack>();
}

namespace PixelSort.LevelData
{
    [HideMonoScript]
    [CreateAssetMenu(fileName = "Level Data", menuName = "PixelSort/Level Data", order = 0)]
    public sealed class LevelData : SerializedScriptableObject
    {
        //===================================================
        // FIELDS
        //===================================================
        [TitleGroup("LEVEL DATA", boldTitle: true)]
        [HorizontalGroup("LEVEL DATA/Split")]
        [VerticalGroup("LEVEL DATA/Split/Left"), LabelWidth(60)]
        [SerializeField] public int Row = 5;
        [InlineButton(nameof(MakeGrid))] [VerticalGroup("LEVEL DATA/Split/Right"), LabelWidth(60)] 
        [SerializeField] public int Column = 5;

        [Space]
        [TitleGroup("GRID", boldTitle: true)]
        [TableMatrix(SquareCells = true, HideRowIndices = false, HideColumnIndices = true, RespectIndentLevel = true,
            ResizableColumns = false, DrawElementMethod = nameof(DrawCells))]
        [SerializeField] public CellData[,] Matrix = new CellData[5, 5];

        //===================================================
        // PROPERTIES
        //===================================================
        public int Width => Matrix?.GetLength(0) ?? 0;
        public int Height => Matrix?.GetLength(1) ?? 0;

        //===================================================
        // METHODS
        //===================================================
        private void MakeGrid()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.DisplayDialog("LEVEL DATA",
                    "Are you sure you want generate a new grid?\n\nWARNING\nGenerating a new grid will reset all current cells.",
                    "Yes", "No"))
            {
                Matrix = new CellData[Column, Row];
                for (int x = 0; x < Column; x++)
                {
                    for (int y = 0; y < Row; y++)
                    {
                        Matrix[x, y] = new CellData();
                    }
                }
            }
#endif
        }

        public static Color GetColor(TileType tileType) => tileType switch
        {
            TileType.None => new Color(1f, .3f, .1f, 1f),
            TileType.Stack => new Color(0.2f, 0.2f, 0.2f, 1f),
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
                EditorGUIUtility.labelWidth = 50;
                value.ChipStackData = (ChipStackData)EditorGUILayout.ObjectField("Chips Data", value.ChipStackData, typeof(ChipStackData), false);

            }

            GUILayout.EndArea();
#endif

            return value;
        }
    }
    
    [Serializable]
    public class ChipStack
    {
        public List<ChipStackData> stackData;
    }

    [Serializable]
    public class ChipStackData : ScriptableObject //check
    {
        [BoxGroup("Stack")] public bool isMulti;
        [ShowIf("@isMulti == true")] public List<ChipSubStack> multiProducerData;
        [BoxGroup("Stack")] public List<ChipSubStack> chipData;
    }

    [Serializable]
    public class ChipSubStack
    {
        public int startIndex;
        public int endIndex;
        public bool isMirror;
        public Types.ChipType stackType;
        [ShowIf("@stackType == Types.ChipType.Mirror")] public int mirrorId;
    }
}


