# Relatório

Comecei por ver todos os vídeos do professor e fazer o projeto do professor a acompanhar.  
Em seguida, criei o meu projeto usando as coisas que o professor fez e que davam no meu projeto.  
Não copiei scripts, tal como o de movimento, porque achei boa ideia ser eu a fazê-lo, mas coisas como o `NetworkSetup` utilizei grande parte da do professor.

Mal comecei o projeto, fiz o script de movimento e segui o vídeo do professor.  
O jogador estava a funcionar e a mexer-se no servidor e no cliente como deve ser.  
Mas depois começaram os problemas, porque decidi explorar mais, para não estar a copiar exatamente o que o professor fez.  

---

## Problema com o spawn das balas

O primeiro problema que tive foi que a bala não fazia spawn no servidor.  
Descobri que era o cliente que estava a fazer spawn das balas, e tinha de ser o servidor.  
Para resolver, tive de meter, no `PlayerController`, no método `Shoot`, um `[ServerRpc]`.  
Apesar de o professor ter feito isto no seu jogo eu como tava a fazer mais de uma prespetiva de ver o video e depois aplicar o que aprendi acabei por deparar me com alguns problemas "simples".

---

## Animações e lógica no cliente

Eventualmente, quando fiz uma animação, decidi fazê-la do lado apenas dos cliente.  
Porque o servidor não serve para renderizar apenas para guardar informação apesar de que, em certos jogos online isso se calhar daria jeito para um sistema de alguem ver o jogo ou sistema de replay se calhar. Mas para isto acontecer nao chamei o servidor e deixei as animações a rodarem apenas localmente.

---

## Uso de NetworkBehaviour e IsLocalPlayer

Outra coisa que fiz, que não é bem um problema: falei com o ChatGPT e ele disse que, entre ir buscar um `NetworkObject` para ter o `IsLocalPlayer` e usar diretamente o `IsLocalPlayer`, ao meter as classes como `NetworkBehaviour`, não faria diferença a não ser que fosse para verificar a partir de outro script. Então decidi colocar os scripts como `NetworkBehaviour`, ate porque o NetworkBehaviour esta a buscar diretamente o NetworkObject que sabe sempre quem esta a tentar aceder ao script. 

E com `NetworkBehaviour` assim tem se logo acesso a usar [ClientRpc] e [ServerRPC] que essencialmente RPCs sao uma forma de chamar os outros programas no caso permite um client dizer para so o servidor fazer alguma coisa o client dizer a todos os clientes para algo acontecer.

---

## Jogador a acertar em si próprio

Depois tive um problema em que o jogador acertava em si próprio às vezes com a bala.  
Então fiz com que a bala recebesse o `GameObject` que a disparava, para saber que, se colidir contra ele, não deve fazer nada.

---

## Sistema de spawn dos jogadores

Agora irei tratar dos pontos de spawn.  
Para isso fui pesquisar e descobri uma forma de o fazer: criar um script que cria e faz spawn dos jogadores quando é iniciado um cliente.  
Fiz dessa forma, mas, sendo honesto, como tive de tirar o prefab do jogador do `NetworkManager`, acredito que existam formas melhores como on spawn alterar a sua posição mas eu não conseguir encontrar como acredito que fosse com o override mas não encontrei.

Mas o script essencialmente faz spawn manual com o `SpawnAsPlayerObject` Em vez de o `NetworkManager` criar o jogador automaticamente, este método instancia o jogador manualmente e associá-o ao cliente que se conectou.

O `NetworkManager` é usado para gerir callbacks importantes da conexão (`ConnectionApprovalCallback` e `OnClientConnectedCallback`).

Usei o `ConnectionApprovalCallback` que é uma forma de validar e aprovar ou recusar conexões de clientes antes de os permitir entrar no jogo. Aqui, é usado para limitar o número máximo de jogadores a 2 e impedir que mais clientes entrem.

E usei o `OnClientConnectedCallback` que faz um código quando um cliente se conecta e, neste caso, faz o spawn manual do jogador no servidor num ponto específico tendo em conta outros players que ja tenham spawnado.



---

## Delay nas balas e sincronização

Irei agora seguir o vídeo do professor para tirar o atraso das balas.  
No servidor, as balas colidem e desaparecem na altura certa, mas no cliente desaparecem mais cedo do que o normal quando acertam um player.

Isto acontece porque as balas do servidor não estão sincronizadas com o player, o que causa a bala no cliente estar atrasada.  
Leva a um problema de *client-side prediction*, ou melhor, à ausência de *client-side prediction*.

