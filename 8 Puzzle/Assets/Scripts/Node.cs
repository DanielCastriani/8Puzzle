
using System.Collections.Generic;

namespace Assets.Scripts {
    class Node {
        private List<Node> _lig;
        public List<Node> lig { get { return _lig; } set { _lig = value; } }

        private List<Jogada> _jogadas;
        public List<Jogada> jogadas { get { return _jogadas; } set { _jogadas = value; } }

        public Node() {
            lig = new List<Node>();
            jogadas = new List<Jogada>();
        }

        public void add(Jogada j) {
            
            lig.Add(new Node());
        }


    }
}
