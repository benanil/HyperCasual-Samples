using UnityEngine;
using UnityEngine.Rendering;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Random = System.Random;

public interface IRenderer
{
    int GetPackIndex();
}


namespace MiddleGames.Misc
{
    [ExecuteInEditMode]
    public class LeafRenderer : MonoBehaviour, IRenderer
    {
        public Material grassMaterial;
        public Material packMaterial;

        public Vector2 LeafSize = new Vector2(80, 100);
        public Transform targetPos;
        public int leafAmount = 30_000;
        [NonSerialized] public float playerRange;

        public int packIndex; // stack pack at Stack.cs's itemprefabs 
        public int GetPackIndex() => packIndex;

        private Matrix4x4[] matrices;

        public int loadIndex = -1;

        public Vector2 anchor;

        ComputeBuffer computeBuffer, argsBuffer;

        public bool CanDeform = true;

        Bounds bounds;
        Bounds cutBounds; // todo

        public Transform fenceParent;

        private const int targetFps = 60;

        public Mesh leafMesh;


        [ContextMenu("reset")]
        private void Reset()
        {
            transform.localScale = new Vector3(50, 30, 65);
        }

        private void Start()
        {
            Application.targetFrameRate = 30;

            // for disabling frustum culling because without this at some point maybe grasses dont drawn
            Camera.main.cullingMatrix = Matrix4x4.Ortho(-1000, 1000, -1000, 1000, 0.001f, 2000) * Matrix4x4.Translate(Vector3.forward * -2000 / 2f) * Camera.main.worldToCameraMatrix;

            UpdateMatrices();
            grassMaterial.SetVector("playerPos", Vector3.zero);
        }

        public void OnValidate()
        {
            if (!Application.isPlaying && leafMesh != null)
            {
                UpdateMatrices();
            }
        }

        public unsafe void UpdateMatrices()
        {
            Vector3 range = transform.localScale * 0.5f;

            matrices = new Matrix4x4[leafAmount];

            Random rand = new Random(888);

            for (int j = 0; j < matrices.Length; j++)
            {
                Vector3 origin = transform.position;
                origin += NextRandomRange(rand) * range.x * transform.right;
                origin += NextRandomRange(rand) * range.z * transform.forward;

                origin.y += NextRandomRange(rand) * 0.1f;

                matrices[j] = Matrix4x4.TRS(origin,
                    Quaternion.Euler(0, NextRandomRange(rand, 0, 360), 0),
                    Vector3.one * NextRandomRange(rand, LeafSize.x, LeafSize.y));
            }

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            min.y = 0; max.y = 2;

            // calculate grass bounds for rendering and colission detection
            for (int i = 0; i < matrices.Length; i++)
            {
                if (matrices[i].m03 > max.x) max.x = matrices[i].m03;
                if (matrices[i].m23 > max.z) max.z = matrices[i].m23;
                if (matrices[i].m03 < min.x) min.x = matrices[i].m03;
                if (matrices[i].m23 < min.z) min.z = matrices[i].m23;
            }

            bounds = new Bounds() { min = min, max = max };
            args[0] = leafMesh.GetIndexCount(0);
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            computeBuffer = new ComputeBuffer(matrices.Length, sizeof(Matrix4x4));

            // bounds = new Bounds(transform.position + (transform.localScale / 2), transform.localScale);
            UpdateArgs();
        }
        public float NextRandomRange(Random rand)
        {
            float sample = (float)rand.NextDouble();
            return (0.5f * sample) + ((-0.5f) * (1f - sample));
        }
        public float NextRandomRange(Random rand, in float min, in float max)
        {
            float sample = (float)rand.NextDouble();
            return (max * sample) + (min * (1f - sample));
        }

        Vector3 oldPos;
        private void LateUpdate()
        {
#if UNITY_EDITOR
            if (transform.position != oldPos)
            {
                OnValidate();
                oldPos = transform.position;
            }
#endif
            if (leafAmount < 5) return; // 5 is random number for finish

            Graphics.DrawMeshInstancedIndirect(leafMesh, 0, grassMaterial, bounds, argsBuffer, 0, null, ShadowCastingMode.Off, true, 0, null, LightProbeUsage.Off);

            if (CanDeform == false || !Application.isPlaying) return;

            if (particleDuration < 0)
            {
                var emission = LeafManager.instance.particle.emission;
                emission.rateOverTime = 0;
                grassMaterial.SetVector("playerPos", Vector3.zero);
            }
            else
            {
                var emission = LeafManager.instance.particle.emission;
                emission.rateOverTime = LeafManager.instance.RateOverTime;
                grassMaterial.SetVector("playerPos", targetPos.position);
            }

            particleDuration -= Time.deltaTime;
        }

