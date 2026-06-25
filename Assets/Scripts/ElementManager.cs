using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ElementoMagico
{
    Fogo,
    Agua,
    Terra,
    Vento
}

public class ElementManager : MonoBehaviour
{
    [Header("Magia selecionada")]
    public List<ElementoMagico> filaDeMagia =
        new List<ElementoMagico>();

    [Header("Slot da magia")]
    public Image slotIcone;

    [Header("Ícones das Magias")]
    public Sprite iconeFogo;
    public Sprite iconeAgua;
    public Sprite iconeTerra;
    public Sprite iconeVento;

    private void Start()
    {
        AtualizarSlot();
    }

    private void Update()
    {
        // 1/2/3/4 — sem conflito com WASD de movimento
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SelecionarMagia(ElementoMagico.Agua);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SelecionarMagia(ElementoMagico.Fogo);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SelecionarMagia(ElementoMagico.Terra);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            SelecionarMagia(ElementoMagico.Vento);
    }

    private void SelecionarMagia(ElementoMagico elemento)
    {
        // Remove a magia anterior
        filaDeMagia.Clear();

        // Coloca somente uma magia no slot
        filaDeMagia.Add(elemento);

        AtualizarSlot();
    }

    public void LimparFila()
    {
        filaDeMagia.Clear();
        AtualizarSlot();
    }

    private void AtualizarSlot()
    {
        if (slotIcone == null)
        {
            Debug.LogWarning(
                "O Slot do ícone não foi associado no Inspector."
            );

            return;
        }

        // Nenhuma magia selecionada
        if (filaDeMagia.Count == 0)
        {
            slotIcone.sprite = null;
            slotIcone.enabled = false;
            return;
        }

        // Mostra a única magia selecionada
        ElementoMagico elemento = filaDeMagia[0];

        slotIcone.sprite = PegarIcone(elemento);
        slotIcone.preserveAspect = true;
        slotIcone.enabled = true;
    }

    private Sprite PegarIcone(ElementoMagico elemento)
    {
        switch (elemento)
        {
            case ElementoMagico.Fogo:
                return iconeFogo;

            case ElementoMagico.Agua:
                return iconeAgua;

            case ElementoMagico.Terra:
                return iconeTerra;

            case ElementoMagico.Vento:
                return iconeVento;

            default:
                return null;
        }
    }
}