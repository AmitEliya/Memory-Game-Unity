using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;
using System.Collections;


public class GameController : MonoBehaviour
{
    public List<Button> buttons = new List<Button>();
    [SerializeField] private Sprite bgImage;
    public List<Sprite> gameCards = new List<Sprite>();
    public Sprite[] cards;
    public Button startButton;
    public GameObject endScreen;
    public GameObject pauseMenu;
    public Button nextLevelButton;
    public Button ShuffleCardsButton;
    public Button Peek2CardsButton;
    public TextMeshProUGUI endScreenText;
    public TextMeshProUGUI levelAndMovesCountText;
    public Image memoryGameLogo;
    public Button restartButton;
    public AudioClip flipSound, clapSound;
    private AudioSource _audioSource;

    private const int maxLevel = 4;
    private const int baseNumberOfCards = 8;
    
    private int _countGuess, _countCorrectGuess, _gameGuesses, _firstGuessIndex, _secondGuessIndex;
    private bool _firstGuess, _secondGuess;
    private bool _isShuffleCards, _isPeek2Cards;
    private float _timer, _delay = 1f;
    private bool _isProcessing;
    private bool _gameStarted;
    private int _movesCounter = 0;
    private int _numberOfCards = baseNumberOfCards;
    public int _currentLevel = 1;

    private bool _isPeeking = false;
    private float _peekTimer = 0f;
    private int _peekFirstIndex, _peekSecondIndex;
    
    [SerializeField] private Transform puzzleField;
    [SerializeField] private GameObject btn;
    [SerializeField] private GridLayoutGroup gridLayout;

    void Start()
    {
        LoadAllFruitsCards();
        _numberOfCards = baseNumberOfCards;
        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(RestartGame);
        nextLevelButton.onClick.AddListener(LoadNextLevel);
        endScreen.SetActive(false);
        restartButton.interactable = false;
        SetCardsTransparency(0f);
        HideButtons();
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (_isProcessing)
        {
            _timer += Time.deltaTime;
            if (_timer >= _delay)
            {
                CheckIfCardsMatch();
                _timer = 0f;
                _isProcessing = false;
            }
        }
    }
    
    private void LoadAllFruitsCards()
    {
        cards = Resources.LoadAll<Sprite>("Fruits");
    }

    private void ShowLevelAndPointsCount()
    {
        levelAndMovesCountText.text = "Level : " + _currentLevel + "/" + maxLevel + "\n" + "Moves : " + _movesCounter +
                                       "\nPress ESC for pause";
        
    }
    
    private void GetButtons()
    {
        buttons.Clear();
        for (int i = 0; i < _numberOfCards; i++)
        {
            GameObject newButton = Instantiate(btn, puzzleField); 
            newButton.name = i.ToString(); 
            Button buttonComponent = newButton.GetComponent<Button>();
            buttons.Add(buttonComponent); 
            buttonComponent.image.sprite = bgImage;
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(() => PickACard());
            buttonComponent.interactable = false;
        }

        AdjustGridLayout(); 
    }
    
    private void HideButtons()
    {
        foreach (Button btn in buttons)
        {
            btn.gameObject.SetActive(false);
            btn.interactable = false;
        }
    }
    
    private void AdjustGridLayout()
    {
        ShowLevelAndPointsCount();
        int columns = 4; // every row there is 4 cards
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
        
        Vector2 cellSize = new Vector2(100, 100); 
        gridLayout.cellSize = cellSize;
        gridLayout.spacing = new Vector2(20, 20); 
        gridLayout.childAlignment = TextAnchor.UpperCenter; 
        gridLayout.padding = new RectOffset(10, 10, 10, 10); 
    }
    
