# Guia Completo de Configuração no Unity

---

## SEÇÃO A — Configurações Globais do Projeto (uma vez só)

### A1. Tags
`Edit > Project Settings > Tags and Layers > Tags`

Adicione:
- `Player`
- `Enemy`

### A2. Layers
Na mesma tela, em **Layers**:

| Slot | Nome |
|------|------|
| 6 | `Player` |
| 7 | `Enemy` |
| 8 | `Projectile` |
| 9 | `EnemyProjectile` |
| 10 | `Wall` |

### A3. Collision Matrix
`Edit > Project Settings > Physics 2D > Layer Collision Matrix`

Desmarque os pares abaixo (= eles NÃO colidem entre si):

| Layer A | Layer B | Por quê |
|---------|---------|---------|
| `Projectile` | `Player` | Projétil do player não acerta o próprio player |
| `Projectile` | `Projectile` | Projéteis não se destroem entre si |
| `EnemyProjectile` | `Enemy` | Projétil inimigo não acerta outros inimigos |
| `EnemyProjectile` | `EnemyProjectile` | Projéteis inimigos não se destroem entre si |
| `Enemy` | `Enemy` | Inimigos não empurram uns aos outros |

### A4. Build Settings
`File > Build Settings > Add Open Scenes`

Adicione nesta ordem:
1. `MenuPrincipal`
2. `FaseTerra`
3. `FaseMarte`
4. `FaseGoblins`
5. `FaseFortaleza`

---

## SEÇÃO B — ScriptableObjects de Arma

`Assets > botão direito > Create > Jogo > Arma` — crie 4 assets:

**Arma1_Pistola** (FaseTerra)
```
Nome: Pistola
Tipo de Ataque: Normal
Prefab Projetil: [Prefab_ProjetilPlayer]
Dano: 25   Velocidade: 14   Alcance: 9
Intervalo Disparo: 0.3
```

**Arma2_Automatica** (FaseMarte)
```
Nome: Metralhadora
Tipo de Ataque: Automatico
Prefab Projetil: [Prefab_ProjetilPlayer]
Dano: 15   Velocidade: 16   Alcance: 10
Intervalo Disparo: 0.12
```

**Arma3_Shotgun** (FaseGoblins)
```
Nome: Espingarda
Tipo de Ataque: Shotgun
Prefab Projetil: [Prefab_ProjetilPlayer]
Dano: 20   Velocidade: 13   Alcance: 6
Intervalo Disparo: 0.7
Quantidade Projeteis: 5   Dispersao: 30
```

**Arma4_LancaChamas** (FaseFortaleza — opcional)
```
Nome: Lança-Chamas
Tipo de Ataque: LancaChamas
Intervalo Disparo: 0.1
(dano configurado no FlamethrowerAttack)
```

---

## SEÇÃO C — Prefabs (criar na pasta Assets/Prefabs)

### C1. Prefab: ProjetilPlayer

1. `GameObject > Create Empty` → renomeie para `Prefab_ProjetilPlayer`
2. Adicione componentes:
   - **SpriteRenderer** — sprite de bola amarela/azul, `Order in Layer = 5`
   - **Rigidbody2D** — `Gravity Scale = 0`, `Collision Detection = Continuous`
   - **CircleCollider2D** — `Is Trigger = true`, raio ≈ 0.15
   - **Script: Projectile**
3. **Layer** do GameObject: `Projectile`
4. Arraste para `Assets/Prefabs` para criar o prefab, delete da cena

### C2. Prefab: ProjetilInimigo

1. Clone o `Prefab_ProjetilPlayer` → renomeie para `Prefab_ProjetilInimigo`
2. Troque o script `Projectile` por **`EnemyProjectile`**
3. Mude a cor do sprite (vermelho/laranja para distinguir)
4. **Layer** do GameObject: `EnemyProjectile`

### C3. Prefab: NumeroDeDano

1. `GameObject > Create Empty` → `Prefab_NumeroDeDano`
2. Adicione **TextMeshPro 3D** (não o UGUI):
   - `Component > Rendering > TextMeshPro - Text (3D)`
3. No componente TextMeshPro:
   - Texto: `0`
   - Alignment: Center
   - Font Size: 4–6
4. No **MeshRenderer** que o TMP cria automaticamente:
   - Sorting Layer: `Default`
   - Order in Layer: `20` (acima dos sprites de inimigos)
5. Adicione **Script: DamageNumber**
6. Salve como prefab

### C4. Prefab: InimigoMelee

