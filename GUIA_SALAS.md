# Guia de Criação de Salas — FaseTerra

> **Fui dormir cedim po: 03:30 do dia 26/06 kkkkk — boa sorte aí**

> Para ler esse arquivo formatado no VS Code: **Ctrl + Shift + V**

> Esse guia é para continuar a montagem das salas do jogo.  
> Todos os scripts já estão prontos — é só seguir os passos.

---

## Passo 0 — Instalar o Python (se não tiver)

O script de remoção de fundo precisa do Python instalado.

1. Acessa **python.org/downloads**
2. Clica no botão amarelo **"Download Python 3.x.x"**
3. Executa o instalador
4. **IMPORTANTE:** marca a caixinha **"Add Python to PATH"** antes de instalar
5. Abre o PowerShell e roda:
```
py -m pip install Pillow
```

Se der erro no `py`, fecha e abre o PowerShell de novo.

---

## Passo 1 — Remover fundo preto dos PNGs

Os PNGs das salas estão em `Assets/mapa` e têm fundo preto que precisa virar transparente.

**Abra o PowerShell e rode:**
```
cd C:\Users\junio\Downloads\jogo-unity
py remover_fundo_preto.py
```

Quando terminar, volte no Unity, clique com botão direito na pasta `mapa` → **Reimport All**.

> Só precisa fazer isso uma vez. Se já foi feito, pula para o Passo 2.

---

## Passo 2 — Criar uma sala

1. No Unity, clique no menu **`Jogo → Criador de Sala`**
2. Na janela que abre:
   - **Nome da Sala** → coloca o nome (ex: `Sala01`, `Spawn`, `Sala_Boss`)
   - **Sprite do Chão** → arrasta o PNG da pasta `mapa`
3. Clica **Criar Sala**

A sala aparece na Scene com:
- O sprite como chão visual
- Paredes (`BoxCollider2D`) geradas automaticamente no formato do PNG
- `BoxCollider2D` trigger para detectar o player
- `RoomController` para controlar portas e inimigos

---

## Passo 3 — Criar um corredor entre salas

1. Na Hierarquia, clique com botão direito → **Create Empty** → renomeia para `Corredor_1`
2. Adiciona os componentes:
   - **Add Component** → `Box Collider 2D` → marca **Is Trigger**
   - **Add Component** → `Room Wall Generator`
3. No `Room Wall Generator`:
   - **Tamanho Corredor** → X = largura, Y = comprimento (ex: X=3, Y=5)
4. Clica com botão direito no `Room Wall Generator` → **"Gerar Corredor"**
5. Posiciona o corredor entre as duas salas na Scene

---

## Passo 4 — Criar a porta

A porta é um GameObject filho do corredor que **bloqueia** a passagem e **abre** quando os inimigos morrem.

1. Dentro do `Corredor_1`, cria um filho → **Create Empty** → renomeia para `Porta`
2. No `Porta`:
   - **Add Component** → `Box Collider 2D` (sem Is Trigger)
   - Ajusta o **Size** para cobrir a entrada do corredor
   - **Add Component** → `Sprite Renderer` → coloca um sprite de porta (ou deixa invisível por enquanto)
3. No próprio `Corredor_1`:
   - **Add Component** → `Door Controller`
   - No campo **Objeto Porta** → arrasta o GameObject `Porta`

> **Como funciona:** quando ativo, o objeto `Porta` bloqueia fisicamente o player.  
> `DoorController.Abrir()` desativa o objeto. `DoorController.Fechar()` ativa.

---

## Passo 5 — Conectar porta ao RoomController

1. Clica na sala (ex: `Spawn`)
2. No componente `Room Controller`:
   - Campo **Porta** → arrasta o `DoorController` do corredor
   - Campo **Spawner** → arrasta o `EnemySpawner` (se a sala tiver inimigos)

**Fluxo automático:**
1. Player entra na sala → porta fecha (após 0.6s)
2. Inimigos spawnam
3. Todos os inimigos morrem → porta abre

---

## Passo 6 — Adicionar inimigos (opcional)

1. Dentro da sala, cria um filho → **Create Empty** → renomeia para `Spawner_01`
2. **Add Component** → `Enemy Spawner`
3. No `Enemy Spawner`:
   - **Prefabs Inimigos** → arrasta os prefabs de `Assets/Prefabs/Enemies`
   - **Pontos de Spawn** → arrasta Transforms filhos que definem onde os inimigos aparecem
4. No `Room Controller` da sala → campo **Spawner** → arrasta o `Spawner_01`

---

## Estrutura esperada na Hierarquia

```
FaseTerra
  ├── Player
  ├── Spawn                        ← sala inicial
  │     ├── Chao                   ← SpriteRenderer com o PNG
  │     ├── Ph_0, Ph_1...          ← paredes horizontais (auto-geradas)
  │     └── Pv_0, Pv_1...          ← paredes verticais (auto-geradas)
  ├── Corredor_1                   ← corredor conectando salas
  │     ├── Porta                  ← objeto que bloqueia (controlado pelo DoorController)
  │     ├── Parede_Cima
  │     └── Parede_Baixo
  ├── Sala01
  │     ├── Chao
  │     ├── Spawner_01
  │     └── ...paredes...
  └── Fundo                        ← imagem de fundo do cenário
```

---

## Dicas rápidas

| Problema | Solução |
|---|---|
| Sala verde em vez do PNG | Clica no filho `Chao` → SpriteRenderer → troca o Sprite |
| Paredes bloqueando dentro da sala | As paredes `Ph_` e `Pv_` são auto-geradas; delete as que estiverem erradas |
| Player não detecta a sala | Verifica se o `BoxCollider2D` da raiz tem **Is Trigger** marcado |
| Porta não abre | Verifica se o `RoomController` tem referência ao `Door Controller` |
| Inimigos não spawnam | Verifica se o `RoomController` tem referência ao `Enemy Spawner` |
