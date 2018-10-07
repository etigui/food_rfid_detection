# Mini projet système nomade RFID

L'objectif de se mini projet est de proposer une solution innovante utilisant la technologie RFID.

## Solution proposée
Scan et vérification des articles disposé sur un plateau.

![Solution](/images/solution.png)


## Réalisation

Lecteur RFID:
- Initialisation du lecteur : 0x02
- Lecture du tag : 4bytes via UART
- Conversion de l’ID sous la forme : x(xx)-x(xx)-x(xx)-x(xx)
- Comparaison avec la base de donnée

![Réalisation](/images/realisation.png)




