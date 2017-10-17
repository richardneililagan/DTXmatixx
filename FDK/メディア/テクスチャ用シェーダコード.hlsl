// 定数バッファのデータ定義
cbuffer cbCBuffer : register(b0)
{ // 常にスロット「0」を使う
    matrix World;      // ワールド変換行列
    matrix View;       // ビュー変換行列
    matrix Projection; // 透視変換行列
    float TexLeft;     // 描画元矩形の左u座標
    float TexTop;      // 描画元矩形の上v座標
    float TexRight;    // 描画元矩形の右u座標
    float TexBottom;   // 描画元矩形の下v座標
    float TexAlpha;    // テクスチャ全体に乗じるアルファ値(0～1)
};

Texture2D myTex2D; // テクスチャ

// サンプラ
SamplerState smpWrap : register(s0);

// ピクセルシェーダの入力データ定義
struct PS_INPUT
{
    float4 Pos : SV_POSITION; // 頂点座標(透視座標系)
    float2 Tex : TEXCOORD0;   // テクスチャ座標
};

// 頂点シェーダの関数
PS_INPUT VS(uint vID : SV_VertexID)
{
    PS_INPUT vt;
    
    // 頂点座標（モデル座標系）の生成
    switch (vID)
    {
        case 0:
            vt.Pos = float4(-0.5, 0.5, 0.0, 1.0); // 左上
            vt.Tex = float2(TexLeft, TexTop);
            break;
        case 1:
            vt.Pos = float4(0.5, 0.5, 0.0, 1.0); // 右上
            vt.Tex = float2(TexRight, TexTop);
            break;
        case 2:
            vt.Pos = float4(-0.5, -0.5, 0.0, 1.0); // 左下
            vt.Tex = float2(TexLeft, TexBottom);
            break;
        case 3:
            vt.Pos = float4(0.5, -0.5, 0.0, 1.0); // 右下
            vt.Tex = float2(TexRight, TexBottom);
            break;
    }

    // ワールド・ビュー・射影変換
    vt.Pos = mul(vt.Pos, World);
    vt.Pos = mul(vt.Pos, View);
    vt.Pos = mul(vt.Pos, Projection);

    // 出力
    return vt;
}

// ピクセルシェーダの関数
float4 PS(PS_INPUT input) : SV_TARGET
{
    // テクスチャ取得
    float4 texCol = myTex2D.Sample(smpWrap, input.Tex); // テクセル読み込み
    texCol.a *= TexAlpha; // アルファを乗算

    // 色
    return saturate(texCol);
}