1. `GameObject > Create Empty` → `Prefab_InimigoMelee`
2. **Tag**: `Enemy` | **Layer**: `Enemy`
3. Componentes:
   - **SpriteRenderer** — sprite de inimigo, `Order in Layer = 2`
   - **Rigidbody2D** — `Gravity Scale = 0`, `Angular Damping = 1000`, `Collision Detection = Continuous`
   - **CapsuleCollider2D** ou **CircleCollider2D** — tamanho do sprite
   - **HealthSystem** — `Vida Maxima = 60`
   - **EnemyController** — `Velocidade = 2.5`, `Distancia De Ataque = 1.2`, `Dano = 15`
   - **DamageFlash** — `Numero Flashes = 2`, `Duracao Flash = 0.05`
   - **DamageNumberSpawner** — `Prefab Numero = [Prefab_NumeroDeDano]`
   - **DeathVFX** — `Prefab VFX` = efeito de partícula (opcional)

### C5. Prefab: InimigoAtirador

1. Clone `Prefab_InimigoMelee` → `Prefab_InimigoAtirador`
2. **Remova** `EnemyController`
3. **Adicione** `EnemyShooter`:
   - `Prefab Projetil = [Prefab_ProjetilInimigo]`
   - `Dano Projetil = 15`, `Velocidade = 8`, `Alcance = 9`
   - `Distancia Minima = 4`, `Distancia Maxima = 7`
   - `Intervalo Disparo = 1.5`
4. Mude a cor do sprite para distinguir

### C6. Prefab: InimigoKamikaze

1. Clone `Prefab_InimigoMelee` → `Prefab_InimigoKamikaze`
2. **Remova** `EnemyController`
3. **Adicione** `EnemyExploder`:
   - `Velocidade = 4.5`
   - `Raio Ativacao = 1.2`, `Raio Explosao = 2.5`, `Dano Explosao = 60`
   - `Avisar Antes De Explodir = true`
4. Mude a cor do sprite (roxo/preto para indicar perigo)

### C7. Prefab: Boss

1. `GameObject > Create Empty` → `Prefab_Boss`
2. **Tag**: `Enemy` | **Layer**: `Enemy`
3. Componentes:
   - **SpriteRenderer** — sprite maior (scale ≈ 1.5x), `Order in Layer = 3`
   - **Rigidbody2D** — `Gravity Scale = 0`, `Angular Damping = 1000`
   - **CircleCollider2D** — raio ≈ 0.6
   - **HealthSystem** — `Vida Maxima = 500`
     - `morteManual` NÃO precisa marcar — o BossController faz isso em Start()
   - **BossController**:
     - `Nome Boss = "Senhor da Fortaleza"`
     - `Prefab Projetil = [Prefab_ProjetilInimigo]`
     - `Prefab VFX Morte` = efeito de explosão grande (opcional)
     - `Delay Vitoria = 1`
   - **DamageFlash** — `Numero Flashes = 1`, `Duracao = 0.05`
   - **DamageNumberSpawner** — `Prefab Numero = [Prefab_NumeroDeDano]`

> **IMPORTANTE**: O Boss deve ser **filho (child)** do GameObject com `RoomController`
> na cena, pois usa `GetComponentInParent<RoomController>()` para se registrar.

---

## SEÇÃO D — Cena: MenuPrincipal

### Hierarquia da Cena

```
MenuPrincipal (cena)
├── GameManager          ← singleton persistente
├── HUDManager           ← singleton persistente
├── AudioManager         ← singleton persistente
├── MenuUI               ← script MainMenuController
└── Main Camera
```

### Como montar

1. **Crie a cena** `MenuPrincipal` em `File > New Scene`

2. **GameObject vazio** → `GameManager`
   - Adicione script **GameManager**
   - `Vida Maxima = 100`
   - `Duracao Fade = 0.6`
   - `Tempo Nome = 2.5`
   - `Fonte Titulo` — TMP Font Asset (opcional)

3. **GameObject vazio** → `HUDManager`
   - Adicione script **HUDManager**
   - `Nome Cena Principal = "FaseTerra"`
   - `Nome Scene Menu = "MenuPrincipal"`
   - `Fonte` — TMP Font Asset (opcional)

4. **GameObject vazio** → `AudioManager`
   - Adicione script **AudioManager**
   - `Volume SFX = 1.0`
   - `Volume Musica = 0.5`
   - `Musica Menu` — AudioClip de música de menu (opcional)

