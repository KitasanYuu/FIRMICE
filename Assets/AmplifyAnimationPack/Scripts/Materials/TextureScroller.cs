// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;

namespace AmplifyAnimationPack
{

    public class TextureScroller : MonoBehaviour
    {
        private Material mat;
        private float textureOffset;

        void Start()
        {
            mat = GetComponent<Renderer>().material;
        }

        void Update()
        {
            textureOffset += Time.deltaTime * 0.5f;
            mat.SetTextureOffset( "_MainTex" , -Vector2.up * textureOffset );
            mat.SetTextureOffset( "_MetallicGlossMap" , -Vector2.up * textureOffset );
            mat.SetTextureOffset( "_DetailAlbedoMap" , -Vector2.up * textureOffset );
            mat.SetTextureOffset( "_DetailNormalMap" , -Vector2.up * textureOffset );
        }

        void OnValidate()
        {
            // flip the uvs for cube side that turns them upside down, not used on gameplay
            MeshFilter meshComp = GetComponent<MeshFilter>();
            Vector2[] uvs = meshComp.sharedMesh.uv;

            uvs[ 6 ] = new Vector2( 0 , 0 );
            uvs[ 7 ] = new Vector2( 1 , 0 );
            uvs[ 10 ] = new Vector2( 0 , 1 );
            uvs[ 11 ] = new Vector2( 1 , 1 );

            meshComp.sharedMesh.uv = uvs;
        }
    }
}
