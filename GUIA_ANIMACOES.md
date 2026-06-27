# Guia — Animações de Sprites (Player e Mobs)

Scripts criados: `Assets/Scripts/VFX/SpriteAnimator.cs` e `Assets/Scripts/VFX/DeathAnimHelper.cs`

---

## O que esses scripts fazem

| Efeito | Descrição |
|---|---|
| **Idle** | Sprite pulsa levemente — dá sensação de estar vivo |
| **Andar** | Sprite vira automaticamente pro lado que está se movendo |
| **Morte** | Cria um "fantasma" que gira, encolhe e some em vermelho |

> O flash vermelho ao receber dano já existe no script `DamageFlash` — não precisa fazer nada.

---

## Como adicionar no Player

1. Seleciona o **Player** na **Hierarchy**
2. No **Inspector** → **Add Component** → digita `Sprite Animator`
3. Seleciona o componente e aparece
4. Pronto — funciona automático

---

## Como adicionar nos Mobs

Repete o mesmo processo para cada mob/inimigo:

1. Seleciona o GameObject do mob na **Hierarchy**
2. **Inspector** → **Add Component** → `Sprite Animator`
3. Pronto

Se você usar **Prefabs** para os mobs (recomendado), adiciona o componente no Prefab uma vez e todos os mobs do mapa herdam automaticamente:

1. Abre o Prefab do mob (duplo clique no Prefab na pasta **Project**)
2. **Add Component** → `Sprite Animator`
3. Salva o Prefab (**Ctrl+S**)

---

## Ajustes disponíveis no Inspector

Após adicionar o `Sprite Animator`, você pode ajustar sem mexer no código:

| Campo | O que faz | Valor padrão |
|---|---|---|
| **Idle Ativo** | Liga/desliga o pulso | ✓ ligado |
| **Amplitude Pulso** | Intensidade do pulso (0 = nenhum, 0.1 = muito) | 0.03 |
| **Velocidade Pulso** | Velocidade do pulso | 1.2 |
| **Flip Ativo** | Liga/desliga o flip ao andar | ✓ ligado |
| **Duracao Morte** | Duração da animação de morte em segundos | 0.45 |

---

## Teste

- [ ] Player pulsa quando parado
- [ ] Player vira o sprite ao andar pra esquerda/direita
- [ ] Mob pulsa quando parado
- [ ] Ao matar um mob, aparece o efeito de giro + fade antes de sumir
- [ ] Nenhum erro vermelho no **Console**