5. **GameObject vazio** → `MenuUI`
   - Adicione script **MainMenuController**
   - `Nome Primeira Scene = "FaseTerra"`
   - O script cria toda a UI por código — não precisa adicionar Canvas

6. **Main Camera**: `Clear Flags = Solid Color`, cor escura (o menu cria fundo por código)

> `GameManager`, `HUDManager` e `AudioManager` ficam **somente nesta cena**.
> O `DontDestroyOnLoad` garante que eles sobrevivam para todas as outras cenas.

---

## SEÇÃO E — Cenas de Fase (FaseTerra, FaseMarte, FaseGoblins)

### Hierarquia Típica da Cena

```
FaseTerra (cena)
├── Tilemap / Paredes        ← layer "Wall", Collider2D não-trigger
├── Player                   ← tag "Player", layer "Player"
├── Sala_01
│   ├── [RoomController]     ← script no próprio GO + BoxCollider2D Trigger
│   ├── Porta_Norte          ← DoorController
│   ├── Porta_Sul            ← DoorController
│   ├── Spawner_01           ← EnemySpawner
│   └── SpawnPt_01 / _02     ← GameObjects vazios (pontos de spawn)
├── Sala_02
│   ├── [RoomController]
│   ├── Porta_Norte
│   ├── Spawner_01
│   └── Portal_ProximaFase   ← PortalController
└── Main Camera
```

### E1. Montar o Player na cena

1. Crie um **GameObject vazio** → `Player`
   - **Tag**: `Player` | **Layer**: `Player`

2. Adicione componentes ao Player:
   - **SpriteRenderer** — sprite do personagem, `Order in Layer = 2`
   - **Rigidbody2D** — `Gravity Scale = 0`, `Angular Damping = 1000`, `Collision Detection = Continuous`
   - **CapsuleCollider2D** — tamanho do personagem
   - **PlayerController** — `Velocidade = 5`
   - **HealthSystem** — `Vida Maxima = 100`
   - **SpellSystem**:
     - `Projetil Prefab` — prefab do Projetil das magias (script `Projetil.cs`)
     - `Ponto De Disparo` — Transform filho (ver passo 3)
     - `Element Manager` — arraste o próprio Player (componente ElementManager)
     - `Lanca Chamas` — arraste o próprio Player (componente FlamethrowerAttack)
   - **ElementManager**:
     - `Slot Icone` — Image do Canvas da cena para exibir o ícone da magia
     - Ícones: arraste sprites de fogo/água/terra/vento
   - **FlamethrowerAttack** — configure no Inspector
   - **WeaponController**:
     - `Ponto De Disparo` — mesmo Transform filho abaixo
     - `Lanca Chamas` — arraste o componente FlamethrowerAttack do Player
     - `Som Disparo` — AudioClip de tiro (opcional)
   - **DamageFlash** — `Numero Flashes = 3`, `Duracao Flash = 0.05`, `Som Dano` (opcional)
   - **DamageNumberSpawner** — `Prefab Numero = [Prefab_NumeroDeDano]`

3. Crie um **filho** do Player chamado `PontoDeDisparo`:
   - Posição local: `(0, 0.5, 0)` (um pouco à frente do centro)
   - Arraste este Transform nos campos `Ponto De Disparo` do SpellSystem e do WeaponController

4. **Conflito SpellSystem × WeaponController**:
   - **FaseTerra**: pode deixar ambos ativos (SpellSystem: teclas 1–4 + LMB para magia; WeaponController: LMB para arma)
   - **FaseMarte, FaseGoblins, FaseFortaleza**: desative o `SpellSystem` e o `ElementManager` no Inspector para evitar conflito de LMB

### E2. Montar uma Sala

1. **GameObject vazio** → `Sala_01`

2. No `Sala_01`:
   - Adicione **RoomController**
   - Adicione **BoxCollider2D** — `Is Trigger = true`, dimensões cobrindo o interior da sala

3. **Portas**: crie filhos do `Sala_01` para cada abertura:
   - **SpriteRenderer** — sprite de parede/porta
   - **BoxCollider2D** — `Is Trigger = false` (colisão sólida, bloqueia o player)
   - **DoorController** — `Cor Fechada = (0.15, 0.15, 0.15, 1)`, `Cor Aberta = (0, 0, 0, 0)`

4. No **RoomController** Inspector:
   - `Portas` → tamanho = número de portas, arraste os DoorController
   - `Spawners` → arraste os EnemySpawners desta sala
   - `Portal` → vazio (só na última sala)
   - `Boss` → vazio (só na sala do boss)

