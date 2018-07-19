﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UltimateGameTools
{
    namespace MeshSimplifier
    {

        /// <summary>
        /// Stores vertex and mapping information.
        /// </summary>
        public class Triangle
        {
            public Vertex[] Vertices
            {
                get { return m_aVertices; }
            }

            public bool HasUVData
            {
                get { return m_bUVData; }
            }

            public int[] IndicesUV
            {
                get { return m_aUV; }
            }

            public Vector3 Normal
            {
                get { return m_v3Normal; }
            }

            public int[] Indices
            {
                get { return m_aIndices; }
            }

            private Vertex[] m_aVertices;
            private bool m_bUVData;
            private int[] m_aUV;
            private int[] m_aIndices;
            private Vector3 m_v3Normal;
            private int m_nSubMesh;

            public Triangle(Simplifier simplifier, int nSubMesh, Vertex v0, Vertex v1, Vertex v2, bool bUVData,
                int nIndex1, int nIndex2, int nIndex3)
            {
                m_aVertices = new Vertex[3];
                m_aUV = new int[3];
                m_aIndices = new int[3];

                m_aVertices[0] = v0;
                m_aVertices[1] = v1;
                m_aVertices[2] = v2;

                m_nSubMesh = nSubMesh;

                m_bUVData = bUVData;

                if (m_bUVData)
                {
                    m_aUV[0] = nIndex1;
                    m_aUV[1] = nIndex2;
                    m_aUV[2] = nIndex3;
                }

                m_aIndices[0] = nIndex1;
                m_aIndices[1] = nIndex2;
                m_aIndices[2] = nIndex3;

                ComputeNormal();

                simplifier.m_aListTriangles[nSubMesh].m_listTriangles.Add(this);

                for (int i = 0; i < 3; i++)
                {
                    m_aVertices[i].m_listFaces.Add(this);

                    for (int j = 0; j < 3; j++)
                    {
                        if (i != j)
                        {
                            if (m_aVertices[i].m_listNeighbors.Contains(m_aVertices[j]) == false)
                            {
                                m_aVertices[i].m_listNeighbors.Add(m_aVertices[j]);
                            }
                        }
                    }
                }
            }

            public void Destructor(Simplifier simplifier)
            {
                int i;
                simplifier.m_aListTriangles[m_nSubMesh].m_listTriangles.Remove(this);

                for (i = 0; i < 3; i++)
                {
                    if (m_aVertices[i] != null)
                    {
                        m_aVertices[i].m_listFaces.Remove(this);
                    }
                }

                for (i = 0; i < 3; i++)
                {
                    int i2 = (i + 1)%3;

                    if (m_aVertices[i] == null || m_aVertices[i2] == null) continue;

                    m_aVertices[i].RemoveIfNonNeighbor(m_aVertices[i2]);
                    m_aVertices[i2].RemoveIfNonNeighbor(m_aVertices[i]);
                }
            }

            public bool HasVertex(Vertex v)
            {
                return (v == m_aVertices[0] || v == m_aVertices[1] || v == m_aVertices[2]);
            }

            public void ComputeNormal()
            {
                Vector3 v0 = m_aVertices[0].m_v3Position;
                Vector3 v1 = m_aVertices[1].m_v3Position;
                Vector3 v2 = m_aVertices[2].m_v3Position;

                m_v3Normal = Vector3.Cross((v1 - v0), (v2 - v1));

                if (m_v3Normal.magnitude == 0.0f) return;

                m_v3Normal = m_v3Normal.normalized;
            }

            public int TexAt(Vertex vertex)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (m_aVertices[i] == vertex)
                    {
                        return m_aUV[i];
                    }
                }

                UnityEngine.Debug.LogError("TexAt(): Vertex not found");
                return 0;
            }

            public int TexAt(int i)
            {
                return m_aUV[i];
            }

            public void SetTexAt(Vertex vertex, int uv)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (m_aVertices[i] == vertex)
                    {
                        m_aUV[i] = uv;
                        return;
                    }
                }

                UnityEngine.Debug.LogError("SetTexAt(): Vertex not found");
            }

            public void SetTexAt(int i, int uv)
            {
                m_aUV[i] = uv;
            }

            public void ReplaceVertex(Vertex vold, Vertex vnew)
            {
                if (vold == m_aVertices[0])
                {
                    m_aVertices[0] = vnew;
                }
                else if (vold == m_aVertices[1])
                {
                    m_aVertices[1] = vnew;
                }
                else
                {
                    m_aVertices[2] = vnew;
                }

                int i;

                vold.m_listFaces.Remove(this);
                vnew.m_listFaces.Add(this);

                for (i = 0; i < 3; i++)
                {
                    vold.RemoveIfNonNeighbor(m_aVertices[i]);
                    m_aVertices[i].RemoveIfNonNeighbor(vold);
                }

                for (i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (i != j)
                        {
                            if (m_aVertices[i].m_listNeighbors.Contains(m_aVertices[j]) == false)
                            {
                                m_aVertices[i].m_listNeighbors.Add(m_aVertices[j]);
                            }
                        }
                    }
                }

                ComputeNormal();
            }
        };
    }
}