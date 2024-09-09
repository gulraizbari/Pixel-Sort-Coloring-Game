// using Sirenix.OdinInspector;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using DG.Tweening;
// using System;
//
// public class GridGenerator : MonoBehaviour, IGridGenerator
// {
//     [SerializeField] private IGridGenerator _grid;
//
//      void IGridGenerator.Initialize()
//     {
//         
//     }
//     
//     int row, column;
//
//     [ShowInInspector]
//     public Tile[,] tileArray;
//     [SerializeField]
//     private Box _wasteTile;
//     [SerializeField] 
//     private GameObject Hurdle;
//     private LevelData level;
//     private int playerCount = 0;
//     public ICombination CombinationInterface { get; set; }
//     public List<GameObject> SpawnedGridData = new List<GameObject>();
//     public int CurrentGridCount;
//     public void SpawnData(GameObject _parentObject,LevelData _level, Tile _freeTile, Tile _hurdleTile, Tile _envTile, Box _box,Tile _tunnel)
//     {
//         playerCount = 0;
//         SpawnedGridData.Clear();
//         level = _level;
//         tileArray = new Tile[_level.Width, _level.Height];
//         row = _level.Width;
//         column = _level.Height;
//
//         // Define spacing between tiles
//         float tileSpacing = 1f; // Adjust this value as needed
//
//         // Calculate total width and height of the grid
//         float totalWidth = (_level.Width - 1) * tileSpacing;
//         float totalHeight = (_level.Height - 1) * tileSpacing;
//
//         // Calculate the offset to center the grid
//         Vector3 gridCenterOffset = new Vector3(totalWidth / 2, 0, totalHeight / 2);
//
//         // First, generate the grid
//         for (int row = _level.Width - 1; row >= 0; row--)
//         {
//             for (int col = _level.Height - 1; col >= 0; col--)
//             {
//                 // Calculate tile position and offset it to center the grid
//                 Vector3 tilePos = new Vector3(row * tileSpacing, 0, (_level.Height - 1 - col) * tileSpacing) - gridCenterOffset + _parentObject.transform.position;
//                 switch (_level.Matrix[row, col].tileType)
//                 {
//                     case TileType.Walkable:
//                         SpawnNewTile(_freeTile, _parentObject,true, tilePos,row,col);
//                         break;
//                     case TileType.Hurdle:
//                         SpawnNewTile(_hurdleTile, _parentObject, true, tilePos, row, col);
//                         break;
//                     case TileType.Waste:
//                         SpawnNewTile(_freeTile, _parentObject, true, tilePos, row, col);
//                         break;
//                     case TileType.None:
//                         SpawnNewTile(_envTile, _parentObject, false, tilePos, row, col);
//                         break;
//                     case TileType.Player:
//                         playerCount++;
//                         SpawnNewTile(_freeTile, _parentObject, true, tilePos, row, col);
//                         break;
//                     case TileType.Tunnel:
//                         SpawnNewTile(_tunnel, _parentObject, true, tilePos, row, col);
//                         break;
//                 }
//             }
//         }
//         CombinationInterface.Initialize();
//         SpawnRandomBoxes(_box);
//     }
//     
//     void SpawnNewTile(Tile _tile,GameObject _parentObject,bool IsTileOn,Vector3 tilepos,int row,int col)
//     {
//         Tile Base;
//         Base = Instantiate(_tile, _parentObject.transform);
//         Base.transform.position = tilepos;
//         Base.gameObject.name = row.ToString() + "*" + col.ToString();
//         Base.Init(row, col, level.Matrix[row, col].tileType, false, level.Matrix[row, col].typeOfPlayer);
//         tileArray[row, col] = Base;
//         if (Base.TileState == TileType.Tunnel)
//         {
//             Base._tunnelData.TotalBoxes = level.Matrix[row, col].NoOfBoxes;
//             Base._tunnelData.Type = level.Matrix[row, col].typeOfTunnel;
//             switch (Base._tunnelData.Type)
//             {
//                 case TunnelType.Top:
//                     Base.transform.rotation = Quaternion.Euler(0,90,0);
//                     break;
//                 case TunnelType.Bottom:
//                     break;
//                 case TunnelType.Left:
//                     Base.transform.rotation = Quaternion.Euler(0, 0, 0);
//                     break;
//                 case TunnelType.Right:
//                     Base.transform.rotation = Quaternion.Euler(0, 180, 0);
//                     break;
//                 default:
//                     break;
//             }
//         }
//         if (Base.TileState == TileType.Hurdle)
//         {
//             Base.GetComponent<MeshRenderer>().enabled = false;
//         }
//         Base.gameObject.SetActive(IsTileOn);
//         SpawnedGridData.Add(Base.gameObject);
//     }
//     public void SpawnRandomBoxes(Box _box)
//     {
//         int LockedBox = 0;
//         CombinationInterface.RandomizeOrder();
//         int Count = playerCount-1;
//         // Now, spawn the players based on the Player tile type
//         for (int row = level.Width - 1; row >= 0; row--)
//         {
//             for (int col = level.Height - 1; col >= 0; col--)
//             {
//                 if (level.Matrix[row, col].tileType == TileType.Player && level.Matrix[row, col].typeOfPlayer != PlayerType.None)
//                 {
//                     Tile tile = tileArray[row, col];
//                     //This Will generate the Boxes from Combinations generated by Combinatin Script
//                     
//                     if (CurrentGridCount >= 2)
//                     {
//                         //This Will Spawn The Random Boxes
//                         if (/*row >= (level.Width / 2) &&*/ col >= (level.Height / 2) && level.NumberOfBoxtoLock > LockedBox)
//                         {
//                             LockedBox++;
//                             SpawnPlayer(tile.gameObject, _box, (PlayerType)(CombinationInterface.Order[Count]), CombinationInterface.Order[Count], true);
//                             Debug.Log("Locked Boxes Has to be Shown");
//                         }
//                         else
//                         {
//                             SpawnPlayer(tile.gameObject, _box, (PlayerType)(CombinationInterface.Order[Count]), CombinationInterface.Order[Count], false);
//                         }
//                         
//                     }
//                     else
//                     {
//                         //This Will pick the values from the grid generation values or leveldata scriptable object
//                         if (row >= (level.Width / 2) && col >= (level.Height / 2) && level.NumberOfBoxtoLock > LockedBox)
//                         {
//                             LockedBox++;
//                             SpawnPlayer(tile.gameObject, _box, tile.playerType, (int)tile.playerType, true);
//                             Debug.Log("Locked Boxes Has to be Shown");
//                         }
//                         else
//                         {
//                             SpawnPlayer(tile.gameObject, _box, tile.playerType, (int)tile.playerType, false);
//                         }
//                     }
//                     Count--;
//                 }
//                 else if (level.Matrix[row, col].tileType == TileType.Waste)
//                 {
//                     //For Waste Tile Spawning
//                     Tile tile = tileArray[row, col];
//                     var wasteTile = Instantiate(_wasteTile, tile.transform);
//                     wasteTile.transform.DOScale(new Vector3(1, 1, 1), 0.15f);
//                     wasteTile.transform.parent.GetComponent<Tile>().TileState = TileType.Player;
//                     wasteTile.transform.localPosition = new Vector3(0, 0, 0);
//                     wasteTile.Merge_Type = PlayerType.None;
//                     wasteTile.gridGeneratorinterace = this;
//                 }
//                 else if (level.Matrix[row, col].tileType == TileType.Hurdle)
//                 {
//                     //For Waste Tile Spawning
//                     Tile tile = tileArray[row, col];
//                     var _hurdle = Instantiate(Hurdle, tile.transform);
//                     _hurdle.transform.DOScale(new Vector3(1, 1, 1), 0.15f);
//                     _hurdle.transform.localPosition = new Vector3(0, 0, 0);
//                 }
//             }
//         }
//     }
//
//     public void SpawnPlayer(GameObject Tile, Box _player, PlayerType B_type,int Materail_id,bool IsLocked)
//     {
//         var Player = Instantiate(_player, Tile.transform);
//         Player.transform.DOScale(new Vector3(1,1,1),0.15f);
//         Player.transform.parent.GetComponent<Tile>().TileState = TileType.Player;
//         Player.transform.localPosition = new Vector3(0, 0, 0);
//         Player.Merge_Type = B_type;
//         Player.BoxNo = Materail_id;
//        
//         if (Player.Is_Crate)
//         {
//             Player._box_render.enabled = true;
//             Player._box_render.material = Player._box_material[Materail_id];
//         }
//         else
//         {
//             Player._boxes[Materail_id].SetActive(true);
//         }
//         Player.gridGeneratorinterace = this;
//         if (IsLocked)
//         {
//             Player.Is_Loock = IsLocked;
//             Player._boxes[Materail_id].SetActive(false);
//             Player.Locked.SetActive(true);
//             Player._col.enabled = false;
//         }
//     }
//
//     public Tile[,] GridView => tileArray;
//     public LevelData Currentlevel => level;
//     public int Row()
//     {
//         return row;
//     }
//     
//     public int Column()
//     {
//         return column;
//     }
//     public int BoxesCount()
//     {
//         return playerCount;
//     }
//     
//     public List<Tile> GetAdjacentTiles(Tile currentTile)
//     {
//         List<Tile> adjacentTiles = new List<Tile>();
//
//         // Get the coordinates of the current tile
//         int x = currentTile.X; // Assuming you have X and Y properties in your tile class
//         int y = currentTile.Y;
//
//         // Define the relative positions of adjacent tiles
//         int[,] offsets = {
//         { -1, 0 }, // Left
//         { 1, 0 },  // Right
//         { 0, -1 }, // Down
//         { 0, 1 }   // Up
//     };
//         // Iterate through the relative positions
//         for (int i = 0; i < offsets.GetLength(0); i++)
//         {
//             int newX = x + offsets[i, 0];
//             int newY = y + offsets[i, 1];
//
//             // Check if the new coordinates are within bounds
//             if (newX >= 0 && newX < row && newY >= 0 && newY < column)
//             {
//                 adjacentTiles.Add(tileArray[newX, newY]);
//             }
//         }
//
//         return adjacentTiles;
//     }
//
// }
//
//
//
//
//