5. **Spawner_01**: GameObject vazio filho da sala
   - Adicione **EnemySpawner**
   - `Prefabs Inimigos` → arraste os prefabs de inimigos
   - `Pontos De Spawn` → crie GameObjects vazios filhos como `SpawnPt_01`, `SpawnPt_02` e arraste aqui

### E3. Portal (última sala de cada fase)

No GameObject `Portal_ProximaFase` dentro da última sala:
- **SpriteRenderer** — sprite de portal (começa invisível, o script controla)
- **CircleCollider2D** — `Is Trigger = true`
- **ParticleSystem** (opcional)
- **PortalController**:
  - `Proxima Scene` = nome exato da próxima cena no Build Settings
  - `Nome Da Dimensao` = texto exibido na tela (ex: `"MARTE"`) — pode deixar vazio
  - `Arma Proxima Fase` → arraste o ScriptableObject de arma para a próxima fase
  - `Particulas` → arraste o ParticleSystem (opcional)

Arraste o `PortalController` no campo `Portal` do `RoomController` desta sala.

### E4. IntroDialogue (opcional, recomendado)

Em qualquer GameObject da cena, adicione **IntroDialogue**:
- `Texto Dialogo` — texto de introdução da fase
- `Velocidade Digitacao = 45`
- `Atraso Inicial = 0.8` (deve ser ≥ duração do fade de entrada do GameManager)
- `Tempo Auto Fechar = 0` (não fecha sozinho — espera input do jogador)
- `Som Digitacao` — AudioClip de tecla (opcional)

### E5. Configuração por fase

| Fase | Inimigos sugeridos nos Spawners | Arma no PortalController da fase anterior |
|------|---------------------------------|------------------------------------------|
| FaseTerra | InimigoMelee | — (GameManager.armaAtual começa null) |
| FaseMarte | InimigoAtirador | Arma2_Automatica |
| FaseGoblins | InimigoKamikaze + InimigoMelee | Arma3_Shotgun |
| FaseFortaleza | (sem spawners — só boss) | Arma4_LancaChamas |

---

## SEÇÃO F — Cena: FaseFortaleza (Boss)

### Hierarquia

```
FaseFortaleza (cena)
├── Tilemap / Paredes
├── Player
├── Sala_Boss
│   ├── [RoomController + BoxCollider2D Trigger]
│   ├── Porta_Entrada         ← DoorController
│   └── Boss_Fortaleza        ← FILHO do mesmo GO com RoomController
│       ├── SpriteRenderer
│       ├── Rigidbody2D
│       ├── CircleCollider2D
│       ├── HealthSystem
│       ├── BossController
│       ├── DamageFlash
│       └── DamageNumberSpawner
└── Main Camera
```

### Como montar

1. Crie a sala normalmente com `RoomController` + `BoxCollider2D (Is Trigger = true)`
2. **Instancie o Boss como filho** do mesmo GameObject que tem o `RoomController`
3. No **RoomController**:
   - `Portas` → porta de entrada
   - `Spawners` → array vazio (sem EnemySpawners)
   - `Boss` → arraste o `BossController` do Boss filho
   - `Portal` → **vazio** (a vitória é gerenciada pelo BossController chamando HUDManager)
4. No **BossController**:
   - `Nome Boss = "Senhor da Fortaleza"`
   - `Prefab Projetil = [Prefab_ProjetilInimigo]`
   - `Prefab VFX Morte` = efeito de explosão (opcional)
   - `Delay Vitoria = 1`

---

## SEÇÃO G — Camera Follow

Em cada cena de fase, a **Main Camera** precisa seguir o Player.

Se há o script `CameraFollow.cs` no projeto:
- Adicione à **Main Camera**
- Arraste o Transform do Player no campo `Alvo`

Se não houver, crie `Assets/Scripts/Managers/CameraFollow.cs`:
```csharp
using UnityEngine;
public class CameraFollow : MonoBehaviour
{
    public Transform alvo;
    public float suavidade = 5f;
    void LateUpdate()
    {
        if (alvo == null) return;
        Vector3 pos = Vector3.Lerp(transform.position,
            new Vector3(alvo.position.x, alvo.position.y, transform.position.z),
            suavidade * Time.deltaTime);
        transform.position = pos;
    }
}
```

---

## SEÇÃO H — Checklist Final

