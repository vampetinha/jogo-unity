# Game Design Document — Projeto jogo-unity

**Engine:** Unity 6.3 LTS  
**Plataforma:** Windows
**Gênero:** Ação 2D Top-Down  
**Perspectiva:** Vista de cima (top-down)

---

## 1. Visão Geral

Jogo de ação 2D em perspectiva top-down onde o jogador percorre dimensões alienígenas, eliminando hordas de inimigos para abrir caminho até portais interdimensionais. Cada dimensão apresenta novos inimigos e desafios, culminando em batalhas contra chefes poderosos.

---

## 2. História / Ambientação

O jogador é um combatente que viaja entre dimensões — Terra, Fortaleza, Mundo dos Goblins e Marte — enfrentando criaturas de cada plano. O objetivo é sobreviver a todas as dimensões e derrotar o chefe final.

---

## 3. Mecânicas de Jogo

### 3.1 Movimento do Jogador

| Ação | Controle |
|---|---|
| Mover | WASD / Setas |
| Dash | Espaço |
| Atirar | Mouse (botão esquerdo) |
| Mirar | Posição do cursor |

**Dash (Sandevistan):**
- O jogador avança rapidamente na direção atual de movimento
- Fica invencível durante toda a duração do dash
- Duração: 0,15s | Cooldown: 0,7s | Força: 18 unidades
- Efeito visual de rastro ciano (fantasmas que desvanecem)

### 3.2 Sistema de Vida

- Vida máxima padrão: 100 pontos
- A vida persiste entre fases (não é restaurada ao trocar de dimensão)
- Ao receber dano, o sprite pisca em vermelho (DamageFlash)
- Números de dano flutuantes aparecem ao atingir inimigos
- Ao chegar a 0: tela de Game Over com opções de Reiniciar ou voltar ao Menu

### 3.3 Armas

Cada fase pode entregar uma arma diferente ao jogador via portal.

| Tipo | Comportamento |
|---|---|
| **Normal** | Um projétil por clique |
| **Automático** | Dispara continuamente enquanto o botão é pressionado |
| **Shotgun** | Cone de múltiplos projéteis por clique |
| **Lança-Chamas** | Jato contínuo com sistema de calor/superaquecimento |

**Atributos configuráveis por arma:**
- Dano, velocidade do projétil, alcance, intervalo de disparo
- Sprite da arma e do projétil
- Se o projétil atravessa inimigos
- Som de disparo

**Lança-Chamas:** possui barra de calor no HUD. Ao superaquecer entra em recarga automática.

### 3.4 Sistema de Salas

Cada sala segue o ciclo:
1. **Jogador entra** no trigger da sala
2. **Inimigos spawnam** em posições aleatórias dentro da sala
3. **Portas fecham** (com delay de 0,6s para o jogador entrar)
4. **Combate** — ao matar cada inimigo, o contador decresce
5. **Sala limpa** — portas abrem e o portal aparece (na última sala)

---

## 4. Inimigos

### 4.1 Tipos de Inimigos

| Nome | Comportamento |
|---|---|
| **Zumbi** | Melee, persegue o jogador |
| **Pálido** | Melee, persegue o jogador |
| **Goblin / InimigoMelee** | Melee corpo a corpo |
| **Robô** | Atirador à distância |
| **Alien** | Atirador à distância |
| **Demônio** | Atirador à distância |
| **Orbe** | Atirador à distância |
| **Golem** | Tanque/melee |
| **Mini Boss** | Inimigo especial de maior dificuldade |

### 4.2 Boss Principal

O boss possui **duas fases** baseadas em porcentagem de vida:

**Fase 1 (HP > 50%) — Corpo a Corpo:**
- Persegue o jogador agressivamente
- Ataque melee ao alcançar corpo a corpo
- Executa dash (avanço) a cada ~5s causando dano extra

**Transição (HP = 50%):**
- Para completamente e pisca em vermelho 5 vezes
- Cresce de tamanho (×1,2) indicando enraivecimento

**Fase 2 (HP ≤ 50%) — Ranged:**
- Mantém distância ideal do jogador
- **Spread:** 3 projéteis em leque direcionados ao jogador
- **Rajada circular:** 8 projéteis em 360° simultâneos
- Continua atacando corpo a corpo se o jogador se aproximar

**Morte do Boss:**
- Pisca branco rapidamente (8 vezes)
- Efeito VFX de morte
- Exibe tela de **Vitória** com total de moedas coletadas

---

## 5. Progressão de Fases

| Ordem | Cena | Dimensão |
|---|---|---|
| 0 | MenuNovo | Menu Principal |
| 1 | FaseTerra | Terra |
| 2 | FaseFortaleza | Fortaleza |
| 3 | FaseGoblins | Mundo dos Goblins |
| 4 | FaseMarte | Marte |

**Transição entre fases:**
- Ao limpar a última sala, um portal aparece
- O jogador entra no portal → fade para preto → nome da dimensão aparece na tela → fade abre na nova fase
- A nova fase pode entregar uma arma diferente ao jogador automaticamente

---

## 6. Interface (HUD)

| Elemento | Posição | Descrição |
|---|---|---|
| Barra de vida | Superior esquerdo | Verde → Amarelo → Vermelho conforme HP cai |
| Ícone da arma | Abaixo da vida | Mostra o sprite da arma equipada |
| Barra de calor | Inferior direito | Visível apenas com lança-chamas |
| Barra do boss | Centro inferior | Aparece somente na sala do boss (roxo → vermelho) |

---

## 7. Telas

### Menu Principal (MenuNovo)
- Botão **Jogar** → carrega FaseTerra
- Botão **Configurações** → painel de volume
- Botão **Créditos** → painel de créditos
- Botão **Sair** → fecha o jogo

### Game Over
- Aparece ao morrer (pausa o jogo)
- Botão **Reiniciar** → reinicia da FaseTerra com vida cheia
- Botão **Menu Principal** → volta ao MenuNovo

### Vitória
- Aparece ao derrotar o boss final
- Exibe dimensões conquistadas
- Botão **Menu Principal** → volta ao MenuNovo

---

## 8. Áudio

- **Música de fundo** por fase (controlada pelo AudioManager)
- **SFX** de disparo por tipo de arma
- **SFX em loop** para o lança-chamas
- Ao entrar no Game Over: música e SFX são interrompidos

---

## 9. Efeitos Visuais

| Efeito | Onde ocorre |
|---|---|
| Dash Trail (fantasmas ciano) | Player ao dar dash |
| DamageFlash (pisca vermelho) | Player e inimigos ao receber dano |
| Números de dano flutuantes | Sobre o alvo atingido |
| DeathVFX | Inimigos e boss ao morrer |
| Fade preto + nome da dimensão | Transição entre fases |
| Partículas do portal | Portal ativo no fim da sala |

---

## 10. Equipe

Desenvolvido por estudantes como projeto acadêmico.  
Engine: Unity 6.3 LTS | Linguagem: C# | Repositório: GitHub (vampetinha/jogo-unity)