Por isso, irei fazer um *client-side prediction*.  
Pois it was not that easy...

Depois de tentar replicar o código do professor para o meu e não saber porque não estava a funcionar até perguntei ao ChatGPT e ele não estava a conseguir dizer me decidi ir fazer outras coisas e depois volto para aqui para tentar o "client-side prediction".

Nesta pesquisa eu descobri o networkrigidbody que supostamente ajudava com sincronização eu usei e não vi diferença.

---

## Sistema de vida e feedback visual

Fiz agora cada jogador ter vida, tal como o professor fez.  
Vou agora fazer a vida ficar vermelha ou verde consoante o jogador que estiver a jogar.

Para isso, fiz com que o script que controla o UI da vida extendesse o `NetworkBehaviour` e detectasse se era o Owner ou se era outra pessoa a tentar aceder ao script.  
Caso seja o jogador, a cor muda para verde.  
Para os outros, não muda de cor (a cor por defeito é vermelho).

Também fiz um piscar no jogador quando ele leva dano.  
Enquanto estava a fazer isso, reparei que tinha coisas mal feitas por exemplo o UI da vida estava em um `update`...
E a vida só deve ser atualizada quando um Hit acontece, e não em `Update`.

Então alterei um pouco o código para que, quando um Hit aconteça, o servidor chame a função de piscar e de atualização da vida para o jogador que levou o dano.

Tive de criar 2 métodos um com e outro sem `[ClientRpc]`, porque senão o jogador só fazia as atualizações no servidor, já que só o servidor cuida do `OnTriggerEnter` desta forma faz nos dois, apesar de que o servidor nao precisa de atualizar propriamente a vida em um UI e nem piscar um player então é algo que acho que irei tirar e deixar essas coisas a acontecer apenas nos clientes.

Como estou a usar uma `NetworkVariable<int>`, mesmo eu atualizando o valor e só depois atualizo o UI da vida, o UI recebia ainda a vida anterior.  
Ficava atrasado porque a `NetworkVariable<int>` pode ter atraso, mesmo que seja chamada primeiro, o  servidor pode so enviar a informação aos clientes depois.

Então fiz com que a função que atualiza o UI receba um `int` com o valor da vida nova.

---

## Client-Side prediction (rewind time)

Cheguei a um ponto agora em que já tenho o jogo funcional e diria que daria para jogar 1 contra 1.

Mas irei tentar fazer o *client-side prediction*, que acho que seria bastante interessante ter neste jogo.  
Caso não esteja na build, estará aqui o que tentei fazer.

Comecei a fazer as coisas de forma mais autónoma.  
Como ja tinha visto o vídeo do professor umas 3x a este ponto e já entendendo melhor o que esta a acontecer, tentei fazer algo por mim.  
Até porque o ChatGPT, sinceramente, não me estava a ajudar nada.

Então decidi fazer por mim.

Acredito que o caminho inicial que tentei seguir foi o melhor porque foi o professor que disse que fez.

Nos videos o professor diz algo ao longo das linhas de "no caso do strikers edge o que eu fazia, como o sistema era quase todo custom, o que eu fazia quando criava a copia local da bala aquilo dava um id local e esse id local era utilizado pelo servidor, ou seja quando eu dizia ao servidor eu quero um spawn e se puder é aqui que spawnas"

eu tentei fazer isso por bastante tempo não sei o que eu estava a fazer de errado mas não dava eu dizia a bala do servidor para dar spawn com a posição da bala local e ela dava spawn com a posição do objeto local mas mesmo assim ficava atrasada ficou mais perto do que quando spawnava "individualmente" mas mesmo assim com delay então aprecebi me que não era bem so fazer isto tal como o professor disse deve ter mais por tras.  
Então fui perguntar ao chatgpt e ele disse me para fazer com que aquilo desse track constante de a posição da bala local mas como a bala é local o servidor nao tem como ter acesso a ela mesmo assim fiz o que ele disse que daria que era guardar o gameobject no script e depois no update ir atualizando a bala do servidor e eu tentei e a bala do servidor ficava so parada e nao se mexia tal como imaginei.  
Depois disso voltei a forma do stor fiz com que as balas se movessem com NetworkManager.Singleton.ServerTime.TimeAsFloat, mas como a forma que eu crio balas nao é igual a do professor eu tinha de adaptar e sei que algo devia estar a faltar porque estava a funcionar mas mais ou menos quando as balas estavam muito lentas dava mas quando metia a uma velocidade normal ja ficavam outra vez com espaço entre elas então denovo fui perguntar ao chat e ele não me disse nada de jeito ele ate tinha me dito para parar de usar o NetworkManager.Singleton.ServerTime.TimeAsFloat então so desisti dele denovo. 