    private void AddGameCard()
    {
        gameCards.Clear();

        int looper = buttons.Count;
        int index = 0;
        for (int i = 0; i < looper; i++)
        {
            if (index == looper / 2)
                index = 0;
            gameCards.Add(cards[index]);
            index++;
        }

        for (int i = 0; i < gameCards.Count; i++) // mix
        {
            Sprite temp = gameCards[i];
            int randomIndex = Random.Range(0, gameCards.Count);
            gameCards[i] = gameCards[randomIndex];
            gameCards[randomIndex] = temp;
        }
    }

    private void PickACard()
    {
        if (_isProcessing || !_gameStarted) return;

        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        int index = int.Parse(clickedButton.name);

        if (_firstGuess && _firstGuessIndex == index)
        {
            return; 
        }
        if (_audioSource != null && flipSound != null)
        {
            _audioSource.PlayOneShot(flipSound);
        }
        if (!_firstGuess)
        {
            _firstGuess = true;
            _firstGuessIndex = index;
            clickedButton.image.sprite = gameCards[_firstGuessIndex];
        }
        else if (!_secondGuess)
        {
            _secondGuess = true;
            _secondGuessIndex = index;
            clickedButton.image.sprite = gameCards[_secondGuessIndex];
            _countGuess++;
            _isProcessing = true;
        }
    }

    private void CheckIfCardsMatch()
    {
        if (gameCards[_firstGuessIndex].name == gameCards[_secondGuessIndex].name)
        {
            _countCorrectGuess++;
            ShowLevelAndPointsCount();
            buttons[_firstGuessIndex].interactable = false;
            buttons[_secondGuessIndex].interactable = false;
            buttons[_firstGuessIndex].image.color = new Color(0, 0, 0, 0);
            buttons[_secondGuessIndex].image.color = new Color(0, 0, 0, 0);
            CheckIfGameFinished();
        }
        else
        {
            buttons[_firstGuessIndex].image.sprite = bgImage;
            buttons[_secondGuessIndex].image.sprite = bgImage;
        }
        _movesCounter++;
        ShowLevelAndPointsCount();
        _firstGuess = false;
        _secondGuess = false;
    }

    private void CheckIfGameFinished()
    {
        if (_countCorrectGuess == _gameGuesses)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        if (_audioSource != null && clapSound != null)
        {
            _audioSource.PlayOneShot(clapSound);
        }
        if (_currentLevel < maxLevel)
        {
            _currentLevel++;
            nextLevelButton.gameObject.SetActive(true);
            ShuffleCardsButton.gameObject.SetActive(false);
            Peek2CardsButton.gameObject.SetActive(false);
        }
        else
        {
            ShowGameCompletedScreen();
            ShuffleCardsButton.gameObject.SetActive(false);
            Peek2CardsButton.gameObject.SetActive(false);
        }
    }
    
    private void LoadNextLevel()
    {
        nextLevelButton.gameObject.SetActive(false);
        _numberOfCards += 4; // update next level number of cards.
        ShowLevelAndPointsCount();
        cleanAllButtonsAndSetting();
        GetButtons(); 
        AddGameCard(); 
        _countGuess = 0;
        _gameGuesses = gameCards.Count / 2;
        _countCorrectGuess = 0;
        _gameStarted = true;
        
        foreach (Button btn in buttons)
        {
            btn.gameObject.SetActive(true);
            btn.interactable = true;
            btn.image.color = Color.white;
        }
        if (!_isPeek2Cards)
        {
            Peek2CardsButton.gameObject.SetActive(true);
        }
        if (!_isShuffleCards)
        {
            ShuffleCardsButton.gameObject.SetActive(true);

        }
        SetCardsTransparency(1f); 
    }
    
    private void ShowGameCompletedScreen()
    {
        endScreen.SetActive(true);
        levelAndMovesCountText.gameObject.SetActive(false);
        restartButton.interactable = true;
        endScreenText.text = "Great Job ! You finish all the levels !!! \nYour Number of Moves are: " + _movesCounter;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        cleanAllButtonsAndSetting();
        _movesCounter = 0;
        _currentLevel = 1;
        _numberOfCards = baseNumberOfCards;
        _gameStarted = false;
        endScreen.SetActive(false);
        StartGame();
    }

