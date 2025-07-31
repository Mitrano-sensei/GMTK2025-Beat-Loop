using UnityEngine;
using UnityEngine.Tilemaps;

namespace Utilities
{
    public static class GridHelpers
    {
        public static Vector3Int WorldToCell(Vector3 worldPosition, Grid grid)
        {
            return grid.WorldToCell(worldPosition).WithZ(0);
        }

        public static Vector3 CellToWorld(Vector3Int cellPosition, Grid grid)
        {
            return grid.CellToWorld(cellPosition);
        }

        public static Vector3Int MousePositionToCell(Grid grid)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return WorldToCell(mousePosition, grid);
        }

        public static Vector3 MouseCellToWorld(Grid grid)
        {
            Vector3Int cellPosition = MousePositionToCell(grid);
            return CellToWorld(cellPosition, grid);
        }

        /// <summary>
        /// Get the transform components for a tile. Convenience Function.
        /// </summary>
        /// <param name="map">Tilemap</param>
        /// <param name="position">position on map</param>
        /// <param name="tPosition">transform's position placed here</param>
        /// <param name="tRotation">transform's rotation placed here</param>
        /// <param name="tScale">transform's scale placed here</param>
        /// <remarks>Handy for tweening the transform (pos,scale,rot) of a tile</remarks>
        /// <remarks>No checking for whether or not a tile exists at that position</remarks>
        public static void GetTransformComponents(Tilemap map,
                                                Vector3Int position,
                                                out Vector3 tPosition,
                                                out Vector3 tRotation,
                                                out Vector3 tScale)
        {
            var transform = map.GetTransformMatrix(position);
            tPosition = transform.GetColumn(3);
            tRotation = transform.rotation.eulerAngles;
            tScale = transform.lossyScale;
        }
        
        /// <summary>
        /// Set the transform for a tile. Convenience function.
        /// </summary>
        /// <param name="map">tilemap</param>
        /// <param name="position">position on map</param>
        /// <param name="tPosition">position for the tile transform</param>
        /// <param name="tRotation">rotation for the tile transform</param>
        /// <param name="tScale">scale for the tile transform</param>
        /// <remarks>Handy for tweening the transform (pos,scale,rot) of a tile's sprite</remarks>
        /// <remarks>No checking for whether or not a tile exists at that position</remarks>
        public static void SetTransform(Tilemap map,
            Vector3Int position,
            Vector3 tPosition,
            Vector3 tRotation,
            Vector3 tScale)
        {
            map.SetTransformMatrix(position, Matrix4x4.TRS(tPosition, Quaternion.Euler(tRotation), tScale));
        }
    }
}