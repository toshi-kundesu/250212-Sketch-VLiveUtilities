using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    /// <summary>
    /// フェード時に表示するパターンを切り替える列挙型
    /// </summary>
    public enum FadeMode
    {
        Black,
        CustomTexture,
        MacbethChecker
    }

    [Header("キー入力・フェード設定")]
    [SerializeField] private KeyCode fadeKey      = KeyCode.Space; 
    [SerializeField] private float  fadeDuration  = 1.0f;
    [SerializeField] private bool   fadeOut       = false;  // 初期は黒くない (false) → 1回目Spaceで黒へ

    [Header("どの描画モードでフェードする？")]
    [SerializeField] private FadeMode fadeMode = FadeMode.Black;

    [Header("カスタムテクスチャ (FadeMode = CustomTexture)")]
    [SerializeField] private Texture2D customTexture; 

    [Header("Macbeth Checker (FadeMode = MacbethChecker)")]
    [SerializeField] private int   rows        = 4;
    [SerializeField] private int   cols        = 6;
    [SerializeField] private float borderWidth = 25f;
    [SerializeField] private Color[] chartColors = new Color[24]
    {
        // 初期値=Macbethカラーチャート近似24色 (6行×4列)
        new Color32(115,  82,  68, 255), // Dark Skin
        new Color32(194, 150, 130, 255), // Light Skin
        new Color32( 98, 122, 157, 255), // Blue Sky
        new Color32( 87, 108,  67, 255), // Foliage
        new Color32(133, 128, 177, 255), // Blue Flower
        new Color32(103, 189, 170, 255), // Bluish Green
        new Color32(214, 126,  44, 255), // Orange
        new Color32( 80,  91, 166, 255), // Purplish Blue
        new Color32(193,  90,  99, 255), // Moderate Red
        new Color32( 94,  60, 108, 255), // Purple
        new Color32(157, 188,  64, 255), // Yellow Green
        new Color32(224, 163,  46, 255), // Orange Yellow
        new Color32( 56,  61, 150, 255), // Blue
        new Color32( 70, 148,  73, 255), // Green
        new Color32(175,  54,  60, 255), // Red
        new Color32(231, 199,  31, 255), // Yellow
        new Color32(187,  86, 149, 255), // Magenta
        new Color32(  8, 133, 161, 255), // Cyan
        new Color32(243, 243, 242, 255), // White
        new Color32(200, 200, 200, 255), // Neutral 8
        new Color32(160, 160, 160, 255), // Neutral 6.5
        new Color32(122, 122, 121, 255), // Neutral 5
        new Color32( 85,  85,  85, 255), // Neutral 3.5
        new Color32( 52,  52,  52, 255), // Black
    };

    // フェード中フラグ & アルファ
    private bool   isFading    = false; 
    private float  elapsed     = 0f;    
    private float  fadeAlpha   = 0f;    

    // 黒テクスチャ & カスタム用テクスチャ
    private Texture2D blackTexture;  
    private Texture2D fadeTexture;   

    // Macbeth用に 1x1 の白＆黒テクスチャ
    private Texture2D whiteTex;
    private Texture2D blackTex;

    private void Start()
    {
        // デフォルト黒テクスチャを作成
        blackTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();

        // fadeTexture はカスタムが指定されていればそれを使い、なければ黒テクスチャを使う
        fadeTexture = (customTexture != null) ? customTexture : blackTexture;

        // Macbeth用の白と黒テクスチャ
        whiteTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.Apply();

        blackTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        blackTex.SetPixel(0, 0, Color.black);
        blackTex.Apply();
    }

    private void Update()
    {
        // キー押下でフェード開始 (トグル)
        if (!isFading && Input.GetKeyDown(fadeKey))
        {
            isFading = true;
            fadeOut  = !fadeOut;
            elapsed  = 0f;
        }

        // フェード中のアルファ補間
        if (isFading)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            if (fadeOut)
            {
                // clear → 表示（黒/テクスチャ/チェッカー 急増）
                fadeAlpha = Mathf.Lerp(0f, 1f, t);
            }
            else
            {
                // 表示 → clear（黒/テクスチャ/チェッカー 減衰）
                fadeAlpha = Mathf.Lerp(1f, 0f, t);
            }

            // フェード完了
            if (t >= 1f)
            {
                isFading = false;
            }
        }
    }

    private void OnGUI()
    {
        // フェードαが0以下なら何も描画しない
        if (fadeAlpha <= 0f) return;

        // GUI.color のα成分のみフェードαを適用
        GUI.color = new Color(1f, 1f, 1f, fadeAlpha);

        switch (fadeMode)
        {
            case FadeMode.Black:
                // 全面を黒テクスチャで覆う
                if (blackTexture != null)
                {
                    GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
                }
                break;

            case FadeMode.CustomTexture:
                // 全面を customTexture(ユーザ定義) かデフォルト黒テクスチャで覆う
                if (fadeTexture != null)
                {
                    GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeTexture);
                }
                break;

            case FadeMode.MacbethChecker:
                // 画面を rows×cols に分割して描画
                DrawMacbethChecker();
                break;
        }

        // 他のGUIへの影響を防ぎ、色を白に戻す
        GUI.color = Color.white;
    }

    /// <summary>
    /// 画面を rows×cols に均等分割し、仕切り線と各マス色を描画
    /// </summary>
    private void DrawMacbethChecker()
    {
        float totalW = Screen.width;
        float totalH = Screen.height;

        float patchW = totalW / cols;
        float patchH = totalH / rows;

        int maxCount = Mathf.Min(chartColors.Length, rows * cols);

        for (int i = 0; i < maxCount; i++)
        {
            int r = i / cols;  
            int c = i % cols;  

            float x = c * patchW;
            float y = r * patchH;

            // 1) 仕切り線用に、マス全体を黒塗り
            GUI.color = new Color(1, 1, 1, fadeAlpha);
            GUI.DrawTexture(new Rect(x, y, patchW, patchH), blackTex);

            // 2) 内側に色パッチを描画
            float innerX = x + borderWidth;
            float innerY = y + borderWidth;
            float innerW = patchW - borderWidth * 2f;
            float innerH = patchH - borderWidth * 2f;

            if (innerW > 0 && innerH > 0)
            {
                GUI.color = new Color(
                    chartColors[i].r,
                    chartColors[i].g,
                    chartColors[i].b,
                    fadeAlpha // ← フェード値を掛ける
                );
                GUI.DrawTexture(new Rect(innerX, innerY, innerW, innerH), whiteTex);
            }
        }
    }
}