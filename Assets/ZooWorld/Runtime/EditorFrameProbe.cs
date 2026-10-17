#if UNITY_EDITOR
using System;
using UnityEngine;
namespace ZooWorld
{
    // Editor verification executes inside a game frame, where Screen dimensions match the Canvas.
    // EditorApplication.update otherwise exposes the active Scene view dimensions to raycasters.
    public sealed class EditorFrameProbe : MonoBehaviour
    {
        public Action Tick;
        private void LateUpdate() { Tick?.Invoke(); }
    }
}
#endif
