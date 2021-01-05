using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthTexture : MonoBehaviour
{

    [SerializeField]
    private Shader _shader;
    private Material _material;
    [SerializeField] private Camera _camera;

    void Start()
    {
        // たとえばライトのShadow TypeがNo Shadowsのときなどに
        // これが設定されていないとデプステクスチャが生成されない
        GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

        _material = new Material(_shader);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        Graphics.Blit(source, dest, _material);
    }
}