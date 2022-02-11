using WordMasterMind.Blazor.Enumerations;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Interfaces;

public interface IGameStateMachine
{
    /// <summary>
    /// Most recent state
    /// </summary>
    public GameState PreviousState { get; }
    /// <summary>
    /// Current state
    /// </summary>
    public GameState State { get; }
    /// <summary>
    /// Whether hard mode is enabled
    /// </summary>
    public bool HardMode { get; set; }
    /// <summary>
    /// Current mastermind instance- only available in playing state.
    /// </summary>
    public WordMasterMindGame? Game { get; }
    /// <summary>
    /// The engine doesn't have a sense of a partially submitted attempt.
    /// This keeps the state until an attempt is made
    /// </summary>
    public string CurrentAttemptString { get; set; }
    public LiteralDictionarySourceType DictionarySourceType { get; set; }
    public IEnumerable<int> ValidWordLengths { get; }
    public LiteralDictionary? LiteralDictionary { get; }
    public int? WordLength { get; set; }
    public HttpClient? HttpClient { get; set; }
    public Task ChangeStateAsync(GameState newState);
}