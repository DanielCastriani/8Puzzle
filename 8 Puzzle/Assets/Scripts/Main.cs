using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {
    #region Var Controle
    private GameObject[] obj;
    private Sprite[] sprites;
    private int qtd_random_;
    private bool inicio;
    private int qtd_mov;
    private float tempo;
    private int modo;

    public float space = 2.8f;
    public int qtd_random = 100;
    public float delay_time = 0;
    public float delay_embaralha = 0.001f;
    public GameObject pref_peca;
    public Text txGanhou;
    public Text txTempo;
    public Text txQtdMovimentos;
    public Text txCaminho;
    public Button btInicio;
    public Button btForcaBruta;
    public Button btHeuristica;
    public AudioSource musicaPrincipal;
    public AudioSource musicaVitoria;
    #endregion

    #region Var Jogo
    private int[,] tabuleiro;
    private int px, py;
    private bool encontrado;
    private Jogada caminho;
    #endregion

    #region Start e Update
    void Start() {
        int seed = Random.Range(-5000, 5000);
        Random.InitState(seed);
        inicio = false;
        musicaPrincipal.Play();
        obj = new GameObject[9];
        sprites = new Sprite[10];
        for (int i = 0; i < 9; i++) {
            sprites[i] = (Sprite)Resources.Load("Sprite/P" + (i + 1), typeof(Sprite));
        }
        sprites[8] = (Sprite)Resources.Load("Sprite/Espaco", typeof(Sprite));
        sprites[9] = (Sprite)Resources.Load("Sprite/P9", typeof(Sprite));
        tabuleiro = new int[3, 3];
        int index = 0;
        for (int y = 0; y < 3; y++) {
            for (int x = 0; x < 3; x++) {
                Vector3 v = toVector3(x, y);
                GameObject go = Instantiate(pref_peca, v, Quaternion.identity);
                go.GetComponent<SpriteRenderer>().sprite = sprites[index];

                if (index < 9) {
                    go.name = "Peca " + index;
                } else {
                    go.name = "Espaco ";
                }
                obj[index] = go;
                tabuleiro[y, x] = index;

                index++;
            }

        }
        px = py = 2;
        refresh_obj();
        StartCoroutine(update_labels());
    }

    private IEnumerator update_labels() {

        while (true) {
            if (inicio) {
                int m = (int)(tempo / 60);
                int s = (int)(tempo % 60);
                int ms = (int)((tempo - ((int)tempo)) * 1000);
                txTempo.text = string.Format("Tempo: {0:00}:{1:00}:{2:000000}", m, s, ms);

            }

            txQtdMovimentos.text = "Movimentos: " + qtd_mov;
            yield return new WaitForSeconds(0.00001f);
        }
    }

    void Update() {

        if (inicio) {
            if (!ganhou()) {
                if (modo == 0) {
                    kb_ctr();
                    tempo += Time.deltaTime;
                } else {
                    caminho = modo == 1 ? brute_force() : heuristica();
                    exibe(caminho);
                }
            }
        }
    }
    #endregion
    #region BTNS
    private void init_tabuleiro() {
        tabuleiro = new int[3, 3];
        int i = 0;
        for (int y = 0; y < 3; y++) {
            for (int x = 0; x < 3; x++) {
                tabuleiro[y, x] = i;
                i++;
            }
        }
        px = 2;
        py = 2;
    }

    private void init() {
        if (!musicaPrincipal.isPlaying) {
            musicaVitoria.Stop();
            musicaPrincipal.Play();
        }
        txCaminho.text = "";
        obj[8].GetComponent<SpriteRenderer>().sprite = sprites[8];
        txGanhou.enabled = false;
        qtd_random_ = qtd_random;
        qtd_mov = 0;
        caminho = null;
        init_tabuleiro();
        inicio = false;
        StartCoroutine(Random_move());
    }

    public void OnAction_iniciar() {
        init();
        tempo = 0;
        modo = 0;
    }
    public void OnAction_Start_BruteForce() {
        init();
        modo = 1;
        btForcaBruta.enabled = false;
        btHeuristica.enabled = false;
        btInicio.enabled = false;
    }
    public void OnAction_heuristica() {
        init();
        modo = 2;
        btForcaBruta.enabled = false;
        btHeuristica.enabled = false;
        btInicio.enabled = false;
    }
    #endregion
    #region IA

    private Jogada brute_force() {
        int[,] tab = new int[3, 3];

        for (int y = 0; y < 3; y++) {
            for (int x = 0; x < 3; x++) {
                tab[y, x] = tabuleiro[y, x];
            }
        }

        List<char> possiveis;
        Queue<Jogada> q = new Queue<Jogada>();

        Jogada atual = new Jogada(tab, null, 'x');
        q.Enqueue(atual);
        int count = 0;
        while (q.Count > 0) {
            atual = q.Dequeue();
            tab = atual.tabuleiro;

            int ex, ey;
            getPosEspaco(tab, out ex, out ey);
            possiveis = movimentos_possiveis(atual, ex, ey);

            for (int i = 0; i < possiveis.Count; i++) {
                char mov = possiveis[i];
                int[,] ntab = mover(tab, mov, ex, ey);
                Jogada nova = new Jogada(ntab, atual, mov);
                if (!atual.existe(ntab)) {
                    q.Enqueue(nova);
                }
                count++;
                if (ganhou(nova)) {
                    txCaminho.text = "Caminho Encontrado\nGerou " + count + " filhos\n";
                    return nova;
                }

            }
        }
        Debug.Log("Terminou mais deu ruim");
        return atual;
    }
    private Jogada heuristica() {
        int[,] tab = new int[3, 3];

        for (int y = 0; y < 3; y++) {
            for (int x = 0; x < 3; x++) {
                tab[y, x] = tabuleiro[y, x];
            }
        }

        List<char> possiveis;
        List<Jogada> q = new List<Jogada>();

        Jogada atual = new Jogada(tab, null, 'x');
        q.Add(atual);

        int count = 0;

        while (q.Count > 0) {
            atual = q[0];
            q.RemoveAt(0);

            tab = atual.tabuleiro;

            int ex, ey;
            getPosEspaco(tab, out ex, out ey);
            possiveis = movimentos_possiveis(atual, ex, ey);

            for (int i = 0; i < possiveis.Count; i++) {

                char mov = possiveis[i];
                Jogada nova = new Jogada(mover(tab, mov, ex, ey), atual, mov);
                if (!atual.existe(nova.tabuleiro)) {
                    q.Add(nova);
                    q.Sort(delegate (Jogada a, Jogada b) { return a.prioridade < b.prioridade ? -1 : 0; });
                }
                count++;
                if (ganhou(q[0])) {
                    txCaminho.text = "Caminho Encontrado\nGerou " + count + " filhos\n";
                    return q[0];
                }

            }
        }
        Debug.Log("Terminou mais deu ruim");
        return atual;
    }
    #endregion
    #region Movimento
    #region Auto

    void getPosEspaco(int[,] tab, out int x, out int y) {
        x = y = 0;
        for (y = 0; y < 3; y++) {
            for (x = 0; x < 3; x++) {
                if (tab[y, x] == 8) {
                    return;
                }
            }
        }
    }
    bool permitido(char dir, Jogada j, int x, int y) {
        int[,] tab = j.tabuleiro;

        switch (dir) {
            case 'd':
                if (y + 1 < 3)
                    return true;
                break;
            case 'u':
                if (y - 1 >= 0)
                    return true;
                break;
            case 'l':
                if (x + 1 < 3)
                    return true;
                break;
            case 'r':
                if (x - 1 >= 0)
                    return true;
                break;
        }
        return false;
    }
    int[,] mover(int[,] tab, char d, int ex, int ey) {

        int nx = ex;
        int ny = ey;
        int[,] ntab = new int[3, 3];
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                ntab[i, j] = tab[i, j];
            }
        }
        switch (d) {
            case 'd':
                ey++;
                break;
            case 'u':
                ey--;
                break;
            case 'l':
                ex++;
                break;
            case 'r':
                ex--;
                break;
        }
        ntab[ny, nx] = ntab[ey, ex];
        ntab[ey, ex] = 8;
        return ntab;
    }
    List<char> movimentos_possiveis(Jogada j, int x, int y) {
        List<char> c = new List<char>();
        if (permitido('d', j, x, y)) {
            c.Add('d');
        }
        if (permitido('u', j, x, y)) {
            c.Add('u');
        }
        if (permitido('l', j, x, y)) {
            c.Add('l');
        }
        if (permitido('r', j, x, y)) {
            c.Add('r');
        }
        return c;
    }
    bool ganhou(Jogada atual) {
        int[,] tab = atual.tabuleiro;
        int index = 0;
        for (int y = 0; y < 3; y++) {
            for (int x = 0; x < 3; x++) {
                if (tab[y, x] != index) {
                    return false;
                }
                index++;
            }
        }
        return true;
    }
    private void exibe(Jogada j) {
        if (j == null)
            return;
        Jogada aux = j;
        Stack<char> movs = new Stack<char>();
        while (aux.pai != null && aux.dir != 'x') {
            movs.Push(aux.dir);
            aux = aux.pai;
        }

        StartCoroutine(mover_exibe(movs));
        inicio = false;
    }

    private IEnumerator mover_exibe(Stack<char> movs) {
        while (movs.Count > 0) {

            char m = movs.Pop();
            switch (m) {
                case 'd':
                    m = 'u';
                    break;
                case 'u':
                    m = 'd';
                    break;
            }
            mover(m);
            refresh_obj();
            txCaminho.text += m + " - ";
            ganhou();
            yield return new WaitForSeconds(delay_time);
        }
    }

    #endregion
    #region Jogador
    bool permitido(char dir) {
        switch (dir) {
            case 'u':
                if (py + 1 < 3)
                    return true;
                break;
            case 'd':
                if (py - 1 >= 0)
                    return true;
                break;
            case 'l':
                if (px + 1 < 3)
                    return true;
                break;
            case 'r':
                if (px - 1 >= 0)
                    return true;
                break;
        }
        return false;
    }
    void mover(char dir) {
        if (permitido(dir)) {
            int nx = px;
            int ny = py;
            switch (dir) {
                case 'u':
                    py++;
                    break;
                case 'd':
                    py--;
                    break;
                case 'l':
                    px++;
                    break;
                case 'r':
                    px--;
                    break;
            }
            tabuleiro[ny, nx] = tabuleiro[py, px];
            tabuleiro[py, px] = 8;
            refresh_obj();
            qtd_mov++;
        }
    }
    List<char> movimentos_possiveis() {
        List<char> c = new List<char>();

        if (permitido('d')) {
            c.Add('d');
        }
        if (permitido('u')) {
            c.Add('u');
        }
        if (permitido('l')) {
            c.Add('l');
        }
        if (permitido('r')) {
            c.Add('r');
        }
        return c;
    }
    void kb_ctr() {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            mover('u');
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            mover('d');
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            mover('l');
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            mover('r');
        }
    }
    #endregion
    #endregion
    #region Jogo
    private void refresh_obj() {

        for (int y = 0; y < 3; y++) {
            for (int x = 0; x < 3; x++) {
                int index = tabuleiro[x, y];
                obj[index].transform.position = toVector3(x, y);
            }
        }
    }

    private bool ganhou() {
        int index = 0;
        for (int y = 0; y < 3; y++) {
            for (int x = 0; x < 3; x++) {
                if (tabuleiro[y, x] != index) {
                    return false;
                }
                index++;
            }
        }
        inicio = false;
        txGanhou.enabled = true;
        //obj[8].GetComponent<SpriteRenderer>().sprite = sprites[9];
        StartCoroutine(tocaMusica());

        btForcaBruta.enabled = true;
        btHeuristica.enabled = true;
        btInicio.enabled = true;
        return true;
    }

    private IEnumerator tocaMusica() {
        musicaPrincipal.Stop();
        musicaVitoria.Play();
        yield return new WaitForSeconds(9.5f);
        musicaPrincipal.Play();
    }


    private Vector3 toVector3(float x, float y) {
        x = 3 - x;
        return new Vector3(y * space, x * space, 0);
        //return new Vector3(x, y, 0);
    }

    private IEnumerator Random_move() {
        txCaminho.text = "Embaralhando!!!";
        //TabuleiroTeste();
        while (qtd_random_ > 0) {
            List<char> c = movimentos_possiveis();
            int i_mover = (int)(Random.value * 10 % c.Count);
            mover(c[i_mover]);
            qtd_random_--;
            qtd_mov--;
            yield return new WaitForSeconds(delay_embaralha);
        }
        tempo = 0;
        txCaminho.text = "";
        inicio = true;
    }

    private void TabuleiroTeste() {

        qtd_random_ = 0;
        /*
        tabuleiro[0, 0] = 8; tabuleiro[0, 1] = 1; tabuleiro[0, 2] = 0;
        tabuleiro[1, 0] = 3; tabuleiro[1, 1] = 4; tabuleiro[1, 2] = 2;
        tabuleiro[2, 0] = 7; tabuleiro[2, 1] = 6; tabuleiro[2, 2] = 5;
        
        tabuleiro[0, 0] = 0; tabuleiro[0, 1] = 2; tabuleiro[0, 2] = 1;
        tabuleiro[1, 0] = 7; tabuleiro[1, 1] = 8; tabuleiro[1, 2] = 3;
        tabuleiro[2, 0] = 6; tabuleiro[2, 1] = 5; tabuleiro[2, 2] = 4;
        */

        tabuleiro[0, 0] = 8; tabuleiro[0, 1] = 7; tabuleiro[0, 2] = 1;
        tabuleiro[1, 0] = 0; tabuleiro[1, 1] = 4; tabuleiro[1, 2] = 5;
        tabuleiro[2, 0] = 3; tabuleiro[2, 1] = 6; tabuleiro[2, 2] = 2;

        getPosEspaco(tabuleiro, out px, out py);
        refresh_obj();
    }
    
    #endregion
}

