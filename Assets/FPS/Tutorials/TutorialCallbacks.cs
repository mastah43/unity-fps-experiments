using UnityEngine;
using Unity.Tutorials.Core.Editor;
using UnityEditor;
using UnityEngine.AI;

namespace Unity.Tutorials
{
    /// <summary>
    /// Implement your Tutorial callbacks here.
    /// </summary>
    public class TutorialCallbacks : ScriptableObject
    {
        // TODO FutureObjectReference was removed / changed from unity
        //public FutureObjectReference futureRoomInstance = default;
        //public FutureObjectReference futureBotInstance = default;
        NavMeshSurface navMeshSurface = default;

        public bool NavMeshIsBuilt()
        {
            return navMeshSurface.navMeshData != null;
        }

        public void ClearAllNavMeshes()
        {
            if (!navMeshSurface)
            {
                navMeshSurface = GameObject.FindObjectOfType<NavMeshSurface>();
            }
            UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
            navMeshSurface.navMeshData = null;
        }

        /// <summary>
        /// Keeps the Room selected during a tutorial. 
        /// </summary>
        public void KeepRoomSelected()
        {
            // TODO FutureObjectReference was removed / changed from unity
            //SelectSpawnedGameObject(futureRoomInstance);
        }

        /// <summary>
        /// Keeps the Room selected during a tutorial. 
        /// </summary>
        public void KeepBotSelected()
        {
            // TODO FutureObjectReference was removed / changed from unity
            //SelectSpawnedGameObject(futureBotInstance);
        }


        /// <summary>
        /// Selects a GameObject in the scene, marking it as the active object for selection
        /// </summary>
        /// <param name="futureObjectReference"></param>
        public void SelectSpawnedGameObject(FutureObjectReference futureObjectReference)
        {
            // TODO FutureObjectReference was removed / changed from unity
            //if (futureObjectReference.sceneObjectReference == null) { return; }
            //Selection.activeObject = futureObjectReference.sceneObjectReference.ReferencedObjectAsGameObject;
        }

        public void SelectMoveTool()
        {
            Tools.current = Tool.Move;
        }

        public void SelectRotateTool()
        {
            Tools.current = Tool.Rotate;
        }
    }
}