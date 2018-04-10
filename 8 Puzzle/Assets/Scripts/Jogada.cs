using System;
using UnityEngine;

namespace Assets.Scripts {
    class Jogada {
        private int[,] _tabuleiro;
        public int[,] tabuleiro { get { return _tabuleiro; } set { _tabuleiro = value; } }

        private Jogada _pai;
        public Jogada pai { get { return _pai; } set { _pai = value; } }

        private char _dir;
        public char dir { get { return _dir; } set { _dir = value; } }

        private int fa,fc,p;
        public int prioridade { get { return p; } private set { p = value; } }

        public Jogada(int[,] tabuleiro, Jogada pai, char dir) {
            this._tabuleiro = tabuleiro;
            this._pai = pai;
            this._dir = dir;

            fa = manhattan();
            if (pai == null)
                fc = 0;
            else
                fc = pai.fc + 1;
            prioridade = fa + fc;
        }

        private bool mat_eq(int[,] mat1, int[,] mat2) {
            for (int y = 0; y < 3; y++) {
                for (int x = 0; x < 3; x++) {
                    if (mat1[y, x] != mat2[y, x]) {
                        return false;
                    }
                }
            }
            return true;
        }


        public bool existe(int[,] tab) {
            if (_pai != null) {
                for (Jogada j = this; j != null; j = j._pai) {
                    int[,] mat_p = j.tabuleiro;
                    if (mat_eq(tab, mat_p))
                        return true;
                }
            }
            return false;
        }

        private void pos_correta(int elm, out int x,out int y) {
            y = elm >= 0 && elm <= 2 ? 0 : (elm >= 3 && elm <= 5 ? 1 : 2);
            x = elm % 3;
        }

        private int manhattan() {
            int d = 0;
            int index = 0;
            for (int y = 0; y < 3;y++) {
                for (int x = 0; x < 3; x++) {
                    int elm = tabuleiro[y, x];
                    if (elm != index) {
                        int xf, yf;
                        pos_correta(elm, out xf, out yf);
                        int d_parcial = Math.Abs(xf - x) + Math.Abs(yf - y);
                        d += d_parcial;
                    }
                    index++;
                }
            }
            return d;
        }
    }
}