E depois de tanto tempo a tentar eu sinceramente não sei o que era melhor deixar uma das tentativas acima no codigo ou fazer o que eu deixei que foi uma dirai que "ilusão" de client side prediction, o que fiz foi como reparei que a bala local parecia que spawnava primeiro fiz com que ela spawna-se um pouco mais atras e dessa forma ela por si ficava sincronizada com a bala do servidor posso dizer que mesmo em um contexto de lan a usar o portatil para testar para o client a bala ficou "certa" então de certa forma funcionou, apesar de não ser a forma mais correta.

---

## Descrição técnica do que foi implementado, assim como as técnicas usadas

Este projeto implementa um sistema multiplayer para um jogo 2D com 2 personagens que se movem, atiram e teem vida, e utiliza o **Unity Netcode for GameObjects** para a sincronização da rede.


- **NetworkSetup:** automatiza o start do servidor ou cliente conforme argumento de linha de comando.  
- Técnicas principais usadas:  
- **Client-side prediction:** para disparos, instanciando projéteis localmente enquanto a confirmação do servidor é aguardada.  
- **NetworkVariables e ClientRpc:** para sincronizar estado da vida dos jogadores e efeitos visuais.  

---

## Descrição de mensagens de rede 
- **NetworkVariable<int> currentHealth:** sincroniza a vida atual dos jogadores automaticamente entre servidor e clientes.  
- **[ServerRpc] ShootServerRpc(Vector2 targetPos):** chamada do cliente para o servidor o cliente pede um spawn oficial da bala.  
- **[ClientRpc] UpdateHealthClientRpc(int newHealth):** chamada do servidor para atualizar o UI de vida no cliente.  
- **[ClientRpc] BlinkClientRpc():** chamada do servidor para todos os clientes, para ativar efeito visual de "blink" no jogador atingido para feedback visual.

---

## Análise de largura de banda

- Uso da largura de banda:  
  - Servidores capazes de correr o jogo que neste caso não é muito dificil
  - Movimentação do jogador é sincronizada via NetworkTransform.  
  - Projéteis locais (client-side prediction) são instanciados localmente, e evitam delay na ação do jogador.  
  - Quando um tiro é confirmado pelo servidor é enviado um comando para spawn oficial da bala.  
  - Dano, UI da vida e blink feedback são atualizados com RPCs. 

---

## Instruções para experimentar num cenário LAN
No GameObject chamado Network Manager, dentro do script UnityTransport.   
É necessário preencher o Adress com o IPv4 do host, ou seja, o IP da máquina para a qual os clientes se vão conectar, e também definir o port. O IP é como se fosse uma casa e o port é como se fosse para o cliente saber para qual porta daquela casa ir para ir ter com o servidor. 
O Allow Remote Connections tambem tem de estar ativo.  
A forma de ter o IPv4 do host é abrir a linha de commandos e escrever "ipconfig"

---

## Diagrama de arquitetura de redes

A arquitetura deste projeto é Cliente/Servidor. Como isso funciona essencialmente é que o cliente envia um pedido ao servidor por exemplo, no caso de disparar, o servidor recebe esse pedido e faz o spawn de uma bala a partir do spawnpoint de balas daquele cliente. O servidor fica constantemente a verificar se essa bala acerta noutro cliente. Caso acerte noutro cliente, o servidor verifica a quantidade de dano que o cliente que disparou causa e aplica esse dano ao cliente que levou o hit. Depois, envia um UpdateHealthClientRpc(newHealth) para atualizar a vida visualmente no cliente, e também chama BlinkClientRpc() para o cliente fazer o player piscar e dar mais feedback visual de que levou dano, esses metodos sao feitos exclusivamente em client side porque para um servidor isso é informação "inutil".

![image](./Diagrama.png)

---

## Biblioteca Utilizada
vi os videos do professor e powerpoints.  
vi este video https://www.youtube.com/watch?v=7glCsF9fv3s&list=PLzDRvYVwl53sSmEcIgZyDzrc0Smpq_9fN&index=1  
Tambem fui usando o chatgpt quando estava preso ou precisava de entender alguma coisa.




