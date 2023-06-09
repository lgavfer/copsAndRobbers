﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;
                    
    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }
        
    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }

    // Para mostrar por pantalla la matriz
    public void PrintAdjacencyMatrix(int[,] adjacencyMatrix)
    {
        int numRows = adjacencyMatrix.GetLength(0);
        int numCols = adjacencyMatrix.GetLength(1);

        for (int row = 0; row < numRows; row++)
        {
            string rowString = "";
            for (int col = 0; col < numCols; col++)
            {
                int value = adjacencyMatrix[row, col];
                rowString += value + " ";
            }
            Debug.Log(rowString);
        }
    }

    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        int[,] matriu = new int[Constants.NumTiles, Constants.NumTiles];

        //TODO: Inicializar matriz a 0's
        // Creamos un primer bucle para reccorer cada fila de la matriz
        for (int i = 0; i<Constants.TilesPerRow; i++) {
            // Ahora, dentro de cada fila, creamos otro para recorrer todas las columnas que tiene
            for (int j = 0; j<Constants.TilesPerRow; j++) {
                matriu[i, j] = 0;
            }
        }

        // PrintAdjacencyMatrix(matriu);
    

        //TODO: Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha)
        for (int i = 0; i < Constants.TilesPerRow; i++)
        {
            for (int j = 0; j < Constants.TilesPerRow; j++)
            {
                int currentTileIndex = i * Constants.TilesPerRow + j;

                // Comprobar casilla de arriba
                if (i> 0)
                {
                    int aboveTileIndex = (i - 1) * Constants.TilesPerRow + j;
                    matriu[currentTileIndex, aboveTileIndex] = 1;
                }

                // Comprobar casilla de abajo
                if (i < Constants.TilesPerRow - 1)
                {
                    int belowTileIndex = (i+ 1) * Constants.TilesPerRow + j;
                    matriu[currentTileIndex, belowTileIndex] = 1;
                }

                // Comprobar casilla de la izquierda
                if (j > 0)
                {
                    int leftTileIndex = i * Constants.TilesPerRow + (j - 1);
                    matriu[currentTileIndex, leftTileIndex] = 1;
                }

                // Comprobar casilla de la derecha
                if (j < Constants.TilesPerRow - 1)
                {
                    int rightTileIndex = i * Constants.TilesPerRow + (j + 1);
                    matriu[currentTileIndex, rightTileIndex] = 1;
                }
            }
        }

        // Muestro por pantalla las matrices para ver si se ha hecho bien
        // PrintAdjacencyMatrix(matriu);
    

        //TODO: Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes
            for (int i = 0; i < Constants.TilesPerRow; i++)
            {
                for (int j = 0; j < Constants.TilesPerRow; j++)
                {
                    int tileIndex = i * Constants.TilesPerRow + j;
                    // tiles[tileIndex].adjacency = new List<int>();

                    int currentTileIndex = i * Constants.TilesPerRow + j;

                    // Comprobar casilla de arriba
                    if (i > 0)
                    {
                        int aboveTileIndex = (i - 1) * Constants.TilesPerRow + j;
                        tiles[tileIndex].adjacency.Add(aboveTileIndex);
                    }

                    // Comprobar casilla de abajo
                    if (i < Constants.TilesPerRow - 1)
                    {
                        int belowTileIndex = (i + 1) * Constants.TilesPerRow + j;
                        tiles[tileIndex].adjacency.Add(belowTileIndex);
                    }

                    // Comprobar casilla de la izquierda
                    if (j > 0)
                    {
                        int leftTileIndex = i * Constants.TilesPerRow + (j - 1);
                        tiles[tileIndex].adjacency.Add(leftTileIndex);
                    }

                    // Comprobar casilla de la derecha
                    if (j < Constants.TilesPerRow - 1)
                    {
                        int rightTileIndex = i * Constants.TilesPerRow + (j + 1);
                        tiles[tileIndex].adjacency.Add(rightTileIndex);
                    }
                }
            }   

        // Muestro por pantalla para ver si funciona -> tiene que devolverme un array con los indices de las adyacentes de la casilla 1
        Debug.Log("Adyacentes de la casilla 1: " + string.Join(", ", tiles[1].adjacency));
        // PrintAdjacencyMatrix(matriu);


    }



    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;   
                    
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        /*TODO: Cambia el código de abajo para hacer lo siguiente
        - Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        - Movemos al caco a esa casilla
        - Actualizamos la variable currentTile del caco a la nueva casilla
        */

        // Hacemos una lista con las casillas a las que puede ir -> las seleccionables segun su posicion
        List<int> selectableTiles = new List<int> ();
        for(int i = 0; i<64; i++) {
            if(tiles[i].selectable) {
                selectableTiles.Add(tiles[i].numTile);
            }
        }

        // Elegimos una de forma aleatoria -> de nuestra lista
        System.Random random = new System.Random();
        // Genera un índice aleatorio dentro del rango de la lista
        int index = random.Next(selectableTiles.Count);  
        int randomTile = selectableTiles[index];

        // Movemos la pieza a la casilla aleatoria    
        robber.GetComponent<RobberMove>().MoveToTile(tiles[randomTile]);

        // Cambiamos el currenTile del ladron
        robber.GetComponent<RobberMove>().currentTile = randomTile; 
        
    }

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {
        // Indice de la posicion donde esta la pieza ahora
        int indexcurrentTile;        

        // Si se trata de un policia
        if (cop==true)
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
        // Si no es un policia sera un ladron
        else
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;
        Debug.Log("Index " + indexcurrentTile);

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //TODO: Implementar BFS. Los nodos seleccionables los ponemos como selectable=true
        //Tendras que cambiar este codigo por el BFS

        // Obtenemos la matriz de las posiciones adyacentes a la posicion actual
        List<int> CurrentAdjacencyList = tiles[indexcurrentTile].adjacency;
        Debug.Log("Current Adjacency " + string.Join(", ", CurrentAdjacencyList));

        // Reseteamos todas las casillas para poder marcarlas como selectable
        ResetTiles();

        // Marcamos las teclas seleccionadas
        for(int i = 0; i<CurrentAdjacencyList.Count; i++) {
            tiles[CurrentAdjacencyList[i]].selectable = true;
        }

        // Ahora que ya tengo la primera fila de casillas posibles marcadas, buscamos la siguiente
        for(int i = 0; i<CurrentAdjacencyList.Count; i++){
            List<int> auxAdjencyList = tiles[CurrentAdjacencyList[i]].adjacency;
            for(int m = 0; m < auxAdjencyList.Count; m++){
                    tiles[auxAdjencyList[m]].selectable = true;
            }
        }

        // Marcamos la casilla en la que estamos somo current y le impedimos que pueda volver a seleccionarla.
        tiles[indexcurrentTile].current = true;
        tiles[indexcurrentTile].selectable = false;

        // Vamos a hacer que mi policia no pueda atravesar a otro para pasar

        // Obtenemos la ubicacion del otro/otros policias
        int indexOtherCop = 0;
        // Si he clickado sobre el primer policia, quiero seleccionar el otro; y viceversa
        if(cops[clickedCop] == cops[0]) {
            indexOtherCop = cops[1].GetComponent<CopMove>().currentTile;
        } 
        if(cops[clickedCop] == cops[1]) {
            indexOtherCop = cops[0].GetComponent<CopMove>().currentTile;
        }

        // Una vez tengo la posicion del segundo policia -> quito de seleccionables las casillas que esten en la trayectoria
        
        // Creamos una variable para que si el otro policia no se encuentra en las primeras adyacentes a nuestro policia actual, marquemos como no seleccionable
        bool encontrada = false;
        // Compruebo primero que no este en la primera tanda de casillas seleccionables
        for(int i = 0; i<CurrentAdjacencyList.Count; i++) {
            if(CurrentAdjacencyList[i] == indexOtherCop) {
                tiles[CurrentAdjacencyList[i]].selectable = false;
                // Sabemos que el otro policia esta en alguna de las cuatro adyacentes a nuestro poli
                encontrada = true;

                // Hacemos qe no pueda saltar al otro policia para pasar, no solo que no pueda ir a donde se encuentra la segunda pieza
                // Primero vemos si esta en la derecha
                if(indexcurrentTile == indexOtherCop+1) {
                    tiles[indexcurrentTile-2].selectable = false;
                }
                // Vemos si está a la izquierda
                if(indexcurrentTile == indexOtherCop-1) {
                    tiles[indexcurrentTile+2].selectable = false;
                }
                // Vemos si esta arriba
                if(indexcurrentTile == indexOtherCop+8) {
                    tiles[indexcurrentTile-16].selectable = false;
                }
                // Vemos si esta abajo
                if(indexcurrentTile == indexOtherCop-8) {
                    tiles[indexcurrentTile+16].selectable = false;
                }
            }     
     
        }
        // Si no hemos encontrado nuestra pieza entre las cuatro adyacentes, nos da igual porque no va a interrumpir ningun camino
        // Pero en cualquier caso, no m epuedo mover a donde este situado el otro policia para que no se solapen las fichas
        if(!encontrada) {
            tiles[indexOtherCop].selectable = false;
        }
    
  
    }

    

    

   

       
}
