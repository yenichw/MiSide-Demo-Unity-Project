using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EPOOutline
{
    public class MeshPool : IDisposable
    {
        private Queue<Mesh> freeMeshes = new Queue<Mesh>();

        private List<Mesh> allMeshes = new List<Mesh>();

        public Mesh AllocateMesh()
        {
            while (freeMeshes.Count > 0 && freeMeshes.Peek() == null)
                freeMeshes.Dequeue();

            if (freeMeshes.Count == 0)
            {
                var mesh = new Mesh();
                mesh.MarkDynamic();
                allMeshes.Add(mesh);
                freeMeshes.Enqueue(mesh);
            }

            return freeMeshes.Dequeue();
        }

        public void ReleaseAllMeshes()
        {
            freeMeshes.Clear();
            foreach (var mesh in allMeshes)
                freeMeshes.Enqueue(mesh);
        }

        public void Dispose()
        {
            foreach (var mesh in allMeshes)
                Object.DestroyImmediate(mesh);
            
            allMeshes.Clear();
            freeMeshes.Clear();
        }
    }
}