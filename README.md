# Relatório

Comecei por ver todos os vídeos do professor e fazer o projecto do professor a acompanhar.  
Em seguida, criei o meu projecto usando as coisas que o professor fez e que davam no meu projecto.  
Não copiei scripts, tal como o de movimento, porque achei boa ideia ser eu a fazê-lo, mas coisas como o `NetworkSetup` copiei.

Mal comecei o projecto, fiz o script de movimento e segui o vídeo do professor.  
O jogador estava a funcionar e a mover-se no servidor e no cliente como deve ser.  
Mas depois começaram os problemas, porque decidi explorar mais, para não estar só a copiar exactamente o que o professor fez.  
Achei que ia ficar muito cópia, e não uma obra minha.

---

## Problema com o spawn das balas

O primeiro problema que tive foi que a bala não fazia spawn no servidor.  
Descobri que era o cliente que estava a fazer spawn das balas, e tinha de ser o servidor.  
Para resolver, tive de meter, no `PlayerController`, no método `Shoot`, um `[ServerRpc]`.

---

## Animações e lógica no cliente

Eventualmente, quando fiz uma animação, decidi fazê-la do lado do cliente.  
Depois de testar, vi que os dois clientes viam a animação e o servidor não.  
Num contexto de jogo, basta os jogadores verem a animação a acontecer.  
Acho que o servidor ter essa informação seria informação “desnecessária”.

---

## Uso de NetworkBehaviour e IsLocalPlayer

Outra coisa que fiz, que não é bem um problema: falei com o ChatGPT e ele disse que, entre ir buscar um `NetworkObject` para ter o `IsLocalPlayer` e usar directamente o `IsLocalPlayer`, ao meter as classes como `NetworkBehaviour`, não faria diferença — a não ser que fosse verificar a partir de outro script.  
Então decidi colocar a maioria dos scripts como `NetworkBehaviour`.

---

## Jogador a acertar em si próprio

Depois tive um problema em que o jogador 1 acertava em si próprio às vezes com a bala.  
Então fiz com que a bala recebesse o `GameObject` que a disparava, para saber que, se colidir contra ele, não deve fazer nada.

Errei ao tentar usar isso directamente.  
Comecei a ter erros em que os jogadores não levavam com as balas.  
O `NetworkObject` dizia que era do servidor, eu dava `Debug.Log`, e aquilo dizia que não era.  
Tive de voltar atrás.  
Ao início isto estava a dar, por causa dos `triggers`, até porque eu tinha posto para fazerem spawn mais à frente.

Acabei por reparar que o erro era meu.  
Isto estava a funcionar, só que eu pensava que dava para fazer debug do servidor.  
Depois percebi-me que, se o Unity Editor está a correr o cliente, não tem como dar debug a partir do servidor.

---

## Sistema de spawn dos jogadores

Agora irei tratar dos pontos de spawn.  
Para isso fui pesquisar e descobri uma forma de o fazer: criar um script que cria e faz spawn dos jogadores quando é iniciado um cliente.  
Fiz dessa forma, mas, sendo honesto, como tive de tirar até o prefab do jogador do `NetworkManager`, acredito que existam formas bastante melhores.

Mas com isto está funcional, irei seguir em frente.  
Caso tenha tempo, voltarei atrás para fazer de uma forma melhor, que na minha visão seria com override ao spawn.

---

## Delay nas balas e sincronização

Como isto já está 90% funcional, irei agora seguir o vídeo do professor para tirar o atraso das balas.  
No servidor, as balas colidem e desaparecem na altura certa, mas no cliente desaparecem mais cedo do que o normal.

Isto acontece porque as balas no servidor não estão sincronizadas, o que causa a bala no cliente estar atrasada.  
Leva a um problema de *client-side prediction*, ou melhor, à ausência de *client-side prediction*.

Por isso, irei fazer um *client-side prediction*.  
Para isso, fiz um tiro do lado do cliente que vai ser sincronizado com o servidor.

---

## Tentativas com sincronização e Rigidbody

Depois de tentar replicar o código do professor para o meu e não saber porque não estava a funcionar — até perguntei ao ChatGPT e ele não estava a conseguir — decidi pesquisar outras formas.

Vi o `NetworkRigidbody`, que por si só tenta fazer uma sincronização melhor.  
Depois de testar, reparei que sim, continua com um atraso, mas acho que um jogador ao olhar consegue pensar que é só a hitbox e não que está a cortar muito cedo.

Se eu tiver tempo, ainda quero voltar atrás para conseguir fazer um *client-side prediction* por mim.

---

## Sistema de vida e feedback visual

Fiz agora cada jogador ter vida, tal como o professor fez.  
Vou agora fazer a vida ficar vermelha ou verde consoante o jogador que estiver a jogar.

Para isso, fiz com que o script que controla o UI da vida também recebesse o `NetworkObject` e detectasse se era o jogador ou era outra pessoa a tentar aceder ao script.  
Caso seja o jogador, a cor muda para verde.  
Para os outros, não muda de cor (a cor por defeito é vermelho).