### MenuPrincipal
- [ ] GameManager com `Nome Cena Principal = "FaseTerra"` e `Vida Maxima = 100`
- [ ] HUDManager com `Nome Cena Principal = "FaseTerra"` e `Nome Scene Menu = "MenuPrincipal"`
- [ ] AudioManager na cena
- [ ] MainMenuController com `Nome Primeira Scene = "FaseTerra"`
- [ ] Cena adicionada no Build Settings

### Cada cena de fase
- [ ] Player com tag `Player`, layer `Player`
- [ ] PontoDeDisparo como filho do Player, referenciado no WeaponController e SpellSystem
- [ ] Paredes com layer `Wall` e Collider2D **não-trigger**
- [ ] Cada sala: BoxCollider2D trigger cobrindo o interior
- [ ] Cada RoomController: arrays Portas, Spawners e Portal preenchidos
- [ ] EnemySpawner: Prefabs Inimigos e Pontos De Spawn preenchidos
- [ ] Portal da última sala: `Proxima Scene` + `Arma Proxima Fase` preenchidos
- [ ] Cena adicionada no Build Settings
- [ ] CameraFollow na Main Camera com o Player no campo Alvo

### FaseFortaleza
- [ ] Boss é **filho** do GameObject com RoomController
- [ ] RoomController.Boss preenchido com BossController do filho
- [ ] RoomController.Portal deixado **vazio**
- [ ] BossController.Prefab Projetil preenchido

### Physics 2D (verificação rápida)
- [ ] Layer `Projectile` não colide com `Player` nem com `Projectile`
- [ ] Layer `EnemyProjectile` não colide com `Enemy` nem com `EnemyProjectile`
- [ ] Layer `Enemy` não colide com `Enemy`

---

## Ordem de Teste Recomendada

1. **Só o MenuPrincipal** — botão JOGAR vai para FaseTerra?
2. **FaseTerra sem inimigos** — player se move, barra de vida aparece, portal funciona?
3. **FaseTerra com inimigos** — sala fecha, inimigos morrem, portal abre?
4. **Transição de cena** — fade funciona, texto de dimensão aparece, vida é preservada?
5. **FaseFortaleza** — barra de boss aparece, transição de fase 1→2 ocorre, tela de vitória aparece?
6. **Fluxo completo** — MenuPrincipal → todas as fases → Vitória → Reiniciar → MenuPrincipal?

---

## Referência Rápida de Scripts

| Script | Onde colocar | Observação |
|--------|-------------|------------|
| `GameManager` | MenuPrincipal > GameObject vazio | Singleton — só uma vez |
| `HUDManager` | MenuPrincipal > GameObject vazio | Singleton — só uma vez |
| `AudioManager` | MenuPrincipal > GameObject vazio | Singleton — só uma vez |
| `MainMenuController` | MenuPrincipal > qualquer GO | Cria UI por código |
| `PlayerController` | Player | Requer Rigidbody2D |
| `HealthSystem` | Player, Inimigos, Boss | |
| `SpellSystem` | Player | Desativar nas fases 2–4 |
| `ElementManager` | Player | Desativar nas fases 2–4 |
| `WeaponController` | Player | Requer PontoDeDisparo |
| `FlamethrowerAttack` | Player | Usado pelo WeaponController (LancaChamas) |
| `DamageFlash` | Player, Inimigos, Boss | Requer SpriteRenderer + HealthSystem |
| `DamageNumberSpawner` | Player, Inimigos, Boss | Requer HealthSystem + Prefab_NumeroDeDano |
| `DamageNumber` | Prefab_NumeroDeDano | Requer TextMeshPro 3D |
| `DeathVFX` | Inimigos (opcional) | Requer HealthSystem |
| `EnemyController` | Prefab_InimigoMelee | Requer Rigidbody2D + HealthSystem |
| `EnemyShooter` | Prefab_InimigoAtirador | Requer Rigidbody2D + HealthSystem |
| `EnemyExploder` | Prefab_InimigoKamikaze | Requer Rigidbody2D + HealthSystem |
| `BossController` | Prefab_Boss (filho do RoomController) | Requer Rigidbody2D + HealthSystem |
| `RoomController` | Sala (com BoxCollider2D Trigger) | |
| `DoorController` | Porta (com Collider2D) | |
| `EnemySpawner` | GO vazio filho da sala | |
| `PortalController` | Portal (última sala) | Requer SpriteRenderer + Collider2D Trigger |
| `IntroDialogue` | Qualquer GO na cena de fase | Opcional |
| `CameraFollow` | Main Camera | |
