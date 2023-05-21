using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Celda      // : MonoBehaviour
{
    public float Lado;
    public GameObject[] m_Walls;
    public Vector3 m_Pos;
    public bool bVisited, bBlock;
    public bool[] bWall;
    public List<Celda> m_Vecinos;
    public Celda m_Parent;

    public float gCost, hCost;
    public int rumbo;
}



[System.Serializable]
public class SaveCelda      // : MonoBehaviour
{
    public int m_x, m_z;
    public bool bVisited, bBlock;
    public bool[] bWall;
}


public class MazeGenerator : MonoBehaviour
{
    public GameObject _Menu;
    MenuManager MazeMenu;

    public GameObject _Wall, _Block;
    public TextMeshProUGUI ProInfo;

    public float Lado = 4.0f;

    //    [SerializeField]
    public Celda[,] m_Celda;

    public Celda m_Start, m_End, m_Tortuga;
    GameObject m_StartObject, m_EndObject, m_TortugaObject;

    public int m_x = 12;
    public int m_z = 12;

    public List<Celda> m_TortList = new List<Celda>();
    public List<Celda> m_OpenSet = new List<Celda>();
    public List<Celda> m_ClosedSet = new List<Celda>();


    MeshRenderer Mesh { get; set; }
    

    enum m_Estado { PS_RESET, PS_GENERAR, PS_RESOLVER, PS_DEFAULT }
    m_Estado GameState;
    // Start is called before the first frame update
    void Start()
    {
        CreateMaze();
        ResetMaze();
//        SaveMaze();
        DrawMaze();

        ProInfo.text = "New Maze";
        GameState = m_Estado.PS_RESET;

        MazeMenu = _Menu.GetComponent<MenuManager>();
        
    }