Também fiz um piscar no jogador quando ele leva dano.  
Enquanto estava a fazer isso, reparei que estava mal feito: partes do código estavam a atualizar constantemente a vida, o que é mau.  
A vida só deve ser atualizada quando um acerto acontece, e não em `Update`.

Então alterei um pouco o código para que, quando um acerto aconteça, o servidor chame a função de piscar e de atualização da vida para o jogador que levou o dano.

Tive de meter nos métodos `[ClientRpc]`, porque senão o jogador só fazia as actualizações no servidor, já que só o servidor cuida do `OnTriggerEnter`.

Como estou a usar uma `NetworkVariable<int>`, mesmo eu actualizando o valor e só depois actualizando o UI da vida, o UI recebia ainda a vida anterior.  
Ficava atrasado porque a `NetworkVariable<int>` pode ter atraso, e mesmo que seja chamada primeiro, os clientes só sabem dessa informação depois.

Então fiz com que a função que actualiza o UI receba um `int` com o valor da vida nova.

---

## Estado actual e conclusões

Cheguei a um ponto agora em que já tenho o jogo funcional e diria que daria para jogar 1 contra 1 num sistema online.

Gostaria de aperfeiçoar mais coisas, como um sistema de login, etc., mas como tenho DJD3, irei tentar apenas fazer o *client-side prediction*, que acho que seria bastante interessante ter neste jogo.  
Caso não esteja na build, estará aqui o que tentei fazer.

Depois de tanto tempo a tentar e falhar porque dava erros como:

[NetworkTransformMessage][Invalid] Targeted NetworkTransform, NetworkBehaviourId (0), does not exist!

— comecei a fazer as coisas de forma mais autónoma.  
Tendo visto o vídeo do professor e já entendendo melhor, tentei fazer algo por mim.  
Até porque o ChatGPT, sinceramente, não me estava a ajudar nada.

Então decidi fazer por mim e irei deixar o jogador com dois scripts: um com e outro sem o *client-side prediction* que eu fiz.  
Acredito que o caminho que segui não foi a forma mais correcta, mas, do que pesquisei e perguntei ao ChatGPT, isto é considerado *client-side prediction*.

Se o professor quiser tirar o *client-side prediction*, é só desactivar a opção “Use Client Side Prediction” no `PlayerController` e activar “Spawn With Observers” no prefab `BulletNetwork`.

---

## Descrição técnica do que foi implementado, assim como as técnicas usadas

Este projecto implementa um sistema multiplayer para um jogo 2D com personagens que se movem, atiram e possuem vida, utilizando o **Unity Netcode for GameObjects** para a sincronização da rede.

- **PlayerController:** controla o movimento do jogador local, animação e disparo de projéteis. Suporta *client-side prediction* para tiros, minimizando latência.
- **PlayerShootingClientSidePrediction:** gere o disparo de projéteis, criando uma instância local para previsão imediata e uma instância na rede para sincronização oficial.
- **Bullet e BulletPrediction:** representam projéteis na rede e localmente, respectivamente. Os projéteis possuem tempo de vida, colidem com outros jogadores e aplicam dano.
- **Health e HealthDisplayUpdate:** gerenciam a vida dos jogadores, sincronizando a barra de vida via RPCs.
- **NetworkSetup:** automatiza o start do servidor ou cliente conforme argumento de linha de comando.
- Técnicas principais usadas:
- **Client-side prediction:** para disparos, instanciando projéteis localmente enquanto a confirmação do servidor é aguardada.
- **NetworkVariables e ClientRpc:** para sincronizar estado da vida dos jogadores e efeitos visuais.
- **Rigidbody2D e física do Unity:** para movimentação e colisões.

---

## Descrição de mensagens de rede

- **NetworkVariable<int> currentHealth:** sincroniza a vida atual dos jogadores automaticamente entre servidor e clientes.
- **[ServerRpc] ShootServerRpc(Vector2 targetPos):** chamada do cliente para o servidor solicitar spawn oficial do projétil.
- **[ClientRpc] UpdateHealthClientRpc(int newHealth):** chamada do servidor para atualizar a UI de vida no cliente.
- **[ClientRpc] BlinkClientRpc():** chamada do servidor para todos os clientes, para ativar efeito visual de "piscada" no jogador atingido.

---

## Análise de largura de banda/custo/etc

- Uso moderado da largura de banda:
  - Movimentação do jogador é sincronizada via NetworkTransform (não detalhado nos scripts, mas presumido).
  - Projéteis locais (client-side prediction) são instanciados localmente, evitando delay na ação do jogador e reduzindo o tráfego.
  - Apenas quando um tiro é confirmado pelo servidor é enviado um comando para spawn do projétil oficial.
  - Dano e vida são atualizados com RPCs ocasionais, minimizando mensagens frequentes.
- Potenciais pontos de otimização:
  - Compressão de posições e ângulos para reduzir payload.
  - Redução da frequência de atualização da vida, ou uso de eventos para evitar atualizações constantes.

---

## Diagrama de arquitetura de redes

