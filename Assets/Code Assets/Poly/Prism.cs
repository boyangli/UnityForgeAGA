using System;
using System.Collections.Generic;
using UnityEngine;

namespace Poly {
    /// <summary>
    /// A simple datastructure that stores both a prism mesh and a simplified
    /// collection of vertices of the underlying polygon.
    /// </summary>
    public class Prism {
        /// <summary>
        /// The 3D mesh of this prism.
        /// </summary>
        public Mesh Mesh { get; private set; }
        /// <summary>
        /// The raw vertices of the polygon that was used to create the prism.
        /// </summary>
        private Vector3[] RawVertices { get; set; }
        /// <summary>
        /// The game object rendering this mesh.
        /// </summary>
        public GameObject GameObject { get; set; }
        /// <summary>
        /// A unique integer ID for this prism.
        /// </summary>
        public int ID { get; private set; }
        private static int Identifier = 0;

        /// <summary>
        /// Straight-forward constructor.
        /// </summary>
        /// <param name="mesh">The prism's mesh.</param>
        /// <param name="vertices">The vertice's of the polygon used to create the mesh.</param>
        public Prism(Mesh mesh, Vector3[] vertices) {
            this.Mesh = mesh;
            this.RawVertices = vertices;

            //Assign next ID.
            this.ID = Prism.Identifier++;
        }

        /// <summary>
        /// The transformed, simplified vertices of this prism.
        /// </summary>
        public Vector3[] TransformedVertices {
            get {
                Vector3[] transformed = new Vector3[this.RawVertices.Length];
                for (int i = 0; i < transformed.Length; i++) {
                    transformed[i] = this.GameObject.transform.TransformPoint(this.RawVertices[i]);
                }
                return transformed;
            }
        }
        /// <summary>
        /// The transformed origin for this prism.
        /// </summary>
        public Vector3 Origin {
            get {
                return this.GameObject.transform.position;
            }
        }

    }
}