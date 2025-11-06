TEXTURE2D(_CameraSortingLayerTexture);
SAMPLER(sampler_CameraSortingLayerTexture);

float4 _CameraSortingLayerTexture_TexelSize;

void Blur_float(float2 uv, float blurAmount, out float4 OUT)
{
    float2 offset = _CameraSortingLayerTexture_TexelSize.xy * blurAmount;

    float4 final = 0;
    int cnt = 0;
    for (int x = -5; x <= 5; x++)
    {
        for (int y = -5; y <= 5; y++)
        {
            final += SAMPLE_TEXTURE2D(
                _CameraSortingLayerTexture,
                sampler_CameraSortingLayerTexture,
                float2(offset.x * x, offset.y * y) + uv);

            cnt += 1;
        }
    }

    final /= cnt;

    OUT = final;
}