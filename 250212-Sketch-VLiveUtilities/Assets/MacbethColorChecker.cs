using UnityEngine;

public class MacbethColorCheckerCustom : MonoBehaviour
{
    [Header("表示設定")]
    [SerializeField] private int rows = 6;
    [SerializeField] private int cols = 4;
    [SerializeField] private float borderWidth = 2f; 
        // 黒い仕切り線の太さ (ピクセル)
    
    [Header("表示する色 (行×列 ぶん登録)")]
    [SerializeField]
    private Color[] chartColors = new Color[24]
    {
        // 6×4 = 24色の初期値（一般的なMacbethカラーチャート近似sRGB）
        // Row 1
        new Color32(115,  82,  68, 255), // Dark Skin
        new Color32(194, 150, 130, 255), // Light Skin
        new Color32( 98, 122, 157, 255), // Blue Sky
        new Color32( 87, 108,  67, 255), // Foliage
        // Row 2
        new Color32(133, 128, 177, 255), // Blue Flower
        new Color32(103, 189, 170, 255), // Bluish Green
        new Color32(214, 126,  44, 255), // Orange
        new Color32( 80,  91, 166, 255), // Purplish Blue
        // Row 3
        new Color32(193,  90,  99, 255), // Moderate Red
        new Color32( 94,  60, 108, 255), // Purple
        new Color32(157, 188,  64, 255), // Yellow Green
        new Color32(224, 163,  46, 255), // Orange Yellow
        // Row 4
        new Color32( 56,  61, 150, 255), // Blue
        new Color32( 70, 148,  73, 255), // Green
        new Color32(175,  54,  60, 255), // Red
        new Color32(231, 199,  31, 255), // Yellow
        // Row 5
        new Color32(187,  86, 149, 255), // Magenta
        new Color32(  8, 133, 161, 255), // Cyan
        new Color32(243, 243, 242, 255), // White
        new Color32(200, 200, 200, 255), // Neutral 8
        // Row 6
        new Color32(160, 160, 160, 255), // Neutral 6.5
        new Color32(122, 122, 121, 255), // Neutral 5
        new Color32( 85,  85,  85, 255), // Neutral 3.5
        new Color32( 52,  52,  52, 255), // Black
    };

    /// <summary>
    /// 1x1 の白テクスチャ（色塗りつぶし用）
    /// </summary>
    private Texture2D whiteTexture;
    
    /// <summary>
    /// 1x1 の黒テクスチャ（仕切り線用）
    /// </summary>
    private Texture2D blackTexture;

    private void Awake()
    {
        // 白テクスチャを動的に作成
        whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();

        // 黒テクスチャを動的に作成
        blackTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();
    }

    private void OnGUI()
    {
        // 画面全体を rows × cols で区切って表示
        float totalW = Screen.width;    // 画面幅全体
        float totalH = Screen.height;   // 画面高さ全体

        float patchW = totalW / cols;   // 各色パッチの幅
        float patchH = totalH / rows;   // 各色パッチの高さ

        // chartColors のうち、実際に描画する数を計算
        // (配列内に色が足りない場合は描画しないマスが発生)
        int maxCount = Mathf.Min(chartColors.Length, rows * cols);

        for (int i = 0; i < maxCount; i++)
        {
            int row = i / cols;  
            int col = i % cols;  

            float x = col * patchW;
            float y = row * patchH;

            // ▼ 1) 黒い仕切り線として、マス全体を黒テクスチャで塗る
            GUI.color = Color.white; // 黒テクスチャの色をそのまま表示するため white に
            GUI.DrawTexture(new Rect(x, y, patchW, patchH), blackTexture);

            // ▼ 2) 仕切り線より内側に、個々の色パッチを描画
            //     borderWidth ピクセル分内側にオフセットしてサイズを縮む
            float innerX = x + borderWidth;
            float innerY = y + borderWidth;
            float innerW = patchW - borderWidth * 2f;
            float innerH = patchH - borderWidth * 2f;

            if (innerW > 0f && innerH > 0f)
            {
                GUI.color = chartColors[i];
                GUI.DrawTexture(new Rect(innerX, innerY, innerW, innerH), whiteTexture);
            }
        }

        // 他のGUI描画への影響を防ぐため、色をリセット
        GUI.color = Color.white;
    }
}