    private void SetCardsTransparency(float alpha)
    {
        foreach (Button btn in buttons)
        {
            CanvasGroup canvasGroup = btn.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
        }
    }
    
    private void StartGame()
    {
        memoryGameLogo.gameObject.SetActive(false);
        GetButtons(); 
        AddGameCard(); 
        _isShuffleCards = false;
        _isPeek2Cards = false;
        ShuffleCardsButton.gameObject.SetActive(true);
        Peek2CardsButton.gameObject.SetActive(true);
        levelAndMovesCountText.gameObject.SetActive(true);
        _gameGuesses = gameCards.Count / 2;
        _gameStarted = true;

        foreach (Button btn in buttons)
        {
            btn.gameObject.SetActive(true);
            btn.interactable = true;
            btn.image.color = Color.white;
        }

        SetCardsTransparency(1f);
        startButton.gameObject.SetActive(false); 
    }

    private void cleanAllButtonsAndSetting()
    {
        _countGuess = 0;
        _countCorrectGuess = 0;
        _gameStarted = false;
        _firstGuess = false;
        _secondGuess = false;
        _firstGuessIndex = 0;
        _secondGuessIndex = 0;
        HideButtons();
        buttons.Clear();
        gameCards.Clear();
    }

    public void ShuffleCards()
    {
        ShuffleCardsButton.gameObject.SetActive(false);
        _isShuffleCards = true;
        List<int> activeButtonIndices = GetActiveButtonIndices();
        List<Sprite> activeCards = new List<Sprite>();
        
        foreach (int index in activeButtonIndices)
        {
            activeCards.Add(gameCards[index]);
        }
        for (int i = 0; i < activeCards.Count; i++) // mix
        {
            Sprite temp = activeCards[i];
            int randomIndex = Random.Range(0, activeCards.Count);
            activeCards[i] = activeCards[randomIndex];
            activeCards[randomIndex] = temp;
        }
        for (int i = 0; i < activeButtonIndices.Count; i++)
        {
            gameCards[activeButtonIndices[i]] = activeCards[i];
        }
    }

    public void PeekTwoCardsUsingDeltaTime(float peekDuration = 10f)
    {
        List<int> activeButtonIndices = GetActiveButtonIndices();
        int firstPeekIndex = activeButtonIndices[Random.Range(0, activeButtonIndices.Count)];
        int secondPeekIndex;

        Peek2CardsButton.gameObject.SetActive(false); 
        _isPeek2Cards = true;

        if (activeButtonIndices.Count < 2 || _isPeeking)
        {
            _isPeek2Cards = false;
            Peek2CardsButton.gameObject.SetActive(true);
            return;
        }
        
        do
        {
            secondPeekIndex = activeButtonIndices[Random.Range(0, activeButtonIndices.Count)];
        } while (secondPeekIndex == firstPeekIndex);

        buttons[firstPeekIndex].image.sprite = gameCards[firstPeekIndex];
        buttons[secondPeekIndex].image.sprite = gameCards[secondPeekIndex];

        StartCoroutine(ClosePeekedCardsAfterDelay(firstPeekIndex, secondPeekIndex)); // peek mode
    }

    private IEnumerator ClosePeekedCardsAfterDelay(int firstIndex, int secondIndex)
    {
        yield return new WaitForSeconds(1f); // wait 1 second
        buttons[firstIndex].image.sprite = bgImage;
        buttons[secondIndex].image.sprite = bgImage;
        _isPeeking = false;
    }

    private List<int> GetActiveButtonIndices()
    {
        List<int> activeButtonIndices = new List<int>();
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i].interactable && buttons[i].image.sprite == bgImage) 
            {
                activeButtonIndices.Add(i);
            }
        }
        return activeButtonIndices;
    }
}