    // Update is called once per frame
    void Update()
    {
        int x, z;

        switch (GameState)
        {
            case m_Estado.PS_RESET:
                DrawMaze();
                DrawTortuga();
                break;
            case m_Estado.PS_GENERAR:
                if (GenCompleto())
                    GameState = m_Estado.PS_DEFAULT;

//                m_TortList.Add(m_Tortuga);
                GenerateMaze();
                DrawMaze();
                DrawTortuga();
                break;
            case m_Estado.PS_RESOLVER:
                ResolverMaze();
                DrawTortuga();
                break;
            case m_Estado.PS_DEFAULT:
//                DrawMaze();
                break;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveMaze();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
//            SceneManager.LoadScene(0);
            ClearMaze();
            ResetParents();


            LoadMaze();
            DrawMaze();


//            OnReset();




            x = Random.Range(0, m_x);
            z = Random.Range(0, m_z);
            m_End = m_Celda[x, z];


            x = Random.Range(0, m_x);
            z = Random.Range(0, m_z);
            m_Start = m_Celda[x, z];



            while (m_Start == m_End)
            {
                x = Random.Range(0, m_x);
                z = Random.Range(0, m_z);
                m_End = m_Celda[x, z];
            }



            x = Random.Range(0, m_x);
            z = Random.Range(0, m_z);
            m_Tortuga = m_Celda[x, z];




            DrawEnds();                     DrawTortuga();


            GameState = m_Estado.PS_DEFAULT;
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClearMaze();
            GameState = m_Estado.PS_DEFAULT;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
//        Gizmos.DrawCube(15 * Vector3.up, Vector3.one);
    }

    public void CreateMaze()
    {
        Celda Cell;        int x, z;

        m_Celda = new Celda[m_x, m_z];


        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
            {
                Cell = new Celda();

                Cell.m_Walls = new GameObject[4];

                Cell.m_Walls[0] = null;
                Cell.m_Walls[1] = null;
                Cell.m_Walls[2] = null;
                Cell.m_Walls[3] = null;

                Cell.m_Pos = new Vector3((x - m_x / 2) * Lado, 0.5f, (z - m_z / 2) * Lado);
                Cell.bVisited = false;
                Cell.bBlock = false;

                Cell.bWall = new bool[4];

                Cell.bWall[0] = true;
                Cell.bWall[1] = true;
                Cell.bWall[2] = true;
                Cell.bWall[3] = true;


                Cell.m_Vecinos = new List<Celda>();
                Cell.m_Parent = null;

                m_Celda[x, z] = Cell;
            }


        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
            {
                if (x - 1 >= 0)
                    m_Celda[x, z].m_Vecinos.Add(m_Celda[x - 1, z]);
                else
                    m_Celda[x, z].m_Vecinos.Add(null);


                if (z - 1 >= 0)
                    m_Celda[x, z].m_Vecinos.Add(m_Celda[x, z - 1]);
                else
                    m_Celda[x, z].m_Vecinos.Add(null);


                if (x + 1 < m_x)
                    m_Celda[x, z].m_Vecinos.Add(m_Celda[x + 1, z]);
                else
                    m_Celda[x, z].m_Vecinos.Add(null);


                if (z + 1 < m_z)
                    m_Celda[x, z].m_Vecinos.Add(m_Celda[x, z + 1]);
                else
                    m_Celda[x, z].m_Vecinos.Add(null);

            }

        m_Tortuga       = new Celda();
        m_Start         = new Celda();
        m_End           = new Celda();
    }

    public void ClearMaze()
    {
        int x, z, i;

        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
            {
                for (i = 0; i < 4; i++)
                {
                    if (m_Celda[x, z].m_Walls[i] != null)
                        Destroy(m_Celda[x, z].m_Walls[i]);
                }
            }

        if (m_TortugaObject)
            Destroy(m_TortugaObject);

        if (m_StartObject)
            Destroy(m_StartObject);

        if (m_EndObject)
            Destroy(m_EndObject);
    }


    public void ResetMaze()
    {
        int x, z; int nBlocks = 0;

        x = Random.Range(0, m_x);
        z = Random.Range(0, m_z);
        m_End = m_Celda[x, z];


        x = Random.Range(0, m_x);
        z = Random.Range(0, m_z);
        m_Start = m_Celda[x, z];



        while (m_Start == m_End)
        {
            x = Random.Range(0, m_x);
            z = Random.Range(0, m_z);
            m_End = m_Celda[x, z];
        }



        x = Random.Range(0, m_x);
        z = Random.Range(0, m_z);
        m_Tortuga = m_Celda[x, z];


        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
            {
                m_Celda[x, z].m_Walls[0] = null;
                m_Celda[x, z].m_Walls[1] = null;
                m_Celda[x, z].m_Walls[2] = null;
                m_Celda[x, z].m_Walls[3] = null;

                m_Celda[x, z].bVisited = false;
                m_Celda[x, z].bBlock = false;

                m_Celda[x, z].bWall[0] = true;
                m_Celda[x, z].bWall[1] = true;
                m_Celda[x, z].bWall[2] = true;
                m_Celda[x, z].bWall[3] = true;

                m_Celda[x, z].m_Parent = null;

                m_Celda[x, z].gCost = Vector3.Distance(m_Celda[x, z].m_Pos, m_End.m_Pos);       // 1500.0f;
                m_Celda[x, z].hCost = Vector3.Distance(m_Celda[x, z].m_Pos, m_End.m_Pos);

                m_Celda[x, z].rumbo = -1;
            }

        x = Random.Range(0, m_x);
        z = Random.Range(0, m_z);

        while (nBlocks < (int)(m_x * m_z / 20))
        {
            while (m_Celda[x, z] == m_Start || m_Celda[x, z] == m_End)
            { 
                x = Random.Range(0, m_x);
                z = Random.Range(0, m_z);

                m_Celda[x, z].bBlock = false;
                Debug.Log("Repeticion");
            }


            x = Random.Range(0, m_x);
            z = Random.Range(0, m_z);
            m_Celda[x, z].bBlock = true;

            nBlocks++;
        }
    }

    public void ResetParents()
    {
        for (int x = 0; x < m_x; x++)
            for (int z = 0; z < m_z; z++)
                m_Celda[x, z].m_Parent = null;
    }

    public void DrawMaze()
    {
        int x, z, i; Vector3 WallPos;
        float yScala = 6.0f;

        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
            {
                for (i = 0; i < 4; i++)
                {
                    if (m_Celda[x, z].m_Walls[i])
                        Destroy(m_Celda[x, z].m_Walls[i]);
                }


                if (m_Celda[x, z].bWall[0])
                {
                    WallPos = new Vector3((x - m_x / 2 - 0.5f) * Lado, 1.5f, (z - m_z / 2) * Lado);
                    m_Celda[x, z].m_Walls[0] = Instantiate(_Wall, WallPos, Quaternion.identity);
                    m_Celda[x, z].m_Walls[0].transform.localScale = new Vector3(1.0f, yScala, Lado + 1.0f);
                }


                if (m_Celda[x, z].bWall[1])
                {
                    WallPos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 - 0.5f) * Lado);
                    m_Celda[x, z].m_Walls[1] = Instantiate(_Wall, WallPos, Quaternion.identity);
                    m_Celda[x, z].m_Walls[1].transform.localScale = new Vector3(Lado + 1.0f, yScala, 1.0f);
                }

                if (m_Celda[x, z].bWall[2])
                {
                    WallPos = new Vector3((x - m_x / 2 + 0.5f) * Lado, 1.5f, (z - m_z / 2) * Lado);
                    m_Celda[x, z].m_Walls[2] = Instantiate(_Wall, WallPos, Quaternion.identity);
                    m_Celda[x, z].m_Walls[2].transform.localScale = new Vector3(1.0f, yScala, Lado + 1.0f);
                }

                if (m_Celda[x, z].bWall[3])
                {
                    WallPos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 + 0.5f) * Lado);
                    m_Celda[x, z].m_Walls[3] = Instantiate(_Wall, WallPos, Quaternion.identity);
                    m_Celda[x, z].m_Walls[3].transform.localScale = new Vector3(Lado + 1.0f, yScala, 1.0f);
                }
            }
    }

    public void DrawTortuga()
    {
        //        m_Tortuga.DrawCelda();
        if (m_Tortuga == null)
        {
            Debug.Log("Falta tor PELOTUDO");
            return;
        }

        Vector3 Pos = new Vector3((m_Tortuga.m_Pos.x), 1.5f, (m_Tortuga.m_Pos.z));

        if (m_TortugaObject)
            Destroy(m_TortugaObject);

        m_TortugaObject = Instantiate(_Wall, Pos, Quaternion.identity);
        //            m_TortugaObject.transform.position = Pos;
        Mesh = m_TortugaObject.GetComponent<MeshRenderer>();
        Mesh.material.color = Color.black;

    }

    public void DrawEnds()
    {
        Vector3 Pos = new Vector3((m_Start.m_Pos.x), 1.5f, (m_Start.m_Pos.z));

        if (m_StartObject)
            Destroy(m_StartObject);

        m_StartObject = Instantiate(_Wall, Pos, Quaternion.identity);

        Mesh = m_StartObject.GetComponent<MeshRenderer>();
        Mesh.material.color = Color.green;

        Pos = new Vector3((m_End.m_Pos.x), 1.5f, (m_End.m_Pos.z));

        if (m_EndObject)
            Destroy(m_EndObject);

        m_EndObject = Instantiate(_Wall, Pos, Quaternion.identity);

        Mesh = m_EndObject.GetComponent<MeshRenderer>();
        Mesh.material.color = Color.red;
    }

    public void DrawBlocks()
    {
        int x, z;        //Vector3 WallPos;
        float yScala = 6.0f;
        GameObject go;

        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
            {
                if (m_Celda[x, z].bBlock)
                {
                    //                    WallPos = new Vector3((x - m_x / 2) * Lado, 1.5f, (z - m_z / 2 + 0.5f) * Lado);
                    go = Instantiate(_Wall, m_Celda[x, z].m_Pos, Quaternion.identity);
                    go.transform.localScale = new Vector3(1.0f, yScala, 1.0f);
                    Mesh = go.GetComponent<MeshRenderer>();

                    Mesh.material.color = Color.white;
                }
            }
    }


    public void GenerateMaze()
    {
        ProInfo.text = "Generando... " + GenPercent() + " %";

        if (m_Tortuga == null)
            return;

        if (GenCompleto())
        {
            ProInfo.text = "Gen Completo";
            DrawBlocks();
            DrawEnds();
            GameState = m_Estado.PS_DEFAULT;
            return;
        }

        m_Tortuga.bVisited = true;
        m_TortList.Add(m_Tortuga);

        int nVec = Random.Range(0, 4);

        Celda Vec = m_Tortuga.m_Vecinos[nVec];

        while (Vec == null)
        {
            nVec = Random.Range(0, 4);
            Vec = m_Tortuga.m_Vecinos[nVec];
        }


        if (Vec != null)
        {
            if (Vec.bVisited == false && m_Tortuga.bWall[nVec])
            {
                m_Tortuga.bWall[nVec] = false;
                nVec = (nVec + 2) % 4;
                Vec.bWall[nVec] = false;

                m_Tortuga = Vec;
            }
            else if (m_TortList.Count > 0)// && !GenCompleto())
            {
                m_Tortuga = m_TortList[0];
                m_TortList.RemoveAt(0);
            }
        }
    }

    public void ResolverMaze()
    {
        ProInfo.text = "Resolviendo...";

        if (m_Tortuga == m_End)
        {
            ProInfo.text = "Destino Logrado";
            DrawRuta();
//            DrawSolution();
            GameState = m_Estado.PS_DEFAULT;
            return;
        }

        m_Tortuga = SetTortuga();

        m_ClosedSet.Add(m_Tortuga);


        if (m_OpenSet.Count > 0)
        {
            int nIndex = m_OpenSet.IndexOf(m_Tortuga);
            m_OpenSet.RemoveAt(nIndex);
        }
        else
        {
            ProInfo.text = "Imposible";
            GameState = m_Estado.PS_DEFAULT;
        }

        SetVecinos(m_Tortuga);
    }

    public bool GenCompleto()
    {
        bool bVisited = true; int x, z;

        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
                if (bVisited)
                    bVisited = m_Celda[x, z].bVisited;

        return bVisited;
    }

    public int GenPercent()
    {
        int pc = 0;
        int x, z;

        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
                if (m_Celda[x, z].bVisited)
                    pc++;

        return (int)(100 * pc / m_x / m_z);
    }


    Celda SetTortuga()
    {
        int i; Celda tort;     // = new Celda();         
        Celda salida = null;            //new Celda();

        float nCost = 10000.0f;

        for (i = 0; i < m_OpenSet.Count; i++)
        {
            tort = m_OpenSet[i];

            if (tort.gCost + tort.hCost < nCost)
            {
                nCost = tort.gCost + tort.hCost;

                salida = tort;      // m_OpenSet[i];
            }
        }

        return salida;
    }

    public Celda SetVecinos(Celda tort)
    {
        if (tort == null)
        {
//            Debug.Log("Rutina");
            return null;
        }

        float dist;

        int i, vec; Celda cVec = null;// = new Celda();

        vec = tort.m_Vecinos.Count;

        for (i = 0; i < 4; i++)
        {
            cVec = tort.m_Vecinos[i];

            if (cVec != null)
            {
                if (!m_ClosedSet.Contains(cVec) && !tort.bWall[i] && !cVec.bBlock)
                {
                    if (m_OpenSet.Contains(cVec))
                    {
                        dist = tort.gCost + Vector3.Distance(tort.m_Pos, cVec.m_Pos);

                        if (dist < cVec.gCost)
                        {
                            cVec.gCost = dist;
                            cVec.m_Parent = tort;
                            //                            cVec.gCost = tort.gCost + Vector3.Distance(tort.m_Pos, cVec.m_Pos);
                            cVec.hCost = Vector3.Distance(cVec.m_Pos, m_End.m_Pos);
                            cVec.rumbo = i;
                        }
                    }
                    else
                    {
                        cVec.m_Parent = tort;
                        cVec.gCost = tort.gCost + Vector3.Distance(tort.m_Pos, cVec.m_Pos);
                        cVec.hCost = Vector3.Distance(cVec.m_Pos, m_End.m_Pos);
                        cVec.rumbo = i;

                        m_OpenSet.Add(cVec);
                    }
                }
            }
        }

        return cVec;
    }

    public void DrawRuta()
    {
        Vector3 Pos;

        if (m_Tortuga == null)
        {
            Debug.Log("Ruta Imposible");
            return;
        }

        Celda par = m_Tortuga.m_Parent;
        GameObject go;

        while (par != null && par != m_Start)
        {
            Pos = new Vector3(par.m_Pos.x, 1.5f, (par.m_Pos.z));
            go = Instantiate(_Wall, Pos, Quaternion.identity);
            //            go.transform.localScale = new Vector3(1.0f, 2.0f, 1.0f);
            Mesh = go.GetComponent<MeshRenderer>();
            Mesh.material.color = Color.yellow;

            par = par.m_Parent;
        }
    }


    public void DrawSolution()
    {
        Vector3 Pos;

        if (m_Tortuga == null)
        {
            Debug.Log("Ruta Imposible");
            return;
        }

        Celda par = m_Tortuga.m_Parent;
        GameObject go;

        while (par != null && par != m_Start)
        {
            Pos = new Vector3(par.m_Pos.x, 1.5f, (par.m_Pos.z));
//            go = Instantiate(_Block, Pos, Quaternion.identity);
            go = Instantiate(_Block, Pos, Quaternion.AngleAxis(90 * par.rumbo, Vector3.up));

/*
            switch (par.rumbo)
            {
                case 0:
                    go.transform.localScale = new Vector3(4.0f, 1.0f, 1.0f);
                    break;
                case 1:
                    go.transform.localScale = new Vector3(1.0f, 1.0f, 4.0f);
                    break;
                case 2:
                    go.transform.localScale = new Vector3(4.0f, 1.0f, 1.0f);
                    break;
                case 3:
                    go.transform.localScale = new Vector3(1.0f, 1.0f, 4.0f);
                    break;
            }

*/
            //            go.transform.localScale = new Vector3(1.0f, 2.0f, 1.0f);
            Mesh = go.GetComponent<MeshRenderer>();
            Mesh.material.color = Color.yellow;

            par = par.m_Parent;
        }
    }

    public void SetWidth(float width)
    {
        m_x = (int)(width);
        //        CreateMaze();
        //        ResetMaze();
        //        SaveMaze();

//        OnReset();
    }

    public void SetHeight(float height)
    {
        m_z = (int)(height);
        //        CreateMaze();
        //        ResetMaze();
//        SaveMaze();

//        OnReset();
    }

    public void OnReset()
    {
        GameState = m_Estado.PS_RESET;
        SceneManager.LoadScene(0);
//        SaveMaze();
    }

    public void OnGenerar()
    {
        m_TortList.Clear();
        m_TortList.Add(m_Tortuga);

        GameState = m_Estado.PS_GENERAR;
    }

    public void OnResolver()
    {
        m_OpenSet.Clear();
        m_ClosedSet.Clear();

        m_OpenSet.Add(m_Start);
        m_Tortuga = m_Start;

        GameState = m_Estado.PS_RESOLVER;
    }





    public void SaveMaze()
    {
        int x, z, i;
        SaveCelda sCelda = null;        // new SaveCelda();
        string path = Application.persistentDataPath + m_x.ToString() + m_z.ToString() + "MazeGen.dat";

        //        Directory.CreateDirectory(path);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream(path, FileMode.Create);
        PlayerPrefs.SetInt("m_x", m_x);                     PlayerPrefs.SetInt("m_z", m_z);

        for (x = 0; x < m_x; x++)
            for (z = 0; z < m_z; z++)
            {
                sCelda = new SaveCelda();
                sCelda.bWall = new bool[4];

                for (i = 0; i < 4; i++)
                    sCelda.bWall[i] = m_Celda[x, z].bWall[i];

                sCelda.bBlock = m_Celda[x, z].bBlock;
                sCelda.bVisited = m_Celda[x, z].bVisited;

                bf.Serialize(fs, sCelda);
            }

        fs.Close();
    }

    public void LoadMaze()
    {
        m_x = PlayerPrefs.GetInt("m_x");        m_z = PlayerPrefs.GetInt("m_z");
        MazeMenu.x_Slider.value = m_x;          MazeMenu.z_Slider.value = m_z;



        CreateMaze();
        ResetMaze();


        int x, z, i;

        string path = Application.persistentDataPath + m_x.ToString() + m_z.ToString() + "MazeGen.dat";

        BinaryFormatter bf = new BinaryFormatter();
//        SaveCelda sCelda = new SaveCelda();         //

        FileStream fs = new FileStream(path, FileMode.Open);


        if (File.Exists(path))
        {
            for (x = 0; x < m_x; x++)
                for (z = 0; z < m_z; z++)
                {
                    SaveCelda sCelda = new SaveCelda();         //
                    sCelda.bWall = new bool[4];
                    sCelda = (SaveCelda)bf.Deserialize(fs);

                    for (i = 0; i < 4; i++)
                    {
                        m_Celda[x, z].bWall[i] = sCelda.bWall[i];
                    }

                    m_Celda[x, z].bBlock = sCelda.bBlock;
                    m_Celda[x, z].bVisited = sCelda.bVisited;
                }
        }
        else
            SaveMaze();

        fs.Close();
    }
}
