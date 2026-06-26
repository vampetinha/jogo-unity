using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CriadorDeSala : EditorWindow
{
    private Sprite  spriteChao;
    private string  nomeSala      = "Sala";
    private int     resolucao     = 32;
    private float   espessura     = 0.25f;
    private float   insetParedes  = 0f;
    private Vector2 offsetParedes = Vector2.zero;

    [MenuItem("Jogo/Criador de Sala")]
    public static void AbrirJanela() => GetWindow<CriadorDeSala>("Criador de Sala");

    void OnGUI()
    {
        GUILayout.Label("Criador de Sala", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        nomeSala  = EditorGUILayout.TextField("Nome da Sala", nomeSala);
        spriteChao = (Sprite)EditorGUILayout.ObjectField("Sprite do Chão", spriteChao, typeof(Sprite), false);

        EditorGUILayout.Space();
        resolucao     = EditorGUILayout.IntSlider("Precisão das Paredes", resolucao, 16, 64);
        espessura     = EditorGUILayout.Slider("Espessura das Paredes", espessura, 0.1f, 0.5f);
        insetParedes  = EditorGUILayout.Slider("Inset das Paredes", insetParedes, -1f, 2f);
        offsetParedes = EditorGUILayout.Vector2Field("Offset das Paredes (X/Y fixo)", offsetParedes);
        EditorGUILayout.HelpBox("Inset: empurra para dentro da sala.\nOffset: desloca todas as paredes num eixo fixo.", MessageType.None);

        EditorGUILayout.Space();
        GUI.enabled = spriteChao != null;
        if (GUILayout.Button("Criar Sala", GUILayout.Height(40)))
            CriarSala();
        GUI.enabled = true;

        if (spriteChao == null)
            EditorGUILayout.HelpBox("Arraste um sprite no campo acima.", MessageType.Info);
        else
            EditorGUILayout.HelpBox(
                "O sprite precisa ter fundo transparente.\n" +
                "Rode o script remover_fundo_preto.py antes de usar.",
                MessageType.Info);
    }

    void CriarSala()
    {
        // Habilita leitura de pixels temporariamente
        string path = AssetDatabase.GetAssetPath(spriteChao.texture);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        bool eraLegivel = importer.isReadable;
        if (!eraLegivel) { importer.isReadable = true; importer.SaveAndReimport(); }

        bool[,] grid = ConstruirGrid(spriteChao.texture);

        if (!eraLegivel) { importer.isReadable = false; importer.SaveAndReimport(); }

        Vector2 tamanho = spriteChao.bounds.size;

        // Raiz da sala
        GameObject sala = new GameObject(nomeSala);
        Undo.RegisterCreatedObjectUndo(sala, "Criar Sala");

        // Trigger para o RoomController
        var trigger = sala.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size      = tamanho;
        sala.AddComponent<RoomController>();

        // Visual do chão
        var chao = new GameObject("Chao");
        chao.transform.SetParent(sala.transform);
        chao.transform.localPosition = Vector3.zero;
        chao.transform.localScale    = Vector3.one;
        var sr = chao.AddComponent<SpriteRenderer>();
        sr.sprite       = spriteChao;
        sr.sortingOrder = -1;

        // Paredes detectadas automaticamente pelo formato do sprite
        GerarParedes(sala, grid, tamanho);

        sala.transform.position = CentroSceneView();
        Selection.activeGameObject = sala;
        SceneView.lastActiveSceneView?.FrameSelected();
        Debug.Log($"[CriadorDeSala] '{nomeSala}' criada com paredes automáticas.");
    }

    // ── Grid de pixels ────────────────────────────────────────────────

    bool[,] ConstruirGrid(Texture2D tex)
    {
        Color[] pixels = tex.GetPixels();
        int w = tex.width, h = tex.height;
        int res = resolucao;
        bool[,] grid = new bool[res, res];

        for (int gy = 0; gy < res; gy++)
        for (int gx = 0; gx < res; gx++)
        {
            int x0 = gx * w / res,       x1 = Mathf.Min((gx + 1) * w / res, w);
            int y0 = gy * h / res,       y1 = Mathf.Min((gy + 1) * h / res, h);
            for (int py = y0; py < y1; py++)
            for (int px = x0; px < x1; px++)
                if (pixels[py * w + px].a > 0.05f) { grid[gx, gy] = true; goto prox; }
            prox:;
        }
        return grid;
    }

    // ── Detecção de arestas e criação de paredes ──────────────────────

    void GerarParedes(GameObject sala, bool[,] grid, Vector2 worldSize)
    {
        int    res = resolucao;
        float  cw  = worldSize.x / res;
        float  ch  = worldSize.y / res;
        float  ox  = -worldSize.x / 2f;
        float  oy  = -worldSize.y / 2f;
        float  esp = espessura;
        int    idx = 0;

        // Arestas horizontais (entre linha gy-1 e gy)
        for (int gy = 0; gy <= res; gy++)
        {
            int inicio = -1;
            for (int gx = 0; gx <= res; gx++)
            {
                bool cima  = gy < res && gx < res && grid[gx, gy];
                bool baixo = gy > 0   && gx < res && grid[gx, gy - 1];
                bool aresta = cima != baixo;

                if (aresta  && inicio < 0) inicio = gx;
                if (!aresta && inicio >= 0)
                {
                    float cx = ox + (inicio + gx) * cw / 2f;
                    float cy = oy + gy * ch;
                    float tw = (gx - inicio) * cw;
                    Parede(sala, $"Ph_{idx++}", new Vector2(cx, cy), new Vector2(tw, esp));
                    inicio = -1;
                }
            }
        }

        // Arestas verticais (entre coluna gx-1 e gx)
        for (int gx = 0; gx <= res; gx++)
        {
            int inicio = -1;
            for (int gy = 0; gy <= res; gy++)
            {
                bool esq = gx > 0   && gy < res && grid[gx - 1, gy];
                bool dir = gx < res && gy < res && grid[gx,     gy];
                bool aresta = esq != dir;

                if (aresta  && inicio < 0) inicio = gy;
                if (!aresta && inicio >= 0)
                {
                    float cx = ox + gx * cw;
                    float cy = oy + (inicio + gy) * ch / 2f;
                    float th = (gy - inicio) * ch;
                    Parede(sala, $"Pv_{idx++}", new Vector2(cx, cy), new Vector2(esp, th));
                    inicio = -1;
                }
            }
        }
    }

    void Parede(GameObject pai, string nome, Vector2 pos, Vector2 tam)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(pai.transform);

        Vector2 posFinal = pos;
        if (!Mathf.Approximately(insetParedes, 0f) && pos != Vector2.zero)
            posFinal += (-pos.normalized) * insetParedes;
        posFinal += offsetParedes;

        go.transform.localPosition = new Vector3(posFinal.x, posFinal.y, 0f);
        go.transform.localScale    = Vector3.one;
        go.AddComponent<BoxCollider2D>().size = tam;
    }

    static Vector3 CentroSceneView()
    {
        var sv = SceneView.lastActiveSceneView;
        return sv != null ? new Vector3(sv.camera.transform.position.x, sv.camera.transform.position.y, 0f) : Vector3.zero;
    }
}
