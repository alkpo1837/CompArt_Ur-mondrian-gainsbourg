using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

public class MondrianDataGenerator : MonoBehaviour
{
    public const int FREE = 0;
    public const int OCCUPIED = 1;

    public struct Quad
    {
        public Quad(int x, int y, Dimensions size)
        {
            X = x;
            Y = y;
            Size = size;
        }

        public int X;
        public int Y;
        public Dimensions Size;

        public override string ToString()
        {
            return string.Format("X = {0}, Y = {1}, {2}", X, Y, Size);
        }
    };

    public struct Position
    {
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X;
        public int Y;

        public override string ToString()
        {
            return string.Format("X = {0}, Y = {1}", X, Y);
        }
    }

    public struct Dimensions
    {
        public Dimensions(int w, int h)
        {
            Width = w;
            Height = h;
        }

        public int Width;
        public int Height;

        public override string ToString()
        {
            return string.Format("W = {0}, H = {1}", Width, Height);
        }
    }

    [Header("References")]
    public Transform ScalerParent;
    public Transform FullMondrianParent;

    [Header("Values")]
    public int Size;
    public int PixelsPerUnit = 32;
    [Space(5)]
    public List<Color> PossibleColors;
    // A ColorProbability to get one of the PossibleColors
    public float ColorProbability = 0.15f;
    [Space(10)]
    public float ScaleFactor = 0.1f;

    [Header("Prefab")]
    public GameObject QuadPrefab;

    private int[,] Squares;
    private List<Quad> _allQuads = new List<Quad>();
    private List<QuadImage> _allQuadsImage = new List<QuadImage>();

    private int[] _possibleQuadSize = new int[] { 1, 2, 4, 6 };

    private bool _isUnscaling = false;

    void Start()
    {
        // First elem is Y, 2nd is X
        Squares = new int[Size, Size];

        SetSquaresEmpty();

        Generate();

        FullMondrianParent.transform.localPosition = new Vector2(-(PixelsPerUnit * Size) / 2.0f, (PixelsPerUnit * Size) / 2.0f);

        foreach (Quad quad in _allQuads)
        {
            GameObject quadGo = Instantiate(QuadPrefab, FullMondrianParent);
            QuadImage quadImage = quadGo.GetComponent<QuadImage>();

            quadImage.Init(quad, PixelsPerUnit, Size);

            if (Random.value < ColorProbability)
            {
                quadImage.SetColor(PossibleColors[Random.Range(0, PossibleColors.Count)]);
            }

            _allQuadsImage.Add(quadImage);
            Debug.LogFormat("Drawing {0}", quad);
        }
    }

    private void SetSquaresEmpty()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Squares[i, j] = 0;
            }
        }
    }

    private void Generate()
    {
        Position currentPosition = new Position(0, 0);

        while (currentPosition.X != -1 && currentPosition.Y != -1)
        {
            // Get all possible quad sizes
            List<Dimensions> possiblesDimensions = GetPossibleQuadsDimension(currentPosition.X, currentPosition.Y);

            if (possiblesDimensions.Count > 0)
            {
                // Choose one, and add to AllQuads
                Dimensions choosenDimensions = possiblesDimensions[Random.Range(0, possiblesDimensions.Count)];

                _allQuads.Add(new Quad(currentPosition.X, currentPosition.Y, choosenDimensions));

                // Then fill the Squares array
                for (int i = 0; i < choosenDimensions.Width; i++)
                {
                    for (int j = 0; j < choosenDimensions.Height; j++)
                    {
                        Squares[currentPosition.Y + j, currentPosition.X + i] = OCCUPIED;
                    }
                }
            }
            else
            {
                Debug.LogError("Cannot find quads ?");
            }

            currentPosition = FindNextAvailablePosition(currentPosition);
        }
    }

    private void PrintAllSquares()
    {
        StringBuilder sb = new StringBuilder();

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                sb.Append(Squares[y, x] + ", ");
            }

            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }

    public List<Dimensions> GetPossibleQuadsDimension(int x, int y)
    {
        List<Dimensions> possibleQuads = new List<Dimensions>();

        foreach (int possibleQuadWidth in _possibleQuadSize)
        {
            bool isSizeAvailable = true;

            for (int i = 0; i < possibleQuadWidth; i++)
            {
                if (x + i >= Size || Squares[y, x + i] == OCCUPIED)
                {
                    isSizeAvailable = false;
                    break;
                }
            }

            if (isSizeAvailable)
            {
                List<int> possibleHeigths = new List<int>();

                foreach (int possibleQuadHeigth in _possibleQuadSize)
                {
                    if (y + possibleQuadHeigth <= Size)
                        possibleHeigths.Add(possibleQuadHeigth);
                }

                int height = possibleHeigths[Random.Range(0, possibleHeigths.Count)];

                possibleQuads.Add(new Dimensions(possibleQuadWidth, height));
            }
        }

        return possibleQuads;
    }

    public Position FindNextAvailablePosition(Position previousPosition)
    {
        // The nextPosition is the one directly after the previous position, according to our rules
        Position nextPosition = previousPosition;

        if (nextPosition.X + 1 == Size)
        {
            nextPosition.X = 0;
            nextPosition.Y++;
        }
        else
        {
            nextPosition.X++;
        }

        if (nextPosition.X < Size && nextPosition.Y < Size)
        {
            while (Squares[nextPosition.Y, nextPosition.X] == OCCUPIED)
            {
                nextPosition.X++;

                if (nextPosition.X == Size)
                {
                    nextPosition.X = 0;
                    nextPosition.Y++;

                    if (nextPosition.Y == Size)
                        return new Position(-1, -1);
                }
            }

            return nextPosition;
        }
        else
        {
            return new Position(-1, -1);
        }
    }

    private float _currentTime = 0.0f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ScalerParent.DOScale(Vector3.one * 0.2f, 25.0f).SetEase(Ease.OutCubic);

            _isUnscaling = true;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            foreach (QuadImage quadImage in _allQuadsImage)
            {
                if (Random.value < ColorProbability)
                {
                    quadImage.SetTransitionColor(PossibleColors[Random.Range(0, PossibleColors.Count)]);
                }
                else
                {
                    quadImage.SetTransitionColor(Color.white);
                }
            }
        }

        if (_isUnscaling)
        {
            ScalerParent.transform.localPosition = new Vector3(Mathf.Sin(Time.time) * 100.0f, Mathf.Sin(Time.time / 2.0f) * 100.0f, 0.0f);
            //ScalerParent.transform.localScale -= Vector3.one * ScaleFactor * Time.deltaTime;
        }
    }
}
