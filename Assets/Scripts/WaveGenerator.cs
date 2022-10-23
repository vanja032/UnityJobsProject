using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

public class WaveGenerator : MonoBehaviour
{
    JobHandle handle;
    UpdateMesh job;

    private Mesh WaterMesh;
    public MeshFilter MeshFilter;

    [Header("Wave properties")]
    public float WaveScale;
    public float WaveSpeed;
    public float WaveHeight;

    NativeArray<Vector3> Vertices;
    NativeArray<Vector3> Normals;

    void Start()
    {
        WaterMesh = MeshFilter.mesh;
        WaterMesh.MarkDynamic();
        Vertices = new NativeArray<Vector3>(WaterMesh.vertices, Allocator.Persistent);
        Normals = new NativeArray<Vector3>(WaterMesh.normals, Allocator.Persistent);
    }

    private void Update()
    {
        job = new UpdateMesh()
        {
            vertices = Vertices,
            normals = Normals,
            speed = WaveSpeed,
            scale = WaveScale,
            height = WaveHeight,
            time = Time.time
        };
        handle = job.Schedule(Vertices.Length, 64);
    }

    private void LateUpdate()
    {
        handle.Complete();
        WaterMesh.SetVertices(job.vertices);
        WaterMesh.RecalculateNormals();
        Debug.Log("Finished Job");
    }

    private void OnDestroy()
    {
        Vertices.Dispose();
        Normals.Dispose();
    }

    [BurstCompile]
    private struct UpdateMesh : IJobParallelFor
    {
        public NativeArray<Vector3> vertices;

        [ReadOnly]
        public NativeArray<Vector3> normals;

        public float speed;
        public float scale;
        public float height;

        public float time;

        private float Noise(float x, float y)
        {
            float2 position = math.float2(x, y);
            return noise.snoise(position);
        }
        public void Execute(int i)
        {
            if(normals[i].z > 0)
            {
                var vertex = vertices[i];
                float value = Noise(vertex.x * scale + speed * time, vertex.y * scale + speed * time);
                vertices[i] = new Vector3(vertex.x, vertex.y, value * height + 0.3f);
            }
        }
    }
}
