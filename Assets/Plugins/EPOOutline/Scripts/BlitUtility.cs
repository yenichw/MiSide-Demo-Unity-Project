using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace EPOOutline
{
    public static class BlitUtility
    {
        private static readonly int MainTexHash = Shader.PropertyToID("_MainTex");
        private static readonly int NormalMatricesHash = Shader.PropertyToID("_NormalMatrices");

        private static Vector4[] normals;
        private static ushort[] tempIndicies;
        private static Vector4[] tempVertecies;

        private static readonly VertexAttributeDescriptor[] vertexParams =
                new VertexAttributeDescriptor[]
                    {
                        new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
                        new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32)
                    };

        private struct MeshSetupResult
        {
            public readonly int ItemsToDraw;
            public readonly int VertexIndex;
            public readonly int TriangleIndex;

            public MeshSetupResult(int itemsToDraw, int vertexIndex, int triangleIndex)
            {
                ItemsToDraw = itemsToDraw;
                VertexIndex = vertexIndex;
                TriangleIndex = triangleIndex;
            }
        }

        
        private const int BatchSize = 128;
        private const int DefaultBufferSize = 16;

        private static Vertex[] vertices = new Vertex[4096];
        private static ushort[] indices = new ushort[4096 * 5];
        private static Matrix4x4[] matrices = new Matrix4x4[DefaultBufferSize];
        private static Matrix4x4[] batchMatrices = new Matrix4x4[BatchSize];
        private static Matrix4x4[] rotationMatrices = new Matrix4x4[DefaultBufferSize];
        private static Matrix4x4[] batchRotationMatrices = new Matrix4x4[BatchSize];
        private static readonly Matrix4x4[] identityMatrixArray = new Matrix4x4[] { Matrix4x4.identity };
        private static MeshSetupResult? currentSetupResult;
        private static MaterialPropertyBlock propertyBlock;

        private static bool? supportsInstancing;

        private static bool SupportsInstancing
        {
            get
            {
                if (supportsInstancing.HasValue)
                    return supportsInstancing.Value;

                supportsInstancing = SystemInfo.supportsInstancing;
                return supportsInstancing.Value;
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct Vertex
        {
            public Vector4 Position;
            public Vector3 Normal;
        }

        private static void UpdateBounds(Renderer renderer, OutlineTarget target)
        {
            if (target.renderer is MeshRenderer)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter.sharedMesh != null)
                    meshFilter.sharedMesh.RecalculateBounds();
            }
            else if (target.renderer is SkinnedMeshRenderer skinedMeshRenderer)
            {
                if (skinedMeshRenderer.sharedMesh != null)
                    skinedMeshRenderer.sharedMesh.RecalculateBounds();
            }
        }

        public static void PrepareForRendering(OutlineParameters parameters)
        {
            if (parameters.BlitMesh == null)
                parameters.BlitMesh = parameters.MeshPool.AllocateMesh();

            var result = SupportsInstancing ? 
                             SetupForInstancing(parameters) :
                             SetupForBruteForce(parameters);

            currentSetupResult = result;

            if (!result.HasValue)
                return;

            const MeshUpdateFlags flags = MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices;
            
            parameters.BlitMesh.SetVertexBufferParams(result.Value.VertexIndex, attributes: vertexParams);
            parameters.BlitMesh.SetVertexBufferData(vertices, 0, 0, result.Value.VertexIndex, 0, flags);
            parameters.BlitMesh.SetIndexBufferParams(result.Value.TriangleIndex, IndexFormat.UInt16);
            parameters.BlitMesh.SetIndexBufferData(indices, 0, 0, result.Value.TriangleIndex, flags);

            parameters.BlitMesh.subMeshCount = 1;
            parameters.BlitMesh.SetSubMesh(0, new SubMeshDescriptor(0, result.Value.TriangleIndex, MeshTopology.Triangles), flags);
        }

        private static void CheckModel()
        {
            if (normals != null &&
                tempVertecies != null &&
                tempIndicies != null)
                return;

            var mesh = Resources.Load<Mesh>("Easy performant outline/Models/Rounded box");

            tempVertecies = Array.ConvertAll(mesh.vertices, x => new Vector4(x.x, x.y, x.z, 1.0f));
            tempIndicies = Array.ConvertAll(mesh.triangles, x => (ushort)x);
            normals = Array.ConvertAll(mesh.normals, x => (Vector4)x);
            
            Resources.UnloadAsset(mesh);
        }

        private static MeshSetupResult? SetupForInstancing(OutlineParameters parameters)
        {
            CheckModel();
            
            if (vertices.Length < tempVertecies.Length)
                Array.Resize(ref vertices, tempVertecies.Length);

            if (indices.Length < tempIndicies.Length)
                Array.Resize(ref indices, tempIndicies.Length);

            var totalCountOfTargets = 0;
            foreach (var outlinable in parameters.OutlinablesToRender)
                totalCountOfTargets += outlinable.OutlineTargets.Count;

            while (matrices.Length < totalCountOfTargets)
                Array.Resize(ref matrices, matrices.Length * 2);
            
            while (rotationMatrices.Length < totalCountOfTargets)
                Array.Resize(ref rotationMatrices, rotationMatrices.Length * 2);

            var currentIndex = 0;

            for (var index = 0; index < tempIndicies.Length; index++)
                indices[index] = tempIndicies[index];

            for (var index = 0; index < tempVertecies.Length; index++)
            {
                vertices[currentIndex++] = new Vertex()
                                           {
                                               Position = tempVertecies[index],
                                               Normal = normals[index]
                                           };
            }

            var itemIndex = 0;
            foreach (var outlinable in parameters.OutlinablesToRender)
            {
                if (outlinable.DrawingMode != OutlinableDrawingMode.Normal)
                    continue;

                foreach (var target in outlinable.OutlineTargets)
                {
                    var renderer = target.Renderer;
                    if (!target.IsVisible)
                        continue;

                    var pretransformedBounds = false;
                    var bounds = new Bounds();
                    if (target.BoundsMode == BoundsMode.Manual)
                    {
                        bounds = target.Bounds;
                        var size = bounds.size;
                        var rendererScale = renderer.transform.localScale;
                        size.x /= rendererScale.x;
                        size.y /= rendererScale.y;
                        size.z /= rendererScale.z;
                        bounds.size = size;
                    }
                    else
                    {
                        if (target.BoundsMode == BoundsMode.ForceRecalculate)
                            UpdateBounds(target.Renderer, target);

                        var meshRenderer = renderer as MeshRenderer;
                        var index = (meshRenderer == null ? 0 : meshRenderer.subMeshStartIndex) + target.SubmeshIndex;
                        var filter = meshRenderer == null ? null : meshRenderer.GetComponent<MeshFilter>();
                        var mesh = filter == null ? null : filter.sharedMesh;

                        if (mesh != null && mesh.subMeshCount > index)
                        {
                            bounds = mesh.GetSubMesh(index).bounds;
                            pretransformedBounds = meshRenderer.isPartOfStaticBatch;
                        }
                        else
                        {
                            pretransformedBounds = true;
                            bounds = renderer.bounds;
                        }
                    }

                    if (pretransformedBounds)
                    {
                        rotationMatrices[itemIndex] = Matrix4x4.identity;
                        matrices[itemIndex++] = Matrix4x4.TRS(bounds.center, Quaternion.identity, bounds.size);
                    }
                    else
                    {
                        var targetTransform = target.renderer.transform;
                        var size = bounds.size;

                        rotationMatrices[itemIndex] = Matrix4x4.Rotate(targetTransform.rotation);
                        matrices[itemIndex++] = targetTransform.localToWorldMatrix *
                                                Matrix4x4.Translate(bounds.center) * Matrix4x4.Scale(size);
                    }
                }
            }

            return new MeshSetupResult(itemIndex, currentIndex, tempIndicies.Length);
        }

        private static MeshSetupResult? SetupForBruteForce(OutlineParameters parameters)
        {
            CheckModel();

            var numberOfVertices = tempVertecies.Length;

            var currentIndex = 0;
            var triangleIndex = 0;
            var expectedCount = 0;
            foreach (var outlinable in parameters.OutlinablesToRender)
                expectedCount += numberOfVertices * outlinable.OutlineTargets.Count;

            if (vertices.Length < expectedCount)
            {
                Array.Resize(ref vertices, expectedCount * 2);
                Array.Resize(ref indices, vertices.Length * 5);
            }

            foreach (var outlinable in parameters.OutlinablesToRender)
            {
                if (outlinable.DrawingMode != OutlinableDrawingMode.Normal)
                    continue;

                for (var outlineIndex = 0; outlineIndex < outlinable.OutlineTargets.Count; outlineIndex++)
                {
                    var target = outlinable.OutlineTargets[outlineIndex];
                    var renderer = target.Renderer;
                    if (!target.IsVisible)
                        continue;

                    var pretransformedBounds = false;
                    var bounds = new Bounds();
                    if (target.BoundsMode == BoundsMode.Manual)
                    {
                        bounds = target.Bounds;
                        var size = bounds.size;
                        var rendererScale = renderer.transform.localScale;
                        size.x /= rendererScale.x;
                        size.y /= rendererScale.y;
                        size.z /= rendererScale.z;
                        bounds.size = size;
                    }
                    else
                    {
                        if (target.BoundsMode == BoundsMode.ForceRecalculate)
                            UpdateBounds(target.Renderer, target);

                        var meshRenderer = renderer as MeshRenderer;
                        var index = (meshRenderer == null ? 0 : meshRenderer.subMeshStartIndex) + target.SubmeshIndex;
                        var filter = meshRenderer == null ? null : meshRenderer.GetComponent<MeshFilter>();
                        var mesh = filter == null ? null : filter.sharedMesh;

                        if (mesh != null && mesh.subMeshCount > index)
                            bounds = mesh.GetSubMesh(index).bounds;
                        else
                        {
                            pretransformedBounds = true;
                            bounds = renderer.bounds;
                        }
                    }

                    Vector4 boundsSize = bounds.size;
                    boundsSize.w = 1;

                    var boundsCenter = (Vector4)bounds.center;

                    var transformMatrix = Matrix4x4.identity;
                    var normalTransformMatrix = Matrix4x4.identity;
                    if (!pretransformedBounds &&
                        (target.BoundsMode == BoundsMode.Manual || !renderer.isPartOfStaticBatch))
                    {
                        transformMatrix = target.renderer.transform.localToWorldMatrix;
                        normalTransformMatrix = Matrix4x4.Rotate(renderer.transform.rotation);
                    }

                    var numberOfIndicies = tempIndicies.Length;
                    for (var index = 0; index < numberOfIndicies; index++)
                        indices[triangleIndex++] = (ushort)(tempIndicies[index] + currentIndex);

                    for (var index = 0; index < numberOfVertices; index++)
                    {
                        var normal = normalTransformMatrix * normals[index];

                        var vert = tempVertecies[index];
                        var scaledVert = new Vector4(vert.x * boundsSize.x, vert.y * boundsSize.y,
                            vert.z * boundsSize.z, 1);

                        var vertex = new Vertex()
                        {
                            Position = transformMatrix * (boundsCenter + scaledVert),
                            Normal = normal
                        };

                        vertices[currentIndex++] = vertex;
                    }
                }
            }

            rotationMatrices[0] = Matrix4x4.identity;
            return new MeshSetupResult(1, currentIndex, triangleIndex);
        }

        private static void RenderInstancedBatched(CommandBufferWrapper buffer, Mesh mesh, Material material, int pass, int count)
        {
            propertyBlock ??= new MaterialPropertyBlock();
            propertyBlock.Clear();
            var index = 0;
            while (count > 0)
            {
                var countForBatch = Mathf.Min(BatchSize, count);
                Array.Copy(rotationMatrices, index, batchRotationMatrices, 0, countForBatch);
                Array.Copy(matrices, index, batchMatrices, 0, countForBatch);

                propertyBlock.SetMatrixArray(NormalMatricesHash, batchRotationMatrices);
                buffer.DrawMeshInstanced(mesh, 0, material, pass, batchMatrices, countForBatch, propertyBlock);
                
                count -= countForBatch;
                index += countForBatch;
            }
        }

        public static void Blit(OutlineParameters parameters, RTHandle source, RTHandle destination, RTHandle destinationDepth, int eyeSlice, Material material, int pass = -1, Rect? viewport = null)
        {
            if (!currentSetupResult.HasValue)
            {
                Debug.LogError("Setup process wasn't completed.");
                return;
            }
            
            var buffer = parameters.Buffer;
            buffer.SetRenderTarget(destination, destinationDepth, eyeSlice);
            if (viewport.HasValue)
                parameters.Buffer.SetViewport(viewport.Value);

            buffer.SetGlobalTexture(MainTexHash, source);

            if (SupportsInstancing)
            {
                RenderInstancedBatched(buffer, parameters.BlitMesh, material, pass,
                    currentSetupResult.Value.ItemsToDraw);
            }
            else
            {
                material.SetMatrixArray(NormalMatricesHash, identityMatrixArray);
                buffer.DrawMesh(parameters.BlitMesh, Matrix4x4.identity, material, 0, pass);
            }
        }
        
        public static void Draw(OutlineParameters parameters, RTHandle destination, RTHandle destinationDepth, int eyeSlice, Material material, int pass = -1, Rect? viewport = null)
        {
            if (!currentSetupResult.HasValue)
            {
                Debug.LogError("Setup process wasn't completed.");
                return;
            }

            var buffer = parameters.Buffer;
            buffer.SetRenderTarget(destination, destinationDepth, eyeSlice);
            if (viewport.HasValue)
                parameters.Buffer.SetViewport(viewport.Value);

            if (SupportsInstancing)
            {
                if (propertyBlock == null)
                    propertyBlock = new MaterialPropertyBlock();
                
                propertyBlock.Clear();
                propertyBlock.SetMatrixArray(NormalMatricesHash, rotationMatrices);
                buffer.DrawMeshInstanced(parameters.BlitMesh, 0, material, pass, matrices, currentSetupResult.Value.ItemsToDraw, propertyBlock);
            }
            else
            {
                material.SetMatrixArray(NormalMatricesHash, identityMatrixArray);
                buffer.DrawMesh(parameters.BlitMesh, Matrix4x4.identity, material, 0, pass);
            }
        }
    }
}