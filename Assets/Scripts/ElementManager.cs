using System.Collections.Generic;
using UnityEngine;
using TMPro; // LINHA NOVA: Necessária para controlar o texto no ecrã!

public enum ElementoMagico { Fogo, Agua, Terra, Raio }

public class ElementManager : MonoBehaviour
{
    [Header("Configurações da Magia")]
    public List<ElementoMagico> filaDeMagia = new List<ElementoMagico>();
    public int limiteDeElementos = 3;

    [Header("Interface")]
    public TextMeshProUGUI txtMagias; // LINHA NOVA: Referência para o nosso texto no ecrã

    void Start()
    {
        AtualizarTexto(); // Garante que começa limpo
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) AdicionarElemento(ElementoMagico.Agua);
        if (Input.GetKeyDown(KeyCode.W)) AdicionarElemento(ElementoMagico.Fogo);
        if (Input.GetKeyDown(KeyCode.E)) AdicionarElemento(ElementoMagico.Terra);
        if (Input.GetKeyDown(KeyCode.R)) AdicionarElemento(ElementoMagico.Raio);
    }

    void AdicionarElemento(ElementoMagico novoElemento)
    {
        // REGRA DE CANCELAMENTO (OPOSTOS)
        if (novoElemento == ElementoMagico.Fogo && filaDeMagia.Contains(ElementoMagico.Agua))
        {
            filaDeMagia.Remove(ElementoMagico.Agua);
            AtualizarTexto();
            return;
        }
        if (novoElemento == ElementoMagico.Agua && filaDeMagia.Contains(ElementoMagico.Fogo))
        {
            filaDeMagia.Remove(ElementoMagico.Fogo);
            AtualizarTexto();
            return;
        }

        if (novoElemento == ElementoMagico.Terra && filaDeMagia.Contains(ElementoMagico.Raio))
        {
            filaDeMagia.Remove(ElementoMagico.Raio);
            AtualizarTexto();
            return;
        }
        if (novoElemento == ElementoMagico.Raio && filaDeMagia.Contains(ElementoMagico.Terra))
        {
            filaDeMagia.Remove(ElementoMagico.Terra);
            AtualizarTexto();
            return;
        }

        // ADICIONAR À FILA
        if (filaDeMagia.Count < limiteDeElementos)
        {
            filaDeMagia.Add(novoElemento);
            AtualizarTexto(); // Atualiza o ecrã sempre que adiciona com sucesso
        }
    }

    public void LimparFila()
    {
        filaDeMagia.Clear();
        AtualizarTexto();
    }

    // FUNÇÃO NOVA: Escreve a lista atual de magias no ecrã
    void AtualizarTexto()
    {
        if (txtMagias == null) return; // Evita erros se esquecer de associar

        if (filaDeMagia.Count == 0)
        {
            txtMagias.text = "Elementos: [ Nenhum ]";
            return;
        }

        string resultado = "Elementos: ";
        foreach (var elemento in filaDeMagia)
        {
            resultado += "[" + elemento.ToString() + "] ";
        }
        txtMagias.text = resultado;
    }
}