        internal void Upgrade(float value)
        {
            playerRange += value;
        }

        float particleDuration;
        public unsafe void Deform()
        {
            if (leafAmount < 2 || Stack.Selling) return;
            Vector2 playerPos = new Vector2(targetPos.position.x, targetPos.position.z) + anchor;
            int collectedLeafAmount = 0;

            fixed (float* ptr = &matrices[0].m03)
            {
                float* end = ptr + (matrices.Length * 16);
                float* curr = ptr;

                float diff_x, diff_z;

                while (curr < end)
                {
                    diff_x = (*curr) - playerPos.x;
                    curr += 2;
                    diff_z = (*curr) - playerPos.y;

                    if (Math.Sqrt(diff_x * diff_x + diff_z * diff_z) < playerRange)
                    {
                        *curr = -64;
                        collectedLeafAmount++;
                        particleDuration = 0.25f;
                    }
                    curr += 14;
                }
            }

            leafAmount -= collectedLeafAmount;
            if (leafAmount <= 800)
            {
                // 30 is random number for finish
                // Stage Complated Code Here
                leafAmount = 1;
                DisposeGrasses();
                OnStageCompleted();
                Stack.instance.OnItemCollected(collectedLeafAmount);
                return;
            }

            Stack.instance.OnItemCollected(collectedLeafAmount);

            UpdateArgs();
        }

        private void DisposeGrasses()
        {
            Array.Clear(matrices, 0, matrices.Length);
            Array.Resize(ref matrices, 1);

            computeBuffer.SetData(matrices);
            grassMaterial.SetBuffer("meshProperties", computeBuffer);

            computeBuffer.Dispose();
            argsBuffer.Dispose();

            matrices = null;
            GC.Collect();
        }

        public void OnStageCompleted()
        {
            for (short i = 0; i < fenceParent.childCount; i++)
            {
                fenceParent.GetChild(i).GetComponent<BoxCollider>().isTrigger = true;
            }
            CanDeform = false;
            LeafManager.NextStage();
            Amazing.Show();
        }

        readonly uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        public void UpdateArgs()
        {
            // Arguments for drawing mesh. 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
            args[1] = (uint)matrices.Length;

            argsBuffer.SetData(args);
            computeBuffer.SetData(matrices);

            grassMaterial.SetBuffer("meshProperties", computeBuffer);
            grassMaterial.SetVector("playerRange", new Vector4(playerRange, anchor.x, anchor.y, 0));
        }

        public unsafe void Paint(Vector3 position, in Color color, in float paintDist)
        {
            position.x += anchor.x;
            position.z += anchor.y;

            fixed (float* ptr = &matrices[0].m03)
            {
                float* end = ptr + matrices.Length; // 20 = number of floats in a property
                float* curr = ptr;

                float diff_x, diff_z;

                while (curr < end)
                {
                    diff_x = (*curr) - position.x;
                    curr += 2;
                    diff_z = (*curr) - position.y;

                    if (Math.Sqrt(diff_x * diff_x + diff_z * diff_z) < playerRange)
                    {
                        *(curr - 18) = color.r;
                        *(curr - 17) = color.g;
                        *(curr - 16) = color.b;
                    }
                    curr += 14; // +2 end matrix + 4 jump color + 11 for m03
                }
            }

            if (computeBuffer != null) computeBuffer.Release();
            computeBuffer = new ComputeBuffer(matrices.Length, sizeof(Matrix4x4));
            computeBuffer.SetData(matrices);
            grassMaterial.SetBuffer("meshProperties", computeBuffer);
            grassMaterial.SetFloat("playerRange", playerRange);
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(LeafRenderer))]
    public class LeafEditor : Editor
    {
        public int saveIndex;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // saveIndex = EditorGUILayout.IntField(saveIndex);
            // 
            // if (GUILayout.Button("Save Colors"))
            // {
            //     ((GrassRendering)target).SaveColors(saveIndex);
            // }
        }
    }
#endif
}
